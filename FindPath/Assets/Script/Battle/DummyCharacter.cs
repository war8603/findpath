using System;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class DummyCharacter : MonoBehaviour
    {
        [Inject] private readonly BattleManager _battleManager;

        [SerializeField] public float MoveSpeed;

        // 다음에 도착할 위치
        private Vector2Int _nextPositionToInt;
        
        // 현재 진행 방향
        private Vector2Int _currentDirectionToInt = Vector2Int.zero;
        
        // 다음에 도착할 위치에 도착한 후 적용할 방향
        private Vector2Int _nextDirectionToInt = Vector2Int.zero;
        //
        // public void SetStartPosition(Vector2 position)
        // {
        //     transform.position = position;
        // }
        //
        // public void Update()
        // {
        //     if (_currentDirectionToInt == Vector2.zero) return;
        //     DoMove();
        // }
        //
        // private void DoMove()
        // {
        //     var newPosition = transform.position + (Vector3)((Vector2)_currentDirectionToInt * MoveSpeed * Time.deltaTime);
        //     transform.position = newPosition;
        //
        //     // 다음 목적지에 도착한 경우
        //     if (Vector2.Distance(_nextPositionToInt, newPosition) < 0.1f)
        //     {
        //         // 다음 진행 방향이 존재하고, 다음 진행 방향이 현재 진행방향과 다르면.
        //         if (_nextDirectionToInt != Vector2Int.zero && _nextDirectionToInt != _currentDirectionToInt)
        //         {
        //             transform.position = (Vector2)_nextPositionToInt;
        //             
        //             // 다음 진행방향으로 갈 경우 막혀있는지 체크
        //             var newNextPosition = _nextPositionToInt + _nextDirectionToInt;
        //             if (_battleManager.IsMovablePosition(newNextPosition))
        //             {
        //                 // 다음 진행방향으로 갈 수 있으면 갱신
        //                 _currentDirectionToInt = _nextDirectionToInt;
        //                 _nextPositionToInt = newNextPosition;
        //                 _battleManager.AddTurnCount();
        //             }
        //             
        //             _nextDirectionToInt = Vector2Int.zero;
        //         }
        //         else
        //         {
        //             // 만약 현재 진행하고 있는 방향으로 더이상 가지 못한다면 방향 전환
        //             if (!_battleManager.IsMovablePosition(_nextPositionToInt + _currentDirectionToInt))
        //             {
        //                 _currentDirectionToInt *= -1;
        //                 transform.position = (Vector2)_nextPositionToInt;
        //             }
        //
        //             _nextPositionToInt += _currentDirectionToInt;
        //         }
        //     }
        // }
        //
        // public void SetDirection(Vector2Int direction)
        // {
        //     var currentPositionToInt = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        //     if (!_battleManager.IsMovablePosition(currentPositionToInt + direction))
        //     {
        //         return;
        //     }
        //
        //     if (_currentDirectionToInt == Vector2Int.zero)
        //     { 
        //         // 진행 방향이 없을경우 바로 반영
        //         _currentDirectionToInt = direction;
        //         _nextPositionToInt = currentPositionToInt + direction;
        //     }
        //     else
        //     {
        //         // 진행 방향과 같거나, 진행 방향의 반대방향이면 기존의 다음 방향을 삭제 후 리턴
        //         if (_currentDirectionToInt == direction || _currentDirectionToInt * -1 == direction)
        //         {
        //             _nextDirectionToInt = Vector2Int.zero;
        //             return;
        //         }
        //         
        //         // 목적지보다 출발지와 가깝다면 (방향 변경), (목적지 변경), (현재위치 변경)
        //         if (currentPositionToInt != _nextPositionToInt)
        //         {
        //             _currentDirectionToInt = direction;
        //             _nextPositionToInt = currentPositionToInt + direction;
        //             _nextDirectionToInt = Vector2Int.zero;
        //             
        //             transform.position = (Vector2)currentPositionToInt;    
        //         }
        //         // 목적지에 가깝다면 도착 후 방향 변경
        //         else
        //         {
        //             _nextDirectionToInt = direction;
        //         }
        //     }
        //}
    }
}