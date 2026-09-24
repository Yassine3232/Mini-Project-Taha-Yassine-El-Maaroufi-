using System.Collections;
using UnityEngine;

/// <summary>
/// Gives a zombie hit points. A bullet deals 1 damage, so with maxHealth = 3
/// it takes exactly 3 bullets to kill a zombie. When the zombie dies it plays
/// a short death effect, notifies the ZombieManager (for the kill counter /
/// level objective) and then destroys itself.
/// </summary>
public class ZombieHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("Number of bullets required to kill this zombie.")]
    public int maxHealth = 3;

    private int currentHealth;
    private bool isDead = false;

    private SpriteRenderer sr;
    private Collider2D col;
    private EnemyChaseAttack chase;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        chase = GetComponent<EnemyChaseAttack>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        Debug.Log($"{name} hit! {currentHealth}/{maxHealth} HP remaining.");

        // Little red flash feedback on hit
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = new Color(1f, 0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = original;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Disable interaction so the corpse can no longer hurt the player
        if (col != null) col.enabled = false;
        if (chase != null) chase.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Report the kill to the manager (counter + level objective)
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.ZombieKilled();
        }

        Debug.Log(name + " has been killed.");
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        float t = 0f;
        float dur = 0.25f;
        Vector3 startScale = transform.localScale;
        Color c = sr != null ? sr.color : Color.white;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            if (sr != null)
            {
                c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t));
                sr.color = c;
            }
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, Mathf.Clamp01(t));
            yield return null;
        }

        Destroy(gameObject);
    }
}