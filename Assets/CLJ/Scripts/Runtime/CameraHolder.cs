using System;
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
        [SerializeField] private Transform _otrhographicCameraTransform;
        [SerializeField] private Transform _perspectiveCameraTransform;
        [SerializeField] private float _orthographicSize;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        public void SetCamera(CameraType cameraType)
        {
            var cameraTransform = _camera.transform;
            if (cameraType.Equals(CameraType.Perspective))
            {
                _camera.orthographic = false;
                cameraTransform.position = _perspectiveCameraTransform.position;
                cameraTransform.rotation = _perspectiveCameraTransform.rotation;
            }
            else if (cameraType.Equals(CameraType.Orthographic))
            {
                _camera.orthographic = true;
                _camera.orthographicSize = _orthographicSize;
                cameraTransform.position = _otrhographicCameraTransform.position;
                cameraTransform.rotation = _otrhographicCameraTransform.rotation;
            }
        }
    }
}