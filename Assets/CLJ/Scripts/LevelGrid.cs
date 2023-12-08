using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace CLJ
{
    public enum GridDirection
    {
        Left,
        Right,
        Up,
        Down
    }
    
    public enum GridObjectType
    {
        None,
        SmallCar,
        LongCar,
        Stickman,
        Obstacle
    }

    [Serializable]
    public class GridObject
    {
        public GridObjectType gridObjectType;
        public Color32 color = Color.white;
        public int gridSpace;
        [HideInInspector] public GridDirection direction;
    }

    [Serializable]
    public class GridCell
    {
        [NonSerialized] public GridObject gridObject;
        [NonSerialized] public List<Vector2Int> linkedCellCoordinates;

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

        public SerializedGridCell(GridObject gridObject, List<Vector2Int> linkedCellCoordinates)
        {
            this.gridObject = gridObject;
            this.linkedCellCoordinates = linkedCellCoordinates;
        }
    }
    
    public class SerializedGrid
    {
        public List<SerializedGridCell> serializedGridCells;
    }

    public class LevelGrid : MonoBehaviour
    {
        public int gridWidth;
        public int gridHeight;
        public List<GridObject> gridObjectList = new List<GridObject>();
        public GridCell[] grid;
        private GridObject objectToPlace;
        public GridDirection objectDirection;
        public int levelIndex;

        public void GenerateGrid()
        {
            grid = new GridCell[gridWidth * gridHeight];
        }
        
        public GridCell[] GetGrid()
        {
            return grid;
        }

        public void ResetGrid()
        {
            grid = new GridCell[gridWidth * gridHeight];
            objectToPlace = null;
        }

        public void GridButtonAction(int x, int y)
        {
            var cell = grid[y * gridWidth + x];
            if (objectToPlace == null)
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
            var cell = grid[y * gridWidth + x];
            foreach (var linkedCellCoordinate in cell.linkedCellCoordinates)
            {
                var linkedCell = grid[linkedCellCoordinate.y * gridWidth + linkedCellCoordinate.x];
                linkedCell.ResetCell();
            }

            cell.ResetCell();
        }

        public void SetObjectToPlace(GridObject gridObject)
        {
            objectToPlace = gridObject;
        }

        public void PlaceSelectedObjectToGrid(int x, int y)
        {
            var gridSpace = objectToPlace.gridSpace;

            if (!IsSpaceAvailable(x, y, gridSpace))
            {
                return;
            }

            switch (objectDirection)
            {
                case GridDirection.Left:
                    PlaceInDirection(x, y, -1, 0, gridSpace);
                    break;
                case GridDirection.Right:
                    PlaceInDirection(x, y, 1, 0, gridSpace);
                    break;
                case GridDirection.Down:
                    PlaceInDirection(x, y, 0, 1, gridSpace);
                    break;
                case GridDirection.Up:
                    PlaceInDirection(x, y, 0, -1, gridSpace);
                    break;
            }

            List<Vector2Int> linkedCellCoordinates = new List<Vector2Int>();

            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x + i * GetDeltaX();
                int newY = y + i * GetDeltaY();

                linkedCellCoordinates.Add(new Vector2Int(newX, newY));
                grid[newY * gridWidth + newX].SetCell(objectToPlace);
            }

            foreach (var coord in linkedCellCoordinates)
            {
                GridCell cell = grid[coord.y * gridWidth + coord.x];
                cell.linkedCellCoordinates = linkedCellCoordinates.Where(c => c != coord).ToList();
            }
        }

        int GetDeltaX()
        {
            switch (objectDirection)
            {
                case GridDirection.Left:
                    return -1;
                case GridDirection.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        int GetDeltaY()
        {
            switch (objectDirection)
            {
                case GridDirection.Down:
                    return 1;
                case GridDirection.Up:
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

                switch (objectDirection)
                {
                    case GridDirection.Left:
                        newX -= i;
                        break;
                    case GridDirection.Right:
                        newX += i;
                        break;
                    case GridDirection.Down:
                        newY += i;
                        break;
                    case GridDirection.Up:
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
            return grid[y * gridWidth + x].gridObject == null;
        }

        void PlaceInDirection(int x, int y, int deltaX, int deltaY, int gridSpace)
        {
            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x + i * deltaX;
                int newY = y + i * deltaY;

                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
                {
                    grid[newY * gridWidth + newX].SetCell(objectToPlace);
                }
            }
        }

        public void SaveGrid()
        {
            List<SerializedGridCell> serializedGridCells = new List<SerializedGridCell>();

            foreach (GridCell gridCell in grid)
            {
                serializedGridCells.Add(new SerializedGridCell(gridCell.gridObject, gridCell.linkedCellCoordinates));
            }

            string json = JsonConvert.SerializeObject(serializedGridCells);
            System.IO.File.WriteAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json", json);
        }


        public void LoadGrid()
        {
            string json = System.IO.File.ReadAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
            List<SerializedGridCell> serializedGridCells = JsonConvert.DeserializeObject<List<SerializedGridCell>>(json);

            List<GridCell> cells = new List<GridCell>();
            foreach (SerializedGridCell serializedGridCell in serializedGridCells)
            {
                GridCell cell = new GridCell();
                cell.gridObject = serializedGridCell.gridObject;
                cell.linkedCellCoordinates = serializedGridCell.linkedCellCoordinates;
                cells.Add(cell);
            }

            grid = cells.ToArray();
        }

    }
}