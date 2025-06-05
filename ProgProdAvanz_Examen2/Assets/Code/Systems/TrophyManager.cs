using UnityEngine;
using GameJolt.API;

public class TrophyManager : MonoBehaviour
{
    [Header("IDs de Trofeos de Game Jolt")]
    public int firstKillTrophyId = 0;
    public int threeKillsTrophyId = 0;
    public int bossKillTrophyId = 0;
    public int restActionTrophyId = 0;
    public int victoryTrophyId = 0;

    [Header("Progreso de Sesión")]
    public int enemiesKilledThisSession = 0;
    public bool hasBossKillThisSession = false;
    public bool hasUsedRestThisSession = false;
    public bool hasWonBoardThisSession = false;

    public static TrophyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetSessionProgress();
    }

    public void ResetSessionProgress()
    {
        enemiesKilledThisSession = 0;
        hasBossKillThisSession = false;
        hasUsedRestThisSession = false;
        hasWonBoardThisSession = false;
    }

    public void OnEnemyKilled()
    {
        enemiesKilledThisSession++;

        if (enemiesKilledThisSession == 1)
        {
            UnlockTrophy(firstKillTrophyId);
        }

        if (enemiesKilledThisSession >= 3)
        {
            UnlockTrophy(threeKillsTrophyId);
        }
    }

    public void OnBossKilled()
    {
        if (!hasBossKillThisSession)
        {
            hasBossKillThisSession = true;
            UnlockTrophy(bossKillTrophyId);
        }
    }

    public void OnRestActionUsed()
    {
        if (!hasUsedRestThisSession)
        {
            hasUsedRestThisSession = true;
            UnlockTrophy(restActionTrophyId);
        }
    }

    public void OnVictoryAchieved()
    {
        if (!hasWonBoardThisSession)
        {
            hasWonBoardThisSession = true;
            UnlockTrophy(victoryTrophyId);
        }
    }

    private void UnlockTrophy(int trophyId)
    {
        if (trophyId == 0) return;

        Trophies.Unlock(trophyId, success => {
            if (success)
            {
                Debug.Log($"Trofeo {trophyId} desbloqueado!");
            }
            else
            {
                Debug.Log($"Error al desbloquear trofeo {trophyId}");
            }
        });
    }
}