using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Tooltip("Posição para onde o jogador é teleportado ao entrar na locomotiva")]
    [SerializeField] private Transform entryPosition;

    [Tooltip("Posição para onde o jogador é teleportado ao sair da locomotiva")]
    [SerializeField] private Transform exitPosition;

    private int currentHealth;
    private float currentFuel;
    private bool playerAboard;

    // Propriedades públicas para UI e sistemas
    public int CurrentHealth => currentHealth;
    public int MaxHealth { get => maxHealth; private set => maxHealth = value; }
    public float CurrentFuel => currentFuel;
    public float MaxFuel { get => maxFuel; private set => maxFuel = value; }
    public bool IsEngineOn => isEngineOn;
    public float InteractionRadius => interactionRadius;
    public TrackPath TrackPathRef => trackPath;
    public float CurrentDistance => currentDistance;
    public bool IsPlayerAboard => playerAboard;

    public void ToggleBoarding(PlayerController player)
    {
        playerAboard = !playerAboard;
        if (playerAboard)
        {
            if (entryPosition != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = entryPosition.position;
                if (cc != null) cc.enabled = true;
            }
            StartEngine();
        }
        else
        {
            if (exitPosition != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = exitPosition.position;
                if (cc != null) cc.enabled = true;
            }
            StopEngine();
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        currentFuel = maxFuel;

        PlayerController player = GameObject.Find("Player").GetComponent<PlayerController>();

        // Configurar o componente InteractableObject
        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<InteractableObject>();
        }

        // Texto dinâmico: "Consertar" ao segurar Ferro, "Abastecer" para os demais itens,
        // e "Sair" ou "Entrar" quando não estiver segurando itens que interagem com ela.
        interactable.SetDynamicActionTextProvider(() =>
        {
            if (player == null) return playerAboard ? "Sair" : "Entrar";
            var slot = player.playerItems.inventory[player.playerItems.selectedSlot];
            
            if (!slot.isTool && slot.count > 0)
            {
                if (slot.itemType == PlayerItems.ItemType.Iron)
                    return "Consertar";
                if (slot.itemType == PlayerItems.ItemType.Ectoplasm || 
                    slot.itemType == PlayerItems.ItemType.SuperEctoplasm || 
                    slot.itemType == PlayerItems.ItemType.Wood)
                    return "Abastecer";
            }
            return playerAboard ? "Sair" : "Entrar";
        });

        // Visibilidade condicional: sempre visível agora, para permitir Entrar/Sair
        interactable.SetVisibilityCondition(() =>
        {
            return true;
        });
    }

    private void Start()
    {
        // Posicionar a locomotiva no início do trilho na inicialização
        UpdatePositionOnTrack();
    }

    private void Update()
    {
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
        currentDistance += moveSpeed * Time.deltaTime;

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
        if (!playerAboard) return;
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
    /// <param name="isSuperEctoplasm">Se verdadeiro, aplica um boost fixo na velocidade da locomotiva</param>
    public void Refuel(float fuelAmount, bool isSuperEctoplasm = false)
    {
        currentFuel = Mathf.Clamp(currentFuel + fuelAmount, 0f, maxFuel);
        Debug.Log($"[LocomotiveController] Locomotiva abastecida com +{fuelAmount}. Combustível atual: {currentFuel}/{maxFuel}");

        if (isSuperEctoplasm)
        {
            ApplyPermanentSpeedBoost(1.0f); // 1.0f a mais de velocidade
        }
    }

    /// <summary>
    /// Aplica um boost fixo e permanente de velocidade à locomotiva.
    /// </summary>
    public void ApplyPermanentSpeedBoost(float addedSpeed)
    {
        moveSpeed += addedSpeed;
        Debug.Log($"[LocomotiveController] Boost permanente ativado! Nova velocidade: {moveSpeed}.");
    }

    /// <summary>
    /// Aplica o upgrade da locomotiva conforme o tier do GDD:
    /// Tier 0 (Primeiro Craft): 2x velocidade
    /// Tier 1 (Upgrade 1): 2x combustível máximo
    /// Tier 2 (Upgrade 2): 2x vida máxima
    /// Tier 3 (Upgrade 3): 1.5x todos os atributos
    /// </summary>
    public void ApplyLocomotiveUpgrade(int tierIndex)
    {
        switch (tierIndex)
        {
            case 0: // Primeiro Craft: 2x velocidade
                moveSpeed *= 2f;
                Debug.Log($"[LocomotiveController] Upgrade 0: Velocidade 2x -> {moveSpeed}");
                break;
            case 1: // Upgrade 1: 2x combustível máximo
                float oldMaxFuel = maxFuel;
                maxFuel *= 2f;
                currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
                currentFuel = currentFuel * maxFuel / oldMaxFuel;
                Debug.Log($"[LocomotiveController] Upgrade 1: Combustível máximo 2x -> {maxFuel}");
                break;
            case 2: // Upgrade 2: 2x vida máxima
                int oldHealth = maxHealth;
                maxHealth *= 2;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                currentHealth = currentHealth * maxHealth / oldHealth;
                Debug.Log($"[LocomotiveController] Upgrade 2: Vida máxima 2x -> {maxHealth}");
                break;
            case 3: // Upgrade 3: 1.5x todos os atributos
                moveSpeed *= 1.5f;
                maxFuel *= 1.5f;
                currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
                maxHealth = Mathf.RoundToInt(maxHealth * 1.5f);
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                Debug.Log($"[LocomotiveController] Upgrade 3: 1.5x tudo. Speed={moveSpeed}, MaxFuel={maxFuel}, MaxHealth={maxHealth}");
                break;
        }
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
    /// Cura a locomotiva, restaurando vida sem ultrapassar o máximo.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"[LocomotiveController] Locomotiva curada em +{amount}. Saúde: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Função disparada ao colidir com um fantasma no trilho.
    /// Fantasmas comuns são eliminados imediatamente pela locomotiva; raros causam dano na locomotiva.
    /// </summary>
    //public void OnGhostCollision(GameObject ghost)
    //{
    //    if (ghost == null) return;
    //    GhostEnemy ghostEnemy = ghost.GetComponent<GhostEnemy>();
    //    if (ghostEnemy != null)
    //    {
    //        if (ghostEnemy.Type == GhostType.Common)
    //        {
    //            Debug.Log($"[LocomotiveController] Fantasma Comum atropelado pela locomotiva: {ghost.name}");
    //            ghostEnemy.TakeDamage(ghostEnemy.CurrentHealth, 1, false); // Morre ao colidir
    //        }
    //        else
    //        {
    //            Debug.Log($"[LocomotiveController] Fantasma Raro colidiu com a locomotiva!");
    //            TakeDamage(1); // Causa 1 de dano na locomotiva
    //            ghostEnemy.TakeDamage(0, 1, false); // Imune à colisão
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null)
        {
            TakeDamage(1);
            //Destroy(other.gameObject);
            moveSpeed *= 0.5f;
            StopEngine();
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Gate"))
        {
            //Destroy(other.gameObject);
            moveSpeed *= 0.5f;
        }

        // Player entra e sai via botão de interação agora (ToggleBoarding)

        //GhostEnemy ghost = other.gameObject.GetComponent<GhostEnemy>();
        //if (ghost != null)
        //{
        //    if (!isEngineOn) return;
        //    OnGhostCollision(ghost.gameObject);
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        DestructibleObject destructible = other.GetComponent<DestructibleObject>();
        if (destructible != null)
        {
            StopEngine();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Gate"))
        {
            StopEngine();
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
        SceneManager.LoadScene("GameOverWin");
        Debug.Log("[LocomotiveController] VITÓRIA! A locomotiva chegou com sucesso ao destino!");
        // TODO: Tela de vitória.
    }

    /// <summary>
    /// Trata a condição de derrota quando a vida da locomotiva chega a zero.
    /// </summary>
    private void TriggerDefeat()
    {
        StopEngine();
        SceneManager.LoadScene("GameOverLose");
        Debug.Log("[LocomotiveController] DERROTA! A locomotiva foi destruída!");
        // TODO: Tela de derrota.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
