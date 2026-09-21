using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TutorialPanels_Night : MonoBehaviour
{
    [Header("감옥 바로 앞의 적")]
    [SerializeField] private TakedownVictim victim;

    [SerializeField] private GameObject[] panels;

    private PlayerInput input;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Player").TryGetComponent(out input);
    }

    private void Start()
    {
        panels[0].SetActive(true);
        input.OnInteractPressed += ClosePanel;
        victim.OnK += ConcealTutorial;
    }

    private void ClosePanel()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }

    private void ConcealTutorial()
    {
        victim.OnK -= ConcealTutorial;
        panels[1].SetActive(true);
    }

    private void OnDestroy()
    {
        input.OnInteractPressed -= ClosePanel;
        victim.OnK -= ConcealTutorial;
    }
}
