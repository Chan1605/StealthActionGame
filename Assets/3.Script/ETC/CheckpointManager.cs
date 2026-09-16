using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private Vector3 _position;
    private Quaternion _rotation;
    private bool _hasCheckpoint;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        _position = position;
        _rotation = rotation;
        _hasCheckpoint = true;

    }

    public bool TryGetCheckpoint(out Vector3 position, out Quaternion rotation)
    {
        position = _position;
        rotation = _rotation;
        Debug.Log($"[Checkpoint] Á¶È¸: hasCheckpoint={_hasCheckpoint}, position={_position}");
        return _hasCheckpoint;
    }
}