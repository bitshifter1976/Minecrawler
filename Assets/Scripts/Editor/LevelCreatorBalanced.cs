using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LevelCreatorBalanced
{
    [MenuItem("Tools/Balance Levels")]
    public static void BalanceAllLevels()
    {
        if (!EditorUtility.DisplayDialog("Balance Levels",
            "Die vorhandenen Leveldateien in Assets/Resources/Levels werden angepasst. Fortfahren?", "Ja", "Abbrechen"))
            return;

        string levelsDir = "Assets/Resources/Levels";
        string backupDir = Path.Combine(levelsDir, "_backup");
        Directory.CreateDirectory(backupDir);

        var log = new List<string>();

        for (int level = 1; level <= 100; level++)
        {
            string filename = $"level{level:000}.txt";
            string path = Path.Combine(levelsDir, filename);

            if (!File.Exists(path))
                continue;

            // create backup
            string backupPath = Path.Combine(backupDir, filename + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak");
            File.Copy(path, backupPath, true);

            string[] lines = File.ReadAllLines(path)
                                 .Select(l => l.Replace("\r", "")).ToArray();

            int height = lines.Length;
            int width = lines[0].Length;

            char[,] grid = new char[height, width];
            for (int r = 0; r < height; r++)
            {
                string row = lines[r].PadRight(width, ' ');
                for (int c = 0; c < width; c++)
                    grid[r, c] = row[c];
            }

            var interior = new List<(int r, int c)>();
            for (int r = 1; r < height - 1; r++)
                for (int c = 1; c < width - 1; c++)
                    interior.Add((r, c));

            (int r, int c)? player = null;
            (int r, int c)? exit = null;
            var coals = new List<(int r, int c)>();
            var rocks = new List<(int r, int c)>();
            var walls = new List<(int r, int c)>();
            var bosses = new List<(int r, int c)>();

            foreach (var p in interior)
            {
                char ch = grid[p.r, p.c];
                switch (ch)
                {
                    case 'P': player = p; break;
                    case 'E': exit = p; break;
                    case 'C': coals.Add(p); break;
                    case 'B': rocks.Add(p); break;
                    case '#': walls.Add(p); break;
                    case 'X': bosses.Add(p); break;
                }
            }

            (int r, int c)[] Neighbors((int r, int c) p) =>
                new[] { (p.r - 1, p.c), (p.r + 1, p.c), (p.r, p.c - 1), (p.r, p.c + 1) };

            bool InBounds((int r, int c) p) => p.r >= 0 && p.r < height && p.c >= 0 && p.c < width;
            bool Walkable((int r, int c) p) => InBounds(p) && grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X';

            // dead-end = walkable tile with <= 1 walkable neighbor (treat out of bounds or '#' as non-walkable)
            bool IsDeadEnd((int r, int c) p)
            {
                if (!Walkable(p)) return false;
                int walkNeighbors = 0;
                foreach (var n in Neighbors(p))
                    if (InBounds(n) && Walkable(n)) walkNeighbors++;
                return walkNeighbors <= 1;
            }

            // BFS reachable from player (treat B and X and # as blocking)
            HashSet<(int r, int c)> ComputeReachableFrom((int r, int c) start)
            {
                var q = new Queue<(int r, int c)>();
                var seen = new HashSet<(int r, int c)>();
                q.Enqueue(start);
                seen.Add(start);
                while (q.Count > 0)
                {
                    var cur = q.Dequeue();
                    foreach (var n in Neighbors(cur))
                    {
                        if (!InBounds(n)) continue;
                        if (seen.Contains(n)) continue;
                        if (!Walkable(n)) continue;
                        seen.Add(n);
                        q.Enqueue(n);
                    }
                }
                return seen;
            }

            // path search allowing carving through walls
            bool TryFindPathAllowingWalls((int r, int c) from, (int r, int c) to, out List<(int r, int c)> path)
            {
                path = null;
                var q = new Queue<(int r, int c)>();
                var prev = new Dictionary<(int r, int c), (int r, int c)?>();
                q.Enqueue(from);
                prev[from] = null;
                while (q.Count > 0)
                {
                    var cur = q.Dequeue();
                    if (cur == to) break;
                    foreach (var n in Neighbors(cur))
                    {
                        if (!InBounds(n)) continue;
                        if (prev.ContainsKey(n)) continue;
                        prev[n] = cur;
                        q.Enqueue(n);
                    }
                }
                if (!prev.ContainsKey(to)) return false;
                var rev = new List<(int r, int c)>();
                var cur2 = to;
                while (cur2 != from)
                {
                    rev.Add(cur2);
                    cur2 = prev[cur2].Value;
                }
                rev.Add(from);
                rev.Reverse();
                path = rev;
                return true;
            }

            float progress = (level - 1) / 99f;
            int targetCoal = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(6, 12, progress)), 0, interior.Count);   // moderate start
            int targetRocks = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, 30, progress)), 0, interior.Count); // few rocks early
            int targetWalls = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, 20, progress)), 0, interior.Count); // few walls early
            int targetBosses = (level % 5 == 0) ? 1 : 0;

            int reserved = 0;
            if (player.HasValue) reserved++;
            if (exit.HasValue) reserved++;
            int availableForPlacement = interior.Count - reserved;

            int totalDesired = targetCoal + targetRocks + targetWalls + targetBosses;
            if (totalDesired > availableForPlacement)
            {
                int nonBossDesired = targetCoal + targetRocks + targetWalls;
                int nonBossAvailable = Math.Max(0, availableForPlacement - targetBosses);
                if (nonBossDesired > nonBossAvailable && nonBossDesired > 0)
                {
                    float scale = nonBossAvailable / (float)nonBossDesired;
                    targetCoal = Mathf.Max(0, Mathf.RoundToInt(targetCoal * scale));
                    targetRocks = Mathf.Max(0, Mathf.RoundToInt(targetRocks * scale));
                    targetWalls = Mathf.Max(0, Mathf.RoundToInt(targetWalls * scale));
                }
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

            var empty = interior.Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E').ToList();

            void RemoveRandom(List<(int r, int c)> list, int desired)
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

            // Grow but avoid placing coal on dead-ends (to prevent 180° turn pickups)
            void GrowCoals(int desired)
            {
                // remove coals in dead-ends first (relocate)
                for (int i = coals.Count - 1; i >= 0; i--)
                {
                    var p = coals[i];
                    if (IsDeadEnd(p))
                    {
                        coals.RemoveAt(i);
                        grid[p.r, p.c] = '.';
                        empty.Add(p);
                    }
                }

                // choose empty positions that are not dead-ends
                var candidates = empty.Where(p => !IsDeadEnd(p)).ToList();
                while (coals.Count < desired && candidates.Count > 0)
                {
                    int idx = rng.Next(candidates.Count);
                    var p = candidates[idx];
                    candidates.RemoveAt(idx);
                    empty.Remove(p);
                    coals.Add(p);
                    grid[p.r, p.c] = 'C';
                    // update candidates because adding coal can change dead-end status nearby
                    candidates = empty.Where(q => !IsDeadEnd(q)).ToList();
                }

                // fallback: if not enough non-dead-end slots, allow remaining coals anywhere empty
                while (coals.Count < desired && empty.Count > 0)
                {
                    int idx = rng.Next(empty.Count);
                    var p = empty[idx];
                    empty.RemoveAt(idx);
                    coals.Add(p);
                    grid[p.r, p.c] = 'C';
                }
            }

            void GrowGeneric(List<(int r, int c)> list, int desired, char mark)
            {
                // remove extras first
                RemoveRandom(list, desired);

                // recompute empty
                empty = interior.Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E').ToList();

                while (list.Count < desired && empty.Count > 0)
                {
                    int idx = rng.Next(empty.Count);
                    var p = empty[idx];
                    empty.RemoveAt(idx);
                    list.Add(p);
                    grid[p.r, p.c] = mark;
                }
            }

            // Walls and rocks: adjust normally but ensure they don't block exit reachability or make coal unreachable
            RemoveRandom(walls, targetWalls);
            GrowGeneric(walls, targetWalls, '#');

            empty = interior.Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E').ToList();

            RemoveRandom(rocks, targetRocks);
            GrowGeneric(rocks, targetRocks, 'B');

            // coals: special placement avoiding dead-ends
            GrowCoals(targetCoal);

            // bosses: ensure target count
            RemoveRandom(bosses, targetBosses);
            empty = interior.Where(p => grid[p.r, p.c] != '#' && grid[p.r, p.c] != 'C' && grid[p.r, p.c] != 'B' && grid[p.r, p.c] != 'X' && grid[p.r, p.c] != 'P' && grid[p.r, p.c] != 'E').ToList();
            while (bosses.Count < targetBosses && empty.Count > 0)
            {
                int idx = rng.Next(empty.Count);
                var p = empty[idx];
                empty.RemoveAt(idx);
                bosses.Add(p);
                grid[p.r, p.c] = 'X';
            }

            // ensure exit reachable from player; if not, carve path
            if (player.HasValue && exit.HasValue)
            {
                var reachable = ComputeReachableFrom(player.Value);
                if (!reachable.Contains(exit.Value))
                {
                    if (TryFindPathAllowingWalls(player.Value, exit.Value, out var path2))
                    {
                        foreach (var p in path2)
                            if (grid[p.r, p.c] == '#')
                                grid[p.r, p.c] = '.';
                    }
                }
            }

            // ensure every coal is reachable and not in a dead-end after carving; if coal is unreachable, carve path to nearest non-dead-end tile and move coal there
            if (player.HasValue)
            {
                var reachable = ComputeReachableFrom(player.Value);
                for (int i = coals.Count - 1; i >= 0; i--)
                {
                    var cpos = coals[i];
                    if (!reachable.Contains(cpos) || IsDeadEnd(cpos))
                    {
                        // find nearest empty non-dead-end reachable position
                        var q = new Queue<(int r, int c)>();
                        var seen = new HashSet<(int r, int c)>();
                        q.Enqueue(player.Value);
                        seen.Add(player.Value);
                        (int r, int c)? found = null;
                        while (q.Count > 0 && found == null)
                        {
                            var cur = q.Dequeue();
                            foreach (var n in Neighbors(cur))
                            {
                                if (!InBounds(n)) continue;
                                if (seen.Contains(n)) continue;
                                seen.Add(n);
                                if ((grid[n.r, n.c] == '.' || grid[n.r, n.c] == ' ') && !IsDeadEnd(n))
                                {
                                    found = n;
                                    break;
                                }
                                q.Enqueue(n);
                            }
                        }
                        if (found.HasValue)
                        {
                            // remove original coal
                            grid[cpos.r, cpos.c] = '.';
                            coals.RemoveAt(i);
                            // place new coal
                            var np = found.Value;
                            grid[np.r, np.c] = 'C';
                            coals.Add(np);
                            // recompute reachable
                            reachable = ComputeReachableFrom(player.Value);
                        }
                    }
                }
            }

            // ensure exit is not fully walled-in
            if (exit.HasValue)
            {
                bool hasOpen = false;
                foreach (var n in Neighbors(exit.Value))
                {
                    if (!InBounds(n)) continue;
                    if (grid[n.r, n.c] != '#') { hasOpen = true; break; }
                }
                if (!hasOpen && player.HasValue)
                {
                    foreach (var n in Neighbors(exit.Value))
                    {
                        if (!InBounds(n)) continue;
                        if (TryFindPathAllowingWalls(player.Value, n, out var path2))
                        {
                            foreach (var p in path2)
                                if (grid[p.r, p.c] == '#') grid[p.r, p.c] = '.';
                            break;
                        }
                    }
                }
            }

            // write back
            for (int r = 0; r < height; r++)
            {
                char[] rc = new char[width];
                for (int c = 0; c < width; c++) rc[c] = grid[r, c];
                lines[r] = new string(rc);
            }

            File.WriteAllLines(path, lines);
            log.Add($"Level {level:000} angepasst (backup: {Path.GetFileName(backupPath)})");
        }

        Debug.Log($"Level Balancer fertig. Angepasste Level: {log.Count}\n" + string.Join("\n", log));
        EditorUtility.DisplayDialog("Balance Levels", $"Fertig. Angepasste Level: {log.Count}", "OK");
        AssetDatabase.Refresh();
    }
}