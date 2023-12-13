using System;
using CLJ.Managers.ViewManager;
using CLJ.Managers.LevelManager;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private CameraHolder cameraHolder;
        
        private ILevelManager _levelManager;
        private IViewManager _viewManager;
        
        [SerializeField] private int _levelIndex = 1;

        private void Awake()
        {
            _levelManager = new LevelManager(_levelIndex);
            Locator.Instance.Register<ILevelManager>(_levelManager);
            
            _viewManager = new ViewManager();
            Locator.Instance.Register<IViewManager>(_viewManager);
            
            Locator.Instance.Register<CameraHolder>(cameraHolder);
        }
    }
}