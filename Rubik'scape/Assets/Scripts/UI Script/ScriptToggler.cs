using UnityEngine;
using UnityEngine.UI;

public class ScriptToggler : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour targetScript;

    [SerializeField]
    private Button toggleButton;

    [SerializeField]
    private Image buttonImage;

    [SerializeField]
    private Sprite activeSprite;

    [SerializeField]
    private Sprite inactiveSprite;

    private void Start()
    {
        toggleButton.onClick.AddListener(ToggleScript);
        UpdateButtonVisual();
    }

    private void ToggleScript()
    {
        targetScript.enabled = !targetScript.enabled;
        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (buttonImage != null && activeSprite != null && inactiveSprite != null)
        {
            buttonImage.sprite = targetScript.enabled ? activeSprite : inactiveSprite;
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleScript);
        }
    }
}
