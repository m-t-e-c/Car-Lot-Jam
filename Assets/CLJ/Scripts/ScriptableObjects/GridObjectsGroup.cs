using System.Collections.Generic;
using CLJ.Runtime.Level;
using UnityEngine;

namespace CLJ.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Grid Objects Group", menuName = "Create Grid Objects Group", order = 0)]
    public class GridObjectsGroup : ScriptableObject
    {
        [field:SerializeField] public List<GridObject> GridObjects { get; private set; }
    }
}