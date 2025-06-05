using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatsUI : MonoBehaviour
{
    [Header("Referencias")]
    public Canvas statsCanvas;
    public Image healthBarBackground;
    public Image healthBarFill;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI attackText;

    [Header("Configuración")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float smoothTime = 0.1f;

    [Header("Colores")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    [Header("Umbrales")]
    [Range(0f, 1f)]
    public float warningThreshold = 0.5f;
    [Range(0f, 1f)]
    public float criticalThreshold = 0.25f;

    private EnemyController enemyController;
    private BossController bossController;

    private Camera playerCamera;
    private Vector3 velocity;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }

        if (statsCanvas != null)
        {
            statsCanvas.worldCamera = playerCamera;
            statsCanvas.renderMode = RenderMode.WorldSpace;

            statsCanvas.transform.localScale = Vector3.one * 0.01f;
        }

        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillAmount = 1f;
        }
    }

    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        Transform targetTransform = null;

        if (enemyController != null)
        {
            targetTransform = enemyController.transform;
        }
        else if (bossController != null)
        {
            targetTransform = bossController.transform;
        }

        if (targetTransform != null)
        {
            Vector3 targetPosition = targetTransform.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void Initialize(EnemyController controller)
    {
        enemyController = controller;
        bossController = null; 

        if (enemyController != null)
        {
            enemyController.OnHealthChanged += OnHealthChanged;
            enemyController.OnAttackChanged += OnAttackChanged;

            if (enemyNameText != null)
            {
                enemyNameText.text = enemyController.enemyName;
            }

            OnHealthChanged(enemyController.GetCurrentHealth(), enemyController.GetMaxHealth());
            OnAttackChanged(enemyController.GetMinAttack(), enemyController.GetMaxAttack());

        }
    }

    public void Initialize(BossController controller)
    {
        bossController = controller;
        enemyController = null; 

        if (bossController != null)
        {
            bossController.OnHealthChanged += OnHealthChanged;
            bossController.OnAttackChanged += OnAttackChanged;

            if (enemyNameText != null)
            {
                enemyNameText.text = bossController.enemyName;
            }

            OnHealthChanged(bossController.GetCurrentHealth(), bossController.GetMaxHealth());
            OnAttackChanged(bossController.GetMinAttack(), bossController.GetMaxAttack());

        }
    }

    void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
        {
            float healthPercentage = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            healthBarFill.fillAmount = healthPercentage;

            UpdateHealthBarColor(healthPercentage);
        }

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnAttackChanged(int minAttack, int maxAttack)
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
        if (enemyController != null)
        {
            enemyController.OnHealthChanged -= OnHealthChanged;
            enemyController.OnAttackChanged -= OnAttackChanged;
        }

        if (bossController != null)
        {
            bossController.OnHealthChanged -= OnHealthChanged;
            bossController.OnAttackChanged -= OnAttackChanged;
        }
    }

    public void SetHealthPercentage(float percentage)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Clamp01(percentage);
            UpdateHealthBarColor(percentage);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}