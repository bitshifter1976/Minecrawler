using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LevelBalancer
{
    [MenuItem("Tools/Balance Levels")]
    public static void BalanceAllLevels()
    {
        if (!EditorUtility.DisplayDialog("Balance Levels",
            "Die vorhandenen Leveldateien in Assets/Resources/Levels werden angepasst. Fortfahren?", "Ja", "Abbrechen"))
            return;

        var log = new List<string>();

        for (int level = 1; level <= 100; level++)
        {
            string path = $"Assets/Resources/Levels/level{level:000}.txt";

            if (!File.Exists(path))
                continue;

            string[] lines = File.ReadAllLines(path)
                                 .Select(l => l.Replace("\r", "")).ToArray();

            int height = lines.Length;
            int width = lines[0].Length;

            // Build grid
            char[,] grid = new char[height, width];
            for (int r = 0; r < height; r++)
            {
                string row = lines[r].PadRight(width, ' ');
                for (int c = 0; c < width; c++)
                    grid[r, c] = row[c];
            }

            // Interior coordinates: rows 1..height-2, cols 1..width-2
            var interiorPositions = new List<(int r, int c)>();
            for (int r = 1; r < height - 1; r++)
                for (int c = 1; c < width - 1; c++)
                    interiorPositions.Add((r, c));

            var walls = new List<(int r, int c)>();
            var coals = new List<(int r, int c)>();
            var rocks = new List<(int r, int c)>();
            var bosses = new List<(int r, int c)>();
            var empty = new List<(int r, int c)>();
            (int r, int c)? player = null;
            (int r, int c)? exit = null;

            foreach (var pos in interiorPositions)
            {
                char ch = grid[pos.r, pos.c];
                switch (ch)
                {
                    case '#': walls.Add(pos); break;
                    case 'C': coals.Add(pos); break;
                    case 'B': rocks.Add(pos); break;
                    case 'X': bosses.Add(pos); break;
                    case 'P': player = pos; break;
                    case 'E': exit = pos; break;
                    default: empty.Add(pos); break;
                }
            }

            float progress = (level - 1) / 99f;
            // Targets (tweakable): more obstacles with level, fewer coals
            int targetCoal = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(20, 8, progress)), 0, interiorPositions.Count);
            int targetRocks = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(2, 30, progress)), 0, interiorPositions.Count);
            int targetWalls = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, 25, progress)), 0, interiorPositions.Count);

            int targetBosses = (level % 10 == 0) ? 1 : 0;

            // Ensure available slots account for player and exit and boss slot(s)
            int reserved = 0;
            if (player.HasValue) reserved++;
            if (exit.HasValue) reserved++;
            // don't double-count existing bosses here; reserved is for planning available slots
            int availableForPlacement = interiorPositions.Count - reserved;

            int totalDesired = targetCoal + targetRocks + targetWalls + targetBosses;
            if (totalDesired > availableForPlacement)
            {
                // scale down proportionally (preserve boss requirement if possible)
                int nonBossDesired = targetCoal + targetRocks + targetWalls;
                int nonBossAvailable = Math.Max(0, availableForPlacement - targetBosses);
                if (nonBossDesired > nonBossAvailable && nonBossDesired > 0)
                {
                    float scale = nonBossAvailable / (float)nonBossDesired;
                    targetCoal = Mathf.Max(0, Mathf.RoundToInt(targetCoal * scale));
                    targetRocks = Mathf.Max(0, Mathf.RoundToInt(targetRocks * scale));
                    targetWalls = Mathf.Max(0, Mathf.RoundToInt(targetWalls * scale));
                }
                // if still overflow, reduce further proportionally including boss (fallback)
                totalDesired = targetCoal + targetRocks + targetWalls + targetBosses;
                if (totalDesired > availableForPlacement)
                {
                    float scaleAll = availableForPlacement / (float)totalDesired;
                    targetCoal = Mathf.Max(0, Mathf.RoundToInt(targetCoal * scaleAll));
                    targetRocks = Mathf.Max(0, Mathf.RoundToInt(targetRocks * scaleAll));
                    targetWalls = Mathf.Max(0, Mathf.RoundToInt(targetWalls * scaleAll));
                    targetBosses = Mathf.Max(0, Mathf.RoundToInt(targetBosses * scaleAll));
                }
            }

            var rng = new System.Random(level);

            // Helper to remove random items from a list (convert back to empty)
            void ReduceList(List<(int r, int c)> list, int desired)
            {
                while (list.Count > desired)
                {
                    int idx = rng.Next(list.Count);
                    var p = list[idx];
                    list.RemoveAt(idx);
                    grid[p.r, p.c] = '.';
                    empty.Add(p);
                }
            }

            // Helper to add items from empty to a target list
            void GrowList(List<(int r, int c)> list, int desired, char mark)
            {
                while (list.Count < desired && empty.Count > 0)
                {
                    int idx = rng.Next(empty.Count);
                    var p = empty[idx];
                    empty.RemoveAt(idx);
                    list.Add(p);
                    grid[p.r, p.c] = mark;
                }
            }

            // Remove or add walls
            ReduceList(walls, targetWalls);
            GrowList(walls, targetWalls, '#');

            // Recompute empty (some walls were removed/added)
            empty = interiorPositions
                .Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E')
                .ToList();

            // Rocks
            ReduceList(rocks, targetRocks);
            empty = interiorPositions
                .Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E')
                .ToList();
            GrowList(rocks, targetRocks, 'B');

            // Coals
            empty = interiorPositions
                .Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E')
                .ToList();
            ReduceList(coals, targetCoal);
            GrowList(coals, targetCoal, 'C');

            // Bosses: ensure targetBosses count (typically 0 or 1)
            ReduceList(bosses, targetBosses);
            empty = interiorPositions
                .Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E')
                .ToList();
            GrowList(bosses, targetBosses, 'X');

            // Write back to lines
            for (int r = 0; r < height; r++)
            {
                char[] rowchars = new char[width];
                for (int c = 0; c < width; c++)
                    rowchars[c] = grid[r, c];
                lines[r] = new string(rowchars);
            }

            File.WriteAllLines(path, lines);
            log.Add($"Level {level:000}: Coal {coals.Count}, Rocks {rocks.Count}, Walls(internal) {walls.Count}, Bosses {bosses.Count}");
        }

        Debug.Log($"Level Balancer fertig. Angepasste Level: {log.Count}\n" + string.Join("\n", log));
        EditorUtility.DisplayDialog("Balance Levels", $"Fertig. Angepasste Level: {log.Count}", "OK");
        AssetDatabase.Refresh();
    }
}