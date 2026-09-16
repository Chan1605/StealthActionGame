using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public PauseAndInputSetting curPlayer;
    public UI_MenuState menuState;

    public void HandleMenuInput()
    {
        menuState.HandleMenuInput();
    }

    public void UpdatePlayerKeyBinds(string bindData)
    {
        if(curPlayer != null)
        {
            curPlayer.ApplyKeybinds(bindData);
        }
    }
}
