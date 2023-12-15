using CLJ.Managers.ViewManager;
using CLJ.Runtime.Views;
using UnityEngine;

namespace CLJ.Runtime.Managers.GameManager
{
    public class GameManager : MonoBehaviour
    {
        private IViewManager _viewManager;

        private int _spawnedCarCount;

        private void Start()
        {
            _viewManager = Locator.Instance.Resolve<IViewManager>();
            
            GameEvents.OnCarSpawned += OnCarSpawned;
            GameEvents.OnCarPassedThroughTheGate += OnCarPassedThroughTheGate;
        }

        private void OnCarSpawned()
        {
            _spawnedCarCount++;
        }

        private void OnCarPassedThroughTheGate()
        {
            _spawnedCarCount--;
            if (_spawnedCarCount.Equals(0))
            {
                // Game Over
                _viewManager.CloseView<GameplayPresenter>();
                _viewManager.LoadView<GameOverPresenter>();
            }
        }

        private void OnDisable()
        {
            GameEvents.OnCarSpawned -= OnCarSpawned;
            GameEvents.OnCarPassedThroughTheGate -= OnCarPassedThroughTheGate;
        }
    }
}