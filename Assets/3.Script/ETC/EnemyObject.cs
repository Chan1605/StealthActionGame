using System;
using UnityEngine;

public class EnemyObject : MonoBehaviour, IInteractable
{
    public Transform ObjectTransform => transform;
    [SerializeField] private Transform uiAnchor;
    public Action OnUse { get; set; }
    public Action OnLook { get; set; }

    public event Action OnTargetCompleted;

    public bool IsPlayerLook { get; set; }
    public bool isAct;

    private EnemyAI enemyAI;
    private TakedownVictim victim;
    private UI_Outliner outLine;
    private UI_ObjKeyPanal keyPanal;
    private HUDManager hudManager;

    public bool IsInteractable
    {
        get
        {
            bool result = true;
            if (enemyAI != null && !enemyAI.CanBeAssassinated) result = false;
            if (victim != null && victim.isDown) result = false;

            Debug.Log($"[EnemyObject] IsInteractable={result}, enemyAI={enemyAI != null}, CanBeAssassinated={enemyAI?.CanBeAssassinated}, victim={victim != null}, isDown={victim?.isDown}");
            return result;
        }
    }

    private void Awake()
    {
        TryGetComponent(out outLine);
        TryGetComponent(out enemyAI);
        TryGetComponent(out victim);
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
        if (isAct && (!IsPlayerLook || !IsInteractable))
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
        if (!IsInteractable) return;

        IsPlayerLook = true;
        isAct = true;
        outLine?.SetOutLine_On();
        keyPanal.SetPanal_On(uiAnchor != null ? uiAnchor : transform);
    }

    private void KeyPanal_Off()
    {
        outLine?.SetOutLine_Off();
        keyPanal.SetPanal_Off();
    }

    public void EnableInteraction() { }
    public void DisableInteraction() { }
}