using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictorySceneManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Referencias de Estadísticas")]
    public TextMeshProUGUI totalTurnsText;
    public TextMeshProUGUI totalStepsText;

    [Header("Configuración")]
    public string gameplaySceneName = "GameplayScene";
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        SetupUI();
        SetupButtonListeners();
        DisplayGameStats();

        //TROFEO
        if (TrophyManager.Instance != null)
        {
            TrophyManager.Instance.OnVictoryAchieved();
        }
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

    void DisplayGameStats()
    {
        if (GameStatsManager.Instance != null)
        {
            int totalTurns = GameStatsManager.Instance.GetTotalPlayerTurns();
            int totalSteps = GameStatsManager.Instance.GetTotalStepsTaken();

            if (totalTurnsText != null)
            {
                totalTurnsText.text = $"Turnos Usados: {totalTurns}";
            }

            if (totalStepsText != null)
            {
                totalStepsText.text = $"Pasos Dados: {totalSteps}";
            }
        }
        else
        {
            Debug.LogWarning("VictoryScene: GameStatsManager no encontrado");

            if (totalTurnsText != null)
            {
                totalTurnsText.text = "Turnos Completados: --";
            }

            if (totalStepsText != null)
            {
                totalStepsText.text = "Pasos Dados: --";
            }
        }
    }

    void OnPlayAgainButtonClicked()
    {
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
        }

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