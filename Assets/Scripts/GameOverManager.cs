using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AfficherGameOver()
    {
        StartCoroutine(AttendreEtChargerGameOver());
    }

    IEnumerator AttendreEtChargerGameOver()
    {
        // Petite pause pour laisser l'animation de mort du joueur se jouer
        yield return new WaitForSecondsRealtime(1.4f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
}
