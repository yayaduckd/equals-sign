using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FadeObstructingObstacles : MonoBehaviour
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private LayerMask obstacleMask;

    private Material fadeMaterial;

    private HashSet<Renderer> currentlyFadedRenderers = new HashSet<Renderer>();

    private void Start()
    {
        fadeMaterial = new Material(Shader.Find("UnityLibrary/URP/Effects/SoftSeethroughCircle"));
    }

    void Update()
    {
        UpdateObstructingObstacleMaterials();
    }

    private void UpdateObstructingObstacleMaterials()
    {
        Vector3 playerToCamDir = Camera.main.transform.position - transform.position;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, detectionRadius, playerToCamDir.normalized, playerToCamDir.magnitude, obstacleMask);


        HashSet<Renderer> newRenderers = new HashSet<Renderer>();
        foreach (RaycastHit hit in hits)
        {
            newRenderers.UnionWith(GetRenderers(hit.collider.gameObject));
        }

        // Find renderers that are no longer faded and reset them
        HashSet<Renderer> noLongerFadedRenderers = new HashSet<Renderer>(currentlyFadedRenderers);
        noLongerFadedRenderers.ExceptWith(newRenderers);

        RemoveFadeMaterial(noLongerFadedRenderers);

        // Find renderers that need to be faded and apply the fade material
        HashSet<Renderer> newlyFadedRenderers = new HashSet<Renderer>(newRenderers);
        newlyFadedRenderers.ExceptWith(currentlyFadedRenderers);

        AddFadeMaterial(newlyFadedRenderers);
    }

    private void RemoveFadeMaterial(HashSet<Renderer> renderersToReset)
    {
        foreach (Renderer rend in renderersToReset)
        {
            Material[] mats = rend.materials;

            if (!mats.Contains(fadeMaterial)) continue; // Doesn't have the fade material, skip

            Material[] newMats = mats.Where(m => m != fadeMaterial).ToArray(); // Remove the fade material
        }
    }

    private void AddFadeMaterial(HashSet<Renderer> renderersToAddFade)
    {
        foreach (Renderer rend in renderersToAddFade)
        {
            Material[] mats = rend.materials;

            if (mats.Contains(fadeMaterial)) continue; // Already has the fade material, skip

            Material[] newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[newMats.Length - 1] = fadeMaterial;
            rend.materials = newMats;
        }
    }

    private HashSet<Renderer> GetRenderers(GameObject obj)
    {
        HashSet<Renderer> renderers = new HashSet<Renderer>();
        
        obj.TryGetComponent<Renderer>(out Renderer renderer);
        if(renderer != null) renderers.Add(renderer);

        foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>())
        {
            renderers.Add(rend);
        }

        return renderers;
    }
}
