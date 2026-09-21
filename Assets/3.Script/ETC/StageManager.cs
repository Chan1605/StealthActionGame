using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("주요 오브젝트")]
    [Tooltip("레벨에 따라 상호작용하는 오브젝트를 순서대로 리스트에 넣어주세요.")]
    [SerializeField] private List<GameObject> targetObjectList;

    private Queue<IInteractable> targetObjectQueue = new Queue<IInteractable>();

    private HUDManager hudManager;
    private MinimapManager minimap;
    [Header("미션 식별")]
    [SerializeField] private string missionId = "Mission";
    [Header("UI")]
    [SerializeField] private string missionSelectPrompt = "미션을 선택하세요";
    [SerializeField] private string missionCompletePrompt = "미션 완료";
    [Header("UI 설명 (targetObjectList와 순서 1:1 대응)")]
    [SerializeField] private List<string> targetDescriptions;
    [Header("완료 시 활성화할 오브젝트 (예: 다음 미션 키)")]
    [SerializeField] private GameObject[] unlockOnComplete;

    public event Action OnMissionCompleted;
    public bool isMissionCompleted { get; private set; }
    public bool HasStarted { get; private set; }
    private UI_MissionObjective objectiveUI;
    private Queue<string> descriptionQueue = new Queue<string>();

    private void Awake()
    {
        foreach (GameObject obj in unlockOnComplete)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        minimap = FindAnyObjectByType<MinimapManager>();
        objectiveUI = hudManager.GetObjective();
    }

    public void InitializeQueue()
    {
        HasStarted = true;
        for (int i = 0; i < targetObjectList.Count; i++)
        {
            if (targetObjectList[i].TryGetComponent(out IInteractable t))
            {
                t.OnTargetCompleted += CompleateTarget;
                targetObjectQueue.Enqueue(t);
                descriptionQueue.Enqueue(i < targetDescriptions.Count ? targetDescriptions[i] : string.Empty);
            }
            else
            {
                Debug.Log($"StageManager : {targetObjectList[i].name}에 IInteractable 타입 컴포넌트가 없습니다.");
            }
        }

        IInteractable firstTarget = GetCurrentTarget();

        if (firstTarget == null)
        {
            Debug.Log("StageManager : 다음 타겟 오브젝트가 없습니다.");
            hudManager.GetTargetMarker(missionId).SetMarker_Off();
            minimap.SetObjectTarget(null);
            objectiveUI.Hide();
            return;
        }

        firstTarget.EnableInteraction();
        hudManager.GetTargetMarker(missionId).SetMarker_On(firstTarget.ObjectTransform);
        minimap.SetObjectTarget(firstTarget.ObjectTransform);
        objectiveUI.Show(descriptionQueue.Peek());

    }

    public IInteractable GetCurrentTarget()
    {
        if (targetObjectQueue.Count > 0)
        {
            return targetObjectQueue.Peek();
        }

        return null;
    }

    public void CompleateTarget()
    {
        if (targetObjectQueue.Count <= 0)
        {
            return;
        }

        IInteractable pastTarget = targetObjectQueue.Dequeue();
        if (descriptionQueue.Count > 0) descriptionQueue.Dequeue();
        pastTarget.DisableInteraction();

        IInteractable curTarget = GetCurrentTarget();

        if (curTarget == null)
        {
            Debug.Log("StageManager : 다음 타겟 오브젝트가 없습니다.");
            hudManager.GetTargetMarker(missionId).SetMarker_Off();
            minimap.SetObjectTarget(null);
            isMissionCompleted = true;
            OnMissionCompleted?.Invoke();
            foreach (GameObject obj in unlockOnComplete)
            {
                if (obj != null) obj.SetActive(true);
            }
            StartCoroutine(ShowCompleteThenHide());
            return;
        }

        curTarget.EnableInteraction();
        hudManager.GetTargetMarker(missionId).SetMarker_On(curTarget.ObjectTransform);
        minimap.SetObjectTarget(curTarget.ObjectTransform);
        objectiveUI.Show(descriptionQueue.Peek());
    }

    private IEnumerator ShowCompleteThenHide()
    {
        objectiveUI.Show(missionCompletePrompt);
        yield return new WaitForSeconds(2f);
        objectiveUI.Hide();
    }

}
