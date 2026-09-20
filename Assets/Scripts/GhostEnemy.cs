using UnityEngine;

/// <summary>
/// Tipo do fantasma inimigo:
/// Common: Ataca o jogador, 1 de vida, morre com ataque do jogador ou colisão com a locomotiva, velocidade média.
/// Rare: Ataca a locomotiva, 2 de vida, morre APENAS com ataque do jogador, velocidade rápida.
/// </summary>
public enum GhostType
{
    Common,
    Rare
}

/// <summary>
/// Controlador dos fantasmas inimigos (Comuns e Raros).
/// Gerencia a IA de perseguição, causação de dano no alvo e stubs de drop de loot.
/// </summary>
public class GhostEnemy : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [Tooltip("Tipo do fantasma (Comum ou Raro)")]
    [SerializeField] private GhostType ghostType = GhostType.Common;

    [Tooltip("Saúde máxima do fantasma (1 para Comum, 2 para Raro)")]
    [SerializeField] private int maxHealth = 1;

    [Tooltip("Velocidade de movimento do fantasma em direção ao alvo")]
    [SerializeField] private float moveSpeed = 4.0f;

    [Tooltip("Dano causado por ataque ao entrar em contato")]
    [SerializeField] private int attackDamage = 1;

    [Tooltip("Tempo de espera entre ataques contínuos de contato (em segundos)")]
    [SerializeField] private float attackCooldown = 1.0f;

    [Tooltip("Distância mínima de contato para aplicar o ataque")]
    [SerializeField] private float attackRadius = 1.2f;

    // Estado interno
    private int currentHealth;
    private float lastAttackTime = -999f;
    private Transform targetTransform;
    private PlayerController playerTarget;
    private LocomotiveController locomotiveTarget;

    public GhostType Type => ghostType;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        // Configuração inicial padrão baseada no tipo
        if (ghostType == GhostType.Common)
        {
            maxHealth = 1;
            moveSpeed = 4.0f;
        }
        else
        {
            maxHealth = 2;
            moveSpeed = 6.0f;
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        if (targetTransform == null)
        {
            FindTarget();
            if (targetTransform == null) return;
        }

        // Movimentação em linha reta em direção ao alvo
        Vector3 direction = (targetTransform.position - transform.position);
        direction.y = 0; // Manter altura constante de voo

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10.0f * Time.deltaTime);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        // Checar distância para aplicar dano por contato
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        if (distanceToTarget <= attackRadius)
        {
            PerformContactAttack();
        }
    }

    /// <summary>
    /// Localiza o alvo apropriado com base no tipo de fantasma.
    /// Comum -> Alvo é o Jogador.
    /// Raro -> Alvo é a Locomotiva.
    /// </summary>
    private void FindTarget()
    {
        if (ghostType == GhostType.Common)
        {
            if (playerTarget == null)
            {
                playerTarget = Object.FindAnyObjectByType<PlayerController>();
            }
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
        }
        else
        {
            if (locomotiveTarget == null)
            {
                locomotiveTarget = Object.FindAnyObjectByType<LocomotiveController>();
            }
            if (locomotiveTarget != null)
            {
                targetTransform = locomotiveTarget.transform;
            }
        }
    }

    /// <summary>
    /// Executa o ataque de contato no alvo quando dentro do alcance.
    /// </summary>
    private void PerformContactAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        if (ghostType == GhostType.Common && playerTarget != null)
        {
            Debug.Log($"[GhostEnemy] Fantasma Comum causou {attackDamage} de dano ao Jogador!");
            playerTarget.TakeDamage(attackDamage);
        }
        else if (ghostType == GhostType.Rare && locomotiveTarget != null)
        {
            Debug.Log($"[GhostEnemy] Fantasma Raro causou {attackDamage} de dano à Locomotiva!");
            locomotiveTarget.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Aplica dano ao fantasma.
    /// Fantasmas Raros só recebem dano de ataques diretos do jogador.
    /// </summary>
    /// <param name="damageAmount">Quantidade de dano recebida</param>
    /// <param name="isPlayerAttack">Indica se o dano veio de um ataque do jogador</param>
    public void TakeDamage(int damageAmount, bool isPlayerAttack = false)
    {
        // Fantasmas Raros imunes a dano que não venha do jogador (ex: atropelamento por locomotiva)
        if (ghostType == GhostType.Rare && !isPlayerAttack)
        {
            Debug.Log("[GhostEnemy] Fantasma Raro é imune à colisão com a locomotiva! Apenas o ataque do jogador causa dano.");
            return;
        }

        currentHealth -= damageAmount;
        Debug.Log($"[GhostEnemy] {ghostType} recebeu {damageAmount} de dano. Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Trata a morte do fantasma, gerando o loot e destruindo o GameObject.
    /// </summary>
    public void Die()
    {
        DropLoot();
        Debug.Log($"[GhostEnemy] {ghostType} foi derrotado!");
        Destroy(gameObject);
    }

    /// <summary>
    /// Stub para o sistema de drop de loot ao morrer.
    /// Gerará Ectoplasma (para Comum) ou Ectoplasma / Super Ectoplasma (para Raro) quando os prefabs de itens forem criados.
    /// </summary>
    public void DropLoot()
    {
        if (ghostType == GhostType.Common)
        {
            // TODO: Instanciar prefab do item Ectoplasma na posição atual
            Debug.Log($"[GhostEnemy] Stub: Droppou Ectoplasma na posição {transform.position}");
        }
        else
        {
            // TODO: Instanciar prefab do item Ectoplasma ou Super Ectoplasma (chance de boost) na posição atual
            bool isSuperBoost = Random.value <= 0.3f; // 30% de chance de Super Ectoplasma
            string lootName = isSuperBoost ? "Super Ectoplasma (Boost)" : "Ectoplasma";
            Debug.Log($"[GhostEnemy] Stub: Droppou {lootName} na posição {transform.position}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = ghostType == GhostType.Common ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
