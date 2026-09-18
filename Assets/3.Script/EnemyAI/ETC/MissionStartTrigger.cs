using UnityEngine;

public class MissionStartTrigger : MonoBehaviour
{
    [SerializeField] private StageManager missionManager;
    private bool _isStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (_isStarted) return;
        if (!other.CompareTag("Player")) return;

        _isStarted = true;
        missionManager.InitializeQueue(); // StageManager에 public으로 열어줘야 함
        gameObject.SetActive(false); // 한 번 시작하면 트리거 제거
    }
}