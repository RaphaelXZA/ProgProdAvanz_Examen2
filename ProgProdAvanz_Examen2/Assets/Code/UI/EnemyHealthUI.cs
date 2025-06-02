using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    public Canvas healthCanvas;
    public Image healthBarBackground;
    public Image healthBarFill;
    public TextMeshProUGUI enemyNameText;

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
    private Camera playerCamera;
    private Vector3 velocity;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }

        if (healthCanvas != null)
        {
            healthCanvas.worldCamera = playerCamera;
            healthCanvas.renderMode = RenderMode.WorldSpace;

            healthCanvas.transform.localScale = Vector3.one * 0.01f;
        }

        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillAmount = 1f; 
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = enemyController.enemyName;
        }
    }

    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (enemyController != null)
        {
            Vector3 targetPosition = enemyController.transform.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void Initialize(EnemyController controller)
    {
        enemyController = controller;

        if (enemyController != null)
        {
            enemyController.OnHealthChanged += OnHealthChanged;

            OnHealthChanged(enemyController.GetCurrentHealth(), enemyController.GetMaxHealth());
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