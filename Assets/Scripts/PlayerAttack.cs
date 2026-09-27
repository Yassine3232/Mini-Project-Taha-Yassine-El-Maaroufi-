using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject prefabBalle;
    public Transform pointDeTir;
    public float vitesseBalle = 10f;
    public Animator animateur;

    private SimplePlayerMovement mouvementJoueur;

    void Start()
    {
        mouvementJoueur = GetComponent<SimplePlayerMovement>();
        if (animateur == null)
        {
            animateur = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Tirer();
        }
    }

    void Tirer()
    {
        if (animateur != null)
        {
            animateur.SetTrigger("AttackTrigger");
        }

        if (prefabBalle != null && pointDeTir != null)
        {
            GameObject nouvelleBalle = Instantiate(prefabBalle, pointDeTir.position, Quaternion.identity);

            Rigidbody2D rigidbodyBalle = nouvelleBalle.GetComponent<Rigidbody2D>();
            if (rigidbodyBalle != null)
            {
                float directionTir = (mouvementJoueur != null && !mouvementJoueur.regardeADroite) ? -1f : 1f;
                rigidbodyBalle.linearVelocity = new Vector2(directionTir * vitesseBalle, 0f);
            }
        }
    }
}
