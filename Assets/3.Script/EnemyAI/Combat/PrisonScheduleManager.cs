using System;
using UnityEngine;

public class PrisonScheduleManager : MonoBehaviour
{
    public static PrisonScheduleManager Instance { get; private set; }

    public bool IsFreeTime { get; private set; }

    public event Action<bool> OnScheduleChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void SetFreeTime(bool isFreeTime)
    {
        if (IsFreeTime == isFreeTime) return;
        IsFreeTime = isFreeTime;
        OnScheduleChanged?.Invoke(isFreeTime);
    }
}