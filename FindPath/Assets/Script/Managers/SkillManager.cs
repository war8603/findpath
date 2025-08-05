using System;
using System.Collections.Generic;
using Managers;
using UniRx;
using UnityEngine;

namespace FindPath
{
    public class SkillManager : MonoBehaviour, IBaseManager
    {
        private const int DefaultCount = 1;
        private Dictionary<SkillType, ReactiveProperty<int>> _skillCount = new();

        public void InitManager()
        {
            foreach (SkillType itemType in Enum.GetValues(typeof(SkillType)))
            {
                SetSkillCount(itemType, DefaultCount);
            }
        }
        
        public void SetSkillCount(SkillType skillType, int count)
        {
            if (_skillCount.ContainsKey(skillType))
            {
                _skillCount[skillType].SetValueAndForceNotify(count);
            }
            else
            {
                AddSkillCount(skillType, count);
            }
        }

        public void DecreaseSkillCount(SkillType skillType, int count)
        {
            if (_skillCount.TryGetValue(skillType, out var reactiveProperty))
            {
                reactiveProperty.Value -= count;
            }
        }
        
        public void IncreaseSkillCount(SkillType skillType, int count)
        {
            if (_skillCount.TryGetValue(skillType, out var property))
            {
                _skillCount[skillType].Value = property.Value + count;
            }
            else
            {
                AddSkillCount(skillType, count);
            }
        }

        public int GetSkillCount(SkillType skillType)
        {
            if (_skillCount.TryGetValue(skillType, out var property))
            {
                return property.Value;
            }
            
            AddSkillCount(skillType, DefaultCount);
            return 0;
        }
        
        private void AddSkillCount(SkillType skillType, int count)
        {
            _skillCount.Add(skillType, new ReactiveProperty<int>(count));            
        }
    }
}