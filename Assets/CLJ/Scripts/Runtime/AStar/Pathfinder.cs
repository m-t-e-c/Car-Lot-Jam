using System;
using System.Collections.Generic;
using System.Linq;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime.AStar
{
    public class Pathfinder
    {
        private readonly List<Node> _nodes = new();
        private readonly int _pathWidth;
        private readonly int _pathHeight;

        public Pathfinder(LevelGrid levelGrid)
        {
            for (int y = 0; y < levelGrid.height; y++)
            {
                for (int x = 0; x < levelGrid.width; x++)
                {
                    _nodes.Add(new Node(new Vector2Int(x,y),new Vector2Int(x, -y)));
                }
            }
        }
        
        public Node GetLastNode()
        {
            return _nodes[^1];
        }
        
        public Pathfinder(Vector2Int[,] path)
        {
            _pathWidth = path.GetUpperBound(0);
            _pathHeight = path.GetUpperBound(1);
            
            for (int y = 0; y < _pathHeight + 1; y++)
            {
                for (int x = 0; x < _pathWidth + 1; x++)
                {
                    bool isOccupied = x >= 1 && x <= _pathWidth - 1 && y >= 1 && y <= _pathHeight - 1;
                    _nodes.Add(new Node(new Vector2Int(x,y),path[x,y], isOccupied));
                }
            }
            
            for (int i = 1; i < 30; i++)
            {
                _nodes.Add(new Node(new Vector2Int(0, -i), new Vector2Int(-1, i)));
            }
        }

        public int GetPathWidth()
        {
            return _pathWidth;
        }
        
        public int GetPathHeight()
        {
            return _pathHeight;
        }
        
        public Node GetNode(Vector2Int coordinate)
        {
            return _nodes.Find(node => node.Coordinate.Equals(coordinate));
        }
        
        public List<Vector2Int> FindPath(Vector2Int startCoordinate, Vector2Int targetCoordinate, Action<Vector2Int> onPathFailed = null)
        {
            Node startNode = _nodes.Find(node => node.Coordinate.Equals(startCoordinate));
            Node targetNode = _nodes.Find(node => node.Coordinate.Equals(targetCoordinate));

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
                    if (neighbour.IsOccupied || closedSet.Contains(neighbour))
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

            onPathFailed?.Invoke(closedSet.LastOrDefault()!.Coordinate);
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
                Vector2Int checkPos = new Vector2Int(node.Coordinate.x + direction.x, node.Coordinate.y + direction.y);
                
                var neighbour = _nodes.Find(n => n.Coordinate.Equals(checkPos));
                
                if (neighbour != null && !neighbour.IsOccupied)
                {
                    neighbours.Add(neighbour);
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