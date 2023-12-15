using CLJ.Runtime.Level;
using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactionLayers;

        private Camera _camera;
        private Stickman _stickman;
        private Car _car;

        private void Start()
        {
            _camera = Camera.main;
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
            return _stickman != null && _stickman.isMoving;
        }

        private void HandleMouseClick()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity, _interactionLayers))
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
            _stickman?.CancelSelection();
            _stickman = stickman;
            _stickman.SetSelected();
        }

        private void HandleGroundClick(Ground ground)
        {
            if (ReferenceEquals(_stickman, null)) return;

            Vector2Int groundCoord = ground.GetCoordinates();
            bool hasPath = _stickman.MoveTo(groundCoord, OnStickmanReachedGround);

            if (!hasPath)
            {
                _stickman.PlayAngerEmoji();
            }

            ground.Highlight(hasPath);
        }

        private void OnStickmanReachedGround()
        {
            _stickman.CancelSelection();
            _stickman = null;
        }

        private void HandleCarClick(Car car)
        {
            if (ReferenceEquals(_stickman, null) || !_stickman.GetColor().Equals(car.GetColor())) return;

            if (car.GetAroundCells().Count.Equals(0))
            {
                _stickman.PlayAngerEmoji();
                return;
            }

            foreach (Vector2Int carAroundCoord in car.GetAroundCells())
            {
                if (_stickman.MoveTo(carAroundCoord, OnStickmanReachedCar))
                {
                    _car = car;
                    _stickman.PlayHappyEmoji();
                    car.Highlight();
                    break;
                }
            }
        }

        private void OnStickmanReachedCar()
        {
            Sequence sequence = DOTween.Sequence();

            var isLeft = IsStickmanLeftFromCar();
            _car.PlayOpenDoorAnimation(isLeft);
            _stickman.PlayEnterCarAnimation();
            var seatPosition = _car.GetSeatPosition();
            var doorPosition = _car.GetDoorPosition(isLeft);
            doorPosition = new Vector3(doorPosition.x, 0, doorPosition.z);

            _stickman.transform.parent = _car.transform;

            sequence
                .Append(_stickman.transform.DOMove(doorPosition, 1f))
                .Append(_stickman.transform.DOMove(seatPosition, 1f))
                .Join(_stickman.transform.DORotate(seatPosition, 0.5f))
                .Join(_stickman.transform.DOScale(0.5f, 1f)
                    .OnComplete(() =>
                    {
                        _car.PlayCloseDoorAnimation();
                        _car.SetReady();
                    }));

            _stickman.CancelSelection();
            _stickman = null;
        }
        
        private bool IsStickmanLeftFromCar()
        {
            var carDirection = _car.GetDirection();
            bool isLeft = false;
            
            if (carDirection == CellDirection.Right)
            {
                isLeft = _stickman.transform.position.z > _car.transform.position.z;
            }
            else if (carDirection == CellDirection.Left)
            {
                isLeft = _stickman.transform.position.z < _car.transform.position.z;
            }
            else if (carDirection == CellDirection.Up)
            {
                isLeft = _stickman.transform.position.x < _car.transform.position.x;

            }
            else if (carDirection == CellDirection.Down)
            {
                isLeft = _stickman.transform.position.x > _car.transform.position.x;
            }
            
            return isLeft;
        }
    }
}