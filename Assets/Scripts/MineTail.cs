using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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
        if (segment != null)
            segments.Add(segment);
    }

    public void Clear()
    {
        segments.Clear();
    }

    public void RemoveOutside(int boardWidth, int boardHeight)
    {
        while (segments.Count > 0)
        {
            TailSegment firstSegment = segments[0];

            if (firstSegment == null)
            {
                segments.RemoveAt(0);
                continue;
            }

            Vector2Int position = firstSegment.GridPosition;

            bool isOutside =
                position.x < 0 ||
                position.x >= boardWidth ||
                position.y < 0 ||
                position.y >= boardHeight;

            if (!isOutside)
                break;

            Object.Destroy(firstSegment.gameObject);
            segments.RemoveAt(0);
        }
    }
}