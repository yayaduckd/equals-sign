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
        }
        else
        {
            Debug.LogError($"[RainDropletCollider]: collider references null? {_waterCollider != null},  {source != null}");
        }
    }
}
