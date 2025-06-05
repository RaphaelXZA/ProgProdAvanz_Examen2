using UnityEngine;
using System.Collections.Generic;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
    BossTurn,
    WaitingForMovement
}

public class TurnManager : MonoBehaviour
{
    [Header("Configuración de Turnos")]
    public int playerMovesPerTurn = 3;
    public int enemyMovesPerTurn = 2;

    [Header("Estado Actual (Solo Lectura)")]
    [SerializeField] private TurnState currentTurnState;
    [SerializeField] private int currentPlayerMoves;
    [SerializeField] private int currentEnemyIndex;
    [SerializeField] private int currentEnemyMoves;
    [SerializeField] private string currentTurnInfo;

    private GridManager gridManager;
    private PlayerController playerController;
    private List<EnemyController> enemies = new List<EnemyController>();
    private BossController bossController;
    private ActionMenuUI actionMenuUI;

    public static TurnManager Instance { get; private set; }

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
        gridManager = FindFirstObjectByType<GridManager>();
        actionMenuUI = FindFirstObjectByType<ActionMenuUI>();

        Invoke(nameof(InitializeTurnSystem), 0.1f);
    }

    void InitializeTurnSystem()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        enemies.Clear();
        EnemyController[] foundEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        enemies.AddRange(foundEnemies);

        bossController = FindFirstObjectByType<BossController>();
        if (bossController == null && gridManager != null)
        {
            bossController = gridManager.GetBossController();
        }

        if (actionMenuUI != null && playerController != null)
        {
            actionMenuUI.SetPlayerController(playerController);
        }

        StartPlayerTurn();
    }

    void Update()
    {
        UpdateTurnInfo();
    }

    void UpdateTurnInfo()
    {
        switch (currentTurnState)
        {
            case TurnState.PlayerTurn:
                currentTurnInfo = $"Turno del Jugador - Movimientos restantes: {currentPlayerMoves}";
                break;
            case TurnState.EnemyTurn:
                if (currentEnemyIndex < enemies.Count)
                {
                    string enemyName = enemies[currentEnemyIndex].enemyName;
                    currentTurnInfo = $"Turno de {enemyName} - Movimientos restantes: {currentEnemyMoves}";
                }
                break;
            case TurnState.BossTurn:
                if (bossController != null)
                {
                    currentTurnInfo = $"Turno del {bossController.bossName}";
                }
                break;
            case TurnState.WaitingForMovement:
                currentTurnInfo = "Esperando movimiento...";
                break;
        }
    }

    public void StartPlayerTurn()
    {
        currentTurnState = TurnState.PlayerTurn;
        currentPlayerMoves = playerMovesPerTurn;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }
        else
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
        }

        if (actionMenuUI != null)
        {
            actionMenuUI.ShowMainActionMenu();
        }

    }

    public void OnPlayerMove()
    {
        if (currentTurnState != TurnState.PlayerTurn) return;

        currentPlayerMoves--;

        if (actionMenuUI != null)
        {
            actionMenuUI.OnPlayerMoved();
        }
    }

    public void ForceEndPlayerTurn()
    {
        if (currentTurnState == TurnState.PlayerTurn)
        {
            EndPlayerTurn();
        }
    }

    void EndPlayerTurn()
    {
        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (actionMenuUI != null)
        {
            actionMenuUI.HideAllPanels();
        }

        StartEnemiesTurn();
    }

    void StartEnemiesTurn()
    {
        enemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive());

        if (enemies.Count == 0)
        {
            StartBossTurn();
            return;
        }

        currentTurnState = TurnState.EnemyTurn;
        currentEnemyIndex = 0;
        currentEnemyMoves = enemyMovesPerTurn;

        StartCurrentEnemyTurn();
    }

    void StartCurrentEnemyTurn()
    {
        if (currentEnemyIndex >= enemies.Count)
        {
            EndEnemiesTurn();
            return;
        }

        if (enemies[currentEnemyIndex] == null || !enemies[currentEnemyIndex].IsAlive())
        {
            currentEnemyIndex++;
            StartCurrentEnemyTurn();
            return;
        }

        EnemyController currentEnemy = enemies[currentEnemyIndex];
        currentEnemyMoves = enemyMovesPerTurn;

        currentEnemy.StartTurn(currentEnemyMoves, OnEnemyMoveComplete);
    }

    public void OnEnemyMoveComplete()
    {
        if (currentTurnState != TurnState.EnemyTurn) return;

        if (currentEnemyIndex >= enemies.Count || enemies[currentEnemyIndex] == null)
        {
            currentEnemyIndex++;
            StartCurrentEnemyTurn();
            return;
        }

        EnemyController currentEnemy = enemies[currentEnemyIndex];

        if (currentEnemy.IsMyTurn() == false)
        {
            currentEnemyIndex++;
            StartCurrentEnemyTurn();
            return;
        }

        currentEnemyMoves--;

        if (currentEnemyMoves <= 0)
        {
            currentEnemyIndex++;
            StartCurrentEnemyTurn();
        }
        else
        {
            currentEnemy.ContinueTurn();
        }
    }

    void EndEnemiesTurn()
    {
        StartBossTurn();
    }

    void StartBossTurn()
    {
        //Verificar si el boss sigue vivo
        if (bossController == null || !bossController.IsAlive())
        {
            StartPlayerTurn();
            return;
        }

        currentTurnState = TurnState.BossTurn;

        bossController.StartTurn(OnBossTurnComplete);
    }

    public void OnBossTurnComplete()
    {
        StartPlayerTurn();
    }

    public TurnState GetCurrentTurnState()
    {
        return currentTurnState;
    }

    public bool IsPlayerTurn()
    {
        return currentTurnState == TurnState.PlayerTurn;
    }

    public int GetPlayerMovesRemaining()
    {
        return currentPlayerMoves;
    }

    public bool IsBossAlive()
    {
        return bossController != null && bossController.IsAlive();
    }

    public BossController GetBossController()
    {
        return bossController;
    }
}