using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface HUD na tela (Screen Overlay) do jogador.
/// Posicionada no canto superior esquerdo para exibir o ícone de coração, barra de vida e valor numérico da saúde.
/// </summary>
public class PlayerHUDUI : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Referência ao controlador do jogador")]
    [SerializeField] private PlayerController playerController;

    [Header("UI Components")]
    [Tooltip("Barra de preenchimento da saúde do jogador")]
    [SerializeField] private Image healthFillBar;

    [Tooltip("Texto numérico com a vida atual (ex: 10 / 10)")]
    [SerializeField] private Text healthText;

    [Tooltip("Texto numérico com o carvão coletado do jogador")]
    [SerializeField] private Text coalText;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
            if (playerController == null) return;
        }

        UpdateHealthUI();
    }

    /// <summary>
    /// Atualiza os elementos visuais de vida do jogador.
    /// </summary>
    private void UpdateHealthUI()
    {
        int currentHealth = playerController.CurrentHealth;
        int maxHealth = playerController.MaxHealth;

        if (healthFillBar != null && maxHealth > 0)
        {
            float fillRatio = (float)currentHealth / maxHealth;
            healthFillBar.fillAmount = Mathf.Clamp01(fillRatio);
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if(coalText != null)
        {
            coalText.text = playerController.gameObject.GetComponent<PlayerItems>().coal.ToString();
        }
    }
}
