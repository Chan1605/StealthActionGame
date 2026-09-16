using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 position = respawnPoint != null ? respawnPoint.position : transform.position;
        Quaternion rotation = respawnPoint != null ? respawnPoint.rotation : transform.rotation;

        CheckpointManager.Instance?.SetCheckpoint(position, rotation);
        Debug.Log($"[Checkpoint] ภ๚ภๅตส: {position}, Instance={CheckpointManager.Instance != null}");
    }
}