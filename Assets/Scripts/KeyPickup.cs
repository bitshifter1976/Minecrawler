using UnityEngine;

public sealed class KeyPickup : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
        transform.position = new Vector3(position.x, position.y, 0f);
    }
}