using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionMenuUI : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject mainActionPanel;
    public GameObject moveActionPanel;
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

    private bool isInMoveMode = false;

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
        restConfirmPanel.SetActive(false);  
        endTurnConfirmPanel.SetActive(false);
        isInMoveMode = false;
    }

    void UpdateButtonAvailability()
    {
        if (attackButton != null)
        {
            attackButton.interactable = false;
        }

        if (restButton != null && playerController != null)
        {
            bool canRest = playerController.GetCurrentHealth() < playerController.GetMaxHealth();
            restButton.interactable = canRest;
        }

        if (moveButton != null)
        {
            bool canMoveNow = TurnManager.Instance.GetPlayerMovesRemaining() > 0;
            moveButton.interactable = canMoveNow;   
        }
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
        //Lógica de ataque para el futuro
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