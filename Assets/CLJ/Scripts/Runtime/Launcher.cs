using CLJ.Managers.ViewManager;
using CLJ.Managers.LevelManager;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private ILevelManager _levelManager;
        private IViewManager _viewManager;

        private void Awake()
        {
            _levelManager = new LevelManager();
            Locator.Instance.Register<ILevelManager>(_levelManager);
            
            _viewManager = new ViewManager();
            Locator.Instance.Register<IViewManager>(_viewManager);

        }
    }
}