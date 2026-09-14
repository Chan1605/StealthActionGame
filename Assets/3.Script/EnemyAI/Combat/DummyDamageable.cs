using UnityEngine;

public class DummyDamageable : MonoBehaviour, IDamageable
{
    [SerializeField] private float debugHealth = 100f;

    public void TakeDamage(float amount)
    {
        debugHealth = Mathf.Max(0f, debugHealth - amount);
    }
}