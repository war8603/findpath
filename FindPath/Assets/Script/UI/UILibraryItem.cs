using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace FindPath
{
    public class UILibraryItem : MonoBehaviour
    {
        [SerializeField] private Image _backImage;
        [SerializeField] private UILibraryCellItem _cellItemPrefab;
        
        private GameObject _root;

        private readonly List<UILibraryCellItem> _items = new();
        
        private void CreateRoot()
        {
            if (_root != null) return;
            
            _root = new GameObject("Root");
            _root.transform.InitParent(gameObject.transform);
        }
        
        private Transform GetRoot()
        {
            if (_root == null)
            {
                CreateRoot();    
            }

            return _root.transform;
        }
        
        public void SetData(GridData gridData)
        {
            CreateRoot();
            
            var size = _backImage.GetComponent<RectTransform>().sizeDelta.x;
            var offset = new Vector2(-size / 2f, -size / 2f);

            var root = GetRoot();
            var cellSize = Mathf.Max(gridData.SizeX, gridData.SizeY);
            offset += new Vector2(size / cellSize / 2f, size / cellSize / 2f);

            // 모두 비활성화
            _items.ForEach(x => x.gameObject.SetActive(false));

            // 부족한 수량만큼 생성
            if (_items.Count < gridData.Cells.Count)
            {
                for (var i = _items.Count; i < gridData.Cells.Count; i++)
                {
                    var cellItem = Instantiate(_cellItemPrefab, root.transform);
                    _items.Add(cellItem);
                }
            }
            
            for (var i = 0; i < gridData.Cells.Count; i++)
            {
                var cellItem = _items[i];
                cellItem.gameObject.SetActive(true);
                cellItem.transform.localScale = new Vector3(1f / cellSize, 1f / cellSize, 1f);
                cellItem.transform.localPosition = (Vector2)gridData.Cells[i].Position * size / cellSize + offset;

                var cellType = CellType.Normal;
                if (gridData.Cells[i].IsObstacle)
                {
                    cellType = CellType.Obstacle;
                }
                else if (gridData.Cells[i].Position == gridData.StartPosition)
                {
                    cellType = CellType.Start;
                }
                else if (gridData.Cells[i].Position == gridData.EndPosition)
                {
                    cellType = CellType.End;
                }

                cellItem.SetData(cellType);
            }
        }
    }
}