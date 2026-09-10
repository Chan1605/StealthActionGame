using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerThrow : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string throwTrigger = "Throw";
    [SerializeField] private float releaseDelay = 0.35f;
    [SerializeField] private float recoverDuration = 0f;

    [Header("Throw")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float throwPower = 12f;
    [SerializeField] private float upwardBoost = 0.15f;

    private PlayerInput _input;
    private PlayerHand _hand;
    private PlayerAnimator _playerAnimator;
    private PlayerInteractionRunner _runner;
    private AssassinationSystem _assassination;

    public bool isBusy { get; private set; }

    public event Action<HoldableItem> OnThrown;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _hand = GetComponentInChildren<PlayerHand>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _runner = GetComponent<PlayerInteractionRunner>();
        _assassination = GetComponent<AssassinationSystem>();

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        _input.OnThrowPressed += HandleThrowPressed;
    }

    private void OnDisable()
    {
        _input.OnThrowPressed -= HandleThrowPressed;
    }

    private void HandleThrowPressed()
    {
        if (isBusy)
        {
            return;
        }

        if (_hand == null || !_hand.isHolding)
        {
            return;
        }

        if (_runner != null && _runner.isBusy)
        {
            return;
        }

        if (_assassination != null && _assassination.isBusy)
        {
            return;
        }

        StartCoroutine(Throw_co());
    }

    private IEnumerator Throw_co()
    {
        isBusy = true;

        try
        {
            if (_playerAnimator != null)
            {
                _playerAnimator.PlayUpperAction(throwTrigger);
            }

            yield return null;

            float total = recoverDuration > 0f
                ? recoverDuration
                : (_playerAnimator != null ? _playerAnimator.upperStateLength : 1f);

            float delay = Mathf.Max(0f, releaseDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            ReleaseItem();

            float rest = total - delay;
            if (rest > 0f)
            {
                yield return new WaitForSeconds(rest);
            }
        }
        finally
        {
            if (_playerAnimator != null)
            {
                _playerAnimator.EndUpperAction(throwTrigger);
            }

            isBusy = false;
        }
    }

    private void ReleaseItem()
    {
        if (_hand == null || !_hand.isHolding)
        {
            return;
        }

        Vector3 direction = GetAimDirection();
        HoldableItem item = _hand.Release(direction * throwPower);

        if (item != null)
        {
            OnThrown?.Invoke(item);
        }
    }

    private Vector3 GetAimDirection()
    {
        Vector3 forward = aimCamera != null ? aimCamera.transform.forward : transform.forward;

        return (forward + Vector3.up * upwardBoost).normalized;
    }
}
