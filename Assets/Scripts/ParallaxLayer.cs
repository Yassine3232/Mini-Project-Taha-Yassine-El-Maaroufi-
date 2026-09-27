using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Camera targetCamera;
    public float parallaxSpeed = 0.8f;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;

    void Awake()
    {
        startPosition = transform.position;
    }

    void Start()
    {
        SetCamera();
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            SetCamera();
            if (targetCamera == null) return;
        }

        Vector3 cameraDelta = targetCamera.transform.position - cameraStartPosition;
        transform.position = startPosition + new Vector3(cameraDelta.x * parallaxSpeed, 0f, 0f);
    }

    void SetCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            cameraStartPosition = targetCamera.transform.position;
        }
    }
}
