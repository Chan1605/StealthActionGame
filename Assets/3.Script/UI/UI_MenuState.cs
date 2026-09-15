using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MenuState : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.menuState = this;
    }
    public enum MenuState
    {
        InPlaying,
        InPauseMenu,
        InOptionMenu,
        InOptionTap
    }

    private MenuState currentState = MenuState.InPlaying;

    public void HandleMenuInput()
    {
        switch (currentState)
        {
            case MenuState.InPlaying:
                break;
            case MenuState.InPauseMenu:
                break;
            case MenuState.InOptionMenu:
                break;
            case MenuState.InOptionTap:
                break;
        }
    }

    private void OpenPauseMenu()
    {
        currentState = MenuState.InPauseMenu;
    }

    private void ClosePauseMenu()
    {
        currentState = MenuState.InPlaying;
    }

    private void CloseOptionMenu()
    {
        currentState = MenuState.InPauseMenu;
    }

    private void LeaveOptionTab()
    {
        currentState = MenuState.InOptionMenu;
    }

}
