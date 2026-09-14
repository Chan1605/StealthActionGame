using UnityEngine;

public class DummyDamageable : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float debugHealth = 100f;
    [SerializeField] private UI_PlayerHealth healthUI;

    [Header("리커버리")]
    [SerializeField] private float regenDelay = 4f;
    [SerializeField] private float regenInterval = 1f;
    [SerializeField] private float regenAmount = 10f;

    private float _timeSinceLastDamage;
    private float _regenTimer;

    private void Awake()
    {
        if (healthUI == null) healthUI = FindAnyObjectByType<UI_PlayerHealth>();
        debugHealth = maxHealth;
        _timeSinceLastDamage = regenDelay;
    }

    private void Start()
    {
        healthUI?.UpdateHealthUI(debugHealth);
    }

    private void Update()
    {
        if (debugHealth >= maxHealth) return;

        _timeSinceLastDamage += Time.deltaTime;
        if (_timeSinceLastDamage < regenDelay) return;

        _regenTimer += Time.deltaTime;
        if (_regenTimer < regenInterval) return;

        _regenTimer = 0f;
        debugHealth = Mathf.Min(maxHealth, debugHealth + regenAmount);
        healthUI?.UpdateHealthUI(debugHealth);
    }

    public void TakeDamage(float amount)
    {
        debugHealth = Mathf.Max(0f, debugHealth - amount);
        _timeSinceLastDamage = 0f;
        _regenTimer = 0f;
        healthUI?.UpdateHealthUI(debugHealth);
    }
}