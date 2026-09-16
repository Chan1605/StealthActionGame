using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseAndInputSetting : MonoBehaviour
{
    private PlayerInput playerInput;
    private void Start()
    {
        GameManager.Instance.curPlayer = this;
        playerInput = GetComponent<PlayerInput>();
    }
    public void OnMenu(InputValue value)
    {
        if (value.isPressed)
        {
            GameManager.Instance.HandleMenuInput();
        }
    }

    public void ApplyKeybinds(string bindData)
    {
        if(!string.IsNullOrEmpty(bindData))
        {
            playerInput.actions.LoadBindingOverridesFromJson(bindData);
        }
    }
}
