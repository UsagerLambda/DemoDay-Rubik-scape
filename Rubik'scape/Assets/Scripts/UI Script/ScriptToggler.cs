using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScriptToggler : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetScript; // rotation à desactiver
    [SerializeField] private Button toggleButton; // bouton
    // [SerializeField] public TextMeshProUGUI buttonText; // texte
    [SerializeField] private GameObject perso; // perso ref
    [SerializeField] private Sprite blueButtonSprite; // Nouvelle référence pour le sprite bleu
    [SerializeField] private Sprite redButtonSprite;
    private MovementController movementController; // script déplacement perso
    public bool isRunning = false; // nouveau flag pour suivre l'état
    public GameObject RubiksCube;

    private void Start()
    {
        movementController = perso.GetComponent<MovementController>();
        toggleButton.onClick.AddListener(ToggleScript);

        // Configuration initiale
        targetScript.enabled = true; // Rotation activée au départ
        movementController.enabled = false; // Mouvement désactivé au départ
        UpdateButtonText(); // Mise à jour initiale du texte
    }

    public void ToggleScript()
    {
        isRunning = !isRunning; // Inverse l'état
        GameObject startTile = GameObject.FindWithTag("start");

        if (startTile == null)
        {
            Debug.LogError("No GameObject with 'start' tag found!");
            return;
        }

        if (isRunning) // Mode "Running"
        {
            Debug.Log("IS RUNNING");
            targetScript.enabled = false; // Désactive la rotation
            perso.transform.parent = null; // Détache le perso
            movementController.enabled = true; // Active le mouvement
        }
        else // Mode "I want to run"
        {
            Debug.Log("IS NOT RUNNING");
            targetScript.enabled = true; // Active la rotation
            movementController.enabled = false; // Désactive le mouvement
            perso.transform.position = startTile.transform.position;
            perso.transform.parent = startTile.transform;
            perso.transform.rotation = startTile.transform.rotation * Quaternion.Euler(0, 180, 0);

            foreach (Transform child in RubiksCube.GetComponentsInChildren<Transform>(true)) {
                if ((child.CompareTag("Point") || child.CompareTag("Multi")) && !child.gameObject.activeSelf) {
                    child.gameObject.SetActive(true);
            }
        }
        }

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        // buttonText.text = isRunning ? "Running ..." : "I want to run";

        Image buttonImage = toggleButton.GetComponent<Image>();
        if (buttonImage != null) {
            buttonImage.sprite = isRunning ? redButtonSprite : blueButtonSprite;
        }
    }
}

