using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance { get; private set; }

    [System.Serializable]
    public class ZombieType
    {
        public string nom = "Zombie";
        public Sprite image;
        public RuntimeAnimatorController controleurAnimation;
        public float hauteurY = -2.3f;
    }

    public int zombiesPourGagner = 10;
    public string messageVictoire = "NIVEAU TERMINE !";
    public string nomSceneSuivante = "";

    public ZombieType[] typesDeZombies;
    public GameObject[] prefabsZombies;

    public float tempsAttenteMin = 1.5f;
    public float tempsAttenteMax = 3.5f;
    public int maxZombiesSimultanes = 5;
    public float limiteGaucheSpawn = -26f;
    public float limiteDroiteSpawn = 7f;
    public float hauteurSpawn = -2.3f;
    public float distanceSecuriteJoueur = 5f;

    public float tailleZombie = 2f;
    public int pointsDeVieZombie = 3;
    public int degatsZombie = 10;
    public float vitesseZombie = 3f;
    public float distanceDetectionZombie = 5f;

    public string titreCarte = "";
    public string sousTitreCarte = "";

    private int nombreZombiesTues = 0;
    private bool niveauGagne = false;
    private readonly List<GameObject> listeZombiesEnVie = new List<GameObject>();

    private Transform joueur;
    private Text texteCompteur;
    private Text texteVie;
    private Text texteVictoire;
    private Button boutonNiveauSuivant;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GameObject objetJoueur = GameObject.FindGameObjectWithTag("Player");
        if (objetJoueur != null)
        {
            joueur = objetJoueur.transform;
        }

        CreerInterface();

        StartCoroutine(BoucleApparitionZombies());
    }

    void CreerInterface()
    {
        GameObject objetCanvas = new GameObject("HUDCanvas");
        Canvas canvas = objetCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        objetCanvas.AddComponent<CanvasScaler>();
        objetCanvas.AddComponent<GraphicRaycaster>();

        Font police = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        texteVie = CreerTexte(objetCanvas.transform, police, "PV : 100 / 100", 30, Color.green);
        DefinirBoite(texteVie.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(300f, 50f));
        texteVie.alignment = TextAnchor.MiddleLeft;

        texteCompteur = CreerTexte(objetCanvas.transform, police, "Zombies : " + zombiesPourGagner, 30, Color.red);
        DefinirBoite(texteCompteur.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(300f, 50f));
        texteCompteur.alignment = TextAnchor.MiddleRight;

        texteVictoire = CreerTexte(objetCanvas.transform, police, messageVictoire, 60, Color.yellow);
        DefinirBoite(texteVictoire.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(800f, 120f));
        texteVictoire.alignment = TextAnchor.MiddleCenter;
        texteVictoire.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(nomSceneSuivante))
        {
            GameObject objetBouton = new GameObject("NextLevelButton");
            objetBouton.transform.SetParent(objetCanvas.transform, false);
            Image imageFond = objetBouton.AddComponent<Image>();
            imageFond.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            boutonNiveauSuivant = objetBouton.AddComponent<Button>();
            boutonNiveauSuivant.onClick.AddListener(PasserAuNiveauSuivant);
            DefinirBoite(objetBouton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(250f, 60f));

            Text texteBouton = CreerTexte(objetBouton.transform, police, "NIVEAU SUIVANT", 24, Color.white);
            DefinirBoite(texteBouton.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            texteBouton.alignment = TextAnchor.MiddleCenter;

            objetBouton.SetActive(false);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject objetEvent = new GameObject("EventSystem");
                objetEvent.AddComponent<EventSystem>();
                objetEvent.AddComponent<StandaloneInputModule>();
            }
        }
    }

    Text CreerTexte(Transform parent, Font police, string texteAfficher, int taillePolice, Color couleurTexte)
    {
        GameObject objetTexte = new GameObject("TexteUI");
        objetTexte.transform.SetParent(parent, false);
        Text composantTexte = objetTexte.AddComponent<Text>();
        composantTexte.font = police;
        composantTexte.fontSize = taillePolice;
        composantTexte.text = texteAfficher;
        composantTexte.color = couleurTexte;
        return composantTexte;
    }

    void DefinirBoite(RectTransform boite, Vector2 ancreMin, Vector2 ancreMax, Vector2 position, Vector2 dimensions)
    {
        boite.anchorMin = ancreMin;
        boite.anchorMax = ancreMax;
        boite.pivot = ancreMin;
        boite.anchoredPosition = position;
        boite.sizeDelta = dimensions;
    }

    IEnumerator BoucleApparitionZombies()
    {
        yield return new WaitForSeconds(1f);

        while (!niveauGagne)
        {
            float tempsAttente = Random.Range(tempsAttenteMin, tempsAttenteMax);
            yield return new WaitForSeconds(tempsAttente);

            if (niveauGagne) yield break;

            listeZombiesEnVie.RemoveAll(z => z == null);

            if (listeZombiesEnVie.Count < maxZombiesSimultanes)
            {
                FaireApparaitreZombie();
            }
        }
    }

    void FaireApparaitreZombie()
    {
        float positionX = Random.Range(limiteGaucheSpawn, limiteDroiteSpawn);
        if (joueur != null && Mathf.Abs(positionX - joueur.position.x) < distanceSecuriteJoueur)
        {
            positionX = (joueur.position.x > (limiteGaucheSpawn + limiteDroiteSpawn) * 0.5f) ? limiteGaucheSpawn : limiteDroiteSpawn;
        }

        float positionY = (hauteurSpawn != 0f) ? hauteurSpawn : ((joueur != null) ? joueur.position.y : -2.3f);
        Vector3 positionSpawn = new Vector3(positionX, positionY, 0f);

        // Si des prefabs de zombies sont disponibles, on instancie directement le prefab
        if (prefabsZombies != null && prefabsZombies.Length > 0)
        {
            GameObject prefabChoisi = prefabsZombies[Random.Range(0, prefabsZombies.Length)];
            GameObject zombieDepuisPrefab = Instantiate(prefabChoisi, positionSpawn, Quaternion.identity);

            zombieDepuisPrefab.transform.localScale = Vector3.one * tailleZombie;

            ZombieHealth santePrefab = zombieDepuisPrefab.GetComponent<ZombieHealth>();
            if (santePrefab != null)
            {
                santePrefab.pointsDeVieMax = pointsDeVieZombie;
            }

            EnemyPatrol patrouillePrefab = zombieDepuisPrefab.GetComponent<EnemyPatrol>();
            if (patrouillePrefab != null)
            {
                patrouillePrefab.limiteGauche = limiteGaucheSpawn;
                patrouillePrefab.limiteDroite = limiteDroiteSpawn;
            }

            EnemyChaseAttack chassePrefab = zombieDepuisPrefab.GetComponent<EnemyChaseAttack>();
            if (chassePrefab != null)
            {
                chassePrefab.degatsAttaque = degatsZombie;
                chassePrefab.vitesseCourse = vitesseZombie;
                chassePrefab.distanceDetection = distanceDetectionZombie;
                chassePrefab.limiteGauche = limiteGaucheSpawn;
                chassePrefab.limiteDroite = limiteDroiteSpawn;
            }

            Collider2D colZombie = zombieDepuisPrefab.GetComponent<Collider2D>();
            if (colZombie != null)
            {
                if (joueur != null)
                {
                    Collider2D colJoueur = joueur.GetComponent<Collider2D>();
                    if (colJoueur != null)
                    {
                        Physics2D.IgnoreCollision(colZombie, colJoueur, true);
                    }
                }

                foreach (GameObject autreZombie in listeZombiesEnVie)
                {
                    if (autreZombie != null)
                    {
                        Collider2D colAutre = autreZombie.GetComponent<Collider2D>();
                        if (colAutre != null)
                        {
                            Physics2D.IgnoreCollision(colZombie, colAutre, true);
                        }
                    }
                }
            }

            listeZombiesEnVie.Add(zombieDepuisPrefab);
            return;
        }

        if (typesDeZombies == null || typesDeZombies.Length == 0) return;
        ZombieType typeChoisi = typesDeZombies[Random.Range(0, typesDeZombies.Length)];

        GameObject nouveauZombie = new GameObject(typeChoisi.nom);
        nouveauZombie.tag = "Enemy";
        nouveauZombie.transform.position = positionSpawn;
        nouveauZombie.transform.localScale = Vector3.one * tailleZombie;

        SpriteRenderer rendu = nouveauZombie.AddComponent<SpriteRenderer>();
        rendu.sprite = typeChoisi.image;
        rendu.sortingLayerName = "Player";

        Rigidbody2D corpsPhysique = nouveauZombie.AddComponent<Rigidbody2D>();
        corpsPhysique.gravityScale = 2f;
        corpsPhysique.constraints = RigidbodyConstraints2D.FreezeRotation;

        PolygonCollider2D collider = nouveauZombie.AddComponent<PolygonCollider2D>();
        collider.isTrigger = false;
        if (typeChoisi.nom.ToLower().Contains("man") && !typeChoisi.nom.ToLower().Contains("woman"))
        {
            collider.points = new Vector2[]
            {
                new Vector2(0f, 0.305f),
                new Vector2(0.09f, 0.26f),
                new Vector2(0.12f, 0.15f),
                new Vector2(0.13f, -0.05f),
                new Vector2(0.10f, -0.325f),
                new Vector2(0f, -0.325f),
                new Vector2(-0.10f, -0.325f),
                new Vector2(-0.13f, -0.05f),
                new Vector2(-0.12f, 0.15f),
                new Vector2(-0.09f, 0.26f)
            };
        }
        else
        {
            collider.points = new Vector2[]
            {
                new Vector2(0f, 0.30f),
                new Vector2(0.08f, 0.25f),
                new Vector2(0.085f, 0.12f),
                new Vector2(0.09f, -0.05f),
                new Vector2(0.07f, -0.32f),
                new Vector2(0f, -0.32f),
                new Vector2(-0.07f, -0.32f),
                new Vector2(-0.095f, -0.05f),
                new Vector2(-0.09f, 0.12f),
                new Vector2(-0.08f, 0.25f)
            };
        }

        if (joueur != null)
        {
            Collider2D colliderJoueur = joueur.GetComponent<Collider2D>();
            if (colliderJoueur != null)
            {
                Physics2D.IgnoreCollision(collider, colliderJoueur, true);
            }
        }

        foreach (GameObject autreZombie in listeZombiesEnVie)
        {
            if (autreZombie != null)
            {
                Collider2D colliderAutre = autreZombie.GetComponent<Collider2D>();
                if (colliderAutre != null)
                {
                    Physics2D.IgnoreCollision(collider, colliderAutre, true);
                }
            }
        }

        if (typeChoisi.controleurAnimation != null)
        {
            Animator anim = nouveauZombie.AddComponent<Animator>();
            anim.runtimeAnimatorController = typeChoisi.controleurAnimation;
        }

        EnemyPatrol patrouille = nouveauZombie.AddComponent<EnemyPatrol>();
        patrouille.vitesseMarche = 1f;
        patrouille.limiteGauche = limiteGaucheSpawn;
        patrouille.limiteDroite = limiteDroiteSpawn;

        GameObject pointDepart = new GameObject("PointDepart");
        float borneA = Mathf.Clamp(positionSpawn.x - 3f, limiteGaucheSpawn, limiteDroiteSpawn);
        pointDepart.transform.position = new Vector3(borneA, positionY, 0f);
        pointDepart.transform.SetParent(nouveauZombie.transform);
        patrouille.pointDepart = pointDepart.transform;

        GameObject pointArrivee = new GameObject("PointArrivee");
        float borneB = Mathf.Clamp(positionSpawn.x + 3f, limiteGaucheSpawn, limiteDroiteSpawn);
        pointArrivee.transform.position = new Vector3(borneB, positionY, 0f);
        pointArrivee.transform.SetParent(nouveauZombie.transform);
        patrouille.pointArrivee = pointArrivee.transform;

        EnemyChaseAttack chasse = nouveauZombie.AddComponent<EnemyChaseAttack>();
        chasse.distanceDetection = distanceDetectionZombie;
        chasse.vitesseCourse = vitesseZombie;
        chasse.degatsAttaque = degatsZombie;
        chasse.limiteGauche = limiteGaucheSpawn;
        chasse.limiteDroite = limiteDroiteSpawn;

        ZombieHealth sante = nouveauZombie.AddComponent<ZombieHealth>();
        sante.pointsDeVieMax = pointsDeVieZombie;

        listeZombiesEnVie.Add(nouveauZombie);
    }

    public void MettreAJourVieJoueur(int vieActuelle, int vieMax)
    {
        if (texteVie != null)
        {
            texteVie.text = "PV : " + vieActuelle + " / " + vieMax;
            texteVie.color = (vieActuelle <= 25) ? Color.red : Color.green;
        }
    }

    public void ZombieElimine()
    {
        if (niveauGagne) return;

        nombreZombiesTues++;

        if (texteCompteur != null)
        {
            int restants = Mathf.Max(0, zombiesPourGagner - nombreZombiesTues);
            texteCompteur.text = "Zombies : " + restants;
        }

        if (nombreZombiesTues >= zombiesPourGagner)
        {
            niveauGagne = true;

            if (texteVictoire != null)
            {
                texteVictoire.gameObject.SetActive(true);
            }

            if (boutonNiveauSuivant != null)
            {
                boutonNiveauSuivant.gameObject.SetActive(true);
            }
        }
    }

    void PasserAuNiveauSuivant()
    {
        if (!string.IsNullOrEmpty(nomSceneSuivante))
        {
            SceneManager.LoadScene(nomSceneSuivante);
        }
    }
}
