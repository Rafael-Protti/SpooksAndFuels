using System.Collections;
using UnityEngine;

/// <summary>
/// Tipo do fantasma inimigo:
/// Common: Ataca o jogador, 1 de vida, morre com ataque do jogador ou colisão com a locomotiva, velocidade média.
/// Rare: Ataca a locomotiva, 2 de vida, morre APENAS com ataque do jogador, velocidade rápida.
/// </summary>
public enum GhostType
{
    Common,
    Rare,
    Fragile,
    Giant,
    GiantMinion,
    GiantMiniature
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

    [Header("Giant Drops/Spawns")]
    [Tooltip("Prefab do minion gigante (spawnado ao morrer o gigante)")]
    [SerializeField] private GameObject giantMinionPrefab;
    [Tooltip("Prefab da miniatura gigante (spawnada ao morrer o minion gigante)")]
    [SerializeField] private GameObject giantMiniaturePrefab;

    public Transform drop1;
    public Transform drop2;
    public float recoilForce = 5;
    public float recoilDuration = 5;
    bool inRecoil = false;

    // Estado interno
    private int currentHealth;
    private float lastAttackTime = -999f;
    private Transform targetTransform;
    private LocomotiveController locomotiveTarget;
    private Animator animator;

    public GhostType Type => ghostType;
    public int CurrentHealth => currentHealth;
    int fortune;

    private void Awake()
    {
        // Configuração inicial padrão baseada no tipo

        currentHealth = maxHealth;
    }

    private void Start()
    {
        FindTarget();
        animator = transform.GetChild(0).GetComponent<Animator>();
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
        if (locomotiveTarget == null)
        {
            locomotiveTarget = Object.FindAnyObjectByType<LocomotiveController>();   
        }
        if (locomotiveTarget != null)
        {
            targetTransform = locomotiveTarget.transform;
        }

    }

    /// <summary>
    /// Executa o ataque de contato no alvo quando dentro do alcance.
    /// </summary>
    private void PerformContactAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        locomotiveTarget.TakeDamage(attackDamage);

        Recoil();
    }

    /// <summary>
    /// Aplica dano ao fantasma.
    /// Fantasmas Raros só recebem dano de ataques diretos do jogador.
    /// </summary>
    /// <param name="damageAmount">Quantidade de dano recebida</param>
    /// <param name="isPlayerAttack">Indica se o dano veio de um ataque do jogador</param>
    public void TakeDamage(int damageAmount, int fortune, bool isPlayerAttack = false)
    {
        Debug.Log("TakeDamage Ghost");
        // Fantasmas Raros imunes a dano que não venha do jogador (ex: atropelamento por locomotiva)
        if (ghostType == GhostType.Rare && !isPlayerAttack)
        {
            Debug.Log("[GhostEnemy] Fantasma Raro é imune à colisão com a locomotiva! Apenas o ataque do jogador causa dano.");
            return;
        }

        currentHealth -= damageAmount;
        animator.SetBool("takeHit", true);
        Debug.Log($"[GhostEnemy] {ghostType} recebeu {damageAmount} de dano. Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            this.fortune = fortune;
            Die();
        }
    }

    /// <summary>
    /// Trata a morte do fantasma, gerando o loot e destruindo o GameObject.
    /// </summary>
    public void Die()
    {
        animator.SetBool("isDying", true);
        attackDamage = 0;
        attackRadius = 0;
        moveSpeed = 0;
        Debug.Log($"[GhostEnemy] {ghostType} foi derrotado!");

        if (ghostType == GhostType.Giant && giantMinionPrefab != null)
        {
            SpawnChildren(giantMinionPrefab, 2);
        }
        else if (ghostType == GhostType.GiantMinion && giantMiniaturePrefab != null)
        {
            SpawnChildren(giantMiniaturePrefab, 2);
        }
    }

    private void SpawnChildren(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            Instantiate(prefab, transform.position + offset, transform.rotation);
        }
    }


    /// <summary>
    /// O que acontece ao fantasma morrer.
    /// </summary>
    public void OnDefeat()
    {
        DropLoot();
        Destroy(transform.gameObject);
    }

    /// <summary>
    /// Stub para o sistema de drop de loot ao morrer.
    /// Gerará Ectoplasma (para Comum) ou Ectoplasma / Super Ectoplasma (para Raro) quando os prefabs de itens forem criados.
    /// </summary>
    public void DropLoot()
    {
        Transform dropPrefab = (ghostType == GhostType.Common
            || ghostType == GhostType.GiantMiniature
            || ghostType == GhostType.GiantMinion
            || ghostType == GhostType.Fragile)
            ? drop1
            : drop2;

        if (dropPrefab == null) return;

        // Instancia um item por ponto de fortune, espalhados ao redor da posição de morte
        for (int i = 0; i < fortune; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
            Transform instanciated = Instantiate(dropPrefab, transform.position + offset, Quaternion.identity);
            instanciated.GetComponent<CollectableObject>().itemCount = 1;
        }
    }

    public void Recoil()
    {
        if (!inRecoil && targetTransform != null)
        {
            StartCoroutine(RotinaRecuo(targetTransform.transform.position));
        }
    }

    private IEnumerator RotinaRecuo(Vector3 posicaoAlvo)
    {
        inRecoil = true;

        // Calcula a direção oposta ao alvo
        Vector3 direcaoOposta = (transform.position - posicaoAlvo).normalized;

        // Posição atual e posição de destino do recuo
        Vector3 posicaoInicial = transform.position;
        Vector3 posicaoRecuada = posicaoInicial + (direcaoOposta * recoilForce);

        float tempoDecorrido = 0f;

        // Fase 1: O recuo rápido para trás
        while (tempoDecorrido < recoilDuration)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = tempoDecorrido / recoilDuration;

            posicaoRecuada = new Vector3(posicaoRecuada.x, transform.position.y, posicaoRecuada.z);

            // Usa Mathf.SmoothStep para um movimento fluido, mas rápido
            transform.position = Vector3.Lerp(posicaoInicial, posicaoRecuada, Mathf.SmoothStep(0f, 1f, progresso));

            yield return null;
        }

        // Garante que a posição final do recuo foi atingida
        transform.position = posicaoRecuada;

        // Libera para o seu código normal de perseguição voltar a assumir o controle total do transform
        inRecoil = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = ghostType == GhostType.Common ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
