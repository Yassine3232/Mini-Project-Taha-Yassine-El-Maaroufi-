using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the zombie wave for the level:
///  - Spawns zombies at random intervals at random positions around the map.
///  - Each spawn picks a random zombie TYPE (sprite + animator controller + Y level) so several
///    kinds of zombies appear (Zombie Woman / Zombie Man / Wild Zombie), each fully animated.
///  - Spawned zombies patrol back and forth and chase / attack the player.
///  - Displays an on-screen "ZOMBIES LEFT" counter inside the camera view.
///  - The player must kill <see cref="killsToWin"/> zombies (default 10) to complete the level.
/// </summary>
public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance { get; private set; }

    /// <summary>
    /// One kind of zombie: its idle sprite, the animator controller driving its clips,
    /// and the Y level it spawns/walks at (so each type can sit on its own line).
    /// </summary>
    [System.Serializable]
    public class ZombieType
    {
        public string name = "Zombie";
        public Sprite sprite;
        public RuntimeAnimatorController controller;
        [Tooltip("Y level this zombie type spawns and walks at (regulate per type).")]
        public float yPosition = -2.3f;
    }

    [Header("Objective")]
    [Tooltip("How many zombies the player must kill to succeed.")]
    public int killsToWin = 10;

    [Header("Zombie Types (variety)")]
    [Tooltip("Each entry is a zombie kind (sprite + animator controller + Y level). A random one is used per spawn.")]
    public ZombieType[] zombieTypes;

    [Header("Spawning")]
    [Tooltip("Minimum / maximum random delay between two spawns (seconds).")]
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3.5f;
    [Tooltip("Maximum number of spawned zombies alive at the same time.")]
    public int maxAliveZombies = 5;
    [Tooltip("Horizontal spawn limits (inside the map edges).")]
    public float spawnMinX = -26f;
    public float spawnMaxX = 7f;
    [Tooltip("Fallback Y level used only if a zombie type has no specific Y.")]
    public float spawnY = -2.3f;
    [Tooltip("Never spawn a zombie closer than this to the player.")]
    public float minSpawnDistanceFromPlayer = 5f;

    [Header("Zombie Settings")]
    [Tooltip("How far a zombie can detect the player (sets the detection range on every spawned zombie).")]
    public float zombieDetectionRange = 5f;
    public float zombieScale = 2f;
    public int zombieHealth = 3;   // 3 bullets to kill
    public int zombieDamage = 10;
    public float zombieChaseSpeed = 3f;

    private int kills = 0;
    private readonly List<GameObject> aliveZombies = new List<GameObject>();

    private Transform player;
    private Text counterText;
    private Text winText;

    // Health UI elements
    private RectTransform healthBarFill;
    private Image healthBarFillImage;
    private Text healthText;
    private float maxHealthBarWidth = 260f;

    // Map UI elements
    private Text mapTitleCard;
    private Text mapSubtitleCard;
    private Text mapHudTag;

    private bool levelComplete = false;
    private Coroutine spawnRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            var pm = playerGO.GetComponent<SimplePlayerMovement>();
            if (pm != null)
            {
                UpdatePlayerHealth(pm.currentHealth, pm.maxHealth);
            }
        }

        BuildHUD();
        MakeExistingZombiesValid();
        UpdateCounterText();
        StartCoroutine(PlayMapIntroTitle());
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    // ── HUD ──────────────────────────────────────────────────────────────── //
    void BuildHUD()
    {
        GameObject canvasGO = new GameObject("ZombieHUDCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        Font mainFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ── 1. Counter text (top-right of camera view) ──────────────────── //
        GameObject textGO = new GameObject("ZombieCounterText");
        textGO.transform.SetParent(canvasGO.transform, false);
        counterText = textGO.AddComponent<Text>();
        counterText.font = mainFont;
        counterText.fontSize = 40;
        counterText.fontStyle = FontStyle.Bold;
        counterText.color = new Color(1f, 0.25f, 0.2f, 1f);
        counterText.alignment = TextAnchor.UpperRight;
        counterText.horizontalOverflow = HorizontalWrapMode.Overflow;
        counterText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = counterText.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-40f, -30f);
        rt.sizeDelta = new Vector2(700f, 90f);

        Shadow shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        // ── 2. Player Health UI (top-left) ──────────────────────────────── //
        GameObject healthPanel = new GameObject("HealthPanel");
        healthPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRT = healthPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 1f);
        panelRT.anchorMax = new Vector2(0f, 1f);
        panelRT.pivot = new Vector2(0f, 1f);
        panelRT.anchoredPosition = new Vector2(40f, -30f);
        panelRT.sizeDelta = new Vector2(320f, 75f);

        // Dark background for health bar
        Image panelBg = healthPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.08f, 0.75f);

        // Health Bar Track (dark border/inset)
        GameObject trackGO = new GameObject("HealthTrack");
        trackGO.transform.SetParent(healthPanel.transform, false);
        RectTransform trackRT = trackGO.AddComponent<RectTransform>();
        trackRT.anchorMin = new Vector2(0f, 0.5f);
        trackRT.anchorMax = new Vector2(0f, 0.5f);
        trackRT.pivot = new Vector2(0f, 0.5f);
        trackRT.anchoredPosition = new Vector2(15f, -12f);
        trackRT.sizeDelta = new Vector2(maxHealthBarWidth, 20f);
        Image trackImg = trackGO.AddComponent<Image>();
        trackImg.color = new Color(0.2f, 0.05f, 0.05f, 0.9f);

        // Health Bar Fill
        GameObject fillGO = new GameObject("HealthFill");
        fillGO.transform.SetParent(trackGO.transform, false);
        healthBarFill = fillGO.AddComponent<RectTransform>();
        healthBarFill.anchorMin = new Vector2(0f, 0f);
        healthBarFill.anchorMax = new Vector2(0f, 1f);
        healthBarFill.pivot = new Vector2(0f, 0.5f);
        healthBarFill.anchoredPosition = Vector2.zero;
        healthBarFill.sizeDelta = new Vector2(maxHealthBarWidth, 0f);
        healthBarFillImage = fillGO.AddComponent<Image>();
        healthBarFillImage.color = new Color(0.15f, 0.85f, 0.25f, 1f);

        // Health Label / Value Text
        GameObject healthTextGO = new GameObject("HealthText");
        healthTextGO.transform.SetParent(healthPanel.transform, false);
        healthText = healthTextGO.AddComponent<Text>();
        healthText.font = mainFont;
        healthText.fontSize = 24;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        healthText.alignment = TextAnchor.MiddleLeft;
        healthText.text = "+ 100 / 100 HP";
        RectTransform htRT = healthText.rectTransform;
        htRT.anchorMin = new Vector2(0f, 0.5f);
        htRT.anchorMax = new Vector2(1f, 0.5f);
        htRT.pivot = new Vector2(0f, 0.5f);
        htRT.anchoredPosition = new Vector2(15f, 16f);
        htRT.sizeDelta = new Vector2(-30f, 28f);

        Shadow htShadow = healthTextGO.AddComponent<Shadow>();
        htShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        htShadow.effectDistance = new Vector2(1.5f, -1.5f);

        // ── 3. Persistent HUD Map Indicator (below health panel) ────────── //
        GameObject mapTagGO = new GameObject("MapHudTag");
        mapTagGO.transform.SetParent(canvasGO.transform, false);
        mapHudTag = mapTagGO.AddComponent<Text>();
        mapHudTag.font = mainFont;
        mapHudTag.fontSize = 20;
        mapHudTag.fontStyle = FontStyle.Bold;
        mapHudTag.color = new Color(0.9f, 0.75f, 0.2f, 0.9f);
        mapHudTag.alignment = TextAnchor.MiddleLeft;
        mapHudTag.text = "MAP: VERR\u00DCCKT (Wittenau Sanatorium)";

        RectTransform mtRT = mapHudTag.rectTransform;
        mtRT.anchorMin = new Vector2(0f, 1f);
        mtRT.anchorMax = new Vector2(0f, 1f);
        mtRT.pivot = new Vector2(0f, 1f);
        mtRT.anchoredPosition = new Vector2(42f, -112f);
        mtRT.sizeDelta = new Vector2(500f, 30f);

        Shadow mtShadow = mapTagGO.AddComponent<Shadow>();
        mtShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        mtShadow.effectDistance = new Vector2(1.5f, -1.5f);

        // ── 4. Cinematic CoD Zombies Style Intro Title Card ─────────────── //
        GameObject introCardGO = new GameObject("MapIntroCard");
        introCardGO.transform.SetParent(canvasGO.transform, false);
        RectTransform introRT = introCardGO.AddComponent<RectTransform>();
        introRT.anchorMin = new Vector2(0.5f, 0.65f);
        introRT.anchorMax = new Vector2(0.5f, 0.65f);
        introRT.pivot = new Vector2(0.5f, 0.5f);
        introRT.anchoredPosition = Vector2.zero;
        introRT.sizeDelta = new Vector2(1000f, 180f);

        // Main German Title: "VERRÜCKT"
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(introCardGO.transform, false);
        mapTitleCard = titleGO.AddComponent<Text>();
        mapTitleCard.font = mainFont;
        mapTitleCard.fontSize = 72;
        mapTitleCard.fontStyle = FontStyle.Bold;
        mapTitleCard.color = new Color(0.95f, 0.15f, 0.15f, 0f); // Starts transparent
        mapTitleCard.alignment = TextAnchor.MiddleCenter;
        mapTitleCard.text = "VERR\u00DCCKT";

        RectTransform titRT = mapTitleCard.rectTransform;
        titRT.anchorMin = new Vector2(0f, 0.4f);
        titRT.anchorMax = new Vector2(1f, 1f);
        titRT.pivot = new Vector2(0.5f, 0.5f);
        titRT.anchoredPosition = Vector2.zero;
        titRT.sizeDelta = Vector2.zero;

        Shadow titShadow = titleGO.AddComponent<Shadow>();
        titShadow.effectColor = new Color(0f, 0f, 0f, 1f);
        titShadow.effectDistance = new Vector2(3f, -3f);

        // Subtitle: "Berlin, Germany — Wittenau Sanatorium"
        GameObject subGO = new GameObject("SubtitleText");
        subGO.transform.SetParent(introCardGO.transform, false);
        mapSubtitleCard = subGO.AddComponent<Text>();
        mapSubtitleCard.font = mainFont;
        mapSubtitleCard.fontSize = 26;
        mapSubtitleCard.fontStyle = FontStyle.Normal;
        mapSubtitleCard.color = new Color(0.85f, 0.85f, 0.85f, 0f); // Starts transparent
        mapSubtitleCard.alignment = TextAnchor.MiddleCenter;
        mapSubtitleCard.text = "Berlin, Germany \u2014 Wittenau Sanatorium";

        RectTransform subRT = mapSubtitleCard.rectTransform;
        subRT.anchorMin = new Vector2(0f, 0f);
        subRT.anchorMax = new Vector2(1f, 0.45f);
        subRT.pivot = new Vector2(0.5f, 0.5f);
        subRT.anchoredPosition = Vector2.zero;
        subRT.sizeDelta = Vector2.zero;

        Shadow subShadow = subGO.AddComponent<Shadow>();
        subShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        subShadow.effectDistance = new Vector2(2f, -2f);

        // ── 5. Win text (center) ────────────────────────────────────────── //
        GameObject winGO = new GameObject("LevelCompleteText");
        winGO.transform.SetParent(canvasGO.transform, false);
        winText = winGO.AddComponent<Text>();
        winText.font = mainFont;
        winText.fontSize = 90;
        winText.fontStyle = FontStyle.Bold;
        winText.color = new Color(0.2f, 1f, 0.3f, 1f);
        winText.alignment = TextAnchor.MiddleCenter;
        winText.text = "LEVEL COMPLETE!";
        winText.enabled = false;

        RectTransform wrt = winText.rectTransform;
        wrt.anchorMin = new Vector2(0.5f, 0.5f);
        wrt.anchorMax = new Vector2(0.5f, 0.5f);
        wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.anchoredPosition = Vector2.zero;
        wrt.sizeDelta = new Vector2(1200f, 200f);
    }

    /// <summary>
    /// Updates the player health bar and numerical readout in the HUD.
    /// Called whenever the player takes damage or starts the game.
    /// </summary>
    public void UpdatePlayerHealth(int current, int max)
    {
        if (max <= 0) max = 100;
        current = Mathf.Clamp(current, 0, max);
        float pct = (float)current / max;

        if (healthBarFill != null)
        {
            healthBarFill.sizeDelta = new Vector2(maxHealthBarWidth * pct, 0f);
        }

        if (healthBarFillImage != null)
        {
            // Dynamic color: Green -> Yellow -> Red based on health percentage
            if (pct > 0.5f)
            {
                healthBarFillImage.color = Color.Lerp(new Color(0.95f, 0.8f, 0.1f), new Color(0.15f, 0.85f, 0.25f), (pct - 0.5f) * 2f);
            }
            else
            {
                healthBarFillImage.color = Color.Lerp(new Color(0.9f, 0.15f, 0.15f), new Color(0.95f, 0.8f, 0.1f), pct * 2f);
            }
        }

        if (healthText != null)
        {
            healthText.text = $"+ {current} / {max} HP";
            if (current <= 25)
            {
                healthText.color = new Color(1f, 0.2f, 0.2f, 1f);
            }
            else
            {
                healthText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            }
        }
    }

    /// <summary>
    /// Displays a Call of Duty Zombies style intro title card for Verrückt at level start.
    /// Fades in, holds for 3.5 seconds, then smoothly fades out.
    /// </summary>
    IEnumerator PlayMapIntroTitle()
    {
        yield return new WaitForSeconds(0.4f);

        float fadeInDur = 1.0f;
        float holdDur = 3.2f;
        float fadeOutDur = 1.2f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDur)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInDur);
            SetTitleAlpha(alpha);
            yield return null;
        }
        SetTitleAlpha(1f);

        yield return new WaitForSeconds(holdDur);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDur)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDur);
            SetTitleAlpha(alpha);
            yield return null;
        }
        SetTitleAlpha(0f);
    }

    void SetTitleAlpha(float a)
    {
        if (mapTitleCard != null)
        {
            Color c = mapTitleCard.color;
            mapTitleCard.color = new Color(c.r, c.g, c.b, a);
        }
        if (mapSubtitleCard != null)
        {
            Color sc = mapSubtitleCard.color;
            mapSubtitleCard.color = new Color(sc.r, sc.g, sc.b, a);
        }
    }

    void UpdateCounterText()
    {
        if (counterText == null) return;
        int remaining = Mathf.Max(0, killsToWin - kills);
        counterText.text = $"ZOMBIES LEFT: {remaining}";
    }

    // ── Spawning ─────────────────────────────────────────────────────────── //
    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1.5f);

        while (!levelComplete)
        {
            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(wait);

            if (levelComplete) yield break;

            CleanupAliveList();
            if (aliveZombies.Count < maxAliveZombies)
            {
                SpawnZombie();
            }
        }
    }

    void CleanupAliveList()
    {
        aliveZombies.RemoveAll(z => z == null);
    }

    Vector3 PickSpawnPosition(float y)
    {
        for (int attempt = 0; attempt < 25; attempt++)
        {
            float x = Random.Range(spawnMinX, spawnMaxX);
            Vector3 candidate = new Vector3(x, y, 0f);

            if (player == null) return candidate;
            if (Mathf.Abs(candidate.x - player.position.x) >= minSpawnDistanceFromPlayer)
                return candidate;
        }
        return new Vector3(spawnMinX, y, 0f);
    }

    ZombieType PickRandomType()
    {
        if (zombieTypes == null || zombieTypes.Length == 0) return null;
        return zombieTypes[Random.Range(0, zombieTypes.Length)];
    }

    void SpawnZombie()
    {
        ZombieType type = PickRandomType();

        // Each zombie type can have its own Y level; fall back to the global spawnY.
        float y = type != null ? type.yPosition : spawnY;
        Vector3 spawnPos = PickSpawnPosition(y);

        string zname = type != null && !string.IsNullOrEmpty(type.name) ? type.name : "Zombie";
        GameObject zombie = new GameObject(zname + "_" + Random.Range(0, 100000));
        zombie.tag = "Enemy";
        zombie.transform.position = spawnPos;
        zombie.transform.localScale = Vector3.one * zombieScale;

        // Sprite
        SpriteRenderer sr = zombie.AddComponent<SpriteRenderer>();
        if (type != null && type.sprite != null) sr.sprite = type.sprite;
        sr.sortingLayerName = "Enemies";

        // Physics - no gravity and frozen Y so the zombie floats at the chosen Y level
        // and never gets stuck in the ground. The collider is a TRIGGER so zombies do
        // not physically collide with the ground or with each other.
        Rigidbody2D rb = zombie.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = zombie.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.9f);
        col.offset = new Vector2(0f, -0.05f);
        col.isTrigger = true;

        // Animator that drives the idle / walk / run / attack animations for this type
        if (type != null && type.controller != null)
        {
            Animator anim = zombie.AddComponent<Animator>();
            anim.runtimeAnimatorController = type.controller;
        }

        // Patrol behaviour (walk back and forth, drives IsMoving)
        EnemyPatrol patrol = zombie.AddComponent<EnemyPatrol>();
        patrol.speed = 1f;

        // Chase + attack behaviour (drives IsRunning / IsAttacking)
        EnemyChaseAttack chase = zombie.AddComponent<EnemyChaseAttack>();
        chase.playerLayer = 1 << 3;             // Layer 3 = Player
        chase.detectionRange = zombieDetectionRange;  // configurable detection range
        chase.chaseSpeed = zombieChaseSpeed;
        chase.attackDamage = zombieDamage;
        chase.attackRange = 1.0f;
        chase.verticalTolerance = 1.5f;

        // Health: 3 bullets to kill
        ZombieHealth health = zombie.AddComponent<ZombieHealth>();
        health.maxHealth = zombieHealth;

        // Give this zombie its own patrol route around where it spawned.
        SetupPatrolAround(zombie, spawnPos);

        IgnoreCollisionWithPlayer(zombie);
        aliveZombies.Add(zombie);
    }

    void SetupPatrolAround(GameObject zombie, Vector3 spawnPos)
    {
        EnemyPatrol patrol = zombie.GetComponent<EnemyPatrol>();
        if (patrol == null) return;

        float range = Random.Range(2f, 4f);

        GameObject a = new GameObject(zombie.name + "_PatrolA");
        GameObject b = new GameObject(zombie.name + "_PatrolB");
        a.transform.position = new Vector3(spawnPos.x - range, spawnPos.y, 0f);
        b.transform.position = new Vector3(spawnPos.x + range, spawnPos.y, 0f);

        a.transform.SetParent(zombie.transform, true);
        b.transform.SetParent(zombie.transform, true);

        patrol.pointA = a.transform;
        patrol.pointB = b.transform;
        patrol.flipSpriteOnTurn = true;
        patrol.isMovingBoolParam = "IsMoving";
    }

    // ── Existing zombies / collisions ────────────────────────────────────── //
    void MakeExistingZombiesValid()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.GetComponent<ZombieHealth>() == null)
            {
                ZombieHealth h = enemy.AddComponent<ZombieHealth>();
                h.maxHealth = zombieHealth;
            }

            // Make hand-placed zombies behave like spawned ones: trigger collider (no
            // ground collisions / no getting stuck) and no gravity (freely movable Y).
            Collider2D c = enemy.GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
            Rigidbody2D r = enemy.GetComponent<Rigidbody2D>();
            if (r != null) r.gravityScale = 0f;

            // Apply the configurable detection range to hand-placed zombies too.
            EnemyChaseAttack chase = enemy.GetComponent<EnemyChaseAttack>();
            if (chase != null) chase.detectionRange = zombieDetectionRange;

            aliveZombies.Add(enemy);
            IgnoreCollisionWithPlayer(enemy);
        }
    }

    void IgnoreCollisionWithPlayer(GameObject zombie)
    {
        if (player == null) return;
        Collider2D playerCol = player.GetComponent<Collider2D>();
        Collider2D zombieCol = zombie.GetComponent<Collider2D>();
        if (playerCol != null && zombieCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, zombieCol, true);
        }
    }

    // ── Objective ────────────────────────────────────────────────────────── //
    /// <summary>Called by ZombieHealth when a zombie dies.</summary>
    public void ZombieKilled()
    {
        if (levelComplete) return;

        kills++;
        UpdateCounterText();
        Debug.Log($"[ZombieManager] Kill {kills}/{killsToWin}");

        if (kills >= killsToWin)
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        levelComplete = true;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        if (winText != null) winText.enabled = true;
        Debug.Log("[ZombieManager] LEVEL COMPLETE!");
    }
}