using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuraci�n del Jugador")]
    public float moveSpeed = 2.0f;
    public string playerName = "Jugador";

    [Header("Sistema de Vida")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Sistema de Ataque")]
    public int minAttack = 7;
    public int maxAttack = 10;

    [Header("Estado del Turno (Solo Lectura)")]
    [SerializeField] private bool canMove = false;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private bool isMoving = false;
    private Vector3 targetWorldPosition;

    [Header("Sistema de Mejora")]
    public int attackIncreasePerKill = 1; 

    public System.Action<int, int> OnHealthChanged;
    public System.Action<int, int> OnAttackChanged;

    void Start()
    {
        currentHealth = maxHealth;

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

        Debug.Log($"Jugador inicializado en: ({gridX}, {gridZ})");
    }

    void Update()
    {
        if (!isMoving && canMove)
        {
            HandleInput();
        }

        HandleMovement();
    }

    void HandleInput()
    {
        if (gridManager == null || !canMove)
        {
            return;
        }

        Vector2Int moveDirection = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDirection = Vector2Int.right;
        }

        if (moveDirection != Vector2Int.zero)
        {
            TryMove(moveDirection);
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
            Debug.Log("Movimiento inv�lido (fuera del tablero o casilla ocupada)");
        }
    }

    void SetGridPosition(int gridX, int gridZ)
    {
        gridManager.SetCellOccupied(currentGridPosition.x, currentGridPosition.y, false);

        currentGridPosition = new Vector2Int(gridX, gridZ);

        gridManager.SetCellOccupied(gridX, gridZ, true, GridManager.CellType.Player);

        targetWorldPosition = gridManager.GetWorldPosition(gridX, gridZ);
        isMoving = true;

        Debug.Log($"Jugador se mueve a: ({gridX}, {gridZ})");

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerMove();
        }
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

                Debug.Log($"Jugador complet� el movimiento a: {currentGridPosition}");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{playerName} recibe {damage} de da�o. Vida actual: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{playerName} se cura {healAmount} puntos. Vida actual: {currentHealth}/{maxHealth}");
    }

    public void OnEnemyKilled()
    {
        IncreaseAttack(attackIncreasePerKill, attackIncreasePerKill);
        Debug.Log($"{playerName} se vuelve m�s fuerte! Nuevo ataque: {minAttack}-{maxAttack}");
    }

    public int PerformAttack()
    {
        int damage = Random.Range(minAttack, maxAttack + 1);
        Debug.Log($"{playerName} ataca por {damage} de da�o (rango: {minAttack}-{maxAttack})");
        return damage;
    }

    public void IncreaseAttack(int minIncrease, int maxIncrease)
    {
        minAttack += minIncrease;
        maxAttack += maxIncrease;

        OnAttackChanged?.Invoke(minAttack, maxAttack);

        Debug.Log($"{playerName} aumenta su ataque. Nuevo rango: {minAttack}-{maxAttack}");
    }

    void Die()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
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

    public void SetCanMove(bool canMoveState)
    {
        canMove = canMoveState;
        Debug.Log($"PlayerController - CanMove establecido a: {canMove}");
    }

    public Vector2Int GetGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool CanMove()
    {
        return canMove;
    }
}