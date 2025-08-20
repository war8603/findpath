using System.Collections.Generic;
using Extensions;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class CellInfo
    {
        public GridCell Cell;
        public GameObject Obj;
        public GameObject LightGameObject;
    }

    public class CoinInfo
    {
        public Vector2 Position;
        public GameObject Object;
    }
    
    public class MapManager
    {
        [Inject] private readonly DataManager _loadMap;
        [Inject] private readonly CustomObjectPool _objectPool;
        [Inject] private readonly InventoryManager _inventoryManager;
        
        private GridData _map;
        private CellInfo[,] _mapCellInfos;
        
        private GameObject _mapRoot;
        private GameObject _coinRoot;
        private float _playTimeOffset = 2f;

        private readonly HashSet<(int x, int y)> _obstacles = new();
        private readonly List<CoinInfo> _coinInfos = new();
        private (int x, int y) _endPosition;
        private GameObject _fog;

        private (int x, int y) _nullPosition = new(-1, -1);

        public void InitializeMap()
        {
            var gridData = _map;
            if (gridData != null)
            {
                _map = null;
            }

            if (_mapRoot == null)
            {
                _mapRoot = new GameObject("MapRoot");
            }

            ClearCellInfos();
            ClearCoinInfos();
            ClearObstacles();
            ClearFog();
        }

        private void ClearFog()
        {
            if (_fog != null)
            {
                _objectPool.ReleaseGameObject(_fog);
            }

            _fog = null;
        }

        private void ClearObstacles()
        {
            _obstacles.Clear();
        }

        private void ClearCellInfos()
        {
            if (_mapCellInfos != null)
            {
                foreach (var cell in _mapCellInfos)
                {
                    _objectPool.ReleaseGameObject(cell.Obj);
                    if (cell.LightGameObject != null)
                    {
                        _objectPool.ReleaseGameObject(cell.LightGameObject);
                    }
                }    
            }
        }

        private void ClearCoinInfos()
        {
            foreach (var coinInfo in _coinInfos)
            {
                _objectPool.ReleaseGameObject(coinInfo.Object);
            }
            _coinInfos.Clear();
        }

        public int GetGridIndex()
        {
            return _map.GridIndex;
        }

        public void CreateMap(int stageClearCount)
        {
            CreateGridMap(stageClearCount);
            CreateFog();
            CreateCoin();
        }

        private void CreateGridMap(int stageClearCount)
        {
            _map = _loadMap.LoadGridDataByStageClearCount(stageClearCount);
            _mapCellInfos = new CellInfo[_map.SizeX, _map.SizeY];
            
            foreach (var cell in _map.Cells)
            {
                var cellPosition = cell.Position;
                _mapCellInfos[cellPosition.x, cellPosition.y] = new CellInfo
                {
                    Cell = new GridCell()
                    {
                        Position = cell.Position,
                        IsObstacle = cell.IsObstacle,
                    }
                };

                if (_mapCellInfos[cellPosition.x, cellPosition.y].Cell.IsObstacle)
                {
                    _obstacles.Add((cellPosition.x, cellPosition.y));
                }
                
                // Cell Object 생성
                var cellObj = cell switch
                {
                    _ when cell.Position == _map.StartPosition => _objectPool.GetGameObject(ObjectNames.StartCellName, _mapRoot.transform),
                    _ when cell.Position == _map.EndPosition => _objectPool.GetGameObject(ObjectNames.EndCellName, _mapRoot.transform),
                    _ => _objectPool.GetGameObject(ObjectNames.NormalCellName, _mapRoot.transform),
                };
                cellObj.transform.position = (Vector2)cell.Position;
                _mapCellInfos[cell.Position.x, cell.Position.y].Obj = cellObj;
            }
        }

        private void CreateFog()
        {
            _fog = _objectPool.GetGameObject(ObjectNames.FogPrefabName, _mapRoot.transform);
            _fog.transform.localScale = new Vector3(_map.SizeX, _map.SizeY, 1);
            _fog.transform.position = new Vector3(_map.SizeX / 2f - 0.5f, _map.SizeY / 2f - 0.5f, 0);
        }

        private void CreateCoin()
        {
            if (_coinRoot == null)
            {
                _coinRoot = new GameObject("CoinRoot");
                _coinRoot.transform.InitParent(_mapRoot.transform);
            }
            
            var normalCells = new List<(int x, int y)>();
            for (var i = 0; i < _map.SizeX; i++)
            {
                for (var j = 0; j < _map.SizeY; j++)
                {
                    if (!_mapCellInfos[i, j].Cell.IsObstacle)
                    {
                        normalCells.Add((i, j));
                    }
                }
            }

            var coinCount = DataConfig.CoinCount;
            while (coinCount > 0)
            {
                var randomIndex = Random.Range(0, normalCells.Count);
                var cell = normalCells[randomIndex];
                if (_map.StartPosition.x == cell.x && _map.StartPosition.y == cell.y)
                {
                    continue;
                }

                if (_map.EndPosition.x == cell.x && _map.EndPosition.y == cell.y)
                {
                    continue;
                }
                
                coinCount--;
                var coin = _objectPool.GetGameObject(ObjectNames.CoinPrefabName, _coinRoot.transform);
                coin.transform.position = new Vector2(cell.x, cell.y);
                normalCells.RemoveAt(randomIndex);
                
                _coinInfos.Add(new CoinInfo()
                {
                    Position = new Vector2(cell.x, cell.y),
                    Object = coin,
                });
            }
        }

        public Vector2Int GetStartPosition()
        {
            if (_map == null)
            {
                Debug.LogError("MapManager not initialized");
                return Vector2Int.zero;
            }

            return _map.StartPosition;
        }

        public int GetTotalMovePathCount()
        {
            return _map?.PathCount ?? 0;
        }

        public int GetMinTurnCount()
        {
            return _map?.MinTurnCount ?? 0;
        }

        /// <summary>
        /// 다음 위치의 block type 리턴
        /// </summary>
        /// <param name="position"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public BlockType GetNextBlockType(Vector2 position, Vector2 characterSize)
        {
            var halfSize = characterSize / 2f;
            
            if (position.x - halfSize.x < -0.5f 
                || position.x + halfSize.x >= _map.SizeX - 1 + 0.5f 
                || position.y - halfSize.y < -0.5f
                || position.y + halfSize.y >= _map.SizeY - 1 + 0.5f)
            {
                return BlockType.Wall;
            }
            
            // Character Rect 생성
            var characterRect = GetCharacterSizeRect(position, characterSize);

            // 충돌된 장애물 확인
            var collisionObstacle = CollisionObstacle(characterRect);
            return collisionObstacle == _nullPosition ? BlockType.None : BlockType.Obstacle;
        }
        
        /// <summary>
        /// Rect와 충돌하는 Obstacle 리턴
        /// </summary>
        /// <param name="characterRect"></param>
        /// <returns></returns>
        private (int x, int y) CollisionObstacle(Rect characterRect)
        {
            foreach (var obstacle in _obstacles)
            {
                // 장애물 셀의 좌하단 좌표 = 중심(x, y) - 0.5
                var obstacleBottomLeft = new Vector2(obstacle.x - 0.5f, obstacle.y - 0.5f);
                var obstacleRect = new Rect(obstacleBottomLeft, Vector2.one);

                if (characterRect.Overlaps(obstacleRect))
                {
                    return (obstacle.x, obstacle.y);
                }
            }

            return _nullPosition;
        }

        /// <summary>
        /// character position을 바탕으로 Rect 생성
        /// </summary>
        /// <param name="position"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        private Rect GetCharacterSizeRect(Vector2 position, Vector2 characterSize)
        {
            var characterBottomLeft = position - characterSize / 2f;
            var characterRect = new Rect(characterBottomLeft, characterSize);
            return characterRect;
        }

        public bool IsArriveEndPosition(Vector2 position)
        {
            if (position.x > _map.EndPosition.x - 0.5f
                && position.y > _map.EndPosition.y - 0.5f
                && position.x < _map.EndPosition.x + 0.5f
                && position.y < _map.EndPosition.y + 0.5f)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 시야 확보
        /// </summary>
        /// <param name="position"></param>
        public void RevealFogAt(Vector2 position)
        {
            var positionToInt = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            CreateLight(positionToInt);
            ActivateSurroundingObstacles(positionToInt);
        }

        /// <summary>
        /// 해당 위치에 코인이 있을경우 획득
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TryCollectCoinAt(Vector2 position)
        {
            var positionToInt = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            var coinInfo = _coinInfos.Find(x => x.Position == positionToInt);
            if (coinInfo == null) return false;

            _objectPool.ReleaseGameObject(coinInfo.Object);
            _coinInfos.Remove(coinInfo);

            return true;
        }

        /// <summary>
        /// 조명 생성
        /// </summary>
        /// <param name="position"></param>
        private void CreateLight(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= _map.SizeX || position.y >= _map.SizeY)
            {
                Debug.LogError($"Position {position} is negative");
                return;
            }
            
            // 이미 생성되어있다면 리턴
            if (_mapCellInfos[position.x, position.y].LightGameObject != null) return;
            
            // 조명 생성
            var light = _objectPool.GetGameObject(ObjectNames.LightPrefabName, _mapRoot.transform);
            light.transform.position = (Vector2)position;
            _mapCellInfos[position.x, position.y].LightGameObject = light;
        }

        /// <summary>
        /// 장애물 활성화
        /// </summary>
        /// <param name="position"></param>
        private void ActivateSurroundingObstacles(Vector2Int position)
        {
            var directions = new []
            {
                new Vector2Int(-1, 0), // left
                new Vector2Int(1, 0), // right
                new Vector2Int(-1, 1), // leftTop
                new Vector2Int(0, 1), // top
                new Vector2Int(1, 1), // right top
                new Vector2Int(-1, -1), // left bottom
                new Vector2Int(0, -1), // bottom
                new Vector2Int(1, -1), // right bottom
            };

            foreach (var direction in directions)
            {
                var directionPos = position + direction;
                if (directionPos.x < 0 || directionPos.y < 0 || directionPos.x >= _map.SizeX ||
                    directionPos.y >= _map.SizeY)
                {
                    continue;
                }
                
                ActivateObstacleAt(directionPos);
            }
        }

        /// <summary>
        /// 장애물 활성화
        /// </summary>
        /// <param name="position"></param>
        private void ActivateObstacleAt(Vector2Int position)
        {
            if (!_mapCellInfos[position.x, position.y].Cell.IsObstacle) return;
                
            if (_mapCellInfos[position.x, position.y].Obj.name == ObjectNames.ObstacleCellName) return;
            
            _objectPool.ReleaseGameObject(_mapCellInfos[position.x, position.y].Obj);
            
            var cellObj = _objectPool.GetGameObject(ObjectNames.ObstacleCellName, _mapRoot.transform);
            _mapCellInfos[position.x, position.y].Obj = cellObj;
            cellObj.transform.position = (Vector2)position;
        }

        /// <summary>
        /// 모든 장애물 활성화
        /// </summary>
        public void ActivateAllObstacles()
        {
            for (var i = 0; i < _map.SizeX; i++)
            {
                for (var j = 0; j < _map.SizeY; j++)
                {
                    ActivateObstacleAt(new Vector2Int(i, j));
                }
            }
        }

        /// <summary>
        /// 장애물 제거
        /// </summary>
        /// <param name="position"></param>
        /// <param name="characterSize"></param>
        public void RemoveObstacleAt(Vector2 position, Vector2 characterSize)
        {
            var characterRect = GetCharacterSizeRect(position, characterSize);
            var collisionObstacle = CollisionObstacle(characterRect);
            if (collisionObstacle == _nullPosition) return;
            
            // 장애물 제거
            _mapCellInfos[collisionObstacle.x, collisionObstacle.y].Cell.IsObstacle = false;
            _obstacles.Remove(collisionObstacle);
            _objectPool.ReleaseGameObject(_mapCellInfos[collisionObstacle.x, collisionObstacle.y].Obj);

            // 일반 타일 교체
            var cellObj = _objectPool.GetGameObject(ObjectNames.NormalCellName, _mapRoot.transform);
            _mapCellInfos[collisionObstacle.x, collisionObstacle.y].Obj = cellObj;
            cellObj.transform.position = new Vector2(collisionObstacle.x, collisionObstacle.y);
        }

        public void HideFog()
        {
            _fog.SetActive(false);           
        }

        public void ShowFog()
        {
            _fog.SetActive(true);
        }
        
        public JumpSkillInfo GetJumpSkillInfoOverObstacle(Vector2 position, Vector2 characterSize, Vector2 direction)
        {
            // Character Rect 생성
            var characterRect = GetCharacterSizeRect(position, characterSize);
            
            // 충돌된 장애물 위치 확인
            var collisionObstacle = CollisionObstacle(characterRect);
            if (collisionObstacle == _nullPosition) return null;
            
            while (true)
            {
                collisionObstacle = (collisionObstacle.x + (int)direction.x, collisionObstacle.y + (int)direction.y);
                if (collisionObstacle.x < 0 || collisionObstacle.x >= _map.SizeX || collisionObstacle.y < 0 || collisionObstacle.y >= _map.SizeY)
                {
                    return null;
                }

                if (_mapCellInfos[collisionObstacle.x, collisionObstacle.y].Cell.IsObstacle) continue;

                return new JumpSkillInfo()
                {
                    Position = new Vector2Int(collisionObstacle.x, collisionObstacle.y)
                };
            }
        }
    }
}