using System;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class Character : MonoBehaviour
    {
        private enum MoveType
        {
            Walk,
            Jump,
        }
        
        [Inject] private readonly BattleManager _battleManager;

        [SerializeField] private float _walkSpeed;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider2D _collider;

        private Vector2 _direction;
        private float _defaultWalkSpeed;
        private MoveType _moveType;

        private void Awake()
        {
            _defaultWalkSpeed = _walkSpeed;
            _moveType = MoveType.Walk;
        }
        
        public void SetStartPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void Update()
        {
            if (!_battleManager.IsStarted) return;
            if (_direction == Vector2.zero) return;
            
            if (_moveType == MoveType.Walk)
            {
                DoWalk();    
            }
        }

        private void DoWalk()
        {
            var nextPosition = transform.position + (Vector3)_direction * _walkSpeed * Time.deltaTime;
            var nextBlockType = _battleManager.GetNextBlockType(nextPosition, _collider.bounds.size);
            
            switch (nextBlockType)
            {
                // 이동
                case BlockType.None:
                    MoveToPosition(nextPosition);
                    return;
                // 벽 충돌 - 방향 전환
                case BlockType.Wall:
                    ChangeDirection();
                    return;
            }

            // 장애물 파괴 스킬 or 점프 스킬 발동중이 아니라면 방향 전환
            if (!_battleManager.IsSkillActive(SkillType.RemoveObstacle)
                && !_battleManager.IsSkillActive(SkillType.Jump))
            {
                ChangeDirection();
                return;
            }
            
            if (_battleManager.IsSkillActive(SkillType.RemoveObstacle))
            {
                // 장애물 파괴 및 캐릭터 이동
                _battleManager.RemoveObstacleAt(nextPosition, _collider.bounds.size);
                MoveToPosition(nextPosition);
            }
            else if (_battleManager.IsSkillActive(SkillType.Jump))
            {
                var jumpSkillInfo = _battleManager.GetJumpSkillInfoOverObstacle(nextPosition, _collider.bounds.size, _direction);
                if (jumpSkillInfo == null)
                {
                    ChangeDirection();    
                }
                else
                {
                    DoJump(jumpSkillInfo);
                }
            }
        }

        /// <summary>
        /// 캐릭터 이동
        /// </summary>
        /// <param name="position"></param>
        private void MoveToPosition(Vector3 position)
        {
            transform.position = position;
            
            // 게임 종료 체크
            CheckGameClear(position);
            
            // 시야 확보
            RevealFogAt(position);
            
            // 코인 획득
            GetCoin(position);
        }

        /// <summary>
        /// 점프
        /// </summary>
        /// <param name="jumpSkillInfo"></param>
        private void DoJump(JumpSkillInfo jumpSkillInfo)
        {
            _moveType = MoveType.Jump;
            transform.DOJump((Vector2)jumpSkillInfo.Position, 0.5f, 1, 1 / _walkSpeed)
                .OnComplete(() => _moveType = MoveType.Walk);
        }

        /// <summary>
        /// 방향 전환
        /// </summary>
        private void ChangeDirection()
        {
            _direction *= -1;
            SetFlipX();
        }

        /// <summary>
        /// 캐릭터 이미지 반전
        /// </summary>
        private void SetFlipX()
        {
            if (_direction.x != 0)
            {
                _spriteRenderer.flipX = !(_direction.x > 0);    
            }    
        }

        /// <summary>
        /// 시야 확보
        /// </summary>
        /// <param name="position"></param>
        private void RevealFogAt(Vector2 position)
        {
            _battleManager.RevealFogAt(position);
        }

        private void GetCoin(Vector2 position)
        {
            _battleManager.CollectCoinAt(position);
        }

        private void CheckGameClear(Vector3 position)
        {
            _battleManager.CheckGameClear(position);
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction == _direction || direction * -1 == _direction ) return;
            
            _battleManager.AddTurnCount();
            _direction = direction;
            SetFlipX();
        }

        public void DecreaseWalkSpeed(float ratio)
        {
            _walkSpeed *= ratio;
        }

        public void ResetWalkSpeed()
        {
            _walkSpeed = _defaultWalkSpeed;
        }
    }
}