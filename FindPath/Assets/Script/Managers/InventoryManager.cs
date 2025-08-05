using GameUtilities;
using Managers;
using UniRx;

namespace FindPath
{
    public class InventoryManager : IBaseManager
    {
        public ReactiveProperty<int> CoinProperty => _coinProperty;
        private ReactiveProperty<int> _coinProperty = new ReactiveProperty<int>(0);

        public void IncreaseCoins(int amount)
        {
            _coinProperty.Value += amount;
        }

        public void DecreaseCoins(int amount)
        {
            _coinProperty.Value -= amount;
        }

        public void LoadData()
        {
            var coin = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.Coin, defaultValue:0);
            _coinProperty.Value = coin;
        }

        public void SaveData()
        {
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.Coin, _coinProperty.Value);
        }

        public void InitManager()
        {
            LoadData();
        }

        public bool IsEnoughCost(int amount)
        {
            return _coinProperty.Value >= amount;
        }
    }
}