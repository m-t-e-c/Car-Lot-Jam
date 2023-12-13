using DG.Tweening;
using UnityEngine;

namespace CLJ.Runtime
{
    public class Ground : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        private Material _material;
        private Vector2Int _coordinates;

        private void Start()
        {
            _material = new Material(_renderer.sharedMaterial);
            _renderer.sharedMaterial = _material;
        }

        public void Highlight(bool canMovable)
        {
            if (canMovable)
            {
                _renderer.sharedMaterial.DOColor(new Color(0.35f, 1f, 0.43f), 0.3f).SetLoops(2, LoopType.Yoyo);
            }
            else
            {
                _renderer.sharedMaterial.DOColor(new Color(1f, 0.28f, 0.28f), 0.3f).SetLoops(2, LoopType.Yoyo);
            }
        }
        
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