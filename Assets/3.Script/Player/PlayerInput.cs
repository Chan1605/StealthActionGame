using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private bool isinvertY = false;

    [Header("Crouch")]
    [SerializeField] private bool isCrouchToggle = true;

    [Header("Cursor")]
    [SerializeField] private bool isLockCursor = true;

    private HitMan _actions;

    private HitMan actions
    {
        get
        {
            if (_actions == null)
            {
                _actions = new HitMan();
            }

            return _actions;
        }
    }

    public Vector2 MoveInput
    {
        get
        {
            return Vector2.ClampMagnitude(actions.Player.Move.ReadValue<Vector2>(), 1f);
        }
    }

    public Vector2 LookInput
    {
        get
        {
            Vector2 d = actions.Player.Look.ReadValue<Vector2>() * lookSensitivity;
            if (isinvertY)
            {
                d.y = -d.y;
            }
            return d;
        }
    }

    public bool isSprint
    {
        get
        {
            return actions.Player.Sprint.IsPressed();
        }
    }

    public bool isCrouching { get; private set; }

    public event Action JumpPressed;
    public event Action InteractPressed;
    public event Action<bool> CrouchChanged;

    private void Awake()
    {
        _actions = actions;
    }

    private void OnEnable()
    {
        actions.Player.Enable();

        actions.Player.Jump.performed += HandleJump;
        actions.Player.Interact.performed += HandleInteract;
        actions.Player.Crouch.performed += HandleCrouchPerformed;
        actions.Player.Crouch.canceled += HandleCrouchCanceled;
    }

    private void OnDisable()
    {
        if (_actions == null)
        {
            return;
        }

        _actions.Player.Jump.performed -= HandleJump;
        _actions.Player.Interact.performed -= HandleInteract;
        _actions.Player.Crouch.performed -= HandleCrouchPerformed;
        _actions.Player.Crouch.canceled -= HandleCrouchCanceled;

        _actions.Player.Disable();

        SetCrouch(false);
    }

    private void OnDestroy()
    {
        if (_actions != null)
        {
            _actions.Dispose();
        }
    }

    private void Start()
    {
        if (isLockCursor)
        {
            SetCursorLocked(true);
        }
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        JumpPressed?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        InteractPressed?.Invoke();
    }

    private void HandleCrouchPerformed(InputAction.CallbackContext context)
    {
        if (isCrouchToggle)
        {
            SetCrouch(!isCrouching);
        }
        else
        {
            SetCrouch(true);
        }
    }

    private void HandleCrouchCanceled(InputAction.CallbackContext context)
    {
        if (isCrouchToggle)
        {
            return;
        }

        SetCrouch(false);
    }

    private void SetCrouch(bool isOn)
    {
        if (isCrouching == isOn)
        {
            return;
        }

        isCrouching = isOn;
        CrouchChanged?.Invoke(isOn);
    }

    public void SetInputEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            actions.Player.Enable();
        }
        else
        {
            actions.Player.Disable();
        }

        SetCursorLocked(isEnabled);
    }

    private void SetCursorLocked(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
