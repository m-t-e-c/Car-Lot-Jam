using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayers;

        private LevelGenerator _levelGenerator;
        private Camera _mainCamera;
        private Stickman _currentStickman;
        
        private bool _isStickmanLeftFromCar;

        private void Start()
        {
            _levelGenerator = Locator.Instance.Resolve<LevelGenerator>();
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
            if (ReferenceEquals(_currentStickman, stickman))
            {
                ResetStickman();
                return;
            }
            
            _currentStickman?.CancelSelection();
            _currentStickman = stickman;
            _currentStickman.SetSelected();
        }

        private void HandleGroundClick(Ground ground)
        {
            if (ReferenceEquals(_currentStickman, null)) return;

            if (ground.isOccupied)
            {
                return;
            }
            
            Vector2Int groundCoord = ground.GetCoordinates();
            bool hasPath = _currentStickman.MoveTo(groundCoord, null, OnPathFailed);

            if (hasPath)
            {
                ground.Highlight(true);
            }
            else
            {
                _currentStickman.PlayAngerEmoji();
            }
            
            ResetStickman();
        }

        private void OnPathFailed(Vector2Int lastReachableNodeCoordinate)
        {
            var lastReachableGround = _levelGenerator.GetGroundByCoordinate(lastReachableNodeCoordinate);
            lastReachableGround.Highlight(false);
        }

        private void HandleCarClick(Car car)
        {
            if (ReferenceEquals(_currentStickman, null))
                return;

            if (!_currentStickman.GetColor().Equals(car.GetColor()))
            {
                car.Highlight(false);
                ResetStickman();
                return;
            }
            
            car.SetStickman(_currentStickman);
            ResetStickman();
        }

        private void ResetStickman()
        {
            _currentStickman.CancelSelection();
            _currentStickman = null;
        }
    }
}