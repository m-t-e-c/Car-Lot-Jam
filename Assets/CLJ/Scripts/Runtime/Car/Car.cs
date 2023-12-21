using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Animation References")] [SerializeField]
        private Animator animator;

        [SerializeField] private Transform leftDoorEnterTransform;
        [SerializeField] private Transform rightDoorEnterTransform;
        [SerializeField] private Transform seatTransform;
        [SerializeField] private Transform carBody;

        [Header("Other References")] [SerializeField]
        private GridObjectColorSetter gridObjectColorSetter;

        [SerializeField] private LayerMask moveBlockLayers;
        [SerializeField] private GameObject trailSmokeParticles;
        [SerializeField] private GameObject smokeParticles;
        [SerializeField] private Outline outline;
        [SerializeField] private float outlineWidth;
        [SerializeField] private float moveSpeed;

        private CellColor _cellColor;
        private Vector2Int _gridPosition;
        private CellDirection _direction;
        private List<Vector2Int> _aroundCells;
        private Pathfinder _pathfinder;
        private Stickman _currentStickman;
        private Sequence _carShakeSequence;
        private Sequence _carEnterSequence;

        private bool _isReadyToGo;
        private bool _isMoving;

        private void Start()
        {
            GameEvents.onCarSpawned?.Invoke();
        }

        public void Init(CellColor color, Pathfinder pathfinder, Vector2Int gridPosition, CellDirection direction,
            List<Vector2Int> aroundCells)
        {
            _aroundCells = aroundCells;
            _direction = direction;
            _gridPosition = gridPosition;
            _pathfinder = pathfinder;
            _cellColor = color;
            gridObjectColorSetter.SetColor(_cellColor);
        }

        public void SetStickman(Stickman stickman, Action<Vector2Int> onPathFailed = null)
        {
            _currentStickman = stickman;
            var enterCoordinates = GetEnterCells();
            var sortedCoordinates = SortCoordinatesByDistance(_currentStickman.GetGridPosition(), enterCoordinates);

            int unreachablePathCount = 0;
            foreach (var coordinate in sortedCoordinates)
            {
                if (_currentStickman.MoveTo(coordinate, OnStickmanReachedCar))
                {
                    _currentStickman.PlayHappyEmoji();
                    Highlight(true);
                    break;
                }

                Highlight(false);
                _currentStickman.PlayAngerEmoji();

                unreachablePathCount++;
                if (unreachablePathCount.Equals(sortedCoordinates.Count))
                {
                    onPathFailed?.Invoke(sortedCoordinates[0]);
                }
            }
        }

        private List<Vector2Int> SortCoordinatesByDistance(Vector2Int characterPosition, List<Vector2Int> coordinates)
        {
            var sortedCoordinates = coordinates.OrderBy(point => GetManhattanDistance(new Vector2Int(characterPosition.x, characterPosition.y), new Vector2Int(point.x, point.y)));
            return sortedCoordinates.ToList();
        }

        private int GetManhattanDistance(Vector2Int positionA, Vector2Int positionB)
        {
            int distX = Mathf.Abs(positionA.x - positionB.x);
            int distY = Mathf.Abs(positionA.y - positionB.y);
            return distX + distY;
        }

        private void OnStickmanReachedCar()
        {
            _carEnterSequence?.Kill();
            _carEnterSequence = DOTween.Sequence();
            bool isLeft = IsStickmanLeftFromCar();
            PlayOpenDoorAnimation(isLeft);
            _currentStickman.PlayEnterCarAnimation();
            Vector3 doorPosition = GetDoorPosition(isLeft);
            doorPosition.y = 0;

            var stickmanTransform = _currentStickman.transform;
            var carTransform = transform;
            stickmanTransform.parent = carTransform;
            stickmanTransform.rotation = carTransform.rotation;
            _currentStickman.gameObject.layer = LayerMask.NameToLayer("Default");

            _carEnterSequence.Insert(0,_currentStickman.transform.DOScale(0.65f, 1f).SetEase(Ease.Linear));
            _carEnterSequence.Insert(0,_currentStickman.transform.DOMove(doorPosition, 0.3f).SetEase(Ease.OutSine));
            _carEnterSequence.InsertCallback(1, PlayCloseDoorAnimation);
            _carEnterSequence.Insert(1,_currentStickman.transform.DOMove(GetSeatPosition(), 0.3f).SetEase(Ease.Linear));
            _carEnterSequence.OnComplete(()=>
            {
                _isReadyToGo = true;
                _carShakeSequence = DOTween.Sequence();
                _carShakeSequence.Append(carBody.transform.DOShakeRotation(0.15f, 1, 1, 0));
                _carShakeSequence.SetLoops(-1, LoopType.Yoyo);
                smokeParticles.SetActive(true);
            });
        }

        private bool IsStickmanLeftFromCar()
        {
            Vector3 stickmanPosition = _currentStickman.transform.position;
            Vector3 carPosition = transform.position;
            CellDirection carDirection = GetDirection();

            return carDirection switch
            {
                CellDirection.Right => stickmanPosition.z > carPosition.z,
                CellDirection.Left => stickmanPosition.z < carPosition.z,
                CellDirection.Up => stickmanPosition.x < carPosition.x,
                CellDirection.Down => stickmanPosition.x > carPosition.x,
                _ => false,
            };
        }

        private CellDirection GetDirection()
        {
            return _direction;
        }

        private Vector3 GetSeatPosition()
        {
            return seatTransform.position;
        }

        private List<Vector2Int> GetEnterCells()
        {
            return _aroundCells;
        }

        public CellColor GetColor()
        {
            return _cellColor;
        }

        private Vector3 GetDoorPosition(bool isLeft)
        {
            return isLeft ? leftDoorEnterTransform.position : rightDoorEnterTransform.position;
        }

        public void Highlight(bool highlightStatus)
        {
            Color32 color = highlightStatus ? new Color32(0, 255, 0, 255) : new Color32(255, 0, 0, 255);
            outline.OutlineColor = color;
            float alpha = 0;
            DOTween.To(() => alpha, x => alpha = x, outlineWidth, 0.7f)
                .SetLoops(2, LoopType.Yoyo)
                .OnUpdate(() =>
                {
                    outline.OutlineWidth = alpha;
                });
        }

        private void PlayOpenDoorAnimation(bool isLeft)
        {
            animator.SetInteger(DoorIndexHash, isLeft ? -1 : 1);
            animator.SetBool(OpenDoorHash, true);
        }

        private void PlayCloseDoorAnimation()
        {
            animator.SetBool(OpenDoorHash, false);
        }

        private void FixedUpdate()
        {
            if (_isMoving || !_isReadyToGo)
            {
                _carShakeSequence?.Kill();
                return;
            }

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
            trailSmokeParticles.SetActive(true);

            var (exitKey, exitTarget) = GetExitGridPosition(forward);
            PlayAccelerateAnimation(forward);
            MoveCar(exitTarget, exitKey);
        }

        private void MoveCar(Vector2Int target, Vector2Int key)
        {
            Vector3 targetPosition = new Vector3(target.x, 0, target.y);
            transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
                {
                    _gridPosition = key;
                    MoveTo(_pathfinder.GetLastNode().Coordinate);
                })
                .SetEase(Ease.Linear);
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
                float distCovered = (Time.time - startTime) * moveSpeed;
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