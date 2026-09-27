using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public string name = "Layer";
        public Transform layerTransform;
        [Range(0f, 1f)] public float speed = 0.5f;
    }

    public Camera targetCamera;

    public ParallaxLayer layer1 = new ParallaxLayer { name = "Layer 1 (Far Background)", speed = 0.85f };
    public ParallaxLayer layer2 = new ParallaxLayer { name = "Layer 2 (Midground)", speed = 0.5f };
    public ParallaxLayer layer3 = new ParallaxLayer { name = "Layer 3 (Near / Foreground)", speed = 0.2f };

    public bool enableVerticalParallax = false;
    [Range(0f, 1f)] public float verticalMultiplier = 0.3f;

    private Vector3 lastCameraPosition;

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
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 currentCameraPos = targetCamera.transform.position;
        Vector3 delta = currentCameraPos - lastCameraPosition;

        ApplyParallax(layer1, delta);
        ApplyParallax(layer2, delta);
        ApplyParallax(layer3, delta);

        lastCameraPosition = currentCameraPos;
    }

    void ApplyParallax(ParallaxLayer layer, Vector3 delta)
    {
        if (layer == null || layer.layerTransform == null) return;

        float moveX = delta.x * layer.speed;
        float moveY = enableVerticalParallax ? delta.y * layer.speed * verticalMultiplier : 0f;

        layer.layerTransform.position += new Vector3(moveX, moveY, 0f);
    }
}
