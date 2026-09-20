using System.Collections;
using UnityEngine;

/// <summary>
/// Gerenciador de spawn aleatório de inimigos (Fantasmas Comuns e Raros).
/// Permite ajustar a probabilidade no Inspector (rareGhostProbability) e garante que
/// os inimigos não nasçam próximos do jogador nem da locomotiva.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Prefabs")]
    [Tooltip("Prefab do fantasma comum")]
    [SerializeField] private GameObject commonGhostPrefab;

    [Tooltip("Prefab do fantasma raro")]
    [SerializeField] private GameObject rareGhostPrefab;

    [Header("Probability & Frequency Settings")]
    [Tooltip("Intervalo de tempo entre as tentativas de spawn (em segundos)")]
    [SerializeField] private float spawnInterval = 4.0f;

    [Tooltip("Probabilidade de spawnar um fantasma RARO (0.0 = 0% até 1.0 = 100%). Ajustável no Inspector.")]
    [Range(0f, 1f)]
    [SerializeField] private float rareGhostProbability = 0.2f;

    [Tooltip("Número máximo de inimigos ativos no mapa simultaneamente")]
    [SerializeField] private int maxEnemiesAlive = 25;

    [Header("Spawn Safety Restrictions")]
    [Tooltip("Distância mínima do jogador para permitir o spawn")]
    [SerializeField] private float minDistanceFromPlayer = 15.0f;

    [Tooltip("Distância mínima da locomotiva para permitir o spawn")]
    [SerializeField] private float minDistanceFromLocomotive = 15.0f;

    [Header("Map Boundaries")]
    [Tooltip("Limite de spawn no eixo X (do centro até o limite positivo/negativo)")]
    [SerializeField] private float mapLimitX = 100.0f;

    [Tooltip("Limite de spawn no eixo Z (do centro até o limite positivo/negativo)")]
    [SerializeField] private float mapLimitZ = 100.0f;

    [Tooltip("Altura Y para o nascimento dos fantasmas voadores")]
    [SerializeField] private float spawnHeightY = 1.5f;

    // Referências dos alvos
    private PlayerController player;
    private LocomotiveController locomotive;
    private float spawnTimer;

    public float RareGhostProbability
    {
        get => rareGhostProbability;
        set => rareGhostProbability = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        player = Object.FindAnyObjectByType<PlayerController>();
        locomotive = Object.FindAnyObjectByType<LocomotiveController>();
    }

    private void Update()
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();
        if (locomotive == null) locomotive = Object.FindAnyObjectByType<LocomotiveController>();

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    /// <summary>
    /// Tenta encontrar uma posição válida no mapa e instanciar um fantasma (Comum ou Raro).
    /// </summary>
    public void TrySpawnEnemy()
    {
        // Checar contagem atual de inimigos no mapa
        GhostEnemy[] currentGhosts = Object.FindObjectsByType<GhostEnemy>(FindObjectsInactive.Exclude);
        if (currentGhosts.Length >= maxEnemiesAlive) return;

        Vector3 spawnPosition;
        if (TryGetValidSpawnPosition(out spawnPosition))
        {
            // Sortear com base na probabilidade configurada no Inspector
            bool spawnRare = Random.value < rareGhostProbability;
            GameObject prefabToSpawn = spawnRare ? rareGhostPrefab : commonGhostPrefab;

            if (prefabToSpawn != null)
            {
                GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                newEnemy.name = spawnRare ? "GhostEnemy_Rare" : "GhostEnemy_Common";
            }
            else
            {
                // Fallback: criar um objeto com o componente GhostEnemy dinamicamente se o prefab não estiver atribuído
                CreatePlaceholderGhost(spawnPosition, spawnRare ? GhostType.Rare : GhostType.Common);
            }
        }
    }

    /// <summary>
    /// Tenta gerar uma posição aleatória no mapa respeitando as distâncias mínimas do jogador e da locomotiva.
    /// </summary>
    private bool TryGetValidSpawnPosition(out Vector3 validPosition)
    {
        validPosition = Vector3.zero;
        int maxAttempts = 15;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-mapLimitX, mapLimitX);
            float randomZ = Random.Range(-mapLimitZ, mapLimitZ);
            Vector3 candidatePos = new Vector3(randomX, spawnHeightY, randomZ);

            // Validar distância do jogador
            if (player != null)
            {
                float distPlayer = Vector3.Distance(candidatePos, player.transform.position);
                if (distPlayer < minDistanceFromPlayer) continue;
            }

            // Validar distância da locomotiva
            if (locomotive != null)
            {
                float distLoco = Vector3.Distance(candidatePos, locomotive.transform.position);
                if (distLoco < minDistanceFromLocomotive) continue;
            }

            // Posição válida encontrada!
            validPosition = candidatePos;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Cria um fantasma placeholder visual com GhostEnemy configurado se nenhum prefab estiver selecionado no Inspector.
    /// </summary>
    private void CreatePlaceholderGhost(Vector3 position, GhostType type)
    {
        GameObject ghostObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ghostObj.name = type == GhostType.Rare ? "GhostEnemy_Rare" : "GhostEnemy_Common";
        ghostObj.transform.position = position;
        ghostObj.transform.localScale = type == GhostType.Rare ? new Vector3(1.3f, 1.3f, 1.3f) : new Vector3(0.9f, 0.9f, 0.9f);

        // Ajustar colisor para ser Trigger
        Collider col = ghostObj.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Cor do fantasma (Ciano para Comum, Roxo/Vermelho para Raro)
        Renderer rend = ghostObj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rend.material.color = type == GhostType.Rare ? new Color(0.8f, 0.1f, 0.9f) : new Color(0.2f, 0.8f, 0.9f);
        }

        GhostEnemy ghostScript = ghostObj.AddComponent<GhostEnemy>();
        // Reflection ou SerializedObject pode ajustar ghostType, ou no Awake padrão
    }

    private void OnDrawGizmosSelected()
    {
        // Desenhar área do mapa
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapLimitX * 2f, 2f, mapLimitZ * 2f));
    }
}
