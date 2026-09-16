using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("연출")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float deathClipLength = 3f;
    [SerializeField] private float postDeathHold = 0.5f;

    [Header("슬로우모션")]
    [SerializeField] private bool isSlowMoEnabled = true;
    [SerializeField] private float slowMoScale = 0.4f;
    [SerializeField] private float slowMoDuration = 0.5f;

    [Header("카메라")]
    [SerializeField] private float deathZoomDistance = 1.5f;

    private CharacterController _controller;
    private PlayerController _movement;
    private PlayerCameraRig _cameraRig;
    private PlayerAnimator _playerAnimator;
    private DummyDamageable _health;
    private PlayerDetectable _detectable;

    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private bool _isDying;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _movement = GetComponent<PlayerController>();
        _cameraRig = GetComponent<PlayerCameraRig>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _health = GetComponent<DummyDamageable>();
        _detectable = GetComponent<PlayerDetectable>();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (_health != null) _health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (_health != null) _health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (_isDying) return;
        StartCoroutine(Die_co());
    }

    private IEnumerator Die_co()
    {
        if (_isDying) yield break; // 이미 죽는 중이면 절대 재진입 안 함
        _isDying = true;
        if (_detectable != null) _detectable.IsDeadOrRespawning = true;
        SetControlEnabled(false);
        _controller.enabled = false;

        _playerAnimator?.PlayDie();
        _cameraRig?.ZoomTo(deathZoomDistance);

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            enemy.ForceForgetPlayer();
        }

        if (isSlowMoEnabled)
        {
            Time.timeScale = slowMoScale;
            yield return new WaitForSecondsRealtime(slowMoDuration);
            Time.timeScale = 1f;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.DOFade(1f, fadeOutDuration);
        }

        float remaining = deathClipLength - (isSlowMoEnabled ? slowMoDuration : 0f);
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        _playerAnimator?.ResetDie(); // 애니메이션이 실제로 끝나는 시점에 맞춰 즉시 리셋
                                     // (화면은 이미 암전 상태라 시각적으로 티 안 남)

        yield return new WaitForSeconds(postDeathHold);

        Respawn();

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.DOFade(0f, fadeInDuration);
            yield return new WaitForSeconds(fadeInDuration);
            fadeCanvasGroup.gameObject.SetActive(false);
        }

        _playerAnimator?.ResetDie();
        SetControlEnabled(true);
        _isDying = false;
    }

    private void Respawn()
    {
        Vector3 pos = _spawnPosition;
        Quaternion rot = _spawnRotation;

        if (CheckpointManager.Instance != null
            && CheckpointManager.Instance.TryGetCheckpoint(out Vector3 checkpointPos, out Quaternion checkpointRot))
        {
            pos = checkpointPos;
            rot = checkpointRot;
        }

        _controller.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        _controller.enabled = true;

        _movement?.StopVertical();
        _cameraRig?.ResetZoom();
        _cameraRig?.ResetLook();

        _health?.ResetHealth();
        if (_detectable != null) _detectable.IsDeadOrRespawning = false;
    }

    private void SetControlEnabled(bool isEnabled)
    {
        if (_movement != null) _movement.enabled = isEnabled;
        if (_cameraRig != null) _cameraRig.enabled = isEnabled;
    }
}