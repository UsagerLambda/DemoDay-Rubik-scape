using UnityEngine;

public class RubikGen : MonoBehaviour
{
    public int cubeSize = 3;
    public float offset = 1.0f;
    public GameObject cubePrefab;
    public GameObject[] planePrefabs;
    public Vector3 pathPrefabScale = Vector3.one;
    private GameObject[,,] cubeArray;

    private int[][] facesData = new int[6][];

    void Start()
    {
        InitializeTestData(); // appel une fonction qui va initialiser des données dans la double list "facesData"
        GenerateRubiksCube(); // Génération du Rubik's cube
        ApplyPlanePrefabs(); // Change les faces visibles avec les données des tableaux dans "facesData"
    }

    void InitializeTestData()
    {
        facesData[0] = new int[] { 1, 2, 3, 2, 1, 3, 2, 1, 3 }; // Face avant
        facesData[1] = new int[] { 2, 1, 2, 3, 1, 2, 1, 3, 2 }; // Face arrière
        facesData[2] = new int[] { 3, 2, 1, 1, 2, 3, 2, 1, 3 }; // Face gauche
        facesData[3] = new int[] { 1, 3, 2, 2, 1, 3, 1, 2, 3 }; // Face droite
        facesData[4] = new int[] { 2, 1, 3, 3, 2, 1, 1, 3, 2 }; // Face dessus
        facesData[5] = new int[] { 3, 2, 1, 1, 3, 2, 2, 1, 3 }; // Face dessous
    }

    void GenerateRubiksCube() {
        cubeArray = new GameObject[cubeSize, cubeSize, cubeSize]; // Tableau 3D de taille cubeSize x cubeSize x cubeSize
        for (int x = 0; x < cubeSize; x++) { // axe X (Largeur)
            for (int y = 0; y < cubeSize; y++) { // axe Y (Profondeur)
                for (int z = 0; z < cubeSize; z++) { // axe Z (Hauteur)
                    // Calcul de la position d'un cube par rapport au 0, 0, 0 (x, y, z)
                    float posX = (x - (cubeSize - 1) / 2f) * offset; // calcule la position x du cube
                    float posY = (y - (cubeSize - 1) / 2f) * offset; // calcule la position y du cube
                    float posZ = (z - (cubeSize - 1) / 2f) * offset; // calcule la position z du cube
                    Vector3 position = transform.position + new Vector3(posX, posY, posZ); // récupère la position de l'objet parent avec transform.position et ajoute le décalage calculé

                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity); // clone le prefab (cubePrefab) avec la position, et aucune rotation
                    cube.transform.parent = transform; // Lie le cube à cet objet comme parent (et l'ajoute à sa hiérarchie)
                    cube.name = $"Cube{x}{y}_{z}"; // Défini le nom du cube par ces coordonnées
                    cubeArray[x, y, z] = cube; // stocke la posiiton du cube dans le tableau 3D: cubeArray
                }
            }
        }
    }

    void ApplyPlanePrefabs()
    {
        // parcourt tous les cubes du Rubik's cube en 3D
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    // Récupère le cube courant dans le tableau
                    GameObject cube = cubeArray[x, y, z];
                    
                    // Parcourt toutes les faces de ce cube (front, back, top ect...)
                    foreach (Transform child in cube.transform)
                    {
                        // Initialisation des variables pour identifier la face
                        bool isExteriorFace = false; // indique si c'est une face extérieure
                        int faceIndex = -1; // Index de la face (0 = Front, 1 = Back, 2 = Left ect...)
                        int planeIndex = -1; // Index spécifique de la face
                        Quaternion rotationOffset = Quaternion.identity; // rotation à appliquer

                        // vérifie chaque type de face et ses conditions pour être une face extérieure
                        switch (child.name)
                        {
                            case "Front":
                                // Identifie les cubes situés sur la face avant (dernière tranche en z)
                                if (z == cubeSize - 1)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 0;
                                    planeIndex = x + (y * cubeSize);
                                    rotationOffset = Quaternion.Euler(-90, 180, 0);
                                }
                                break;
                            case "Back":
                                // Identifie les cubes situés sur la face arrière (première tranche en z)
                                if (z == 0)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 1;
                                    planeIndex = (cubeSize - 1 - x) + (y * cubeSize);
                                    rotationOffset = Quaternion.Euler(-90, 0, 0);
                                }
                                break;
                            case "Left":
                                // Identifie les cubes situés sur la face gauche (première colonne en x)
                                if (x == 0)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 2;
                                    planeIndex = z + (y * cubeSize);
                                    rotationOffset = Quaternion.Euler(-90, 90, 0);
                                }
                                break;
                            case "Right":
                                // Identifie les cubes situés sur la face droite (dernière colonne en x)
                                if (x == cubeSize - 1)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 3;
                                    planeIndex = (cubeSize - 1 - z) + (y * cubeSize);
                                    rotationOffset = Quaternion.Euler(-90, -90, 0);
                                }
                                break;
                            case "Top":
                                // Identifie les cubes situés sur la face supérieure (dernière ligne en y)
                                if (y == cubeSize - 1)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 4;
                                    planeIndex = x + (z * cubeSize);
                                    rotationOffset = Quaternion.Euler(0, 0, 0);
                                }
                                break;
                            case "Bottom":
                                // Identifie les cubes situés sur la face inférieure (première ligne en y)
                                if (y == 0)
                                {
                                    isExteriorFace = true;
                                    faceIndex = 5;
                                    planeIndex = x + ((cubeSize - 1 - z) * cubeSize);
                                    rotationOffset = Quaternion.Euler(-180, 0, 0);
                                }
                                break;
                        }

                        // Si c'est une face extérieure valide
                        if (isExteriorFace && faceIndex != -1 && planeIndex != -1)
                        {
                            // Récupère l'index du prefab à utiliser depuis facesData
                            int prefabIndex = facesData[faceIndex][planeIndex];
                            // Vérifie que l'index est valide
                            if (prefabIndex >= 0 && prefabIndex < planePrefabs.Length)
                            {
                                // Crée une nouvelle instance de plan
                                GameObject newPlane = Instantiate(
                                    planePrefabs[prefabIndex], // Prefab choisi
                                    child.position, // Position de l'ancienne face
                                    rotationOffset, // Rotation calculée
                                    child.parent); // Même parent que l'ancienne face
                                
                                // Ajuste l'échelle du nouveau plan
                                newPlane.transform.localScale = Vector3.Scale(child.localScale, pathPrefabScale);
                                // Supprime l'ancienne face
                                Destroy(child.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
