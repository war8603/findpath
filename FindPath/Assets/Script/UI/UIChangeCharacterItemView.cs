using System;
using Managers;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIChangeCharacterItemView : MonoBehaviour
    {
        [Inject] private readonly AssetManager _assetManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly DataManager _dataManager;
        
        [SerializeField] private Button _onclick;
        [SerializeField] private GameObject _corverObj;
        [SerializeField] private Image _characterIamge;
        [SerializeField] private TMP_Text _costText;
        
        public CharacterType CharacterType { get; private set; }
        
        public void SetData(CharacterType characterType, Action<CharacterType> callback, bool isOwned)
        {
            CharacterType = characterType;
            _onclick.onClick.AddListener(() => callback?.Invoke(characterType));
            _characterIamge.sprite = _assetManager.LoadSprite(DataConfig.GetCharacterIconName(characterType));
            SetOwnedItem(isOwned);

            _inventoryManager.DiamondProperty.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                const int characterCost = DataConfig.CharacterCost;
                _costText.text = characterCost.ToString();
                _costText.color = !_inventoryManager.IsEnoughCost(CostType.Diamond, DataConfig.CharacterCost) ? Color.red : Color.white;
            });
        }

        public void SetOwnedItem(bool isOwned)
        {
            _corverObj.SetActive(!isOwned);
            _costText.gameObject.SetActive(!isOwned);
        }
    }
}