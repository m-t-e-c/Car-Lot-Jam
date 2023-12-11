using System;
using System.Collections.Generic;
using System.Linq;
using CLJ.ScriptableObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace CLJ.Scripts.Runtime.Level
{
    public enum GridObjectDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public enum GridObjectColor
    {
        Red,
        Green,
        Yellow,
        Blue,
        Purple,
        Orange,
    }

    public enum GridObjectType
    {
        None,
        SmallCar,
        LongCar,
        Stickman,
        Obstacle,
        StraightRoad,
        CurvedRoad,
    }

    [Serializable]
    public class GridObject
    {
        public GridObjectType gridObjectType;
        public GridObjectColor gridObjectColor;
        public int gridSpace;
        public Vector2Int point;
    }

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

    public class SerializedGridCell
    {
        public GridObject gridObject;
        public List<Vector2Int> linkedCellCoordinates;
        public GridObjectDirection objectDirection;

        public SerializedGridCell(GridObject gridObject, List<Vector2Int> linkedCellCoordinates,
            GridObjectDirection objectDirection)
        {
            this.gridObject = gridObject;
            this.linkedCellCoordinates = linkedCellCoordinates;
            this.objectDirection = objectDirection;
        }
    }

    public class LevelGrid : MonoBehaviour
    {
        [HideInInspector] public int gridWidth;
        [HideInInspector] public int gridHeight;
        [HideInInspector] public GridObjectDirection objectObjectDirection;
        [HideInInspector] public GridCell[,] grid;
        public GridObjectsGroup gridObjectsGroup;
        private GridObject _objectToPlace;

        public void GenerateGrid()
        {
            grid = new GridCell[gridHeight, gridWidth];
        }
        public GridCell GetCell(int x, int y)
        {
            var cell = grid[x, y];
            if (ReferenceEquals(cell, null))
            {
                grid[x, y] = new GridCell();
            }
            return grid[x,y];
        }

        public void ResetGrid()
        {
            grid = new GridCell[gridHeight, gridWidth];
            _objectToPlace = null;
        }

        public void GridButtonAction(int x, int y)
        {
            var cell = grid[x, y];
            if (_objectToPlace == null)
            {
                if (cell.gridObject == null)
                    return;

                ClearCell(x, y);
            }
            else
            {
                PlaceSelectedObjectToGrid(x, y);
            }
        }

        public void ClearCell(int x, int y)
        {
            var cell = grid[x,y];
            foreach (var linkedCellCoordinate in cell.linkedCellCoordinates)
            {
                var linkedCell = grid[linkedCellCoordinate.x,linkedCellCoordinate.y];
                linkedCell.ResetCell();
            }

            cell.ResetCell();
        }

        public void SetObjectToPlace(GridObject gridObject)
        {
            _objectToPlace = gridObject;
        }

        public void SetObjectDirection(GridObjectDirection gridObjectDirection)
        {
            objectObjectDirection = gridObjectDirection;
        }

        public void PlaceSelectedObjectToGrid(int x, int y)
        {
            var gridSpace = _objectToPlace.gridSpace;

            if (!IsSpaceAvailable(x, y, gridSpace))
            {
                return;
            }

            switch (objectObjectDirection)
            {
                case GridObjectDirection.Left:
                    PlaceInDirection(x, y, -1, 0, gridSpace);
                    break;
                case GridObjectDirection.Right:
                    PlaceInDirection(x, y, 1, 0, gridSpace);
                    break;
                case GridObjectDirection.Down:
                    PlaceInDirection(x, y, 0, 1, gridSpace);
                    break;
                case GridObjectDirection.Up:
                    PlaceInDirection(x, y, 0, -1, gridSpace);
                    break;
            }

            List<Vector2Int> linkedCellCoordinates = new List<Vector2Int>();

            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x + i * GetDeltaX();
                int newY = y + i * GetDeltaY();

                linkedCellCoordinates.Add(new Vector2Int(newX, newY));
                var cell = GetCell(newX,newY);
                cell.SetCell(_objectToPlace);
                cell.objectDirection = objectObjectDirection;
            }

            foreach (var coord in linkedCellCoordinates)
            {
                GridCell cell = GetCell(coord.x, coord.y);
                cell.linkedCellCoordinates = linkedCellCoordinates.Where(c => c != coord).ToList();
            }
        }

        int GetDeltaX()
        {
            switch (objectObjectDirection)
            {
                case GridObjectDirection.Left:
                    return -1;
                case GridObjectDirection.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        int GetDeltaY()
        {
            switch (objectObjectDirection)
            {
                case GridObjectDirection.Down:
                    return 1;
                case GridObjectDirection.Up:
                    return -1;
                default:
                    return 0;
            }
        }

        bool IsSpaceAvailable(int x, int y, int gridSpace)
        {
            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x, newY = y;

                switch (objectObjectDirection)
                {
                    case GridObjectDirection.Left:
                        newX -= i;
                        break;
                    case GridObjectDirection.Right:
                        newX += i;
                        break;
                    case GridObjectDirection.Down:
                        newY += i;
                        break;
                    case GridObjectDirection.Up:
                        newY -= i;
                        break;
                }

                if (newX < 0 || newX >= gridWidth || newY < 0 || newY >= gridHeight)
                {
                    return false;
                }

                if (!IsCellAvailable(newX, newY))
                {
                    return false;
                }
            }

            return true;
        }

        bool IsCellAvailable(int x, int y)
        {
            return GetCell(x,y).gridObject == null;
        }

        void PlaceInDirection(int x, int y, int deltaX, int deltaY, int gridSpace)
        {
            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x + i * deltaX;
                int newY = y + i * deltaY;

                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
                {
                    var cell = GetCell(newX,newY);
                    cell.SetCell(_objectToPlace);
                    cell.objectDirection = objectObjectDirection;
                }
            }
        }

        public void SaveGrid(int levelIndex)
        {
            List<SerializedGridCell> serializedGridCells = new List<SerializedGridCell>();

            foreach (GridCell gridCell in grid)
            {
                serializedGridCells.Add(new SerializedGridCell(gridCell.gridObject, gridCell.linkedCellCoordinates,
                    gridCell.objectDirection));
            }

            string json = JsonConvert.SerializeObject(serializedGridCells);
            System.IO.File.WriteAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json", json);
        }

        public void LoadGrid(int levelIndex)
        {
            string json = System.IO.File.ReadAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
            List<SerializedGridCell> serializedGridCells = JsonConvert.DeserializeObject<List<SerializedGridCell>>(json);

            grid = new GridCell[gridHeight, gridWidth];

            int totalCells = gridHeight * gridWidth;
            for (int i = 0; i < serializedGridCells.Count && i < totalCells; i++)
            {
                SerializedGridCell serializedGridCell = serializedGridCells[i];

                int x = i % gridWidth;
                int y = i / gridWidth;

                GridCell cell = new GridCell
                {
                    gridObject = serializedGridCell.gridObject,
                    linkedCellCoordinates = serializedGridCell.linkedCellCoordinates,
                    objectDirection = serializedGridCell.objectDirection
                };

                grid[y, x] = cell;
            }
        }
    }
}