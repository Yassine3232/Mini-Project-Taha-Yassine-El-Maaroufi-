using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform cible;
    public float vitesseSuivi = 5f;
    public Vector3 decalage = new Vector3(0f, 2f, 0f);
    public bool bloquerLimites = true;
    public float limiteGauche = -28.5f;
    public float limiteDroite = 8.5f;

    private Camera composantCamera;

    void Start()
    {
        composantCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (cible == null) return;

        Vector3 positionVisee = cible.position + decalage;
        positionVisee.z = transform.position.z;

        if (bloquerLimites && composantCamera != null && composantCamera.orthographic)
        {
            float ratioEcran = (float)Screen.width / Screen.height;
            float demiLargeurCamera = composantCamera.orthographicSize * ratioEcran;

            float minXCamera = limiteGauche + demiLargeurCamera;
            float maxXCamera = limiteDroite - demiLargeurCamera;

            if (minXCamera < maxXCamera)
            {
                positionVisee.x = Mathf.Clamp(positionVisee.x, minXCamera, maxXCamera);
            }
            else
            {
                positionVisee.x = (limiteGauche + limiteDroite) * 0.5f;
            }
        }

        transform.position = Vector3.Lerp(transform.position, positionVisee, vitesseSuivi * Time.deltaTime);
    }
}
