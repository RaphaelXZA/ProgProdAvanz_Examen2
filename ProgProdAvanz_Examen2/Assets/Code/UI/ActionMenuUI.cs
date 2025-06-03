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

        Debug.Log("Menú de acciones mostrado");
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
            attackButton.interactable = adjacentEnemies.Count > 0 && !isAttacking;  

            Debug.Log($"Enemigos adyacentes encontrados: {adjacentEnemies.Count}");
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
                Debug.Log($"Enemigo {enemy.enemyName} encontrado en posición {checkPos}");
            }
        }

        return adjacentEnemies;
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

    void OnMoveButtonClicked()
    {
        int actualMovesLeft = TurnManager.Instance.GetPlayerMovesRemaining();
        if (actualMovesLeft <= 0)
        {
            Debug.Log("No se puede entrar al modo movimiento - no quedan movimientos");
            return;
        }

        Debug.Log("Botón Moverse clickeado");
        EnterMoveMode();
    }

    void OnAttackButtonClicked()
    {
        Debug.Log("Botón Atacar clickeado");
        ShowAttackMenu();
    }

    void ShowAttackMenu()
    {
        List<EnemyController> adjacentEnemies = GetAdjacentEnemies();

        if (adjacentEnemies.Count == 0)
        {
            Debug.Log("No hay enemigos adyacentes para atacar");
            return;
        }

        mainActionPanel.SetActive(false);
        moveActionPanel.SetActive(false);
        restConfirmPanel.SetActive(false);
        endTurnConfirmPanel.SetActive(false);
        attackResultPanel.SetActive(false);
        attackActionPanel.SetActive(true);

        ClearAttackButtons();
        GenerateAttackButtons(adjacentEnemies);

        Debug.Log($"Panel de ataque mostrado con {adjacentEnemies.Count} enemigos");
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

    void GenerateAttackButtons(List<EnemyController> enemies)
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

            Debug.Log($"Botón generado para atacar a {enemy.enemyName}");
        }
    }

    void OnEnemyAttackButtonClicked(EnemyController targetEnemy)
    {
        if (isAttacking || playerController == null || targetEnemy == null || !targetEnemy.IsAlive())
        {
            Debug.Log("No se puede atacar en este momento");
            return;
        }

        Debug.Log($"Iniciando ataque a {targetEnemy.enemyName}");

        isAttacking = true;

        attackActionPanel.SetActive(false);

        StartCoroutine(ExecuteAttackSequence(targetEnemy));
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

        Debug.Log($"Animación de ataque: {originalPosition} -> {attackPosition}");

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

        Debug.Log($"Daño aplicado: {damage} a {target.enemyName}");

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

        ShowAttackResult(target.enemyName, damage, target.IsAlive());
    }

    void ShowAttackResult(string enemyName, int damage, bool enemyStillAlive)
    {
        attackResultPanel.SetActive(true);

        if (attackResultText != null)
        {
            string resultMessage;
            if (enemyStillAlive)
            {
                resultMessage = $"Atacaste a {enemyName} por {damage} de daño.";
            }
            else
            {
                resultMessage = $"Atacaste a {enemyName} por {damage} de daño.\n\n¡{enemyName} ha sido derrotado!";
            }

            attackResultText.text = resultMessage;
        }

        Debug.Log($"Resultado del ataque mostrado. Enemigo vivo: {enemyStillAlive}");
    }

    void OnFinishTurnAfterAttackClicked()
    {
        Debug.Log("Terminando turno después del ataque");
        EndPlayerTurn();
    }

    void OnBackFromAttackClicked()
    {
        Debug.Log("Volviendo del panel de ataque al menú principal");
        ShowMainActionMenu();
    }

    void OnRestButtonClicked()
    {
        Debug.Log("Botón Descansar clickeado");
        ShowRestConfirmation();
    }

    void OnEndTurnButtonClicked()
    {
        Debug.Log("Botón Pasar Turno clickeado");
        ShowEndTurnConfirmation();
    }

    void OnBackToMainMenuClicked()
    {
        Debug.Log("Volviendo al menú principal");
        ExitMoveMode();
        ShowMainActionMenu();
    }

    void OnConfirmRestClicked()
    {
        Debug.Log("Confirmando descansar");
        PerformRest();
    }

    void OnCancelRestClicked()
    {
        Debug.Log("Cancelando descansar");
        ShowMainActionMenu();
    }

    void OnConfirmEndTurnClicked()
    {
        Debug.Log("Confirmando pasar turno");
        EndPlayerTurn();
    }

    void OnCancelEndTurnClicked()
    {
        Debug.Log("Cancelando pasar turno");
        ShowMainActionMenu();
    }

    void ShowRestConfirmation()
    {
        if (playerController == null)
        {
            Debug.LogError("No se puede descansar - PlayerController no encontrado");
            return;
        }

        int currentHealth = playerController.GetCurrentHealth();
        int maxHealth = playerController.GetMaxHealth();

        if (currentHealth >= maxHealth)
        {
            Debug.Log("No se puede descansar - vida ya está al máximo");
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

        Debug.Log("Panel de confirmación de descanso mostrado");
    }

    void PerformRest()
    {
        if (playerController == null)
        {
            Debug.LogError("No se puede descansar - PlayerController no encontrado");
            return;
        }

        playerController.Heal(healAmount);
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

        Debug.Log("Panel de confirmación de turno mostrado");
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
                Debug.Log("PlayerController no encontrado en Start - se buscará más tarde");
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
                        Debug.Log($"PlayerController encontrado en búsqueda exhaustiva: {playerController.name}");
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
            Debug.Log("No se puede entrar al modo movimiento - no quedan movimientos reales");
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
            Debug.LogError("PlayerController TODAVÍA no encontrado después de búsqueda exhaustiva!");
        }

        Debug.Log($"Modo movimiento activado. Movimientos restantes: {actualMovesLeft}");
    }

    void ExitMoveMode()
    {
        isInMoveMode = false;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        Debug.Log("Modo movimiento desactivado");
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

        Debug.Log($"Jugador se movió. Movimientos restantes: {actualMovesLeft}");

        if (actualMovesLeft <= 0)
        {
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            Debug.Log("Se agotaron los movimientos. El jugador debe usar 'Volver' para regresar al menú principal.");
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
        Debug.Log($"PlayerController asignado desde TurnManager: {controller.name}");
    }
}