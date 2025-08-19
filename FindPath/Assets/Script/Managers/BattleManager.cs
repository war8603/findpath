using System.Threading;
using Cysharp.Threading.Tasks;
using GameUtilities;
using Managers;
using UniRx;
using UnityEngine;
using VContainer;

namespace FindPath
{
    public class BattleManager : MonoBehaviour, IBaseManager
    {
        [Inject] private readonly IObjectResolver _objectResolver;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly SkillManager _skillManager;
        [Inject] private readonly GameManager _gameManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly AdsManager _adsManager;
        
        private MapManager _mapManager;
        private CharacterManager _characterManager;
        private MainCameraFollower _mainCameraFollower;
        private UIBattleView _uiBattleView;
        
        // 게임 플레이시 체크
        public bool IsStarted => _isStarted.Value;
        private readonly ReactiveProperty<bool> _isStarted = new(false);
        
        public readonly ReactiveProperty<float> PlayingTime = new(0f);
        public readonly IntReactiveProperty RemainTurnCount = new(0);
        private int _stageClearCount;
        
        private CancellationTokenSource _cts = new();
        
        // 스킬
        private SkillController _skillController;

        public ReactiveProperty<int> CurrentScore { get; private set; } = new(0);
        
        public void InitManager()
        {
            
        }
        
        private void CreateWithInitMapManager()
        {
            _mapManager = new MapManager();
            _objectResolver.Inject(_mapManager);           
        }

        private void CreateWithInitCharacterManager()
        {
            _characterManager = new CharacterManager();
            _objectResolver.Inject(_characterManager);
            _characterManager.Init();
        }

        private void CreateSkillController()
        {
            var skillObj = new GameObject("SkillController");
            skillObj.transform.SetParent(transform);
            _skillController = skillObj.AddComponent<SkillController>();
            _objectResolver.Inject(_skillController);
            _skillController.SetCharacterManager(_characterManager);
            _skillController.SetMapManager(_mapManager);
            _skillController.Init();
        }

        /// <summary>
        /// 전투 관련 생성
        /// </summary>
        public void EnterBattle()
        {
            CreateWithInitMapManager();
            CreateWithInitCharacterManager();
            CreateSkillController();
            
            CurrentScore.Value = 0;
            _stageClearCount = 0;
            StartBattle().Forget();
        }

        private async UniTask StartBattle()
        {
            CreateMap();
            CreatePlayer();

            // 캐릭터 위치 초기화
            InitCharacterPosition();
            
            // UI생성
            await CreateBattleView();
            
            // 카메라 설정
            InitBattleCamera();
            
            // 게임 변수 설정
            InitGameParameter();
            
            // 게임 시작
            _isStarted.Value = true;
        }

        public void ExitBattle()
        {
            _mapManager?.InitializeMap();
            _characterManager?.Init();
            if (_mainCameraFollower != null)
            {
                _mainCameraFollower.InitPosition();    
            }
        }

        private void Update()
        {
            if (!_isStarted.Value) return;
            
            PlayingTime.Value -= Time.deltaTime;
            if (PlayingTime.Value <= 0)
            {
                GameOver();
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }

        /// <summary>
        /// 게임 클리어
        /// </summary>
        private void GameClear()
        {
            _stageClearCount++;
            _isStarted.Value = false;
            
            _uiManager.DestroyView(_uiBattleView);
            _uiBattleView = null;

            CalculateBattleScore();
            NextStage().Forget();
        }

        /// <summary>
        /// 스테이지 클리어 점수 정산
        /// </summary>
        private void CalculateBattleScore()
        {
            CurrentScore.Value += (int)PlayingTime.Value * DataConfig.TimeScore;
            CurrentScore.Value += DataConfig.StageClearScore;
        }

        /// <summary>
        /// 최고 점수 저장
        /// </summary>
        private void SaveBestScore()
        {
            var preScore = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.BestScore, 0);
            if (preScore < CurrentScore.Value)
            {
                PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.BestScore, CurrentScore.Value);
            }
        }

        private async UniTask NextStage()
        {
            _inventoryManager.SaveData();
            await ShowGameClearView();
            StartBattle().Forget();
        }

        /// <summary>
        /// 게임 오버
        /// </summary>
        private void GameOver()
        {
            _isStarted.Value = false;
            
            _uiManager.DestroyView(_uiBattleView);
            _uiBattleView = null;
            
            SaveBestScore();
            GoToOutGame().Forget();
        }

        private async UniTask GoToOutGame()
        {
            _inventoryManager.SaveData();
            await ShowGameOverView();
            _gameManager.StartOutGame();
            _adsManager.ShowRewardedAd(null, null);
        }

        private async UniTask ShowGameClearView()
        {
            var view = await _uiManager.CreateView<UIGameOverView>();
            await UniTask.WaitUntil(() => view == null, cancellationToken: _cts.Token);
        }

        private async UniTask ShowGameOverView()
        {
            var view = await _uiManager.CreateView<UIGameOverView>();
            await UniTask.WaitUntil(() => view == null, cancellationToken: _cts.Token);
        }

        /// <summary>
        /// 맵 생성
        /// </summary>
        private void CreateMap()
        {
            _mapManager.InitializeMap();
            _mapManager.CreateMap(_stageClearCount);
        }

        /// <summary>
        /// 게임 설정값 초기화
        /// </summary>
        private void InitGameParameter()
        {
            var pathCount = _mapManager.GetTotalMovePathCount();
            if (pathCount != 0)
            {
                PlayingTime.SetValueAndForceNotify(pathCount * DataConfig.PlayTimeOffset);
            }

            RemainTurnCount.SetValueAndForceNotify(Mathf.FloorToInt(_mapManager.GetMinTurnCount() * DataConfig.MinTurnCountOffset));
        }

        /// <summary>
        /// 캐릭터 생성
        /// </summary>
        private void CreatePlayer()
        {
            _characterManager.Init();
            _characterManager.CreateCharacter();
        }

        /// <summary>
        /// 캐릭터 위치 초기화
        /// </summary>
        private void InitCharacterPosition()
        {
            var startPosition = _mapManager.GetStartPosition();
            _characterManager.SetStartPosition(startPosition);
            
            _mapManager.RevealFogAt(startPosition);
        }

        /// <summary>
        /// 전투 메인 뷰 생성
        /// </summary>
        private async UniTask CreateBattleView()
        {
            _uiBattleView = await _uiManager.CreateView<UIBattleView>(UIViewNames.UIBattleView);
            _uiBattleView.SetData(OnClickArrowButton);
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
            
                if (!cameraObj.TryGetComponent<MainCameraFollower>(out var battleCamera))
                {
                    Debug.LogError("Not Found BattleCamera Component");
                    return;
                }
            
                _mainCameraFollower = battleCamera;    
            }
            
            _mainCameraFollower.SetTarget(_characterManager.GetCharacterTransform());
        }

        private void OnClickArrowButton(Vector2Int direction)
        {
            _characterManager.SetDirection(direction);
        }

        public BlockType GetNextBlockType(Vector2 position, Vector2 characterSize)
        {
            return _mapManager.GetNextBlockType(position, characterSize);
        }

        public void AddTurnCount()
        {
            RemainTurnCount.Value -= 1;
            if (RemainTurnCount.Value < 0)
            {
                GameOver();
            }
        }

        public void CheckGameClear(Vector2 characterPosition)
        {
            if (_mapManager.IsArriveEndPosition(characterPosition))
            {
                GameClear();
            }
        }

        /// <summary>
        /// 시야 확보
        /// </summary>
        /// <param name="position"></param>
        public void RevealFogAt(Vector2 position)
        {
            _mapManager.RevealFogAt(position);
        }

        public void CollectCoinAt(Vector2 position)
        {
            if (!_mapManager.TryCollectCoinAt(position)) return;
            
            _inventoryManager.IncreaseCoins(1);
            CurrentScore.Value += DataConfig.CoinScore;
        }

        public void TryUseSkill(SkillType skillType)
        {
            if (!IsStarted) return;
            
            _skillController.TryUseSkill(skillType);
        }

        public bool IsSkillActive(SkillType skillType)
        {
            return _skillController.IsSkillActive(skillType);
        }

        public ReactiveProperty<float> GetSkillCoolTimeProperty(SkillType skillType)
        {
            return _skillController.GetSkillCoolTimeProperty(skillType);
        }

        /// <summary>
        /// 현재 위치에서 direction방향으로 장애물을 넘어선 다음 타일의 위치를 가져오기
        /// </summary>
        /// <param name="position"></param>
        /// <param name="characterSize"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public JumpSkillInfo GetJumpSkillInfoOverObstacle(Vector2 position, Vector2 characterSize, Vector2 direction)
        {
            return _mapManager.GetJumpSkillInfoOverObstacle(position, characterSize, direction);
        }

        /// <summary>
        /// 장애물 제거
        /// </summary>
        /// <param name="position"></param>
        /// <param name="characterSize"></param>
        public void RemoveObstacleAt(Vector2 position, Vector2 characterSize)
        {
            _mapManager.RemoveObstacleAt(position, characterSize);
        }
    }
}