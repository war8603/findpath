using UnityEngine;
using VContainer;

namespace FindPath
{
    public class CharacterManager
    {
        [Inject] private readonly CustomObjectPool _objectPool;
        private Character _character;
        
        public void Init()
        {
            if (_character != null)
            {
                Object.Destroy(_character.gameObject);
            }

            _character = null;
        }

        public Transform GetCharacterTransform()
        {
            if (_character == null)
            {
                Debug.LogError("Not found character");
                return null;
            }
            return _character.transform;
        }

        public void CreateCharacter()
        {
            var obj = _objectPool.GetGameObject(ObjectNames.CharacterPrefabName);
            if (!obj.TryGetComponent<Character>(out var character))
            {
                Debug.LogError("Character object not found");
                return;
            }
            
            _character = character;
        }

        public void SetStartPosition(Vector2Int position)
        {
            _character.SetStartPosition(position);
        }

        /// <summary>
        /// 방향 전환
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(Vector2Int direction)
        {
            _character.SetDirection(direction);
        }
        
        /// <summary>
        /// 이동속도 감속
        /// </summary>
        /// <param name="ratio"></param>
        public void DecreaseWalkSpeed(float ratio)
        {
            _character.DecreaseWalkSpeed(ratio);
        }

        /// <summary>
        /// 이동속도 초기화
        /// </summary>
        public void ResetWalkSpeed()
        {
            _character.ResetWalkSpeed();
        }
    }
}