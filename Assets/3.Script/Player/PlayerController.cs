using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float acceleration = 14f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.1f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.1f;
    [SerializeField] private float heightLerpSpeed = 10f;
    [SerializeField] private LayerMask ceilingMask;

    private CharacterController _controller;
    private PlayerInput _input;

    private Vector3 _planarVelocity;
    private Vector2 _moveDirection;
    private float _verticalVelocity;
    private float _standHeight;
    private float _targetHeight;
    private float _capsuleBottom;
    private float _centerX;
    private float _centerZ;
    private float _groundedTimer;
    private bool _isGroundedPrev;

    public bool isGrounded
    {
        get
        {
            return _groundedTimer > 0f;
        }
    }

    public bool isCrouched { get; private set; }

    public float currentSpeed
    {
        get
        {
            return _planarVelocity.magnitude;
        }
    }

    public float normalizedSpeed
    {
        get
        {
            return _planarVelocity.magnitude / sprintSpeed;
        }
    }

    public float moveX
    {
        get
        {
            return _moveDirection.x;
        }
    }

    public float moveY
    {
        get
        {
            return _moveDirection.y;
        }
    }

    public event Action Jumped;
    public event Action Landed;
    public event Action<bool> CrouchStateChanged;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();

        _moveDirection = Vector2.up;
        _groundedTimer = coyoteTime;
        _isGroundedPrev = true;
        _standHeight = _controller.height;
        _targetHeight = _standHeight;
        _capsuleBottom = _controller.center.y - _standHeight * 0.5f;
        _centerX = _controller.center.x;
        _centerZ = _controller.center.z;
    }

    private void OnEnable()
    {
        _input.JumpPressed += HandleJumpPressed;
        _input.CrouchChanged += HandleCrouchChanged;
    }

    private void OnDisable()
    {
        _input.JumpPressed -= HandleJumpPressed;
        _input.CrouchChanged -= HandleCrouchChanged;
    }

    private void Update()
    {
        ApplyYaw();
        ApplyGravity();
        ApplyHeight();
        ApplyMove();
        UpdateGrounded();
    }

    private void ApplyYaw()
    {
        float yaw = _input.LookInput.x;
        if (Mathf.Approximately(yaw, 0f))
        {
            return;
        }

        transform.Rotate(Vector3.up, yaw, Space.World);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundedGravity;
            return;
        }

        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void ApplyHeight()
    {
        if (Mathf.Approximately(_controller.height, _targetHeight))
        {
            return;
        }

        float height = Mathf.Lerp(_controller.height, _targetHeight, heightLerpSpeed * Time.deltaTime);
        _controller.height = height;
        _controller.center = new Vector3(_centerX, _capsuleBottom + height * 0.5f, _centerZ);
    }

    private void ApplyMove()
    {
        Vector2 move = _input.MoveInput;
        Vector3 direction = transform.right * move.x + transform.forward * move.y;
        Vector3 target = direction * GetTargetSpeed();

        _planarVelocity = Vector3.MoveTowards(_planarVelocity, target, acceleration * Time.deltaTime);

        UpdateMoveDirection();

        Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateMoveDirection()
    {
        Vector3 local = transform.InverseTransformDirection(_planarVelocity);
        Vector2 flat = new Vector2(local.x, local.z);

        if (flat.sqrMagnitude < 0.0001f)
        {
            return;
        }

        _moveDirection = flat.normalized;
    }

    private float GetTargetSpeed()
    {
        if (isCrouched)
        {
            return crouchSpeed;
        }

        if (_input.isSprint)
        {
            return sprintSpeed;
        }

        return walkSpeed;
    }

    private void UpdateGrounded()
    {
        bool isGroundedNow = _controller.isGrounded;

        if (isGroundedNow)
        {
            _groundedTimer = coyoteTime;
        }
        else
        {
            _groundedTimer -= Time.deltaTime;
        }

        if (isGroundedNow && !_isGroundedPrev)
        {
            Landed?.Invoke();
        }

        _isGroundedPrev = isGroundedNow;
    }

    private void HandleJumpPressed()
    {
        if (!isGrounded)
        {
            return;
        }

        if (isCrouched)
        {
            TryStandUp();
            return;
        }

        _groundedTimer = 0f;
        _isGroundedPrev = false;
        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        Jumped?.Invoke();
    }

    private void HandleCrouchChanged(bool isOn)
    {
        if (isOn)
        {
            SetCrouched(true);
            return;
        }

        TryStandUp();
    }

    private void TryStandUp()
    {
        if (IsCeilingBlocked())
        {
            return;
        }

        SetCrouched(false);
    }

    private void SetCrouched(bool isOn)
    {
        if (isCrouched == isOn)
        {
            return;
        }

        isCrouched = isOn;
        _targetHeight = isOn ? Mathf.Min(crouchHeight, _standHeight) : _standHeight;
        CrouchStateChanged?.Invoke(isOn);
    }

    private bool IsCeilingBlocked()
    {
        float distance = _standHeight - _controller.height;
        if (distance <= 0.01f)
        {
            return false;
        }

        float radius = _controller.radius * 0.95f;
        Vector3 center = transform.TransformPoint(_controller.center);
        Vector3 origin = center + Vector3.up * (_controller.height * 0.5f - radius);

        return Physics.SphereCast(origin, radius, Vector3.up, out _, distance, ceilingMask, QueryTriggerInteraction.Ignore);
    }
}
