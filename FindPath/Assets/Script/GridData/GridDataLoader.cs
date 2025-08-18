using Managers;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class GridDataLoader : IBaseManager
    {
        [Inject] private readonly AssetManager _assetManager;
        
        private GridDataAsset _gridDataAsset;

        public void InitManager()
        {
            var gridDataAsset = _gridDataAsset;
            if (gridDataAsset != null)
            {
                _gridDataAsset = null;
            }
        }
        
        public GridData LoadRandomGridData()
        {
            _gridDataAsset = _assetManager.LoadScriptableObject<GridDataAsset>(ObjectNames.GridDataName);
            if (_gridDataAsset == null) return null;
            
            const int minTurnCount = 3;

            var gridDataGroup = _gridDataAsset.groups.Find(x => x.minTurnCount == minTurnCount);
            return gridDataGroup.grids[Random.Range(0, gridDataGroup.grids.Count)];
        }
    }
}