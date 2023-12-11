using System;
using UnityEngine;

namespace CLJ.Runtime.Level
{
    [Serializable]
    public class GridObject
    {
        public GridObjectType gridObjectType;
        public GridObjectColor gridObjectColor;
        public int gridSpace;
        public Vector2Int point;
    }
}