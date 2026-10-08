using System.Collections;
using UnityEngine;

/// <summary>
/// Controla a abertura de um portão composto por duas portas.
/// Uma porta gira no Y positivo e a outra no Y negativo, simulando portas duplas abrindo.
/// </summary>
public class GateController : MonoBehaviour
{
    [Header("Portas")]
    [Tooltip("Porta que gira no eixo Y positivo (abre para um lado)")]
    [SerializeField] private Transform doorPositive;

    [Tooltip("Porta que gira no eixo Y negativo (abre para o outro lado)")]
    [SerializeField] private Transform doorNegative;

    [Header("Configurações de Abertura")]
    [Tooltip("Ângulo total de abertura (em graus)")]
    [SerializeField] private float openAngle = 90f;

    [Tooltip("Duração da animação de abertura (em segundos)")]
    [SerializeField] private float openDuration = 1.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    /// <summary>
    /// Abre os portões com animação suave de rotação.
    /// </summary>
    public void OpenGate()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(AnimateOpen());
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;

        Quaternion startRotPos = doorPositive != null ? doorPositive.localRotation : Quaternion.identity;
        Quaternion startRotNeg = doorNegative != null ? doorNegative.localRotation : Quaternion.identity;

        Quaternion endRotPos = startRotPos * Quaternion.Euler(0f, openAngle, 0f);
        Quaternion endRotNeg = startRotNeg * Quaternion.Euler(0f, -openAngle, 0f);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);

            if (doorPositive != null)
                doorPositive.localRotation = Quaternion.Slerp(startRotPos, endRotPos, t);

            if (doorNegative != null)
                doorNegative.localRotation = Quaternion.Slerp(startRotNeg, endRotNeg, t);

            yield return null;
        }

        // Garante a rotação final
        if (doorPositive != null) doorPositive.localRotation = endRotPos;
        if (doorNegative != null) doorNegative.localRotation = endRotNeg;

        isOpen = true;
        isAnimating = false;

        Debug.Log("[GateController] Portão aberto!");
    }
}
