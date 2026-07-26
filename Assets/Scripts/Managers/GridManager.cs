using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 16;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSize = 1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.8f, 0.3f); // Cyberpunk cyan tint

        for (int x = 0; x <= width; x++)
        {
            Vector3 startPos = new Vector3(x * cellSize, 0, 0);
            Vector3 endPos = new Vector3(x * cellSize, height * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 startPos = new Vector3(0, y * cellSize, 0);
            Vector3 endPos = new Vector3(width * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}