using UnityEngine;

public abstract class GridActor : MonoBehaviour
{
    public Vector2Int GridPosition { get; protected set; }

    public virtual void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
