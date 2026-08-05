using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitGameButton;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool hideSettingsOnStart = true;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Awake()
    {
        if (hideSettingsOnStart && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
        }
    }

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("Game Scene Name이 비어 있습니다.", this);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("Settings Panel이 지정되지 않았습니다.", this);
            return;
        }

        settingsPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(StartGame);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveListener(QuitGame);
        }
    }
}
