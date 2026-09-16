using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstepSender : MonoBehaviour
{
    [Header("레이캐스트 설정")]
    [SerializeField] private Transform footTransform;
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private LayerMask floorLayer;

    [Header("자세 데이터")]
    //TODO
    //플레이어 상태 (걷기 뛰기 앉기) 연결
    public float currentStance = 0f;

    public void Step()
    {
        float currentSurface = GetSurfaceType();
        Vector3 soundPos;

        if (footTransform != null)
        {
            soundPos = footTransform.position;
        }
        else
        {
            soundPos = transform.position;
        }

        AudioManager.Instance.PlayFootStep(soundPos, currentSurface, currentStance);
    }

    private float GetSurfaceType()
    {
        Vector3 rayOrigin;

        if (footTransform != null)
        {
            rayOrigin = footTransform.position;
        }
        else
        {
            rayOrigin = transform.position;
        }

        rayOrigin.y += 0.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, floorLayer))
        {
            Collider col = hit.collider;

            if (col.sharedMaterial != null)
            {
                string matname = col.sharedMaterial.name;

                if (matname.Contains("Dirt")|| matname.Contains("Sand"))
                {
                    return 1f;
                }
                if (matname.Contains("Stain")|| matname.Contains("Still"))
                {
                    return 2f;
                }
                if (matname.Contains("Wood"))
                {
                    return 3f;
                }
            }
        }
        return 0f;
    }

}
