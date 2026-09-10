using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInteractionRunner : MonoBehaviour
{
    [Header("Link")]
    [SerializeField] private PlayerInteractor interactor;

    [Header("Put Down")]
    [SerializeField] private string putDownTrigger = "PutDown";
    [SerializeField] private float putDownActionDelay = 0.6f;
    [SerializeField] private float putDownDuration = 1.2f;
    [SerializeField] private float putDownDistance = 0.7f;
    [SerializeField] private float putDownHeightOffset = 0.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Debug")]
    [SerializeField] private bool isDebugLog = true;

    private PlayerInput _input;
    private PlayerController _movement;
    private PlayerCameraRig _cameraRig;
    private PlayerAnimator _playerAnimator;
    private AssassinationSystem _assassination;
    private PlayerHand _hand;

    private string _activeTrigger;

    public bool isBusy { get; private set; }

    public event Action<InteractionAction> OnInteractionStarted;
    public event Action<InteractionAction> OnInteractionFinished;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerController>();
        _cameraRig = GetComponent<PlayerCameraRig>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _assassination = GetComponent<AssassinationSystem>();
        _hand = GetComponentInChildren<PlayerHand>();

        if (interactor == null)
        {
            interactor = GetComponent<PlayerInteractor>();
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

    private void HandleInteractPressed()
    {
        if (isBusy)
        {
            return;
        }

        if (_assassination != null && _assassination.isBusy)
        {
            return;
        }

        InteractionAction action = FindAction();
        if (action != null)
        {
            StartCoroutine(Run_co(action));
            return;
        }

        if (_hand != null && _hand.isHolding)
        {
            StartCoroutine(PutDown_co());
        }
    }

    private IEnumerator PutDown_co()
    {
        isBusy = true;

        try
        {
            SetPlayerControlEnabled(false);
            PlayTrigger(putDownTrigger);

            float delay = Mathf.Max(0f, putDownActionDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            PlaceHeldItem();

            float rest = putDownDuration - delay;
            if (rest > 0f)
            {
                yield return new WaitForSeconds(rest);
            }
        }
        finally
        {
            Release();
        }
    }

    private void PlaceHeldItem()
    {
        if (_hand == null || !_hand.isHolding)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * putDownDistance;
        Vector3 position = origin;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f, groundMask, QueryTriggerInteraction.Ignore))
        {
            position = hit.point;
        }

        position += Vector3.up * putDownHeightOffset;

        Quaternion rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        HoldableItem item = _hand.PutDown(position, rotation);

        if (isDebugLog && item != null)
        {
            Debug.Log($"[Interaction] '{item.name}' 내려놓기", item);
        }
    }

    private InteractionAction FindAction()
    {
        if (interactor == null || interactor.CurrentTarget == null)
        {
            return null;
        }

        InteractionAction action = interactor.CurrentTarget.GetComponent<InteractionAction>();
        if (action == null)
        {
            if (isDebugLog)
            {
                Debug.Log($"[Interaction] '{interactor.CurrentTarget.name}'에 InteractionAction이 없습니다.", interactor.CurrentTarget);
            }

            return null;
        }

        if (!action.IsReady(transform))
        {
            if (isDebugLog)
            {
                Debug.Log($"[Interaction] '{action.name}' 지금은 사용할 수 없습니다.", action);
            }

            return null;
        }

        return action;
    }

    private IEnumerator Run_co(InteractionAction action)
    {
        isBusy = true;

        try
        {
            OnInteractionStarted?.Invoke(action);
            SetPlayerControlEnabled(false);

            if (action.alignTarget != null)
            {
                yield return Align_co(action.alignTarget, action.alignDuration);
            }

            PlayActionAnimation(action);

            float delay = Mathf.Max(0f, action.actionDelayTime);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            action.Execute(transform);

            if (isDebugLog)
            {
                Debug.Log($"[Interaction] '{action.name}' 실행 (+{delay:F2}초)", action);
            }

            float rest = action.durationTime - delay;
            if (rest > 0f)
            {
                yield return new WaitForSeconds(rest);
            }
        }
        finally
        {
            Release();
            OnInteractionFinished?.Invoke(action);
        }
    }

    private IEnumerator Align_co(Transform anchor, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = anchor.position;
        targetPosition.y = startPosition.y;

        Vector3 flatForward = anchor.forward;
        flatForward.y = 0f;

        Quaternion targetRotation = flatForward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(flatForward)
            : startRotation;

        if (duration <= 0f)
        {
            transform.SetPositionAndRotation(targetPosition, targetRotation);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, k);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, k);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void PlayActionAnimation(InteractionAction action)
    {
        PlayTrigger(action.animationTriggerName);
    }

    private void PlayTrigger(string triggerName)
    {
        _activeTrigger = null;

        if (_playerAnimator == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        if (_playerAnimator.PlayAction(triggerName))
        {
            _activeTrigger = triggerName;
        }
    }

    private void Release()
    {
        if (_playerAnimator != null && !string.IsNullOrEmpty(_activeTrigger))
        {
            _playerAnimator.EndAction(_activeTrigger);
        }

        _activeTrigger = null;

        SetPlayerControlEnabled(true);
        isBusy = false;
    }

    public void ForceRelease()
    {
        StopAllCoroutines();
        Release();
    }

    private void SetPlayerControlEnabled(bool isEnabled)
    {
        if (_movement != null)
        {
            _movement.enabled = isEnabled;
        }

        if (_cameraRig != null)
        {
            _cameraRig.enabled = isEnabled;
        }
    }
}
