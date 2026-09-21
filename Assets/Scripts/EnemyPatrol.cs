using UnityEngine;

/// <summary>
/// Moves a 2D enemy back and forth between two points (A and B).
/// Attach this to your enemy GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [Tooltip("Leave empty to auto-use the enemy's starting position as Point A.")]
    public Transform pointA;
    [Tooltip("Where the enemy should roam to.")]
    public Transform pointB;

    [Header("Movement")]
    public float speed = 2f;
    [Tooltip("How close the enemy needs to get before it turns around.")]
    public float reachThreshold = 0.1f;
    [Tooltip("Seconds to wait at each point before turning around.")]
    public float waitTime = 0f;

    [Header("Sprite")]
    [Tooltip("Flip the sprite on the X axis when changing direction.")]
    public bool flipSpriteOnTurn = true;

    [Header("Animation")]
    public string isMovingBoolParam = "IsMoving";

    private Rigidbody2D rb;
    private Vector2 currentTarget;
    private bool movingToB = true;
    private float waitTimer = 0f;
    private SpriteRenderer sr;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // If no Point A was assigned, use the enemy's spawn position.
        if (pointA == null)
        {
            GameObject autoPointA = new GameObject(name + "_PointA");
            autoPointA.transform.position = transform.position;
            pointA = autoPointA.transform;
        }

        currentTarget = pointB != null ? pointB.position : transform.position;
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null) return;

        // Handle waiting at a point
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero; // use rb.velocity instead if on an older Unity version
            SetMoving(false);
            return;
        }

        // Move toward the current target
        Vector2 position = rb.position;
        Vector2 direction = (currentTarget - position).normalized;
        Vector2 newPosition = position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        SetMoving(true);

        // Flip sprite to face movement direction
        if (flipSpriteOnTurn && sr != null && Mathf.Abs(direction.x) > 0.01f)
        {
            sr.flipX = direction.x < 0;
        }

        // Check if we've reached the target
        if (Vector2.Distance(position, currentTarget) <= reachThreshold)
        {
            movingToB = !movingToB;
            currentTarget = movingToB ? (Vector2)pointB.position : (Vector2)pointA.position;
            waitTimer = waitTime;
        }
    }

    void SetMoving(bool moving)
    {
        if (animator != null && !string.IsNullOrEmpty(isMovingBoolParam))
        {
            animator.SetBool(isMovingBoolParam, moving);
        }
    }

    // When EnemyChaseAttack disables this script (chase/attack), make sure
    // the animator doesn't stay stuck thinking the zombie is still roaming.
    void OnDisable()
    {
        SetMoving(false);
    }

    // Visualize the patrol path in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 a = pointA != null ? pointA.position : transform.position;
        Vector3 b = pointB != null ? pointB.position : transform.position;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.DrawWireSphere(b, 0.15f);
    }
}