using UnityEngine;

namespace DPBomberman.Models.Map
{
    public class MapGrid
    {
        public int Width { get; }
        public int Height { get; }

        private readonly CellType[,] cells;

        public MapGrid(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new CellType[width, height];

            Fill(CellType.Ground);
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public CellType GetCell(int x, int y)
        {
            if (!InBounds(x, y))
                return CellType.Empty;

            return cells[x, y];
        }

        public void SetCell(int x, int y, CellType type)
        {
            if (!InBounds(x, y))
                return;

            cells[x, y] = type;
        }

        public void Fill(CellType type)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    cells[x, y] = type;
                }
            }
        }
    }
}