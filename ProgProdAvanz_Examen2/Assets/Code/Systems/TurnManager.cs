using UnityEngine;
using System.Collections.Generic;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn,
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

        Debug.Log($"TurnManager inicializado - Jugador: {(playerController != null ? "Encontrado" : "No encontrado")}, Enemigos: {enemies.Count}");

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
        if (enemies.Count == 0)
        {
            StartPlayerTurn();
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

        EnemyController currentEnemy = enemies[currentEnemyIndex];
        currentEnemyMoves = enemyMovesPerTurn;

        currentEnemy.StartTurn(currentEnemyMoves, OnEnemyMoveComplete);
    }

    public void OnEnemyMoveComplete()
    {
        if (currentTurnState != TurnState.EnemyTurn) return;

        currentEnemyMoves--;

        if (currentEnemyMoves <= 0)
        {
            currentEnemyIndex++;
            StartCurrentEnemyTurn();
        }
        else
        {
            EnemyController currentEnemy = enemies[currentEnemyIndex];
            currentEnemy.ContinueTurn();
        }
    }

    void EndEnemiesTurn()
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
}