using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class SkillController : MonoBehaviour
    {
        [Inject] private readonly BattleManager _battleManager;
        [Inject] private readonly SkillManager _skillManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        
        private const float MoveSpeedRatio = 0.5f; // 캐릭터 이동 속도 감소 아이템 사용시 변경 속도
        
        // 아이템 사용시 유지되는 시간 및 사용중 체크
        private Dictionary<SkillType, ReactiveProperty<float>> _usedSkillDuration = new();
       
        private CharacterManager _characterManager;
        private MapManager _mapManager;
        
        private readonly Dictionary<SkillType, ReactiveProperty<int>> _skillPurchaseCount = new();

        public void Init()
        {
            InitSkillData();
        }

        private void InitSkillData()
        {
            foreach (SkillType skillType in Enum.GetValues(typeof(SkillType)))
            {
                // 스킬 사용 시간 초기화
                if (!_usedSkillDuration.TryAdd(skillType, new ReactiveProperty<float>(0f)))
                {
                    _usedSkillDuration[skillType].Value = 0f;
                }

                // 스킬 구매 횟수 초기화
                if (!_skillPurchaseCount.TryAdd(skillType, new ReactiveProperty<int>(_skillManager.SkillConfig.GetDefaultCount(skillType))))
                {
                    _skillPurchaseCount[skillType].Value = _skillManager.SkillConfig.GetDefaultCount(skillType);
                }
            }
        }
        
        private void Update()
        {
            if (!_battleManager.IsStarted) return;

            UpdateSkillDuration();
        }

        public void SetCharacterManager(CharacterManager characterManager)
        {
            _characterManager = characterManager;
        }

        public void SetMapManager(MapManager mapManager)
        {
            _mapManager = mapManager;
        }
        
        private void UpdateSkillDuration()
        {
            if (_usedSkillDuration.Count <= 0) return;
            
            foreach (var usedSkillDuration in _usedSkillDuration)
            {
                if (usedSkillDuration.Value.Value <= 0f) continue;
                
                usedSkillDuration.Value.Value -= Time.deltaTime;
                if (usedSkillDuration.Value.Value <= 0f)
                {
                    switch (usedSkillDuration.Key)
                    {
                        case SkillType.RevealFog:
                            _mapManager.ShowFog();
                            break;
                        case SkillType.DecreaseWalkSpeed:
                            _characterManager.ResetWalkSpeed();
                            break;
                        case SkillType.Jump:
                        case SkillType.RemoveObstacle:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        
        public void TryUseSkill(SkillType skillType)
        {
            // 현재 스킬을 사용중이라면 리턴
            if (_usedSkillDuration[skillType].Value > 0f)
            {
                return;
            }
            
            // 사용 횟수가 없으면 리턴
            if (_skillManager.GetSkillCount(skillType) <= 0)
            {
                return;
            }
            
            // 사용 처리
            _skillManager.DecreaseSkillCount(skillType,1);
            _usedSkillDuration[skillType].Value = _skillManager.SkillConfig.GetCoolTime(skillType);
            switch (skillType)
            {
                case SkillType.RevealFog:
                    _mapManager.HideFog();
                    _mapManager.ActivateAllObstacles();
                    break;
                case SkillType.DecreaseWalkSpeed:
                    _characterManager.DecreaseWalkSpeed(MoveSpeedRatio);
                    break;
                case SkillType.RemoveObstacle:
                case SkillType.Jump:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }

        public bool IsSkillActive(SkillType skillType)
        {
            return _usedSkillDuration[skillType].Value > 0f;
        }

        public ReactiveProperty<float> GetSkillCoolTimeProperty(SkillType skillType)
        {
            return _usedSkillDuration[skillType];
        }
    }
}