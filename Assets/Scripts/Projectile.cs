using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float tempsDeVie = 0.5f;
    public int degats = 1;

    void Start()
    {
        Destroy(gameObject, tempsDeVie);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            IDamageable ennemiTouche = collision.GetComponent<IDamageable>();
            if (ennemiTouche != null)
            {
                ennemiTouche.RecevoirDegats(degats);
            }

            Destroy(gameObject);
        }
    }
}
