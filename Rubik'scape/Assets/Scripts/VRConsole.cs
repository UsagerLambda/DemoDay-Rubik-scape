using UnityEngine;
using TMPro;

public class VRConsole : MonoBehaviour
{
    public TextMeshProUGUI logText; // Référence au texte
    private string logBuffer = ""; // Stocke les logs

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog; // Écoute les logs Unity
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logBuffer = logString + "\n" + logBuffer; // Ajoute au buffer
        if (logBuffer.Length > 5000) // Évite un texte trop long
            logBuffer = logBuffer.Substring(0, 4000);
        
        logText.text = logBuffer; // Affiche le texte
    }

    void Start()
    {
        Debug.Log("VRConsole initialisee !"); // Test de log
    }
}
