using UnityEngine;
using UnityEngine.UI;

public class SettingToMenu : MonoBehaviour
{
    public Button BackButton;
    public GameObject Menu;
    public GameObject Setting;
    void Start()
    {
        BackButton.onClick.AddListener(backToMenu);
    }

    void backToMenu() {
        Setting.SetActive(false);
        Menu.SetActive(true);
    }

}
