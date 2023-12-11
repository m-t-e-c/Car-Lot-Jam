using System.Collections.Generic;
using CLJ.Managers.LevelManager;
using CLJ.Runtime.AStar;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CLJ.Runtime.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        readonly Dictionary<Vector2Int, GameObject> _spawnedCells = new();

        private ILevelManager _levelManager;
        private LevelGrid _levelGrid;
        private Pathfinder _pathfinder;

        public async void Start()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            _levelGrid = _levelManager.GetCurrentLevel();
            _pathfinder = new Pathfinder(_levelGrid);
            await SpawnGround();
            SpawnObjects();
            SpawnRoads();
        }
        async UniTask<GameObject> GetObjectFromAddressable(string key)
        {
            var result = await Addressables.LoadAssetAsync<GameObject>(key);
            return result;
        }

        async UniTask SpawnGround()
        {
            for (int y = 0; y < _levelGrid.Height; y++)
            {
                for (int x = 0; x < _levelGrid.Width; x++)
                {
                    var ground = await GetObjectFromAddressable("Ground");
                    var obj = Instantiate(ground, new Vector3(x, 0, y), Quaternion.identity);
                    obj.GetComponent<Ground>().SetCoordinates(x, y);
                    _spawnedCells.Add(new Vector2Int(x, y), obj);
                }
            }
        }

        async void SpawnStraightRoads()
        {
            var road = await GetObjectFromAddressable("StraightRoad");

            for (int i = 0; i < _levelGrid.Width; i++)
            {
                Vector3 topPosition = new Vector3(i, 0, _levelGrid.Height);
                Vector3 bottomPosition = new Vector3(i, 0, -1);
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                Instantiate(road, topPosition, rotation);
                Instantiate(road, bottomPosition, rotation);
            }

            for (int i = 0; i < _levelGrid.Height; i++)
            {
                Vector3 topPosition = new Vector3(_levelGrid.Width, 0, i);
                Vector3 bottomPosition = new Vector3(-1, 0, i);
                Instantiate(road, topPosition, Quaternion.identity);
                Instantiate(road, bottomPosition, Quaternion.identity);
            }
        }

        async void SpawnCornerRoads()
        {
            var corner = await GetObjectFromAddressable("CornerRoad");

            Vector3 topRightPosition = new Vector3(_levelGrid.Width, 0, _levelGrid.Height);
            Vector3 bottomLeftPosition = new Vector3(-1, 0, -1);
            Vector3 bottomRightPosition = new Vector3(_levelGrid.Width, 0, -1);

            Instantiate(corner, topRightPosition, Quaternion.Euler(0, 90, 0));
            Instantiate(corner, bottomLeftPosition, Quaternion.Euler(0, 270, 0));
            Instantiate(corner, bottomRightPosition, Quaternion.Euler(0, 180, 0));
        }

        async void SpawnExitGate()
        {
            var tRoad = await GetObjectFromAddressable("TRoad");
            var road = await GetObjectFromAddressable("StraightRoad");
            Vector3 topLeftPosition = new Vector3(-1, 0, _levelGrid.Height);
            Instantiate(tRoad, topLeftPosition, Quaternion.Euler(0, 0, 0));

            var exitGate = await GetObjectFromAddressable("ExitGate");

            for (int i = 1; i < 30; i++)
            {
                if (i == 4)
                {
                    Instantiate(exitGate, topLeftPosition + i * Vector3.forward + Vector3.left, Quaternion.identity);
                }

                Instantiate(road, topLeftPosition + i * Vector3.forward, Quaternion.Euler(0, 0, 0));
            }
        }

        async void SpawnObjects()
        {
            for (int y = 0; y < _levelGrid.Height; y++)
            {
                for (int x = 0; x < _levelGrid.Width; x++)
                {
                    var cell = _levelGrid.Cells[x, y];
                    if (cell == null || cell.gridObject == null || cell.isSpawned)
                        continue;

                    cell.linkedCellCoordinates.Add(new Vector2Int(x, y));

                    var middlePosition = GetCenterOfCells(cell.linkedCellCoordinates);
                    var rotation = GetObjectDirection(cell.objectDirection);
                    var gridObject = await GetObjectFromAddressable(cell.gridObject.gridObjectType.ToString());
                    var obj = Instantiate(gridObject, middlePosition, rotation);

                    if (obj.TryGetComponent(out Stickman stickman))
                    {
                        stickman.Init(_pathfinder, new Vector2Int(x, y));
                    }

                    SetLinkedCellsSpawned(cell.linkedCellCoordinates);
                }
            }
        }
        
        void SpawnRoads()
        {
            SpawnStraightRoads();
            SpawnCornerRoads();
            SpawnExitGate();
        }

        void SetLinkedCellsSpawned(List<Vector2Int> linkedCells)
        {
            foreach (Vector2Int coordinate in linkedCells)
            {
                var linkedCell = _levelGrid.Cells[coordinate.x, coordinate.y];
                linkedCell.isSpawned = true;
            }
        }

        Quaternion GetObjectDirection(GridObjectDirection direction)
        {
            var rotation =
                direction == GridObjectDirection.Down ? Quaternion.Euler(0, 180, 0) :
                direction == GridObjectDirection.Left ? Quaternion.Euler(0, 270, 0) :
                direction == GridObjectDirection.Right ? Quaternion.Euler(0, 90, 0) :
                Quaternion.identity;

            return rotation;
        }

        Vector3 GetCenterOfCells(List<Vector2Int> cellCoords)
        {
            Vector3 sumPositions = Vector3.zero;
            foreach (var coord in cellCoords)
            {
                if (_spawnedCells.TryGetValue(coord, out GameObject cell))
                {
                    sumPositions += cell.transform.position;
                }
            }

            return sumPositions / cellCoords.Count;
        }

    }
}