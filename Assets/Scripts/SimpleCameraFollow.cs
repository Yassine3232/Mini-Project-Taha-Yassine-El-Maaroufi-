using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;          
    public float smoothSpeed = 5f;    
    public float zoomSize = 5f;       
    
    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 2f, 0f); 

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = zoomSize;
    }

    [Header("Map Boundaries")]
    [Tooltip("Enable clamping camera position within the map limits.")]
    public bool clampToBounds = true;
    public float minX = -28.5f;
    public float maxX = 8.5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; 

        if (clampToBounds && cam != null && cam.orthographic)
        {
            float vertExtent = cam.orthographicSize;
            float horzExtent = vertExtent * Screen.width / Screen.height;

            float minCamX = minX + horzExtent;
            float maxCamX = maxX - horzExtent;

            if (minCamX < maxCamX)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minCamX, maxCamX);
            }
            else
            {
                desiredPosition.x = (minX + maxX) * 0.5f;
            }
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}