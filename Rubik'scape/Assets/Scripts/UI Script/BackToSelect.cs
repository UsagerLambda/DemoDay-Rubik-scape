using UnityEngine;
using UnityEngine.UI;

public class BackToSelect : MonoBehaviour
{
    public Button myButton;
    public Transform RubiksCube;
    public GameObject perso;
    public GameObject Canvas;
    public GameObject BackToMenu;
    public ScriptToggler scriptToggler;
    public GameObject VictoryUI;
    public Transform ContentContainer;

    void Start() {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
    }

    public void OnButtonClick() {
        if (scriptToggler.isRunning != false) {
            scriptToggler.ToggleScript();
        }

        if (IsDescendant(perso.transform, RubiksCube))
        {
            Debug.Log("PERSO PAS ENCORE DETACHER");
            perso.transform.parent = null; // Détache perso de son parent
            Debug.Log("perso a été détaché. Nouveau parent : " + (perso.transform.parent != null ? perso.transform.parent.name : "Aucun (null)"));
        }
        else
        {
            Debug.Log("perso n'était pas un enfant de RubiksCube.");
        }
        perso.SetActive(false);

        if (BackToMenu.activeSelf) {
            BackToMenu.SetActive(false);
        }

        Canvas.SetActive(true);
        RemoveAllChildren();
        if (VictoryUI.activeSelf) {
            VictoryUI.SetActive(false);
        }
        EnableAllLevelButtons();
        Debug.Log("Done");
    }

    bool IsDescendant(Transform child, Transform parent)
    {
        while (child.parent != null)
        {
            if (child.parent == parent)
                return true;
            child = child.parent;
        }
        return false;
    }

    public void RemoveAllChildren() {
        foreach (Transform child in RubiksCube)
        {
            Destroy(child.gameObject);
        }
        RubiksCube.gameObject.SetActive(false);
    }

    private void EnableAllLevelButtons() {
        foreach (Transform child in ContentContainer) {
            Button button = child.transform
                .Find("SelectButton/LevelSelectorButton")
                .GetComponent<Button>();

            button.interactable = true;
        }
    }
}
