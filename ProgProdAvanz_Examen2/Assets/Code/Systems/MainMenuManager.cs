using UnityEngine;
using GameJolt.API;
using UnityEngine.UI;
using GameJolt.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button trophiesListButton;
    [SerializeField] Button startGameButton;
    [SerializeField] Button leaderboardsButton;
    [SerializeField] string gameSceneName;

    private void Awake()
    {
        trophiesListButton.onClick.AddListener(() =>
        {
            GameJoltUI.Instance.ShowTrophies();

        });

        startGameButton.onClick.AddListener(() =>
        {
            StartGame();
        });

        leaderboardsButton.onClick.AddListener(() =>
        {
            GameJoltUI.Instance.ShowLeaderboards();
        });
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}
