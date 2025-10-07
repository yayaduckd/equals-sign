using UnityEngine;

public class DuckHitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider other) {
        
        // layer must be attackable
        if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
        Debug.Log("DuckHitbox triggered by: " + other.name);
        }
        
    }
    
    
}
