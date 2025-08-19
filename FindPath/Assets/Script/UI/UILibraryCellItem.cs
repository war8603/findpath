using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FindPath
{
    public class UILibraryCellItem : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _startCellColor; //FFFFFF
        [SerializeField] private Color _endCellColor; //285EFF
        [SerializeField] private Color _obstacleCellColor; //6C6C6C
        [SerializeField] private Color _normalCellColor; //FCFFB3

        public void SetData(CellType cellType)
        {
            _image.color = cellType switch
            {
                CellType.Start => _startCellColor,
                CellType.End => _endCellColor,
                CellType.Obstacle => _obstacleCellColor,
                CellType.Normal => _normalCellColor,
                _ => throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null)
            };
        }
    }
}