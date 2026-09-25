using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador do jogador (Fantasma) utilizando o novo Input System da Unity.
/// Gerencia movimentação 3D, pulo, rotação, saúde e stubs de interação/ataque.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Saúde máxima do jogador")]
    [SerializeField] private int maxHealth = 10;

    [Header("Movement Settings")]
    [Tooltip("Velocidade de movimento do jogador")]
    [SerializeField] private float moveSpeed = 7.0f;

    [Tooltip("Velocidade de rotação do personagem para encarar a direção de movimento")]
    [SerializeField] private float rotationSpeed = 12.0f;

    [Tooltip("Força/Altura do pulo")]
    [SerializeField] private float jumpHeight = 2.0f;

    [Tooltip("Gravidade aplicada ao jogador")]
    [SerializeField] private float gravity = -20.0f;

    [Header("Ground Check")]
    [Tooltip("Transform para checagem de chão")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Raio da esfera de checagem de chão")]
    [SerializeField] private float groundDistance = 0.3f;

    [Tooltip("Camada considerada chão")]
    [SerializeField] private LayerMask groundMask;

    // Componentes internos
    private CharacterController characterController;
    private PlayerInput playerInput;

    // Ações de Input
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;

    // Variáveis de estado
    private Vector2 rawInputVector;
    private Vector3 velocity;
    private bool isGrounded;
    private int currentHealth;
    private Transform currentMovingPlatform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    // Propriedades públicas para acesso externo
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        currentHealth = maxHealth;

        // Configuração das ações do Input System
        if (playerInput != null && playerInput.actions != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
            interactAction = playerInput.actions["Interact"];
        }
    }

    private void OnEnable()
    {
        if (jumpAction != null) jumpAction.performed += OnJumpPerformed;
        if (attackAction != null) attackAction.performed += OnAttackPerformed;
        if (interactAction != null) interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (jumpAction != null) jumpAction.performed -= OnJumpPerformed;
        if (attackAction != null) attackAction.performed -= OnAttackPerformed;
        if (interactAction != null) interactAction.performed -= OnInteractPerformed;
    }

    private void Update()
    {
        CheckGroundStatus();
        HandleMovingPlatform();
        HandleMovement();
        ApplyGravity();
        CheckNearestInteractable();
    }

    /// <summary>
    /// Verifica se o jogador está encostando no chão.
    /// Acompanha a plataforma móvel (Locomotiva) se estiver sobre ela.
    /// </summary>
    private void CheckGroundStatus()
    {
        Transform hitPlatform = null;

        if (groundCheck != null)
        {
            Collider[] hits = Physics.OverlapSphere(groundCheck.position, groundDistance, groundMask);
            isGrounded = hits.Length > 0;
            if (isGrounded)
            {
                foreach (var hit in hits)
                {
                    LocomotiveController loco = hit.GetComponentInParent<LocomotiveController>();
                    if (loco != null)
                    {
                        hitPlatform = loco.transform;
                        break;
                    }
                }
            }
        }
        else
        {
            isGrounded = characterController.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Mantém o personagem colado ao chão
        }

        if (hitPlatform != currentMovingPlatform)
        {
            currentMovingPlatform = hitPlatform;
            transform.SetParent(currentMovingPlatform); // Null remove o parentesco
            if (currentMovingPlatform != null)
            {
                lastPlatformPosition = currentMovingPlatform.position;
                lastPlatformRotation = currentMovingPlatform.rotation;
            }
        }
    }

    /// <summary>
    /// Move o CharacterController acompanhando a movimentação e rotação da plataforma móvel (Locomotiva).
    /// </summary>
    private void HandleMovingPlatform()
    {
        if (currentMovingPlatform == null) return;

        Vector3 platformDeltaPos = currentMovingPlatform.position - lastPlatformPosition;
        Quaternion platformDeltaRot = currentMovingPlatform.rotation * Quaternion.Inverse(lastPlatformRotation);

        Vector3 pointRelativeToPlatform = transform.position - currentMovingPlatform.position;
        Vector3 rotatedPoint = platformDeltaRot * pointRelativeToPlatform;
        Vector3 rotationDeltaPos = rotatedPoint - pointRelativeToPlatform;

        Vector3 totalDeltaPos = platformDeltaPos; //+ rotationDeltaPos;

        if (totalDeltaPos.sqrMagnitude > 0.0000001f)
        {
            characterController.Move(totalDeltaPos);
        }

        if (platformDeltaRot != Quaternion.identity)
        {
            transform.rotation = platformDeltaRot * transform.rotation;
        }

        lastPlatformPosition = currentMovingPlatform.position;
        lastPlatformRotation = currentMovingPlatform.rotation;
    }

    /// <summary>
    /// Processa o movimento 3D com base nos inputs recebidos.
    /// </summary>
    private void HandleMovement()
    {
        if (moveAction != null)
        {
            rawInputVector = moveAction.ReadValue<Vector2>();
        }

        // Converte o input 2D para movimento 3D no plano XZ
        Vector3 moveDirection = new Vector3(rawInputVector.x, 0f, rawInputVector.y).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Calcula o ângulo de rotação para encarar a direção de movimento
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Rotação suave do personagem
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move o personagem
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Aplica a força de gravidade contínua.
    /// </summary>
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Callback executado ao pressionar a ação de pulo.
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    /// <summary>
    /// Callback executado ao pressionar o botão de ataque.
    /// </summary>
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        PerformAttack();
    }

    /// <summary>
    /// Callback executado ao pressionar o botão de interação.
    /// </summary>
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        PerformInteract();
    }

    [Header("Tool Settings")]
    [Tooltip("Soquete de transformação onde a ferramenta equipada é anexada (ex: mão/cabeça)")]
    [SerializeField] private Transform toolSocket;
    [SerializeField] private Transform itemSocket;

    [Header("Attack Settings")]
    [Tooltip("Raio de alcance do ataque do jogador")]
    [SerializeField] private float attackRange = 2.5f;

    private ToolItem currentEquippedTool;
    public ToolItem CurrentEquippedTool => currentEquippedTool;
    private CollectableObject currentEquippedItem;
    public CollectableObject CurrentEquippedItem => currentEquippedItem;

    /// <summary>
    /// Função executada ao atacar.
    /// Sem ferramenta equipada, o ataque não é permitido.
    /// Se a Espada estiver equipada, ataca os fantasmas próximos.
    /// Se a Picareta ou Machado estiverem equipados, executa suas funções de ação.
    /// </summary>
    public void PerformAttack()
    {
        if (currentEquippedTool == null)
        {
            Debug.Log("[PlayerController] Nenhuma ferramenta equipada! Para atacar fantasmas é necessário estar com a Espada equipada.");
            return;
        }

        // Executar a animação procedural de swing da ferramenta
        currentEquippedTool.PlaySwingAnimation();

        switch (currentEquippedTool.Type)
        {
            case ToolType.Sword:
                PerformSwordGhostAttack();
                break;
            case ToolType.Pickaxe:
                UsePickaxe();
                break;
            case ToolType.Axe:
                UseAxe();
                break;
        }
    }

    /// <summary>
    /// Executa o ataque de Espada atingindo os fantasmas no raio de alcance.
    /// </summary>
    private void PerformSwordGhostAttack()
    {
        Debug.Log("[PlayerController] Golpes com a Espada acionados contra os fantasmas!");

        Vector3 attackCenter = transform.position + transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);
        foreach (var col in hitColliders)
        {
            GhostEnemy ghost = col.GetComponentInParent<GhostEnemy>();
            if (ghost != null)
            {
                Debug.Log($"[PlayerController] Espada atingiu fantasma: {ghost.name}");
                ghost.TakeDamage(1, true); // true = Ataque direto do jogador
            }
        }
    }

    /// <summary>
    /// Função executada ao utilizar a Picareta.
    /// Detecta pedras destrutíveis no raio de alcance e aplica o golpe.
    /// </summary>
    public void UsePickaxe()
    {
        bool hitSomething = false;
        Vector3 attackCenter = transform.position + transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);
        foreach (var col in hitColliders)
        {
            DestructibleObject obj = col.GetComponentInParent<DestructibleObject>();
            if (obj != null && obj.ObjectType == DestructibleType.Rock)
            {
                obj.TryHit(ToolType.Pickaxe);
                hitSomething = true;
            }
        }
        if (!hitSomething)
        {
            Debug.Log("[PlayerController] Picareta: Nenhuma pedra no alcance.");
        }
    }

    /// <summary>
    /// Função executada ao utilizar o Machado.
    /// Detecta caixas destrutíveis no raio de alcance e aplica o golpe.
    /// </summary>
    public void UseAxe()
    {
        bool hitSomething = false;
        Vector3 attackCenter = transform.position + transform.forward * 1.0f;
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);
        foreach (var col in hitColliders)
        {
            DestructibleObject obj = col.GetComponentInParent<DestructibleObject>();
            if (obj != null && obj.ObjectType == DestructibleType.Crate)
            {
                obj.TryHit(ToolType.Axe);
                hitSomething = true;
            }
        }
        if (!hitSomething)
        {
            Debug.Log("[PlayerController] Machado: Nenhuma caixa no alcance.");
        }
    }

    private InteractableObject currentNearestInteractable;

    /// <summary>
    /// Procura o objeto interagível mais próximo dentro do raio de interação
    /// e exibe/oculta o prompt flutuante de interface.
    /// </summary>
    private void CheckNearestInteractable()
    {
        InteractableObject nearest = FindNearestInteractable();
        if (nearest != currentNearestInteractable)
        {
            currentNearestInteractable = nearest;
        }

        if (currentNearestInteractable != null)
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt(currentNearestInteractable, playerInput);
            }
        }
        else
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }

    /// <summary>
    /// Encontra o InteractableObject mais próximo dentro do raio de interação.
    /// </summary>
    public InteractableObject FindNearestInteractable()
    {
        InteractableObject[] interactables = Object.FindObjectsByType<InteractableObject>(FindObjectsInactive.Exclude);
        InteractableObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var obj in interactables)
        {
            if (obj == null || !obj.enabled || !obj.gameObject.activeInHierarchy) continue;
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance <= obj.InteractionRadius && distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }
        return closest;
    }

    /// <summary>
    /// Função executada ao interagir.
    /// Interage com o objeto interagível mais próximo (Locomotiva, Ferramentas, Itens).
    /// </summary>
    public void PerformInteract()
    {
        InteractableObject interactable = FindNearestInteractable();
        if (interactable != null)
        {
            // 1. Interação com a Locomotiva
            LocomotiveController locomotive = interactable.GetComponent<LocomotiveController>();
            if (locomotive != null)
            {
                if(currentEquippedItem != null)
                {
                    currentEquippedItem.AddCoal();
                    Destroy(currentEquippedItem.gameObject);
                    currentEquippedItem = null;
                    return;
                }

                locomotive.ToggleEngine();
                return;
            }

            // 2. Interação com Ferramentas (ToolItem)
            ToolItem tool = interactable.GetComponent<ToolItem>();
            if (tool != null)
            {
                EquipTool(tool);
                return;
            }

            CollectableObject item = interactable.GetComponent<CollectableObject>();
            if (item != null)
            {
                PickupItem(item);
                return;
            }

            // TODO: Interagir com outros itens do chão, baús ou caixas.
            Debug.Log($"[PlayerController] Interagiu com {interactable.gameObject.name}: {interactable.GetActionText()}");
            return;
        }

        Debug.Log("[PlayerController] Nenhuma interação disponível no alcance.");
    }

    public void PickupItem(CollectableObject item)
    {
        if (item == null) return;

        if(currentEquippedItem != null)
        {
            currentEquippedItem.Drop(item.transform);
        }

        currentEquippedItem = item;

        currentEquippedItem.transform.position = itemSocket.transform.position;
        currentEquippedItem.transform.SetParent(itemSocket);
        currentEquippedItem.transform.GetComponent<InteractableObject>().enabled = false;
    }

    /// <summary>
    /// Equipa a ferramenta informada no soquete do jogador.
    /// Se o jogador já possuir uma ferramenta equipada, solta a antiga no chão no local.
    /// </summary>
    public void EquipTool(ToolItem toolToEquip)
    {
        if (toolToEquip == null) return;

        // Se já tiver uma ferramenta equipada, soltá-la no chão primeiro
        if (currentEquippedTool != null)
        {
            Vector3 dropPos = transform.position + transform.forward * 0.8f + Vector3.up * 0.2f;
            currentEquippedTool.Drop(dropPos);
        }

        currentEquippedTool = toolToEquip;

        // Criar ou localizar o soquete para a ferramenta no jogador
        if (toolSocket == null)
        {
            Transform socketTransform = transform.Find("ToolSocket");
            if (socketTransform == null)
            {
                GameObject socketObj = new GameObject("ToolSocket");
                socketObj.transform.SetParent(transform, false);
                socketObj.transform.localPosition = new Vector3(0.35f, 0.7f, 0.4f);
                toolSocket = socketObj.transform;
            }
            else
            {
                toolSocket = socketTransform;
            }
        }

        currentEquippedTool.Equip(toolSocket);
    }

    /// <summary>
    /// Aplica dano ao jogador e reduz sua saúde.
    /// </summary>
    /// <param name="damageAmount">Quantidade de dano a aplicar</param>
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"[PlayerController] Jogador recebeu {damageAmount} de dano. Saúde atual: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Trata a morte do jogador quando a vida chega a zero.
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerController] O jogador foi derrotado!");
        // TODO: Desencadear tela de derrota / Game Over.
        SceneManager.LoadScene("GameOverLose");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
