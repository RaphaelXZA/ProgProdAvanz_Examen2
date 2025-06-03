using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI attackText;  

    [Header("Colores de la Barra")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    [Header("Umbrales de Color")]
    [Range(0f, 1f)]
    public float warningThreshold = 0.5f;
    [Range(0f, 1f)]
    public float criticalThreshold = 0.25f;

    private PlayerController playerController;

    public static PlayerStatsUI Instance { get; private set; }

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
        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        }

        FindPlayerController();
    }

    void FindPlayerController()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.OnHealthChanged += OnPlayerHealthChanged;
            playerController.OnAttackChanged += OnPlayerAttackChanged;  

            if (playerNameText != null)
            {
                playerNameText.text = playerController.playerName;
            }

            OnPlayerHealthChanged(playerController.GetCurrentHealth(), playerController.GetMaxHealth());
            OnPlayerAttackChanged(playerController.GetMinAttack(), playerController.GetMaxAttack());  

            Debug.Log("PlayerStatsUI conectado al PlayerController");
        }
        else
        {
            Debug.LogWarning("PlayerController no encontrado para PlayerStatsUI");
            Invoke(nameof(FindPlayerController), 0.1f);
        }
    }

    void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
        {
            float healthPercentage = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            healthBarFill.fillAmount = healthPercentage;

            UpdateHealthBarColor(healthPercentage);
        }

        if (healthText != null)
        {
            healthText.text = $"Vida: {currentHealth} / {maxHealth}";
        }
    }

    void OnPlayerAttackChanged(int minAttack, int maxAttack)
    {
        if (attackText != null)
        {
            attackText.text = $"ATK: {minAttack} - {maxAttack}";
        }
    }

    void UpdateHealthBarColor(float healthPercentage)
    {
        if (healthBarFill == null) return;

        Color targetColor;

        if (healthPercentage <= criticalThreshold)
        {
            targetColor = criticalColor;
        }
        else if (healthPercentage <= warningThreshold)
        {
            targetColor = warningColor;
        }
        else
        {
            targetColor = healthyColor;
        }

        healthBarFill.color = targetColor;
    }

    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnHealthChanged -= OnPlayerHealthChanged;
            playerController.OnAttackChanged -= OnPlayerAttackChanged;  
        }
    }

    public void SetPlayerController(PlayerController controller)
    {
        if (playerController != null)
        {
            playerController.OnHealthChanged -= OnPlayerHealthChanged;
            playerController.OnAttackChanged -= OnPlayerAttackChanged;  
        }

        playerController = controller;

        if (playerController != null)
        {
            playerController.OnHealthChanged += OnPlayerHealthChanged;
            playerController.OnAttackChanged += OnPlayerAttackChanged;  

            if (playerNameText != null)
            {
                playerNameText.text = playerController.playerName;
            }

            OnPlayerHealthChanged(playerController.GetCurrentHealth(), playerController.GetMaxHealth());
            OnPlayerAttackChanged(playerController.GetMinAttack(), playerController.GetMaxAttack());  
        }
    }
}