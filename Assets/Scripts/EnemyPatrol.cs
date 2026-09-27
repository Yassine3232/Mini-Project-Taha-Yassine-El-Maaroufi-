using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointDepart;
    public Transform pointArrivee;
    public float vitesseMarche = 2f;
    public bool retournerSprite = true;
    public bool bloquerDansLeNiveau = true;
    public float limiteGauche = -28.5f;
    public float limiteDroite = 8.5f;

    private float positionXMin;
    private float positionXMax;
    private float cibleX;
    private SpriteRenderer renduSprite;
    private Rigidbody2D corpsPhysique;
    private Animator animateur;

    void Start()
    {
        renduSprite = GetComponent<SpriteRenderer>();
        corpsPhysique = GetComponent<Rigidbody2D>();
        animateur = GetComponent<Animator>();

        if (pointDepart != null && pointArrivee != null)
        {
            positionXMin = Mathf.Clamp(Mathf.Min(pointDepart.position.x, pointArrivee.position.x), limiteGauche, limiteDroite);
            positionXMax = Mathf.Clamp(Mathf.Max(pointDepart.position.x, pointArrivee.position.x), limiteGauche, limiteDroite);
        }
        else
        {
            positionXMin = Mathf.Clamp(transform.position.x - 3f, limiteGauche, limiteDroite);
            positionXMax = Mathf.Clamp(transform.position.x + 3f, limiteGauche, limiteDroite);
        }
        cibleX = positionXMax;

        ActiverAnimationMarche();
    }

    void OnEnable()
    {
        if (animateur == null) animateur = GetComponent<Animator>();
        ActiverAnimationMarche();
    }

    void OnDisable()
    {
        if (animateur != null && animateur.runtimeAnimatorController != null)
        {
            animateur.SetBool("IsMoving", false);
        }
    }

    void ActiverAnimationMarche()
    {
        if (animateur != null && animateur.runtimeAnimatorController != null)
        {
            animateur.SetBool("IsMoving", true);
            animateur.SetBool("IsRunning", false);
            animateur.SetBool("IsAttacking", false);
        }
    }

    void Update()
    {
        float directionX = (cibleX > transform.position.x) ? 1f : -1f;

        if (corpsPhysique != null)
        {
            corpsPhysique.linearVelocity = new Vector2(directionX * vitesseMarche, corpsPhysique.linearVelocity.y);
        }

        if (animateur != null && animateur.runtimeAnimatorController != null && !animateur.GetBool("IsMoving"))
        {
            ActiverAnimationMarche();
        }

        if (Mathf.Abs(transform.position.x - cibleX) < 0.4f)
        {
            cibleX = (cibleX == positionXMax) ? positionXMin : positionXMax;
        }

        if (retournerSprite && renduSprite != null)
        {
            renduSprite.flipX = directionX < 0;
        }

        if (bloquerDansLeNiveau)
        {
            float positionXBloquee = Mathf.Clamp(transform.position.x, limiteGauche, limiteDroite);
            transform.position = new Vector3(positionXBloquee, transform.position.y, transform.position.z);
        }
    }
}
