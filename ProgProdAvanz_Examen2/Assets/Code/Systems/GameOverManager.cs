using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button retryButton;
    public Button mainMenuButton; 

    [Header("Configuración")]
    public string gameplaySceneName = "GameplayScene";  
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        SetupUI();
        SetupButtonListeners();
    }

    void SetupUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetupButtonListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        else
        {
            Debug.LogWarning("Botón de reintentar no asignado en el inspector");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
        else
        {
            Debug.LogWarning("Botón de menu no asignado en el inspector");
        }
    }

    void OnRetryButtonClicked()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}