using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class EnemyPerception : MonoBehaviour
{
    [SerializeField] private Transform eyeOrigin;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private EnemyAIData _data;
    private IDetectable _target;
    public IDetectable Target => _target;

    public float VisionScore { get; private set; }
    public float HearingScore { get; private set; }
    public float MaxScore => Mathf.Max(VisionScore, HearingScore);
    public Vector3 LastKnownPosition { get; private set; }
    public bool IsCurrentlySensing { get; private set; }
    public Vector3 SoundMemoryPosition { get; private set; }
    private float _soundMemoryIntensity;
    private float _soundLockTimer;
    private bool _soundRegisteredThisFrame;

    public void Initialize(EnemyAIData data, IDetectable target)
    {
        _data = data;
        _target = target;
        if (eyeOrigin == null) eyeOrigin = transform;
    }

    public void Tick(float deltaTime)
    {
        if (_target == null) return;

        TickVision(deltaTime);
        TickHearing(deltaTime);
    }

    private void TickVision(float deltaTime)
    {
        if (_target.IsStealthed)
        {
            VisionScore = Mathf.Max(0f, VisionScore - _data.scoreDecayPerSec * deltaTime);
            IsCurrentlySensing = false;
            return;
        }

        float distance = Vector3.Distance(eyeOrigin.position, _target.Position);
        Vector3 toTarget = _target.Position - eyeOrigin.position;


        float instantAngle = Vector3.Angle(eyeOrigin.forward, toTarget); // 근접 인식: 넓은 각도(정면+측면)까지만, 완전한 등 뒤는 제외

        if (distance <= _data.instantDetectRange
            && instantAngle <= _data.instantDetectAngle * 0.5f
            && HasLineOfSight(distance))
        {
            VisionScore = _data.maxScore;
            LastKnownPosition = _target.Position;
            IsCurrentlySensing = true;
            return;
        }

        float angle = instantAngle; // 위에서 이미 계산한 각도 재사용
        bool canSee = angle <= _data.viewAngle * 0.5f && HasLineOfSight(distance);

        float rate = 0f;
        if (canSee)
        {
            if (distance <= _data.viewRangeShort) rate = _data.viewScorePerSecShort;
            else if (distance <= _data.viewRangeMid) rate = _data.viewScorePerSecMid;
            else if (distance <= _data.viewRangeLong) rate = _data.viewScorePerSecLong;
            if (_target.IsCrouching) rate *= 0.5f;
        }

        if (rate > 0f)
        {
            VisionScore = Mathf.Min(_data.maxScore, VisionScore + rate * deltaTime);
            LastKnownPosition = _target.Position;
            IsCurrentlySensing = true;
        }
        else
        {
            VisionScore = Mathf.Max(0f, VisionScore - _data.scoreDecayPerSec * deltaTime);
            IsCurrentlySensing = false;
        }
    }

    private void TickHearing(float deltaTime)
    {
        _soundLockTimer = Mathf.Max(0f, _soundLockTimer - deltaTime);
        _soundRegisteredThisFrame = false;

        if (_target != null)
            RegisterSound(_target.Position, _target.SoundIntensity, false);

        if (!_soundRegisteredThisFrame)
            HearingScore = Mathf.Max(0f, HearingScore - _data.scoreDecayPerSec * deltaTime);
    }

    private bool HasLineOfSight(float distance)
    {
        Vector3 dir = (_target.Position - eyeOrigin.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(eyeOrigin.position, dir, distance, obstacleMask);

        foreach (var hit in hits)
        {
            if (Vector3.Distance(hit.point, _target.Position) > 0.5f)
                return false; // 타겟 근처가 아닌 지점에 맞았으면 진짜 장애물
        }
        return true;
    }

    public Vector3 GetSuspectedPosition()
    {
        return VisionScore >= HearingScore ? LastKnownPosition : SoundMemoryPosition;
    }

    public void ReduceScoreSharply()
    {
        VisionScore = Mathf.Max(0f, VisionScore - _data.scoreDropOnFailedCheck);
        HearingScore = Mathf.Max(0f, HearingScore - _data.scoreDropOnFailedCheck);
        _soundLockTimer = 0f;
    }

    public void RegisterSound(Vector3 sourcePosition, float sourceIntensity, bool isInstant)
    {
        if (sourceIntensity <= 0f) return;

        float distance = Vector3.Distance(eyeOrigin.position, sourcePosition);
        float attenuated = sourceIntensity * Mathf.Clamp01(1f - distance / _data.hearingRadius);
        if (attenuated <= 0f) return;

        bool lockActive = _soundLockTimer > 0f;
        if (!lockActive || attenuated >= _soundMemoryIntensity)
        {
            SoundMemoryPosition = sourcePosition;
            _soundMemoryIntensity = attenuated;
            _soundLockTimer = _data.soundMemoryLockDuration;
        }

        float gain = isInstant ? attenuated : attenuated * Time.deltaTime;
        HearingScore = Mathf.Min(_data.maxScore, HearingScore + gain);
        _soundRegisteredThisFrame = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_data == null) return;
        Transform eye = eyeOrigin != null ? eyeOrigin : transform;

        Handles.color = new Color(1f, 0.6f, 0f, 0.15f);
        Vector3 leftEdge = Quaternion.AngleAxis(-_data.viewAngle * 0.5f, Vector3.up) * eye.forward;
        Handles.DrawSolidArc(eye.position, Vector3.up, leftEdge, _data.viewAngle, _data.viewRangeLong);

        Gizmos.color = Color.yellow;
        DrawViewBoundary(eye, -_data.viewAngle * 0.5f);
        DrawViewBoundary(eye, _data.viewAngle * 0.5f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(eye.position, _data.hearingRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(eye.position, _data.instantDetectRange);
    }

    private void DrawViewBoundary(Transform eye, float angleOffset)
    {
        Vector3 dir = Quaternion.AngleAxis(angleOffset, Vector3.up) * eye.forward;
        Gizmos.DrawLine(eye.position, eye.position + dir * _data.viewRangeLong);
    }
#endif
}
