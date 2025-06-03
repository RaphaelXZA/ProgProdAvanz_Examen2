using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictorySceneManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button playAgainButton;
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
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }

    void OnPlayAgainButtonClicked()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void PlayAgain()
    {
        OnPlayAgainButtonClicked();
    }

    public void GoToMainMenu()
    {
        OnMainMenuButtonClicked();
    }

    void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}