namespace CLJ.Runtime.Level
{
    public class LevelGrid
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public GridCell[,] Cells { get; }

        public LevelGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new GridCell[width, height];
        }
    }
}