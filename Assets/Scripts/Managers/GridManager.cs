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

        float startX = -width * cellSize / 2f;
        float endX = width * cellSize / 2f;
        float startY = -height * cellSize / 2f;
        float endY = height * cellSize / 2f;

        // Draw vertical lines
        for (int x = 0; x <= width; x++)
        {
            float xPos = startX + (x * cellSize);
            Vector3 lineStart = transform.position + new Vector3(xPos, startY, 0);
            Vector3 lineEnd = transform.position + new Vector3(xPos, endY, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }

        // Draw horizontal lines
        for (int y = 0; y <= height; y++)
        {
            float yPos = startY + (y * cellSize);
            Vector3 lineStart = transform.position + new Vector3(startX, yPos, 0);
            Vector3 lineEnd = transform.position + new Vector3(endX, yPos, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float startX = transform.position.x - (width * cellSize / 2f);
        float startY = transform.position.y - (height * cellSize / 2f);

        int x = Mathf.FloorToInt((worldPos.x - startX) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - startY) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float startX = transform.position.x - (width * cellSize / 2f);
        float startY = transform.position.y - (height * cellSize / 2f);

        float x = startX + (gridPos.x * cellSize) + (cellSize / 2f);
        float y = startY + (gridPos.y * cellSize) + (cellSize / 2f);
        return new Vector3(x, y, 0f);
    }
}