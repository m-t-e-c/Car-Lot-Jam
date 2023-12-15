using CLJ.Managers.LevelManager;

namespace CLJ.Runtime.Models
{
    public class GameplayModel
    {
        public int CurrentLevel { get; private set; }
        
        private readonly ILevelManager _levelManager;

        public GameplayModel()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            CurrentLevel = _levelManager.GetCurrentLevelIndex();
        }
        
        public void RestartLevel()
        {
            _levelManager.RestartLevel();
        }
    }
}