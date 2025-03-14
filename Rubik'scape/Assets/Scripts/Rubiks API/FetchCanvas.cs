using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class FetchCanvas : MonoBehaviour
{
    [Header("Prefab & Conteneur")]
    public GameObject PanelPrefab;
    public Transform ContentContainer;
    [Header("Références Externes")]
    public RubiksAPIManager apiManager;
    public GameObject curvedUnityCanvas;
    public GameObject rubikGenerator;
    public RubikGen rubikGen;
    public GameObject ChangeMode;
    public GameObject SetPlayerPosition;

    public void Initialize(GameObject canvas, RubikGen gen) {
        curvedUnityCanvas = canvas;
        rubikGen = gen;
    }

    public void HandleAllLevels(List<Level> levels)
    {
        ClearContentContainer();

        foreach (Level level in levels)
        {
            // Instancie la carte depuis le prefab dans le conteneur
            GameObject levelCard = Instantiate(PanelPrefab, ContentContainer);

            // Récupérer les composants TextMeshPro dans l'arborescence de la card crée
            TMP_Text levelNameText = levelCard.transform.Find("TitleContainer/LevelName").GetComponent<TMP_Text>();
            TMP_Text levelSizeText = levelCard.transform.Find("SizeContainer/LevelSize").GetComponent<TMP_Text>();

            // Et en modifie les valeurs
            levelNameText.text = level.name;
            levelSizeText.text = $"Size : {level.cube_size}";

            // Récupération du bouton avec le chemin exact
            Button levelSelectorButton = levelCard.transform
                .Find("SelectButton/LevelSelectorButton")
                .GetComponent<Button>();

            // Stockage de l'ID et configuration du listener
            string levelId = level.id;
            levelSelectorButton.onClick.AddListener(() => OnPlayButtonClick(levelId)); // écoute le button
        }
    }

    private void ClearContentContainer()
    {
        foreach (Transform child in ContentContainer) // Pour chaque enfant du parent ContentContainer
        {
            Destroy(child.gameObject); // Détruit l'enfant
        }
    }

    private void OnPlayButtonClick(string levelId) // fonction appelée au click du joueur sur le bouton
    {
        DisableAllLevelButtons();
        StartCoroutine(LoadLevelCoroutine(levelId)); // Appel la fonction pour faire un appel API Get pour un niveau spécifique
    }
    private IEnumerator LoadLevelCoroutine(string levelId) { // fonction coroutine pour attendre le resultat de l'appel GetLevel
        if (apiManager != null) { // si le lien avec RubiksAPIManager existe
            yield return StartCoroutine(apiManager.GetLevel(levelId)); // appel la fonction getLevel dans RubiksAPIManager
        }
        else {
            Debug.LogError("apiManager is null in LoadLevelCoroutine"); // si le lien avec RubiksAPIManager n'existe pas
        }
    }

    private void DisableAllLevelButtons() {
        // Parcourir tous les enfants du conteneur
        foreach (Transform child in ContentContainer) {
            // Trouver le bouton dans chaque carte
            Button button = child.transform
                .Find("SelectButton/LevelSelectorButton")
                .GetComponent<Button>();

            button.interactable = false;
        }
    }

    private void EnableAllLevelButtons() {
        foreach (Transform child in ContentContainer) {
            Button button = child.transform
                .Find("SelectButton/LevelSelectorButton")
                .GetComponent<Button>();

            button.interactable = true;
        }
    }

    public void HandleSingleLevel(Level level)
    {
        // Vérifier que les références sont bien initialisées
        if (curvedUnityCanvas == null || rubikGen == null)
        {
            Debug.LogError("CurvedUnityCanvas ou RubikGen non initialisé dans FetchCanvas");
            return;
        }

        // Cache le canvas curved
        curvedUnityCanvas.SetActive(false);

        // Prépare les données des faces
        int[][] facesData = new int[6][];
        for (int i = 1; i <= 6; i++)
        {
            string faceKey = $"face_{i}";
            if (level.faces_data.TryGetValue(faceKey, out int[] faceData))
            {
                facesData[i-1] = faceData;
            }
            else
            {
                Debug.Log($"Clé '{faceKey}' non trouvée dans faces_data.");
            }
        }
        rubikGenerator.SetActive(true);
        ChangeMode.SetActive(true);

        StartCoroutine(CheckUntilInitialized(level.name, level.cube_size, facesData));
    }

    IEnumerator CheckUntilInitialized(string name, int cube_size, int[][] facesData) {
        float timeout = 5f;
        rubikGen.InitializeRubiksCube(name, cube_size, facesData);

        float timer = 0f;
        while (!rubikGen.IsInitialized() && timer < timeout) {
            Debug.Log("waiting...");
            timer += Time.deltaTime;
            yield return null;
        }

        if (rubikGen.IsInitialized()) {
            SetPlayerPosition.SetActive(true);
        } else {
            Debug.LogError("Failed to initialize RubikGen properly");
            EnableAllLevelButtons();
        }
    }
}

