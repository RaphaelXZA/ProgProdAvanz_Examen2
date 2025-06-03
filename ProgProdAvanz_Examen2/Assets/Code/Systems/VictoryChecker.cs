using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryChecker : MonoBehaviour
{
    [Header("Configuración")]
    public string enemyTag = "Enemy";
    public string victorySceneName = "VictoryScene";
    public float checkDelay = 0.5f;

    [Header("Estado (Solo Lectura)")]
    [SerializeField] private int currentEnemyCount = 0;
    [SerializeField] private bool victoryTriggered = false;

    public static VictoryChecker Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateEnemyCount();
    }

    public void OnEnemyKilled()
    {
        if (victoryTriggered) return;

        Debug.Log("VictoryManager: Enemigo eliminado, verificando condición de victoria...");

        StartCoroutine(CheckVictoryAfterDelay());
    }

    IEnumerator CheckVictoryAfterDelay()
    {
        yield return new WaitForSeconds(checkDelay);

        UpdateEnemyCount();

        if (currentEnemyCount <= 0 && !victoryTriggered)
        {
            TriggerVictory();
        }
    }

    void UpdateEnemyCount()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        currentEnemyCount = enemies.Length;

        Debug.Log($"VictoryManager: Enemigos restantes: {currentEnemyCount}");
    }

    void TriggerVictory()
    {
        victoryTriggered = true;

        Debug.Log("¡VICTORIA! Todos los enemigos han sido derrotados");

        SceneManager.LoadScene(victorySceneName);
    }

    public int GetEnemyCount()
    {
        return currentEnemyCount;
    }

    public bool IsVictoryTriggered()
    {
        return victoryTriggered;
    }
}