using CLJ.Runtime.Level;
using Newtonsoft.Json;
using UnityEngine;

namespace CLJ.Runtime.Utils
{
    public class LevelSaveSystem
    {
        public static LevelGrid LoadLevel(int levelIndex)
        {
            var json =
                System.IO.File.ReadAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
            return JsonConvert.DeserializeObject<LevelGrid>(json);
        }
        
        public static void SaveGrid(LevelGrid levelGrid, int levelIndex)
        {
            string json = JsonConvert.SerializeObject(levelGrid);
            System.IO.File.WriteAllText(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json", json);
        }
        
        public static bool IsLevelExists(int levelIndex)
        {
            return System.IO.File.Exists(Application.dataPath + $"/CLJ/LevelData/LevelGrid{levelIndex}.json");
        }
    }
}