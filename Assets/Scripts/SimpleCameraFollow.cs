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

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; 

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}