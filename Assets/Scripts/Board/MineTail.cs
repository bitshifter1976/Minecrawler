using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MineTail
{
    private readonly List<TailSegment> segments = new();
    public bool IsEmpty => segments.Count == 0;

    public int Count => segments.Count;

    public bool Contains(Vector2Int position)
    {
        foreach (TailSegment segment in segments)
        {
            if (segment != null &&
                segment.GridPosition == position)
            {
                return true;
            }
        }

        return false;
    }

    public Vector2Int GetNewSegmentPosition(
        Vector2Int previousMinerPosition)
    {
        if (segments.Count == 0)
            return previousMinerPosition;

        return segments[^1].GridPosition;
    }

    public void Move(Vector2Int previousMinerPosition)
    {
        Vector2Int nextPosition =
            previousMinerPosition;

        foreach (TailSegment segment in segments)
        {
            Vector2Int oldPosition =
                segment.GridPosition;

            segment.SetGridPosition(nextPosition);

            nextPosition = oldPosition;
        }
    }

    public void Add(TailSegment segment)
    {
        if (segment == null)
            return;

        segments.Add(segment);
        RefreshDustRoles();
    }

    public void Clear()
    {
        segments.Clear();
    }

    private void RefreshDustRoles()
    {
        for (int index = 0;
             index < segments.Count;
             index++)
        {
            TailSegment segment =
                segments[index];

            if (segment == null)
                continue;

            segment.SetDustEnabled(
                index == 0);
        }
    }


    public void RemoveOutside(
        int boardWidth,
        int boardHeight)
    {
        bool removedAny = false;

        for (int index = segments.Count - 1;
             index >= 0;
             index--)
        {
            TailSegment segment =
                segments[index];

            if (segment == null)
            {
                segments.RemoveAt(index);
                removedAny = true;
                continue;
            }

            Vector2Int position =
                segment.GridPosition;

            bool isOutside =
                position.x < 0 ||
                position.x >= boardWidth ||
                position.y < 0 ||
                position.y >= boardHeight;

            if (!isOutside)
                continue;

            UnityEngine.Object.Destroy(
                segment.gameObject);

            segments.RemoveAt(index);
            removedAny = true;
        }

        if (removedAny)
            RefreshDustRoles();
    }

    public void DeleteOnExit()
    {
        if (segments.Count == 0)
            return;

        TailSegment firstSegment =
            segments[0];

        if (firstSegment != null)
        {
            UnityEngine.Object.Destroy(
                firstSegment.gameObject);
        }

        segments.RemoveAt(0);
        RefreshDustRoles();
    }
}