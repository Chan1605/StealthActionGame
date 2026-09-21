using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_TutorialPanels_Day : MonoBehaviour
{
    [Header("동선 상 가장 먼저 볼 병 묶음")]
    [SerializeField] private GameObject bottles;

    [SerializeField] private GameObject[] panels;
    private GameObject[] pages;
    private PlayerInput input;
    private int curPage;
    private List<GameObject> bottle_list;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Player").TryGetComponent(out input);
        bottle_list = new List<GameObject>();

        int bottle_num = bottles.transform.childCount;
        for (int i = 0; i < bottle_num; i++)
        {
            Transform bottle = bottles.transform.GetChild(i);
            bottle_list.Add(bottle.gameObject);
        }

        int pageCount = panels[0].transform.childCount;
        pages = new GameObject[pageCount];
        for (int i = 0; i < pageCount; i++)
        {
            pages[i] = panels[0].transform.GetChild(i).gameObject;
        }
    }

    private void Start()
    {
        panels[0].SetActive(true);
        input.OnInteractPressed += ClosePanel;
        foreach (GameObject bottle in bottle_list)
        {
           bottle.TryGetComponent(out GeneralObject g);
            g.OnThrowTutorial += ThrowTutorial;
        }
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
        panels[1].SetActive(true);

        foreach (GameObject bottle in bottle_list)
        {
            if (bottle.TryGetComponent(out GeneralObject g))
            {
                g.OnThrowTutorial -= ThrowTutorial;
            }
        }
    }

    private void OnDestroy()
    {
        input.OnInteractPressed -= ClosePanel;
        foreach (GameObject bottle in bottle_list)
        {
            if(bottle.TryGetComponent(out GeneralObject g))
            {
                g.OnThrowTutorial -= ThrowTutorial;
            }
        }
    }
}
