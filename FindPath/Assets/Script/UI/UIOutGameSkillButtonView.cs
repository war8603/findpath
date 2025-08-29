using Managers;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIOutGameSkillButtonView : MonoBehaviour
    {
        [Inject] private readonly SkillManager _skillManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly AdsManager _adsManager;
        
        [SerializeField] private SkillType _skillType;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private TMP_Text _coinCostText;
        [SerializeField] private GameObject _coinCostObj;
        [SerializeField] private GameObject _adsCostObj;
        [SerializeField] private TMP_Text _currentSkillCount;
        
        [Inject]
        public void Construct(SkillManager skillManager, InventoryManager inventoryManager)
        {
            BindOnClickEvent();
            InitSkillCost();
            SubscribeSkillCount(skillManager, inventoryManager);
        }

        private void SubscribeSkillCount(SkillManager skillManager, InventoryManager inventoryManager)
        {
            var maxCount = skillManager.SkillConfig.GetMaxCount(_skillType);
            inventoryManager.CoinProperty.ObserveEveryValueChanged(value => value.Value)
                .Subscribe(value =>
                {
                    var costType = _skillManager.SkillConfig.GetCostType(_skillType);
                    if (costType == CostType.Coin)
                    {
                        _purchaseButton.interactable = inventoryManager.IsEnoughCost(costType,
                            _skillManager.SkillConfig.GetCost(_skillType));
                    }
                    else
                    {
                        _purchaseButton.interactable = true;
                    }
                }).AddTo(this);
           
            skillManager.GetSkillCountProperty(_skillType).ObserveEveryValueChanged(x => x.Value).
                Subscribe(value =>
                {
                    _currentSkillCount.text = $"({value}/{maxCount})";
                    _purchaseButton.gameObject.SetActive(value < maxCount);
                }).AddTo(this);
        }

        private void BindOnClickEvent()
        {
            _purchaseButton.onClick.AddListener(OnClickPurchaseButton);
        }

        private void InitSkillCost()
        {
            if (_skillManager.SkillConfig.GetCostType(_skillType) == CostType.Coin)
            {
                _coinCostText.text = _skillManager.SkillConfig.GetCost(_skillType).ToString();
                _coinCostObj.SetActive(true);
                _adsCostObj.SetActive(false);
            }
            else
            {
                _coinCostObj.SetActive(false);
                _adsCostObj.SetActive(true);
            }
        }

        private void OnClickPurchaseButton()
        {
            var currentCount = _skillManager.GetSkillCount(_skillType);
            var maxCount = _skillManager.SkillConfig.GetMaxCount(_skillType);

            // 보유 수량 초과시 리턴
            if (currentCount >= maxCount) return;

            var costType = _skillManager.SkillConfig.GetCostType(_skillType);
            if (costType == CostType.Coin)
            {
                // 재화 부족시 리턴
                var cost = _skillManager.SkillConfig.GetCost(_skillType);
                if (!_inventoryManager.IsEnoughCost(costType, cost)) return;

                // 스킬 증가
                _skillManager.IncreaseSkillCount(_skillType, 1);
            
                // 재화 감소
                _inventoryManager.DecreaseCoin(cost);
            }
            else
            {
                _adsManager.ShowRewardedAd(
                    () => {
                    // 스킬 증가
                    _skillManager.IncreaseSkillCount(_skillType, 1);
                    }, 
                    () => {
                        Debug.LogError("OnFail ShowRewardedAd");
                    });               
            }
        }   
    }
}