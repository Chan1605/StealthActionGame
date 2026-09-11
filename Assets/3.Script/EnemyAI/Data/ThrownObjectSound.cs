using UnityEngine;

public class ThrownObjectSound : MonoBehaviour
{
    [SerializeField] private float soundIntensity = 70f;
    [SerializeField] private float soundRadius = 25f;
    [SerializeField] private LayerMask surfaceMask = ~0;
    private bool _hasTriggered;

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasTriggered) return;
        if ((surfaceMask.value & (1 << collision.gameObject.layer)) == 0) return;
        _hasTriggered = true;

        EnemyPerception[] enemies = FindObjectsByType<EnemyPerception>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.RegisterSound(transform.position, soundIntensity, true, soundRadius);
        }
    }
}