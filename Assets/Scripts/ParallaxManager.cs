using System;
using UnityEngine;

/// <summary>
/// Manages 2D parallax scrolling for 3 distinct background/foreground layers.
/// Speed for each layer can be regulated via Inspector sliders ([Range])
/// as well as via an on-screen interactive UI slider panel during gameplay.
/// </summary>
public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public string name = "Layer";
        [Tooltip("The GameObject or parent Transform for this layer.")]
        public Transform layerTransform;

        [Range(0f, 1f)]
        [Tooltip("0 = fixed in world (foreground), 0.5 = midground, ~0.9 = far background (moves with camera).")]
        public float speed = 0.5f;

        [HideInInspector] public Vector3 startPosition;
    }

    [Header("Camera Reference")]
    [Tooltip("Target camera to follow. If null, Camera.main is used automatically.")]
    public Camera targetCamera;

    [Header("Parallax Layers (3 Layers)")]
    public ParallaxLayer layer1 = new ParallaxLayer { name = "Layer 1 (Far Background)", speed = 0.85f };
    public ParallaxLayer layer2 = new ParallaxLayer { name = "Layer 2 (Midground)", speed = 0.5f };
    public ParallaxLayer layer3 = new ParallaxLayer { name = "Layer 3 (Near / Foreground)", speed = 0.2f };

    [Header("Vertical Parallax")]
    [Tooltip("Enable vertical parallax movement alongside horizontal.")]
    public bool enableVerticalParallax = false;
    [Range(0f, 1f)]
    public float verticalMultiplier = 0.3f;

    [Header("On-Screen UI Controls")]
    [Tooltip("Show on-screen sliders during gameplay to adjust layer speeds live.")]
    public bool showOnScreenSliders = true;

    private Vector3 lastCameraPosition;
    private bool uiMinimized = false;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            lastCameraPosition = targetCamera.transform.position;
        }

        // Auto-assign default layers if not wired in Inspector
        AutoAssignLayersIfEmpty();

        InitLayer(layer1);
        InitLayer(layer2);
        InitLayer(layer3);
    }

    void InitLayer(ParallaxLayer layer)
    {
        if (layer != null && layer.layerTransform != null)
        {
            layer.startPosition = layer.layerTransform.position;
        }
    }

    void AutoAssignLayersIfEmpty()
    {
        // Try to find reasonable defaults in the scene if slots are unassigned
        if (layer1.layerTransform == null)
        {
            var bg = GameObject.Find("War3_0") ?? GameObject.Find("Background");
            if (bg != null) layer1.layerTransform = bg.transform;
        }
        if (layer2.layerTransform == null)
        {
            var mid = GameObject.Find("War3_0 (1)") ?? GameObject.Find("Road");
            if (mid != null) layer2.layerTransform = mid.transform;
        }
        if (layer3.layerTransform == null)
        {
            var fore = GameObject.Find("Craters") ?? GameObject.Find("crater1_0");
            if (fore != null) layer3.layerTransform = fore.transform;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 currentCameraPos = targetCamera.transform.position;
        Vector3 deltaMovement = currentCameraPos - lastCameraPosition;

        // Apply parallax to each layer
        ApplyParallax(layer1, deltaMovement);
        ApplyParallax(layer2, deltaMovement);
        ApplyParallax(layer3, deltaMovement);

        lastCameraPosition = currentCameraPos;
    }

    void ApplyParallax(ParallaxLayer layer, Vector3 delta)
    {
        if (layer == null || layer.layerTransform == null) return;

        // Move the layer with a fraction of the camera's delta movement.
        // Higher speed multiplier = moves closer to camera movement = appears further in the background.
        float moveX = delta.x * layer.speed;
        float moveY = enableVerticalParallax ? delta.y * layer.speed * verticalMultiplier : 0f;

        layer.layerTransform.position += new Vector3(moveX, moveY, 0f);
    }

    // ── On-Screen Interactive Sliders ────────────────────────────────────── //
    void OnGUI()
    {
        if (!showOnScreenSliders) return;

        int panelWidth = 260;
        int panelHeight = uiMinimized ? 35 : 210;
        Rect boxRect = new Rect(15, 15, panelWidth, panelHeight);

        GUI.Box(boxRect, GUIContent.none);

        GUILayout.BeginArea(new Rect(20, 20, panelWidth - 10, panelHeight - 10));

        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Parallax Speed Sliders</b>", GUILayout.ExpandWidth(true));
        if (GUILayout.Button(uiMinimized ? "+" : "—", GUILayout.Width(25)))
        {
            uiMinimized = !uiMinimized;
        }
        GUILayout.EndHorizontal();

        if (!uiMinimized)
        {
            GUILayout.Space(6);
            DrawLayerSlider(layer1);
            GUILayout.Space(4);
            DrawLayerSlider(layer2);
            GUILayout.Space(4);
            DrawLayerSlider(layer3);
        }

        GUILayout.EndArea();
    }

    void DrawLayerSlider(ParallaxLayer layer)
    {
        if (layer == null) return;

        string layerName = string.IsNullOrEmpty(layer.name) ? "Layer" : layer.name;
        if (layer.layerTransform != null)
        {
            layerName = $"{layer.name} ({layer.layerTransform.name})";
        }

        GUILayout.Label($"{layerName}: {layer.speed:F2}");
        layer.speed = GUILayout.HorizontalSlider(layer.speed, 0f, 1f);
    }
}