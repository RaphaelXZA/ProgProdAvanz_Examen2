using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ActionMenuUI : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject mainActionPanel;
    public GameObject moveActionPanel;
    public GameObject attackActionPanel;
    public GameObject attackResultPanel;
    public GameObject restConfirmPanel;
    public GameObject endTurnConfirmPanel;

    [Header("Botones del Menú Principal")]
    public Button moveButton;
    public Button attackButton;
    public Button restButton;
    public Button endTurnButton;

    [Header("Panel de Movimiento")]
    public TextMeshProUGUI movesRemainingText;
    public Button backToMainMenuButton;

    [Header("Panel de Ataque")]
    public Transform attackButtonsParent;
    public GameObject attackButtonPrefab;
    public Button backFromAttackButton;

    [Header("Panel de Resultado de Ataque")]
    public TextMeshProUGUI attackResultText;
    public Button finishTurnAfterAttackButton;

    [Header("Panel de Confirmación de Descanso")]
    public TextMeshProUGUI restDescriptionText;
    public Button confirmRestButton;
    public Button cancelRestButton;

    [Header("Panel de Confirmación de Turno")]
    public Button confirmEndTurnButton;
    public Button cancelEndTurnButton;

    [Header("Referencia al Jugador")]
    public PlayerController playerController;

    [Header("Configuración de Descanso")]
    public int healAmount = 25;

    [Header("Configuración de Ataque")]
    public float attackAnimationSpeed = 10f;
    public float attackDistance = 0.3f;

    private bool isInMoveMode = false;
    private bool isAttacking = false;
    private List<GameObject> generatedAttackButtons = new List<GameObject>();

    public static ActionMenuUI Instance { get; private set; }

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
        SetupButtonListeners();
        HideAllPanels();

        TryFindPlayerController();
    }

    void SetupButtonListeners()
    {
        //Botones del panel principal
        if (moveButton != null)
            moveButton.onClick.AddListener(OnMoveButtonClicked);

        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);

        if (restButton != null)
            restButton.onClick.AddListener(OnRestButtonClicked);

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        //Botón de volver del panel de movimiento
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);

        //Botón de volver del panel de ataque
        if (backFromAttackButton != null)
            backFromAttackButton.onClick.AddListener(OnBackFromAttackClicked);

        //Botón para terminar turno después de atacar
        if (finishTurnAfterAttackButton != null)
            finishTurnAfterAttackButton.onClick.AddListener(OnFinishTurnAfterAttackClicked);

        //Botones del panel de confirmación de descanso
        if (confirmRestButton != null)
            confirmRestButton.onClick.AddListener(OnConfirmRestClicked);

        if (cancelRestButton != null)
            cancelRestButton.onClick.AddListener(OnCancelRestClicked);

        //Botones del panel de confirmación de turno
        if (confirmEndTurnButton != null)
            confirmEndTurnButton.onClick.AddListener(OnConfirmEndTurnClicked);

        if (cancelEndTurnButton != null)
            cancelEndTurnButton.onClick.AddListener(OnCancelEndTurnClicked);
    }

    public void ShowMainActionMenu()
    {
        mainActionPanel.SetActive(true);
        moveActionPanel.SetActive(false);
        attackActionPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        restConfirmPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(false);
        isInMoveMode = false;

        UpdateButtonAvailability();

    }

    public void HideAllPanels()
    {
        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(false);
        attackActionPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        restConfirmPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(false);
        isInMoveMode = false;
    }

    void UpdateButtonAvailability()
    {
        if (attackButton != null)
        {
            List<EnemyController> adjacentEnemies = GetAdjacentEnemies();
            BossController adjacentBoss = GetAdjacentBoss();

            bool hasTargets = adjacentEnemies.Count > 0 || adjacentBoss != null;
            attackButton.interactable = hasTargets && !isAttacking;

        }

        if (restButton != null && playerController != null)
        {
            bool canRest = playerController.GetCurrentHealth() < playerController.GetMaxHealth() && !isAttacking;
            restButton.interactable = canRest;
        }

        if (moveButton != null)
        {
            bool canMoveNow = TurnManager.Instance.GetPlayerMovesRemaining() > 0 && !isAttacking;
            moveButton.interactable = canMoveNow;
        }
    }

    List<EnemyController> GetAdjacentEnemies()
    {
        List<EnemyController> adjacentEnemies = new List<EnemyController>();

        if (playerController == null) return adjacentEnemies;

        Vector2Int playerPos = playerController.GetGridPosition();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int checkPos = playerPos + direction;
            EnemyController enemy = GetEnemyAtPosition(checkPos);

            if (enemy != null && enemy.IsAlive())
            {
                adjacentEnemies.Add(enemy);
            }
        }

        return adjacentEnemies;
    }

    BossController GetAdjacentBoss()
    {
        if (playerController == null) return null;

        Vector2Int playerPos = playerController.GetGridPosition();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int checkPos = playerPos + direction;
            BossController boss = GetBossAtPosition(checkPos);

            if (boss != null && boss.IsAlive())
            {
                return boss;
            }
        }

        return null;
    }

    EnemyController GetEnemyAtPosition(Vector2Int position)
    {
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (EnemyController enemy in allEnemies)
        {
            if (enemy.GetGridPosition() == position && enemy.IsAlive())
            {
                return enemy;
            }
        }

        return null;
    }

    BossController GetBossAtPosition(Vector2Int position)
    {
        BossController[] allBosses = FindObjectsByType<BossController>(FindObjectsSortMode.None);

        foreach (BossController boss in allBosses)
        {
            if (boss.GetGridPosition() == position && boss.IsAlive())
            {
                return boss;
            }
        }

        return null;
    }

    void OnMoveButtonClicked()
    {
        int actualMovesLeft = TurnManager.Instance.GetPlayerMovesRemaining();
        if (actualMovesLeft <= 0)
        {
            return;
        }

        EnterMoveMode();
    }

    void OnAttackButtonClicked()
    {
        ShowAttackMenu();
    }

    void ShowAttackMenu()
    {
        List<EnemyController> adjacentEnemies = GetAdjacentEnemies();
        BossController adjacentBoss = GetAdjacentBoss();

        if (adjacentEnemies.Count == 0 && adjacentBoss == null)
        {
            Debug.Log("No hay enemigos ni boss adyacentes para atacar");
            return;
        }

        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(false);
        restConfirmPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        attackActionPanel.SetActive(true);

        ClearAttackButtons();
        GenerateAttackButtons(adjacentEnemies, adjacentBoss);

        int totalTargets = adjacentEnemies.Count + (adjacentBoss != null ? 1 : 0);
    }

    void ClearAttackButtons()
    {
        foreach (GameObject button in generatedAttackButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        generatedAttackButtons.Clear();
    }

    void GenerateAttackButtons(List<EnemyController> enemies, BossController boss)
    {
        if (attackButtonPrefab == null || attackButtonsParent == null)
        {
            Debug.LogError("Attack button prefab o parent no asignados en el inspector");
            return;
        }

        foreach (EnemyController enemy in enemies)
        {
            GameObject buttonObj = Instantiate(attackButtonPrefab, attackButtonsParent);
            generatedAttackButtons.Add(buttonObj);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = enemy.enemyName;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                EnemyController targetEnemy = enemy;
                button.onClick.AddListener(() => OnEnemyAttackButtonClicked(targetEnemy));
            }

        }

        if (boss != null)
        {
            GameObject buttonObj = Instantiate(attackButtonPrefab, attackButtonsParent);
            generatedAttackButtons.Add(buttonObj);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = boss.bossName;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnBossAttackButtonClicked(boss));
            }

        }
    }

    void OnEnemyAttackButtonClicked(EnemyController targetEnemy)
    {
        if (isAttacking || playerController == null || targetEnemy == null || !targetEnemy.IsAlive())
        {
            Debug.Log("No se puede atacar al enemigo");
            return;
        }


        isAttacking = true;
        attackActionPanel.SetActive(false);

        StartCoroutine(ExecuteAttackSequence(targetEnemy));
    }

    void OnBossAttackButtonClicked(BossController targetBoss)
    {
        if (isAttacking || playerController == null || targetBoss == null || !targetBoss.IsAlive())
        {
            Debug.Log("No se puede atacar al boss");
            return;
        }

        isAttacking = true;
        attackActionPanel.SetActive(false);

        StartCoroutine(ExecuteBossAttackSequence(targetBoss));
    }

    IEnumerator ExecuteAttackSequence(EnemyController target)
    {
        if (playerController == null || target == null)
        {
            Debug.LogError("PlayerController o target es null");
            yield break;
        }

        int damage = playerController.PerformAttack();

        Vector3 originalPosition = playerController.transform.position;

        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - originalPosition).normalized;
        Vector3 attackPosition = targetPosition - direction * attackDistance;

        float attackDuration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / attackDuration;

            playerController.transform.position = Vector3.Lerp(originalPosition, attackPosition, progress);

            yield return null;
        }

        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.1f);

        float returnDuration = 0.2f;
        elapsedTime = 0f;

        //ANIMACION DE ATAQUE
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / returnDuration;

            playerController.transform.position = Vector3.Lerp(attackPosition, originalPosition, progress);

            yield return null;
        }

        playerController.transform.position = originalPosition;

        ShowAttackResult(target.enemyName, damage, target.IsAlive());
    }

    IEnumerator ExecuteBossAttackSequence(BossController target)
    {
        if (playerController == null || target == null)
        {
            Debug.LogError("PlayerController o boss target es null");
            yield break;
        }

        int damage = playerController.PerformAttack();

        Vector3 originalPosition = playerController.transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - originalPosition).normalized;
        Vector3 attackPosition = targetPosition - direction * attackDistance;

        float attackDuration = 0.2f;
        float elapsedTime = 0f;

        //ANIMACION DE ATAQUE
        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / attackDuration;
            playerController.transform.position = Vector3.Lerp(originalPosition, attackPosition, progress);
            yield return null;
        }

        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.1f);

        float returnDuration = 0.2f;
        elapsedTime = 0f;

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / returnDuration;
            playerController.transform.position = Vector3.Lerp(attackPosition, originalPosition, progress);
            yield return null;
        }

        playerController.transform.position = originalPosition;

        ShowAttackResult(target.bossName, damage, target.IsAlive());
    }

    void ShowAttackResult(string targetName, int damage, bool targetStillAlive)
    {
        attackResultPanel.SetActive(true);

        if (attackResultText != null)
        {
            string resultMessage;
            if (targetStillAlive)
            {
                resultMessage = $"Atacaste a {targetName} por {damage} de daño.";
            }
            else
            {
                resultMessage = $"Atacaste a {targetName} por {damage} de daño.\n\n¡{targetName} ha sido derrotado!";
            }

            attackResultText.text = resultMessage;
        }

    }

    void OnFinishTurnAfterAttackClicked()
    {
        EndPlayerTurn();
    }

    void OnBackFromAttackClicked()
    {
        ShowMainActionMenu();
    }

    void OnRestButtonClicked()
    {
        ShowRestConfirmation();
    }

    void OnEndTurnButtonClicked()
    {
        ShowEndTurnConfirmation();
    }

    void OnBackToMainMenuClicked()
    {
        ExitMoveMode();
        ShowMainActionMenu();
    }

    void OnConfirmRestClicked()
    {
        PerformRest();
    }

    void OnCancelRestClicked()
    {
        ShowMainActionMenu();
    }

    void OnConfirmEndTurnClicked()
    {
        EndPlayerTurn();
    }

    void OnCancelEndTurnClicked()
    {
        ShowMainActionMenu();
    }

    void ShowRestConfirmation()
    {
        if (playerController == null)
        {
            return;
        }

        int currentHealth = playerController.GetCurrentHealth();
        int maxHealth = playerController.GetMaxHealth();

        if (currentHealth >= maxHealth)
        {
            return;
        }

        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(false);
        attackActionPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(false);
        restConfirmPanel.SetActive(true);

        if (restDescriptionText != null)
        {
            int healthToRecover = Mathf.Min(healAmount, maxHealth - currentHealth);
            restDescriptionText.text = $"Recuperarás {healAmount} de vida y tu turno terminará";
        }

    }

    void PerformRest()
    {
        if (playerController == null)
        {
            return;
        }

        playerController.Heal(healAmount);

        //TROFEO
        if (TrophyManager.Instance != null)
        {
            TrophyManager.Instance.OnRestActionUsed();
        }

        EndPlayerTurn();
    }

    void ShowEndTurnConfirmation()
    {
        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(false);
        attackActionPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        restConfirmPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(true);
    }

    void TryFindPlayerController()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                Debug.Log($"PlayerController encontrado en Start: {playerController.name}");
            }
            else
            {
                Debug.Log("PlayerController no encontrado en Start - se buscará más profundo...");
            }
        }
    }

    void EnsurePlayerControllerReference()
    {
        if (playerController == null)
        {
            Debug.Log("Buscando PlayerController...");
            playerController = FindFirstObjectByType<PlayerController>();

            if (playerController != null)
            {
                Debug.Log($"PlayerController encontrado: {playerController.name}");
            }
            else
            {
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        Debug.Log("PlayerController encontrado por nombre 'Player'");
                    }
                }

                if (playerController == null)
                {
                    PlayerController[] allControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
                    if (allControllers.Length > 0)
                    {
                        playerController = allControllers[0];
                        Debug.Log($"PlayerController encontrado en búsqueda profunda: {playerController.name}");
                    }
                }
            }
        }
    }

    void EnterMoveMode()
    {
        int actualMovesLeft = TurnManager.Instance.GetPlayerMovesRemaining();

        if (actualMovesLeft <= 0)
        {
            Debug.Log("No se puede entrar al modo movimiento - no quedan pasos");
            return;
        }

        isInMoveMode = true;

        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(true);

        UpdateMovesRemainingText();

        EnsurePlayerControllerReference();

        if (playerController != null)
        {
            Debug.Log($"PlayerController encontrado: {playerController.name}");
            playerController.SetCanMove(true);
            Debug.Log($"¿El jugador puede moverse? {playerController.CanMove()}");
        }
        else
        {
            Debug.LogError("PlayerController TODAVÍA no encontrado después de búsqueda profunda");
        }
    }

    void ExitMoveMode()
    {
        isInMoveMode = false;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }
    }

    void UpdateMovesRemainingText()
    {
        if (movesRemainingText != null)
        {
            int actualMovesLeft = TurnManager.Instance.GetPlayerMovesRemaining();
            movesRemainingText.text = $"Pasos restantes: {actualMovesLeft}";
        }
    }

    public void OnPlayerMoved()
    {
        if (!isInMoveMode) return;

        int actualMovesLeft = TurnManager.Instance.GetPlayerMovesRemaining();
        UpdateMovesRemainingText();

        if (actualMovesLeft <= 0)
        {
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
        }
    }

    public void EndPlayerTurn()
    {
        isAttacking = false;

        HideAllPanels();

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.ForceEndPlayerTurn();
        }
    }

    public bool IsInMoveMode()
    {
        return isInMoveMode;
    }

    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }
}