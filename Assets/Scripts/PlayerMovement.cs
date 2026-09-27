using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePlayerMovement : MonoBehaviour, IDamageable
{
    public float vitesse = 5f;
    public bool regardeADroite = true;
    public int pointsDeVieMax = 100;
    public int pointsDeVieActuels;
    public bool bloquerDansLeNiveau = true;
    public float limiteGauche = -28.5f;
    public float limiteDroite = 8.5f;

    private Rigidbody2D corpsPhysique;
    private Animator animateur;
    private SpriteRenderer renduSprite;
    private bool estMort = false;

    void Start()
    {
        pointsDeVieActuels = pointsDeVieMax;
        corpsPhysique = GetComponent<Rigidbody2D>();
        animateur = GetComponent<Animator>();
        renduSprite = GetComponent<SpriteRenderer>();

        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.MettreAJourVieJoueur(pointsDeVieActuels, pointsDeVieMax);
        }
    }

    void Update()
    {
        if (estMort) return;

        float deplacementX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) deplacementX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) deplacementX = 1f;

        corpsPhysique.linearVelocity = new Vector2(deplacementX * vitesse, corpsPhysique.linearVelocity.y);

        if (animateur != null)
        {
            animateur.SetFloat("Speed", Mathf.Abs(deplacementX));
        }

        if (deplacementX > 0)
        {
            renduSprite.flipX = false;
            regardeADroite = true;
        }
        else if (deplacementX < 0)
        {
            renduSprite.flipX = true;
            regardeADroite = false;
        }

        if (bloquerDansLeNiveau)
        {
            float positionXBloquee = Mathf.Clamp(transform.position.x, limiteGauche, limiteDroite);
            transform.position = new Vector3(positionXBloquee, transform.position.y, transform.position.z);
        }
    }

    public void RecevoirDegats(int montantDegats)
    {
        if (estMort) return;

        pointsDeVieActuels -= montantDegats;
        if (pointsDeVieActuels < 0) pointsDeVieActuels = 0;

        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.MettreAJourVieJoueur(pointsDeVieActuels, pointsDeVieMax);
        }

        if (animateur != null)
        {
            animateur.SetBool("IsHurt", true);
            Invoke(nameof(FinDegat), 0.25f);
        }

        if (pointsDeVieActuels <= 0)
        {
            Mourir();
        }
    }

    void FinDegat()
    {
        if (animateur != null)
        {
            animateur.SetBool("IsHurt", false);
        }
    }

    void Mourir()
    {
        estMort = true;
        corpsPhysique.linearVelocity = Vector2.zero;

        if (animateur != null)
        {
            animateur.SetBool("IsDead", true);
        }

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.AfficherGameOver();
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
