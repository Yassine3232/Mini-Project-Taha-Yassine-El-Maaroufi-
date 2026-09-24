using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 0.5f;   // Destroy the bullet after 0.5 second (halves the travel distance)
    public int damage = 1;        // Each bullet deals 1 damage (3 bullets kill a zombie)

    void Start()
    {
        // Destroy the bullet after a moment so it doesn't stay forever
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Check if what we hit is an enemy
        if (hitInfo.CompareTag("Enemy"))
        {
            // Deal damage to the zombie (through the shared IDamageable interface)
            IDamageable damageable = hitInfo.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Destroy the projectile upon impact with an enemy
            Destroy(gameObject);
        }
    }
}