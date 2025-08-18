using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace FindPath
{
    public class UIBattleView : UIMainView
    {
        [Inject] private readonly BattleManager _battleManager;
        [Inject] private readonly InventoryManager _inventoryManager;
        [Inject] private readonly SkillManager _skillManager;
        
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        [SerializeField] private TMP_Text _coinText;
        [SerializeField] private TMP_Text _playTimeText;
        [SerializeField] private TMP_Text _remainTurnCount;
        [SerializeField] private TMP_Text _scoreText;

        [SerializeField] private List<UIBattleSkillButtonView> _skillButtonViews;

        private Action<Vector2Int> _onclickArrowButtonCallback;

        private void Awake()
        {
            _upButton.onClick.AddListener(() => OnClickArrowButton(new Vector2Int(0, 1)));
            _downButton.onClick.AddListener(() => OnClickArrowButton(new Vector2Int(0, -1)));
            _leftButton.onClick.AddListener(() => OnClickArrowButton(new Vector2Int(-1, 0)));
            _rightButton.onClick.AddListener(() => OnClickArrowButton(new Vector2Int(1, 0)));
        }

        public void SetData(Action<Vector2Int> onClickArrowButtonCallback)
        {
            _onclickArrowButtonCallback = onClickArrowButtonCallback;

            InitScore();

            SetCoinData();
            SetPlayTimeData();
            SetTurnCountData();
            
            // 스킬 버튼 설정
            SetSkillButtonState();
        }

        private void InitScore()
        {
            _battleManager.CurrentScore.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                _scoreText.text = _battleManager.CurrentScore.ToString();
            }).AddTo(this);
        }

        private void SetCoinData()
        {
            _inventoryManager.CoinProperty.ObserveEveryValueChanged(property => property.Value).Subscribe(value =>
            {
                _coinText.text = value.ToString();
            }).AddTo(this);
        }

        private void SetPlayTimeData()
        {
            _battleManager.PlayingTime
                .Subscribe(value =>
                {
                    var remainTimeToInt = Mathf.CeilToInt(value);
                    var min = remainTimeToInt / 60;
                    var sec = remainTimeToInt % 60;
                    _playTimeText.text = $"{min:D2}:{sec:D2}";
                    _playTimeText.color = min == 0 && sec < 10 ? Color.red : Color.white;
                })
                .AddTo(this);  
        }

        private void SetTurnCountData()
        {
            _battleManager.RemainTurnCount.ObserveEveryValueChanged(value => value.Value)
                .Subscribe(value =>
                {
                    _remainTurnCount.text = value.ToString();
                    _remainTurnCount.color = value > 2 ? Color.white : Color.red;
                }).AddTo(this);
        }

        private void SetSkillButtonState()
        {
            foreach (var itemButton in _skillButtonViews)
            {
                itemButton.SetData();
            }            
        }

        private void OnClickArrowButton(Vector2Int direction)
        {
            _onclickArrowButtonCallback?.Invoke(direction);   
        }
    }
}