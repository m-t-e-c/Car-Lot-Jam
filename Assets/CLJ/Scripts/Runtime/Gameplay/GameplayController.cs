using UnityEngine;

namespace CLJ.Runtime
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactionLayers;

        private Camera _camera;

        private Stickman _stickman;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || (_stickman && _stickman.isMoving))
                return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity, _interactionLayers);
            if (hitInfo.collider != null)
            {
                
                if (hitInfo.collider.TryGetComponent(out Stickman stickman))
                {
                    if (_stickman)
                    {
                        _stickman.CancelSelection();
                    }
                    
                    _stickman = stickman;
                    _stickman.SetSelected();
                }

                if (hitInfo.collider.TryGetComponent(out Ground ground))
                {
                    if (ReferenceEquals(_stickman,null))
                    {
                        return;
                    }
                    
                    var groundCoord = ground.GetCoordinates();
                    var hasPath = _stickman.MoveTo(groundCoord, () =>
                    {
                        _stickman.CancelSelection();
                        _stickman = null;
                    });

                    if (!hasPath)
                    {
                        _stickman.PlayAngerEmoji();
                    }
                    
                    ground.Highlight(hasPath);
                }

                if (hitInfo.collider.TryGetComponent(out Car car))
                {
                    if (ReferenceEquals(_stickman,null))
                    {
                        return;
                    }

                    if (car.GetAroundCell().Count.Equals(0))
                    {
                        _stickman.PlayAngerEmoji();
                    }
                    
                    foreach (Vector2Int carAroundCoord in car.GetAroundCell())
                    {
                        var hasPath = _stickman.MoveTo(carAroundCoord);
                        if (hasPath)
                        {
                            _stickman.PlayHappyEmoji();
                            car.Highlight();
                            break;
                        }
                    }
                }
            }
        }
    }
}