using UnityEngine;

namespace CLJ.Runtime
{
    public enum CameraType
    {
        Orthographic,
        Perspective
    }

    public class CameraHolder : MonoBehaviour
    {
        [SerializeField] private Transform otrhographicCameraTransform;
        [SerializeField] private Transform perspectiveCameraTransform;
        [SerializeField] private float orthographicSize;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        public void Init(CameraType cameraType)
        {
            var cameraTransform = _camera.transform;
            if (cameraType.Equals(CameraType.Perspective))
            {
                _camera.orthographic = false;
                cameraTransform.position = perspectiveCameraTransform.position;
                cameraTransform.rotation = perspectiveCameraTransform.rotation;
            }
            else if (cameraType.Equals(CameraType.Orthographic))
            {
                _camera.orthographic = true;
                _camera.orthographicSize = orthographicSize;
                cameraTransform.position = otrhographicCameraTransform.position;
                cameraTransform.rotation = otrhographicCameraTransform.rotation;
            }
        }
    }
}