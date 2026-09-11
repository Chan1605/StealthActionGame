using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TargetMarker : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private Image Target_img;
    private Text TargetDistance_t;

    private RectTransform rectTransform;

    private Transform playerTransform;
    private Transform targetTransform;

    private Camera cam;

    private void Awake()
    {
        TryGetComponent(out rectTransform);
        TryGetComponent(out canvasGroup);

        transform.GetChild(1).TryGetComponent(out TargetDistance_t);
        transform.GetChild(2).TryGetComponent(out Target_img);

        cam = Camera.main;
        gameObject.SetActive(false);
    }
    private void Start()
    {
        GameObject.FindWithTag("Player").TryGetComponent(out playerTransform);
    }
    private void Update()
    {
        if (targetTransform == null || playerTransform == null)
        {
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(targetTransform.position);

        if (screenPos.z > 0)
        {
            rectTransform.position = screenPos;
        }

        float distance = Vector3.Distance(playerTransform.position, targetTransform.position);
        TargetDistance_t.text = $"{ Mathf.RoundToInt(distance)}m";
    }

    public void SetMarker_On(Transform target)
    {
        targetTransform = target;
        gameObject.SetActive(true);
    }

    public void SetMarker_Off()
    {
        targetTransform = null;
        gameObject.SetActive(false);
    }
}
