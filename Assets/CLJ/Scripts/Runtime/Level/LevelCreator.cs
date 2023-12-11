using System;
using System.Collections.Generic;
using System.Linq;
using CLJ.ScriptableObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace CLJ.Runtime.Level
{
    #region GridObject Enums

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

        #endregion
    

    #region SerializedGridCell
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
    #endregion


    public class LevelCreator : MonoBehaviour
    {
        [HideInInspector] public int gridWidth;
        [HideInInspector] public int gridHeight;
        [HideInInspector] public GridObjectDirection selectedCellObjectDirection;
        public GridObjectsGroup gridObjectsGroup;

        private LevelGrid _levelGrid;
        private GridObject _objectToPlace;

        public void GenerateGrid()
        {
            _levelGrid = new LevelGrid(gridWidth, gridHeight);
        }

        public void ResetGrid()
        {
            GenerateGrid();
            _objectToPlace = null;
        }

        public LevelGrid GetGrid()
        {
            return _levelGrid;
        }

        public GridCell GetCell(int x, int y)
        {
            var cell = _levelGrid.Cells[x, y];
            if (ReferenceEquals(cell, null))
            {
                _levelGrid.Cells[x, y] = new GridCell();
            }

            return _levelGrid.Cells[x, y];
        }

        public void GridButtonAction(int x, int y)
        {
            var cell = _levelGrid.Cells[x, y];
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
            var cell = _levelGrid.Cells[x, y];
            foreach (var linkedCellCoordinate in cell.linkedCellCoordinates)
            {
                var linkedCell = _levelGrid.Cells[linkedCellCoordinate.x, linkedCellCoordinate.y];
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
            selectedCellObjectDirection = gridObjectDirection;
        }

        public void PlaceSelectedObjectToGrid(int x, int y)
        {
            var gridSpace = _objectToPlace.gridSpace;

            if (!IsSpaceAvailable(x, y, gridSpace))
            {
                return;
            }

            switch (selectedCellObjectDirection)
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
                var cell = GetCell(newX, newY);
                cell.SetCell(_objectToPlace);
                cell.objectDirection = selectedCellObjectDirection;
            }

            foreach (var coord in linkedCellCoordinates)
            {
                GridCell cell = GetCell(coord.x, coord.y);
                cell.linkedCellCoordinates = linkedCellCoordinates.Where(c => c != coord).ToList();
            }
        }
        
        public void SaveGrid(int levelIndex)
        {
            List<SerializedGridCell> serializedGridCells = new List<SerializedGridCell>();

            foreach (GridCell gridCell in _levelGrid.Cells)
            {
                serializedGridCells.Add(new SerializedGridCell(gridCell.gridObject, gridCell.linkedCellCoordinates,
                    gridCell.objectDirection));
            }

            string json = JsonConvert.SerializeObject(serializedGridCells);
            System.IO.File.WriteAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json", json);
        }

        public void LoadGrid(int levelIndex)
        {
            string json =
                System.IO.File.ReadAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
            List<SerializedGridCell> serializedGridCells =
                JsonConvert.DeserializeObject<List<SerializedGridCell>>(json);

            _levelGrid = new LevelGrid(gridWidth, gridHeight);

            int totalCells = gridWidth * gridHeight;
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

                _levelGrid.Cells[y, x] = cell;
            }
        }

        private int GetDeltaX()
        {
            switch (selectedCellObjectDirection)
            {
                case GridObjectDirection.Left:
                    return -1;
                case GridObjectDirection.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        private int GetDeltaY()
        {
            switch (selectedCellObjectDirection)
            {
                case GridObjectDirection.Down:
                    return 1;
                case GridObjectDirection.Up:
                    return -1;
                default:
                    return 0;
            }
        }

        private bool IsSpaceAvailable(int x, int y, int gridSpace)
        {
            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x, newY = y;

                switch (selectedCellObjectDirection)
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

        private bool IsCellAvailable(int x, int y)
        {
            return GetCell(x, y).gridObject == null;
        }

        private void PlaceInDirection(int x, int y, int deltaX, int deltaY, int gridSpace)
        {
            for (int i = 0; i < gridSpace; i++)
            {
                int newX = x + i * deltaX;
                int newY = y + i * deltaY;

                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
                {
                    var cell = GetCell(newX, newY);
                    cell.SetCell(_objectToPlace);
                    cell.objectDirection = selectedCellObjectDirection;
                }
            }
        }
    }
}