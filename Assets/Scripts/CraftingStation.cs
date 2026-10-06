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
        // TODO: Implementar a lógica real de aplicação de cada upgrade nos respectivos sistemas
        switch (type)
        {
            case StationType.Locomotive:
                // Aplicar melhorias da locomotiva (velocidade, combustível, vida, etc)
                break;
            case StationType.Broom:
                // Aplicar melhorias da vassoura (ataque, velocidade)
                break;
            case StationType.Axe:
                // Aplicar melhorias do machado (tempo de quebra, drop)
                break;
            case StationType.Pickaxe:
                // Aplicar melhorias da picareta (tempo de quebra, drop)
                break;
        }
    }
}
