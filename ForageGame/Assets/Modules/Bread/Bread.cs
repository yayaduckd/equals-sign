using UnityEngine;

public class Bread : MonoBehaviour
{
    HealthComponent healthComponent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthComponent = GetComponent<HealthComponent>();
        if (healthComponent == null) {
            Debug.LogError("Bread: No HealthComponent found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
