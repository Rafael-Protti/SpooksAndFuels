using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exibe no Canvas o progresso da wave de inimigos.
/// Exemplo: "Inimigos a derrotar: 3/10"
/// </summary>
public class WaveProgressUI : MonoBehaviour
{
    [Tooltip("Texto da UI que exibe o progresso da wave")]
    [SerializeField] private RawImage waveContainer;
    [SerializeField] private Text waveText;

    private void Update()
    {
        if (waveContainer == null || EnemySpawner.Instance == null) return;

        // Oculta o texto caso a wave ainda não tenha iniciado
        if (!EnemySpawner.Instance.IsWaveStarted)
        {
            waveContainer.enabled = false;
            waveContainer.transform.GetChild(0).GetComponent<Text>().enabled = false;
            waveContainer.transform.GetChild(1).GetComponent<Text>().enabled = false;
            return;
        }

        waveContainer.enabled = true;
        waveContainer.transform.GetChild(0).GetComponent<Text>().enabled = true;
        waveContainer.transform.GetChild(1).GetComponent<Text>().enabled = true;

        int total = EnemySpawner.Instance.EnemiesToDefeat;
        int defeated = EnemySpawner.Instance.EnemiesDefeated;
        int remaining = Mathf.Max(0, total - defeated);

        if (EnemySpawner.Instance.IsWaveComplete)
        {
            waveText.text = "Wave concluída!";
        }
        else
        {
            waveText.text = $"{total - remaining}/{total}";
        }
    }
}
