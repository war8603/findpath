using System.Threading;
using Cysharp.Threading.Tasks;
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
        
        private MapManager _mapManager;
        private CharacterManager _characterManager;
        private BattleCamera _battleCamera;
        private UIBattleView _uiBattleView;
        
        // 게임 플레이시 체크
        public bool IsStarted => _isStarted.Value;
        private readonly ReactiveProperty<bool> _isStarted = new(false);
        
        public bool IsPlaying => _isPlaying.Value;
        private readonly ReactiveProperty<bool> _isPlaying = new(false);
        
        public readonly ReactiveProperty<float> PlayingTime = new(0f);
        public readonly IntReactiveProperty RemainTurnCount = new(0);
        public readonly ReactiveProperty<int> StageClearCount = new(0);
        
        private CancellationTokenSource _cts = new();
        
        // 스킬
        private SkillController _skillController;
        
        // ConfigValue
        private const float MinTurnCountOffset = 2f;
        private const float PlayTimeOffset = 3f;
        
        /// <summary>
        /// 매니저 클래스 등을 생성 및 초기화
        /// </summary>
        public void InitManager()
        {
            CreateWithInitMapManager();
            CreateWithInitCharacterManager();
            CreateSkillController();
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
        public void CreateBattle()
        {
            CreateMap();
            CreatePlayer();

            // 캐릭터 위치 초기화
            InitCharacterPosition();
            
            // UI생성
            CreateBattleView();
            
            // 카메라 설정
            InitBattleCamera();
            
            // 게임 변수 설정
            InitGameParameter();
            
            // 게임 시작
            StartBattle();
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        private void StartBattle()
        {
            _isStarted.Value = true;
        }

        private void Update()
        {
            if (!_isStarted.Value) return;
            if (!_isPlaying.Value) return;
            
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
            _isStarted.Value = false;
            _isPlaying.Value = false;
            
            _uiManager.DestroyView(_uiBattleView);
            _uiBattleView = null;
            
            StageClearCount.Value += 1;
            RestartGame().Forget();
        }

        /// <summary>
        /// 게임 오버
        /// </summary>
        private void GameOver()
        {
            return;
            
            _isStarted.Value = false;
            _isPlaying.Value = false;
            
            _uiManager.DestroyView(_uiBattleView);
            _uiBattleView = null;
            
            RestartGame().Forget();
        }

        private async UniTask RestartGame()
        {
            await ShowGameOverView();
            CreateBattle();
        }

        private async UniTask ShowGameOverView()
        {
            var view = _uiManager.CreateView<UIGameOverView>();
            await UniTask.WaitUntil(() => view == null, cancellationToken: _cts.Token);
        }

        /// <summary>
        /// 맵 생성
        /// </summary>
        private void CreateMap()
        {
            _mapManager.Init();
            _mapManager.CreateMap();
        }

        /// <summary>
        /// 게임 설정값 초기화
        /// </summary>
        private void InitGameParameter()
        {
            var pathCount = _mapManager.GetTotalMovePathCount();
            if (pathCount != 0)
            {
                PlayingTime.SetValueAndForceNotify(pathCount * PlayTimeOffset);
            }

            RemainTurnCount.SetValueAndForceNotify(Mathf.FloorToInt(_mapManager.GetMinTurnCount() * MinTurnCountOffset));
            
            StageClearCount.SetValueAndForceNotify(0);
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
        private void CreateBattleView()
        {
            _uiBattleView = _uiManager.CreateView<UIBattleView>(UIViewNames.UIBattleView);
            _uiBattleView.SetData(OnClickArrowButton);
        }

        private void InitBattleCamera()
        {
            if (_battleCamera == null)
            {
                var cameraObj = GameObject.FindGameObjectWithTag(TagNames.BattleCamera);
                if (cameraObj == null)
                {
                    Debug.LogError("Not Found BattleCamera");
                    return;
                }
            
                if (!cameraObj.TryGetComponent<BattleCamera>(out var battleCamera))
                {
                    Debug.LogError("Not Found BattleCamera Component");
                    return;
                }
            
                _battleCamera = battleCamera;    
            }
            
            _battleCamera.SetTarget(_characterManager.GetCharacterTransform());
        }

        private void OnClickArrowButton(Vector2Int direction)
        {
            _isPlaying.Value = true;
            _characterManager.SetDirection(direction);
        }

        public BlockType GetNextBlockType(Vector2 position, Vector2 characterSize)
        {
            return _mapManager.GetNextBlockType(position, characterSize);
        }

        public void AddTurnCount()
        {
            RemainTurnCount.Value -= 1;
            if (RemainTurnCount.Value <= 0)
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
            _mapManager.CollectCoinAt(position);
        }

        public void TryUseSkill(SkillType skillType)
        {
            if (!IsStarted) return;
            if (!IsPlaying) return;
            
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
        
        /// <summary>
        /// 스킬 구매 가능한지 확인
        /// </summary>
        /// <returns></returns>
        public bool IsPurchasableSkill(SkillType skillType)
        {
            return _skillController.IsPurchasableSkill(skillType);
        }

        public void TryPurchaseSkill(SkillType skillType)
        {
            _skillController.TryPurchaseSkill(skillType);
        }
    }
}