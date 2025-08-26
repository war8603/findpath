using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace FindPath
{
    public class UICollectionPopupView : UIMainView, IEnhancedScrollerDelegate
    {
        [Inject] private readonly DataManager _dataManager;
        [Inject] private readonly IObjectResolver _objectResolver;
        [Inject] private readonly UIManager _uiManager;

        [SerializeField] private UILibraryRowItem _rowItem;
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private Button _closeButton;
        
        private List<GridData> _gridDataList;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnClickCloseButton);
        }
        
        public void ShowView()
        {
            _gridDataList = _dataManager.GetGridDataList();
            _gridDataList.Sort((a, b) =>
            {
                var ca = _dataManager.GetClearCount(a.GridIndex);
                var cb = _dataManager.GetClearCount(b.GridIndex);

                // 1) 둘 다 0 이상 → gridIndex 오름차순
                if (ca > 0 && cb > 0) return a.GridIndex.CompareTo(b.GridIndex);

                // 2) 둘 다 0 -> gridIndex 오름차순
                if (ca == 0 && cb == 0) return a.GridIndex.CompareTo(b.GridIndex);

                // 3) 하나만 0 이상 → 0 이상인 쪽 우선
                return (ca > 0) ? -1 : 1;
            });
            _scroller.Delegate = this;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            var rowItemCount = _gridDataList.Count % 2 == 0 ? _gridDataList.Count / 2 : _gridDataList.Count / 2 + 1;
            return rowItemCount;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (_rowItem.TryGetComponent<UILibraryRowItem>(out var rowItem))
            {
                return rowItem.Size();
            }

            Debug.LogError("[GetCellViewSize] itemView is null");
            return 0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var gridCount = _dataManager.GetGroupCount();
            var rowItemCount = gridCount % 2 == 0 ? gridCount / 2 : gridCount / 2 + 1;
            
            if (dataIndex >= rowItemCount)
            {
                return null;
            }
            
            var scrollCell = _rowItem.GetComponent<EnhancedScrollerCellView>();
            var item = scroller.GetCellView(scrollCell) as UILibraryRowItem;
            if (item == null) return null;

            item.gameObject.SetActive(true);
            _objectResolver.InjectGameObject(item.gameObject);

            var gridDataList = new List<GridData>();
            var clearCounts = new List<int>();
            gridDataList.Add(_gridDataList[dataIndex * 2]);
            clearCounts.Add(_dataManager.GetClearCount(_gridDataList[dataIndex * 2].GridIndex));
            if (dataIndex * 2 + 1 < _gridDataList.Count)
            {
                gridDataList.Add(_gridDataList[dataIndex * 2 + 1]);
                clearCounts.Add(_dataManager.GetClearCount(_gridDataList[dataIndex * 2 + 1].GridIndex));
            }
            
            item.SetData(gridDataList, clearCounts);

            return item;
        }

        private void OnClickCloseButton()
        {
            _uiManager.DestroyView(this);
        }
    }
}