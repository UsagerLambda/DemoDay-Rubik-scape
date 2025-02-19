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

    void Start() {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick() {
        if (IsDescendant(perso.transform, RubiksCube))
        {
            perso.transform.parent = null; // Détache perso de son parent
            Debug.Log("perso a été détaché. Nouveau parent : " + (perso.transform.parent != null ? perso.transform.parent.name : "Aucun (null)"));
        }
        else
        {
            Debug.Log("perso n'était pas un enfant de RubiksCube.");
        }
        perso.SetActive(false);
        BackToMenu.SetActive(false);
        Canvas.SetActive(true);
        RemoveAllChildren();
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
}
