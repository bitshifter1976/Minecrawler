using UnityEngine;

public sealed class MineLevelData
{
    public string[] Rows { get; }
    public int Width { get; }
    public int Height { get; }

    public MineLevelData(string[] rows)
    {
        Rows = rows;
        Height = rows.Length;

        int width = 0;

        foreach (string row in rows)
        {
            width = Mathf.Max(width, row.Length);
        }

        Width = width;
    }

    public char GetTile(int x, int row)
    {
        if (row < 0 || row >= Height)
            return '#';

        if (x < 0 || x >= Rows[row].Length)
            return '#';

        return Rows[row][x];
    }
}