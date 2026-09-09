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

    private PlayerController _controller;

    private static readonly int MoveXId = Animator.StringToHash("MoveX");
    private static readonly int MoveYId = Animator.StringToHash("MoveY");
    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int IsCrouchedId = Animator.StringToHash("IsCrouched");
    private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
    private static readonly int JumpId = Animator.StringToHash("Jump");

    public event Action Footstep;
    public event Action LandImpact;

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
        _controller.Jumped += HandleJumped;
        _controller.CrouchStateChanged += HandleCrouchStateChanged;
    }

    private void OnDisable()
    {
        _controller.Jumped -= HandleJumped;
        _controller.CrouchStateChanged -= HandleCrouchStateChanged;
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

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        Footstep?.Invoke();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }

        LandImpact?.Invoke();
    }
}
