using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class GridObjectColorSetter : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Material[] _colorMaterials;
        
        public void SetColor(CellColor cellColor)
        {
            foreach (Renderer renderer in _renderers)
            {
                renderer.sharedMaterial = _colorMaterials[(int)cellColor -1];
            }
        }
    }
}