using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerenciador de spawn de inimigos em pontos pré-definidos na cena (wave system).
/// Os fantasmas spawnam periodicamente em objetos da lista de spawn points.
/// O spawn para quando o número necessário de derrotas é atingido.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Lista de objetos na cena onde os fantasmas podem spawnar")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Prefabs & Probabilidades")]
    [Tooltip("Prefab do fantasma comum")]
    [SerializeField] private GameObject commonGhostPrefab;

    [Tooltip("Prefab do fantasma raro")]
    [SerializeField] private GameObject rareGhostPrefab;

    [Tooltip("Prefab do fantasma frágil")]
    [SerializeField] private GameObject fragileGhostPrefab;

    [Tooltip("Prefab do fantasma gigante")]
    [SerializeField] private GameObject giantGhostPrefab;

    [Header("Probabilidades de Spawn")]
    [Tooltip("Probabilidade de spawnar um fantasma COMUM (0 = desativado)")]
    [Range(0f, 1f)]
    [SerializeField] private float commonGhostProbability = 0.6f;

    [Tooltip("Probabilidade de spawnar um fantasma RARO (0 = desativado)")]
    [Range(0f, 1f)]
    [SerializeField] private float rareGhostProbability = 0.2f;

    [Tooltip("Probabilidade de spawnar um fantasma FRÁGIL (0 = desativado)")]
    [Range(0f, 1f)]
    [SerializeField] private float fragileGhostProbability = 0.15f;

    [Tooltip("Probabilidade de spawnar um fantasma GIGANTE (0 = desativado)")]
    [Range(0f, 1f)]
    [SerializeField] private float giantGhostProbability = 0.05f;

    [Header("Frequência & Limites")]
    [Tooltip("Intervalo de tempo entre as tentativas de spawn (em segundos)")]
    [SerializeField] private float spawnInterval = 4.0f;

    [Tooltip("Número máximo de inimigos ativos no mapa simultaneamente")]
    [SerializeField] private int maxEnemiesAlive = 25;

    [Header("Wave Settings")]
    [Tooltip("Quantos fantasmas precisam ser derrotados para completar a wave (0 = infinito)")]
    [SerializeField] private int enemiesToDefeat = 10;

    [Header("Wave Trigger")]
    [Tooltip("Waypoint do TrackPath que ativa a wave quando a locomotiva o alcança")]
    [SerializeField] private Transform waveTriggerPoint;

    [Tooltip("Distância mínima da locomotiva ao ponto de trigger para iniciar a wave")]
    [SerializeField] private float triggerDistance = 5f;

    [Header("Spawn Height")]
    [Tooltip("Habilitar para forçar uma altura Y específica no momento do spawn (ignorando o Y do spawn point)")]
    [SerializeField] private bool overrideSpawnHeight = false;

    [Tooltip("A altura Y na qual os fantasmas irão nascer se a opção acima estiver ligada")]
    [SerializeField] private float spawnHeightY = 1.5f;

    [Header("Gate")]
    [Tooltip("Portão que abre ao completar a wave (deve ter o script GateController)")]
    [SerializeField] private GateController gateToOpen;

    [Header("Encadeamento de Waves")]
    [Tooltip("Próximo spawner a ser ativado quando esta wave for concluída (deixe vazio se for a última)")]
    [SerializeField] private EnemySpawner nextSpawner;

    private float spawnTimer;
    private int currentSpawnPointIndex = 0;
    private int enemiesDefeated = 0;
    private bool waveComplete = false;
    private bool waveStarted = false;
    private LocomotiveController locomotive;

    // Propriedades públicas para a UI
    public int EnemiesToDefeat => enemiesToDefeat;
    public int EnemiesDefeated => enemiesDefeated;
    public bool IsWaveComplete => waveComplete;
    public bool IsWaveStarted => waveStarted;

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        // O primeiro spawner ativo se torna a instância
        if (Instance == null)
        {
            Instance = this;
        }

        if (enemiesToDefeat == 0) TriggerWaveWin();
    }

    private void Update()
    {
        if (waveComplete) return;

        // Verificar se a wave deve iniciar (trigger por waypoint)
        if (!waveStarted)
        {
            CheckWaveTrigger();
            return;
        }

        if (spawnPoints.Count == 0) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    /// <summary>
    /// Verifica se a locomotiva chegou ao ponto de trigger para iniciar a wave.
    /// Se não houver trigger point configurado, a wave inicia imediatamente.
    /// </summary>
    private void CheckWaveTrigger()
    {
        if (waveTriggerPoint == null)
        {
            waveStarted = true;
            Debug.Log("[EnemySpawner] Wave iniciada (sem trigger point configurado).");
            return;
        }

        if (locomotive == null)
        {
            locomotive = Object.FindAnyObjectByType<LocomotiveController>();
            if (locomotive == null) return;
        }

        float dist = Vector3.Distance(locomotive.transform.position, waveTriggerPoint.position);
        if (dist <= triggerDistance)
        {
            waveStarted = true;
            Debug.Log("[EnemySpawner] Locomotiva alcançou o ponto de trigger! Wave iniciada.");
        }
    }

    /// <summary>
    /// Tenta spawnar um fantasma em um dos pontos da lista.
    /// </summary>
    public void TrySpawnEnemy()
    {
        if (waveComplete) return;

        // Checar contagem atual de inimigos no mapa
        GhostEnemy[] currentGhosts = Object.FindObjectsByType<GhostEnemy>(FindObjectsInactive.Exclude);
        if (currentGhosts.Length >= maxEnemiesAlive) return;

        // Construir lista de tipos disponíveis (probabilidade > 0 e prefab atribuído)
        List<(GameObject prefab, float weight, string name)> availableTypes = new List<(GameObject, float, string)>();

        if (commonGhostProbability > 0f && commonGhostPrefab != null)
            availableTypes.Add((commonGhostPrefab, commonGhostProbability, "GhostEnemy_Common"));

        if (rareGhostProbability > 0f && rareGhostPrefab != null)
            availableTypes.Add((rareGhostPrefab, rareGhostProbability, "GhostEnemy_Rare"));

        if (fragileGhostProbability > 0f && fragileGhostPrefab != null)
            availableTypes.Add((fragileGhostPrefab, fragileGhostProbability, "GhostEnemy_Fragile"));

        if (giantGhostProbability > 0f && giantGhostPrefab != null)
            availableTypes.Add((giantGhostPrefab, giantGhostProbability, "GhostEnemy_Giant"));

        if (availableTypes.Count == 0) return;

        // Calcular peso total para normalizar
        float totalWeight = 0f;
        foreach (var t in availableTypes)
        {
            totalWeight += t.weight;
        }

        // Sortear tipo com base nos pesos normalizados
        float roll = Random.value * totalWeight;
        float accumulated = 0f;
        GameObject prefabToSpawn = availableTypes[0].prefab;
        string ghostName = availableTypes[0].name;

        foreach (var t in availableTypes)
        {
            accumulated += t.weight;
            if (roll <= accumulated)
            {
                prefabToSpawn = t.prefab;
                ghostName = t.name;
                break;
            }
        }

        // Escolher o próximo spawn point (ciclando pela lista)
        Transform spawnPoint = spawnPoints[currentSpawnPointIndex];
        currentSpawnPointIndex = (currentSpawnPointIndex + 1) % spawnPoints.Count;

        if (spawnPoint != null && prefabToSpawn != null)
        {
            Vector3 spawnPos = spawnPoint.position;
            if (overrideSpawnHeight)
            {
                spawnPos.y = spawnHeightY;
            }

            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, spawnPoint.rotation);
            newEnemy.name = ghostName;
        }
    }

    /// <summary>
    /// Chamado quando um fantasma é derrotado. Incrementa o contador e verifica se a wave foi concluída.
    /// </summary>
    public void OnEnemyDefeated()
    {
        enemiesDefeated++;

        if (enemiesToDefeat > 0 && enemiesDefeated >= enemiesToDefeat)
        {
            waveComplete = true;
            Debug.Log("[EnemySpawner] Wave concluída! Todos os inimigos necessários foram derrotados.");

            // Abrir o portão ao concluir a wave
            TriggerWaveWin();
        }
    }

    void TriggerWaveWin()
    {
        if (gateToOpen != null)
        {
            gateToOpen.OpenGate();
        }

        // Desativar este spawner e ativar o próximo
        if (nextSpawner != null)
        {
            Instance = nextSpawner;
            nextSpawner.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Reinicia a wave com novos parâmetros (útil para múltiplas waves).
    /// </summary>
    public void ResetWave(int newEnemiesToDefeat)
    {
        enemiesToDefeat = newEnemiesToDefeat;
        enemiesDefeated = 0;
        waveComplete = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.75f);
            }
        }
    }
}
