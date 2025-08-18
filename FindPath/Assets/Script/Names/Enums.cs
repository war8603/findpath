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
    }
}