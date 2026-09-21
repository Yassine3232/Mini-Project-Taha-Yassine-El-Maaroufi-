using System.Collections;
using UnityEngine;

/// <summary>
/// Détecte le joueur, le poursuit en courant et l'attaque au corps à corps.
/// Fonctionne en équipe avec EnemyPatrol : désactive la patrouille
/// pendant la poursuite/attaque, puis la réactive quand le joueur s'éloigne.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseAttack : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Distance à laquelle l'ennemi détecte le joueur.")]
    public float detectionRange = 5f;
    [Tooltip("Layer du joueur (assigne le layer 'Player' dans l'inspecteur).")]
    public LayerMask playerLayer;
    [Tooltip("Distance horizontale à laquelle l'ennemi arrête de courir et attaque à la place.")]
    public float attackRange = 1.8f;
    [Tooltip("Tolérance de hauteur maximale pour autoriser l'attaque.")]
    public float verticalTolerance = 2.0f;

    [Header("Movement")]
    public float chaseSpeed = 3f;

    [Header("Attack")]
    [Tooltip("Temps d'attente entre deux attaques (secondes).")]
    public float attackCooldown = 0.8f;
    public int attackDamage = 10;
    [Tooltip("Durée de l'animation d'attaque (secondes). Permet de réinitialiser l'état même sans Animation Event.")]
    public float attackDuration = 0.4f;
    [Tooltip("Délai avant l'impact du coup pendant l'animation d'attaque.")]
    public float damageDelay = 0.2f;
    [Tooltip("Nom du trigger d'animation pour l'attaque.")]
    public string attackAnimTrigger = "Attack";
    [Tooltip("Nom du paramètre Bool dans l'Animator qui indique qu'une attaque est en cours.")]
    public string isAttackingBoolParam = "IsAttacking";

    [Header("Animation Parameters")]
    [Tooltip("Nom du paramètre Bool pour la course.")]
    public string isRunningBoolParam = "IsRunning";
    [Tooltip("Nom du paramètre Bool pour le mouvement (marche / patrouille).")]
    public string isMovingBoolParam = "IsMoving";

    [Header("Sprite")]
    public bool flipSpriteOnTurn = true;

    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private EnemyPatrol patrol; // référence au script de patrouille séparé

    private Transform player;
    private float attackTimer = 0f;
    private bool isAttacking = false; // true pendant que l'anim d'attaque joue
    private bool damageDealtThisAttack = false;
    private Coroutine attackCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        patrol = GetComponent<EnemyPatrol>(); // optionnel, peut être null si pas de patrouille
    }

    void FixedUpdate()
    {
        // Décompte continu du timer d'attaque pour être toujours prêt lors de l'engagement
        if (attackTimer > 0f)
        {
            attackTimer -= Time.fixedDeltaTime;
        }

        DetectPlayer();

        switch (currentState)
        {
            case State.Idle:
                // EnemyPatrol (s'il existe) gère le mouvement et ses animations
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        State previousState = currentState;

        if (hit != null)
        {
            player = hit.transform;
            float distX = Mathf.Abs(transform.position.x - player.position.x);
            float distY = Mathf.Abs(transform.position.y - player.position.y);
            bool inAttackReach = distX <= attackRange && distY <= verticalTolerance;

            currentState = inAttackReach ? State.Attack : State.Chase;
        }
        else
        {
            player = null;
            currentState = State.Idle;
            attackTimer = 0f;
        }

        // Bascule la patrouille ON/OFF
        if (patrol != null && patrol.enabled != (currentState == State.Idle))
        {
            patrol.enabled = (currentState == State.Idle);
        }

        // Si on quitte la poursuite / attaque pour revenir en Idle, réinitialise les animations
        if (previousState != currentState && currentState == State.Idle)
        {
            SetAnimBool(isRunningBoolParam, false);
            SetAnimBool(isAttackingBoolParam, false);
            isAttacking = false;
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }
    }

    void Chase()
    {
        if (player == null) return;

        // Pendant une attaque active, ne pas glisser
        if (isAttacking) return;

        SetAnimBool(isRunningBoolParam, true);
        SetAnimBool(isMovingBoolParam, true);
        SetAnimBool(isAttackingBoolParam, false);

        float dx = player.position.x - transform.position.x;
        float dirX = Mathf.Sign(dx);
        Vector2 position = rb.position;
        Vector2 targetPos = new Vector2(position.x + dirX * chaseSpeed * Time.fixedDeltaTime, position.y);
        rb.MovePosition(targetPos);

        if (Mathf.Abs(dx) > 0.05f)
        {
            FaceDirection(new Vector2(dirX, 0));
        }
    }

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.05f)
        {
            FaceDirection(new Vector2(dx, 0));
        }

        SetAnimBool(isRunningBoolParam, false);
        SetAnimBool(isMovingBoolParam, false);

        // Si l'animation d'attaque en cours n'est pas terminée, attendre
        if (isAttacking) return;

        // Dès que le cooldown est écoulé, lancer une attaque
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            DoAttack();
        }
    }

    void DoAttack()
    {
        isAttacking = true;
        damageDealtThisAttack = false;

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(attackAnimTrigger)) animator.SetTrigger(attackAnimTrigger);
            if (!string.IsNullOrEmpty(isAttackingBoolParam)) animator.SetBool(isAttackingBoolParam, true);
        }

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(damageDelay);
        if (!damageDealtThisAttack)
        {
            DealDamage();
        }

        float remaining = Mathf.Max(0f, attackDuration - damageDelay);
        yield return new WaitForSeconds(remaining);
        EndAttack();
    }

    void DealDamage()
    {
        damageDealtThisAttack = true;
        if (player == null) return;

        float distX = Mathf.Abs(transform.position.x - player.position.x);
        float distY = Mathf.Abs(transform.position.y - player.position.y);
        if (distX > attackRange * 1.5f || distY > verticalTolerance * 1.5f) return;

        var damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
        }

        Debug.Log(name + " attaque le joueur pour " + attackDamage + " dégâts.");
    }

    void EndAttack()
    {
        isAttacking = false;
        if (animator != null && !string.IsNullOrEmpty(isAttackingBoolParam))
        {
            animator.SetBool(isAttackingBoolParam, false);
        }
    }

    /// <summary>
    /// Support pour Animation Event : sur la frame de fin de l'attaque
    /// </summary>
    public void AnimEvent_AttackEnd()
    {
        EndAttack();
    }

    /// <summary>
    /// Support pour Animation Event : sur la frame d'impact du coup
    /// </summary>
    public void AnimEvent_DealDamage()
    {
        DealDamage();
    }

    void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    void FaceDirection(Vector2 direction)
    {
        if (flipSpriteOnTurn && sr != null && Mathf.Abs(direction.x) > 0.01f)
        {
            sr.flipX = direction.x < 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRange * 2f, verticalTolerance * 2f, 0.1f));
    }
}