using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace FindPath
{
    public class UIChangeCharacterView : UIMainView
    {
        [Inject] private readonly DataManager _dataManager;
        [Inject] private readonly AssetManager _assetManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly IObjectResolver _objectResolver;
        
        [SerializeField] private Transform _root;
        [SerializeField] private Button _closeButton;

        private readonly List<UIChangeCharacterItemView> _items = new();

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnClickCloseButton);
        }
        
        public void SetData()
        {
            var itemBaseObj = _assetManager.LoadGameObject(UIViewNames.UIChangeCharacterItemView);
            foreach (CharacterType characterType in Enum.GetValues(typeof(CharacterType)))
            {
                var obj = GameObject.Instantiate(itemBaseObj, _root.transform);
                _objectResolver.InjectGameObject(obj);
                var item = obj.GetComponent<UIChangeCharacterItemView>();
                var isOwned = _dataManager.IsOwnedCharacter(characterType);
                item.SetData(characterType, OnClickItem, isOwned);

                _items.Add(item);
            }
        }

        private void OnClickItem(CharacterType characterType)
        {
            if (_dataManager.IsOwnedCharacter(characterType))
            {
                _dataManager.SetCurrentCharacterType(characterType);
                return;
            }

            // 구매가 가능하면 구매
            var isEnoughCost = _inventoryManager.IsEnoughCost(CostType.Diamond, DataConfig.CharacterCost);
            if (isEnoughCost)
            {
                _dataManager.OnPurchaseCharacter(characterType);
                _dataManager.SetCurrentCharacterType(characterType);
                _inventoryManager.DecreaseDiamond(DataConfig.CharacterCost);
                
                var item = _items.Find(x => x.CharacterType == characterType);
                item.SetOwnedItem(true);
            }
        }

        private void OnClickCloseButton()
        {
            _uiManager.DestroyView(this);           
        }
    }
}