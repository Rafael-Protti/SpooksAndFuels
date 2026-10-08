using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exibe no Canvas o progresso da wave de inimigos.
/// Exemplo: "Inimigos a derrotar: 3/10"
/// </summary>
public class WaveProgressUI : MonoBehaviour
{
    [Tooltip("Texto da UI que exibe o progresso da wave")]
    [SerializeField] private Text waveText;

    private void Update()
    {
        if (waveText == null || EnemySpawner.Instance == null) return;

        // Oculta o texto caso a wave ainda não tenha iniciado
        if (!EnemySpawner.Instance.IsWaveStarted)
        {
            waveText.enabled = false;
            return;
        }

        waveText.enabled = true;

        int total = EnemySpawner.Instance.EnemiesToDefeat;
        int defeated = EnemySpawner.Instance.EnemiesDefeated;
        int remaining = Mathf.Max(0, total - defeated);

        if (EnemySpawner.Instance.IsWaveComplete)
        {
            waveText.text = "Wave concluída!";
        }
        else
        {
            waveText.text = $"Derrote: {remaining}/{total} fastasmas";
        }
    }
}
