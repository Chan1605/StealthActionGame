using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_TutorialPanels : MonoBehaviour
{
    private GameObject[] panels;
    private GameObject[] pages;
    private PlayerInput input;
    private int page_index;
    private int curPage;

    private bool isThrowTut = false;
    private bool isConcealTut = false;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Player").TryGetComponent(out input);

        int panel_count = transform.childCount;
        panels = new GameObject[panel_count];
        for (int i = 0; i < panel_count; i++)
        {
            Transform child = transform.GetChild(i);
            panels[i] = transform.GetChild(i).gameObject;
        }

        pages = panels[0].GetComponentsInChildren<GameObject>();
    }

    private void OnEnable()
    {
        panels[0].SetActive(true);
        input.OnInteractPressed += ClosePanel;
        //이벤트 구독
    }

    public void Next_Page()
    {
        if (curPage.Equals(pages.Length-1))
        {
            return;
        }

        pages[curPage].SetActive(false);
        pages[curPage + 1].SetActive(true);
    }

    public void Prev_Page()
    {
        if (curPage.Equals(0))
        {
            return;
        }

        pages[curPage].SetActive(false);
        pages[curPage - 1].SetActive(true);
    }

    private void ClosePanel()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }


    private void ThrowTutorial()
    {
        //던지기 이벤트 구독 해제
        panels[1].SetActive(true);
    }

    private void concealTutorial()
    {
        //숨기기 이벤트 구독 해제
        panels[1].SetActive(true);
    }

    private void OnDisable()
    {
        input.OnInteractPressed -= ClosePanel;
        //이벤트 구독 해제
    }
}
