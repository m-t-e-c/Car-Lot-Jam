using CLJ.Runtime.Level;

namespace CLJ.Managers.LevelManager
{
    public class LevelManager : ILevelManager
    {
        private int _levelIndex;
        
        public LevelManager(int levelIndex)
        {
            _levelIndex = levelIndex;
        }
        
        public LevelGrid GetLevelGrid()
        {
            return LevelCreator.LoadLevel(_levelIndex);
        }
    }
}