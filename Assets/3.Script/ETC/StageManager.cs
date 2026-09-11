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

    private void Start()
    {
        hudManager = FindAnyObjectByType<HUDManager>();
        InitializeQueue();
    }

    private void InitializeQueue()
    {
        for(int i = 0; i <targetObjectList.Count; i++)
        {
            if (targetObjectList[i].TryGetComponent(out IInteractable t))
            {
                t.OnTargetCompleted += CompleateTarget;
                targetObjectQueue.Enqueue(t);
            }
            else
            {
                Debug.Log($"StageManager : {targetObjectList[i].name}에 IInteractable 타입 컴포넌트가 없습니다.");
            }
        }

        IInteractable firstTarget = GetCurrentTarget();

        if(firstTarget == null)
        {
            Debug.Log("StageManager : 다음 타겟 오브젝트가 없습니다.");
            hudManager.GetTargetMarker().SetMarker_Off();

            return;
        }

        firstTarget.EnableInteraction();
        hudManager.GetTargetMarker().SetMarker_On(firstTarget.ObjectTransform);

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
        pastTarget.DisableInteraction();

        IInteractable curTarget = GetCurrentTarget();

        if (curTarget == null)
        {
            Debug.Log("StageManager : 다음 타겟 오브젝트가 없습니다.");
            hudManager.GetTargetMarker().SetMarker_Off();
            return;
        }

        curTarget.EnableInteraction();
        hudManager.GetTargetMarker().SetMarker_On(curTarget.ObjectTransform);
    }

}
