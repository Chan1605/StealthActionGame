using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [Header("Hold Pose")]
    [SerializeField] private Vector3 holdPosition;
    [SerializeField] private Vector3 holdEuler;

    [Header("Physics")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private Collider[] colliders;

    private Transform _originalParent;
    private bool _isPrevKinematic;

    public bool isHeld { get; private set; }

    public Rigidbody itemBody
    {
        get
        {
            return body;
        }
    }

    public event Action<HoldableItem> OnHeld;
    public event Action<HoldableItem> OnReleased;

    private void Awake()
    {
        if (body == null)
        {
            TryGetComponent(out body);
        }

        if (colliders == null || colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>();
        }

        _originalParent = transform.parent;
    }

    public void AttachTo(Transform socket)
    {
        if (socket == null || isHeld)
        {
            return;
        }

        isHeld = true;

        SetPhysicsEnabled(false);

        transform.SetParent(socket, false);
        transform.localPosition = holdPosition;
        transform.localRotation = Quaternion.Euler(holdEuler);

        OnHeld?.Invoke(this);
    }

    public void Detach(Vector3 velocity)
    {
        if (!isHeld)
        {
            return;
        }

        isHeld = false;

        transform.SetParent(_originalParent, true);

        SetPhysicsEnabled(true);

        if (body != null && !body.isKinematic)
        {
            body.linearVelocity = velocity;
        }

        OnReleased?.Invoke(this);
    }

    private void SetPhysicsEnabled(bool isEnabled)
    {
        if (body != null)
        {
            if (!isEnabled)
            {
                _isPrevKinematic = body.isKinematic;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = isEnabled ? _isPrevKinematic : true;
            body.detectCollisions = isEnabled;
        }

        foreach (Collider col in colliders)
        {
            if (col == null)
            {
                continue;
            }

            col.enabled = isEnabled;
        }
    }
}
