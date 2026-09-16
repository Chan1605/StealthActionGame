using System;
using UnityEngine;

public class KeyDropOnDeath : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform dropPoint;

    public Transform ObjectTransform => transform;
    public Action OnUse { get; set; }
    public Action OnLook { get; set; }
    public event Action OnTargetCompleted;

    public bool IsPlayerLook { get; set; }
    public bool IsInteractable { get; private set; } = true;

    private TakedownVictim _victim;

    private void Awake()
    {
        TryGetComponent(out _victim);
    }

    private void OnEnable()
    {
        if (_victim != null) _victim.OnDowned += HandleDowned;
    }

    private void OnDisable()
    {
        if (_victim != null) _victim.OnDowned -= HandleDowned;
    }

    private void HandleDowned(TakedownVictim victim)
    {
        Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position;
        Instantiate(keyPrefab, spawnPos, Quaternion.identity);

        OnTargetCompleted?.Invoke();
    }

    public void EnableInteraction() { }
    public void DisableInteraction() { }
}