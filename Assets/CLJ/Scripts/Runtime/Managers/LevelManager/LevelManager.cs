using System;
using CLJ.Runtime.Level;
using CLJ.Runtime.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLJ.Runtime.Managers.LevelManager
{
    public class LevelManager : ILevelManager
    {
        public event Action<LevelGrid> OnLevelLoad;
        
        private int _currentLevelIndex;
        private const string CURRENT_LEVEL_INDEX_KEY = "current_level_index";

        public LevelManager()
        {
            _currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_INDEX_KEY,1);
        }

        // For testing purposes
        public void LoadLevelByIndex(int index)
        {
            var level = LevelSaveSystem.LoadLevel(index);
            OnLevelLoad?.Invoke(level);
        }

        public void LoadCurrentLevel()
        {
            var level = LevelSaveSystem.LoadLevel(_currentLevelIndex);
            OnLevelLoad?.Invoke(level);
        }

        public void NextLevel()
        {
            _currentLevelIndex++;
            if (!LevelSaveSystem.IsLevelExists(_currentLevelIndex))
            {
                _currentLevelIndex = 1;
            }
            
            PlayerPrefs.SetInt(CURRENT_LEVEL_INDEX_KEY, _currentLevelIndex);
           RestartLevel();
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public int GetCurrentLevelIndex()
        {
            return _currentLevelIndex;
        }
    }
}