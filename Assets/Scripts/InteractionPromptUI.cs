using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Gerencia o ícone flutuante de interação no mundo.
/// Exibe a tecla/botão correto ("E" para Teclado/Mouse e "Y" para Controle/Gamepad)
/// e a descrição da ação abaixo do ícone (ex: "Ligar", "Desligar", "Equipar", "Coletar").
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto que exibe o ícone do botão ('R' ou 'Y')")]
    [SerializeField] private Text buttonText;

    [Tooltip("Texto posicionado abaixo do ícone que descreve a ação (ex: 'Ligar', 'Desligar')")]
    [SerializeField] private Text actionText;

    [Tooltip("Container principal do prompt para ativar/desativar")]
    [SerializeField] private GameObject promptContainer;

    [Header("Crafting UI")]
    [Tooltip("Container (ex: HorizontalLayoutGroup) onde os ícones dos ingredientes serão instanciados")]
    [SerializeField] private GameObject recipeContainer;
    
    [Tooltip("Prefab do ingrediente (deve conter o script RecipeIngredientUI)")]
    [SerializeField] private RecipeIngredientUI ingredientPrefab;

    [Tooltip("Mapeamento visual para os ícones de cada recurso")]
    [SerializeField] private ItemIcon[] itemIcons;

    [System.Serializable]
    public struct ItemIcon
    {
        public PlayerItems.ItemType type;
        public Sprite icon;
    }

    [Header("Player Input Reference")]
    [Tooltip("Referência ao PlayerInput para detectar mudança de esquema de controle")]
    [SerializeField] private PlayerInput playerInput;

    private InteractableObject currentTarget;
    private Camera mainCamera;

    public static InteractionPromptUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        mainCamera = Camera.main;
        HidePrompt();
    }

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (currentTarget == null)
        {
            if (promptContainer != null && promptContainer.activeSelf)
            {
                promptContainer.SetActive(false);
            }
            return;
        }

        // Atualizar posição flutuante no mundo
        transform.position = currentTarget.GetPromptWorldPosition();

        // Efeito Billboard: encarar a câmera
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        // Atualizar textos dinamicamente
        UpdatePromptContent();
    }

    /// <summary>
    /// Exibe o prompt de interação para o objeto alvo fornecido.
    /// </summary>
    public void ShowPrompt(InteractableObject target, PlayerInput inputRef = null)
    {
        if (inputRef != null) playerInput = inputRef;
        currentTarget = target;

        if (currentTarget != null)
        {
            if (promptContainer != null) promptContainer.SetActive(true);
            UpdatePromptContent();
        }
    }

    /// <summary>
    /// Oculta o prompt de interação.
    /// </summary>
    public void HidePrompt()
    {
        currentTarget = null;
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Atualiza o texto do botão ("E" ou "Y") e a descrição da ação, além da receita de crafting (se houver).
    /// </summary>
    private void UpdatePromptContent()
    {
        if (currentTarget == null) return;

        // Determinar letra do botão conforme controle ativo
        string buttonDisplay = GetButtonForCurrentControl();
        if (buttonText != null)
        {
            buttonText.text = buttonDisplay;
        }

        // Determinar texto da ação (ex: "Ligar", "Desligar", "Equipar")
        if (actionText != null)
        {
            actionText.text = currentTarget.GetActionText();
        }

        // --- Lógica de Receita de Crafting ---
        if (recipeContainer != null)
        {
            // Limpa os ícones antigos
            foreach (Transform child in recipeContainer.transform)
            {
                Destroy(child.gameObject);
            }
            recipeContainer.SetActive(false);

            CraftingStation crafting = currentTarget.GetComponent<CraftingStation>();
            if (crafting != null && ingredientPrefab != null)
            {
                var tier = crafting.GetCurrentTier();
                if (tier != null && tier.requirements.Count > 0)
                {
                    recipeContainer.SetActive(true);
                    foreach (var req in tier.requirements)
                    {
                        RecipeIngredientUI ing = Instantiate(ingredientPrefab, recipeContainer.transform);
                        ing.Setup(GetIconFor(req.itemType), req.amount);
                    }
                }
            }
        }
    }

    private Sprite GetIconFor(PlayerItems.ItemType type)
    {
        if (itemIcons == null) return null;
        foreach (var mapping in itemIcons)
        {
            if (mapping.type == type) return mapping.icon;
        }
        return null;
    }

    /// <summary>
    /// Retorna "Y" se estiver usando Gamepad/Controle, ou "E" para Teclado/Mouse.
    /// </summary>
    private string GetButtonForCurrentControl()
    {
        if (playerInput != null && playerInput.currentControlScheme == "Gamepad")
        {
            return "Y";
        }
        return "R";
    }
}
