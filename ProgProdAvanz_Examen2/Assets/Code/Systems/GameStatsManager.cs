using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatsManager : MonoBehaviour
{
    [Header("Estadísticas de la Partida (Solo Lectura)")]
    [SerializeField] private int totalPlayerTurns = 0;
    [SerializeField] private int totalStepsTaken = 0;

    [Header("Configuración")]
    public string gameplaySceneName = "GameplayScene";

    public static GameStatsManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetStats();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameplaySceneName)
        {
            ResetStats();
        }

        //REINICIAR TROFEOS
        if (TrophyManager.Instance != null)
        {
            TrophyManager.Instance.ResetSessionProgress();
        }
    }

    public void ResetStats()
    {
        totalPlayerTurns = 0;
        totalStepsTaken = 0;
        Debug.Log("GameStatsManager: Estadísticas reiniciadas");
    }

    public void OnPlayerTurnStarted()
    {
        totalPlayerTurns++;
    }

    public void OnPlayerStepTaken()
    {
        totalStepsTaken++;
    }

    public int GetTotalPlayerTurns()
    {
        return totalPlayerTurns;
    }

    public int GetTotalStepsTaken()
    {
        return totalStepsTaken;
    }

    public string GetStatsString()
    {
        return $"Turnos Usados: {totalPlayerTurns}\nPasos Dados: {totalStepsTaken}";
    }
}