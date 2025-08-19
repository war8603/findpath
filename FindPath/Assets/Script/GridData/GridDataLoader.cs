using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class GridDataLoader : IBaseManager
    {
        [Inject] private readonly AssetManager _assetManager;
        
        private GridDataAsset _gridDataAsset;

        private GridDataAsset GetGridDataAsset()
        {
            if (_gridDataAsset == null)
            {
                _gridDataAsset = _assetManager.LoadScriptableObject<GridDataAsset>(ObjectNames.GridDataName);    
            }

            if (_gridDataAsset == null)
            {
                Debug.LogError($"Can't not load {ObjectNames.GridDataName}");
            }
            return _gridDataAsset;
        }
        
        public void InitManager()
        {
            var gridDataAsset = _gridDataAsset;
            if (gridDataAsset != null)
            {
                _gridDataAsset = null;
            }
        }

        public int GetGroupCount()
        {
            var gridDataAsset = GetGridDataAsset();
            return gridDataAsset.groups.Sum(x => x.grids.Count);
        }

        public List<GridData> GetGridDataList()
        {
            var gridDataAsset = GetGridDataAsset();
            var gridDataList = new List<GridData>();
            foreach (var gridDataGroup in gridDataAsset.groups)
            {
                gridDataList.AddRange(gridDataGroup.grids);
            }
            return gridDataList;
        }
        
        public GridData LoadRandomGridData()
        {
            var gridDataAsset = GetGridDataAsset();
            
            const int minTurnCount = 3;

            var gridDataGroup = gridDataAsset.groups.Find(x => x.minTurnCount == minTurnCount);
            return gridDataGroup.grids[Random.Range(0, gridDataGroup.grids.Count)];
        }
    }
}