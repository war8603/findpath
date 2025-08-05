using FindPath;
using UnityEngine;
using VContainer;

namespace Managers
{
    public class GameManager : MonoBehaviour, IBaseManager 
    {
        [Inject] private readonly BattleManager _battleManager;
        [Inject] private readonly OutGameManager _outGameManager;
        
        public void InitManager()
        {
            
        }
        
        public void StartGame()
        {
            StartBattle();
        }

        public void StartBattle()
        {
            _battleManager.CreateBattle();
        }
    }
}