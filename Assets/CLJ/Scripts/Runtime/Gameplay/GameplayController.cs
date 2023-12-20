using UnityEngine;

namespace CLJ.Runtime
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayers;

        private Camera _mainCamera;
        private Stickman _currentStickman;
        
        private bool _isStickmanLeftFromCar;

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
            bool hasPath = _currentStickman.MoveTo(groundCoord);

            if (!hasPath)
            {
                _currentStickman.PlayAngerEmoji();
            }
            
            ground.Highlight(hasPath);
            ResetStickman();
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