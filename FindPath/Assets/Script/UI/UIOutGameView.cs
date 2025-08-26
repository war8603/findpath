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
        [Inject] private readonly DataManager _loadMap;
        [Inject] private readonly IAPManager _iapManager;
        
        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _coin;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private Button _libraryButton;
        [SerializeField] private Button _removeAdsButton;
        [SerializeField] private Button _restoreButton;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnClickStartButton);
            _bestScoreText.text = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.BestScore, 0).ToString();
            
            _libraryButton.onClick.AddListener(OnClickLibraryButton);
            _removeAdsButton.onClick.AddListener(OnClickRemoveAdsButton);
            _restoreButton.onClick.AddListener(OnClickRestoreButton);            
        }

        [Inject]
        public void Construct(InventoryManager inventoryManager, IAPManager iapManager)
        {
            inventoryManager.CoinProperty.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                _coin.text = value.ToString();
            }).AddTo(this);

            iapManager.HasNoAds.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                _removeAdsButton.gameObject.SetActive(!value);
            }).AddTo(this);
        }

        private void OnClickStartButton()
        {
            _gameManager.StartBattle();
        }

        private void OnClickLibraryButton()
        {
            CreateCollectionPopup().Forget();   
        }

        private async UniTask CreateCollectionPopup()
        {
            var view = await _uiManager.CreateView<UICollectionPopupView>(UIViewNames.UICollectionPopupView);
            view.ShowView();
        }

        private void OnClickRemoveAdsButton()
        {
            _iapManager.BuyRemoveAds();           
        }

        private void OnClickRestoreButton()
        {
            _iapManager.RestorePurchases();
        }
    }
}