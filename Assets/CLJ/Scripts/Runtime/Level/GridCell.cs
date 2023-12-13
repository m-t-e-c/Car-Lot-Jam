using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLJ.Runtime.Level
{
    [Serializable]
    public class GridCell
    {
        public GridObject gridObject;
        public List<Vector2Int> linkedCellCoordinates;
        public CellDirection cellDirection;
        public CellColor cellColor;
        public bool isSpawned;

        public GridCell()
        {
            gridObject = null;
            linkedCellCoordinates = null;
        }

        public void SetCell(GridObject obj, CellDirection direction, CellColor color)
        {
            gridObject = obj;
            cellDirection = direction;
            cellColor = color;
            gridObject = obj;
        }

        public void ResetCell()
        {
            gridObject = null;
        }
    }
}