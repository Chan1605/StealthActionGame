using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    public void OnMenu(InputValue value)
    {
        if (value.isPressed)
        {
            GameManager.Instance.HandleMenuInput();
        }
    }
}
