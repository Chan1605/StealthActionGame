using System.Collections.Generic;
using UnityEngine;

public class MissionSelector : MonoBehaviour
{
    public static MissionSelector Instance { get; private set; }

    private readonly List<MissionStartTrigger> _allTriggers = new List<MissionStartTrigger>();
    private MissionStartTrigger _activeMission;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(MissionStartTrigger trigger)
    {
        _allTriggers.Add(trigger);
    }

    public bool CanStart(MissionStartTrigger trigger)
    {
        return _activeMission == null || _activeMission == trigger;
    }

    public void SelectMission(MissionStartTrigger chosen)
    {
        _activeMission = chosen;

        foreach (MissionStartTrigger trigger in _allTriggers)
        {
            if (trigger != chosen)
            {
                trigger.LockEntrance();
            }
        }
    }

    public void ReleaseMission()
    {
        _activeMission = null;

        foreach (MissionStartTrigger trigger in _allTriggers)
        {
            trigger.UnlockEntrance();
        }
    }
}