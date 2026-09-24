using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a Call of Duty Zombies style dusty / grimy filter over the camera view:
///  - warm sepia tint
///  - dark vignette on the edges
///  - animated film grain / airborne dust noise
///  - slowly drifting dust motes
/// All textures are generated procedurally (no assets required) and every intensity
/// can be regulated with Inspector [Range] sliders or the on-screen control panel.
/// </summary>
public class DustyFilter : MonoBehaviour
{
    [Header("Master")]
    [Tooltip("Enable or disable the whole dusty filter.")]
    public bool filterEnabled = true;

    [Header("Tint (warm dusty sepia)")]
    [Range(0f, 1f)] public float tintStrength = 0.16f;
    public Color tintColor = new Color(0.88f, 0.72f, 0.48f);

    [Header("Vignette (dark edges)")]
    [Range(0f, 1f)] public float vignetteStrength = 0.55f;
    [Range(0.05f, 0.95f)] public float vignetteSoftness = 0.55f;

    [Header("Film grain / dust noise")]
    [Range(0f, 1f)] public float grainStrength = 0.12f;
    [Tooltip("Scroll the grain so it feels alive (like dust in the air).")]
    public bool animateGrain = true;
    [Range(0.1f, 4f)] public float grainSpeed = 1.2f;

    [Header("Floating dust motes")]
    [Range(0f, 1f)] public float dustMotesStrength = 0.5f;
    [Range(0, 30)] public int dustMoteCount = 14;

    [Header("On-screen controls")]
    public bool showOnScreenControls = true;

    // ── runtime refs ─────────────────────────────────────────────────────── //
    private Image tintImage;
    private RawImage vignetteImage;
    private RawImage grainImage;
    private RectTransform motesRoot;

    private Texture2D vignetteTex;
    private Texture2D grainTex;
    private Texture2D moteTex;

    private float grainTime = 0f;
    private float lastVignetteSoftness = -1f;
    private bool controlsExpanded = true;

    private readonly List<RectTransform> motes = new List<RectTransform>();
    private readonly List<Vector2> moteSpeeds = new List<Vector2>();
    private readonly List<float> moteBaseAlpha = new List<float>();

    void Start()
    {
        BuildLayers();
        BuildDustMotes();
        ApplySettings();
    }

    // ── construction ─────────────────────────────────────────────────────── //
    void BuildLayers()
    {
        GameObject canvasGO = new GameObject("DustyFilterCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // above the gameplay HUD
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // 1) warm sepia tint (solid full screen color)
        tintImage = MakeFullscreenImage(canvasGO.transform, "SepiaTint");

        // 2) vignette (procedural radial gradient texture)
        vignetteTex = MakeVignetteTexture(256, vignetteSoftness);
        lastVignetteSoftness = vignetteSoftness;
        vignetteImage = MakeFullscreenRawImage(canvasGO.transform, "Vignette", vignetteTex);

        // 3) film grain / dust noise (procedural noise texture, scrolling)
        grainTex = MakeNoiseTexture(160);
        grainImage = MakeFullscreenRawImage(canvasGO.transform, "FilmGrain", grainTex);

        // 4) dust motes container
        GameObject motesGO = new GameObject("DustMotes");
        motesGO.transform.SetParent(canvasGO.transform, false);
        motesRoot = motesGO.AddComponent<RectTransform>();
        motesRoot.anchorMin = Vector2.zero;
        motesRoot.anchorMax = Vector2.one;
        motesRoot.offsetMin = Vector2.zero;
        motesRoot.offsetMax = Vector2.zero;
    }

    Image MakeFullscreenImage(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.raycastTarget = false;
        img.sprite = null;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    RawImage MakeFullscreenRawImage(Transform parent, string name, Texture2D tex)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RawImage img = go.AddComponent<RawImage>();
        img.raycastTarget = false;
        img.texture = tex;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    void BuildDustMotes()
    {
        if (motesRoot == null) return;

        // clear old
        foreach (Transform child in motesRoot) Destroy(child.gameObject);
        motes.Clear();
        moteSpeeds.Clear();
        moteBaseAlpha.Clear();

        if (moteTex == null) moteTex = MakeDotTexture(48);

        for (int i = 0; i < dustMoteCount; i++)
        {
            GameObject go = new GameObject("Mote_" + i);
            go.transform.SetParent(motesRoot, false);
            RawImage img = go.AddComponent<RawImage>();
            img.raycastTarget = false;
            img.texture = moteTex;

            float size = Random.Range(7f, 22f);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(Random.Range(-940f, 940f), Random.Range(-540f, 540f));

            motes.Add(rt);
            moteSpeeds.Add(new Vector2(Random.Range(-7f, 7f), Random.Range(1f, 5f)));
            moteBaseAlpha.Add(Random.Range(0.35f, 1f));
        }
    }

    // ── procedural textures ──────────────────────────────────────────────── //
    Texture2D MakeVignetteTexture(int size, float softness)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float half = size * 0.5f;
        Color[] px = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float t = Mathf.Clamp01((d - (1f - softness)) / Mathf.Max(0.05f, softness));
                t = t * t * (3f - 2f * t); // smoothstep
                px[y * size + x] = new Color(0f, 0f, 0f, t);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    Texture2D MakeNoiseTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++)
        {
            float v = Random.value;
            float a = Random.Range(0.35f, 1f);
            px[i] = new Color(v, v, v, a);
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    Texture2D MakeDotTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float half = size * 0.5f;
        Color[] px = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // ── per-frame ────────────────────────────────────────────────────────── //
    void Update()
    {
        if (!filterEnabled) return;

        // animated grain (scroll + slight jitter so dust feels alive)
        if (grainImage != null && animateGrain)
        {
            grainTime += Time.unscaledDeltaTime * grainSpeed * 0.06f;
            float jx = Mathf.PingPong(Time.unscaledTime * 7f, 0.35f);
            grainImage.uvRect = new Rect(grainTime + jx, grainTime * 0.7f, 1f, 1f);
        }

        UpdateMotes();
        ApplySettings();
    }

    void ApplySettings()
    {
        if (tintImage != null)
        {
            tintImage.enabled = filterEnabled;
            tintImage.color = new Color(tintColor.r, tintColor.g, tintColor.b, tintStrength);
        }

        // regenerate vignette when softness changes noticeably
        if (Mathf.Abs(vignetteSoftness - lastVignetteSoftness) > 0.02f)
        {
            Texture2D old = vignetteTex;
            vignetteTex = MakeVignetteTexture(256, vignetteSoftness);
            lastVignetteSoftness = vignetteSoftness;
            if (vignetteImage != null) vignetteImage.texture = vignetteTex;
            if (old != null) Destroy(old);
        }

        if (vignetteImage != null)
        {
            vignetteImage.enabled = filterEnabled;
            vignetteImage.color = new Color(0f, 0f, 0f, vignetteStrength);
        }

        if (grainImage != null)
        {
            grainImage.enabled = filterEnabled;
            grainImage.color = new Color(1f, 1f, 1f, grainStrength);
        }

        if (motesRoot != null)
        {
            motesRoot.gameObject.SetActive(filterEnabled);
            for (int i = 0; i < motes.Count; i++)
            {
                RawImage img = motes[i].GetComponent<RawImage>();
                if (img != null)
                {
                    float a = dustMotesStrength * moteBaseAlpha[i];
                    img.color = new Color(1f, 0.97f, 0.9f, a);
                }
            }
        }
    }

    void UpdateMotes()
    {
        for (int i = 0; i < motes.Count; i++)
        {
            RectTransform rt = motes[i];
            if (rt == null) continue;
            Vector2 p = rt.anchoredPosition + moteSpeeds[i] * Time.unscaledDeltaTime;

            // wrap around the screen so motes drift forever
            if (p.x > 970f) p.x = -970f;
            else if (p.x < -970f) p.x = 970f;
            if (p.y > 560f) p.y = -560f;
            else if (p.y < -560f) p.y = 560f;

            rt.anchoredPosition = p;
        }
    }

    // ── on-screen control panel ──────────────────────────────────────────── //
    void OnGUI()
    {
        if (!showOnScreenControls) return;

        int w = 260;
        int h = controlsExpanded ? 235 : 34;
        float y = Screen.height - h - 15;
        Rect box = new Rect(15f, y, w, h);

        GUI.Box(box, GUIContent.none);
        GUILayout.BeginArea(new Rect(box.x + 6f, box.y + 6f, w - 12f, h - 12f));

        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Dusty Filter</b>", GUILayout.ExpandWidth(true));
        if (GUILayout.Button(controlsExpanded ? "—" : "+", GUILayout.Width(26)))
        {
            controlsExpanded = !controlsExpanded;
        }
        GUILayout.EndHorizontal();

        if (controlsExpanded)
        {
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enabled", GUILayout.Width(120));
            filterEnabled = GUILayout.Toggle(filterEnabled, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            DrawSlider("Tint", ref tintStrength);
            DrawSlider("Vignette", ref vignetteStrength);
            DrawSlider("Grain", ref grainStrength);
            DrawSlider("Dust motes", ref dustMotesStrength);
        }

        GUILayout.EndArea();
    }

    void DrawSlider(string label, ref float value)
    {
        GUILayout.Space(2);
        GUILayout.Label($"{label}: {value:F2}");
        value = GUILayout.HorizontalSlider(value, 0f, 1f);
    }
}