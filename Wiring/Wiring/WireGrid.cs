using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring
{
    class WireGrid
    {
        /*
         * Wires
         * 0: Empty
         * 1-4: Endpoints. Left, up, right, down.
         * 5-8: Corners. Left/up, up/right, right/down, down/left.
         * 9-12: Ts. Up/down/left, left/right/up, up/down/right, left/right/down.
         * 13-14: Straights. Left/right, up/down.
         * 15: + bridge.
         * 16: + intersection.
         */
        private Tile[,] grid;
        private int width;
        private int height;
        public int zoomlevel;
        public int xcenter;
        public int ycenter;
        public WireGrid()
        {
            width = 101;
            height = 101;
            xcenter = width / 2;
            ycenter = height / 2;
            zoomlevel = 40;
            grid = new Tile[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Tile();
                    grid[x, y].type = x%17;
                }
        }
        public void render(Graphics g, Size sz)
        {
            SolidBrush b = new SolidBrush(Color.FromArgb(63, 63, 63));
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    grid[x, y].render(g, x * (zoomlevel-1), y * (zoomlevel-1), zoomlevel);
        }
    }
}
