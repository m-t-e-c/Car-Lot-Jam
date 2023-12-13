using System.Collections.Generic;
using CLJ.Managers.LevelManager;
using CLJ.Runtime.AStar;
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
        private CameraHolder _cameraHolder;
        
        private Vector2Int _exitPosition;

        private GameObject _roadHolder;
        private GameObject _gridHolder;

        public async void Start()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            _cameraHolder = Locator.Instance.Resolve<CameraHolder>();
            
            _levelGrid = _levelManager.GetLevelGrid();
            _cameraHolder.SetCamera(_levelGrid.Height >= 6 ? CameraType.Orthographic : CameraType.Perspective);

            _roadHolder = new GameObject("Roads");
            _gridHolder = new GameObject("Grid");
            
            await SpawnGround();
            await SpawnRoads();
            SpawnObjects();
        }

        async UniTask<GameObject> GetObjectFromAddressable(string key)
        {
            var result = await Addressables.LoadAssetAsync<GameObject>(key);
            return result;
        }

        async UniTask SpawnGround()
        {
            _gridPath = new Pathfinder(_levelGrid);
            
            for (int y = 0; y < _levelGrid.Height; y++)
            {
                for (int x = 0; x < _levelGrid.Width; x++)
                {
                    var ground = await GetObjectFromAddressable("Ground");
                    var obj = Instantiate(ground, new Vector3(x, 0, y), Quaternion.identity);
                    var node = _gridPath.GetNode(new Vector2Int(x, y));
                    obj.GetComponent<Ground>().SetNode(node);
                    obj.transform.GetChild(1).GetComponent<TextMeshPro>().SetText($"{x},{y}");
                    _spawnedGridCells.Add(new Vector2Int(x, y), obj);
                    obj.transform.parent = _gridHolder.transform;
                }
            }

        }

        async UniTask SpawnStraightRoads()
        {
            var road = await GetObjectFromAddressable("StraightRoad");

            for (int i = 0; i < _levelGrid.Width; i++)
            {
                Vector3 topPosition = new Vector3(i, 0, _levelGrid.Height);
                Vector3 bottomPosition = new Vector3(i, 0, -1);
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                var road1 = Instantiate(road, topPosition, rotation);
                var road2 = Instantiate(road, bottomPosition, rotation);

                road1.transform.parent = _roadHolder.transform;
                road2.transform.parent = _roadHolder.transform;
            }

            for (int i = 0; i < _levelGrid.Height; i++)
            {
                Vector3 topPosition = new Vector3(_levelGrid.Width, 0, i);
                Vector3 bottomPosition = new Vector3(-1, 0, i);
                var road1 = Instantiate(road, topPosition, Quaternion.identity);
                var road2 = Instantiate(road, bottomPosition, Quaternion.identity);

                road1.transform.parent = _roadHolder.transform;
                road2.transform.parent = _roadHolder.transform;
            }
        }

        async UniTask SpawnCornerRoads()
        {
            var corner = await GetObjectFromAddressable("CornerRoad");

            Vector3 topRightPosition = new Vector3(_levelGrid.Width, 0, _levelGrid.Height);
            Vector3 bottomRightPosition = new Vector3(_levelGrid.Width, 0, -1);
            Vector3 bottomLeftPosition = new Vector3(-1, 0, -1);
            
            var corner1 = Instantiate(corner, topRightPosition, Quaternion.Euler(0, 90, 0));
            var corner2=Instantiate(corner, bottomLeftPosition, Quaternion.Euler(0, 270, 0));
            var corner3 = Instantiate(corner, bottomRightPosition, Quaternion.Euler(0, 180, 0));
            
            corner1.transform.parent = _roadHolder.transform;
            corner2.transform.parent = _roadHolder.transform;
            corner3.transform.parent = _roadHolder.transform;
        }

        async UniTask SpawnExitGateRoad()
        {
            var tRoad = await GetObjectFromAddressable("TRoad");
            var straightRoad = await GetObjectFromAddressable("StraightRoad");
            Vector3 topLeftPosition = new Vector3(-1, 0, _levelGrid.Height);
            var tRoadGO = Instantiate(tRoad, topLeftPosition, Quaternion.Euler(0, 0, 0));
            
            tRoadGO.transform.parent = _roadHolder.transform;
            
            var exitGate = await GetObjectFromAddressable("ExitGate");

            for (int i = 1; i < 30; i++)
            {
                if (i == 4)
                {
                    var exitPosition = topLeftPosition + i * Vector3.forward + Vector3.left;
                    Instantiate(exitGate, exitPosition, Quaternion.identity);
                }

                var roadPosition = topLeftPosition + i * Vector3.forward;
                var road = Instantiate(straightRoad, roadPosition, Quaternion.Euler(0, 0, 0));
                road.transform.parent = _roadHolder.transform;
            }
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
                        car.Init(
                            cell.cellColor,
                            GetNeighborCoordinates(new Vector2Int(x,y), cell.linkedCellCoordinates),
                            _gridPath,
                            new Vector2Int(x,y),
                            new Vector2Int(0,0)
                            );
                    }
                    
                    SetLinkedCellsSpawned(cell.linkedCellCoordinates);
                }
            }
        }
        
        public List<Vector2Int> GetNeighborCoordinates(Vector2Int targetCoord, List<Vector2Int> linkedCells)
        {
            List<Vector2Int> neighbours = new List<Vector2Int>();

            Vector2Int[] directions = new[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)
            };
            
            foreach (var direction in directions)
            {
                var neighbourCoord = targetCoord + direction;
                if (IsWithinGrid(neighbourCoord))
                {
                    var cell = _levelGrid.Cells[neighbourCoord.x, neighbourCoord.y];
                    if (cell.isSpawned)
                        continue;
                    
                    if(linkedCells.Contains(neighbourCoord))
                        continue;
                    
                    neighbours.Add(neighbourCoord);
                }
            }
            
            return neighbours;
        }
        
        bool IsWithinGrid(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < _levelGrid.Width && coordinate.y >= 0 && coordinate.y < _levelGrid.Height;
        }

        async UniTask SpawnRoads()
        {
            await SpawnStraightRoads();
            await SpawnCornerRoads();
            await SpawnExitGateRoad();
            
          
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