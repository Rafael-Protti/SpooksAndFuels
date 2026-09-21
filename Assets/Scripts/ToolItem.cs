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

    [Header("Equipped Offset")]
    [Tooltip("Posição local relativa ao soquete do jogador quando equipada")]
    [SerializeField] private Vector3 equippedLocalPosition = new Vector3(0.4f, 0.8f, 0.4f);

    [Tooltip("Rotação local relativa ao soquete do jogador quando equipada")]
    [SerializeField] private Vector3 equippedLocalRotation = new Vector3(0f, 0f, 0f);

    [Tooltip("Posição da ferramenta no vagão")]
    [SerializeField] private Transform vagonLocation;

    private bool isEquipped;
    private Collider toolCollider;
    private InteractableObject interactable;
    private Coroutine swingCoroutine;
    private Quaternion originalLocalRotation;

    public ToolType Type => toolType;
    public string ToolName => toolName;
    public bool IsEquipped => isEquipped;

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
        transform.position = vagonLocation.position;
        transform.SetParent(vagonLocation);
    }

    /// <summary>
    /// Equipa a ferramenta no soquete/mão do jogador.
    /// </summary>
    public void Equip(Transform socket)
    {
        isEquipped = true;

        transform.position = socket.transform.position;
        originalLocalRotation = transform.rotation;

        if (transform.parent != socket)
        {
            transform.SetParent(socket);
        }

        transform.localRotation = originalLocalRotation;
        if (toolCollider != null) toolCollider.enabled = false;
        if (interactable != null) interactable.enabled = false;

        Debug.Log($"[ToolItem] {toolName} equipada com sucesso no jogador.");
    }

    /// <summary>
    /// Solta a ferramenta no chão na posição informada.
    /// </summary>
    public void Drop(Vector3 dropPosition)
    {
        StopAllCoroutines();
        isEquipped = false;
        transform.SetParent(null);
        // transform.position = dropPosition;
        // transform.rotation = Quaternion.identity;

        transform.position = vagonLocation.position;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.SetParent(vagonLocation);

        if (toolCollider != null) toolCollider.enabled = true;
        if (interactable != null) interactable.enabled = true;

        // Debug.Log($"[ToolItem] {toolName} solta no chão na posição {dropPosition}.");

    }

    /// <summary>
    /// Executa uma animação procedural de swing (golpe de ataque).
    /// </summary>
    public void PlaySwingAnimation()
    {
        if (swingCoroutine != null)
        {
            StopCoroutine(swingCoroutine);
        }
        swingCoroutine = StartCoroutine(SwingRoutine());
    }

    /// <summary>
    /// Corrotina que rotaciona a ferramenta para frente e volta rapidamente simulando um golpe.
    /// </summary>
    private IEnumerator SwingRoutine()
    {
        float duration = 0.22f;
        float elapsed = 0f;

        Quaternion startRot = originalLocalRotation;
        Quaternion swingRot = startRot * Quaternion.Euler(65f, 0f, 0f); // Inclina 65 graus para frente

        // Fase de ida do golpe
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            transform.localRotation = Quaternion.Slerp(startRot, swingRot, t);
            yield return null;
        }

        // Fase de volta do golpe
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            transform.localRotation = Quaternion.Slerp(swingRot, startRot, t);
            yield return null;
        }

        transform.localRotation = startRot;
        swingCoroutine = null;
    }
}
