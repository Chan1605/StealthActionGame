using System;
using UnityEngine;

public class EnemyObject : MonoBehaviour, IInteractable
{
    public Transform ObjectTransform => transform;

    public Action OnUse { get; set; }
    public Action OnLook { get; set; }

    public event Action OnTargetCompleted;

    [SerializeField] private Transform uiAnchor;

    public bool IsPlayerLook { get; set; }
    public bool isAct;

    private TakedownVictim victim;
    private AssassinationSystem _assassination;
    private UI_Outliner outLine;
    private UI_ObjKeyPanal keyPanal;
    private HUDManager hudManager;

    public bool IsInteractable
    {
        get
        {
            if (_assassination == null || victim == null) return false;
            return _assassination.CanTarget(victim);
        }
    }

    private void Awake()
    {
        TryGetComponent(out outLine);
        TryGetComponent(out victim);
    }

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        keyPanal = hudManager.GetKeyPanal();
        _assassination = FindAnyObjectByType<AssassinationSystem>();

        if (_assassination != null)
        {
            _assassination.OnTargetAcquired += HandleTargetAcquired;
            _assassination.OnTargetLost += HandleTargetLost;
            _assassination.OnTakedownStarted += HandleTakedownStarted;
        }

        KeyPanal_Off();
    }

    private void OnDestroy()
    {
        if (_assassination != null)
        {
            _assassination.OnTargetAcquired -= HandleTargetAcquired;
            _assassination.OnTargetLost -= HandleTargetLost;
            _assassination.OnTakedownStarted -= HandleTakedownStarted;
        }
    }

    private void HandleTargetAcquired(TakedownVictim target)
    {
        if (target != victim) return;
        IsPlayerLook = true;
        isAct = true;
        outLine?.SetOutLine_On();
        keyPanal.SetPanal_On(uiAnchor != null ? uiAnchor : transform);
    }

    private void HandleTargetLost()
    {
        if (!isAct) return;
        KeyPanal_Off();
        isAct = false;
        IsPlayerLook = false;
    }

    private void HandleTakedownStarted(TakedownVictim target)
    {
        if (target != victim) return;
        KeyPanal_Off();
        isAct = false;
        IsPlayerLook = false;
    }

    private void KeyPanal_Off()
    {
        outLine?.SetOutLine_Off();
        keyPanal.SetPanal_Off();
    }

    public void EnableInteraction() { }
    public void DisableInteraction() { }
}