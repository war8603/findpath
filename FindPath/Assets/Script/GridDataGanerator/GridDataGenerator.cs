using System;
using System.Collections.Generic;
using UnityEngine;

public static class GridDataGenerator
{
    private static readonly Vector2Int[] Directions = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0)
    };

    public static GridData CreateGridData(Vector2Int startPosition, Vector2Int endPosition, int sizeX, int sizeY, int minTurnCount)
    {
        var rand = new System.Random();
        int attempts = 0, maxAttempts = 1000;
        var subAttempts = 0;
        var subMaxAttempts = 100;
        while (attempts++ < maxAttempts)
        {
            var grid = CreateEmptyGrid(startPosition, endPosition, sizeX, sizeY);
            var result = FindBestPath(grid, startPosition, endPosition);

            while (result.Path != null && result.Turns < minTurnCount && subAttempts++ < maxAttempts)
            {
                List<Vector2> candidates = new(result.Path);
                candidates.Remove(startPosition);
                candidates.Remove(endPosition);

                if (candidates.Count == 0)
                    break;

                var toBlock = candidates[rand.Next(candidates.Count)];
                var cell = grid.GetCell((int)toBlock.x, (int)toBlock.y);
                cell.IsObstacle = true;

                result = FindBestPath(grid, startPosition, endPosition);

                if (result.Path == null)
                {
                    cell.IsObstacle = false;
                }
            }

            if (result.Path != null && result.Turns >= minTurnCount)
            {
                Debug.Log($"✅ Success with {result.Turns} turns");
                grid.MinTurnCount = result.Turns;
                grid.PathCount = result.Path.Count;
                return grid;
            }
        }

        Debug.LogError("❌ Failed to generate valid grid.");
        return null;
    }

    private static GridData CreateEmptyGrid(Vector2Int startPosition, Vector2Int endPosition, int sizeX, int sizeY)
    {
        var grid = new GridData { SizeX = sizeX, SizeY = sizeY, StartPosition = startPosition, EndPosition = endPosition };
        for (var y = 0; y < sizeY; y++)
        {
            for (var x = 0; x < sizeX; x++)
            {
                grid.Cells.Add(new GridCell { Position = new Vector2Int(x, y), IsObstacle = false });
            }
        }
            
        return grid;
    }

    private class PathResult
    {
        public List<Vector2> Path;
        public int Turns;
    }

    private static PathResult FindBestPath(GridData grid, Vector2 start, Vector2 end)
    {
        var queue = new Queue<(Vector2 pos, Vector2 dir, int turns, List<Vector2> path)>();
        var visited = new Dictionary<(int, int, int, int), int>();

        foreach (var d in Directions)
        {
            var next = start + d;
            if (!IsValid(grid, (int)next.x, (int)next.y)) continue;

            queue.Enqueue((next, d, 0, new List<Vector2> { start, next }));
            visited[((int)next.x, (int)next.y, d.x, d.y)] = 0;
        }

        List<Vector2> bestPath = null;
        var minTurns = int.MaxValue;

        while (queue.Count > 0)
        {
            var (pos, dir, turns, path) = queue.Dequeue();
            if (pos == end)
            {
                if (turns < minTurns)
                {
                    minTurns = turns;
                    bestPath = new List<Vector2>(path);
                }
                continue;
            }

            foreach (var nd in Directions)
            {
                var next = pos + nd;
                if (!IsValid(grid, (int)next.x, (int)next.y)) continue;

                var newTurns = turns + (nd != dir ? 1 : 0);
                var state = ((int)next.x, (int)next.y, nd.x, nd.y);
                if (visited.ContainsKey(state) && visited[state] <= newTurns) continue;

                visited[state] = newTurns;
                var newPath = new List<Vector2>(path) { next };
                queue.Enqueue((next, nd, newTurns, newPath));
            }
        }

        return new PathResult { Path = bestPath, Turns = minTurns };
    }

    private static bool IsValid(GridData grid, int x, int y)
    {
        return x >= 0 && y >= 0 && x < grid.SizeX && y < grid.SizeY && !grid.IsObstacle(x, y);
    }
}