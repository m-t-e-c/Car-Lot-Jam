using System;
using System.Collections;
using System.Collections.Generic;
using CLJ.Runtime.AStar;
using CLJ.Runtime.Level;
using UnityEngine;
using DG.Tweening;

namespace CLJ.Runtime
{
    public class Car : MonoBehaviour
    {
        private static readonly int OpenDoorHash = Animator.StringToHash("OpenDoor");
        private static readonly int DoorIndexHash = Animator.StringToHash("DoorIndex");

        [Header("Animation References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform leftDoorEnterTransform;
        [SerializeField] private Transform rightDoorEnterTransform;
        [SerializeField] private Transform seatTransform;
        [SerializeField] private Transform carBody;

        [Header("Other References")]
        [SerializeField] private GridObjectColorSetter gridObjectColorSetter;
        [SerializeField] private GameObject smokeParticles;
        [SerializeField] private Outline outline;
        [SerializeField] private LayerMask moveBlockLayers;

        private CellColor _cellColor;
        private Vector2Int _gridPosition;
        private CellDirection _direction;
        private List<Vector2Int> _aroundCells;
        private Pathfinder _pathfinder;

        private bool _isReadyToGo;
        private bool _isMoving;

        private void Start()
        {
            GameEvents.onCarSpawned?.Invoke();
        }

        public void Init(CellColor color, Pathfinder pathfinder, Vector2Int gridPosition, CellDirection direction, List<Vector2Int> aroundCells)
        {
            _aroundCells = aroundCells;
            _direction = direction;
            _gridPosition = gridPosition;
            _pathfinder = pathfinder;
            _cellColor = color;
            gridObjectColorSetter.SetColor(_cellColor);
        }
        
        public CellDirection GetDirection()
        {
            return _direction;
        }
        
        public Vector3 GetSeatPosition()
        {
            return seatTransform.position;
        }
        
        public List<Vector2Int> GetEnterCells()
        {
            return _aroundCells;
        }
        
        public CellColor GetColor()
        {
            return _cellColor;
        }

        public Vector3 GetDoorPosition(bool isLeft)
        {
            return isLeft ? leftDoorEnterTransform.position : rightDoorEnterTransform.position;
        }

        public void Highlight(bool highlightStatus)
        {
            outline.enabled = highlightStatus;
        }

        public void SetReady()
        {
            Highlight(false);
            _isReadyToGo = true;
        }
        
        public void PlayOpenDoorAnimation(bool isLeft)
        {
            animator.SetInteger(DoorIndexHash, isLeft ? -1 : 1);
            animator.SetBool(OpenDoorHash, true);
        }

        public void PlayCloseDoorAnimation()
        {
            animator.SetBool(OpenDoorHash, false);
        }

        private void FixedUpdate()
        {
            if (_isMoving || !_isReadyToGo)
                return;

            CheckAndExitRoad();
        }

        private void CheckAndExitRoad()
        {
            var myTransform = transform;
            Ray ray = new Ray(myTransform.position + Vector3.up * 0.5f, myTransform.forward);
            bool isBlockedFront = Physics.Raycast(ray.origin, ray.direction, 10, moveBlockLayers);
            bool isBlockedBack = Physics.Raycast(ray.origin, -ray.direction, 10, moveBlockLayers);

            if (isBlockedFront && !isBlockedBack)
            {
                ExitToRoad(false);
            }
            else if (!isBlockedFront)
            {
                ExitToRoad(true);
            }
        }

        private void ExitToRoad(bool forward)
        {
            _isMoving = true;
            smokeParticles.SetActive(true);

            var (exitKey, exitTarget) = GetExitGridPosition(forward);
            PlayAccelerateAnimation(forward);
            MoveCar(exitTarget, exitKey);
        }

        private void MoveCar(Vector2Int target, Vector2Int key)
        {
            Vector3 targetPosition = new Vector3(target.x, 0, target.y);
            transform.DOMove(targetPosition, 1f).OnComplete(() =>
            {
                _gridPosition = key;
                MoveTo(_pathfinder.GetLastNode().Coordinate);
            });
        }

        private (Vector2Int, Vector2Int) GetExitGridPosition(bool forward)
        {
            Vector2Int key = Vector2Int.zero;

            switch (_direction)
            {
                case CellDirection.Right:
                    key = new Vector2Int(forward ? _pathfinder.GetPathWidth() : 0, _gridPosition.y + 1);
                    break;
                case CellDirection.Left:
                    key = new Vector2Int(forward ? 0 : _pathfinder.GetPathWidth(), _gridPosition.y + 1);
                    break;
                case CellDirection.Up:
                    key = new Vector2Int(_gridPosition.x + 1, forward ? 0 : _pathfinder.GetPathHeight());
                    break;
                case CellDirection.Down:
                    key = new Vector2Int(_gridPosition.x + 1, forward ? _pathfinder.GetPathHeight() : 0);
                    break;
            }

            var target = _pathfinder.GetNode(key).Position;
            return (key, target);
        }

        private void MoveTo(Vector2Int targetPosition, Action onMoveComplete = null)
        {
            List<Vector2Int> path = _pathfinder.FindPath(_gridPosition, targetPosition);
            if (path == null || path.Count == 0) return;

            _gridPosition = targetPosition;
            StartCoroutine(FollowPath(path, onMoveComplete));
        }

        private IEnumerator FollowPath(List<Vector2Int> path, Action onMoveComplete)
        {
            foreach (var point in path)
            {
                yield return MoveToPoint(point);
            }
            onMoveComplete?.Invoke();
        }

        private IEnumerator MoveToPoint(Vector2Int point)
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
                    Quaternion.LookRotation(endPosition - startPosition), Time.deltaTime * 20f);
                transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                yield return null;
            }
        }

        private void PlayAccelerateAnimation(bool forward)
        {
            carBody
                .DOLocalRotate(new Vector3(forward ? -10f : 10f, 0f, 0f), 0.25f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }
}