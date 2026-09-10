using UnityEngine;

public class PlayerDetectable : MonoBehaviour, IDetectable
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float runSpeedThreshold = 4f;
    [SerializeField] private float walkSoundScore = 20f;
    [SerializeField] private float runSoundScore = 40f;

    [Header("디버그 (읽기 전용)")]
    [SerializeField] private float debugSpeed;
    [SerializeField] private float debugSoundIntensity;

    public Vector3 Position => transform.position;
    public bool IsStealthed => false;
    public bool IsCrouching => false;

    public float SoundIntensity
    {
        get
        {
            debugSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

            float intensity;
            if (debugSpeed >= runSpeedThreshold) intensity = runSoundScore;
            else if (debugSpeed >= walkSpeedThreshold) intensity = walkSoundScore;
            else intensity = 0f;

            if (IsCrouching) intensity *= 0.5f;

            debugSoundIntensity = intensity;
            return intensity;
        }
    }

    private void Awake()
    {
        if (controller == null)
            TryGetComponent(out controller);

    }
}