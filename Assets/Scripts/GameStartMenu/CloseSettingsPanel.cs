using UnityEngine;
using UnityEngine.UI;

public class CloseSettingsPanel : MonoBehaviour
{
    [Header("Close Buttons")]
    [SerializeField] private Button[] closeButtons;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        if (closeButtons == null)
        {
            return;
        }

        foreach (Button closeButton in closeButtons)
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSettings);
            }
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("Settings Panel이 지정되지 않았습니다.", this);
            return;
        }

        settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButtons == null)
        {
            return;
        }

        foreach (Button closeButton in closeButtons)
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseSettings);
            }
        }
    }
}
