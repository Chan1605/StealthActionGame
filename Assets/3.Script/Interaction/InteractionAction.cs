using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableObj))]
public abstract class InteractionAction : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string animationTrigger;
    [SerializeField] private float actionDelay = 0.5f;
    [SerializeField] private float duration = 1.2f;

    [Header("Align")]
    [SerializeField] private Transform alignAnchor;
    [SerializeField] private float alignTime = 0.15f;

    [Header("Rule")]
    [SerializeField] private bool isRepeatable = true;

    private InteractableObj _interactable;

    public virtual string animationTriggerName
    {
        get
        {
            return animationTrigger;
        }
    }

    public virtual float actionDelayTime
    {
        get
        {
            return actionDelay;
        }
    }

    public virtual float durationTime
    {
        get
        {
            return duration;
        }
    }

    public Transform alignTarget
    {
        get
        {
            return alignAnchor;
        }
    }

    public float alignDuration
    {
        get
        {
            return alignTime;
        }
    }

    public bool isUsed { get; private set; }

    public bool IsReady(Transform user)
    {
        if (isUsed && !isRepeatable)
        {
            return false;
        }

        return CanExecute(user);
    }

    public event Action<InteractionAction> OnExecuted;

    protected InteractableObj interactable
    {
        get
        {
            return _interactable;
        }
    }

    protected virtual void Awake()
    {
        _interactable = GetComponent<InteractableObj>();
    }

    public void Execute(Transform user)
    {
        if (!IsReady(user))
        {
            return;
        }

        isUsed = true;

        OnExecute(user);

        if (_interactable != null && _interactable.OnUse != null)
        {
            _interactable.OnUse.Invoke();
        }

        OnExecuted?.Invoke(this);
    }

    protected abstract void OnExecute(Transform user);

    protected virtual bool CanExecute(Transform user)
    {
        return true;
    }
}
