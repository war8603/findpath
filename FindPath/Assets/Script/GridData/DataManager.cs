using System;
using System.Collections.Generic;
using System.Linq;
using GameUtilities;
using Managers;
using UnityEngine;
using VContainer;
using Newtonsoft.Json;
using UniRx;
using Random = UnityEngine.Random;

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
        
        // 현재 캐릭터 테마
        private readonly Dictionary<CharacterType, bool> _ownedCharacters = new();
        public ReactiveProperty<CharacterType> CurrentCharacterType = new(CharacterType.Cat);

        public void InitManager()
        {
            var gridDataAsset = _gridDataAsset;
            if (gridDataAsset != null)
            {
                _gridDataAsset = null;
            }

            LoadStageClearData();
            LoadOwnedCharacters();
            LoadCurrentCharacterType();
        }

        /// <summary>
        /// 캐릭터 보유정보 로드
        /// </summary>
        private void LoadOwnedCharacters()
        {
            _ownedCharacters.Add(CharacterType.Cat, true);
            foreach (CharacterType characterType in Enum.GetValues(typeof(CharacterType)))
            {
                if (_ownedCharacters.ContainsKey(characterType)) continue;
                _ownedCharacters.Add(characterType, PlayerPrefsTool.GetPlayerPrefs(characterType.ToString(), false));
            }
        }

        /// <summary>
        /// 현재 캐릭터 타입 로드
        /// </summary>
        private void LoadCurrentCharacterType()
        {
            CurrentCharacterType.Value =
                PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.CurrentCharacterType, CharacterType.Cat);
        }

        /// <summary>
        /// 해당 캐릭터 타입을 보유했는지 여부 리턴
        /// </summary>
        /// <param name="characterType"></param>
        /// <returns></returns>
        public bool IsOwnedCharacter(CharacterType characterType)
        {
            if (_ownedCharacters.TryGetValue(characterType, out var isOwned))
            {
                return isOwned;
            }

            isOwned = PlayerPrefsTool.GetPlayerPrefs(characterType.ToString(), false);
            _ownedCharacters.Add(characterType, isOwned);
            return isOwned;
        }

        public void SetCurrentCharacterType(CharacterType characterType)
        {
            if (characterType == CurrentCharacterType.Value) return;
            CurrentCharacterType.Value = characterType;
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.CurrentCharacterType, characterType);
        }

        public void OnPurchaseCharacter(CharacterType characterType)
        {
            _ownedCharacters[characterType] = true;
            PlayerPrefsTool.SetPlayerPrefs(characterType.ToString(), true);
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