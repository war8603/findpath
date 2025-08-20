using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GridData
{
    public int GridIndex;
    public List<GridCell> Cells = new();
    public int SizeX;
    public int SizeY;
    public Vector2Int StartPosition;
    public Vector2Int EndPosition;

    public int MinTurnCount;
    public int PathCount;

    public GridCell GetCell(int x, int y) => Cells.Find(c => (int)c.Position.x == x && (int)c.Position.y == y);
    public bool IsObstacle(int x, int y) => GetCell(x, y)?.IsObstacle ?? true;
}

[Serializable]
public class GridCell
{
    public Vector2Int Position;
    public bool IsObstacle;
}