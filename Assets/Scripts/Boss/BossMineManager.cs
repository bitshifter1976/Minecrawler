using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reliable boss mine-laying controller.
///
/// Mines can be created in two ways:
/// 1. automatically on a field the boss has just vacated;
/// 2. immediately when a mine-based attack pattern calls TryDropMineNearBoss().
///
/// No HUD changes are required.
/// </summary>
[DisallowMultipleComponent]
public sealed class BossMineManager : MonoBehaviour
{
    private readonly List<BossMine> activeMines =
        new();

    private LevelBoss owner;
    private GameDifficulty difficulty;

    private float dropIntervalMin;
    private float dropIntervalMax;
    private float nextDropTime;
    private int maximumMines;

    private bool initialized;
    private bool hasVacatedPosition;
    private Vector2Int lastVacatedPosition;
    private Vector2Int lastMovementDirection;

    public void Initialize(
        LevelBoss boss,
        GameDifficulty gameDifficulty)
    {
        owner = boss;
        difficulty = gameDifficulty;

        switch (difficulty)
        {
            case GameDifficulty.Easy:
                dropIntervalMin = 8f;
                dropIntervalMax = 10f;
                maximumMines = 3;
                break;

            case GameDifficulty.Hardcore:
                dropIntervalMin = 3f;
                dropIntervalMax = 4f;
                maximumMines = 8;
                break;

            case GameDifficulty.Normal:
            default:
                dropIntervalMin = 5f;
                dropIntervalMax = 6f;
                maximumMines = 5;
                break;
        }

        initialized = true;

        // The first mine appears sooner so the feature is clearly visible.
        nextDropTime =
            Time.time +
            Random.Range(
                1.5f,
                2.4f);
    }

    /// <summary>
    /// Called after the boss finishes moving to a new grid field.
    /// </summary>
    public void NotifyBossMoved(
        Vector2Int vacatedPosition,
        Vector2Int movementDirection)
    {
        lastVacatedPosition =
            vacatedPosition;

        lastMovementDirection =
            movementDirection;

        hasVacatedPosition =
            true;

        if (!CanCreateMines())
            return;

        if (Time.time < nextDropTime)
            return;

        if (TryCreateMineAt(
                vacatedPosition))
        {
            ScheduleNextDrop();
            return;
        }

        // Do not wait another full interval if this particular field was invalid.
        nextDropTime =
            Time.time + 0.75f;
    }

    /// <summary>
    /// Used by MineLayer, CrossAndMines and MineKing attack patterns.
    /// Searches for a valid nearby field and drops a mine immediately.
    /// </summary>
    public bool TryDropMineNearBoss()
    {
        if (!CanCreateMines())
            return false;

        MineBoard board =
            MineGameManager.Instance?
                .Board;

        if (board == null)
            return false;

        List<Vector2Int> candidates =
            new();

        if (hasVacatedPosition)
            candidates.Add(lastVacatedPosition);

        Vector2Int behind =
            owner.GridPosition -
            lastMovementDirection;

        if (lastMovementDirection !=
            Vector2Int.zero)
        {
            candidates.Add(behind);
        }

        candidates.Add(
            owner.GridPosition +
            Vector2Int.up);

        candidates.Add(
            owner.GridPosition +
            Vector2Int.right);

        candidates.Add(
            owner.GridPosition +
            Vector2Int.down);

        candidates.Add(
            owner.GridPosition +
            Vector2Int.left);

        for (int index = 0;
             index < candidates.Count;
             index++)
        {
            Vector2Int candidate =
                candidates[index];

            if (!TryCreateMineAt(candidate))
                continue;

            ScheduleNextDrop();
            return true;
        }

        return false;
    }

    public bool ContainsMineAt(
        Vector2Int position)
    {
        RemoveMissingReferences();

        foreach (BossMine mine in
                 activeMines)
        {
            if (mine != null &&
                mine.GridPosition ==
                position)
            {
                return true;
            }
        }

        return false;
    }

    public void NotifyMineRemoved(
        BossMine mine)
    {
        if (mine != null)
            activeMines.Remove(mine);
    }

    public void ClearAll(
        bool explode)
    {
        BossMine[] snapshot =
            activeMines.ToArray();

        activeMines.Clear();

        foreach (BossMine mine in
                 snapshot)
        {
            if (mine == null)
                continue;

            if (explode)
                mine.ExplodeWithoutGameOver();
            else
                mine.RemoveSilently();
        }
    }

    private bool TryCreateMineAt(
        Vector2Int position)
    {
        MineGameManager game =
            MineGameManager.Instance;

        MineBoard board =
            game?.Board;

        if (game == null ||
            !game.IsPlaying ||
            board == null ||
            board.Miner == null ||
            owner == null ||
            owner.IsDestroyed)
        {
            return false;
        }

        if (!IsValidMinePosition(
                board,
                position))
        {
            return false;
        }

        RemoveMissingReferences();

        if (activeMines.Count >=
            maximumMines)
        {
            BossMine oldest =
                activeMines[0];

            activeMines.RemoveAt(0);

            if (oldest != null)
                oldest.RemoveSilently();
        }

        GameObject mineObject =
            new("Boss Mine");

        mineObject.transform.position =
            owner.transform.position;

        mineObject.transform.SetParent(
            owner.transform.parent);

        BossMine mine =
            mineObject.AddComponent<BossMine>();

        mine.Initialize(
            position,
            owner.transform.position,
            0.52f,
            this);

        activeMines.Add(mine);

        return true;
    }

    private bool IsValidMinePosition(
        MineBoard board,
        Vector2Int position)
    {
        if (!board.IsInside(position) ||
            board.IsWall(position) ||
            board.IsExit(position) ||
            board.GetObstacle(position) != null ||
            board.GetCoal(position) != null ||
            board.GetBoss(position) != null ||
            board.Tail.Contains(position) ||
            board.Miner.GridPosition ==
            position ||
            ContainsMineAt(position))
        {
            return false;
        }

        if (board.SpawnedKey != null &&
            board.SpawnedKey.GridPosition ==
            position)
        {
            return false;
        }

        // Keep mine placement fair: never create one directly beside the miner.
        int minerDistance =
            Mathf.Abs(
                board.Miner.GridPosition.x -
                position.x) +
            Mathf.Abs(
                board.Miner.GridPosition.y -
                position.y);

        return minerDistance > 1;
    }

    private bool CanCreateMines()
    {
        MineGameManager game =
            MineGameManager.Instance;

        return
            initialized &&
            owner != null &&
            !owner.IsDestroyed &&
            game != null &&
            game.IsPlaying;
    }

    private void ScheduleNextDrop()
    {
        nextDropTime =
            Time.time +
            Random.Range(
                dropIntervalMin,
                dropIntervalMax);
    }

    private void RemoveMissingReferences()
    {
        for (int index =
                 activeMines.Count - 1;
             index >= 0;
             index--)
        {
            if (activeMines[index] == null)
                activeMines.RemoveAt(index);
        }
    }

    // Mines are children of the level board and intentionally survive
    // the boss. MineBoard.Clear() removes them on reload/level change.
}
