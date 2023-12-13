using System;

namespace CLJ.Runtime.Level
{
    [Serializable]
    public class LevelGrid
    {
        public int Width;
        public int Height;
        public GridCell[,] Cells;

        public LevelGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new GridCell[width, height];
        }
    }
}