using UnityEngine;

namespace CLJ.Runtime.AStar
{
    public class Node
    {
        public Vector2Int Position;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public bool IsOccupied;
        public Node Parent;

        public Node(Vector2Int position)
        {
            Position = position;
        }
    }
}