using UnityEngine;

public sealed class BreakableObstacle : GridActor
{
    [SerializeField] private int hitPoints = 1;

    public bool Hit()
    {
        hitPoints--;
        return hitPoints <= 0;
    }
}
