using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, 2);
    private Camera mainCamera;

    void OnEnable()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Vérifie si une caméra principale existe
        if (mainCamera != null)
        {
            // Calcule et applique la nouvelle position du canvas
            transform.position = mainCamera.transform.position     // Position de base (position de la caméra)
                               + mainCamera.transform.forward * offset.z    // Décalage vers l'avant
                               + mainCamera.transform.up * offset.y        // Décalage vertical
                               + mainCamera.transform.right * offset.x;    // Décalage horizontal

            // Fait en sorte que le canvas garde la même orientation que la caméra
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
