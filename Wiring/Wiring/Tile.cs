﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring
{
    class Tile
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
        public int type;
        public float signal,signal2;
        public Tile()
        {
            type = 0;
            signal = 0;
            signal2 = 0;
        }

        public void power(int p = 0)
        {
            if (p > signal)
                signal = p;
        }

        public void power(int pl = 0, int pu = 0, int pr = 0, int pd = 0)
        {
            var left  = new int[] {  1,  5,  8,  9, 10, 12, 13, 15 };
            var up    = new int[] {  2,  5,  6,  9, 10, 11, 14, 15 };
            var right = new int[] {  3,  6,  7, 10, 11, 12, 13, 15 };
            var down  = new int[] {  4,  7,  8,  9, 11, 12, 14, 15 };
            for (int x = 0; x < 8; x++)
                if (type == left[x] && pl > signal)
                    signal = pl;
            for (int x = 0; x < 8; x++)
                if (type == up[x] && pu > signal)
                    signal = pu;
            for (int x = 0; x < 8; x++)
                if (type == right[x] && pr > signal)
                    signal = pr;
            for (int x = 0; x < 8; x++)
                if (type == down[x] && pd > signal)
                    signal = pd;
            if (type == 16) {
                if (pl > signal)
                    signal = pl;
                if (pr > signal)
                    signal = pr;
                if (pu > signal2)
                    signal2 = pu;
                if (pd > signal2)
                    signal2 = pd;
            }
        }
        /// <summary>
        /// Returns the signal strength of this particular wire.
        /// </summary>
        /// <param name="dir">The direction that it outputs power. 0 = left, 1 = up, 2 = right, 3 = down.</param>
        /// <returns>Electrical strength here. 0 if not powered, 1 at full.</returns>
        public float getPower(int dir)
        {
            var left  = new int[] {  1,  5,  8,  9, 10, 12, 13, 15 };
            var up    = new int[] {  2,  5,  6,  9, 10, 11, 14, 15 };
            var right = new int[] {  3,  6,  7, 10, 11, 12, 13, 15 };
            var down  = new int[] {  4,  7,  8,  9, 11, 12, 14, 15 };
            if (dir == 0) {
                for (int x = 0; x < 8; x++)
                    if (type == left[x])
                        return signal;
            }
            if (dir == 1) {
                for (int x = 0; x < 8; x++)
                    if (type == up[x])
                        return signal;
            }
            if (dir == 2) {
                for (int x = 0; x < 8; x++)
                    if (type == right[x])
                        return signal;
            }
            if (dir == 3) {
                for (int x = 0; x < 8; x++)
                    if (type == down[x])
                        return signal;
            }
            if (type == 16)
            {
                if (dir == 0 || dir == 2)
                    return signal;
                if (dir == 1 || dir == 3)
                    return signal2;
            }
            return 0;
        }
        /// <summary>
        /// Returns if a tile goes a particular direction
        /// </summary>
        /// <param name="dir">Which direction to check. 0 = left, 1 = up, 2 = right, 3 = down.</param>
        public bool canDir(int dir)
        {
            var left  = new int[] {  1,  5,  8,  9, 10, 12, 13, 15, 16 };
            var up    = new int[] {  2,  5,  6,  9, 10, 11, 14, 15, 16 };
            var right = new int[] {  3,  6,  7, 10, 11, 12, 13, 15, 16 };
            var down  = new int[] {  4,  7,  8,  9, 11, 12, 14, 15, 16 };
            if (dir == 0)
            {
            }
            return false;
        }
        public void reset()
        {
            signal = 0;
            signal2 = 0;
        }
        public void render(Graphics g, int x, int y, int zoomlevel)
        {
            SolidBrush b = new SolidBrush(Color.FromArgb(63, 63, 63));
            g.FillRectangle(b, x, y, zoomlevel, zoomlevel);
            b.Color = Color.FromArgb(0, 0, 0);
            g.FillRectangle(b, x + 1, y + 1, zoomlevel - 2, zoomlevel - 2);
            b.Color = Color.FromArgb((int)(Math.Min(Math.Max(128 * signal + 127, 0), 255)), 0, 0);
            switch (type)
            {
                case 1:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 2:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), 2 * zoomlevel / 5, zoomlevel / 5);
                    break;
                case 3:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 4:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), 2 * zoomlevel / 5, zoomlevel / 5);
                    break;
                case 5:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    break;
                case 6:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    break;
                case 7:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    break;
                case 8:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    break;
                case 9:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    break;
                case 10:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    break;
                case 11:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    break;
                case 12:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    break;
                case 13:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    break;
                case 14:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    break;
                case 15:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 5), 2 * zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 16:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 5), 2 * zoomlevel / 5, 2 * zoomlevel / 5);
                    b.Color = Color.FromArgb(0, 0, 0);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 5);
                    break;
                default:
                    break;
            }
        }
    }
}
