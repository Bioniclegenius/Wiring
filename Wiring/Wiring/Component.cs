using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring
{
    public class Component
    {
        private int dir;
        public List<List<int>> truthtable;
        public double tempVal;
        public double thisVal;
        public int type;
        public int x, y;
        public Point size;

        public Component(int width, int height, int xi = 0, int yi = 0, int t = 0, int orientation = 0)
        {
            x = xi;
            y = yi;
            type = t;
            tempVal = 0;
            thisVal = 0;
            dir = orientation % 4;//0 = right, 1 = down, 2 = left, 3 = up - in 0, inputs on left, outputs on right
            truthtable = new List<List<int>>();
            makeSize();
            if (x + size.X >= width)
                x = width - size.X - 1;
            if (y + size.Y >= height)
                y = height - size.Y - 1;
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
        }

        private void makeSize()
        {
            switch (type)
            {
                case ComponentTypes.Diode:
                case ComponentTypes.Inverter:
                    if (dir == 0 || dir == 2)
                        size = new Point(2, 1);
                    else
                        size = new Point(1, 2);
                    break;
                case ComponentTypes.Or:
                case ComponentTypes.And:
                case ComponentTypes.Lamp2x2:
                case ComponentTypes.BigToggle:
                    size = new Point(2, 2);
                    break;
                default://Anything else
                    size = new Point(1, 1);
                    break;
            }
        }

        public void Load(string filename)
        {
            if (type == ComponentTypes.Chip)
            {
                var lines = File.ReadAllLines(filename);
                truthtable = new List<List<int>>();
                for (int x = 0; x < lines.Length; x++)
                {
                    truthtable.Add(new List<int>());
                    var nums = lines[x].Split(' ');
                    for (int y = 0; y < nums.Length; y++)
                        truthtable[x].Add(nums[y] == "0" ? 0 : 1);
                }
                size = new Point(2, (int)(Math.Max(Math.Sqrt(truthtable.Count()), truthtable[0].Count())));
            }
        }

        public void Load(Component c)
        {
            truthtable = new List<List<int>>();
            for (int x = 0; x < c.truthtable.Count(); x++)
            {
                truthtable.Add(new List<int>());
                for (int y = 0; y < c.truthtable[x].Count(); y++)
                    truthtable[x].Add(c.truthtable[x][y]);
            }
            size = new Point(2, (int)(Math.Max(Math.Sqrt(truthtable.Count()), truthtable[0].Count())));
        }

        public void clip(Tile[,] grid, int width, int height)
        {
            for (int xx = 0; xx < size.X; xx++)
                for (int yy = 0; yy < size.Y; yy++)
                {
                    if (xx > 0)
                        grid[xx + x, yy + y].subDir(0);
                    if (yy > 0)
                        grid[xx + x, yy + y].subDir(1);
                    if (xx < size.X - 1)
                        grid[xx + x, yy + y].subDir(2);
                    if (yy < size.Y - 1)
                        grid[xx + x, yy + y].subDir(3);
                }
        }

        public List<Point> eval(Tile[,] grid, int width, int height)
        {
            List<Point> outputs = new List<Point>();
            double val;
            tempVal = 0;
            switch (type)
            {

                #region Power sources

                case ComponentTypes.Power1:
                case ComponentTypes.Power2:
                case ComponentTypes.Power3:
                    grid[x, y].power(ComponentTypes.powerOutput(type));
                    outputs.Add(new Point(x, y));
                    break;

                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                    for (int xx = 0; xx < size.X; xx++)
                        for (int yy = 0; yy < size.Y; yy++)
                        {
                            grid[x + xx, y + yy].power(thisVal);
                            outputs.Add(new Point(x + xx, y + yy));
                        }
                    break;

                #endregion

                #region Or gate

                case ComponentTypes.Or:
                    switch (dir)
                    {
                        case 1://down
                            grid[x + 1, y + 1].power(Math.Max(grid[x, y].getPower(), grid[x + 1, y].getPower()));
                            outputs.Add(new Point(x + 1, y + 1));
                            break;
                        case 2://left
                            grid[x, y + 1].power(Math.Max(grid[x + 1, y].getPower(), grid[x + 1, y + 1].getPower()));
                            outputs.Add(new Point(x, y + 1));
                            break;
                        case 3://up
                            grid[x, y].power(Math.Max(grid[x, y + 1].getPower(), grid[x + 1, y + 1].getPower()));
                            outputs.Add(new Point(x, y));
                            break;
                        default://right
                            grid[x + 1, y].power(Math.Max(grid[x, y].getPower(), grid[x, y + 1].getPower()));
                            outputs.Add(new Point(x + 1, y));
                            break;
                    }
                    break;

                #endregion

                #region And gate

                case ComponentTypes.And:
                    switch (dir)
                    {
                        case 1://down
                            grid[x + 1, y + 1].power(Math.Min(grid[x, y].getPower(), grid[x + 1, y].getPower()));
                            outputs.Add(new Point(x + 1, y + 1));
                            break;
                        case 2://left
                            grid[x, y + 1].power(Math.Min(grid[x + 1, y].getPower(), grid[x + 1, y + 1].getPower()));
                            outputs.Add(new Point(x, y + 1));
                            break;
                        case 3://up
                            grid[x, y].power(Math.Min(grid[x, y + 1].getPower(), grid[x + 1, y + 1].getPower()));
                            outputs.Add(new Point(x, y));
                            break;
                        default://right
                            grid[x + 1, y].power(Math.Min(grid[x, y].getPower(), grid[x, y + 1].getPower()));
                            outputs.Add(new Point(x + 1, y));
                            break;
                    }
                    break;

                #endregion

                #region Inverter

                case ComponentTypes.Inverter:
                    switch (dir)
                    {
                        case 1://down
                            val = grid[x, y].getPower();
                            grid[x, y + 1].power(val == 0 ? ComponentTypes.powerOutput(type) : 0);
                            outputs.Add(new Point(x, y + 1));
                            break;
                        case 2://left
                            val = grid[x + 1, y].getPower();
                            grid[x, y].power(val == 0 ? ComponentTypes.powerOutput(type) : 0);
                            outputs.Add(new Point(x, y));
                            break;
                        case 3://up
                            val = grid[x, y + 1].getPower();
                            grid[x, y].power(val == 0 ? ComponentTypes.powerOutput(type) : 0);
                            outputs.Add(new Point(x, y));
                            break;
                        default://right
                            val = grid[x, y].getPower();
                            grid[x + 1, y].power(val == 0 ? ComponentTypes.powerOutput(type) : 0);
                            outputs.Add(new Point(x + 1, y));
                            break;
                    }
                    break;

                #endregion

                #region 1x1 Lamps

                case ComponentTypes.Lamp1x1:
                    tempVal = grid[x, y].getPower();
                    break;

                #endregion

                #region 2x2 Lamps

                case ComponentTypes.Lamp2x2:
                    tempVal = Math.Max(Math.Max(grid[x, y].getPower(), grid[x + 1, y].getPower()),
                                       Math.Max(grid[x, y + 1].getPower(), grid[x + 1, y + 1].getPower()));
                    break;

                #endregion

                #region Diode

                case ComponentTypes.Diode:
                    switch (dir)
                    {
                        case 1://down
                            grid[x, y + 1].power(grid[x, y].getPower());
                            outputs.Add(new Point(x, y + 1));
                            break;
                        case 2://left
                            grid[x, y].power(grid[x + 1, y].getPower());
                            outputs.Add(new Point(x, y));
                            break;
                        case 3://up
                            grid[x, y].power(grid[x, y + 1].getPower());
                            outputs.Add(new Point(x, y));
                            break;
                        default://right
                            grid[x + 1, y].power(grid[x, y].getPower());
                            outputs.Add(new Point(x + 1, y));
                            break;
                    }
                    break;

                #endregion

                default:
                    break;

            }
            return outputs;
        }

        public void render(Graphics g, Size sz, int scrx, int scry, int zoomlevel)
        {
            Bitmap image;
            SolidBrush b = new SolidBrush(Color.FromArgb(0, 0, 0));
            try
            {
                image = (Bitmap)Image.FromFile(ComponentTypes.imageFileName(type));
                switch (type)
                {
                    case ComponentTypes.Diode://diode
                        switch (dir)
                        {
                            case 0:
                            case 2:
                                g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel), new RectangleF(0, 20 * dir, 80, 40), GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel * 2), new RectangleF(dir == 1 ? 80 : 120, 0, 40, 80), GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                    case ComponentTypes.Power1:
                    case ComponentTypes.Power2:
                    case ComponentTypes.Power3:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel));
                        break;
                    case ComponentTypes.Or:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel * 2), new RectangleF(80 * (dir % 2), (dir < 2 ? 0 : 80), 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.And:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel * 2), new RectangleF(80 * (dir % 2), (dir < 2 ? 0 : 80), 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.Inverter:
                        switch (dir)
                        {
                            case 0:
                            case 2:
                                g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel), new RectangleF(0, 20 * dir, 80, 40), GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel * 2), new RectangleF(dir == 1 ? 80 : 120, 0, 40, 80), GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                    case ComponentTypes.Lamp1x1:
                        b.Color = Color.FromArgb((int)(Math.Min(Math.Max(Tile.baseColor + tempVal * (255 - Tile.baseColor), 0), 255)),
                                                 (int)(Math.Min(Math.Max((tempVal - 1) * 255, 0), 255)),
                                                 (int)(Math.Min(Math.Max((tempVal - 2) * 255, 0), 255)));
                        if (tempVal > 0)
                            g.FillRectangle(b, scrx, scry, zoomlevel, zoomlevel);
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel), new RectangleF(0, 0, 40, 40), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.Lamp2x2:
                        b.Color = Color.FromArgb((int)(Math.Min(Math.Max(Tile.baseColor + tempVal * (255 - Tile.baseColor), 0), 255)),
                                                 (int)(Math.Min(Math.Max((tempVal - 1) * 255, 0), 255)),
                                                 (int)(Math.Min(Math.Max((tempVal - 2) * 255, 0), 255)));
                        if (tempVal > 0)
                            g.FillRectangle(b, scrx, scry, zoomlevel * 2, zoomlevel * 2);
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel * 2), new RectangleF(0, 0, 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.Toggle:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel), new RectangleF(thisVal == 0 ? 0 : 40, 0, 40, 40), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.BigToggle:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel * 2, zoomlevel * 2), new RectangleF(thisVal == 0 ? 0 : 80, 0, 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.Chip:
                        if (thisVal == 1)
                        {
                            b.Color = Color.FromArgb(255, 255, 0);
                            g.FillRectangle(b, scrx + 1, scry + 1, (zoomlevel - 2) * size.X - 1 + size.X, (zoomlevel - 2) * size.Y - 1 + size.Y);
                        }
                        switch (dir)
                        {
                            case 0:
                                for (int xx = 0; xx < 2; xx++)
                                    for (int yy = 0; yy < size.Y; yy++)
                                    {
                                        switch (xx)
                                        {
                                            case 0:
                                                g.DrawImage(image, new RectangleF(scrx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(0, yy < Math.Sqrt(truthtable.Count()) ? 0 : 80, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                            case 1:
                                                g.DrawImage(image, new RectangleF(scrx + zoomlevel - 1, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(0, yy < truthtable[0].Count() ? 40 : 120, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                        }
                                    }
                                b.Color = Color.FromArgb(0, 0, 160);
                                g.FillRectangle(b, scrx, scry, (zoomlevel - 1) * (size.X) - 1, zoomlevel / 10);
                                g.FillRectangle(b, scrx, scry + (zoomlevel - 1) * (size.Y) + 1 - zoomlevel / 10, (zoomlevel - 1) * (size.X) - 1, zoomlevel / 10);
                                break;
                            case 1:
                                for (int yy = 0; yy < 2; yy++)
                                    for (int xx = 0; xx < size.X; xx++)
                                    {
                                        switch (yy)
                                        {
                                            case 0:
                                                g.DrawImage(image, new RectangleF(scrx + (zoomlevel - 1) * xx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(40, xx >= size.X - Math.Sqrt(truthtable.Count()) ? 0 : 80, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                            case 1:
                                                g.DrawImage(image, new RectangleF(scrx + (zoomlevel - 1) * xx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(40, xx >= size.X - truthtable[0].Count() ? 40 : 120, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                        }
                                    }
                                b.Color = Color.FromArgb(0, 0, 160);
                                g.FillRectangle(b, scrx, scry, zoomlevel/10, (zoomlevel - 1) * (size.Y) - 1);
                                g.FillRectangle(b, scrx+(zoomlevel-1)*(size.X)+1-zoomlevel/10, scry,zoomlevel/10, (zoomlevel - 1) * (size.Y) - 1);
                                break;
                            case 2:
                                for (int xx = 0; xx < 2; xx++)
                                    for (int yy = 0; yy < size.Y; yy++)
                                    {
                                        switch (xx)
                                        {
                                            case 0:
                                                g.DrawImage(image, new RectangleF(scrx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(0, yy >= size.Y - truthtable[0].Count() ? 0 : 80, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                            case 1:
                                                g.DrawImage(image, new RectangleF(scrx + zoomlevel - 1, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(0, yy >= size.Y - Math.Sqrt(truthtable.Count()) ? 40 : 120, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                        }
                                    }
                                b.Color = Color.FromArgb(0, 0, 160);
                                g.FillRectangle(b, scrx, scry, (zoomlevel - 1) * (size.X) - 1, zoomlevel / 10);
                                g.FillRectangle(b, scrx, scry + (zoomlevel - 1) * (size.Y) + 1 - zoomlevel / 10, (zoomlevel - 1) * (size.X) - 1, zoomlevel / 10);
                                break;
                            case 3:
                                for (int yy = 0; yy < 2; yy++)
                                    for (int xx = 0; xx < size.X; xx++)
                                    {
                                        switch (yy)
                                        {
                                            case 0:
                                                g.DrawImage(image, new RectangleF(scrx + (zoomlevel - 1) * xx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(40, xx < truthtable[0].Count() ? 0 : 80, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                            case 1:
                                                g.DrawImage(image, new RectangleF(scrx + (zoomlevel - 1) * xx, scry + (zoomlevel - 1) * yy, zoomlevel, zoomlevel), new RectangleF(40, xx < Math.Sqrt(truthtable.Count()) ? 40 : 120, 40, 40), GraphicsUnit.Pixel);
                                                break;
                                        }
                                    }
                                b.Color = Color.FromArgb(0, 0, 160);
                                g.FillRectangle(b, scrx, scry, zoomlevel / 10, (zoomlevel - 1) * (size.Y) - 1);
                                g.FillRectangle(b, scrx + (zoomlevel - 1) * (size.X) + 1 - zoomlevel / 10, scry, zoomlevel / 10, (zoomlevel - 1) * (size.Y) - 1);
                                break;
                            default:
                                throw new Exception();
                        }
                        break;
                    default:
                        throw new Exception();
                }
                image.Dispose();
            }
            catch (Exception)
            {
                b.Color = Color.FromArgb(0, 255, 255);
                g.FillRectangle(b, scrx + 1, scry + 1, (zoomlevel - 2) * size.X - 1 + size.X, (zoomlevel - 2) * size.Y - 1 + size.Y);
            }
            /*Font f = new Font("Arial",12);
            b.Color = Color.FromArgb(255,255,255);
            g.DrawString(string.Format("{0}",thisVal),f,b,new PointF(scrx,scry));*/
        }

        public void click()
        {
            switch (type)
            {
                case ComponentTypes.Diode:
                case ComponentTypes.Inverter:
                case ComponentTypes.And:
                case ComponentTypes.Or:
                    dir = (dir + 1) % 4;
                    makeSize();
                    break;
                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                    thisVal = ComponentTypes.powerOutput(type) - thisVal;
                    break;
                case ComponentTypes.Chip:
                    dir = (dir + 1) % 4;
                    size = new Point(size.Y, size.X);
                    break;
                default:
                    break;
            }
        }

    }
}
