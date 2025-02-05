using UnityEngine;
using TMPro;

public class ToggleScript : MonoBehaviour
{
    public GameObject targetObject;
    public string scriptName;
    public TMP_Text buttonText;

    public void OnButtonClick()
    {
        MonoBehaviour script = (MonoBehaviour)targetObject.GetComponent(scriptName);
        if(script != null)
        {
            script.enabled = !script.enabled;
            if(buttonText != null)
            {
                buttonText.text = script.enabled ? "ON" : "OFF";
            }
        }
    }
}
