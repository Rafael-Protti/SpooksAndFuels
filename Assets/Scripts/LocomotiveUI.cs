using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface em Espaço de Mundo (World Space) para a locomotiva.
/// Exibe a barra de vida na parte superior e a barra de combustível logo abaixo (entre a locomotiva e a vida).
/// Possui uma variável ajustável no Inspector para configurar a distância/altura em relação à locomotiva.
/// </summary>
public class LocomotiveUI : MonoBehaviour
{
    [Header("Locomotive Reference")]
    [Tooltip("Referência ao controlador da locomotiva")]
    [SerializeField] private LocomotiveController locomotive;

    [Header("Position & Offset")]
    [Tooltip("Distância/Offset vertical das barras acima da locomotiva (Ajustável no Inspector)")]
    [SerializeField] private float heightOffset = 3.8f;

    [Tooltip("Offset horizontal ou de profundidade (opcional)")]
    [SerializeField] private Vector3 additionalOffset = Vector3.zero;

    [Header("Health UI (Mais Acima)")]
    [Tooltip("Barra de preenchimento da vida da locomotiva")]
    [SerializeField] private Image healthFillBar;

    [Tooltip("Texto com o valor da vida (ex: VIDA: 5/5)")]
    [SerializeField] private Text healthText;

    [Header("Fuel UI (Entre a Locomotiva e a Vida)")]
    [Tooltip("Barra de preenchimento do combustível da locomotiva")]
    [SerializeField] private Image fuelFillBar;

    [Tooltip("Texto com o valor do combustível (ex: COMBUSTÍVEL: 100%)")]
    [SerializeField] private Text fuelText;

    private void LateUpdate()
    {
        UpdateLocomotiveBars();
    }

    /// <summary>
    /// Atualiza o preenchimento das barras e textos de saúde e combustível.
    /// </summary>
    private void UpdateLocomotiveBars()
    {
        // 1. Atualizar Barra e Texto de Saúde (Superior)
        if (healthFillBar != null && locomotive.MaxHealth > 0)
        {
            float healthPct = (float)locomotive.CurrentHealth / locomotive.MaxHealth;
            healthFillBar.fillAmount = Mathf.Clamp01(healthPct);
        }

        if (healthText != null)
        {
            healthText.text = $"INTEGRIDADE  {locomotive.CurrentHealth} / {locomotive.MaxHealth}";
        }

        // 2. Atualizar Barra e Texto de Combustível (Intermediária)
        if (fuelFillBar != null && locomotive.MaxFuel > 0f)
        {
            float fuelPct = locomotive.CurrentFuel / locomotive.MaxFuel;
            fuelFillBar.fillAmount = Mathf.Clamp01(fuelPct);
        }

        if (fuelText != null)
        {
            int fuelInt = Mathf.CeilToInt(locomotive.CurrentFuel);
            int maxFuelInt = Mathf.CeilToInt(locomotive.MaxFuel);
            fuelText.text = $"COMBUSTÍVEL  {fuelInt} / {maxFuelInt}";
        }
    }
}
