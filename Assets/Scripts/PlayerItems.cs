using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItems : MonoBehaviour
{
    public enum ItemType
    {
        None,
        Wood,
        Ectoplasm,
        SuperEctoplasm,
        Stone,
        Iron
    }

    [System.Serializable]
    public class InventorySlot
    {
        public bool isTool;
        public ToolType toolType;
        public ToolItem toolInstance;
        public ItemType itemType;
        public int count;
    }

    public List<InventorySlot> inventory = new List<InventorySlot>();
    public int selectedSlot = 0;

    [Header("Sockets")]
    [Tooltip("Socket onde ferramentas são exibidas ao serem equipadas")]
    [SerializeField] private Transform toolSocket;

    [Tooltip("Socket onde itens selecionados são exibidos na mão do jogador")]
    [SerializeField] private Transform itemSocket;
    [Tooltip("Lista de itens coletáveis para serem instanciados")]
    [SerializeField] private List<Transform> pickupItems = new();

    // Representa visual do item corrente (cubo colorido)
    private Transform currentItemVisual;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        // Localiza sockets automaticamente se não configurados no Inspector
        if (toolSocket == null)
        {
            var t = transform.Find("ToolSocket");
            if (t != null) toolSocket = t;
        }

        if (itemSocket == null)
        {
            var t = transform.Find("ItemSocket");
            if (t != null) itemSocket = t;
        }

        // Inicializa os slots: 3 primeiros para ferramentas
        inventory.Add(new InventorySlot { isTool = true,  toolType = ToolType.Sword });
        inventory.Add(new InventorySlot { isTool = true,  toolType = ToolType.Pickaxe });
        inventory.Add(new InventorySlot { isTool = true,  toolType = ToolType.Axe });

        // Slots de itens consumíveis / recursos
        inventory.Add(new InventorySlot { isTool = false, itemType = ItemType.Wood });
        inventory.Add(new InventorySlot { isTool = false, itemType = ItemType.Stone });
        inventory.Add(new InventorySlot { isTool = false, itemType = ItemType.Iron });
        inventory.Add(new InventorySlot { isTool = false, itemType = ItemType.Ectoplasm });
        inventory.Add(new InventorySlot { isTool = false, itemType = ItemType.SuperEctoplasm });
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0) ChangeSelectedSlot(-1);
            else if (scroll < 0) ChangeSelectedSlot(1);
        }

        // Teclas numéricas 1-8
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetSelectedSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SetSelectedSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SetSelectedSlot(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SetSelectedSlot(3);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SetSelectedSlot(4);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) SetSelectedSlot(5);
            if (Keyboard.current.digit7Key.wasPressedThisFrame) SetSelectedSlot(6);
            if (Keyboard.current.digit8Key.wasPressedThisFrame) SetSelectedSlot(7);
        }
    }

    public void ChangeSelectedSlot(int direction)
    {
        int newSlot = selectedSlot + direction;
        if (newSlot < 0) newSlot = inventory.Count - 1;
        if (newSlot >= inventory.Count) newSlot = 0;
        SetSelectedSlot(newSlot);
    }

    public void SetSelectedSlot(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        // Desativa ferramenta anterior
        if (inventory[selectedSlot].isTool && inventory[selectedSlot].toolInstance != null)
            inventory[selectedSlot].toolInstance.gameObject.SetActive(false);

        selectedSlot = index;
        var current = inventory[selectedSlot];

        // Remove visual de item anterior
        HideItemVisual();

        if (current.isTool)
        {
            // Equipar ferramenta no toolSocket
            if (current.toolInstance != null)
            {
                current.toolInstance.gameObject.SetActive(true);
                playerController.EquipTool(current.toolInstance);
            }
            else
            {
                playerController.EquipTool(null);
            }
        }
        else
        {
            // Desequipa ferramenta
            playerController.EquipTool(null);

            // Exibe visual do item no itemSocket
            if (current.count > 0)
                ShowItemVisual(current.itemType);
        }
    }

    /// <summary>
    /// Cria um cubo colorido no itemSocket representando o item selecionado.
    /// </summary>
    private void ShowItemVisual(ItemType type)
    {
        if (itemSocket == null) return;

        switch (type)
        {
            case ItemType.Wood: currentItemVisual = pickupItems[0];
                break;
            case ItemType.Stone: currentItemVisual = pickupItems[1];
                break;
            case ItemType.Iron: currentItemVisual = pickupItems[2];
                break;
            case ItemType.Ectoplasm: currentItemVisual = pickupItems[3];
                break;
            case ItemType.SuperEctoplasm: currentItemVisual = pickupItems[4];
                break;
        }

        Transform instanciated = Instantiate(currentItemVisual);
        instanciated.position = itemSocket.position;
        instanciated.rotation = itemSocket.rotation;
        instanciated.transform.SetParent(itemSocket);
    }

    /// <summary>
    /// Remove o visual do item atual do itemSocket.
    /// </summary>
    private void HideItemVisual()
    {
        if (currentItemVisual != null)
        {
            Destroy(itemSocket.transform.GetChild(0).gameObject);
        }

        currentItemVisual = null;
    }

    public void AddTool(ToolItem tool)
    {
        int slotIndex = 0;
        if (tool.Type == ToolType.Sword)   slotIndex = 0;
        else if (tool.Type == ToolType.Pickaxe) slotIndex = 1;
        else if (tool.Type == ToolType.Axe)     slotIndex = 2;

        inventory[slotIndex].toolInstance = tool;

        if (selectedSlot == slotIndex)
            SetSelectedSlot(selectedSlot);
        else
            tool.gameObject.SetActive(false);
    }

    public void AddItem(ItemType type, int amount)
    {
        foreach (var slot in inventory)
        {
            if (!slot.isTool && slot.itemType == type)
            {
                slot.count += amount;
                if (slot.count < 0) slot.count = 0;

                // Atualiza visual se o slot selecionado for este
                if (inventory[selectedSlot] == slot)
                {
                    HideItemVisual();
                    if (slot.count > 0) ShowItemVisual(type);
                }
                break;
            }
        }
    }

    public int GetItemCount(ItemType type)
    {
        foreach (var slot in inventory)
        {
            if (!slot.isTool && slot.itemType == type)
                return slot.count;
        }
        return 0;
    }

    public void UseSelectedItem()
    {
        InventorySlot current = inventory[selectedSlot];

        if (current.isTool)
        {
            if (current.toolInstance != null)
                playerController.PerformAttack();
        }
        else
        {
            if (current.count > 0)
            {
                UseItem(current.itemType);
                // Nota: o consumo é feito dentro de UseItem quando implementado
            }
        }
    }

    private void UseItem(ItemType itemType)
    {
        LocomotiveController loco = Object.FindAnyObjectByType<LocomotiveController>();

        switch (itemType)
        {
            case ItemType.Wood:
                // TODO: Adicione uso da madeira (abastecer locomotiva)
                break;
            case ItemType.Ectoplasm:
                // TODO: Adicione uso do ectoplasma (abastecer locomotiva)
                break;
            case ItemType.SuperEctoplasm:
                if (loco != null)
                {
                    loco.Refuel(10f, true); // Abastece e aplica boost fixo
                    AddItem(itemType, -1);  // Desconta 1 item do inventário
                }
                break;
            case ItemType.Stone:
                // TODO: Adicione uso da pedra
                break;
            case ItemType.Iron:
                if (loco != null)
                {
                    loco.Heal(1);
                    AddItem(itemType, -1);
                }
                break;
        }
    }
}
