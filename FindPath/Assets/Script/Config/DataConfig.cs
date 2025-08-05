using System;
using UnityEngine;

namespace FindPath
{
    public static class DataConfig
    {
        private const float RevealFogSkillCoolTime = 5f; // 전체맵 보기 아이템 유지 시간
        private const float DecreaseWalkSpeedSkillCoolTime = 5f; // 캐릭터 이동 속도 감소 아이템 유지 시간
        private const float RemoveObstacleSkillCoolTime = 5f;
        private const float JumpSkillCoolTime = 20f;

        private const int DefaultCountRevealFog = 1;
        private const int DefaultCountDecreaseWalkSpeed = 1;
        private const int DefaultCountRemoveObstacle = 1;
        private const int DefaultCountJump = 1;

        public const int RevealFogSkillPurchaseCost = 5;
        public const int DecreaseWalkSpeedSkillPurchaseCost = 2;
        public const int RemoveObstacleSkillPurchaseCost = 10;
        public const int JumpSkillPurchaseCost = 8;
        
        public const int CoinCount = 5;

        public static float GetCoolTime(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.RevealFog => RevealFogSkillCoolTime,
                SkillType.RemoveObstacle => RemoveObstacleSkillCoolTime,
                SkillType.Jump => JumpSkillCoolTime,
                SkillType.DecreaseWalkSpeed => DecreaseWalkSpeedSkillCoolTime,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }

        public static int GetCost(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Jump => DataConfig.JumpSkillPurchaseCost,
                SkillType.DecreaseWalkSpeed => DataConfig.DecreaseWalkSpeedSkillPurchaseCost,
                SkillType.RemoveObstacle => DataConfig.RemoveObstacleSkillPurchaseCost,
                SkillType.RevealFog => DataConfig.RevealFogSkillPurchaseCost,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }

        public static int GetDefaultCount(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.RevealFog => DefaultCountRevealFog,
                SkillType.RemoveObstacle => DefaultCountRemoveObstacle,
                SkillType.Jump => DefaultCountJump,
                SkillType.DecreaseWalkSpeed => DefaultCountDecreaseWalkSpeed,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }
    }
}