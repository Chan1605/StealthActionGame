using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class CarrySystem : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private LayerMask bodyMask;
    [SerializeField] private float pickUpRange = 1.8f;
    [SerializeField] private float pickUpAngle = 120f;
    [SerializeField] private bool isCrouchRequired = true;

    [Header("Align")]
    [SerializeField] private bool isAlignEnabled = true;
    [SerializeField] private float alignTime = 0.18f;

    /// <summary>
    /// Holding Down 클립은 몸이 좌우 22cm, 앞 15cm 움직입니다.
    /// 손이 닿는 지점이 루트 정중앙이 아니라서, 시체를 이 오프셋 위치에 두도록 정렬합니다.
    /// 좌우 부호는 재생해보고 맞추세요.
    /// </summary>
    [SerializeField] private Vector3 reachOffset = new Vector3(-0.22f, 0f, 0.15f);

    [Header("Socket")]
    [SerializeField] private Transform carrySocket;

    [Header("Animation")]
    [SerializeField] private string pickUpTrigger = "PickUpBody";
    [SerializeField] private string putDownTrigger = "PutDownBody";
    [SerializeField] private string carrySpeedParameter = "CarrySpeed";
    [SerializeField] private float pickUpDuration = 1.27f;
    [SerializeField] private float putDownDuration = 1.27f;

    [Tooltip("들기 클립의 몇 % 지점부터 시체를 품으로 끌어당길지. 0.55면 0.70초쯤 시작합니다.")]
    [SerializeField] [Range(0f, 1f)] private float attachStartRatio = 0.55f;

    [Tooltip("내려놓기 클립의 몇 % 지점에서 시체를 놓을지.")]
    [SerializeField] [Range(0f, 1f)] private float detachRatio = 0.5f;

    [Header("Move")]
    [SerializeField] private float carrySpeed = 1.4f;
    [SerializeField] private float carrySpeedDamping = 0.12f;

    [Header("Put Down")]
    [SerializeField] private float putDownForward = 0.7f;
    [SerializeField] private float putDownProbeHeight = 1.2f;
    [SerializeField] private LayerMask groundMask = 1;

    [Header("Debug")]
    [SerializeField] private bool isDebugLog = true;
    [SerializeField] private bool isDebugGizmo = true;

    private CharacterController _controller;
    private PlayerInput _input;
    private PlayerController _movement;
    private PlayerAnimator _playerAnimator;
    private Animator _animator;
    private PlayerInteractionRunner _runner;
    private AssassinationSystem _assassination;
    private PlayerThrow _throw;
    private PlayerWallClimb _wallClimb;

    private int _carrySpeedId;

    public bool isBusy { get; private set; }
    public CarriableBody heldBody { get; private set; }

    public bool isCarrying
    {
        get
        {
            return heldBody != null;
        }
    }

    /// <summary>운반 중이거나 들기/놓기 모션 중이면 다른 행동을 막아야 합니다.</summary>
    public bool isBlockingOtherActions
    {
        get
        {
            return isBusy || isCarrying;
        }
    }

    public event Action<CarriableBody> OnCarryStarted;
    public event Action<CarriableBody> OnCarryEnded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerController>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _runner = GetComponent<PlayerInteractionRunner>();
        _assassination = GetComponent<AssassinationSystem>();
        _throw = GetComponent<PlayerThrow>();
        _wallClimb = GetComponent<PlayerWallClimb>();

        _animator = GetComponentInChildren<Animator>();
        _carrySpeedId = Animator.StringToHash(carrySpeedParameter);

        if (carrySocket == null)
        {
            Debug.LogWarning("[CarrySystem] Carry Socket이 비어 있습니다. 가슴 높이에 빈 오브젝트를 만들어 연결하세요.", this);
        }
    }

    private void OnEnable()
    {
        _input.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        _input.OnInteractPressed -= HandleInteractPressed;
    }

    private void Update()
    {
        UpdateCarrySpeedParameter();
    }

    private void UpdateCarrySpeedParameter()
    {
        if (_animator == null)
        {
            return;
        }

        float target = 0f;

        if (isCarrying && _movement != null && carrySpeed > 0.01f)
        {
            target = Mathf.Clamp01(_movement.currentSpeed / carrySpeed);
        }

        _animator.SetFloat(_carrySpeedId, target, carrySpeedDamping, Time.deltaTime);
    }

    private void HandleInteractPressed()
    {
        if (isBusy)
        {
            return;
        }

        if (isCarrying)
        {
            TryPutDown();
            return;
        }

        TryPickUp();
    }

    public bool TryPickUp()
    {
        if (isBusy || isCarrying)
        {
            return false;
        }

        if (!IsFree())
        {
            return false;
        }

        if (isCrouchRequired && _movement != null && !_movement.isCrouched)
        {
            Log("앉은 상태에서만 시체를 들 수 있습니다.");
            return false;
        }

        CarriableBody body = FindBody();

        if (body == null)
        {
            return false;
        }

        StartCoroutine(PickUp_co(body));

        return true;
    }

    public bool TryPutDown()
    {
        if (isBusy || !isCarrying)
        {
            return false;
        }

        StartCoroutine(PutDown_co());

        return true;
    }

    /// <summary>
    /// 드럼통 같은 처리 장치가 호출합니다. 시체의 소유권을 넘겨받고 플레이어는 Idle로 돌아갑니다.
    /// 반환된 CarriableBody의 연출과 Dispose는 호출한 쪽이 책임집니다.
    /// </summary>
    public CarriableBody ReleaseForDispose()
    {
        if (!isCarrying)
        {
            return null;
        }

        CarriableBody body = heldBody;
        heldBody = null;

        if (_movement != null)
        {
            _movement.ClearSpeedOverride();
        }

        if (_playerAnimator != null)
        {
            _playerAnimator.EndAction(pickUpTrigger);
        }

        OnCarryEnded?.Invoke(body);
        Log("시체를 처리 장치로 넘겼습니다.");

        return body;
    }

    private bool IsFree()
    {
        if (_runner != null && _runner.isBusy)
        {
            return false;
        }

        if (_assassination != null && _assassination.isBusy)
        {
            return false;
        }

        if (_throw != null && _throw.isBusy)
        {
            return false;
        }

        if (_wallClimb != null && _wallClimb.isBusy)
        {
            return false;
        }

        return true;
    }

    private CarriableBody FindBody()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Collider[] hits = Physics.OverlapSphere(origin, pickUpRange, bodyMask, QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            Log("주변에 시체가 없습니다. Body Mask에 시체 레이어가 들어 있는지 확인하세요.");
            return null;
        }

        CarriableBody best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            CarriableBody body = hit.GetComponentInParent<CarriableBody>();

            if (body == null || !body.canCarry)
            {
                continue;
            }

            Vector3 flat = body.transform.position - transform.position;
            flat.y = 0f;

            float distance = flat.magnitude;

            if (distance > pickUpRange)
            {
                continue;
            }

            if (distance > 0.01f && Vector3.Angle(transform.forward, flat) > pickUpAngle * 0.5f)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = body;
            }
        }

        if (best == null)
        {
            Log("들 수 있는 시체가 앞쪽에 없습니다.");
        }

        return best;
    }

    private IEnumerator PickUp_co(CarriableBody body)
    {
        isBusy = true;

        try
        {
            if (isAlignEnabled)
            {
                yield return Align_co(body.transform.position);
            }

            if (_playerAnimator != null)
            {
                _playerAnimator.PlayAction(pickUpTrigger);
            }

            float duration = Mathf.Max(0.05f, pickUpDuration);
            float attachStart = duration * Mathf.Clamp01(attachStartRatio);
            float elapsed = 0f;
            bool isAttached = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (!isAttached && elapsed >= attachStart)
                {
                    isAttached = true;
                    body.BeginCarry(carrySocket);
                }

                if (isAttached)
                {
                    float span = Mathf.Max(0.01f, duration - attachStart);
                    body.ApplyCarryBlend((elapsed - attachStart) / span);
                }

                yield return null;
            }

            if (!isAttached)
            {
                body.BeginCarry(carrySocket);
            }

            body.ApplyCarryBlend(1f);

            heldBody = body;

            if (_movement != null)
            {
                // Carrying 클립은 선 자세입니다. 앉아서 들었다면 캡슐도 같이 세워야 합니다.
                _movement.ForceStand();
                _movement.SetSpeedOverride(carrySpeed);
            }

            OnCarryStarted?.Invoke(body);
            Log($"{body.victimRoot?.name} 운반 시작");
        }
        finally
        {
            isBusy = false;
        }
    }

    private IEnumerator PutDown_co()
    {
        isBusy = true;

        CarriableBody body = heldBody;

        try
        {
            if (_playerAnimator != null)
            {
                _playerAnimator.PlayAction(putDownTrigger);
            }

            float duration = Mathf.Max(0.05f, putDownDuration);
            float detachAt = duration * Mathf.Clamp01(detachRatio);
            float elapsed = 0f;
            bool isDetached = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (!isDetached && elapsed >= detachAt)
                {
                    isDetached = true;

                    GetPutDownPose(out Vector3 position, out Quaternion rotation);

                    body.EndCarry(position, rotation);

                    heldBody = null;

                    if (_movement != null)
                    {
                        _movement.ClearSpeedOverride();
                    }

                    OnCarryEnded?.Invoke(body);
                }

                yield return null;
            }
        }
        finally
        {
            if (heldBody != null)
            {
                GetPutDownPose(out Vector3 position, out Quaternion rotation);
                heldBody.EndCarry(position, rotation);
                heldBody = null;

                if (_movement != null)
                {
                    _movement.ClearSpeedOverride();
                }

                OnCarryEnded?.Invoke(body);
            }

            if (_playerAnimator != null)
            {
                _playerAnimator.EndAction(putDownTrigger);
            }

            isBusy = false;
        }
    }

    private void GetPutDownPose(out Vector3 position, out Quaternion rotation)
    {
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 target = transform.position + flatForward * putDownForward;
        Vector3 probe = target + Vector3.up * putDownProbeHeight;

        if (Physics.Raycast(probe, Vector3.down, out RaycastHit hit, putDownProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            target.y = hit.point.y + 0.15f;
        }
        else
        {
            target.y = transform.position.y + 0.15f;
        }

        position = target;
        rotation = Quaternion.LookRotation(flatForward);
    }

    private IEnumerator Align_co(Vector3 bodyPosition)
    {
        Vector3 flat = bodyPosition - transform.position;
        flat.y = 0f;

        if (flat.sqrMagnitude < 0.0001f)
        {
            yield break;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flat.normalized);

        Vector3 targetPosition = bodyPosition - targetRotation * reachOffset;
        targetPosition.y = transform.position.y;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        if (_movement != null)
        {
            _movement.enabled = false;
        }

        _controller.enabled = false;

        float duration = Mathf.Max(0.01f, alignTime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float k = Mathf.Clamp01(elapsed / duration);

            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, k),
                Quaternion.Slerp(startRotation, targetRotation, k));

            yield return null;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        _controller.enabled = true;

        if (_movement != null)
        {
            _movement.StopVertical();
            _movement.enabled = true;
        }
    }

    private void Log(string message)
    {
        if (isDebugLog)
        {
            Debug.Log($"[CarrySystem] {message}", this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!isDebugGizmo)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(origin, pickUpRange);

        Quaternion left = Quaternion.Euler(0f, -pickUpAngle * 0.5f, 0f);
        Quaternion right = Quaternion.Euler(0f, pickUpAngle * 0.5f, 0f);

        Gizmos.DrawLine(origin, origin + left * transform.forward * pickUpRange);
        Gizmos.DrawLine(origin, origin + right * transform.forward * pickUpRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.rotation * reachOffset, 0.08f);
    }
#endif
}
