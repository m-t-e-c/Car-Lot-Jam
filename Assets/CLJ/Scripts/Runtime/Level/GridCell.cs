using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLJ.Runtime.Level
{
   
    [Serializable]
    public class GridCell
    {
        [NonSerialized] public GridObject gridObject;
        [NonSerialized] public List<Vector2Int> linkedCellCoordinates;
        public GridObjectDirection objectDirection;
        public bool isSpawned;

        public GridCell()
        {
            gridObject = null;
            linkedCellCoordinates = null;
        }

        public void SetCell(GridObject obj)
        {
            gridObject = obj;
        }

        public void ResetCell()
        {
            gridObject = null;
        }
    }
}