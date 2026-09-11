using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerWallClimb : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private LayerMask wallMask = 1;
    [SerializeField] private float checkDistance = 0.7f;
    [SerializeField] private float checkRadius = 0.25f;
    [SerializeField] private float chestHeight = 1f;
    [SerializeField] private float ledgeProbeForward = 0.25f;
    [SerializeField] private float minLedgeFlatDot = 0.7f;
    [SerializeField] private float minLedgeDepth = 0.4f;

    [Header("Height Rule")]
    [SerializeField] private float reachHeight = 2.4f;
    [SerializeField] private float minClimbHeight = -1f;

    [Header("Climb")]
    [SerializeField] private string climbTrigger = "Climb";
    [SerializeField] private float climbDuration = 1f;
    [SerializeField] private float landDepth = 0.45f;
    [SerializeField] private bool isFaceWall = true;
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0f, 0f, 0.72f, 1f);
    [SerializeField] private AnimationCurve forwardCurve = AnimationCurve.EaseInOut(0.45f, 0f, 1f, 1f);

    [Header("Hand IK")]
    [SerializeField] private bool isHandIkEnabled = true;
    [SerializeField] private float handSpacing = 0.26f;
    [SerializeField] private float handDepth = 0.08f;
    [SerializeField] private float handUpOffset = 0.02f;
    [SerializeField] private AnimationCurve handIkCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.55f, 1f),
        new Keyframe(0.8f, 0f));

    [Header("Debug")]
    [SerializeField] private bool isDebugLog = true;
    [SerializeField] private bool isDebugGizmo = true;

    private CharacterController _controller;
    private PlayerController _movement;
    private PlayerAnimator _playerAnimator;
    private PlayerInteractionRunner _runner;
    private AssassinationSystem _assassination;
    private PlayerThrow _throw;

    private Vector3 _wallPoint;

    public bool isBusy { get; private set; }

    public event Action OnClimbStarted;
    public event Action OnClimbFinished;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _movement = GetComponent<PlayerController>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _runner = GetComponent<PlayerInteractionRunner>();
        _assassination = GetComponent<AssassinationSystem>();
        _throw = GetComponent<PlayerThrow>();
    }

    private float footOffset
    {
        get
        {
            return _controller.center.y - _controller.height * 0.5f;
        }
    }

    private float feetY
    {
        get
        {
            return transform.position.y + footOffset;
        }
    }

    private float maxProbeHeight
    {
        get
        {
            float jump = _movement != null ? _movement.maxJumpHeight : 1.2f;

            return reachHeight + jump + 1f;
        }
    }

    public bool TryClimb()
    {
        if (isBusy)
        {
            return false;
        }

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

        if (!TryFindLedge(out Vector3 ledgePoint, out Vector3 wallNormal))
        {
            return false;
        }

        float height = ledgePoint.y - feetY;
        float minHeight = minClimbHeight >= 0f
            ? minClimbHeight
            : (_movement != null ? _movement.maxJumpHeight : 1.2f);

        if (height < minHeight)
        {
            Log($"벽이 낮아서 그냥 점프 ({height:F2}m < {minHeight:F2}m)");
            return false;
        }

        if (height > reachHeight)
        {
            Log($"벽이 높아서 손이 안 닿음 ({height:F2}m > {reachHeight:F2}m). 점프 후 다시 눌러보세요.");
            return false;
        }

        StartCoroutine(Climb_co(ledgePoint, wallNormal));

        return true;
    }

    private bool TryFindLedge(out Vector3 ledgePoint, out Vector3 wallNormal)
    {
        ledgePoint = Vector3.zero;
        wallNormal = -transform.forward;

        Vector3 origin = new Vector3(transform.position.x, feetY + chestHeight, transform.position.z);

        if (!Physics.SphereCast(origin, checkRadius, transform.forward, out RaycastHit wallHit, checkDistance, wallMask, QueryTriggerInteraction.Ignore))
        {
            Log("앞에 벽이 없습니다.");
            return false;
        }

        wallNormal = wallHit.normal;

        if (Mathf.Abs(Vector3.Dot(wallNormal, Vector3.up)) > 0.35f)
        {
            Log("벽이 수직이 아닙니다. (경사면)");
            return false;
        }

        Vector3 flatNormal = wallNormal;
        flatNormal.y = 0f;

        if (flatNormal.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        flatNormal.Normalize();
        wallNormal = flatNormal;

        Vector3 topOrigin = wallHit.point - flatNormal * ledgeProbeForward;
        topOrigin.y = feetY + maxProbeHeight;

        if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, maxProbeHeight * 2f, wallMask, QueryTriggerInteraction.Ignore))
        {
            Log("난간 윗면을 찾지 못했습니다.");
            return false;
        }

        if (Vector3.Dot(topHit.normal, Vector3.up) < minLedgeFlatDot)
        {
            Log("난간 윗면이 평평하지 않습니다.");
            return false;
        }

        Vector3 depthOrigin = topHit.point - flatNormal * minLedgeDepth + Vector3.up * 0.1f;

        if (!Physics.Raycast(depthOrigin, Vector3.down, out RaycastHit depthHit, 0.4f, wallMask, QueryTriggerInteraction.Ignore)
            || Mathf.Abs(depthHit.point.y - topHit.point.y) > 0.2f)
        {
            Log("올라설 자리가 좁습니다.");
            return false;
        }

        ledgePoint = topHit.point;
        _wallPoint = wallHit.point;

        return true;
    }

    private IEnumerator Climb_co(Vector3 ledgePoint, Vector3 wallNormal)
    {
        isBusy = true;

        try
        {
            OnClimbStarted?.Invoke();

            if (_movement != null)
            {
                _movement.enabled = false;
            }

            _controller.enabled = false;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            Quaternion targetRotation = isFaceWall
                ? Quaternion.LookRotation(-wallNormal)
                : startRotation;

            Vector3 endPosition = ledgePoint - wallNormal * landDepth;
            endPosition.y = ledgePoint.y - footOffset;

            Vector3 grabCenter = new Vector3(_wallPoint.x, ledgePoint.y + handUpOffset, _wallPoint.z) - wallNormal * handDepth;
            Vector3 grabRight = Vector3.Cross(Vector3.up, -wallNormal).normalized;
            Vector3 leftHandTarget = grabCenter - grabRight * handSpacing;
            Vector3 rightHandTarget = grabCenter + grabRight * handSpacing;
            Quaternion handRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);

            if (_playerAnimator != null)
            {
                _playerAnimator.PlayAction(climbTrigger);
            }

            float duration = Mathf.Max(0.05f, climbDuration);
            float elapsed = 0f;

            Vector3 flatStart = new Vector3(startPosition.x, 0f, startPosition.z);
            Vector3 flatEnd = new Vector3(endPosition.x, 0f, endPosition.z);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float k = Mathf.Clamp01(elapsed / duration);
                Vector3 flat = Vector3.Lerp(flatStart, flatEnd, Mathf.Clamp01(forwardCurve.Evaluate(k)));

                transform.position = new Vector3(
                    flat.x,
                    Mathf.Lerp(startPosition.y, endPosition.y, Mathf.Clamp01(heightCurve.Evaluate(k))),
                    flat.z);

                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Mathf.Clamp01(k * 4f));

                if (isHandIkEnabled && _playerAnimator != null)
                {
                    float handWeight = Mathf.Clamp01(handIkCurve.Evaluate(k));

                    _playerAnimator.SetHandIK(true, leftHandTarget, handRotation, handWeight);
                    _playerAnimator.SetHandIK(false, rightHandTarget, handRotation, handWeight);
                }

                yield return null;
            }

            transform.SetPositionAndRotation(endPosition, targetRotation);
        }
        finally
        {
            if (_playerAnimator != null)
            {
                _playerAnimator.ClearHandIK();
                _playerAnimator.EndAction(climbTrigger);

                if (isHandIkEnabled && !_playerAnimator.isIkPassActive)
                {
                    Debug.LogWarning("[WallClimb] 손 IK가 적용되지 않았습니다. Animator의 해당 레이어에서 'IK Pass'를 켜주세요.", this);
                }
            }

            _controller.enabled = true;

            if (_movement != null)
            {
                _movement.StopVertical();
                _movement.enabled = true;
            }

            isBusy = false;

            OnClimbFinished?.Invoke();
        }
    }

    private void Log(string message)
    {
        if (isDebugLog)
        {
            Debug.Log($"[WallClimb] {message}", this);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!isDebugGizmo || _controller == null)
        {
            TryGetComponent(out _controller);
        }

        if (!isDebugGizmo || _controller == null)
        {
            return;
        }

        Vector3 origin = new Vector3(transform.position.x, feetY + chestHeight, transform.position.z);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, checkRadius);
        Gizmos.DrawWireSphere(origin + transform.forward * checkDistance, checkRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * checkDistance);

        Gizmos.color = Color.yellow;
        Vector3 reachPoint = new Vector3(transform.position.x, feetY + reachHeight, transform.position.z);
        Gizmos.DrawLine(reachPoint, reachPoint + transform.forward * checkDistance);
    }
#endif
}
