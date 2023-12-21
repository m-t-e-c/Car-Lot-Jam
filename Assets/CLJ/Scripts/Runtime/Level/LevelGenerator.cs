using System;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Managers.LevelManager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CLJ.Runtime.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, GameObject> _spawnedGridCells = new();
        private ILevelManager _levelManager;
        private LevelGrid _levelGrid;
        private Pathfinder _gridPath;
        private Pathfinder _roadPath;
        private CameraHolder _cameraHolder;
        private Vector2Int[,] _path;

        public void Start()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            _levelManager.OnLevelLoad += OnLoadLevel;
        }

        private async void OnLoadLevel(LevelGrid levelGrid)
        {
            _levelGrid = levelGrid;
            InitializeCamera();
            InitializePath();
            await SpawnGround();
            await SpawnRoads();
            SpawnObjects();
        }

        private void InitializeCamera()
        {
            _cameraHolder = Locator.Instance.Resolve<CameraHolder>();
            _cameraHolder.Init(_levelGrid.height >= 6 ? CameraType.Orthographic : CameraType.Perspective);
        }

        private void InitializePath()
        {
            int pathWidth = _levelGrid.width + 2;
            int pathHeight = _levelGrid.height + 2;

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

        private async UniTask<GameObject> GetObjectFromAddressable(string key)
        {
            return await Addressables.LoadAssetAsync<GameObject>(key).Task;
        }

        private async UniTask SpawnGround()
        {
            _gridPath = new Pathfinder(_levelGrid);

            GameObject gridParent = new GameObject("Grid");
            for (int y = 0; y < _levelGrid.height; y++)
            {
                for (int x = 0; x < _levelGrid.width; x++)
                {
                    GameObject groundPrefab = await GetObjectFromAddressable("Ground");
                    Vector3 spawnPosition = new Vector3(x, 0, -y);
                    GameObject groundGo = Instantiate(groundPrefab, spawnPosition, Quaternion.identity,
                        gridParent.transform);
                    Node node = _gridPath.GetNode(new Vector2Int(x, y));
                    groundGo.GetComponent<Ground>().SetNode(node);
                    _spawnedGridCells.Add(new Vector2Int(x, y), groundGo);
                }
            }
        }

        private bool IsWithinGrid(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < _levelGrid.width && coordinate.y >= 0 &&
                   coordinate.y < _levelGrid.height;
        }

        private async void SpawnObjects()
        {
            GameObject levelObjectsParent = new GameObject("LevelObjects");
            for (int y = 0; y < _levelGrid.height; y++)
            {
                for (int x = 0; x < _levelGrid.width; x++)
                {
                    GridCell cell = _levelGrid.cells[x, y];
                    if (cell == null || cell.gridObject == null || cell.isSpawned || cell.isLinkCell) continue;

                    cell.linkedCellCoordinates.Add(new Vector2Int(x, y));
                    Vector3 middlePosition = GetCenterOfCells(cell.linkedCellCoordinates);
                    Quaternion rotation = GetObjectDirection(cell.cellDirection);
                    GameObject gridObjectPrefab =
                        await GetObjectFromAddressable(cell.gridObject.gridObjectType.ToString());
                    GameObject obj = Instantiate(gridObjectPrefab, middlePosition, rotation,
                        levelObjectsParent.transform);

                    InitializeGameObject(obj, cell, new Vector2Int(x, y));
                    SetLinkedCellsSpawned(cell.linkedCellCoordinates);
                }
            }
        }

        private void InitializeGameObject(GameObject obj, GridCell cell, Vector2Int cellPosition)
        {
            if (obj.TryGetComponent(out Stickman stickman))
            {
                stickman.Init(_gridPath, cellPosition, cell.cellColor);
            }
            else if (obj.TryGetComponent(out Car car))
            {
                List<Vector2Int> neighborCoordinates = GetNeighborCoordinates(cellPosition, cell.cellDirection, cell.gridObject.gridSpace);
                car.Init(cell.cellColor, _roadPath, cellPosition, cell.cellDirection, neighborCoordinates);
            }
        }
        
        private List<Vector2Int> GetNeighborCoordinates(Vector2Int cellCoordinate, CellDirection cellDirection, int gridSpace)
        {
            var neighbours = new List<Vector2Int>();
            var directions = cellDirection switch
            {
                CellDirection.Up or CellDirection.Down => new[] { Vector2Int.left , Vector2Int.right } ,
                CellDirection.Left or CellDirection.Right => new[] { Vector2Int.up, Vector2Int.down},
                _ => throw new ArgumentOutOfRangeException(nameof(cellDirection), cellDirection, null)
            };
            
            var lastCell = cellDirection == CellDirection.Up ? cellCoordinate - new Vector2Int(0,gridSpace - 1) : 
                cellDirection == CellDirection.Down ? cellCoordinate + new Vector2Int(0,gridSpace - 1) :
                cellDirection == CellDirection.Left ? cellCoordinate - new Vector2Int(gridSpace - 1,0) :
                cellCoordinate + new Vector2Int(gridSpace - 1,0);
            
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbourCoordinate = lastCell + direction;
                if (IsWithinGrid(neighbourCoordinate))
                {
                    neighbours.Add(neighbourCoordinate);
                }
            }
            
            return neighbours;
        }

        private async UniTask SpawnRoads()
        {
            GameObject roadsParent = new GameObject("Roads");
            await InstantiateRoads(roadsParent);
        }

        private async UniTask InstantiateRoads(GameObject roadsParent)
        {
            GameObject straightRoadPrefab = await GetObjectFromAddressable("StraightRoad");
            GameObject tRoadPrefab = await GetObjectFromAddressable("TRoad");
            GameObject cornerRoadPrefab = await GetObjectFromAddressable("CornerRoad");

            for (int i = 0; i < _levelGrid.width; i++)
            {
                Vector3 topPosition = new Vector3(i, 0, 1);
                Vector3 bottomPosition = new Vector3(i, 0, -_levelGrid.height);
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                Instantiate(straightRoadPrefab, topPosition, rotation, roadsParent.transform);
                Instantiate(straightRoadPrefab, bottomPosition, rotation, roadsParent.transform);
            }

            for (int i = 0; i < _levelGrid.height; i++)
            {
                Vector3 rightPosition = new Vector3(_levelGrid.width, 0, -i);
                Vector3 leftPosition = new Vector3(-1, 0, -i);
                Instantiate(straightRoadPrefab, rightPosition, Quaternion.identity, roadsParent.transform);
                Instantiate(straightRoadPrefab, leftPosition, Quaternion.identity, roadsParent.transform);
            }

            Vector3 topLeftPosition = new Vector3(-1, 0, 1);
            Vector3 topRightPosition = new Vector3(_levelGrid.width, 0, 1);
            Vector3 bottomRightPosition = new Vector3(_levelGrid.width, 0, -_levelGrid.height);
            Vector3 bottomLeftPosition = new Vector3(-1, 0, -_levelGrid.height);

            Instantiate(cornerRoadPrefab, topRightPosition, Quaternion.Euler(0, 90, 0), roadsParent.transform);
            Instantiate(cornerRoadPrefab, bottomLeftPosition, Quaternion.Euler(0, 270, 0), roadsParent.transform);
            Instantiate(cornerRoadPrefab, bottomRightPosition, Quaternion.Euler(0, 180, 0), roadsParent.transform);
            Instantiate(tRoadPrefab, topLeftPosition, Quaternion.identity, roadsParent.transform);

            for (int i = 1; i < 30; i++)
            {
                if (i == 4)
                {
                    GameObject exitGatePrefab = await GetObjectFromAddressable("ExitGate");
                    Vector3 exitPosition = topLeftPosition + i * Vector3.forward + Vector3.left;
                    Instantiate(exitGatePrefab, exitPosition, Quaternion.identity, roadsParent.transform);
                }

                Vector3 roadPosition = topLeftPosition + i * Vector3.forward;
                Instantiate(straightRoadPrefab, roadPosition, Quaternion.identity, roadsParent.transform);
            }
        }

        private void SetLinkedCellsSpawned(List<Vector2Int> linkedCells)
        {
            foreach (Vector2Int coordinate in linkedCells)
            {
                GridCell linkedCell = _levelGrid.cells[coordinate.x, coordinate.y];
                linkedCell.isSpawned = true;
            }
        }

        private Quaternion GetObjectDirection(CellDirection direction)
        {
            return direction switch
            {
                CellDirection.Down => Quaternion.Euler(0, 180, 0),
                CellDirection.Left => Quaternion.Euler(0, 270, 0),
                CellDirection.Right => Quaternion.Euler(0, 90, 0),
                _ => Quaternion.identity
            };
        }

        private Vector3 GetCenterOfCells(List<Vector2Int> cellCoords)
        {
            Vector3 sumPositions = Vector3.zero;
            foreach (Vector2Int coord in cellCoords)
            {
                if (_spawnedGridCells.TryGetValue(coord, out GameObject cell))
                {
                    sumPositions += cell.transform.position;
                }
            }

            return sumPositions / cellCoords.Count;
        }

        private void OnDisable()
        {
            _levelManager.OnLevelLoad -= OnLoadLevel;
            DOTween.KillAll();
        }
        
        public Ground GetGroundByCoordinate(Vector2Int coordinate)
        {
            return _spawnedGridCells[coordinate].GetComponent<Ground>();
        }
    }
}