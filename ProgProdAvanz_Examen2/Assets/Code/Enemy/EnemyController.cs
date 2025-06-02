using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public float moveSpeed = 1.5f;
    public string enemyName = "Enemigo";

    [Header("Estado del Turno (Solo Lectura)")]
    [SerializeField] private int movesRemaining = 0;
    [SerializeField] private bool isMyTurn = false;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;
    private Action onMoveCompleteCallback;

    public void SetGridManager(GridManager manager)
    {
        gridManager = manager;
    }

    public void SetInitialGridPosition(int gridX, int gridZ)
    {
        currentGridPosition = new Vector2Int(gridX, gridZ);
        transform.position = gridManager.GetWorldPosition(gridX, gridZ);

    }

    public void StartTurn(int totalMoves, Action onMoveComplete)
    {
        isMyTurn = true;
        movesRemaining = totalMoves;
        onMoveCompleteCallback = onMoveComplete;

        PerformNextMove();
    }

    public void ContinueTurn()
    {
        if (isMyTurn && movesRemaining > 0)
        {
            PerformNextMove();
        }
    }

    void PerformNextMove()
    {
        if (!isMyTurn || movesRemaining <= 0 || isMoving)
        {
            return;
        }

        if (gridManager != null)
        {
            Vector2Int playerPos = gridManager.GetPlayerPosition();
            MoveTowardsPlayer(playerPos);
        }
    }

    void MoveTowardsPlayer(Vector2Int playerPosition)
    {
        Vector2Int direction = Vector2Int.zero;
        Vector2Int difference = playerPosition - currentGridPosition;

        // Verificar si ya está adyacente al jugador
        if (Mathf.Abs(difference.x) <= 1 && Mathf.Abs(difference.y) <= 1 &&
            (Mathf.Abs(difference.x) + Mathf.Abs(difference.y)) == 1)
        {
            EndMove();
            return;
        }

        if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
        {
            direction.x = difference.x > 0 ? 1 : -1;
        }
        else if (difference.y != 0)
        {
            direction.y = difference.y > 0 ? 1 : -1;
        }

        if (direction != Vector2Int.zero)
        {
            TryMove(direction);
        }
        else
        {
            Debug.Log($"{enemyName} no puede determinar dirección de movimiento");
            EndMove();
        }
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int newGridPos = currentGridPosition + direction;

        if (gridManager.IsValidGridPosition(newGridPos.x, newGridPos.y) &&
            gridManager.IsCellFree(newGridPos.x, newGridPos.y))
        {
            SetGridPosition(newGridPos.x, newGridPos.y);
        }
        else
        {
            Debug.Log($"{enemyName} no puede moverse (casilla ocupada o inválida)");
            EndMove();
        }
    }

    void SetGridPosition(int gridX, int gridZ)
    {
        gridManager.SetCellOccupied(currentGridPosition.x, currentGridPosition.y, false);

        currentGridPosition = new Vector2Int(gridX, gridZ);

        gridManager.SetCellOccupied(gridX, gridZ, true, GridManager.CellType.Enemy);

        targetWorldPosition = gridManager.GetWorldPosition(gridX, gridZ);
        isMoving = true;

        Debug.Log($"{enemyName} se mueve a: ({gridX}, {gridZ})");
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
            {
                transform.position = targetWorldPosition;
                isMoving = false;

                EndMove();
            }
        }
    }

    void EndMove()
    {
        if (isMyTurn)
        {
            onMoveCompleteCallback?.Invoke();

            if (movesRemaining <= 1)
            {
                EndTurn();
            }
        }
    }

    void EndTurn()
    {
        isMyTurn = false;
        movesRemaining = 0;
        Debug.Log($"{enemyName} termina su turno");
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.SetCellOccupied(currentGridPosition.x, currentGridPosition.y, false);
        }
    }

    public Vector2Int GetGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsMyTurn()
    {
        return isMyTurn;
    }
}