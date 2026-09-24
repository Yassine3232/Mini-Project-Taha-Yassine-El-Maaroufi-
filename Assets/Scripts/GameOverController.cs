using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [Header("Scene to Reload")]
    public string gameSceneName = "SampleScene";

    [Header("Animation Timings")]
    public float titleSlideDistance = 80f;
    public float staggerDelay       = 0.18f;
    public float btnPopOvershoot    = 1.15f;

    private Image         panelImage;
    private RectTransform titleRect;
    private RectTransform subtitleRect;
    private RectTransform replayBtnRect;
    private Image         bloodSplash;

    void Awake()
    {
        // Destroy any cameras that leaked in from the previous scene
        // (anything not tagged MainCamera in THIS scene should go)
        var allCams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var c in allCams)
        {
            // Keep only cameras in the active GameOver scene
            if (c.gameObject.scene != gameObject.scene)
                Destroy(c.gameObject);
        }
    }

    void Start()
    {
        // Find by name — these names are unique in this scene
        panelImage    = FindImage("Panel");
        bloodSplash   = FindImage("BloodSplash");
        titleRect     = FindRect("TitleText");
        subtitleRect  = FindRect("SubtitleText");
        replayBtnRect = FindRect("ReplayButton");

        // Wire button in code
        var replayBtnGO = GameObject.Find("ReplayButton");
        if (replayBtnGO != null)
        {
            var btn = replayBtnGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnReplay);
                Debug.Log("[GameOver] Replay button wired.");
            }
        }

        // Immediately show a fully opaque dark panel (no fade-from-nothing)
        // so you always see something even if animation fails
        SetAlpha(panelImage, 0.97f);
        SetAlpha(bloodSplash, 0f);

        // Slide start positions
        if (titleRect    != null) titleRect   .anchoredPosition += new Vector2(0f, titleSlideDistance);
        if (subtitleRect != null) subtitleRect.anchoredPosition += new Vector2(0f, titleSlideDistance);
        if (replayBtnRect!= null) replayBtnRect.localScale       = Vector3.zero;

        StartCoroutine(AnimateIn());
    }

    // ── helpers ──────────────────────────────────────────────────────────── //
    static Image FindImage(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Image>() : null;
    }
    static RectTransform FindRect(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }
    static void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; img.color = new Color(c.r, c.g, c.b, a);
    }

    // ── animation ────────────────────────────────────────────────────────── //
    IEnumerator AnimateIn()
    {
        // Blood vignette
        yield return FadeImage(bloodSplash, 0f, 0.55f, 0.35f);

        yield return new WaitForSecondsRealtime(0.05f);

        // Title slides down
        if (titleRect    != null) yield return SlideIn(titleRect,    staggerDelay);
        // Subtitle slides down
        if (subtitleRect != null) yield return SlideIn(subtitleRect, staggerDelay * 0.5f);

        yield return new WaitForSecondsRealtime(0.1f);

        // Button bounces in
        if (replayBtnRect != null) yield return PopIn(replayBtnRect, 0.35f);
    }

    IEnumerator FadeImage(Image img, float from, float to, float dur)
    {
        if (img == null) yield break;
        float t = 0f; Color c = img.color;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            img.color = new Color(c.r, c.g, c.b, Mathf.Lerp(from, to, Mathf.Clamp01(t)));
            yield return null;
        }
        img.color = new Color(c.r, c.g, c.b, to);
    }

    IEnumerator SlideIn(RectTransform rect, float dur)
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end   = start - new Vector2(0f, titleSlideDistance);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            rect.anchoredPosition = Vector2.Lerp(start, end, EaseOut(Mathf.Clamp01(t)));
            yield return null;
        }
        rect.anchoredPosition = end;
    }

    IEnumerator PopIn(RectTransform rect, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            rect.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(t));
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    // ── replay ───────────────────────────────────────────────────────────── //
    public void OnReplay()
    {
        Debug.Log("[GameOver] Replaying...");
        StartCoroutine(ReplayRoutine());
    }

    IEnumerator ReplayRoutine()
    {
        yield return FadeImage(panelImage, panelImage != null ? panelImage.color.a : 1f, 0f, 0.25f);
        SceneManager.LoadScene(gameSceneName);
    }

    // ── easing ───────────────────────────────────────────────────────────── //
    static float EaseOut(float t)        => 1f - Mathf.Pow(1f - t, 3f);
    static float EaseOutBack(float t)
    {
        const float s = 1.15f;
        return 1f + (s + 1f) * Mathf.Pow(t - 1f, 3f) + s * Mathf.Pow(t - 1f, 2f);
    }
}
