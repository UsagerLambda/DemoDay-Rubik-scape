using UnityEngine;
using UnityEngine.UI;

public class MenuToSetting : MonoBehaviour
{
    public Button SettingButton;
    public GameObject Menu;
    public GameObject Setting;
    void Start()
    {
        SettingButton.onClick.AddListener(goToSetting);
    }

    void goToSetting() {
        Menu.SetActive(false);
        Setting.SetActive(true);
    }
}
