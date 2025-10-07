using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Hit(int damage) {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);
        if (currentHealth <= 0) {
            Die();
        }
    }
    
    void Die() {
        Debug.Log(gameObject.name + " died.");
        // play death animation, sound, etc.
        Destroy(gameObject);
    }
}
