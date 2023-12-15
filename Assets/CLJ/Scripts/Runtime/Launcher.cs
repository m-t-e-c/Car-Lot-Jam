using CLJ.Managers.ViewManager;
using CLJ.Managers.LevelManager;
using CLJ.Runtime.Views;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Launcher : MonoBehaviour
    {
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
                _levelManager.LoadCurrentLevel();
            });
        }
    }
}