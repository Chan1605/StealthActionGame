using UnityEngine;

public class MissionStartTrigger : MonoBehaviour
{
    [SerializeField] private string missionId = "Mission";
    [SerializeField] private StageManager missionManager;
    [SerializeField] private Collider entranceCollider;

    private HUDManager _hudManager;
    private bool _isCompleted;

    private void Awake()
    {
        if (entranceCollider == null) entranceCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        _hudManager = FindAnyObjectByType<HUDManager>();
        _hudManager.GetTargetMarker(missionId).SetMarker_On(transform);
        MissionSelector.Instance?.Register(this);

        missionManager.OnMissionCompleted += HandleMissionCompleted;
    }

    private void OnDestroy()
    {
        if (missionManager != null) missionManager.OnMissionCompleted -= HandleMissionCompleted;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCompleted) return;
        if (!other.CompareTag("Player")) return;
        if (MissionSelector.Instance != null && !MissionSelector.Instance.CanStart(this)) return;

        MissionSelector.Instance?.SelectMission(this);
        missionManager.InitializeQueue();
    }

    private void HandleMissionCompleted()
    {
        _isCompleted = true;
        _hudManager.GetTargetMarker(missionId).SetMarker_Off();
        MissionSelector.Instance?.ReleaseMission();
        gameObject.SetActive(false);
    }

    public void LockEntrance()
    {
        if (entranceCollider != null) entranceCollider.enabled = false;
        _hudManager.GetTargetMarker(missionId).SetMarker_Off();
    }

    public void UnlockEntrance()
    {
        if (_isCompleted) return; // 이미 완료된 미션은 다시 안 열림
        if (entranceCollider != null) entranceCollider.enabled = true;
        _hudManager.GetTargetMarker(missionId).SetMarker_On(transform);
    }
}