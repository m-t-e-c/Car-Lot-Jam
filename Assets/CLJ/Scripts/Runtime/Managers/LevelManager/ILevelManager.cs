using System;
using CLJ.Runtime.Level;

namespace CLJ.Runtime.Managers.LevelManager
{
    public interface ILevelManager
    {
        public event Action<LevelGrid> OnLevelLoad;
        public void LoadLevelByIndex(int index);
        public void LoadCurrentLevel();
        public void NextLevel();
        public void RestartLevel();
        public int GetCurrentLevelIndex();
    }
}