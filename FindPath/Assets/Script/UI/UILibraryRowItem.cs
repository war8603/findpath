using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace FindPath
{
    public class UILibraryRowItem : EnhancedScrollerCellView
    {
        [SerializeField] private UILibraryItem _leftItem;
        [SerializeField] private UILibraryItem _rightItem;

        [SerializeField] private RectTransform _sizeRectTransform;

        public float Size()
        {
            return _sizeRectTransform.sizeDelta.y;
        }

        public void SetData(List<GridData> gridData)
        {
            _leftItem.SetData(gridData[0]);
            if (gridData.Count > 1)
            {
                _rightItem.gameObject.SetActive(true);
                _rightItem.SetData(gridData[1]);
            }
            else
            {
                _rightItem.gameObject.SetActive(false);
            }
        }
    }
}