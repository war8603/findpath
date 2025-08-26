using System;
using System.Collections.Generic;
using GameUtilities;
using Managers;
using UniRx;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class SkillManager : MonoBehaviour, IBaseManager
    {
        [Inject] private readonly ObjectFactory _objectFactory;
        
        private const int DefaultCount = 1;
        private Dictionary<SkillType, ReactiveProperty<int>> _skillCount = new();

        public SkillConfig SkillConfig { get; private set; }

        public void InitManager()
        {
            // 스킬 카운트 초기화
            foreach (SkillType itemType in Enum.GetValues(typeof(SkillType)))
            {
                var count = PlayerPrefsTool.GetPlayerPrefs(itemType.ToString(), DefaultCount);
                _skillCount.Add(itemType, new ReactiveProperty<int>(count));
            }
            
            InitSkillConfig();
        }
        
        /// <summary>
        /// SkillConfig 초기화
        /// </summary>
        private void InitSkillConfig()
        {
            SkillConfig = _objectFactory.LoadResource<SkillConfig>(ObjectNames.ConfigPath, ObjectNames.SkillConfig);
        }

        /// <summary>
        /// 스킬 사용 후 스킬카운트 감소
        /// </summary>
        /// <param name="skillType"></param>
        /// <param name="count"></param>
        public void DecreaseSkillCount(SkillType skillType, int count)
        {
            if (!_skillCount.TryGetValue(skillType, out var reactiveProperty)) return;
            
            reactiveProperty.Value -= count;
            SaveSkillCount(skillType);
        }
        
        /// <summary>
        /// 스킬 구매 후 스킬 카운트 증가
        /// </summary>
        /// <param name="skillType"></param>
        /// <param name="count"></param>
        public void IncreaseSkillCount(SkillType skillType, int count)
        {
            if (!_skillCount.TryGetValue(skillType, out var property)) return;
            
            _skillCount[skillType].Value = property.Value + count;
            SaveSkillCount(skillType);
        }

        public int GetSkillCount(SkillType skillType)
        {
            return _skillCount[skillType].Value;
        }

        public ReactiveProperty<int> GetSkillCountProperty(SkillType skillType)
        {
            return _skillCount[skillType];
        }

        private void SaveSkillCount(SkillType skillType)
        {
            PlayerPrefsTool.SetPlayerPrefs(skillType.ToString(), _skillCount[skillType].Value);
            PlayerPrefs.Save();
        }
    }
}