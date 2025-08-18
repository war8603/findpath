using GameUtilities;
using Managers;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIOutGameView : UIMainView
    {
        [Inject] private readonly GameManager _gameManager;
        
        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _coin;
        [SerializeField] private TMP_Text _bestScoreText;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnClickStartButton);
            _bestScoreText.text = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.BestScore, 0).ToString();
        }

        [Inject]
        public void Construct(InventoryManager inventoryManager)
        {
            inventoryManager.CoinProperty.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                _coin.text = value.ToString();
            }).AddTo(this);
        }

        private void OnClickStartButton()
        {
            _gameManager.StartBattle();
        }
    }
}