using System.Collections.Generic;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime.AStar
{
    public class Pathfinder
    {
        private readonly Dictionary<Vector2Int, Node> _nodes = new();

        public Pathfinder(LevelGrid levelGrid)
        {
            for (int y = 0; y < levelGrid.Height; y++)
            {
                for (int x = 0; x < levelGrid.Width; x++)
                {
                    var cell = levelGrid.Cells[x, y];
                    _nodes.Add(new Vector2Int(x,y), new Node(new Vector2Int(x, y), cell.gridObject == null));
                }
            }
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
        {
            Node startNode = _nodes[start];
            Node targetNode = _nodes[target];

            List<Node> openList = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost &&
                        openList[i].HCost < currentNode.HCost)
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbour in GetNeighbours(currentNode))
                {
                    if (!neighbour.IsWalkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                    if (newCostToNeighbour < neighbour.GCost || !openList.Contains(neighbour))
                    {
                        neighbour.GCost = newCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

                        if (!openList.Contains(neighbour))
                            openList.Add(neighbour);
                    }
                }
            }

            return new List<Vector2Int>();
        }

        private List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            Vector2Int[] directions = {
                new Vector2Int(0, 1),  
                new Vector2Int(0, -1), 
                new Vector2Int(1, 0), 
                new Vector2Int(-1, 0) 
            };

            foreach (var direction in directions)
            {
                Vector2Int checkPos = new Vector2Int(node.Position.x + direction.x, node.Position.y + direction.y);
                if (_nodes.ContainsKey(checkPos) && _nodes[checkPos].IsWalkable)
                {
                    neighbours.Add(_nodes[checkPos]);
                }
            }

            return neighbours;
        }

        private List<Vector2Int> RetracePath(Node startNode, Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
            int distY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);

            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);
            return 14 * distX + 10 * (distY - distX);
        }
    }
}