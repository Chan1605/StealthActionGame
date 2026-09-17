using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DayToNightTrigger : MonoBehaviour
{
    [Header("조건")]
    [SerializeField] private int requiredKeyCount = 3;
    [SerializeField] private string[] requiredKeyIds; // 3개의 키 id를 순서대로

    [Header("연출")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Text messageText;
    [SerializeField] private string message = "밤이 되었습니다...";
    [SerializeField] private float messageHoldDuration = 2.5f;
    [SerializeField] private float fadeDuration = 2f;

    [Header("씬")]
    [SerializeField] private string nightSceneName;

    private KeyInventory _keyInventory;
    private bool _isTriggered;

    private void Start()
    {
        _keyInventory = FindAnyObjectByType<KeyInventory>();
    }

    private void Update()
    {
        if (_isTriggered) return;

        if (_keyInventory == null)
        {
            _keyInventory = FindAnyObjectByType<KeyInventory>();
            if (_keyInventory == null) return;
        }

        if (HasAllRequiredKeys())
        {
            _isTriggered = true;
            StartCoroutine(TransitionToNight_co());
        }
    }

    private bool HasAllRequiredKeys()
    {
        if (requiredKeyIds == null || requiredKeyIds.Length == 0) return false;

        int count = 0;
        foreach (string keyId in requiredKeyIds)
        {
            bool has = _keyInventory.HasKey(keyId);
            if (has) count++;
        }

        return count >= requiredKeyCount;
    }

    private IEnumerator TransitionToNight_co()
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageHoldDuration);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            yield return fadeCanvasGroup.DOFade(1f, fadeDuration).WaitForCompletion();
        }

        GameSession.Instance?.SavePlayerKeys(_keyInventory);

        if (SceneTransitionMgr.Instance != null)
        {
            SceneTransitionMgr.Instance.LoadScene(nightSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nightSceneName);
        }
    }
}