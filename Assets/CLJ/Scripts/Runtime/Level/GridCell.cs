using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLJ.Runtime.Level
{
    public enum CellDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public enum CellColor
    {
        None,
        Purple,
        Black,
        Green,
        Blue,
        Orange,
        Pink,
        Red,
        Yellow
    }
    
    [Serializable]
    public class GridCell
    {
        public GridObject gridObject;
        public List<Vector2Int> linkedCellCoordinates;
        public CellDirection cellDirection;
        public CellColor cellColor;
        public bool isSpawned;
        public bool isLinkCell;

        public GridCell()
        {
            gridObject = null;
            linkedCellCoordinates = null;
        }

        public void SetCell(GridObject obj, CellDirection direction, CellColor color, bool linkCell = false)
        {
            gridObject = obj;
            cellDirection = direction;
            cellColor = color;
            gridObject = obj;
            isLinkCell = linkCell;
        }

        public void ResetCell()
        {
            gridObject = null;
        }
    }
}