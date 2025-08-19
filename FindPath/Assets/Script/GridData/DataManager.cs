using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class DataManager : IBaseManager
    {
        [Inject] private readonly AssetManager _assetManager;
        
        private GridDataAsset _gridDataAsset;
        private DifficultyDataConfig _difficultyDataConfig;

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

        private DifficultyDataConfig GetDifficultyDataConfig()
        {
            if (_difficultyDataConfig == null)
            {
                _difficultyDataConfig = _assetManager.LoadScriptableObject<DifficultyDataConfig>(ObjectNames.DifficultyDataName);    
            }

            if (_difficultyDataConfig == null)
            {
                Debug.LogError($"Can't not load {ObjectNames.GridDataName}");
            }
            return _difficultyDataConfig;
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

        public GridData LoadGridDataByStageClearCount(int stageClearCount)
        {
            var difficultyDataConfig = GetDifficultyDataConfig();
            var difficultyData = difficultyDataConfig.DifficultyDataList.FirstOrDefault(x => x.MaxStageClearCount >= stageClearCount);
            if (difficultyData != null)
            {
                return LoadGridData(difficultyData.MinTurnCount);    
            }
            var lastDifficultyData = difficultyDataConfig.DifficultyDataList.Last();
            return LoadGridData(lastDifficultyData.MinTurnCount);
        }
        
        private GridData LoadGridData(int turnCount)
        {
            var gridDataAsset = GetGridDataAsset();
            
            var gridDataGroup = gridDataAsset.groups.Find(x => x.minTurnCount == turnCount);
            return gridDataGroup.grids[Random.Range(0, gridDataGroup.grids.Count)];
        }
    }
}