using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GridDataCollection", menuName = "Custom/GridData Collection")]
public class GridDataAsset : ScriptableObject
{
    public List<GridDataGroup> groups = new();
}

[System.Serializable]
public class GridDataGroup
{
    public int minTurnCount;
    public List<GridData> grids = new();
}