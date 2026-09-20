using System;
using UnityEngine;

/// <summary>
/// Componente reutilizável anexado a qualquer objeto interagível no jogo (Locomotiva, Itens, Ferramentas, Caixas).
/// Define o raio de interação, offset do prompt flutuante e o texto de ação dinâmico (ex: "Ligar", "Desligar", "Equipar", "Coletar").
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Nome base da ação (ex: Interagir, Equipar, Coletar)")]
    [SerializeField] private string defaultActionName = "Interagir";

    [Tooltip("Raio máximo para o jogador interagir com este objeto")]
    [SerializeField] private float interactionRadius = 3.5f;

    [Tooltip("Offset relativo à posição do objeto onde o ícone flutuante de interação será exibido")]
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    // Delegate para textos dinâmicos de ação (ex: alternar entre "Ligar" e "Desligar" na locomotiva)
    private Func<string> dynamicActionTextProvider;

    public float InteractionRadius => interactionRadius;
    public Vector3 PromptOffset => promptOffset;

    /// <summary>
    /// Configura um provedor de texto dinâmico para a ação de interação.
    /// </summary>
    public void SetDynamicActionTextProvider(Func<string> provider)
    {
        dynamicActionTextProvider = provider;
    }

    /// <summary>
    /// Retorna o texto atual da ação (ex: "Ligar", "Desligar", "Equipar").
    /// </summary>
    public string GetActionText()
    {
        if (dynamicActionTextProvider != null)
        {
            return dynamicActionTextProvider.Invoke();
        }
        return defaultActionName;
    }

    /// <summary>
    /// Retorna a posição em coordenadas do mundo onde o prompt flutuante deve ser exibido.
    /// </summary>
    public Vector3 GetPromptWorldPosition()
    {
        return transform.position + transform.TransformDirection(promptOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetPromptWorldPosition(), new Vector3(0.3f, 0.3f, 0.3f));
    }
}
