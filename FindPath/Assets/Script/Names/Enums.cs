using UnityEngine;

namespace FindPath
{
    public enum SkillType
    {
        RevealFog,
        RemoveObstacle,
        Jump,
        DecreaseWalkSpeed,
    }

    public enum BlockType
    {
        None,
        Wall,
        Obstacle,
    }

    public enum CostType
    {
        Coin,
        Ads,
        Diamond,
    }

    public enum CellType
    {
        Start,
        End,
        Obstacle,
        Normal,
    }

    public enum SoundType
    {
        BGM,
        SE,
    }

    public enum CharacterType
    {
        Cat,
        Dog,
        Capybara,
    }
}