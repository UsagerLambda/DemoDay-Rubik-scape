using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Meta.XR;

/// <summary>
/// Classe responsable de la génération du Rubik's Cube
/// </summary>
public class RubikGen : MonoBehaviour
{
    // Configuration du cube
    [Header("Configuration du cube")]
    public int cubeSize = 3;                    // Taille du cube (3x3x3 par défaut)
    public float offset = 1.0f;                 // Espace entre les cubes
    public GameObject cubePrefab;               // Prefab du petit cube
    
    // Composants physiques
    [Header("Configuration physique")]
    public float cubeWeight = 1f;               // Poids de chaque petit cube
    public PhysicMaterial cubeMaterial;         // Matériau physique des cubes
    
    // Références
    private GameObject[,,] cubeArray;           // Tableau des cubes
    public RubiksCubeDynamicRotation dynamicRotationScript;

    void Start()
    {
        // Vérifications initiales
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab non assigné dans RubikGen");
            return;
        }

        GenerateRubiksCube();

        // Initialisation du script de rotation
        if (dynamicRotationScript != null)
        {
            dynamicRotationScript.InitializeCube(cubeArray, cubeSize);
            dynamicRotationScript.rubiksCubeTransform = transform;
        }
    }

    void GenerateRubiksCube()
    {
        cubeArray = new GameObject[cubeSize, cubeSize, cubeSize];

        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    // Calcul de la position
                    float posX = (x - (cubeSize - 1) / 2f) * offset;
                    float posY = (y - (cubeSize - 1) / 2f) * offset;
                    float posZ = (z - (cubeSize - 1) / 2f) * offset;
                    Vector3 position = transform.position + new Vector3(posX, posY, posZ);

                    // Création et configuration du cube
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                    cube.transform.parent = transform;
                    cube.name = $"Cube{x}{y}_{z}";

                    // Ajout des composants physiques
                    AddPhysicsComponents(cube);

                    cubeArray[x, y, z] = cube;
                }
            }
        }
    }

    /// <summary>
    /// Ajoute les composants physiques nécessaires à un cube
    /// </summary>
    private void AddPhysicsComponents(GameObject cube)
    {
        // Ajout/Configuration du Collider
        BoxCollider collider = cube.AddComponent<BoxCollider>();
        collider.size = Vector3.one;  // Taille 1x1x1
        if (cubeMaterial != null)
            collider.material = cubeMaterial;

        // Ajout/Configuration du Rigidbody
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = cubeWeight;
        rb.useGravity = false;        // Désactive la gravité
        rb.isKinematic = true;        // Rend le cube kinématique
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // Lisse le mouvement
    }
}

/// <summary>
/// Classe gérant la rotation dynamique du Rubik's Cube en VR
/// </summary>
public class RubiksCubeDynamicRotation : MonoBehaviour
{
    [Header("Configuration de la rotation")]
    [SerializeField] private float rotationSensitivity = 5f;    // Sensibilité de la rotation
    [SerializeField] private float snapThreshold = 0.5f;        // Seuil pour le snap de rotation

    [HideInInspector] public Transform rubiksCubeTransform;
    
    // État du cube
    private GameObject[,,] cubeArray;
    private int cubeSize;
    private Vector3 rotationAxis = Vector3.zero;
    private GameObject selectedCube;
    private Vector3 initialGrabPosition;
    private Vector3 initialRotation;
    private int selectedX, selectedY, selectedZ;

    private void OnEnable()
    {
        ConfigureInteractables();
    }

    /// <summary>
    /// Configure les interactables sur tous les cubes
    /// </summary>
    private void ConfigureInteractables()
    {
        if (cubeArray == null) return;

        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    if (cubeArray[x, y, z] != null)
                    {
                        var cube = cubeArray[x, y, z];
                        
                        // Configuration de l'interactable
                        var interactable = cube.AddComponent<XRGrabInteractable>();
                        interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                        interactable.throwOnDetach = false;
                        
                        // Ajout des listeners d'événements
                        int capturedX = x, capturedY = y, capturedZ = z;  // Capture pour la closure
                        interactable.selectEntered.AddListener((args) => OnCubeGrabbed(args, capturedX, capturedY, capturedZ));
                        interactable.selectExited.AddListener(OnCubeReleased);
                    }
                }
            }
        }
    }

    private void OnCubeGrabbed(SelectEnterEventArgs args, int x, int y, int z)
    {
        selectedCube = cubeArray[x, y, z];
        selectedX = x;
        selectedY = y;
        selectedZ = z;
        initialGrabPosition = args.interactorObject.transform.position;
        initialRotation = selectedCube.transform.localRotation.eulerAngles;
        
        DetermineRotationAxis(args.interactorObject.transform.position);
    }

    private void OnCubeReleased(SelectExitEventArgs args)
    {
        SnapRotation();
        selectedCube = null;
    }

    /// <summary>
    /// Détermine l'axe de rotation en fonction de la position de grab
    /// </summary>
    private void DetermineRotationAxis(Vector3 grabPosition)
    {
        Vector3 grabDirection = (grabPosition - rubiksCubeTransform.position).normalized;
        
        float dotX = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.right));
        float dotY = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.up));
        float dotZ = Mathf.Abs(Vector3.Dot(grabDirection, Vector3.forward));

        if (dotX > dotY && dotX > dotZ)
            rotationAxis = Vector3.right;
        else if (dotY > dotX && dotY > dotZ)
            rotationAxis = Vector3.up;
        else
            rotationAxis = Vector3.forward;
    }

    private void Update()
    {
        if (selectedCube != null)
        {
            var sectionCubes = GetSectionCubes();
            
            // Calcul de la rotation
            Vector3 currentGrabPosition = selectedCube.GetComponent<XRGrabInteractable>()
                .selectingInteractor.transform.position;
            Vector3 movement = currentGrabPosition - initialGrabPosition;
            float rotationAmount = Vector3.Dot(movement, rotationAxis) * rotationSensitivity;

            // Application de la rotation
            foreach (GameObject cube in sectionCubes)
            {
                if (cube != null)
                {
                    cube.transform.RotateAround(
                        rubiksCubeTransform.position,
                        rotationAxis,
                        rotationAmount
                    );
                }
            }
        }
    }

    /// <summary>
    /// Récupère tous les cubes de la même section (ligne/colonne)
    /// </summary>
    private GameObject[] GetSectionCubes()
    {
        GameObject[] sectionCubes = new GameObject[cubeSize];
        
        if (rotationAxis == Vector3.right)
        {
            for (int i = 0; i < cubeSize; i++)
                sectionCubes[i] = cubeArray[i, selectedY, selectedZ];
        }
        else if (rotationAxis == Vector3.up)
        {
            for (int i = 0; i < cubeSize; i++)
                sectionCubes[i] = cubeArray[selectedX, i, selectedZ];
        }
        else
        {
            for (int i = 0; i < cubeSize; i++)
                sectionCubes[i] = cubeArray[selectedX, selectedY, i];
        }
        
        return sectionCubes;
    }

    /// <summary>
    /// Aligne la rotation sur les multiples de 90 degrés
    /// </summary>
    private void SnapRotation()
    {
        if (selectedCube == null) return;

        Vector3 currentRotation = selectedCube.transform.localRotation.eulerAngles;
        Vector3 snappedRotation = new Vector3(
            Mathf.Round(currentRotation.x / 90f) * 90f,
            Mathf.Round(currentRotation.y / 90f) * 90f,
            Mathf.Round(currentRotation.z / 90f) * 90f
        );
        
        GameObject[] sectionCubes = GetSectionCubes();
        foreach (GameObject cube in sectionCubes)
        {
            if (cube != null)
            {
                cube.transform.localRotation = Quaternion.Euler(snappedRotation);
            }
        }
    }

    public void InitializeCube(GameObject[,,] array, int size)
    {
        cubeArray = array;
        cubeSize = size;
        ConfigureInteractables();
    }
}