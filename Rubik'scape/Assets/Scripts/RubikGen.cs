using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubikGen : MonoBehaviour
{
    public int cubeSize = 3; // Taille du Rubik's cube (3x3x3 par défaut)
    public float offset = 1.0f; // Taille de l'espacement entre les cubes (1 par défaut)
    public GameObject cubePrefab; // Le prefab du cube vide qui contiendra les faces
    private GameObject[,,] cubeArray; // Tableau tridimensionnel qui stocke les coordonnées des cubes crée

    void Start() {
        GenerateRubiksCube(); // Appel de la fonction au lancement du jeu
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
}
