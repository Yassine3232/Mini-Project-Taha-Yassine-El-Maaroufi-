using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float projectileSpeed = 10f;

    public Animator animator;           // Drag your Player's Animator component here in the Inspector
    public float attackAnimationLength = 0.3f; // How long the attack animation plays, in seconds

    public bool isAttacking = false; // Other scripts (like movement) can check this

    private Rigidbody2D rb;
    private SimplePlayerMovement playerMovement;
    private Vector3 firePointStartLocalPos; // firePoint's original position relative to the player

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<SimplePlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerAttack could not find a SimplePlayerMovement component on this GameObject. Make sure both scripts are attached to the same Player object.");
        }

        if (firePoint != null)
        {
            firePointStartLocalPos = firePoint.localPosition;
        }
    }

    void Update()
    {
        // Only allow shooting if the player is standing still (not moving left/right)
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.05f;

        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. Play the attack animation
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }

        // Lock the attack state on, then turn it back off once the animation is done playing
        isAttacking = true;
        Invoke("EndAttack", attackAnimationLength);

        // Figure out which way the player is facing
        bool facingRight = playerMovement == null || playerMovement.facingRight;
        Debug.Log("Shooting. facingRight = " + facingRight);

        // Flip the fire point to the other side of the player if facing left
        if (firePoint != null)
        {
            float xOffset = Mathf.Abs(firePointStartLocalPos.x);
            firePoint.localPosition = new Vector3(facingRight ? xOffset : -xOffset, firePointStartLocalPos.y, firePointStartLocalPos.z);
        }

        // 2. Spawn the projectile
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 3. Give it movement, matching the direction the player is facing
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
            bulletRb.linearVelocity = shootDirection * projectileSpeed;
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }
}