using UnityEngine;

/// <summary>
/// Gerencia a locomotiva a vapor, sua saúde, combustível, movimento linear ao longo do trilho
/// e interações (Ligar, Parar, Abastecer, Dano).
/// </summary>
public class LocomotiveController : MonoBehaviour
{
    [Header("Track & Movement Settings")]
    [Tooltip("Referência ao caminho do trilho que a locomotiva deve seguir")]
    [SerializeField] private TrackPath trackPath;

    [Tooltip("Velocidade normal de movimento da locomotiva")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("Distância atual percorrida ao longo do trilho")]
    [SerializeField] private float currentDistance = 0f;

    [Header("Engine State")]
    [Tooltip("Indica se o motor da locomotiva está ligado")]
    [SerializeField] private bool isEngineOn = false;

    [Header("Health & Fuel")]
    [Tooltip("Saúde máxima da locomotiva (padrão GDD: 5)")]
    [SerializeField] private int maxHealth = 5;

    [Tooltip("Combustível máximo")]
    [SerializeField] private float maxFuel = 100.0f;

    [Tooltip("Taxa de consumo de combustível por segundo quando em movimento")]
    [SerializeField] private float fuelConsumptionRate = 2.0f;

    [Header("Interaction Settings")]
    [Tooltip("Distância máxima para o jogador interagir com a locomotiva")]
    [SerializeField] private float interactionRadius = 4.0f;

    // Estado interno
    private int currentHealth;
    private float currentFuel;
    private bool isSpecialSpeedActive;
    private float specialSpeedMultiplier = 1.0f;
    private float specialSpeedTimer = 0.0f;

    // Propriedades públicas para UI e sistemas
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public bool IsEngineOn => isEngineOn;
    public float InteractionRadius => interactionRadius;
    public TrackPath TrackPathRef => trackPath;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentFuel = maxFuel;

        // Configurar o componente InteractableObject para prover o texto dinâmico ("Ligar" / "Desligar")
        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<InteractableObject>();
        }
        interactable.SetDynamicActionTextProvider(() => isEngineOn ? "Desligar" : "Ligar");
    }

    private void Start()
    {
        // Posicionar a locomotiva no início do trilho na inicialização
        UpdatePositionOnTrack();
    }

    private void Update()
    {
        HandleSpeedBoostTimer();
        HandleLocomotiveMovement();
    }

    /// <summary>
    /// Processa a movimentação e consumo de combustível da locomotiva.
    /// </summary>
    private void HandleLocomotiveMovement()
    {
        if (!isEngineOn || currentFuel <= 0f || trackPath == null) return;

        // Consumir combustível
        currentFuel -= fuelConsumptionRate * Time.deltaTime;
        if (currentFuel <= 0f)
        {
            currentFuel = 0f;
            StopEngine();
            Debug.Log("[LocomotiveController] Combustível esgotado! A locomotiva parou.");
            return;
        }

        // Calcular nova distância e atualizar posição
        float effectiveSpeed = moveSpeed * (isSpecialSpeedActive ? specialSpeedMultiplier : 1.0f);
        currentDistance += effectiveSpeed * Time.deltaTime;

        trackPath.GetPositionAndRotationAtDistance(currentDistance, out Vector3 nextPos, out Quaternion nextRot, out bool isAtEnd);

        nextPos.y = transform.position.y;
        transform.position = nextPos;
        transform.rotation = nextRot;

        if (isAtEnd)
        {
            TriggerVictory();
        }
    }

    /// <summary>
    /// Gerencia o temporizador do efeito de velocidade especial (Super Ectoplasma).
    /// </summary>
    private void HandleSpeedBoostTimer()
    {
        if (isSpecialSpeedActive)
        {
            specialSpeedTimer -= Time.deltaTime;
            if (specialSpeedTimer <= 0f)
            {
                isSpecialSpeedActive = false;
                specialSpeedMultiplier = 1.0f;
                Debug.Log("[LocomotiveController] Efeito de velocidade extra expirou.");
            }
        }
    }

    /// <summary>
    /// Alterna entre ligar e parar a locomotiva.
    /// </summary>
    public void ToggleEngine()
    {
        if (isEngineOn)
        {
            StopEngine();
        }
        else
        {
            StartEngine();
        }
    }

    /// <summary>
    /// Liga a locomotiva.
    /// </summary>
    public void StartEngine()
    {
        if (currentFuel <= 0f)
        {
            Debug.Log("[LocomotiveController] Não é possível ligar: Sem combustível!");
            return;
        }

        isEngineOn = true;
        Debug.Log("[LocomotiveController] Locomotiva LIGADA.");
    }

    /// <summary>
    /// Para a locomotiva.
    /// </summary>
    public void StopEngine()
    {
        isEngineOn = false;
        Debug.Log("[LocomotiveController] Locomotiva PARADA.");
    }

    /// <summary>
    /// Função para abastecer a locomotiva com combustível (Madeira, Ectoplasma ou Super Ectoplasma).
    /// </summary>
    /// <param name="fuelAmount">Quantidade de combustível adicionada</param>
    /// <param name="isSuperEctoplasm">Se verdadeiro, ativa boost temporário de velocidade</param>
    public void Refuel(float fuelAmount, bool isSuperEctoplasm = false)
    {
        currentFuel = Mathf.Clamp(currentFuel + fuelAmount, 0f, maxFuel);
        Debug.Log($"[LocomotiveController] Locomotiva abastecida com +{fuelAmount}. Combustível atual: {currentFuel}/{maxFuel}");

        if (isSuperEctoplasm)
        {
            ApplySpeedBoost(1.5f, 5.0f); // 50% mais rápido por 5 segundos
        }
    }

    /// <summary>
    /// Aplica um boost temporário de velocidade à locomotiva.
    /// </summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        isSpecialSpeedActive = true;
        specialSpeedMultiplier = multiplier;
        specialSpeedTimer = duration;
        Debug.Log($"[LocomotiveController] Boost de velocidade ativado! multiplicador: {multiplier}x por {duration}s.");
    }

    /// <summary>
    /// Aplica dano à locomotiva por ataque de fantasma raro ou colisão com rocha.
    /// </summary>
    /// <param name="damageAmount">Quantidade de dano recebido</param>
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"[LocomotiveController] Locomotiva recebeu {damageAmount} de dano! Saúde restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            TriggerDefeat();
        }
    }

    /// <summary>
    /// Função disparada ao colidir com um fantasma no trilho.
    /// Fantasmas comuns são eliminados imediatamente pela locomotiva; raros causam dano na locomotiva.
    /// </summary>
    public void OnGhostCollision(GameObject ghost)
    {
        if (ghost == null) return;
        GhostEnemy ghostEnemy = ghost.GetComponent<GhostEnemy>();
        if (ghostEnemy != null)
        {
            if (ghostEnemy.Type == GhostType.Common)
            {
                Debug.Log($"[LocomotiveController] Fantasma Comum atropelado pela locomotiva: {ghost.name}");
                ghostEnemy.TakeDamage(ghostEnemy.CurrentHealth, false); // Morre ao colidir
            }
            else
            {
                Debug.Log($"[LocomotiveController] Fantasma Raro colidiu com a locomotiva!");
                TakeDamage(1); // Causa 1 de dano na locomotiva
                ghostEnemy.TakeDamage(0, false); // Imune à colisão
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GhostEnemy ghost = other.GetComponent<GhostEnemy>();
        if (ghost != null)
        {
            OnGhostCollision(ghost.gameObject);
        }
    }

    /// <summary>
    /// Atualiza a posição da locomotiva no trilho com base no valor atual de currentDistance.
    /// </summary>
    public void UpdatePositionOnTrack()
    {
        if (trackPath != null)
        {
            trackPath.GetPositionAndRotationAtDistance(currentDistance, out Vector3 pos, out Quaternion rot, out _);
            pos = new Vector3(pos.x, transform.position.y, pos.z);
            transform.position = pos;
            transform.rotation = rot;
        }
    }

    /// <summary>
    /// Trata a condição de vitória ao chegar ao destino final do trilho.
    /// </summary>
    private void TriggerVictory()
    {
        StopEngine();
        Debug.Log("[LocomotiveController] VITÓRIA! A locomotiva chegou com sucesso ao destino!");
        // TODO: Tela de vitória.
    }

    /// <summary>
    /// Trata a condição de derrota quando a vida da locomotiva chega a zero.
    /// </summary>
    private void TriggerDefeat()
    {
        StopEngine();
        Debug.Log("[LocomotiveController] DERROTA! A locomotiva foi destruída!");
        // TODO: Tela de derrota.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
