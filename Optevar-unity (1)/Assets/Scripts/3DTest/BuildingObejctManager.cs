using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObjectManager : MonoBehaviour
{
    Bounds bounds;
    
    // 건물 객체의 BoundBox를 생성하여 반환한다.
    // 해당 BoundBox를 참고하여 그 부분에만 NavMesh Surface를 생성할 수 있다.
    public Bounds GetBounds()
    {
        if (bounds.center == Vector3.zero)
        {
            bounds = new Bounds();
            MeshRenderer[] renderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
            Debug.Log(renderers.ToString());
            if (renderers.Length > 0)
            {
                int i = 0;
                for (; i < renderers.Length; i++)
                {
                    if (renderers[i].enabled)
                    {
                        bounds = renderers[i].bounds;
                        break;
                    }
                }
                for (; i < renderers.Length; i++)
                {
                    if (renderers[i].enabled)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }
                }
            }
        }
        return bounds;
    }
}
