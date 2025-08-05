using Extensions;
using FindPath;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

namespace Managers
{
    public class OutGameManager : MonoBehaviour, IBaseManager
    {
        [Inject] private readonly ObjectFactory _objectFactory;
        
        private GameObject _outGameRoot;
        private static readonly Vector2Int LeftBottomPosition = new Vector2Int(-2, -2);
        private static readonly Vector2Int LeftObstaclePosition = new Vector2Int(-2, 0);
        private static readonly Vector2Int RightObstaclePosition = new Vector2Int(2, 0);
        private const int OutGameMapSizeX = 5;
        private const int OutGameMapSizeY = 5;
        
        public void InitManager()
        {
            // CreateRoot();
            // CreateOutMap();
            // CreateOutPlayer();
        }

        private void CreateRoot()
        {
            if (_outGameRoot == null)
            {
                _outGameRoot = new GameObject("OutGameRoot");
            }
        }

        private void CreateOutPlayer()
        {
            var characterObj = _objectFactory.LoadGameObject(ObjectNames.OutCharacterPrefabName);
            characterObj.transform.position = Vector3.zero;
        }
        
        private void CreateOutMap()
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

                    var cellObj = _objectFactory.LoadCellGameObject(cellName);
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