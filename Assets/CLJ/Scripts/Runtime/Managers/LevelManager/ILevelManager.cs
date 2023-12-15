using System;
using CLJ.Runtime.Level;

namespace CLJ.Managers.LevelManager
{
    public interface ILevelManager
    {
        public event Action<LevelGrid> OnLevelLoad; 
        public void LoadCurrentLevel();
        public void NextLevel();
        public void RestartLevel();
        public int GetCurrentLevelIndex();
    }
}