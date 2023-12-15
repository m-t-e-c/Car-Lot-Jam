using CLJ.Managers.LevelManager;

namespace CLJ.Runtime.Models
{
    public class GameOverModel
    {
        public int CurrentLevelIndex { get; private set; }
        
        private readonly ILevelManager _levelManager;
        
        public GameOverModel()
        {
            _levelManager = Locator.Instance.Resolve<ILevelManager>();
            CurrentLevelIndex = _levelManager.GetCurrentLevelIndex();
        }
        
        public void LoadNextLevel()
        {
            _levelManager.NextLevel();
        }
    }
}