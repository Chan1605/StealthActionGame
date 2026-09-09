using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCameraRig : MonoBehaviour
{
    [Header("Pitch")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float startPitch = 45;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 50f;

    [Header("Zoom")]
    [SerializeField] private CinemachineThirdPersonFollow thirdPersonFollow;
    [SerializeField] private float defaultDistance = 3.5f;
    [SerializeField] private float zoomDuration = 0.25f;

    [Header("Shake")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private PlayerInput _input;
    private float _pitch;
    private Coroutine _zoomRoutine;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraDistance = defaultDistance;
        }

        ResetPitch();
    }

    public void ResetPitch()
    {
        _pitch = Mathf.Clamp(startPitch, minPitch, maxPitch);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    private void LateUpdate()
    {
        if (cameraPivot == null)
        {
            return;
        }

        _pitch = Mathf.Clamp(_pitch + _input.LookInput.y, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void ZoomTo(float distance)
    {
        if (thirdPersonFollow == null)
        {
            return;
        }

        if (_zoomRoutine != null)
        {
            StopCoroutine(_zoomRoutine);
        }

        _zoomRoutine = StartCoroutine(Zoom_co(distance));
    }

    public void ResetZoom()
    {
        ZoomTo(defaultDistance);
    }

    public void Shake(float force)
    {
        if (impulseSource == null)
        {
            return;
        }

        impulseSource.GenerateImpulseWithForce(force);
    }

    private IEnumerator Zoom_co(float targetDistance)
    {
        float start = thirdPersonFollow.CameraDistance;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            thirdPersonFollow.CameraDistance = Mathf.Lerp(start, targetDistance, elapsed / zoomDuration);
            yield return null;
        }

        thirdPersonFollow.CameraDistance = targetDistance;
        _zoomRoutine = null;
    }
}
