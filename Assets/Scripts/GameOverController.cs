using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Game Over screen. Attach to the GameOverController GameObject in the GameOver scene.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup panelGroup;        // The root panel CanvasGroup (for fade-in)
    public RectTransform titleRect;       // "GAME OVER" text rect (for slide-in)
    public RectTransform subtitleRect;    // "YOU WERE ELIMINATED" text rect
    public RectTransform replayBtnRect;   // Replay button rect (for pop-in)
    public Image bloodSplash;            // Full-screen blood overlay image

    [Header("Animation Settings")]
    public float fadeInDuration = 0.6f;
    public float titleSlideDistance = 80f;
    public float staggerDelay = 0.18f;
    public float btnPopOvershoot = 1.15f;

    [Header("Scene to Reload")]
    public string gameSceneName = "SampleScene";

    void Start()
    {
        // Hide everything at start
        if (panelGroup != null) panelGroup.alpha = 0f;
        if (bloodSplash != null)
        {
            Color c = bloodSplash.color;
            bloodSplash.color = new Color(c.r, c.g, c.b, 0f);
        }

        Vector2 offscreen = new Vector2(0f, titleSlideDistance);
        if (titleRect != null) titleRect.anchoredPosition += offscreen;
        if (subtitleRect != null) subtitleRect.anchoredPosition += offscreen;
        if (replayBtnRect != null) replayBtnRect.localScale = Vector3.zero;

        StartCoroutine(AnimateIn());
    }

    IEnumerator AnimateIn()
    {
        // 1. Fade in blood splash
        if (bloodSplash != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.35f;
                Color c = bloodSplash.color;
                bloodSplash.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 0.55f, t));
                yield return null;
            }
        }

        // 2. Fade in panel
        if (panelGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeInDuration;
                panelGroup.alpha = Mathf.Lerp(0f, 1f, EaseOutCubic(t));
                yield return null;
            }
            panelGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(0.05f);

        // 3. Slide in title
        if (titleRect != null)
        {
            yield return StartCoroutine(SlideIn(titleRect, staggerDelay));
        }

        // 4. Slide in subtitle
        if (subtitleRect != null)
        {
            yield return StartCoroutine(SlideIn(subtitleRect, staggerDelay * 0.5f));
        }

        yield return new WaitForSeconds(0.1f);

        // 5. Pop in the replay button with bounce
        if (replayBtnRect != null)
        {
            yield return StartCoroutine(PopIn(replayBtnRect, 0.35f));
        }
    }

    IEnumerator SlideIn(RectTransform rect, float duration)
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos - new Vector2(0f, titleSlideDistance);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseOutCubic(Mathf.Clamp01(t)));
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

    IEnumerator PopIn(RectTransform rect, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float tc = Mathf.Clamp01(t);
            float scale = EaseOutBack(tc, btnPopOvershoot);
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    /// <summary>Called by the Replay button's OnClick.</summary>
    public void OnReplay()
    {
        StartCoroutine(ReplayRoutine());
    }

    IEnumerator ReplayRoutine()
    {
        // Quick flash out
        if (panelGroup != null)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / 0.25f;
                panelGroup.alpha = t;
                yield return null;
            }
        }
        SceneManager.LoadScene(gameSceneName);
    }

    // Easing helpers
    static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    static float EaseOutBack(float t, float overshoot)
    {
        float c1 = overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
