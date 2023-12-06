using System;
using UnityEngine;

namespace CLJ.Scripts
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactionLayers;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;
            
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, Mathf.Infinity, _interactionLayers);
            if (hitInfo.collider != null)
            {
                if (hitInfo.collider.TryGetComponent(out Stickman stickman))
                {
                    stickman.SetSelected();
                }
            }
        }
    }
}