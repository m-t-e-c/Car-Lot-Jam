using System.Collections.Generic;
using CLJ.Managers.LevelManager;
using CLJ.Runtime.AStar;
using CLJ.Scripts;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CLJ.Runtime.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        readonly Dictionary<Vector2Int, GameObject> _spawnedGridCells = new();

        private ILevelManager _levelManager;
        private LevelGrid _levelGrid;
        private Pathfinder _gridPath;
        private Pathfinder _roadPath;
        private CameraHolder _cameraHolder;

        public Vector2Int[,] _path;

        public async void Start()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            _levelGrid = _levelManager.GetLevelGrid();

            _cameraHolder = Locator.Instance.Resolve<CameraHolder>();
            _cameraHolder.SetCamera(_levelGrid.Height >= 6 ? CameraType.Orthographic : CameraType.Perspective);

            InitializePath();

            await SpawnGround();
            await SpawnRoads();
            SpawnObjects();
        }

        void InitializePath()
        {
            var pathWidth = _levelGrid.Width + 2;
            var pathHeight = _levelGrid.Height + 2;

            _path = new Vector2Int[pathWidth, pathHeight];

            for (int y = 0; y < pathHeight; y++)
            {
                for (int x = 0; x < pathWidth; x++)
                {
                    _path[x, y] = new Vector2Int(x - 1, 1 - y);
                }
            }

            _roadPath = new Pathfinder(_path);
        }

        async UniTask<GameObject> GetObjectFromAddressable(string key)
        {
            var result = await Addressables.LoadAssetAsync<GameObject>(key);
            return result;
        }

        async UniTask SpawnGround()
        {
            _gridPath = new Pathfinder(_levelGrid);

            GameObject gridParent = new GameObject("Grid");
            for (int y = 0; y < _levelGrid.Height; y++)
            {
                for (int x = 0; x < _levelGrid.Width; x++)
                {
                    var ground = await GetObjectFromAddressable("Ground");
                    var groundGo = Instantiate(ground, new Vector3(x, 0, -y), Quaternion.identity);
                    var node = _gridPath.GetNode(new Vector2Int(x, y));

                    groundGo.GetComponent<Ground>().SetNode(node);
                    groundGo.transform.GetChild(1).GetComponent<TextMeshPro>().SetText($"{x},{y}");
                    groundGo.transform.parent = gridParent.transform;

                    _spawnedGridCells.Add(new Vector2Int(x, y), groundGo);
                }
            }
        }

        public List<Vector2Int> GetNeighborCoordinates(Vector2Int targetCoord, List<Vector2Int> linkedCells,
            CellDirection cellDirection)
        {
            List<Vector2Int> neighbours = new List<Vector2Int>();
            List<Vector2Int> directions = new List<Vector2Int>();

            if (cellDirection == CellDirection.Up)
            {
                directions.Add(Vector2Int.left);
                directions.Add(Vector2Int.right);
            }
            else if (cellDirection == CellDirection.Down)
            {
                directions.Add(Vector2Int.left);
                directions.Add(Vector2Int.right);
            }
            else if (cellDirection == CellDirection.Left)
            {
                directions.Add(Vector2Int.up);
                directions.Add(Vector2Int.down);
            }
            else if (cellDirection == CellDirection.Right)
            {
                directions.Add(Vector2Int.up);
                directions.Add(Vector2Int.down);
            }

            foreach (var direction in directions)
            {
                var neighbourCoord = targetCoord + direction;
                if (IsWithinGrid(neighbourCoord))
                {
                    var cell = _levelGrid.Cells[neighbourCoord.x, neighbourCoord.y];
                    if (cell.isSpawned)
                        continue;

                    if (linkedCells.Contains(neighbourCoord))
                        continue;

                    neighbours.Add(neighbourCoord);
                }
            }

            return neighbours;
        }

        bool IsWithinGrid(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < _levelGrid.Width && coordinate.y >= 0 &&
                   coordinate.y < _levelGrid.Height;
        }


        async void SpawnObjects()
        {
            GameObject levelObjects = new GameObject("LevelObjects");
            for (int y = 0; y < _levelGrid.Height; y++)
            {
                for (int x = 0; x < _levelGrid.Width; x++)
                {
                    var cell = _levelGrid.Cells[x, y];
                    if (cell == null || cell.gridObject == null || cell.isSpawned)
                        continue;

                    cell.linkedCellCoordinates.Add(new Vector2Int(x, y));

                    var middlePosition = GetCenterOfCells(cell.linkedCellCoordinates);
                    var rotation = GetObjectDirection(cell.cellDirection);
                    var gridObject = await GetObjectFromAddressable(cell.gridObject.gridObjectType.ToString());
                    var obj = Instantiate(gridObject, middlePosition, rotation);

                    obj.transform.parent = levelObjects.transform;

                    if (obj.TryGetComponent(out Stickman stickman))
                    {
                        stickman.Init(_gridPath, new Vector2Int(x, y), cell.cellColor);
                    }
                    else if (obj.TryGetComponent(out Car car))
                    {
                        var neighborCoordinates = GetNeighborCoordinates(new Vector2Int(x, y),
                            cell.linkedCellCoordinates, cell.cellDirection);
                        car.Init(cell.cellColor, _roadPath, new Vector2Int(x, y), cell.cellDirection,
                            neighborCoordinates);
                    }

                    SetLinkedCellsSpawned(cell.linkedCellCoordinates);
                }
            }
        }

        async UniTask SpawnRoads()
        {
            GameObject roadsParent = new GameObject("Roads");

            var straightRoad = await GetObjectFromAddressable("StraightRoad");
            var tRoad = await GetObjectFromAddressable("TRoad");
            var cornerRoad = await GetObjectFromAddressable("CornerRoad");

            // Spawn Upper and Lower Roads
            for (int i = 0; i < _levelGrid.Width; i++)
            {
                Vector3 topPosition = new Vector3(i, 0, 1);
                Vector3 bottomPosition = new Vector3(i, 0, -_levelGrid.Height);
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                var road1 = Instantiate(straightRoad, topPosition, rotation);
                var road2 = Instantiate(straightRoad, bottomPosition, rotation);

                road1.transform.parent = roadsParent.transform;
                road2.transform.parent = roadsParent.transform;
            }

            // Spawn Right and Left Roads
            for (int i = 0; i < _levelGrid.Height; i++)
            {
                Vector3 rightPos = new Vector3(_levelGrid.Width, 0, -i);
                Vector3 leftPos = new Vector3(-1, 0, -i);
                var road1 = Instantiate(straightRoad, rightPos, Quaternion.identity);
                var road2 = Instantiate(straightRoad, leftPos, Quaternion.identity);

                road1.transform.parent = roadsParent.transform;
                road2.transform.parent = roadsParent.transform;
            }


            Vector3 topLeftPosition = new Vector3(-1, 0, 1);
            Vector3 topRightPosition = new Vector3(_levelGrid.Width, 0, 1);
            Vector3 bottomRightPosition = new Vector3(_levelGrid.Width, 0, -_levelGrid.Height);
            Vector3 bottomLeftPosition = new Vector3(-1, 0, -_levelGrid.Height);

            var topRightCornerRoad = Instantiate(cornerRoad, topRightPosition, Quaternion.Euler(0, 90, 0));
            var bottomLeftCornerRoad = Instantiate(cornerRoad, bottomLeftPosition, Quaternion.Euler(0, 270, 0));
            var bottomRightCornerRoad = Instantiate(cornerRoad, bottomRightPosition, Quaternion.Euler(0, 180, 0));

            topRightCornerRoad.transform.parent = roadsParent.transform;
            bottomLeftCornerRoad.transform.parent = roadsParent.transform;
            bottomRightCornerRoad.transform.parent = roadsParent.transform;


            var tRoadGo = Instantiate(tRoad, topLeftPosition, Quaternion.Euler(0, 0, 0));
            tRoadGo.transform.parent = roadsParent.transform;

            for (int i = 1; i < 30; i++)
            {
                if (i == 4)
                {
                    var exitGate = await GetObjectFromAddressable("ExitGate");
                    var exitPosition = topLeftPosition + i * Vector3.forward + Vector3.left;
                    Instantiate(exitGate, exitPosition, Quaternion.identity);
                }

                var roadPosition = topLeftPosition + i * Vector3.forward;
                var road = Instantiate(straightRoad, roadPosition, Quaternion.Euler(0, 0, 0));
                road.transform.parent = roadsParent.transform;
            }
        }

        void SetLinkedCellsSpawned(List<Vector2Int> linkedCells)
        {
            foreach (Vector2Int coordinate in linkedCells)
            {
                var linkedCell = _levelGrid.Cells[coordinate.x, coordinate.y];
                linkedCell.isSpawned = true;
            }
        }

        Quaternion GetObjectDirection(CellDirection direction)
        {
            var rotation =
                direction == CellDirection.Down ? Quaternion.Euler(0, 180, 0) :
                direction == CellDirection.Left ? Quaternion.Euler(0, 270, 0) :
                direction == CellDirection.Right ? Quaternion.Euler(0, 90, 0) :
                Quaternion.identity;

            return rotation;
        }

        Vector3 GetCenterOfCells(List<Vector2Int> cellCoords)
        {
            Vector3 sumPositions = Vector3.zero;
            foreach (var coord in cellCoords)
            {
                if (_spawnedGridCells.TryGetValue(coord, out GameObject cell))
                {
                    sumPositions += cell.transform.position;
                }
            }

            return sumPositions / cellCoords.Count;
        }
    }
}