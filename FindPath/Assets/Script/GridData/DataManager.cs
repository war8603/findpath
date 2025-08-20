using System.Collections.Generic;
using System.Linq;
using GameUtilities;
using Managers;
using UnityEngine;
using VContainer;
using Newtonsoft.Json;

namespace FindPath
{
    public class StageClearData
    {
        public int GridIndex;
        public int ClearCount;
    }
    
    public class DataManager : IBaseManager
    {
        [Inject] private readonly AssetManager _assetManager;
        
        private GridDataAsset _gridDataAsset;
        private DifficultyDataConfig _difficultyDataConfig;
        private List<StageClearData> _stageClearData;

        public void InitManager()
        {
            var gridDataAsset = _gridDataAsset;
            if (gridDataAsset != null)
            {
                _gridDataAsset = null;
            }

            LoadStageClearData();
        }

        public int GetClearCount(int gridIndex)
        {
            var stageData = _stageClearData.Find(x => x.GridIndex == gridIndex);
            return stageData?.ClearCount ?? 0;
        }

        private void LoadStageClearData()
        {
            var stageClearData = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.StageClearData, string.Empty);
            if (!string.IsNullOrEmpty(stageClearData))
            {
                _stageClearData = JsonConvert.DeserializeObject<List<StageClearData>>(stageClearData);
            }
            else
            {
                _stageClearData = new List<StageClearData>();
            }
        }
        
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
            gridDataList.Sort((a, b) => a.GridIndex.CompareTo(b.GridIndex));
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

        public void SaveStageClearData(int gridIndex)
        {
            var stageClearData = _stageClearData.Find(x => x.GridIndex == gridIndex);
            if (stageClearData != null)
            {
                stageClearData.ClearCount += 1;
            }
            else
            {
                _stageClearData.Add(new StageClearData { GridIndex = gridIndex, ClearCount = 1});
            }
            
            var json = JsonConvert.SerializeObject(_stageClearData);
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.StageClearData, json);
        }
    }
}