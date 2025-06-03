using UnityEngine;
using System;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public float moveSpeed = 1.5f;
    public string enemyName = "Enemigo";

    [Header("Sistema de Vida")]
    public int maxHealth = 50;
    [SerializeField] private int currentHealth;

    [Header("Sistema de Ataque")]
    public int minAttack = 3;
    public int maxAttack = 6;

    [Header("Configuración de Ataque")]
    public float attackAnimationSpeed = 8f;
    public float attackDistance = 0.3f;

    [Header("Estado del Turno (Solo Lectura)")]
    [SerializeField] private int movesRemaining = 0;
    [SerializeField] private bool isMyTurn = false;
    [SerializeField] private bool hasAttackedThisTurn = false;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private bool isMoving = false;
    private bool isAttacking = false;
    private Vector3 targetWorldPosition;
    private Action onMoveCompleteCallback;

    public System.Action<int, int> OnHealthChanged;
    public System.Action<int, int> OnAttackChanged;

    void Start()
    {
        currentHealth = maxHealth;

        var statsUI = GetComponentInChildren<EnemyStatsUI>();
        if (statsUI != null)
        {
            statsUI.Initialize(this);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnAttackChanged?.Invoke(minAttack, maxAttack);
    }

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
        hasAttackedThisTurn = false;
        onMoveCompleteCallback = onMoveComplete;

        PerformNextMove();
    }

    public void ContinueTurn()
    {
        if (isMyTurn && movesRemaining > 0 && !hasAttackedThisTurn)
        {
            PerformNextMove();
        }
    }

    void PerformNextMove()
    {
        if (!isMyTurn || movesRemaining <= 0 || isMoving || isAttacking || hasAttackedThisTurn)
        {
            return;
        }

        if (gridManager != null)
        {
            Vector2Int playerPos = gridManager.GetPlayerPosition();

            if (CanAttackPlayer())
            {
                Debug.Log($"{enemyName} puede atacar al jugador!");
                StartCoroutine(ExecuteAttackSequence());
                return;
            }

            MoveTowardsPlayer(playerPos);
        }
    }

    bool CanAttackPlayer()
    {
        if (gridManager == null) return false;

        Vector2Int playerPos = gridManager.GetPlayerPosition();
        Vector2Int difference = playerPos - currentGridPosition;

        bool isAdjacent = (Mathf.Abs(difference.x) <= 1 && Mathf.Abs(difference.y) <= 1 &&
                          (Mathf.Abs(difference.x) + Mathf.Abs(difference.y)) == 1);

        return isAdjacent;
    }

    IEnumerator ExecuteAttackSequence()
    {
        if (gridManager == null || hasAttackedThisTurn)
        {
            yield break;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError($"{enemyName}: No se encontró PlayerController para atacar");
            EndTurnWithCallback();
            yield break;
        }

        Debug.Log($"{enemyName} inicia ataque al jugador");

        isAttacking = true;
        hasAttackedThisTurn = true;

        int damage = PerformAttack();

        Vector3 originalPosition = transform.position;

        Vector3 targetPosition = player.transform.position;
        Vector3 direction = (targetPosition - originalPosition).normalized;
        Vector3 attackPosition = targetPosition - direction * attackDistance;

        float attackDuration = 0.15f;
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;

            if (Time.deltaTime <= 0)
            {
                break;
            }

            float progress = Mathf.Clamp01(elapsedTime / attackDuration);
            transform.position = Vector3.Lerp(originalPosition, attackPosition, progress);

            yield return null;
        }

        if (player == null || player.gameObject == null)
        {
            Debug.LogWarning($"{enemyName}: El jugador fue destruido durante el ataque");
            transform.position = originalPosition;
            isAttacking = false;
            EndTurnWithCallback();
            yield break;
        }

        player.TakeDamage(damage);

        Debug.Log($"{enemyName} causa {damage} de daño al jugador");

        yield return new WaitForSeconds(0.05f);

        if (player == null || player.gameObject == null)
        {
            Debug.Log($"{enemyName}: El jugador murió, terminando ataque");
            transform.position = originalPosition;
            isAttacking = false;
            EndTurnWithCallback();
            yield break;
        }

        float returnDuration = 0.15f;
        elapsedTime = 0f;

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;

            if (Time.deltaTime <= 0)
            {
                break;
            }

            float progress = Mathf.Clamp01(elapsedTime / returnDuration);
            transform.position = Vector3.Lerp(attackPosition, originalPosition, progress);

            yield return null;
        }

        transform.position = originalPosition;

        isAttacking = false;

        Debug.Log($"{enemyName} completa ataque y termina su turno");

        EndTurnWithCallback();
    }

    void MoveTowardsPlayer(Vector2Int playerPosition)
    {
        Vector2Int direction = Vector2Int.zero;
        Vector2Int difference = playerPosition - currentGridPosition;

        if (Mathf.Abs(difference.x) <= 1 && Mathf.Abs(difference.y) <= 1 &&
            (Mathf.Abs(difference.x) + Mathf.Abs(difference.y)) == 1)
        {
            Debug.Log($"{enemyName} está adyacente al jugador, terminando movimiento");
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
        if (isMoving && !isAttacking)
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
        if (isMyTurn && !hasAttackedThisTurn)
        {
            onMoveCompleteCallback?.Invoke();

            if (movesRemaining <= 1)
            {
                EndTurnWithCallback();
            }
        }
        else if (hasAttackedThisTurn)
        {
            EndTurnWithCallback();
        }
    }

    void EndTurnWithCallback()
    {
        isMyTurn = false;
        movesRemaining = 0;
        hasAttackedThisTurn = false;

        onMoveCompleteCallback?.Invoke();
    }

    void EndTurn()
    {
        isMyTurn = false;
        movesRemaining = 0;
        hasAttackedThisTurn = false;
        Debug.Log($"{enemyName} termina su turno");
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.SetCellOccupied(currentGridPosition.x, currentGridPosition.y, false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{enemyName} recibe {damage} de daño. Vida actual: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int PerformAttack()
    {
        int damage = UnityEngine.Random.Range(minAttack, maxAttack + 1);
        Debug.Log($"{enemyName} ataca por {damage} de daño (rango: {minAttack}-{maxAttack})");
        return damage;
    }

    void Die()
    {
        Debug.Log($"{enemyName} ha muerto!");

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnEnemyKilled();
        }

        if (VictoryChecker.Instance != null)
        {
            VictoryChecker.Instance.OnEnemyKilled();
        }

        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    public int GetMinAttack()
    {
        return minAttack;
    }

    public int GetMaxAttack()
    {
        return maxAttack;
    }

    public string GetAttackRange()
    {
        return $"{minAttack}-{maxAttack}";
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

    public bool IsAttacking()
    {
        return isAttacking;
    }
}