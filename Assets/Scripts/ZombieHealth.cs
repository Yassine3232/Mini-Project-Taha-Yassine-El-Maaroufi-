using System.Collections;
using UnityEngine;

public class ZombieHealth : MonoBehaviour, IDamageable
{
    public int pointsDeVieMax = 3;

    private int pointsDeVieActuels;
    private SpriteRenderer renduSprite;

    void Start()
    {
        pointsDeVieActuels = pointsDeVieMax;
        renduSprite = GetComponent<SpriteRenderer>();
    }

    public void RecevoirDegats(int montantDegats)
    {
        pointsDeVieActuels -= montantDegats;

        StartCoroutine(ClignoterRouge());

        if (pointsDeVieActuels <= 0)
        {
            Mourir();
        }
    }

    IEnumerator ClignoterRouge()
    {
        if (renduSprite != null)
        {
            renduSprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renduSprite.color = Color.white;
        }
    }

    void Mourir()
    {
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.ZombieElimine();
        }

        Destroy(gameObject);
    }
}
