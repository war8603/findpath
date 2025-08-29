using UnityEngine;

namespace FindPath
{
    public static class DataConfig
    {
        public const int CoinCount = 5;
        
        public const int CoinScore = 50;
        public const int TimeScore = 1;
        public const int StageClearScore = 100;
        
        public const float MinTurnCountOffset = 3f;
        public const float PlayTimeOffset = 2f;

        public const int CharacterCost = 50;
        
        // BGM
        public const string OutGameBGMName = "outBgm";
        public const string BattleGameBGMName = "battleBgm";
        
        // SE
        public const string CollectCoinSEName = "getCoin";
        public const string JumpSkillSEName = "jump1";
        public const string GameOverSEName = "game_over";
        public const string GameClearSEName = "game_clear";
        public const string ButtonClickSoundSEName = "ui_click_heavy";

        public static string GetCharacterIdleName(CharacterType characterType)
        {
            return "Character_" + characterType + "_Idle";
        }

        public static string GetCharacterAnimatorName(CharacterType characterType)
        {
            return "Animator_" + characterType;
        }

        public static string GetCharacterIconName(CharacterType characterType)
        {
            return "Character_" + characterType;
        }
    }
}