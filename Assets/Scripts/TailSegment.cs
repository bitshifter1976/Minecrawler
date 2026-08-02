using UnityEngine;

/// <summary>
/// Ein einzelner Lore-Wagen.
/// Nur der erste Wagen direkt hinter dem Miner erzeugt Fahrstaub.
/// </summary>
public sealed class TailSegment : GridActor
{
    private MinerDustTrail dustTrail;
    private bool positionInitialized;
    private bool dustEnabled;

    private void Awake()
    {
        dustTrail =
            GetComponent<MinerDustTrail>();

        if (dustTrail == null)
        {
            dustTrail =
                gameObject.AddComponent<MinerDustTrail>();
        }

        dustTrail.SetEmissionMultiplier(
            0.50f);
    }

    public void SetDustEnabled(
        bool enabled)
    {
        dustEnabled = enabled;

        if (dustTrail != null)
            dustTrail.enabled = enabled;
    }

    public override void SetGridPosition(
        Vector2Int position)
    {
        Vector2Int previousGridPosition =
            GridPosition;

        Vector3 previousWorldPosition =
            transform.position;

        base.SetGridPosition(position);

        // Beim Erzeugen eines neuen Wagens kein künstlicher Staubstoß.
        if (!positionInitialized)
        {
            positionInitialized = true;
            return;
        }

        if (!dustEnabled)
            return;

        Vector2Int movementDirection =
            position -
            previousGridPosition;

        if (movementDirection == Vector2Int.zero)
            return;

        movementDirection =
            new Vector2Int(
                Mathf.Clamp(
                    movementDirection.x,
                    -1,
                    1),
                Mathf.Clamp(
                    movementDirection.y,
                    -1,
                    1));

        dustTrail?.EmitMovementDust(
            previousWorldPosition,
            movementDirection);
    }
}
