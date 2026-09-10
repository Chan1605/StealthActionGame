using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class RebindButton : MonoBehaviour
{
    [SerializeField] private InputActionReference actionRef;
    [SerializeField] private int bindingIndex;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Text keyLabel;

    private InputActionRebindingExtensions.RebindingOperation rebindOp;

    private void Awake()
    {
        rebindButton.onClick.AddListener(StartRebind);
        UpdateLabel();
    }

    private void StartRebind()
    {
        rebindButton.interactable = false;
        keyLabel.text = "...";

        actionRef.action.Disable();
        rebindOp = actionRef.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                actionRef.action.Enable();
                rebindButton.interactable = true;
                UpdateLabel();
                op.Dispose();
            })
            .OnCancel(op =>
            {
                actionRef.action.Enable();
                rebindButton.interactable = true;
                UpdateLabel();
                op.Dispose();
            })
            .Start();
    }

    private void UpdateLabel()
    {
        keyLabel.text = actionRef.action.GetBindingDisplayString(bindingIndex);
    }

    private void OnDestroy()
    {
        rebindOp?.Dispose();
    }
}
