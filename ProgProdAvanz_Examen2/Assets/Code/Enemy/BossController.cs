using UnityEngine;
using System;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Configuración del Boss")]
    public string bossName = "Boss Final";

    [Header("Sistema de Vida")]
    public int maxHealth = 150;
    [SerializeField] private int currentHealth;

    [Header("Sistema de Ataque")]
    public int minAttack = 15;
    public int maxAttack = 25;

    [Header("Configuración de Ataque")]
    public float attackAnimationSpeed = 8f;
    public float attackDistance = 0.3f;

    [Header("Estado del Turno (Solo Lectura)")]
    [SerializeField] private bool isMyTurn = false;
    [SerializeField] private bool hasAttackedThisTurn = false;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private bool isAttacking = false;
    private Action onTurnCompleteCallback;

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

    public void StartTurn(Action onTurnComplete)
    {
        isMyTurn = true;
        hasAttackedThisTurn = false;
        onTurnCompleteCallback = onTurnComplete;

        PerformTurnAction();
    }

    void PerformTurnAction()
    {
        if (!isMyTurn || isAttacking || hasAttackedThisTurn)
        {
            return;
        }

        if (gridManager != null)
        {
            if (CanAttackPlayer())
            {
                StartCoroutine(ExecuteAttackSequence());
                return;
            }
            else
            {
                EndTurnWithCallback();
            }
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
            Debug.LogError($"{bossName}: No se encontró PlayerController para atacar");
            EndTurnWithCallback();
            yield break;
        }

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
            Debug.LogWarning($"{bossName}: El jugador fue destruido durante el ataque");
            transform.position = originalPosition;
            isAttacking = false;
            EndTurnWithCallback();
            yield break;
        }

        player.TakeDamage(damage);


        yield return new WaitForSeconds(0.05f);

        if (player == null || player.gameObject == null)
        {
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

        EndTurnWithCallback();
    }

    void EndTurnWithCallback()
    {
        isMyTurn = false;
        hasAttackedThisTurn = false;

        onTurnCompleteCallback?.Invoke();
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int PerformAttack()
    {
        int damage = UnityEngine.Random.Range(minAttack, maxAttack + 1);
        return damage;
    }

    void Die()
    {
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

    public bool IsMyTurn()
    {
        return isMyTurn;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public string enemyName
    {
        get { return bossName; }
        set { bossName = value; }
    }
}