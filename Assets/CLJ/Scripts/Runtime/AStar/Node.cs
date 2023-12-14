using UnityEngine;

namespace CLJ.Runtime.AStar
{
    public class Node
    {
        public Vector2Int Coordinate;
        public Vector2Int Position;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public bool IsOccupied;
        public Node Parent;

        public Node(Vector2Int coordinate,Vector2Int position, bool isOccupied = false)
        {
            Coordinate = coordinate;
            IsOccupied = isOccupied;
            Position = position;
        }
    }
}