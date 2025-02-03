using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Classe qui gère l'interaction avec le Rubik's Cube en utilisant les contrôleurs XR.
/// </summary>
public class RubiksCubeInteraction : MonoBehaviour
{
    [Header("Références")]
    public RubikGen rubikGen;                   // Référence au script de génération du Rubik's Cube
    public XRDirectInteractor leftHand;         // Interacteur pour la main gauche
    public XRDirectInteractor rightHand;        // Interacteur pour la main droite

    [Header("Paramètres de rotation")]
    public float rotationSpeed = 300f;          // Vitesse de rotation appliquée aux cubes
    public float snapAngleThreshold = 45f;      // Seuil d'angle pour l'alignement (snap) des rotations

    // Variables internes pour la gestion de l'interaction
    private GameObject selectedCube;            // Cube sélectionné lors de la saisie
    private GameObject[] selectedLayer;         // Ensemble de cubes formant la couche à faire tourner
    private Vector3 rotationAxis;               // Axe autour duquel la couche sera tournée
    private int currentLayer;                   // Indice de la couche sélectionnée
    private bool isRotating;                    // Indique si une rotation est en cours
    private Vector3 grabStartPosition;          // Position initiale de la main lors de la saisie
    private Quaternion grabStartRotation;       // Rotation initiale lors de la saisie
    private InteractionMode currentMode;        // Mode d'interaction en cours
    private IXRSelectInteractable currentInteractable; // Objet interactif actuellement sélectionné
    private Vector3 previousPosition;           // Position précédente de la main pour calculer le mouvement
    private bool isGrabbing;                    // Indique si la main est en train de saisir

    /// <summary>
    /// Modes d'interaction possibles.
    /// </summary>
    private enum InteractionMode
    {
        None,
        RightHandClassic,
        LeftHandAxisBased
    }

    /// <summary>
    /// Initialisation des événements d'interaction.
    /// </summary>
    private void Start()
    {
        SetupInteractionEvents();
    }

    /// <summary>
    /// Configure les événements pour les mains gauche et droite.
    /// </summary>
    private void SetupInteractionEvents()
    {
        // Événements pour la main gauche
        leftHand.selectEntered.AddListener(OnLeftHandGrab);
        leftHand.selectExited.AddListener(OnLeftHandRelease);

        // Événements pour la main droite
        rightHand.selectEntered.AddListener(OnRightHandGrab);
        rightHand.selectExited.AddListener(OnRightHandRelease);
    }

    /// <summary>
    /// Gère la saisie avec la main droite.
    /// </summary>
    /// <param name="args">Arguments de l'événement de saisie</param>
    private void OnRightHandGrab(SelectEnterEventArgs args)
    {
        if (currentMode != InteractionMode.None) return;

        var grabbedObj = args.interactableObject.transform.gameObject;
        if (grabbedObj.CompareTag("CubePiece"))
        {
            currentMode = InteractionMode.RightHandClassic;
            // Le XR Toolkit gère le déplacement avec la main droite
        }
    }

    /// <summary>
    /// Gère le relâchement de la main droite.
    /// </summary>
    /// <param name="args">Arguments de l'événement de relâchement</param>
    private void OnRightHandRelease(SelectExitEventArgs args)
    {
        if (currentMode == InteractionMode.RightHandClassic)
        {
            currentMode = InteractionMode.None;
            SnapToNearestRotation();
        }
    }

    /// <summary>
    /// Gère la saisie avec la main gauche.
    /// </summary>
    /// <param name="args">Arguments de l'événement de saisie</param>
    private void OnLeftHandGrab(SelectEnterEventArgs args)
    {
        if (currentMode != InteractionMode.None) return;

        var grabbedObj = args.interactableObject.transform.gameObject;
        if (grabbedObj.CompareTag("CubePiece"))
        {
            currentMode = InteractionMode.LeftHandAxisBased;
            selectedCube = grabbedObj;
            currentInteractable = args.interactableObject;

            // Stockage de la position initiale de la main
            grabStartPosition = leftHand.transform.position;
            previousPosition = grabStartPosition;

            // Détermination de l'axe de rotation et de l'indice de la couche à partir du cube saisi
            (rotationAxis, currentLayer) = GetCubeAxisAndLayer(grabbedObj);
            // Récupère tous les cubes appartenant à la couche sélectionnée
            selectedLayer = GetLayerCubes(rotationAxis, currentLayer);

            // Désactivation temporaire du suivi (tracking) de position et rotation pour éviter les interférences
            var grabInteractable = grabbedObj.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.trackPosition = false;
                grabInteractable.trackRotation = false;
            }

            isGrabbing = true;
        }
    }

    /// <summary>
    /// Gère le relâchement de la main gauche.
    /// </summary>
    /// <param name="args">Arguments de l'événement de relâchement</param>
    private void OnLeftHandRelease(SelectExitEventArgs args)
    {
        if (currentMode == InteractionMode.LeftHandAxisBased)
        {
            // Réactive le suivi de position et rotation pour l'objet saisi
            if (selectedCube != null)
            {
                var grabInteractable = selectedCube.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.trackPosition = true;
                    grabInteractable.trackRotation = true;
                }
            }

            // Ajuste la rotation de la couche à l'angle le plus proche
            SnapToNearestRotation();
            ResetLeftHandState();
        }
    }

    /// <summary>
    /// Réinitialise l'état de l'interaction pour la main gauche après le relâchement.
    /// </summary>
    private void ResetLeftHandState()
    {
        currentMode = InteractionMode.None;
        selectedCube = null;
        selectedLayer = null;
        currentInteractable = null;
        isGrabbing = false;
    }

    /// <summary>
    /// Met à jour la rotation en fonction du déplacement de la main gauche.
    /// </summary>
    private void Update()
    {
        if (currentMode == InteractionMode.LeftHandAxisBased && isGrabbing)
        {
            HandleAxisBasedRotation();
        }
    }

    /// <summary>
    /// Gère la rotation de la couche en se basant sur le déplacement de la main gauche.
    /// </summary>
    private void HandleAxisBasedRotation()
    {
        if (selectedLayer == null || selectedLayer.Length == 0) return;

        Vector3 currentPosition = leftHand.transform.position;
        Vector3 movement = currentPosition - previousPosition;

        // Projection du mouvement sur le plan perpendiculaire à l'axe de rotation
        Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, rotationAxis);

        // Calcul de l'angle de rotation en fonction de la distance parcourue
        float rotationAmount = projectedMovement.magnitude * rotationSpeed;

        // Détermination du sens de rotation
        Vector3 cross = Vector3.Cross(rotationAxis, projectedMovement);
        float direction = Vector3.Dot(cross, Vector3.up);
        rotationAmount *= Mathf.Sign(direction);

        // Applique la rotation à chaque cube de la couche sélectionnée
        foreach (GameObject cube in selectedLayer)
        {
            if (cube != null)
            {
                cube.transform.RotateAround(
                    rubikGen.transform.position, // Point autour duquel tourner
                    rotationAxis,                // Axe de rotation
                    rotationAmount               // Angle de rotation
                );
            }
        }

        previousPosition = currentPosition;
    }

    /// <summary>
    /// Ajuste la rotation de chaque cube de la couche pour qu'elle soit un multiple de 90°.
    /// </summary>
    private void SnapToNearestRotation()
    {
        if (selectedLayer == null) return;

        foreach (GameObject cube in selectedLayer)
        {
            if (cube != null)
            {
                Vector3 currentRotation = cube.transform.rotation.eulerAngles;
                Vector3 snappedRotation = new Vector3(
                    Mathf.Round(currentRotation.x / 90f) * 90f,
                    Mathf.Round(currentRotation.y / 90f) * 90f,
                    Mathf.Round(currentRotation.z / 90f) * 90f
                );
                cube.transform.rotation = Quaternion.Euler(snappedRotation);
            }
        }
    }

    /// <summary>
    /// Détermine l'axe de rotation et l'indice de la couche en fonction du cube sélectionné.
    /// Cette méthode est un exemple simple utilisant la position Y du cube.
    /// </summary>
    /// <param name="cube">Le cube sélectionné</param>
    /// <returns>Un tuple contenant l'axe de rotation (Vector3) et l'indice de la couche (int)</returns>
    private (Vector3, int) GetCubeAxisAndLayer(GameObject cube)
    {
        // Ici, on choisit l'axe vertical (Y) pour la rotation.
        // La position locale en Y du cube par rapport au Rubik's Cube est utilisée pour déterminer la couche.
        int layerIndex = 0;
        Vector3 localPos = rubikGen.transform.InverseTransformPoint(cube.transform.position);

        // Seuil d'évaluation de la position en Y (à ajuster en fonction de votre configuration)
        float threshold = 0.5f;
        if (localPos.y > threshold)
        {
            layerIndex = rubikGen.cubeSize - 1; // Couche supérieure
        }
        else if (localPos.y < -threshold)
        {
            layerIndex = 0; // Couche inférieure
        }
        else
        {
            layerIndex = 1; // Couche du milieu
        }

        return (Vector3.up, layerIndex);
    }

    /// <summary>
    /// Récupère tous les cubes d'une couche donnée en parcourant le tableau 3D du Rubik's Cube.
    /// Cette implémentation utilise l'axe Y pour déterminer l'appartenance à une couche.
    /// </summary>
    /// <param name="axis">L'axe de rotation (non utilisé ici car l'exemple est basé sur Y)</param>
    /// <param name="layerIndex">L'indice de la couche</param>
    /// <returns>Un tableau de GameObject correspondant à la couche sélectionnée</returns>
    private GameObject[] GetLayerCubes(Vector3 axis, int layerIndex)
    {
        int cubeSize = rubikGen.cubeSize;
        System.Collections.Generic.List<GameObject> layerCubes = new System.Collections.Generic.List<GameObject>();

        // Parcourt tous les cubes et ajoute ceux dont l'indice Y correspond à la couche sélectionnée
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    if (y == layerIndex)
                    {
                        layerCubes.Add(rubikGen.cubeArray[x, y, z]);
                    }
                }
            }
        }
        return layerCubes.ToArray();
    }
}
