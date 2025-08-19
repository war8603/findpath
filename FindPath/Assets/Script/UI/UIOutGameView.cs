using Cysharp.Threading.Tasks;
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
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly GridDataLoader _loadMap;
        
        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _coin;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private Button _libraryButton;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnClickStartButton);
            _bestScoreText.text = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.BestScore, 0).ToString();
            
            _libraryButton.onClick.AddListener(OnClickLibraryButton);
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

        private void OnClickLibraryButton()
        {
            CreateLibraryPopup().Forget();   
        }

        private async UniTask CreateLibraryPopup()
        {
            var view = await _uiManager.CreateView<UILibraryPopupView>(UIViewNames.UILibraryPopupView);
            view.ShowView();
        }
    }
}