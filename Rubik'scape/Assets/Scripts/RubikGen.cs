using UnityEngine;

/// <summary>
/// Classe responsable de la génération du Rubik's Cube et de sa configuration initiale.
/// </summary>
public class RubikGen : MonoBehaviour
{
    // Configuration visuelle et structurelle du cube
    [Header("Configuration du cube")]
    public int cubeSize = 3;                    // Taille du cube (exemple : 3x3x3)
    public float offset = 1.0f;                   // Espacement entre chaque petit cube
    public GameObject cubePrefab;               // Préfab à instancier pour chaque petit cube

    // Configuration des propriétés physiques
    [Header("Configuration physique")]
    public float cubeWeight = 1f;               // Masse de chaque petit cube
    public PhysicMaterial cubeMaterial;         // Matériau physique pour les collisions

    // Tableau 3D stockant toutes les références des cubes
    [HideInInspector]
    public GameObject[,,] cubeArray;

    /// <summary>
    /// Appelé au démarrage, vérifie le préfab et génère le Rubik's Cube.
    /// </summary>
    void Start()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab non assigné dans RubikGen");
            return;
        }
        GenerateRubiksCube();
    }

    /// <summary>
    /// Génère la structure complète du Rubik's Cube.
    /// </summary>
    void GenerateRubiksCube()
    {
        // Initialise le tableau 3D en fonction de la taille du cube
        cubeArray = new GameObject[cubeSize, cubeSize, cubeSize];

        // Boucle sur chaque dimension pour créer chaque petit cube
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    // Calcul de la position de chaque cube
                    float posX = (x - (cubeSize - 1) / 2f) * offset;
                    float posY = (y - (cubeSize - 1) / 2f) * offset;
                    float posZ = (z - (cubeSize - 1) / 2f) * offset;
                    Vector3 position = transform.position + new Vector3(posX, posY, posZ);

                    // Instanciation du cube et configuration de ses paramètres
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                    cube.transform.parent = transform;
                    cube.name = $"Cube{x}{y}_{z}";
                    cube.tag = "CubePiece"; // Permet l'interaction avec ce cube

                    // Ajoute les composants physiques (collider et rigidbody)
                    AddPhysicsComponents(cube);

                    // Stocke la référence du cube dans le tableau
                    cubeArray[x, y, z] = cube;
                }
            }
        }
    }

    /// <summary>
    /// Ajoute et configure les composants physiques sur un cube.
    /// </summary>
    /// <param name="cube">Le GameObject du cube</param>
    private void AddPhysicsComponents(GameObject cube)
    {
        // Ajout d'un BoxCollider
        BoxCollider collider = cube.AddComponent<BoxCollider>();
        collider.size = Vector3.one;
        if (cubeMaterial != null)
            collider.material = cubeMaterial;

        // Ajout d'un Rigidbody
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = cubeWeight;
        rb.useGravity = false;    // Désactive la gravité
        rb.isKinematic = true;    // Permet un contrôle manuel des mouvements
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // Lissage du mouvement
    }
}
