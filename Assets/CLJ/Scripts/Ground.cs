using UnityEngine;

namespace CLJ.Scripts
{
    public class Ground : MonoBehaviour
    {
        Vector2Int _coordinates;
        
        public void SetCoordinates(int x, int y)
        {
            _coordinates = new Vector2Int(x, y);
        }
        
        public Vector2Int GetCoordinates()
        {
            return _coordinates;
        }
    }
}