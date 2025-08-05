using UnityEngine;

namespace FindPath
{
    public class ObjectCollider : MonoBehaviour
    {
        [SerializeField] private BattleObject _battleObject;

        public BattleObject GetBattleObject()
        {
            return _battleObject;
        }
    }
}