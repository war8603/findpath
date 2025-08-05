using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIBattleSkillButtonView : MonoBehaviour
    {
        [Inject] private readonly BattleManager _battleManager;
        [Inject] private readonly SkillManager _skillManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        
        [SerializeField] private SkillType skillType;
        [SerializeField] private Button _button;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private GameObject _notEnoughCover;
        [SerializeField] private Button _purchaseButton;

        private void Awake()
        {
            _button.onClick.AddListener(OnClickItemButton);
            _purchaseButton.onClick.AddListener(OnClickPurchaseButton);
            
            _notEnoughCover.SetActive(false);
            _purchaseButton.gameObject.SetActive(false);
        }
        
        public void SetData()
        {
            var coolTimeProperty = _battleManager.GetSkillCoolTimeProperty(skillType);
            coolTimeProperty.ObserveEveryValueChanged(prop => prop.Value).Subscribe(value =>
            {
                if (value > 0f)
                {
                    var coolTime = DataConfig.GetCoolTime(skillType);
                    _skillIcon.fillAmount = (coolTime - value) / coolTime;
                }
                else
                {
                    _skillIcon.fillAmount = 1f;
                    ShowNotEnoughCover();
                }
            });

            _inventoryManager.CoinProperty.ObserveEveryValueChanged(property => property.Value)
                .Subscribe(_ => ShowPurchaseButton()).AddTo(this);
        }

        private void ShowNotEnoughCover()
        {
            var isUsableSkill = _skillManager.GetSkillCount(skillType) > 0;
            _notEnoughCover.SetActive(!isUsableSkill);
            if (!isUsableSkill)
            {
                ShowPurchaseButton();
            }
        }

        private void ShowPurchaseButton()
        {
            var isUsableSkill = _skillManager.GetSkillCount(skillType) > 0;
            if (!isUsableSkill)
            {
                _purchaseButton.gameObject.SetActive(_battleManager.IsPurchasableSkill(skillType));    
            }
        }

        private void OnClickItemButton()
        {
            _battleManager.TryUseSkill(skillType);
        }

        private void OnClickPurchaseButton()
        {
            _battleManager.TryPurchaseSkill(skillType);
        }
    }
}