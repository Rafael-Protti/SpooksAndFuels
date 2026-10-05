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

    [Tooltip("Posição da ferramenta no vagão")]
    [SerializeField] private Transform vagonLocation;

    private bool isEquipped;
    private Collider toolCollider;
    private InteractableObject interactable;
    private Quaternion originalLocalRotation;

    public ToolType Type => toolType;
    public string ToolName => toolName;
    public bool IsEquipped => isEquipped;
    public int damage = 1;
    public int swingSpeed = 1;
    public int dropFortune = 1;
    public bool inInventory = false;

    private void Awake()
    {
        toolCollider = GetComponent<Collider>();
        interactable = GetComponent<InteractableObject>();

        if (interactable == null)
        {
            interactable = gameObject.AddComponent<InteractableObject>();
        }

        // Configurar a legenda de interação como "Equipar"
        interactable.SetDynamicActionTextProvider(() => "Equipar");
    }

    private void Start()
    {
        // Posiciona no vagão apenas se a referência estiver configurada
        if (vagonLocation != null)
        {
            transform.position = vagonLocation.position;
            transform.SetParent(vagonLocation);
        }
    }

    /// <summary>
    /// Equipa a ferramenta no soquete/mão do jogador.
    /// </summary>
    public void Equip(Transform socket)
    {
        isEquipped = true;
        if (inInventory) return;

        transform.position = socket.transform.position;
        originalLocalRotation = transform.rotation;

        if (transform.parent != socket)
        {
            transform.SetParent(socket);
        }

        transform.rotation = originalLocalRotation;
        if (toolCollider != null) toolCollider.enabled = false;
        if (interactable != null) interactable.enabled = false;

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
