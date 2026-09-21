using UnityEngine;

/// <summary>
/// Tipo de objeto destrutível no cenário.
/// Rock (Pedra): Destruída com a Picareta. Bloqueia os trilhos e causa dano à locomotiva se colidir.
/// Crate (Caixa): Destruída com o Machado. Espalhada pelo cenário e solta madeira como loot.
/// </summary>
public enum DestructibleType
{
    Rock,
    Crate
}

/// <summary>
/// Componente de objeto destrutível (Pedra e Caixa).
/// Define qual ferramenta destrói o objeto, a saúde atual e o stub de drop de loot.
/// Ao ser destruído com a ferramenta correta, anima uma quebra simples e instancia o loot.
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("Destructible Properties")]
    [Tooltip("Tipo do objeto destrutível (Pedra ou Caixa)")]
    [SerializeField] private DestructibleType objectType = DestructibleType.Crate;

    [Tooltip("Quantidade de golpes necessários para destruir o objeto")]
    [SerializeField] private int health = 3;

    [Header("Hit Feedback")]
    [Tooltip("Escala de agitação ao ser atingido")]
    [SerializeField] private float hitShakeMagnitude = 0.08f;

    [Tooltip("Duração da agitação ao ser atingido")]
    [SerializeField] private float hitShakeDuration = 0.12f;

    [Header("Loot")]
    [Tooltip("Prefab do item a ser droppado (Madeira)")]
    [SerializeField] private GameObject drop1;

    private int currentHealth;
    private Vector3 originalLocalScale;
    private bool isBeingDestroyed = false;

    public DestructibleType ObjectType => objectType;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = health;
        originalLocalScale = transform.localScale;
    }

    /// <summary>
    /// Tenta aplicar dano ao objeto com a ferramenta informada.
    /// Apenas a ferramenta correta causa dano.
    /// Pedra: somente Picareta.
    /// Caixa: somente Machado.
    /// </summary>
    public void TryHit(ToolType usedTool)
    {
        if (isBeingDestroyed) return;

        // Verificar se a ferramenta é a correta para este objeto
        bool isCorrectTool = (objectType == DestructibleType.Rock && usedTool == ToolType.Pickaxe)
                          || (objectType == DestructibleType.Crate && usedTool == ToolType.Axe);

        if (!isCorrectTool)
        {
            Debug.Log($"[DestructibleObject] Ferramenta errada! {objectType} exige {GetRequiredToolName()}.");
            return;
        }

        currentHealth--;
        Debug.Log($"[DestructibleObject] {objectType} atingido! Vida restante: {currentHealth}/{health}");

        // Feedback visual de golpe
        StartCoroutine(HitShakeRoutine());

        if (currentHealth <= 0)
        {
            Destroy();
        }
    }

    /// <summary>
    /// Destrói o objeto, aciona o drop de loot e remove da cena.
    /// </summary>
    public void Destroy()
    {
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;

        Debug.Log($"[DestructibleObject] {objectType} destruída!");
        DropLoot();

        // Pequena quebra visual: escalar para zero antes de destruir
        StartCoroutine(BreakAnimationAndDestroy());
    }

    /// <summary>
    /// Stub para o sistema de drop de loot.
    /// Pedra: dropa Pedrinhas.
    /// Caixa: dropa Madeira.
    /// </summary>
    public void DropLoot()
    {
        switch (objectType)
        {
            case DestructibleType.Rock:
                // TODO: Instanciar prefab do item Pedrinha na posição do objeto.
                Debug.Log($"[DestructibleObject] Stub: Droppou Pedrinha(s) em {transform.position}");
                break;
            case DestructibleType.Crate:
                // TODO: Instanciar prefab do item Madeira na posição do objeto.
                Debug.Log($"[DestructibleObject] Stub: Droppou Madeira em {transform.position}");
                Instantiate(drop1, transform.position, Quaternion.identity);
                break;
        }
    }

    /// <summary>
    /// Retorna o nome da ferramenta necessária para destruir este objeto.
    /// </summary>
    private string GetRequiredToolName()
    {
        return objectType == DestructibleType.Rock ? "Picareta" : "Machado";
    }

    /// <summary>
    /// Corrotina de agitação visual rápida ao receber um golpe.
    /// </summary>
    private System.Collections.IEnumerator HitShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < hitShakeDuration)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 60f) * hitShakeMagnitude * (1f - elapsed / hitShakeDuration);
            transform.localScale = originalLocalScale + new Vector3(shake, -shake * 0.5f, shake);
            yield return null;
        }
        transform.localScale = originalLocalScale;
    }

    /// <summary>
    /// Corrotina que anima a quebra do objeto (diminui para zero) e o destroi do jogo.
    /// </summary>
    private System.Collections.IEnumerator BreakAnimationAndDestroy()
    {
        float duration = 0.18f;
        float elapsed = 0f;
        Vector3 startScale = originalLocalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        UnityEngine.Object.Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = objectType == DestructibleType.Rock ? Color.gray : new Color(0.6f, 0.35f, 0.1f);
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
    }
}
