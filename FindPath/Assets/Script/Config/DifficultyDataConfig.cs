using System.Collections.Generic;
using UnityEngine;

namespace FindPath
{
    [CreateAssetMenu(fileName = "DifficultyDataConfig", menuName = "Scriptable Objects/DifficultyDataConfig")]
    public class DifficultyDataConfig : ScriptableObject
    {
        [SerializeField] public List<DifficultyData> DifficultyDataList;
    }

    [System.Serializable]
    public class DifficultyData
    {
        public int MaxStageClearCount;
        public int MinTurnCount;
    }
}