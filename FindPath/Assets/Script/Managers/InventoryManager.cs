using System;
using GameUtilities;
using Managers;
using UniRx;
using UnityEngine;

namespace FindPath
{
    public class InventoryManager : IBaseManager
    {
        public ReactiveProperty<int> CoinProperty => _coinProperty;
        private ReactiveProperty<int> _coinProperty = new ReactiveProperty<int>(0);
        
        public ReactiveProperty<int> DiamondProperty => _diamondProperty;
        private ReactiveProperty<int> _diamondProperty = new ReactiveProperty<int>(0);

        public void IncreaseCoins(int amount)
        {
            _coinProperty.Value += amount;
        }

        public void DecreaseCoin(int amount)
        {
            _coinProperty.Value -= amount;
        }

        public void IncreaseDiamond(int amount)
        {
            _diamondProperty.Value += amount;
        }

        public void DecreaseDiamond(int amount)
        {
            _diamondProperty.Value -= amount;
        }

        public void LoadData()
        {
            var coin = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.Coin, defaultValue:0);
            _coinProperty.Value = coin;
            
            var diamond = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.Diamond, defaultValue:0);
            _diamondProperty.Value = diamond;
            _diamondProperty.Value = 50;
        }

        public void SaveData()
        {
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.Coin, _coinProperty.Value);
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.Diamond, _diamondProperty.Value);
            
            PlayerPrefs.Save();
        }

        public void InitManager()
        {
            LoadData();
        }

        public bool IsEnoughCost(CostType costType, int amount)
        {
            return costType switch
            {
                CostType.Coin => _coinProperty.Value >= amount,
                CostType.Diamond => _diamondProperty.Value >= amount,
                _ => throw new ArgumentOutOfRangeException(nameof(costType), costType, null)
            };
        }
    }
}