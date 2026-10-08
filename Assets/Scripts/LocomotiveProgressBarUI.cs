using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a interface da barra de progresso da locomotiva.
/// </summary>
public class LocomotiveProgressBarUI : MonoBehaviour
{
    [Tooltip("Referência ao controlador da locomotiva")]
    [SerializeField] private LocomotiveController locomotive;

    [Tooltip("Barra de progresso (Slider)")]
    [SerializeField] private Slider progressBar;

    [Tooltip("Texto opcional para exibir a porcentagem")]
    [SerializeField] private Text progressText;

    private float totalDistance = 0f;

    private void Start()
    {
        // Se não foi definido no inspector, tenta achar automaticamente
        if (locomotive == null)
        {
            locomotive = FindAnyObjectByType<LocomotiveController>();
        }

        // Obtém a distância total do trilho
        if (locomotive != null && locomotive.TrackPathRef != null)
        {
            totalDistance = locomotive.TrackPathRef.GetTotalDistance();
        }

        // Configura o slider para ir de 0 a 1 (0% a 100%)
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }
    }

    private void Update()
    {
        if (locomotive == null || totalDistance <= 0f) return;

        // Calcula a porcentagem (0.0 até 1.0)
        float progress = Mathf.Clamp01(locomotive.CurrentDistance / totalDistance);

        // Atualiza o slider
        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        // Atualiza o texto, se existir
        if (progressText != null)
        {
            progressText.text = Mathf.RoundToInt(progress * 100f).ToString() + "%";
        }
    }
}
