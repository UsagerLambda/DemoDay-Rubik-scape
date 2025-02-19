using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public Button quitGame;
    void Start() {
        quitGame.onClick.AddListener(Exit);
    }

    // Update is called once per frame
    void Exit() {
        Application.Quit();
    }
}
