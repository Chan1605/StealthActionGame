using UnityEngine;
using UnityEngine.InputSystem;

public class TestScheduleToggle : MonoBehaviour
{
  
    private PrisonScheduleManager _manager;

    private void Awake()
    {
        _manager = GetComponent<PrisonScheduleManager>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            _manager.SetFreeTime(!_manager.IsFreeTime);
            Debug.Log($"[Test] 자유시간 = {_manager.IsFreeTime}");
        }
    }
}