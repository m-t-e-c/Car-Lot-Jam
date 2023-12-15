using System;

namespace CLJ.Runtime.Level
{
    public enum GridObjectType
    {
        None,
        SmallCar,
        LongCar,
        Stickman,
        Cone,
        Barrier
    }
    
    [Serializable]
    public class GridObject
    {
        public string title;
        public GridObjectType gridObjectType;
        public int gridSpace;
    }
}