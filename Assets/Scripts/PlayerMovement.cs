using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePlayerMovement : MonoBehaviour, IDamageable
{
    public float moveSpeed = 5f;
    public bool facingRight = true; // Other scripts (like attacking) can check this

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Hurt Flash Duration")]
    [Tooltip("How long the Hurt animation plays before returning to Idle (seconds).")]
    public float hurtDuration = 0.25f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private PlayerAttack playerAttack;

    private bool isDead = false;
    private bool isHurt = false;
    private Coroutine hurtCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {amount} damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHurt();
        }
    }

    void PlayHurt()
    {
        if (anim != null)
        {
            // Stop any previous hurt coroutine first
            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtRoutine());
        }
    }

    IEnumerator HurtRoutine()
    {
        isHurt = true;
        anim.SetBool("IsHurt", true);
        yield return new WaitForSeconds(hurtDuration);
        isHurt = false;
        anim.SetBool("IsHurt", false);
    }

    void Die()
    {
        isDead = true;
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }
        isHurt = false;

        if (anim != null)
        {
            anim.SetBool("IsHurt", false);
            anim.SetBool("IsDead", true);
        }

        // Freeze the soldier in place
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Debug.Log("Player has been defeated! Loading Game Over screen...");
        StartCoroutine(LoadGameOverScreen());
    }

    IEnumerator LoadGameOverScreen()
    {
        // Wait for the death animation to play
        yield return new WaitForSeconds(1.4f);
        SceneManager.LoadScene("GameOver");
    }

    void Update()
    {
        // Disable all input once dead
        if (isDead) return;

        // Left and Right Movement (A and D keys)
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        // Apply movement to the Rigidbody
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Send movement data to the Animator
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(moveInput));

        // Don't flip the sprite while the attack or hurt animation is playing
        bool isAttacking = playerAttack != null && playerAttack.isAttacking;

        if (!isAttacking && !isHurt && spriteRenderer != null)
        {
            if (moveInput > 0)
            {
                spriteRenderer.flipX = false;
                facingRight = true;
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = true;
                facingRight = false;
            }
        }
    }
}