using System;

namespace CLJ.Runtime.Level
{
    [Serializable]
    public class LevelGrid
    {
        public int width;
        public int height;
        public GridCell[,] cells;

        public LevelGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            cells = new GridCell[width, height];
        }
    }
}