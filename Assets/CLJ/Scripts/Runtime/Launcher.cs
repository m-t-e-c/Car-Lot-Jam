using CLJ.Runtime.Managers.LevelManager;
using CLJ.Runtime.Managers.ViewManager;
using CLJ.Runtime.Presenters;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Launcher : MonoBehaviour
    {
        public bool OverrideLevel = false; // For testing purposes
        public int OverrideLevelIndex = 0; // For testing purposes
        
        [SerializeField] private CameraHolder cameraHolder;
        
        private ILevelManager _levelManager;
        private IViewManager _viewManager;

        private void Awake()
        {
            RegisterManagers();
            RegisterMonoReferences();
        }

        private void RegisterManagers()
        {
            _levelManager = new LevelManager();
            Locator.Instance.Register<ILevelManager>(_levelManager);
            
            _viewManager = new ViewManager();
            Locator.Instance.Register<IViewManager>(_viewManager);
        }

        private void RegisterMonoReferences()
        {
            Locator.Instance.Register<CameraHolder>(cameraHolder);
        }
        
        private void Start()
        {
            _viewManager.LoadView<GameplayPresenter>((view) =>
            {
                if (OverrideLevel)
                {
                    _levelManager.LoadLevelByIndex(OverrideLevelIndex);
                }
                else
                {
                    _levelManager.LoadCurrentLevel();
                }
            });
        }
    }
}