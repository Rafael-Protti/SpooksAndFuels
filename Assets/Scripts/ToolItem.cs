using System.Collections;
using UnityEngine;

/// <summary>
/// Tipo da ferramenta:
/// Sword (Espada): Permite atacar os fantasmas.
/// Pickaxe (Picareta): Utilizada para quebrar pedras.
/// Axe (Machado): Utilizado para quebrar caixas/barris.
/// </summary>
public enum ToolType
{
    Sword,
    Pickaxe,
    Axe
}

/// <summary>
/// Componente de Item/Ferramenta que pode ser coletado e equipado pelo jogador.
/// Possui física de chão, montagem no soquete do jogador e animação procedural de swing.
/// </summary>
public class ToolItem : MonoBehaviour
{
    [Header("Tool Properties")]
    [Tooltip("Tipo da ferramenta")]
    [SerializeField] private ToolType toolType = ToolType.Sword;

    [Tooltip("Nome de exibição da ferramenta")]
    [SerializeField] private string toolName = "Espada";

    private bool isEquipped;
    private Collider toolCollider;
    private Quaternion originalLocalRotation;

    public ToolType Type => toolType;
    public string ToolName => toolName;
    public bool IsEquipped => isEquipped;
    public int damage = 1;
    public float swingCooldown = 1.25f; // Tempo (segundos) entre ataques/quebras
    public int dropFortune = 1;
    public bool inInventory = false;

    /// <summary>Incrementa o dano da ferramenta (+1 por chamada).</summary>
    public void ApplyDamageUpgrade() => damage++;

    /// <summary>Reduz o cooldown de swing da ferramenta (-0.25s por chamada).</summary>
    public void ApplySwingSpeedUpgrade() => swingCooldown = Mathf.Max(0.25f, swingCooldown - 0.25f);

    /// <summary>Incrementa a fortuna de drop (+1 por chamada).</summary>
    public void ApplyDropFortuneUpgrade() => dropFortune++;

    private void Awake()
    {
        toolCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// Equipa a ferramenta no soquete/mão do jogador.
    /// </summary>
    public void Equip(Transform socket)
    {
        isEquipped = true;
        if (inInventory) return;

        // SetParent(socket, false) preserva a posição e rotação locais originais do prefab,
        // ignorando se a mão (socket) está rotacionada no meio de uma animação.
        transform.SetParent(socket, false);

        if (toolCollider != null) toolCollider.enabled = false;

        Debug.Log($"[ToolItem] {toolName} equipada com sucesso no jogador.");
        inInventory = true;
    }

    public void OnCollisionDetected(Collider other)
    {
        switch (toolType)
        {
            case ToolType.Sword:
                Attack(other);
                break;
            case ToolType.Pickaxe:
                Break(other);
                break;
            case ToolType.Axe:
                Break(other);
                break;
        }
    }

    void Attack(Collider target)
    {
        GhostEnemy ghost = target.gameObject.transform.GetComponent<GhostEnemy>();
        if (ghost == null) return;
        ghost.TakeDamage(damage, dropFortune, true);
        PlayerController.playerController.toolHitBox.ToggleBoxCollider(false);
    }

    void Break(Collider target)
    {
        DestructibleObject obj = target.gameObject.transform.GetComponent<DestructibleObject>();
        if (obj == null) return;
        obj.TryHit(toolType, dropFortune);
        PlayerController.playerController.toolHitBox.ToggleBoxCollider(false);
    }
}
