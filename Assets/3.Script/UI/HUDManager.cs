using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{

    [Header("UI ╫ц╫╨еш")]
    [SerializeField] private UI_ObjKeyPanal KeyPanal_Prefab;
    [SerializeField] private UI_TargetMarker TargetMarker_Prefab;

    private UI_ObjKeyPanal KeyPanalUI;
    private UI_TargetMarker TargetMarkerUI;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindAnyObjectByType<Canvas>();
    }

    public UI_ObjKeyPanal GetKeyPanal()
    {
        if (KeyPanalUI == null)
        {
            KeyPanalUI = Instantiate(KeyPanal_Prefab, canvas.transform);
        }

        return KeyPanalUI;
    }

    public UI_TargetMarker GetTargetMarker()
    {
        if (TargetMarkerUI == null)
        {
            TargetMarkerUI = Instantiate(TargetMarker_Prefab, canvas.transform);
        }

        return TargetMarkerUI;
    }
}
