using UnityEngine;
using GameJolt.API;
using System;

public class RankingsManager : MonoBehaviour
{
    [Header("IDs de Leaderboards de GameJolt")]
    [SerializeField] private int totalTurnsRankingID = 1009434;
    [SerializeField] private int totalStepsRankingID = 1010446;

    public static RankingsManager Instance;

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

    public void SubmitGameStats(int totalTurns, int totalSteps)
    {
        try
        {
            if (GameJoltAPI.Instance == null)
            {
                Debug.LogError("GameJoltAPI.Instance es null - no se pueden subir puntuaciones");
                return;
            }

            if (GameJoltAPI.Instance.CurrentUser == null || !GameJoltAPI.Instance.CurrentUser.IsAuthenticated)
            {
                Debug.LogWarning("Usuario no autenticado - no se pueden subir puntuaciones");
                return;
            }

            //Puntuacion de turnos usados
            if (totalTurnsRankingID != 0)
            {
                try
                {
                    Scores.Add(totalTurns, $"{totalTurns} Turnos usados.", totalTurnsRankingID, "", success => {
                        if (success)
                        {
                            Debug.Log($"Turnos usados subidos exitosamente: {totalTurns}");
                        }
                        else
                        {
                            Debug.LogError($"Error al subir turnos usados: {totalTurns}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Excepción al subir turnos usados: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("ID del ranking de turnos no configurado");
            }

            //Puntuacion de pasos dados
            if (totalStepsRankingID != 0)
            {
                try
                {
                    Scores.Add(totalSteps, $"{totalSteps} Pasos dados.", totalStepsRankingID, "", success => {
                        if (success)
                        {
                            Debug.Log($"Pasos dados subidos exitosamente: {totalSteps}");
                        }
                        else
                        {
                            Debug.LogError($"Error al subir pasos dados: {totalSteps}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Excepción al subir pasos dados: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("ID del ranking de pasos no configurado");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error general al intentar subir records del juego: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }
}