using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshCollider))]
public class RaindDropletCollider : MonoBehaviour
{
    [SerializeField] private MeshCollider _waterCollider;

    void Awake()
    {
        // Get the authoritative collider ProBuilder manages
        MeshCollider source = transform.parent.GetComponent<MeshCollider>();
        
        if (_waterCollider != null && source != null)
        {
            // Copy the mesh ProBuilder assigned at runtime
            _waterCollider.sharedMesh = source.sharedMesh;
            Debug.LogError($"waaaaaa???: {source != null},  {source.sharedMesh}");
        }
        else
        {
            Debug.LogError($"waa???: {_waterCollider != null},  {source != null}");
        }
    }

    // void Awake()
    // {
    //     StartCoroutine(Start());
    // }


    // IEnumerator Start()
    // {
    //     MeshCollider source = GetComponentInParent<MeshCollider>();
    //     // Wait until ProBuilder has actually assigned the mesh
    //     while (source.sharedMesh == null)
    //     {
    //         Debug.LogError($"pro builder sucks: {source.sharedMesh != null}");
    //         yield return null;
    //     }
    //     Debug.LogError($"yippee");
    //     _waterCollider.sharedMesh = source.sharedMesh;
    // }
}
