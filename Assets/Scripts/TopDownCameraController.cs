using UnityEngine;

/// <summary>
/// Controlador de câmera top-down / isométrica que segue o jogador suavemente.
/// Permite configuração de offset, ângulo de visão e velocidade de suavização pelo Inspector.
/// </summary>
public class TopDownCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Transform do alvo (Jogador) a ser seguido pela câmera")]
    [SerializeField] private Transform target;

    [Header("Position & Offset")]
    [Tooltip("Deslocamento da câmera em relação ao alvo (X, Y, Z)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);

    [Tooltip("Tempo de suavização do movimento da câmera (SmoothDamp)")]
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("Ângulo de rotação fixo da câmera (Pitch, Yaw, Roll)")]
    [SerializeField] private Vector3 cameraRotation = new Vector3(55f, 0f, 0f);

    // Velocidade interna usada pelo SmoothDamp
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        // Aplica a rotação inicial configurada no Inspector
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Posiciona a câmera mantendo a rotação fixa e aplicando o offset suavemente
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

    /// <summary>
    /// Define programmaticamente o alvo da câmera.
    /// </summary>
    /// <param name="newTarget">Transform do novo alvo</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnValidate()
    {
        // Atualiza a rotação no Inspector em tempo de edição para fácil visualização
        transform.rotation = Quaternion.Euler(cameraRotation);
    }
}
