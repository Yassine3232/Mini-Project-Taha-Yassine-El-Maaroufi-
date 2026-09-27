using UnityEngine;

public class EnemyChaseAttack : MonoBehaviour
{
    public float distanceDetection = 5f;
    public float distanceAttaque = 1.0f;
    public float vitesseCourse = 3f;
    public int degatsAttaque = 10;
    public float tempsEntreAttaques = 1.0f;
    public bool retournerSprite = true;
    public bool bloquerDansLeNiveau = true;
    public float limiteGauche = -28.5f;
    public float limiteDroite = 8.5f;

    private Transform joueur;
    private SpriteRenderer renduSprite;
    private Rigidbody2D corpsPhysique;
    private Animator animateur;
    private EnemyPatrol patrouille;
    private float chronoAttaque = 0f;

    void Start()
    {
        renduSprite = GetComponent<SpriteRenderer>();
        corpsPhysique = GetComponent<Rigidbody2D>();
        animateur = GetComponent<Animator>();
        patrouille = GetComponent<EnemyPatrol>();

        GameObject objetJoueur = GameObject.FindGameObjectWithTag("Player");
        if (objetJoueur != null)
        {
            joueur = objetJoueur.transform;
        }
    }

    void Update()
    {
        if (joueur == null) return;

        if (chronoAttaque > 0f)
        {
            chronoAttaque -= Time.deltaTime;
        }

        float distanceAvecJoueur = Vector2.Distance(transform.position, joueur.position);

        if (distanceAvecJoueur <= distanceDetection)
        {
            if (patrouille != null) patrouille.enabled = false;

            float directionX = (joueur.position.x > transform.position.x) ? 1f : -1f;
            if (retournerSprite && renduSprite != null)
            {
                renduSprite.flipX = directionX < 0;
            }

            if (distanceAvecJoueur > distanceAttaque)
            {
                if (corpsPhysique != null)
                {
                    corpsPhysique.linearVelocity = new Vector2(directionX * vitesseCourse, corpsPhysique.linearVelocity.y);
                }

                if (animateur != null && animateur.runtimeAnimatorController != null)
                {
                    animateur.SetBool("IsRunning", true);
                    animateur.SetBool("IsMoving", false);
                    animateur.SetBool("IsAttacking", false);
                }
            }
            else
            {
                if (corpsPhysique != null)
                {
                    corpsPhysique.linearVelocity = new Vector2(0f, corpsPhysique.linearVelocity.y);
                }

                if (animateur != null && animateur.runtimeAnimatorController != null)
                {
                    animateur.SetBool("IsRunning", false);
                    animateur.SetBool("IsMoving", false);
                }

                if (chronoAttaque <= 0f)
                {
                    AttaquerJoueur();
                }
            }
        }
        else
        {
            if (patrouille != null && !patrouille.enabled)
            {
                patrouille.enabled = true;
            }

            if (animateur != null && animateur.runtimeAnimatorController != null)
            {
                animateur.SetBool("IsRunning", false);
                animateur.SetBool("IsMoving", true);
                animateur.SetBool("IsAttacking", false);
            }
        }

        if (bloquerDansLeNiveau)
        {
            float positionXBloquee = Mathf.Clamp(transform.position.x, limiteGauche, limiteDroite);
            transform.position = new Vector3(positionXBloquee, transform.position.y, transform.position.z);
        }
    }

    void AttaquerJoueur()
    {
        chronoAttaque = tempsEntreAttaques;

        if (animateur != null && animateur.runtimeAnimatorController != null)
        {
            animateur.SetTrigger("Attack");
            animateur.SetBool("IsAttacking", true);
            Invoke(nameof(FinAnimationAttaque), 0.4f);
        }

        IDamageable cibleJoueur = joueur.GetComponent<IDamageable>();
        if (cibleJoueur != null)
        {
            cibleJoueur.RecevoirDegats(degatsAttaque);
        }
    }

    void FinAnimationAttaque()
    {
        if (animateur != null && animateur.runtimeAnimatorController != null)
        {
            animateur.SetBool("IsAttacking", false);
        }
    }
}
