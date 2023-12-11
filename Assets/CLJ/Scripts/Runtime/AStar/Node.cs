using UnityEngine;

namespace CLJ.Runtime.AStar
{
    public class Node
    {
        public Vector2Int Position;
        public bool IsWalkable;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public Node Parent;

        public Node(Vector2Int position, bool isWalkable)
        {
            Position = position;
            IsWalkable = isWalkable;
        }
    }
}