using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Interface HUD na tela (Screen Overlay) do jogador.
/// Exibe vida, e inventário com slots de ferramentas e itens.
/// </summary>
public class PlayerHUDUI : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlotUI
    {
        public Image iconImage;
        public Image selectionFrame;
        public Text countText;
    }

    [Header("Player Reference")]
    [Tooltip("Referência ao controlador do jogador")]
    [SerializeField] private PlayerController playerController;

    //[Header("UI Components")]
    //[Tooltip("Barra de preenchimento da saúde do jogador")]
    //[SerializeField] private Image healthFillBar;

    //[Tooltip("Texto numérico com a vida atual")]
    //[SerializeField] private Text healthText;

    [Header("Inventory UI")]
    [Tooltip("Slots de inventário na interface — preenchidos automaticamente via AutoConnect")]
    [SerializeField] private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    [Tooltip("Textos que exibem o ícones dos botões ('Q e E' ou 'R1 ou L1')")]
    [SerializeField] private Text buttonTextNext;
    [SerializeField] private Text buttonTextPrevious;
    [Header("Player Input Reference")]
    [Tooltip("Referência ao PlayerInput para detectar mudança de esquema de controle")]
    [SerializeField] private PlayerInput playerInput;

    // Nomes dos slots na hierarquia do Canvas (mesma ordem do inventário em PlayerItems)
    private static readonly string[] SlotObjectNames = {
        "Slot_Sword", "Slot_Pickaxe", "Slot_Axe",
        "Slot_Wood", "Slot_Stone", "Slot_Iron", "Slot_Ecto", "Slot_SuperEcto"
    };

    private void Awake()
    {
        if (playerController == null)
            playerController = Object.FindAnyObjectByType<PlayerController>();

        // Conecta automaticamente os slots se a lista estiver vazia
        if (inventorySlots.Count == 0)
            AutoConnectSlots();
    }

    private void LateUpdate()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
            if (playerController == null) return;
        }

        playerInput = playerController.transform.gameObject.GetComponent<PlayerInput>();

        UpdateNextAndPreviousIcons();
        //UpdateHealthUI();
        UpdateInventoryUI();
    }

    /// <summary>
    /// Busca os GameObjects dos slots pelo nome dentro do InventoryPanel e preenche a lista.
    /// </summary>
    public void AutoConnectSlots()
    {
        inventorySlots.Clear();

        var inventoryPanel = transform.Find("InventoryPanel");
        if (inventoryPanel == null)
        {
            Debug.LogWarning("[PlayerHUDUI] InventoryPanel não encontrado no Canvas.");
            return;
        }

        foreach (var slotName in SlotObjectNames)
        {
            var slotTransform = inventoryPanel.Find(slotName);
            if (slotTransform == null)
            {
                Debug.LogWarning($"[PlayerHUDUI] Slot '{slotName}' não encontrado no InventoryPanel.");
                inventorySlots.Add(new InventorySlotUI());
                continue;
            }

            var slot = new InventorySlotUI
            {
                iconImage      = slotTransform.Find("Icon")?.GetComponent<Image>(),
                selectionFrame = slotTransform.Find("SelectionFrame")?.GetComponent<Image>(),
                countText      = slotTransform.Find("CountText")?.GetComponent<Text>()
            };
            inventorySlots.Add(slot);
        }

        Debug.Log($"[PlayerHUDUI] {inventorySlots.Count} slots conectados automaticamente.");
    }

    /// <summary>
    /// Atualiza os elementos visuais de vida do jogador.
    /// </summary>
    //private void UpdateHealthUI()
    //{
    //    int currentHealth = playerController.CurrentHealth;
    //    int maxHealth     = playerController.MaxHealth;

    //    if (healthFillBar != null && maxHealth > 0)
    //    {
    //        float fillRatio = (float)currentHealth / maxHealth;
    //        healthFillBar.fillAmount = Mathf.Clamp01(fillRatio);
    //    }

    //    if (healthText != null)
    //        healthText.text = $"{currentHealth} / {maxHealth}";
    //}

    /// <summary>
    /// Atualiza todos os slots visuais do inventário refletindo o estado de PlayerItems.
    /// </summary>
    private void UpdateInventoryUI()
    {
        PlayerItems items = playerController.GetComponent<PlayerItems>();
        if (items == null) return;

        // Auto-conecta se ainda não foi feito
        if (inventorySlots.Count == 0)
            AutoConnectSlots();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i >= items.inventory.Count) break;

            var logicSlot = items.inventory[i];
            var uiSlot    = inventorySlots[i];

            // Destaque do slot selecionado
            if (uiSlot.selectionFrame != null)
                uiSlot.selectionFrame.gameObject.SetActive(i == items.selectedSlot);

            if (logicSlot.isTool)
            {
                // Ferramenta: mostra cor cheia se equipada, apagada se vazia
                if (uiSlot.countText  != null) uiSlot.countText.text = "";
                if (uiSlot.iconImage  != null)
                    uiSlot.iconImage.color = logicSlot.toolInstance != null
                        ? new Color(0.8f, 0.6f, 1.0f, 1f)
                        : new Color(0.4f, 0.3f, 0.5f, 0.5f);
            }
            else
            {
                // Item: mostra contagem e esmaece se vazio
                if (uiSlot.countText  != null)
                    uiSlot.countText.text = logicSlot.count > 0 ? logicSlot.count.ToString() : "0";
                if (uiSlot.iconImage  != null)
                    uiSlot.iconImage.color = logicSlot.count > 0
                        ? new Color(0.6f, 0.9f, 0.6f, 1f)
                        : new Color(0.3f, 0.4f, 0.3f, 0.5f);
            }
        }
    }

    /// <summary>
    /// Atualiza o texto do botão ("E" ou "Y") e a descrição da ação.
    /// </summary>
    private void UpdateNextAndPreviousIcons()
    {

        // Determinar letra do botão conforme controle ativo
        string buttonDisplayNext = GetButtonForCurrentControl(true);
        if (buttonTextNext != null)
        {
            buttonTextNext.text = buttonDisplayNext;
        }
        string buttonDisplayPrevious = GetButtonForCurrentControl(false);
        if (buttonTextPrevious != null)
        {
            buttonTextPrevious.text = buttonDisplayPrevious;
        }
    }

    /// <summary>
    /// Retorna "Y" se estiver usando Gamepad/Controle, ou "E" para Teclado/Mouse.
    /// </summary>
    private string GetButtonForCurrentControl(bool next)
    {
        if (next)
        {
            if (playerInput != null && playerInput.currentControlScheme == "Gamepad")
            {
                return "R1";
            }
            return "E";
        }

        else 
        {
            if (playerInput != null && playerInput.currentControlScheme == "Gamepad")
            {
                return "L1";
            }
            return "Q";
        }
    }
}
