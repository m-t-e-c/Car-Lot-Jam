using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Car : MonoBehaviour
    {
        #region Animation Hashes
        private static readonly int OpenDoor = Animator.StringToHash("OpenDoor");
        private static readonly int DoorIndex = Animator.StringToHash("DoorIndex");
        #endregion

        [Header("Animation References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _leftDoorEnterTransform;
        [SerializeField] private Transform _rightDoorEnterTransform;
        [SerializeField] private Transform _seatTransform;
        [SerializeField] private Transform _carBody;

        [Header("Other References")]
        [SerializeField] private GridObjectColorSetter _gridObjectColorSetter;
        [SerializeField] private GameObject _smokeParticles;
        [SerializeField] private Outline _outline;
        [SerializeField] private LayerMask _moveBlockLayers;

        private CellColor _cellColor;
        private Vector2Int _gridPosition;
        private CellDirection _direction;
        private List<Vector2Int> _aroundCells;
        private Pathfinder _pathfinder;

        private bool _isReadyToGo;
        private bool _isMoving;

        public void Init(CellColor color, Pathfinder pathfinder, Vector2Int gridPosition, CellDirection direction, List<Vector2Int> aroundCells)
        {
            _aroundCells = aroundCells;
            _direction = direction;
            _gridPosition = gridPosition;
            _pathfinder = pathfinder;
            _cellColor = color;
            _gridObjectColorSetter.SetColor(_cellColor);
        }
        
        public CellDirection GetDirection()
        {
            return _direction;
        }
        
        public CellColor GetColor()
        {
            return _cellColor;
        }

        public Vector3 GetDoorPosition(bool isLeft)
        {
            return isLeft ? _leftDoorEnterTransform.position : _rightDoorEnterTransform.position;
        }

        public Vector3 GetSeatPosition()
        {
            return _seatTransform.position;
        }
        
        public List<Vector2Int> GetAroundCells()
        {
            return _aroundCells;
        }
        
        public void Highlight()
        {
            _outline.enabled = true;
        }

        public void SetReady()
        {
            _outline.enabled = false;
            _isReadyToGo = true;
        }

        private void FixedUpdate()
        {
            if (_isMoving)
                return;

            if (_isReadyToGo)
            {
                Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
                Physics.Raycast(ray.origin, ray.direction, out RaycastHit frontHit, 10,_moveBlockLayers);
                if (frontHit.collider)
                {
                    Physics.Raycast(ray.origin, -ray.direction, out RaycastHit backHit, 10,_moveBlockLayers);
                    if (!backHit.collider)
                    {
                        ExitToRoad(false);
                        return;
                    }
                    return;
                }
                
                ExitToRoad(true);
            }
        }

        public void ExitToRoad(bool forward)
        {
            _isMoving = true;
            _smokeParticles.SetActive(true);

            var (key, target) = GetExitGridPosition(forward);
          
            PlayAccelerateAnimation(forward);
            transform.DOMove(new Vector3(target.x,0,target.y), 1f)
                .OnComplete(() =>
                {
                   
                    _gridPosition = key;
                    MoveTo(new Vector2Int(0, -28));
                });
        }

        public (Vector2Int, Vector2Int) GetExitGridPosition(bool forward)
        {
            Vector2Int key = Vector2Int.zero;
            Vector2Int target = Vector2Int.zero;
            if (_direction == CellDirection.Right)
            {
                if (forward)
                {
                    var pathXMax = _pathfinder.GetPathWidth();
                    key = new Vector2Int(pathXMax, _gridPosition.y + 1);
                    target = _pathfinder.GetNode(key).Position;
                }
                else
                {
                    key = new Vector2Int(0, _gridPosition.y + 1);
                    target = _pathfinder.GetNode(key).Position;
                }
            }
            else if (_direction == CellDirection.Left)
            {
                if (forward)
                {
                    key = new Vector2Int(0, _gridPosition.y + 1);
                    target = _pathfinder.GetNode(key).Position;
                }
                else
                {
                    var pathXMax = _pathfinder.GetPathWidth();
                    key = new Vector2Int(pathXMax, _gridPosition.y + 1);
                    target = _pathfinder.GetNode(key).Position;
                }
            }
            else if (_direction == CellDirection.Up)
            {
                if (forward)
                {
                    key = new Vector2Int(_gridPosition.x + 1,0);
                    target = _pathfinder.GetNode(key).Position;
                }
                else
                {
                    var pathYMax =_pathfinder.GetPathHeight();
                    key = new Vector2Int(_gridPosition.x + 1, pathYMax);
                    target = _pathfinder.GetNode(key).Position;
                }
            }
            else if (_direction == CellDirection.Down)
            {
                if (forward)
                {
                    var pathYMax =_pathfinder.GetPathHeight();
                    key = new Vector2Int(_gridPosition.x + 1, pathYMax);
                    target = _pathfinder.GetNode(key).Position;
                }
                else
                {
                    key = new Vector2Int(_gridPosition.x + 1,0);
                    target = _pathfinder.GetNode(key).Position;
                }
            }
            
            return (key, target);
        }
        
        public void MoveTo(Vector2Int targetPosition, Action onMoveComplete = null)
        {
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition);

            if (path == null || path.Count == 0)
            {
                return;
            }

            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path, onMoveComplete));
        }
        
        private IEnumerator FollowPath(List<Vector2Int> path, Action onMoveComplete = null)
        {
            foreach (var point in path)
            {
                Vector3 startPosition = transform.position;
                Vector3 endPosition = new Vector3(point.x, 0, point.y);
                float journeyLength = Vector3.Distance(startPosition, endPosition);
                float startTime = Time.time;

                while (transform.position != endPosition)
                {
                    float distCovered = (Time.time - startTime) * 5;
                    float fractionOfJourney = distCovered / journeyLength;
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(endPosition - startPosition), Time.deltaTime * 10f);
                    transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                    yield return null;
                }
            }

            onMoveComplete?.Invoke();
        }
        
        #region Animation Methods
        public void PlayOpenDoorAnimation(bool isLeft)
        {
            _animator.SetInteger(DoorIndex, isLeft ? -1 : 1);
            _animator.SetBool(OpenDoor, true);
        }
        
        public void PlayCloseDoorAnimation()
        {
            _animator.SetBool(OpenDoor, false);
        }
        
        public void PlayAccelerateAnimation(bool forward)
        {
            _carBody
                .DOLocalRotate(new Vector3(forward ? -10f : 10f, 0f,0f ), 0.25f)
                .SetLoops(2, LoopType.Yoyo);
        }

        #endregion
    }
}