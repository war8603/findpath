using System.Collections.Generic;
using UnityEngine;

namespace FindPath
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "Scriptable Objects/SkillConfig")]
    public class SkillConfig : ScriptableObject
    {
        [SerializeField] private List<SkillConfigData> _skillConfigData = new();

        private SkillConfigData GetSkillConfigData(SkillType skillType)
        {
            var skillConfigData = _skillConfigData.Find(x => x.SkillType == skillType);
            if (skillConfigData != null) return skillConfigData;
            
            Debug.LogError($"Not Found SkillConfigData {skillType}");
            return null;
        }
        
        public int GetMaxCount(SkillType skillType)
        {
            return GetSkillConfigData(skillType).MaxCount;
        }

        public CostType GetCostType(SkillType skillType)
        {
            return GetSkillConfigData(skillType).CostType;
        }
        
        public int GetCost(SkillType skillType)
        {
            return GetSkillConfigData(skillType).PurchaseCost;
        }
        
        public float GetCoolTime(SkillType skillType)
        {
            return GetSkillConfigData(skillType).CoolTime;
        }

        public int GetDefaultCount(SkillType skillType)
        {
            return GetSkillConfigData(skillType).DefaultCount;
        }
    }

    [System.Serializable]
    public class SkillConfigData
    {
        public SkillType SkillType;
        public CostType CostType;
        public int DefaultCount;
        public int MaxCount;
        public int PurchaseCost;
        public int CoolTime;
    }
}