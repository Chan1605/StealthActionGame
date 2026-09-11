using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObject : MonoBehaviour, IInteractable
{
    public Transform ObjectTransform => transform;

    public Action OnUse { get; set; }
    public Action OnLook { get; set; }

    public event Action OnTargetCompleted;


    public bool IsInteractable { get; }
    public bool IsPlayerLook { get; set; }

    public void EnableInteraction()
    {

    }
    public void DisableInteraction()
    {

    }
}
