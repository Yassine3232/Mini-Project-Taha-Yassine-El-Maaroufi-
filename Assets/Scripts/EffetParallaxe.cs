using UnityEngine;

public class EffetParallaxe : MonoBehaviour
{
    [Header("Caméra")]
    [SerializeField] private Transform cameraCible;

    [Header("Suivi de la caméra")]
    [SerializeField, Range(0f, 1f)] private float suiviHorizontal = 0.85f;
    [SerializeField, Range(0f, 1f)] private float suiviVertical = 0.90f;

    [Header("Déplacement automatique très lent")]
    [SerializeField] private Vector2 vitesseAutomatique = new(0.01f, 0f);

    private Vector3 positionInitiale;
    private Vector3 positionCameraInitiale;
    private Vector2 decalageAutomatique;

    private void Start()
    {
        positionInitiale = transform.position;

        if (cameraCible == null && Camera.main != null)
            cameraCible = Camera.main.transform;

        if (cameraCible != null)
            positionCameraInitiale = cameraCible.position;
    }

    private void LateUpdate()
    {
        if (cameraCible == null) return;

        Vector3 mouvementCamera = cameraCible.position - positionCameraInitiale;
        decalageAutomatique += vitesseAutomatique * Time.deltaTime;

        transform.position = new Vector3(
            positionInitiale.x + mouvementCamera.x * suiviHorizontal + decalageAutomatique.x,
            positionInitiale.y + mouvementCamera.y * suiviVertical + decalageAutomatique.y,
            positionInitiale.z
        );
    }
}
