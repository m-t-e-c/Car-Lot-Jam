using System.Collections.Generic;
using System.Linq;
using CLJ.Runtime.Level;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayers;

        private Camera _mainCamera;
        private Stickman _currentStickman;
        private Car _currentCar;
        private bool _currentStickmanEnteringCar;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !IsStickmanMoving())
            {
                HandleMouseClick();
            }
        }

        private bool IsStickmanMoving()
        {
            return _currentStickman != null && _currentStickman.IsMoving;
        }

        private void HandleMouseClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, interactionLayers))
            {
                ProcessHit(hitInfo);
            }
        }

        private void ProcessHit(RaycastHit hitInfo)
        {
            if (hitInfo.collider.TryGetComponent(out Stickman stickman))
            {
                SelectStickman(stickman);
            }
            else if (hitInfo.collider.TryGetComponent(out Ground ground))
            {
                HandleGroundClick(ground);
            }
            else if (hitInfo.collider.TryGetComponent(out Car car))
            {
                HandleCarClick(car);
            }
        }

        private void SelectStickman(Stickman stickman)
        {
            if (_currentStickmanEnteringCar)
                return;

            _currentStickman?.CancelSelection();
            _currentStickman = stickman;
            _currentStickman.SetSelected();
        }

        private void HandleGroundClick(Ground ground)
        {
            if (ReferenceEquals(_currentStickman, null)) return;

            Vector2Int groundCoord = ground.GetCoordinates();
            bool hasPath = _currentStickman.MoveTo(groundCoord);

            if (!hasPath)
            {
                _currentStickman.PlayAngerEmoji();
            }

            ground.Highlight(hasPath);
        }


        private void HandleCarClick(Car car)
        {
            if (ReferenceEquals(_currentStickman, null) || !_currentStickman.GetColor().Equals(car.GetColor())) return;

            var enterCoordinates = car.GetEnterCells();
            var sortedCoordinates = SortCoordinatesByDistance(_currentStickman.GetGridPosition(), enterCoordinates);

            foreach (var coordinate in sortedCoordinates)
            {
                if (_currentStickman.MoveTo(coordinate, OnStickmanReachedCar))
                {
                    _currentStickmanEnteringCar = true;
                    _currentCar = car;
                    _currentStickman.PlayHappyEmoji();
                    car.Highlight(true);
                    return;
                }
            }

            _currentStickman.PlayAngerEmoji();
        }

        private IEnumerable<Vector2Int> SortCoordinatesByDistance(Vector2Int characterPosition, List<Vector2Int> coordinates)
        {
            var sortedCoordinates = coordinates.OrderBy(point =>
                GetManhattanDistance(new Vector2Int(characterPosition.x,characterPosition.y), new Vector2Int(point.x, point.y)));
            return sortedCoordinates;
        }

        private int GetManhattanDistance(Vector2Int positionA, Vector2Int positionB)
        {
            int distX = Mathf.Abs(positionA.x - positionB.x);
            int distY = Mathf.Abs(positionA.y - positionB.y);
            return distX + distY;
        }

        private void OnStickmanReachedCar()
        {
            Sequence sequence = DOTween.Sequence();

            bool isLeft = IsStickmanLeftFromCar();
            _currentCar.PlayOpenDoorAnimation(isLeft);
            _currentStickman.PlayEnterCarAnimation();
            Vector3 doorPosition = _currentCar.GetDoorPosition(isLeft);
            doorPosition.y = 0;

            var stickmanTransform = _currentStickman.transform;
            var carTransform = _currentCar.transform;
            stickmanTransform.parent = carTransform;
            stickmanTransform.rotation = carTransform.rotation;
            _currentStickman.gameObject.layer = LayerMask.NameToLayer("Default");

            sequence.Append(_currentStickman.transform.DOMove(doorPosition, 0.5f).SetEase(Ease.Linear));
            sequence.Append(_currentStickman.transform.DOMove(_currentCar.GetSeatPosition(), 0.7f).SetEase(Ease.Linear));
            sequence.AppendCallback(() => _currentCar.PlayCloseDoorAnimation());
            sequence.Join(_currentStickman.transform.DOScale(0.5f, 0.5f).SetEase(Ease.Linear));
            sequence.OnComplete(() =>
            {
                _currentCar.SetReady();
                _currentStickman.CancelSelection();
                _currentStickman = null;
                _currentStickmanEnteringCar = false;
            });
        }

        private bool IsStickmanLeftFromCar()
        {
            Vector3 stickmanPosition = _currentStickman.transform.position;
            Vector3 carPosition = _currentCar.transform.position;
            CellDirection carDirection = _currentCar.GetDirection();

            return carDirection switch
            {
                CellDirection.Right => stickmanPosition.z > carPosition.z,
                CellDirection.Left => stickmanPosition.z < carPosition.z,
                CellDirection.Up => stickmanPosition.x < carPosition.x,
                CellDirection.Down => stickmanPosition.x > carPosition.x,
                _ => false,
            };
        }
    }
}