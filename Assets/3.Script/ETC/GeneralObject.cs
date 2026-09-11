using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralObject : MonoBehaviour, IInteractable
{
    public Transform ObjectTransform => transform;

    public Action OnUse { get; set; }
    public Action OnLook { get; set; }

    public event Action OnTargetCompleted;


    public bool IsInteractable => true;
    public bool IsPlayerLook { get; set; }
    public bool isAct;

    private Object_Data Data;

    private UI_Outliner outLine;
    private UI_ObjKeyPanal keyPanal;

    private HUDManager hudManager;


    private void Awake()
    {
        TryGetComponent(out outLine);
    }

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        keyPanal = hudManager.GetKeyPanal();

        OnLook += Look;
        KeyPanal_Off();
    }

    private void Update()
    {
        if (!IsPlayerLook && isAct)
        {
            KeyPanal_Off();
            isAct = false;
        }
    }

    private void OnDestroy()
    {
        OnLook -= Look;
    }

    private void Look()
    {
        KeyPanal_On();
    }

    private void KeyPanal_On()
    {
        Debug.Log($"outline 할당 상태 : {outLine != null}");
        Debug.Log($"keyPanal 할당 상태 : {keyPanal != null}");

        IsPlayerLook = true;
        isAct = true;
        outLine.SetOutLine_On();
        keyPanal.SetPanal_On(transform);
    }

    private void KeyPanal_Off()
    {
        outLine.SetOutLine_Off();
        keyPanal.SetPanal_Off();
    }

    public void Debug_InvokeAction()
    {
        OnLook.Invoke();
    }

    public void Debug_PlayerDontLook()
    {
        IsPlayerLook = false;
    }
    public void EnableInteraction()
    {
        Debug.Log("오브젝트의 IInteractable 스크립트가 상시 개체용입니다. 리스트를 확인해주세요.");
    }

    public void DisableInteraction()
    {
        Debug.Log("오브젝트의 IInteractable 스크립트가 상시 개체용입니다. 리스트를 확인해주세요.");
    }



}
