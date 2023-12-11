using System.Collections.Generic;
using CLJ.Scripts;
using CLJ.Scripts.Runtime.AStar;
using CLJ.Scripts.Runtime.Level;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CLJ.Runtime
{
    public class LevelGenerator : MonoBehaviour
    {
        Dictionary<Vector2Int, GameObject> _spawnedCells = new Dictionary<Vector2Int, GameObject>();
        [SerializeField] private LevelGrid _levelGrid;

        private Pathfinder _pathfinder;

        public async void Start()
        {
            _pathfinder = new Pathfinder(_levelGrid);
            await SpawnGrid();
            SpawnObjects();
            SpawnRoad();
        }

        async UniTask SpawnGrid()
        {
            for (int y = 0; y < _levelGrid.gridHeight; y++)
            {
                for (int x = 0; x < _levelGrid.gridWidth; x++)
                {
                    var ground = await GetObjectFromAddressable("Ground");
                    var obj = Instantiate(ground, new Vector3(x, 0, y), Quaternion.identity);
                    obj.GetComponent<Ground>().SetCoordinates(x,y);
                    _spawnedCells.Add(new Vector2Int(x, y), obj);
                }
            }
        }

        async void SpawnRoad()
        {
            var road = await GetObjectFromAddressable("StraightRoad");

            // Up and Down Roads
            for (int i = 0; i < _levelGrid.gridWidth; i++)
            {
                Vector3 topPosition = new Vector3(i, 0, _levelGrid.gridHeight);
                Vector3 bottomPosition = new Vector3(i, 0, -1);
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                Instantiate(road, topPosition, rotation);
                Instantiate(road, bottomPosition, rotation);
            }
            
            // Left and Right Roads
            for (int i = 0; i < _levelGrid.gridHeight; i++)
            {
                Vector3 topPosition = new Vector3(_levelGrid.gridWidth, 0, i);
                Vector3 bottomPosition = new Vector3(-1, 0, i);
                Instantiate(road, topPosition, Quaternion.identity);
                Instantiate(road, bottomPosition, Quaternion.identity);
            }

            // Corners
            Vector3 topRightPosition = new Vector3(_levelGrid.gridWidth, 0, _levelGrid.gridHeight);
            Vector3 bottomLeftPosition = new Vector3(-1, 0, -1);
            Vector3 bottomRightPosition = new Vector3(_levelGrid.gridWidth, 0, -1);
            
            var corner = await GetObjectFromAddressable("CornerRoad");
           
            Instantiate(corner, topRightPosition, Quaternion.Euler(0, 90, 0));
            Instantiate(corner, bottomLeftPosition, Quaternion.Euler(0,270,0));
            Instantiate(corner, bottomRightPosition, Quaternion.Euler(0, 180, 0));
            
            var tRoad = await GetObjectFromAddressable("TRoad");
            Vector3 topLeftPosition = new Vector3(-1, 0, _levelGrid.gridHeight);
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
            for (int y = 0; y < _levelGrid.gridHeight; y++)
            {
                for (int x = 0; x < _levelGrid.gridWidth; x++)
                {
                    var cell = _levelGrid.grid[x, y];
                    if (cell == null || cell.gridObject == null || cell.isSpawned)
                        continue;

                    cell.linkedCellCoordinates.Add(new Vector2Int(x, y));
                    var middlePosition = GetCenterOfCells(cell.linkedCellCoordinates);

                    var rotation =
                        cell.objectDirection == GridObjectDirection.Down ? Quaternion.Euler(0, 180, 0) :
                        cell.objectDirection == GridObjectDirection.Left ? Quaternion.Euler(0, 270, 0) :
                        cell.objectDirection == GridObjectDirection.Right ? Quaternion.Euler(0, 90, 0) :
                        Quaternion.identity;

                    var gridObject = await GetObjectFromAddressable(cell.gridObject.gridObjectType.ToString());
                    var obj = Instantiate(gridObject, middlePosition, rotation);

                    if (obj.TryGetComponent(out Stickman stickman))
                    {
                        stickman.SetPathfinder(_pathfinder);
                        stickman.SetGridPosition(new Vector2Int(x,y));
                    }

                    foreach (Vector2Int coordinate in cell.linkedCellCoordinates)
                    {
                        var linkedCell = _levelGrid.grid[coordinate.x, coordinate.y];
                        linkedCell.isSpawned = true;
                    }
                }
            }
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

        async UniTask<GameObject> GetObjectFromAddressable(string key)
        {
            var result = await Addressables.LoadAssetAsync<GameObject>(key);
            return result;
        }
    }
}