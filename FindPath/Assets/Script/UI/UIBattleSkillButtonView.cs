using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIBattleSkillButtonView : MonoBehaviour
    {
        [Inject] private readonly BattleManager _battleManager;
        [Inject] private readonly SkillManager _skillManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        
        [FormerlySerializedAs("skillType")] [SerializeField] private SkillType _skillType;
        [SerializeField] private Button _button;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private GameObject _notEnoughCover;
        [SerializeField] private TMP_Text _currentSkillCount;

        private void Awake()
        {
            _button.onClick.AddListener(OnClickItemButton);
        }
        
        public void SetData()
        {
            ShowNotEnoughCover();
            SubscribeSkillCoolTime();
            SubscribeSkillCount();
            InitSkillConfig();
        }
        
        private void SubscribeSkillCount()
        {
            var maxCount = _skillManager.SkillConfig.GetMaxCount(_skillType);
           
            _skillManager.GetSkillCountProperty(_skillType).ObserveEveryValueChanged(x => x.Value).
                Subscribe(value =>
                {
                    _currentSkillCount.text = $"({value}/{maxCount})";
                    _currentSkillCount.color = value > 0 ? Color.white : Color.red;
                }).AddTo(this);
        }

        private void InitSkillConfig()
        {
            
        }

        private void SubscribeSkillCoolTime()
        {
            var coolTimeProperty = _battleManager.GetSkillCoolTimeProperty(_skillType);
            coolTimeProperty.ObserveEveryValueChanged(prop => prop.Value).Subscribe(value =>
            {
                if (value > 0f)
                {
                    var coolTime = _skillManager.SkillConfig.GetCoolTime(_skillType);
                    _skillIcon.fillAmount = (coolTime - value) / coolTime;
                }
                else
                {
                    _skillIcon.fillAmount = 1f;
                    ShowNotEnoughCover();
                }
            }).AddTo(this);
        }

        private void ShowNotEnoughCover()
        {
            var isUsableSkill = _skillManager.GetSkillCount(_skillType) > 0;
            _notEnoughCover.SetActive(!isUsableSkill);
        }

        private void OnClickItemButton()
        {
            _battleManager.TryUseSkill(_skillType);
        }
    }
}