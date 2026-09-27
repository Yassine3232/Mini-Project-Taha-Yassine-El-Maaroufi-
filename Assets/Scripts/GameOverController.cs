using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    public string nomSceneJeu = "Verruckt";

    void Start()
    {
        GameObject objetBouton = GameObject.Find("ReplayButton");
        if (objetBouton != null)
        {
            Button bouton = objetBouton.GetComponent<Button>();
            if (bouton != null)
            {
                bouton.onClick.AddListener(RecommencerPartie);
            }
        }
    }

    public void RecommencerPartie()
    {
        SceneManager.LoadScene(nomSceneJeu);
    }
}
