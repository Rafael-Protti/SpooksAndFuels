using UnityEngine;
using System.Collections.Generic;
using System.Text;

[RequireComponent(typeof(InteractableObject))]
public class CraftingStation : MonoBehaviour
{
    public enum StationType
    {
        Locomotive,
        Broom,
        Axe,
        Pickaxe
    }

    [System.Serializable]
    public struct ResourceRequirement
    {
        public PlayerItems.ItemType itemType;
        public int amount;
    }

    [System.Serializable]
    public class UpgradeTier
    {
        public string upgradeName;
        public List<ResourceRequirement> requirements;
    }

    [Header("Station Settings")]
    public StationType stationType;
    public List<UpgradeTier> upgrades = new List<UpgradeTier>();

    [Header("Tool Unlock")]
    [Tooltip("Prefab da ferramenta a ser entregue no primeiro upgrade (Deixe vazio para a locomotiva)")]
    public GameObject toolPrefab;

    private int currentTierIndex = 0;
    private InteractableObject interactableObject;
    private PlayerItems playerItems; // Referência ao inventário do jogador

    private void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        if (interactableObject != null)
        {
            // Vincula a geração de texto dinâmico para a UI do prompt flutuante
            interactableObject.SetDynamicActionTextProvider(GetCraftingText);
        }
    }
    
    private void Start()
    {
        // Tenta achar o playerItems caso não tenha sido atribuído
        if (PlayerController.playerController != null)
        {
            playerItems = PlayerController.playerController.playerItems;
        }
    }

    public UpgradeTier GetCurrentTier()
    {
        if (currentTierIndex >= upgrades.Count) return null;
        return upgrades[currentTierIndex];
    }

    private string GetCraftingText()
    {
        if (currentTierIndex >= upgrades.Count)
        {
            return $"{stationType} (Nível Máximo)";
        }

        return upgrades[currentTierIndex].upgradeName; // Apenas o nome do upgrade
    }

    /// <summary>
    /// Chamado quando o jogador interage com esta estação.
    /// Acionado pelo PlayerController.
    /// </summary>
    public void Interact()
    {
        if (currentTierIndex >= upgrades.Count) 
        {
            Debug.Log($"[{stationType}] Nível máximo atingido.");
            return;
        }

        if (playerItems == null)
        {
            if (PlayerController.playerController != null)
                playerItems = PlayerController.playerController.playerItems;
            
            if (playerItems == null) return;
        }

        UpgradeTier currentTier = upgrades[currentTierIndex];

        // Verificar se o jogador tem os recursos necessários
        bool canCraft = true;
        foreach (var req in currentTier.requirements)
        {
            if (playerItems.GetItemCount(req.itemType) < req.amount)
            {
                canCraft = false;
                break;
            }
        }

        if (canCraft)
        {
            // Descontar recursos do inventário
            foreach (var req in currentTier.requirements)
            {
                playerItems.AddItem(req.itemType, -req.amount);
            }

            // Aplicar o upgrade no jogo
            ApplyUpgrade(stationType, currentTierIndex);

            // Avançar para o próximo tier de upgrade
            currentTierIndex++;
            Debug.Log($"[{stationType}] {currentTier.upgradeName} realizado com sucesso!");
        }
        else
        {
            Debug.Log($"[{stationType}] Recursos insuficientes para {currentTier.upgradeName}.");
        }
    }

    private void ApplyUpgrade(StationType type, int tierIndex)
    {
        // Se for o primeiro upgrade de uma ferramenta, entrega para o jogador
        if (tierIndex == 0 && type != StationType.Locomotive && toolPrefab != null)
        {
            GameObject toolObj = Instantiate(toolPrefab);
            ToolItem toolItem = toolObj.GetComponent<ToolItem>();
            if (toolItem != null && playerItems != null)
            {
                playerItems.AddTool(toolItem);
            }
            // Primeiro craft de ferramenta não tem buff adicional – a ferramenta em si é o upgrade
            return;
        }

        switch (type)
        {
            case StationType.Locomotive:
                ApplyLocomotiveUpgrade(tierIndex);
                break;
            case StationType.Broom:
                ApplyToolUpgrade(ToolType.Sword, tierIndex);
                break;
            case StationType.Axe:
                ApplyToolUpgrade(ToolType.Axe, tierIndex);
                break;
            case StationType.Pickaxe:
                ApplyToolUpgrade(ToolType.Pickaxe, tierIndex);
                break;
        }
    }

    /// <summary>
    /// Aplica o upgrade da locomotiva via LocomotiveController.
    /// Tier 0: 2x velocidade | Tier 1: 2x combustível | Tier 2: 2x vida | Tier 3: 1.5x tudo
    /// </summary>
    private void ApplyLocomotiveUpgrade(int tierIndex)
    {
        LocomotiveController loco = Object.FindAnyObjectByType<LocomotiveController>();
        if (loco == null)
        {
            Debug.LogWarning("[CraftingStation] LocomotiveController não encontrado para aplicar upgrade.");
            return;
        }
        loco.ApplyLocomotiveUpgrade(tierIndex);
    }

    /// <summary>
    /// Aplica o upgrade em uma ferramenta do inventário do jogador.
    /// GDD:
    ///   Vassoura  - Upg1: +1 ataque | Upg2: -1 tempo/ataque | Upg3: +1 ataque &amp; -1 tempo/ataque
    ///   Picareta  - Upg1: -1 tempo/quebra | Upg2: +1 drop | Upg3: +1 drop &amp; -1 tempo/quebra
    ///   Machado   - Upg1: -1 tempo/quebra | Upg2: +1 drop | Upg3: +1 drop &amp; -1 tempo/quebra
    /// </summary>
    private void ApplyToolUpgrade(ToolType toolType, int tierIndex)
    {
        if (playerItems == null) return;

        // Localiza a instância da ferramenta no inventário
        ToolItem tool = null;
        foreach (var slot in playerItems.inventory)
        {
            if (slot.isTool && slot.toolInstance != null && slot.toolInstance.Type == toolType)
            {
                tool = slot.toolInstance;
                break;
            }
        }

        if (tool == null)
        {
            Debug.LogWarning($"[CraftingStation] Ferramenta do tipo {toolType} não encontrada no inventário.");
            return;
        }

        switch (toolType)
        {
            case ToolType.Sword: // Vassoura
                // Upg1: +1 ataque | Upg2: -1 tempo/ataque | Upg3: +1 ataque & -1 tempo/ataque
                if (tierIndex == 1) { tool.ApplyDamageUpgrade(); }
                else if (tierIndex == 2) { tool.ApplySwingSpeedUpgrade(); }
                else if (tierIndex == 3) { tool.ApplyDamageUpgrade(); tool.ApplySwingSpeedUpgrade(); }
                break;

            case ToolType.Pickaxe: // Picareta
            case ToolType.Axe:     // Machado
                // Upg1: -1 tempo/quebra | Upg2: +1 drop | Upg3: +1 drop & -1 tempo/quebra
                if (tierIndex == 1) { tool.ApplySwingSpeedUpgrade(); }
                else if (tierIndex == 2) { tool.ApplyDropFortuneUpgrade(); }
                else if (tierIndex == 3) { tool.ApplyDropFortuneUpgrade(); tool.ApplySwingSpeedUpgrade(); }
                break;
        }

        Debug.Log($"[CraftingStation] {toolType} Upgrade {tierIndex} aplicado! DMG={tool.damage}, Cooldown={tool.swingCooldown:F2}s, Fortune={tool.dropFortune}");
    }
}
