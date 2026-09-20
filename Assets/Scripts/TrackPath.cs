using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que define o caminho de trilhos da locomotiva através de waypoints.
/// Desenha gizmos no Editor para fácil visualização e edição manual.
/// </summary>
public class TrackPath : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Lista de pontos que compõem o caminho do trilho")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Gizmo Settings")]
    [Tooltip("Cor da linha do trilho no Editor")]
    [SerializeField] private Color pathColor = Color.cyan;

    [Tooltip("Raio das esferas dos waypoints")]
    [SerializeField] private float gizmoRadius = 0.5f;

    public int WaypointCount => waypoints.Count;
    public List<Transform> Waypoints => waypoints;

    /// <summary>
    /// Retorna a posição do waypoint no índice especificado.
    /// </summary>
    public Vector3 GetPoint(int index)
    {
        if (waypoints == null || waypoints.Count == 0) return transform.position;
        index = Mathf.Clamp(index, 0, waypoints.Count - 1);
        return waypoints[index] != null ? waypoints[index].position : transform.position;
    }

    /// <summary>
    /// Retorna a distância total acumulada do caminho.
    /// </summary>
    public float GetTotalDistance()
    {
        float total = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                total += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }
        }
        return total;
    }

    /// <summary>
    /// Calcula a posição e rotação ao longo do caminho com base na distância percorrida.
    /// </summary>
    public void GetPositionAndRotationAtDistance(float distance, out Vector3 position, out Quaternion rotation, out bool isAtEnd)
    {
        position = transform.position;
        rotation = Quaternion.identity;
        isAtEnd = false;

        if (waypoints == null || waypoints.Count < 2) return;

        float accumulatedDistance = 0f;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;

            Vector3 startPt = waypoints[i].position;
            Vector3 endPt = waypoints[i + 1].position;
            float segmentLength = Vector3.Distance(startPt, endPt);

            if (distance <= accumulatedDistance + segmentLength)
            {
                float segmentT = (distance - accumulatedDistance) / segmentLength;
                position = Vector3.Lerp(startPt, endPt, segmentT);

                Vector3 direction = (endPt - startPt).normalized;
                if (direction != Vector3.zero)
                {
                    rotation = Quaternion.LookRotation(direction);
                }

                return;
            }

            accumulatedDistance += segmentLength;
        }

        // Se a distância for maior ou igual ao comprimento total, fica no último ponto
        int lastIndex = waypoints.Count - 1;
        position = waypoints[lastIndex].position;
        if (waypoints.Count >= 2 && waypoints[lastIndex - 1] != null)
        {
            Vector3 finalDir = (waypoints[lastIndex].position - waypoints[lastIndex - 1].position).normalized;
            if (finalDir != Vector3.zero) rotation = Quaternion.LookRotation(finalDir);
        }
        isAtEnd = true;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].position, gizmoRadius);

            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
