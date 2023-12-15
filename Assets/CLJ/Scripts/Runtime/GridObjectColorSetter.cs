using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.Runtime
{
    public class GridObjectColorSetter : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material[] colorMaterials;
        
        public void SetColor(CellColor cellColor)
        {
            foreach (Renderer rnd in renderers)
            {
                rnd.sharedMaterial = colorMaterials[(int)cellColor -1];
            }
        }
    }
}