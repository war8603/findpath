using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GridDataGenerator
{
    private static readonly Vector2Int[] Directions = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0)
    };

    public static GridData CreateGridData(int gridIndex, Vector2Int startPosition, Vector2Int endPosition, int sizeX, int sizeY, int minTurnCount)
    {
        var rand = new System.Random();
        var attempts = 0;
        var maxAttempts = 1000;
        var grid = CreateEmptyGrid(startPosition, endPosition, sizeX, sizeY);

        // 랜덤한 위치에 장애물 생성
        var randomObstacleCells = new List<Vector2>();
        var randomCellCount = 5;
        while (randomCellCount > 0)
        {
            var randomCell = new Vector2(rand.Next(0, sizeX), rand.Next(0, sizeY));
            if (!randomCell.Equals(startPosition) && !randomCell.Equals(endPosition)
                                                  && !randomObstacleCells.Contains(randomCell))
            {
                randomCellCount--;
                randomObstacleCells.Add(randomCell);
                grid.GetCell((int)randomCell.x, (int)randomCell.y).IsObstacle = true;
            }    
        }
        
        var result = FindBestPath(grid, startPosition, endPosition);
        while (result.Path != null && result.Turns < minTurnCount && attempts++ < maxAttempts)
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
            grid.MinTurnCount = result.Turns;
            grid.PathCount = result.Path.Count;
            grid.GridIndex = gridIndex;
            return grid;
        }

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

        foreach (var direction in Directions)
        {
            var next = start + direction;
            if (!IsValid(grid, (int)next.x, (int)next.y)) continue;

            queue.Enqueue((next, direction, 0, new List<Vector2> { start, next }));
            visited[((int)next.x, (int)next.y, direction.x, direction.y)] = 0;
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

            foreach (var direction in Directions)
            {
                var next = pos + direction;
                if (!IsValid(grid, (int)next.x, (int)next.y)) continue;

                var newTurns = turns + (direction != dir ? 1 : 0);
                var state = ((int)next.x, (int)next.y, direction.x, direction.y);
                if (visited.ContainsKey(state) && visited[state] <= newTurns) continue;

                visited[state] = newTurns;
                var newPath = new List<Vector2>(path) { next };
                queue.Enqueue((next, direction, newTurns, newPath));
            }
        }

        return new PathResult { Path = bestPath, Turns = minTurns };
    }

    /// <summary>
    /// 맵을 벗어나거나, 장애물이면 갈 수 없음
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static bool IsValid(GridData grid, int x, int y)
    {
        return x >= 0 && y >= 0 && x < grid.SizeX && y < grid.SizeY && !grid.IsObstacle(x, y);
    }
}