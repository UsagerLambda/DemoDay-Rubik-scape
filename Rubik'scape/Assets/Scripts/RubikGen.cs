using UnityEngine;

/// <summary>
/// Classe responsable de la génération du Rubik's Cube et de sa configuration initiale
/// </summary>
public class RubikGen : MonoBehaviour
{
    // Configuration visuelle et structurelle du cube
    [Header("Configuration du cube")]
    public int cubeSize = 3;                    // Définit la taille du cube (3x3x3 par défaut)
    public float offset = 1.0f;                 // Espace entre chaque petit cube
    public GameObject cubePrefab;               // Préfab du petit cube à instancier
    
    // Configuration des propriétés physiques
    [Header("Configuration physique")]
    public float cubeWeight = 1f;               // Masse de chaque petit cube
    public PhysicMaterial cubeMaterial;         // Matériau physique pour les collisions
    
    // Variables privées pour le stockage des références
    [HideInInspector]
    public GameObject[,,] cubeArray;           // Tableau 3D stockant tous les cubes

    /// <summary>
    /// Appelé au démarrage, initialise le Rubik's Cube
    /// </summary>
    void Start()
    {
        // Vérifie que le préfab est bien assigné
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab non assigné dans RubikGen");
            return;
        }

        GenerateRubiksCube();     // Génère la structure du cube
    }

    /// <summary>
    /// Génère la structure complète du Rubik's Cube
    /// </summary>
    void GenerateRubiksCube()
    {
        // Initialise le tableau 3D
        cubeArray = new GameObject[cubeSize, cubeSize, cubeSize];

        // Génère chaque cube individuel
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    // Calcule la position de chaque cube
                    float posX = (x - (cubeSize - 1) / 2f) * offset;
                    float posY = (y - (cubeSize - 1) / 2f) * offset;
                    float posZ = (z - (cubeSize - 1) / 2f) * offset;
                    Vector3 position = transform.position + new Vector3(posX, posY, posZ);

                    // Crée et configure le cube
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                    cube.transform.parent = transform;
                    cube.name = $"Cube{x}{y}_{z}";
                    cube.tag = "CubePiece"; // Ajoute le tag pour l'interaction

                    // Ajoute les composants physiques
                    AddPhysicsComponents(cube);
                    
                    // Stocke la référence
                    cubeArray[x, y, z] = cube;
                }
            }
        }
    }

    /// <summary>
    /// Ajoute et configure les composants physiques sur un cube
    /// </summary>
    private void AddPhysicsComponents(GameObject cube)
    {
        // Ajoute et configure le collider
        BoxCollider collider = cube.AddComponent<BoxCollider>();
        collider.size = Vector3.one;
        if (cubeMaterial != null)
            collider.material = cubeMaterial;

        // Ajoute et configure le rigidbody
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = cubeWeight;
        rb.useGravity = false;    // Désactive la gravité pour le contrôle manuel
        rb.isKinematic = true;    // Active le mode cinématique pour le contrôle précis
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // Lisse le mouvement
    }
}