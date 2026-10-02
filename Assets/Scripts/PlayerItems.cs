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

    // Representa visual do item corrente (cubo colorido)
    private GameObject currentItemVisual;

    // Cores por tipo de item para o visual no itemSocket
    private static readonly Dictionary<ItemType, Color> ItemColors = new Dictionary<ItemType, Color>
    {
        { ItemType.Wood,          new Color(0.55f, 0.27f, 0.07f) },
        { ItemType.Stone,         new Color(0.55f, 0.55f, 0.55f) },
        { ItemType.Iron,          new Color(0.72f, 0.72f, 0.80f) },
        { ItemType.Ectoplasm,     new Color(0.30f, 0.80f, 0.30f) },
        { ItemType.SuperEctoplasm,new Color(0.10f, 0.90f, 0.90f) },
    };

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
        // Scroll do mouse para trocar slot
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0)  ChangeSelectedSlot(-1);
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

        currentItemVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentItemVisual.name = "ItemVisual_" + type.ToString();
        currentItemVisual.transform.SetParent(itemSocket, false);
        currentItemVisual.transform.localPosition = Vector3.zero;
        currentItemVisual.transform.localRotation = Quaternion.identity;
        currentItemVisual.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Remove collider para não interferir no jogo
        var col = currentItemVisual.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        // Aplica cor por tipo
        Color color = Color.white;
        if (ItemColors.ContainsKey(type)) color = ItemColors[type];
        var renderer = currentItemVisual.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    /// <summary>
    /// Remove o visual do item atual do itemSocket.
    /// </summary>
    private void HideItemVisual()
    {
        if (currentItemVisual != null)
        {
            Object.Destroy(currentItemVisual);
            currentItemVisual = null;
        }
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
        switch (itemType)
        {
            case ItemType.Wood:
                // TODO: Adicione uso da madeira (abastecer locomotiva)
                break;
            case ItemType.Ectoplasm:
                // TODO: Adicione uso do ectoplasma (abastecer locomotiva)
                break;
            case ItemType.SuperEctoplasm:
                // TODO: Adicione uso do super ectoplasma
                break;
            case ItemType.Stone:
                // TODO: Adicione uso da pedra
                break;
            case ItemType.Iron:
                // TODO: Adicione uso do ferro (curar locomotiva)
                break;
        }
    }
}
