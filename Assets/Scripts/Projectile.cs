using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 1f; // Changed from 3f to 1f second

    void Start()
    {
        // Destroy the bullet after 1 second so it doesn't stay forever
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Check if what we hit is an enemy
        if (hitInfo.CompareTag("Enemy"))
        {
            // Destroy the projectile upon impact with an enemy
            Destroy(gameObject);
        }
    }
}