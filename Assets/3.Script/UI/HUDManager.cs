using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{

    [Header("UI ╫ц╫╨еш")]
    [SerializeField] private UI_ObjKeyPanal KeyPanal_Prefab;

    private UI_ObjKeyPanal KeyPanalUI;

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
}
