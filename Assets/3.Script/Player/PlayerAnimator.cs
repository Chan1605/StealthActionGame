using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Animator animator;

    [Header("Damping")]
    [SerializeField] private float directionDamping = 0.12f;

    [Header("Action Layer")]
    [SerializeField] private int actionLayer = 1;
    [SerializeField] private string emptyStateName = "none";
    [SerializeField] private float emptyBlendTime = 0.1f;

    [Header("Upper Body Layer")]
    [SerializeField] private int upperBodyLayer = 2;
    [SerializeField] private string upperBodyEmptyStateName = "none";
    [SerializeField] private float upperBodyBlendIn = 0.12f;
    [SerializeField] private float upperBodyBlendOut = 0.22f;

    private PlayerController _controller;
    private Coroutine _upperFadeRoutine;

    private static readonly int MoveXId = Animator.StringToHash("MoveX");
    private static readonly int MoveYId = Animator.StringToHash("MoveY");
    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int IsCrouchedId = Animator.StringToHash("IsCrouched");
    private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
    private static readonly int JumpId = Animator.StringToHash("Jump");
    private static readonly int AssassinateId = Animator.StringToHash("Assassinate");

    public event Action OnFootstepPlayed;
    public event Action OnLandImpact;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        _controller.OnJumped += HandleJumped;
        _controller.OnCrouchStateChanged += HandleCrouchStateChanged;
    }

    private void OnDisable()
    {
        _controller.OnJumped -= HandleJumped;
        _controller.OnCrouchStateChanged -= HandleCrouchStateChanged;
    }

    private void Start()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsCrouchedId, _controller.isCrouched);
        animator.SetBool(IsGroundedId, _controller.isGrounded);
    }

    private void LateUpdate()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(MoveXId, _controller.moveX, directionDamping, Time.deltaTime);
        animator.SetFloat(MoveYId, _controller.moveY, directionDamping, Time.deltaTime);
        animator.SetFloat(SpeedId, _controller.normalizedSpeed);
        animator.SetBool(IsGroundedId, _controller.isGrounded);
    }

    private void HandleJumped()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(JumpId);
    }

    private void HandleCrouchStateChanged(bool isOn)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsCrouchedId, isOn);
    }

    public void PlayAssassinate()
    {
        PlayAction(AssassinateId);
    }

    public void EndAssassinate()
    {
        EndAction(AssassinateId);
    }

    public bool PlayAction(string triggerName)
    {
        if (!HasTrigger(triggerName))
        {
            Debug.LogWarning($"[PlayerAnimator] Animator에 '{triggerName}' 트리거가 없습니다. 컨트롤러에 Trigger 파라미터를 추가하세요.", this);
            return false;
        }

        PlayAction(Animator.StringToHash(triggerName));
        return true;
    }

    public void EndAction(string triggerName)
    {
        if (!HasTrigger(triggerName))
        {
            EndAction(0);
            return;
        }

        EndAction(Animator.StringToHash(triggerName));
    }

    public bool PlayUpperAction(string triggerName)
    {
        if (!HasTrigger(triggerName))
        {
            Debug.LogWarning($"[PlayerAnimator] Animator에 '{triggerName}' 트리거가 없습니다. 컨트롤러에 Trigger 파라미터를 추가하세요.", this);
            return false;
        }

        if (animator == null)
        {
            return false;
        }

        FadeUpperLayer(1f, upperBodyBlendIn);
        animator.SetTrigger(triggerName);

        return true;
    }

    public void EndUpperAction(string triggerName)
    {
        if (animator == null)
        {
            return;
        }

        FadeUpperLayer(0f, upperBodyBlendOut);

        if (HasTrigger(triggerName))
        {
            animator.ResetTrigger(triggerName);
        }
    }

    private void FadeUpperLayer(float target, float duration)
    {
        if (_upperFadeRoutine != null)
        {
            StopCoroutine(_upperFadeRoutine);
        }

        _upperFadeRoutine = StartCoroutine(FadeUpperLayer_co(target, duration));
    }

    private IEnumerator FadeUpperLayer_co(float target, float duration)
    {
        float start = animator.GetLayerWeight(upperBodyLayer);

        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                animator.SetLayerWeight(upperBodyLayer, Mathf.Lerp(start, target, elapsed / duration));
                yield return null;
            }
        }

        animator.SetLayerWeight(upperBodyLayer, target);

        if (target <= 0f && !string.IsNullOrEmpty(upperBodyEmptyStateName))
        {
            animator.CrossFade(upperBodyEmptyStateName, 0f, upperBodyLayer);
        }

        _upperFadeRoutine = null;
    }

    public float upperStateLength
    {
        get
        {
            if (animator == null)
            {
                return 0f;
            }

            AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(upperBodyLayer);

            if (info.length <= 0f)
            {
                info = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            }

            return info.length / Mathf.Max(0.01f, info.speed == 0f ? 1f : Mathf.Abs(info.speed));
        }
    }

    public bool HasTrigger(string triggerName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(triggerName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayAction(int triggerId)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetLayerWeight(actionLayer, 1f);
        animator.SetTrigger(triggerId);
    }

    private void EndAction(int triggerId)
    {
        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(emptyStateName))
        {
            animator.CrossFade(emptyStateName, emptyBlendTime, actionLayer);
        }

        animator.SetLayerWeight(actionLayer, 0f);

        if (triggerId != 0)
        {
            animator.ResetTrigger(triggerId);
        }
    }

    public bool isRootMotionEnabled
    {
        get
        {
            return animator != null && animator.applyRootMotion;
        }
        set
        {
            if (animator != null)
            {
                animator.applyRootMotion = value;
            }
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        OnFootstepPlayed?.Invoke();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        OnLandImpact?.Invoke();
    }
}
