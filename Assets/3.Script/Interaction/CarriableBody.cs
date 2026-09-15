using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시체의 Hips 본에 붙입니다. (예: mixamorig:Hips)
/// 래그돌 본들이 전부 Hips의 자식이라, 본 콜라이더에서 GetComponentInParent로 찾을 수 있습니다.
/// </summary>
public class CarriableBody : MonoBehaviour
{
    [Header("Carried Pose")]
    [SerializeField] private Vector3 carriedLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 carriedLocalEuler = new Vector3(0f, 90f, 0f);

    [Header("Debug")]
    [SerializeField] private bool isDebugLog = true;

    private TakedownVictim _victim;
    private Transform _victimRoot;
    private Transform _originalParent;

    private Rigidbody[] _bodies;
    private bool[] _originalKinematic;
    private RigidbodyInterpolation[] _originalInterpolation;

    private Vector3 _blendStartPosition;
    private Quaternion _blendStartRotation;

    public bool isCarried { get; private set; }
    public bool isDisposed { get; private set; }

    public bool canCarry
    {
        get
        {
            return !isCarried && !isDisposed && _victim != null && _victim.isDown;
        }
    }

    public Transform victimRoot
    {
        get
        {
            return _victimRoot;
        }
    }

    private void Awake()
    {
        _victim = GetComponentInParent<TakedownVictim>();

        if (_victim == null)
        {
            Debug.LogError("[CarriableBody] 부모에서 TakedownVictim을 찾지 못했습니다. Hips 본에 붙였는지 확인하세요.", this);
        }
        else
        {
            _victimRoot = _victim.transform;
        }

        _originalParent = transform.parent;

        _bodies = GetComponentsInChildren<Rigidbody>(true);
        _originalKinematic = new bool[_bodies.Length];
        _originalInterpolation = new RigidbodyInterpolation[_bodies.Length];

        if (_bodies.Length < 5)
        {
            Debug.LogWarning($"[CarriableBody] 자식 Rigidbody가 {_bodies.Length}개뿐입니다. Hips 본이 맞는지 확인하세요.", this);
        }
    }

    /// <summary>
    /// 래그돌을 굳히고 소켓에 붙입니다. 이후 매 프레임 ApplyCarryBlend를 호출하세요.
    /// </summary>
    public void BeginCarry(Transform socket)
    {
        if (isCarried || socket == null)
        {
            return;
        }

        isCarried = true;

        for (int i = 0; i < _bodies.Length; i++)
        {
            Rigidbody body = _bodies[i];

            if (body == null)
            {
                continue;
            }

            _originalKinematic[i] = body.isKinematic;
            _originalInterpolation[i] = body.interpolation;

            // 움직이는 부모에 붙는 동안 Interpolate를 켜두면 부모 트랜스폼과 싸워서
            // 몸이 손이 아닌 엉뚱한 곳에 붙습니다. 반드시 꺼야 합니다.
            body.interpolation = RigidbodyInterpolation.None;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        transform.SetParent(socket, true);

        _blendStartPosition = transform.localPosition;
        _blendStartRotation = transform.localRotation;

        Log("들기 시작");
    }

    /// <summary>k: 0 = 붙잡은 순간의 자세, 1 = 품에 안긴 자세</summary>
    public void ApplyCarryBlend(float k)
    {
        if (!isCarried)
        {
            return;
        }

        float t = Mathf.Clamp01(k);

        transform.localPosition = Vector3.Lerp(_blendStartPosition, carriedLocalPosition, t);
        transform.localRotation = Quaternion.Slerp(_blendStartRotation, Quaternion.Euler(carriedLocalEuler), t);
    }

    /// <summary>바닥에 내려놓습니다. 래그돌이 다시 살아납니다.</summary>
    public void EndCarry(Vector3 position, Quaternion rotation)
    {
        if (!isCarried)
        {
            return;
        }

        isCarried = false;

        transform.SetParent(_originalParent, true);
        transform.SetPositionAndRotation(position, rotation);

        for (int i = 0; i < _bodies.Length; i++)
        {
            Rigidbody body = _bodies[i];

            if (body == null)
            {
                continue;
            }

            body.interpolation = _originalInterpolation[i];
            body.isKinematic = _originalKinematic[i];
            body.detectCollisions = true;

#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = Vector3.zero;
#else
            body.velocity = Vector3.zero;
#endif
            body.angularVelocity = Vector3.zero;
        }

        Log("내려놓음");
    }

    /// <summary>드럼통 등에 처리합니다. 계층을 원래대로 돌린 뒤 통째로 비활성화합니다.</summary>
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isCarried = false;
        isDisposed = true;

        if (_originalParent != null)
        {
            transform.SetParent(_originalParent, true);
        }

        if (_victimRoot != null)
        {
            _victimRoot.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        Log("처리 완료");
    }

    private void Log(string message)
    {
        if (isDebugLog)
        {
            Debug.Log($"[CarriableBody] {_victimRoot?.name} - {message}", this);
        }
    }
}
