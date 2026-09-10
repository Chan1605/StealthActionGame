using UnityEngine;

public class DummyDetectable : MonoBehaviour, IDetectable
{
    [Header("테스트용 토글 (인스펙터에서 조작)")]
    [SerializeField] private bool isStealthed;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool isMoving;
    [SerializeField] private float testSoundIntensity;

    public Vector3 Position => transform.position;
    public bool IsStealthed => isStealthed;
    public bool IsCrouching => isCrouching;
    public bool IsMoving => isMoving;
    public float SoundIntensity => testSoundIntensity;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isStealthed ? new Color(0.3f, 0.3f, 0.3f, 0.6f) : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
#endif
}
