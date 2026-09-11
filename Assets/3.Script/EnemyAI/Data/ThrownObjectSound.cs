using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownObjectSound : MonoBehaviour
{
    [SerializeField] private float soundIntensity = 70f;
    [SerializeField] private float soundRadius = 25f;
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private float armDelayAfterRelease = 0.15f;

    private bool _hasTriggered;
    private bool _armed = true;
    private HoldableItem _holdable;

    private void Awake()
    {
        TryGetComponent(out _holdable);
    }

    private void OnEnable()
    {
        if (_holdable != null)
        {
            _armed = !_holdable.isHeld;
            _holdable.OnHeld += HandleHeld;
            _holdable.OnReleased += HandleReleased;
        }
    }

    private void OnDisable()
    {
        if (_holdable != null)
        {
            _holdable.OnHeld -= HandleHeld;
            _holdable.OnReleased -= HandleReleased;
        }
    }

    private void HandleHeld(HoldableItem item)
    {
        _armed = false;
    }

    private void HandleReleased(HoldableItem item)
    {
        _hasTriggered = false;
        CancelInvoke(nameof(Arm));
        Invoke(nameof(Arm), armDelayAfterRelease);
    }

    private void Arm()
    {
        _armed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryRegister(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryRegister(other.gameObject);
    }

    private void TryRegister(GameObject other)
    {
        if (!_armed || _hasTriggered) return;
        if ((surfaceMask.value & (1 << other.layer)) == 0) return;
        _hasTriggered = true;

        EnemyPerception[] enemies = FindObjectsByType<EnemyPerception>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.RegisterSound(transform.position, soundIntensity, true, soundRadius);
        }
    }
}