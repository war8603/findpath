using Cysharp.Threading.Tasks;
using Extensions;
using FindPath;
using UnityEngine;
using VContainer;

namespace Managers
{
    public class OutGameManager : MonoBehaviour, IBaseManager
    {
        [Inject] private readonly CustomObjectPool _objectPool;
        [Inject] private readonly UIManager _uiManager;
        
        private GameObject _outGameRoot;
        private static readonly Vector2Int LeftBottomPosition = new Vector2Int(-2, -2);
        private static readonly Vector2Int LeftObstaclePosition = new Vector2Int(-2, 0);
        private static readonly Vector2Int RightObstaclePosition = new Vector2Int(2, 0);
        private const int OutGameMapSizeX = 5;
        private const int OutGameMapSizeY = 5;
        
        private UIOutGameView _outGameView;
        private GameObject _character;
        private MainCameraFollower _mainCameraFollower;
        
        public void InitManager()
        {
            
        }

        public void EnterOutGame()
        {
            InitBattleCamera();
            CreateRoot();
            CreateMap();
            CreateCharacter();
            CreateUIView().Forget();
        }

        public void ExitOutGame()
        {
            DestroyRoot();
            DestroyCharacter();
            DestroyView();
        }

        private void CreateRoot()
        {
            if (_outGameRoot == null)
            {
                _outGameRoot = new GameObject("OutGameRoot");
            }
        }

        private void DestroyRoot()
        {
            if (_outGameRoot == null) return;
            
            Destroy(_outGameRoot);
        }

        private void CreateCharacter()
        {
            if (_character != null)
            {
                DestroyCharacter();
            }
            _character = _objectPool.GetGameObject(ObjectNames.OutCharacterPrefabName);
            _character.transform.position = Vector3.zero;
        }
        
        private void InitBattleCamera()
        {
            if (_mainCameraFollower == null)
            {
                var cameraObj = GameObject.FindGameObjectWithTag(TagNames.MainCameraFollower);
                if (cameraObj == null)
                {
                    Debug.LogError("Not Found BattleCamera");
                    return;
                }
            
                if (!cameraObj.TryGetComponent<MainCameraFollower>(out var mainCameraFollower))
                {
                    Debug.LogError("Not Found BattleCamera Component");
                    return;
                }
            
                _mainCameraFollower = mainCameraFollower;    
            }

            _mainCameraFollower.InitPosition();
        }

        private void DestroyCharacter()
        {
            if (_character == null) return;
            
            Destroy(_character);   
        }

        private async UniTaskVoid CreateUIView()
        {
            _outGameView = await _uiManager.CreateView<UIOutGameView>(UIViewNames.UIOutGameView);
        }

        private void DestroyView()
        {
            if (_outGameView == null) return;
            
            _uiManager.DestroyView(_outGameView);
        }
        
        private void CreateMap()
        {
            var startPosition = LeftBottomPosition;
            for (var i = 0; i < OutGameMapSizeX; i++)
            {
                for (var j = 0; j < OutGameMapSizeY; j++)
                {
                    var cellPosition = (Vector2)startPosition + new Vector2(i, j);
                    
                    var cellName = ObjectNames.NormalCellName;
                    if (((int)cellPosition.x == LeftObstaclePosition.x && (int)cellPosition.y == LeftObstaclePosition.y) 
                        || ((int)cellPosition.x == RightObstaclePosition.x && (int)cellPosition.y == RightObstaclePosition.y))
                    {
                        cellName = ObjectNames.ObstacleCellName;
                    }

                    var cellObj = _objectPool.GetGameObject(cellName);
                    cellObj.InitParent(_outGameRoot.transform);
                    cellObj.transform.position = startPosition + new Vector2(i, j);
                }
            }
        }

        public bool IsWalkable(Vector2 position, Vector2 characterSize)
        {
            var halfSize = characterSize / 2f;
            
            if (position.x - halfSize.x < -OutGameMapSizeX / 2f 
                || position.x + halfSize.x >= OutGameMapSizeX / 2f 
                || position.y - halfSize.y < -OutGameMapSizeX / 2f
                || position.y + halfSize.y >= OutGameMapSizeX / 2f)
            {
                return false;
            }

            var characterBottomLeft = position - characterSize / 2f;
            var characterRect = new Rect(characterBottomLeft, characterSize);
            return !CollisionObstacle(LeftObstaclePosition, characterRect) &&
                   !CollisionObstacle(RightObstaclePosition, characterRect);
        }
        
        private bool CollisionObstacle(Vector2Int obstaclePosition, Rect characterRect)
        {
            var obstacleBottomLeft = new Vector2(obstaclePosition.x - 0.5f, obstaclePosition.y - 0.5f);
            var obstacleRect = new Rect(obstacleBottomLeft, Vector2.one);

            return characterRect.Overlaps(obstacleRect);
        }
    }
}