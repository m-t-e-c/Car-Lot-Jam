using System;
using System.Collections.Generic;
using System.Linq;
using CLJ.ScriptableObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace CLJ.Runtime.Level
{
    #region GridObject Enums

    public enum CellDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public enum CellColor
    {
        None,
        Purple,
        Black,
        Green,
        Blue,
        Orange,
        Pink,
        Red,
        Yellow
    }

    public enum GridObjectType
    {
        None,
        SmallCar,
        LongCar,
        Stickman,
        Cone,
        Barrier
    }

    #endregion

    public class LevelCreator : MonoBehaviour
    {
        [HideInInspector] public int gridWidth;
        [HideInInspector] public int gridHeight;
        [HideInInspector] public CellDirection selectedCellDirection;
        [HideInInspector] public CellColor selectedCellColor;
        public GridObjectsGroup gridObjectsGroup;

        private LevelGrid _levelGrid;
        private GridObject _objectToPlace;

        public void GenerateGrid()
        {
            _levelGrid = new LevelGrid(gridWidth, gridHeight);
        }

        public void ResetGrid()
        {
            _levelGrid = null;
            _objectToPlace = null;
            GenerateGrid();
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

        public void SetObjectDirection(CellDirection cellDirection)
        {
            selectedCellDirection = cellDirection;
        }

        public void SetObjectColor(CellColor cellColor)
        {
            selectedCellColor = cellColor;
        }
        
        public void PlaceSelectedObjectToGrid(int x, int y)
        {
            var gridSpace = _objectToPlace.gridSpace;

            if (!IsSpaceAvailable(x, y, gridSpace))
            {
                return;
            }

            switch (selectedCellDirection)
            {
                case CellDirection.Left:
                    PlaceInDirection(x, y, -1, 0, gridSpace);
                    break;
                case CellDirection.Right:
                    PlaceInDirection(x, y, 1, 0, gridSpace);
                    break;
                case CellDirection.Down:
                    PlaceInDirection(x, y, 0, 1, gridSpace);
                    break;
                case CellDirection.Up:
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
                cell.SetCell(_objectToPlace, selectedCellDirection, selectedCellColor);
            }

            foreach (var coord in linkedCellCoordinates)
            {
                GridCell cell = GetCell(coord.x, coord.y);
                cell.linkedCellCoordinates = linkedCellCoordinates.Where(c => c != coord).ToList();
            }
        }

        public void SaveGrid(int levelIndex)
        {
            string json = JsonConvert.SerializeObject(_levelGrid);
            System.IO.File.WriteAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json", json);
        }

        public void LoadGrid(int levelIndex)
        {
            var levelGrid = LoadLevel(levelIndex);
            _levelGrid = new LevelGrid(levelGrid.Width, levelGrid.Height)
            {
                Cells = levelGrid.Cells
            };

            gridWidth = _levelGrid.Width;
            gridHeight = _levelGrid.Height;
        }

        private int GetDeltaX()
        {
            switch (selectedCellDirection)
            {
                case CellDirection.Left:
                    return -1;
                case CellDirection.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        private int GetDeltaY()
        {
            switch (selectedCellDirection)
            {
                case CellDirection.Down:
                    return 1;
                case CellDirection.Up:
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

                switch (selectedCellDirection)
                {
                    case CellDirection.Left:
                        newX -= i;
                        break;
                    case CellDirection.Right:
                        newX += i;
                        break;
                    case CellDirection.Down:
                        newY += i;
                        break;
                    case CellDirection.Up:
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
                    cell.SetCell(_objectToPlace, selectedCellDirection, selectedCellColor);
                }
            }
        }

        public static LevelGrid LoadLevel(int levelIndex)
        {
            var json =
                System.IO.File.ReadAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
            return JsonConvert.DeserializeObject<LevelGrid>(json);
        }
    }
}