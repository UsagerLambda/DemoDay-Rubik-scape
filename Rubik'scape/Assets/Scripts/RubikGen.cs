using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Classe responsable de la génération du Rubik's Cube
/// </summary>
public class RubikGen : MonoBehaviour
{
    // Taille du cube (nombre de cubes par côté)
    public int cubeSize = 3;

    // Espace entre chaque petit cube
    public float offset = 1.0f;

    // Prefab du petit cube à instancier
    public GameObject cubePrefab;

    // Tableau 3D stockant tous les petits cubes
    private GameObject[,,] cubeArray;

    // Référence au script de rotation dynamique
    public RubiksCubeDynamicRotation dynamicRotationScript;

    /// <summary>
    /// Méthode appelée au démarrage pour générer le cube
    /// </summary>
    void Start() {
        // Vérification du prefab avant génération
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab non assigné dans RubikGen");
            return;
        }

        // Génération de la structure du cube
        GenerateRubiksCube();

        // Initialisation du script de rotation dynamique
        if (dynamicRotationScript != null)
        {
            dynamicRotationScript.InitializeCube(cubeArray, cubeSize);
            dynamicRotationScript.rubiksCubeTransform = transform;
        }
    }

    /// <summary>
    /// Génère la structure complète du Rubik's Cube
    /// </summary>
    void GenerateRubiksCube() {
        // Initialisation du tableau 3D
        cubeArray = new GameObject[cubeSize, cubeSize, cubeSize];

        // Génération de chaque petit cube
        for (int x = 0; x < cubeSize; x++) {
            for (int y = 0; y < cubeSize; y++) {
                for (int z = 0; z < cubeSize; z++) {
                    // Calcul précis du placement de chaque cube
                    float posX = (x - (cubeSize - 1) / 2f) * offset;
                    float posY = (y - (cubeSize - 1) / 2f) * offset;
                    float posZ = (z - (cubeSize - 1) / 2f) * offset;
                    Vector3 position = transform.position + new Vector3(posX, posY, posZ);

                    // Instanciation du cube
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                    cube.transform.parent = transform;
                    cube.name = $"Cube{x}{y}_{z}";
                    cubeArray[x, y, z] = cube;
                }
            }
        }
    }
}

/// <summary>
/// Classe gérant la rotation dynamique du Rubik's Cube en VR
/// </summary>
public class RubiksCubeDynamicRotation : MonoBehaviour
{
    // Interacteur de grab pour la main
    [SerializeField] private XRBaseInteractor handInteractor;

    // Sensibilité de rotation
    [SerializeField] private float rotationSensitivity = 5f;

    // Transform du cube principal
    [HideInInspector] public Transform rubiksCubeTransform;

    // Tableau des cubes
    private GameObject[,,] cubeArray;
    private int cubeSize;

    // Variables de suivi de l'état de grab
    private bool isGrabbed = false;
    private Vector3 initialHandPosition;
    private Vector3 currentHandPosition;
    private Vector3 initialCubeRotation;
    private Vector3 rotationAxis = Vector3.zero;

    /// <summary>
    /// Mise à jour à chaque frame pour gérer l'interaction
    /// </summary>
    void Update()
    {
        // Vérification de la présence et du tracking de l'interacteur
        if (handInteractor != null && handInteractor is XRDirectInteractor directInteractor)
        {
            HandleCubeGrab();
            HandleDynamicRotation();
        }
        else
        {
            // Réinitialisation de l'état de grab
            isGrabbed = false;
        }
    }

    /// <summary>
    /// Gère la capture initiale du cube
    /// </summary>
    void HandleCubeGrab()
    {
        // Vérifie si le cube n'est pas déjà grab et si une sélection est en cours
        if (!isGrabbed && handInteractor is XRBaseInteractor interactor && interactor.hasSelection)
        {
            // Enregistre la position initiale et la rotation
            initialHandPosition = handInteractor.transform.position;
            initialCubeRotation = rubiksCubeTransform.localRotation.eulerAngles;
            isGrabbed = true;

            // Détermine l'axe de rotation
            DetermineRotationAxis();
        }
    }

    /// <summary>
    /// Détermine dynamiquement l'axe de rotation le plus probable
    /// </summary>
    void DetermineRotationAxis()
    {
        // Récupère la direction du grab
        Vector3 grabDirection = handInteractor.transform.forward;

        // Calcule la projection sur chaque axe
        float dotX = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.right));
        float dotY = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.up));
        float dotZ = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.forward));

        // Sélectionne l'axe avec la projection la plus importante
        if (dotX > dotY && dotX > dotZ)
            rotationAxis = Vector3.right;
        else if (dotY > dotX && dotY > dotZ)
            rotationAxis = Vector3.up;
        else
            rotationAxis = Vector3.forward;
    }

    /// <summary>
    /// Gère la rotation dynamique du cube
    /// </summary>
    void HandleDynamicRotation()
    {
        // Vérifie que le cube est grab et en cours de sélection
        if (isGrabbed && handInteractor is XRBaseInteractor interactor && interactor.hasSelection)
        {
            // Récupère la position actuelle de la main
            currentHandPosition = handInteractor.transform.position;
            Vector3 handMovement = currentHandPosition - initialHandPosition;

            // Calcule l'angle de rotation
            float rotationAmount = Vector3.Dot(handMovement, rotationAxis) * rotationSensitivity;

            // Applique la rotation au cube principal
            Quaternion newRotation = Quaternion.Euler(rotationAxis * rotationAmount + initialCubeRotation);
            rubiksCubeTransform.localRotation = newRotation;

            // Fait tourner les sous-parties
            RotateSubParts(rotationAxis, rotationAmount);
        }
    }

    /// <summary>
    /// Fait tourner les sous-parties du cube
    /// </summary>
    void RotateSubParts(Vector3 axis, float angle)
    {
        // Vérifie que le tableau de cubes est initialisé
        if (cubeArray == null) return;

        // Fait tourner chaque petit cube autour du centre du cube principal
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    if (cubeArray[x, y, z] != null)
                    {
                        cubeArray[x, y, z].transform.RotateAround(
                            rubiksCubeTransform.position,
                            axis,
                            angle
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Initialise le cube avec son tableau et sa taille
    /// </summary>
    public void InitializeCube(GameObject[,,] array, int size)
    {
        cubeArray = array;
        cubeSize = size;
    }
}