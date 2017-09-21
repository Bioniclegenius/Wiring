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
        public List<double> sevseginputs;
        public List<double> thisVal;
        public double tempVal;
        public int type;
        public int x, y;
        public Point size;

        public Component(int width, int height, int xi = 0, int yi = 0, int t = 0, int orientation = 0)
        {
            x = xi;
            y = yi;
            type = t;
            tempVal = 0;
            thisVal = new List<double>();
            dir = orientation % 4;//0 = right, 1 = down, 2 = left, 3 = up - in 0, inputs on left, outputs on right
            truthtable = new List<List<int>>();
            sevseginputs = new List<double>();
            for (int x = 0; x < 7; x++)
                sevseginputs.Add(0);
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

        public string fingerprint(int xout,int yout, double power) {
            string fp = string.Format("{0}{1}{2}{3}{4}{5}",x.ToString("X8"),y.ToString("X8"),dir.ToString("X8"),xout.ToString("X8"),yout.ToString("X8"),Math.Min(power,3));
            return fp;
        }

        private void makeSize()
        {
            switch (type)
            {
                case ComponentTypes.Diode:
                case ComponentTypes.Inverter:
                case ComponentTypes.RisingEdge:
                    if (dir == 0 || dir == 2)
                        size = new Point(2, 1);
                    else
                        size = new Point(1, 2);
                    break;
                case ComponentTypes.And:
                case ComponentTypes.Or:
                case ComponentTypes.XOr:
                case ComponentTypes.Lamp2x2:
                case ComponentTypes.BigToggle:
                case ComponentTypes.DLatch:
                    size = new Point(2, 2);
                    break;
                case ComponentTypes.SevSeg:
                    size = new Point(4, 7);
                    break;
                default://Anything else
                    size = new Point(1, 1);
                    break;
            }
        }

        #region Load

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

        #endregion

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

        public List<PointFP> eval(Tile[,] grid, int width, int height, long time)
        {
            List<PointFP> outputs = new List<PointFP>();
            double val;
            tempVal = 0;
            switch (type)
            {

                #region Power sources

                case ComponentTypes.Power1:
                case ComponentTypes.Power2:
                case ComponentTypes.Power3:
                    outputs.Add(new PointFP(x, y,fingerprint(x,y,ComponentTypes.powerOutput(type))));
                    break;

                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                    if (thisVal.Count == 0)
                        thisVal.Add(0);
                    for (int xx = 0; xx < size.X; xx++)
                        for (int yy = 0; yy < size.Y; yy++)
                            outputs.Add(new PointFP(x + xx, y + yy,fingerprint(x,y,thisVal[0])));
                    break;

                #endregion

                #region And gate

                case ComponentTypes.And:
                    switch (dir)
                    {
                        case 1://down
                            double power = Math.Min(grid[x,y].getPower(),grid[x + 1,y].getPower());
                            outputs.Add(new PointFP(x + 1, y + 1,fingerprint(x+1,y+1,power)));
                            break;
                        case 2://left
                            power = Math.Min(grid[x + 1,y].getPower(),grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,power)));
                            break;
                        case 3://up
                            power = Math.Min(grid[x,y + 1].getPower(),grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,power)));
                            break;
                        default://right
                            power = Math.Min(grid[x,y].getPower(),grid[x,y + 1].getPower());
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,power)));
                            break;
                    }
                    break;

                #endregion

                #region Or gate

                case ComponentTypes.Or:
                    switch (dir)
                    {
                        case 1://down
                            double power = Math.Max(grid[x,y].getPower(),grid[x + 1,y].getPower());
                            outputs.Add(new PointFP(x + 1, y + 1,fingerprint(x+1,y+1,power)));
                            break;
                        case 2://left
                            power = Math.Max(grid[x + 1,y].getPower(),grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,power)));
                            break;
                        case 3://up
                            power = Math.Max(grid[x,y + 1].getPower(),grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,power)));
                            break;
                        default://right
                            power = Math.Max(grid[x,y].getPower(),grid[x,y + 1].getPower());
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,power)));
                            break;
                    }
                    break;

                #endregion

                #region XOr gate

                case ComponentTypes.XOr:
                    switch (dir)
                    {
                        case 1://down
                            double power = Math.Abs(grid[x,y].getPower() - grid[x + 1,y].getPower());
                            outputs.Add(new PointFP(x + 1, y + 1,fingerprint(x+1,y+1,power)));
                            break;
                        case 2://left
                            power = Math.Abs(grid[x + 1,y].getPower() - grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,power)));
                            break;
                        case 3://up
                            power = Math.Abs(grid[x,y + 1].getPower() - grid[x + 1,y + 1].getPower());
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,power)));
                            break;
                        default://right
                            power = Math.Abs(grid[x,y].getPower() - grid[x,y + 1].getPower());
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,power)));
                            break;
                    }
                    break;

                #endregion

                #region D Latch

                case ComponentTypes.DLatch:
                    if (thisVal.Count() == 0)
                        thisVal.Add(0);
                    if (grid[x, y].getPower() > 0)
                        thisVal[0] = grid[x, y+1].getPower();
                    outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,thisVal[0])));
                    break;

                #endregion

                #region Diode

                case ComponentTypes.Diode:
                    switch (dir)
                    {
                        case 1://down
                            double power = grid[x,y].getPower();
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,power)));
                            break;
                        case 2://left
                            power = grid[x + 1,y].getPower();
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,power)));
                            break;
                        case 3://up
                            power = grid[x,y + 1].getPower();
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,power)));
                            break;
                        default://right
                            power = grid[x,y].getPower();
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,power)));
                            break;
                    }
                    break;

                #endregion

                #region Inverter

                case ComponentTypes.Inverter:
                    switch (dir)
                    {
                        case 1://down
                            val = grid[x, y].getPower()==0?ComponentTypes.powerOutput(type):0;
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,val)));
                            break;
                        case 2://left
                            val = grid[x + 1, y].getPower()==0?ComponentTypes.powerOutput(type):0;
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,val)));
                            break;
                        case 3://up
                            val = grid[x, y + 1].getPower()==0?ComponentTypes.powerOutput(type):0;
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,val)));
                            break;
                        default://right
                            val = grid[x, y].getPower()==0?ComponentTypes.powerOutput(type):0;
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,val)));
                            break;
                    }
                    break;

                #endregion

                #region Rising Edge Detector

                case ComponentTypes.RisingEdge:
                    while (thisVal.Count() < 2)
                        thisVal.Add(0);
                    switch (dir)
                    {
                        case 1://down
                            if (grid[x, y].getPower() > thisVal[0])
                                thisVal[1] = ComponentTypes.powerOutput(ComponentTypes.RisingEdge) - thisVal[1];
                            thisVal[0] = grid[x, y].getPower();
                            outputs.Add(new PointFP(x, y + 1,fingerprint(x,y+1,thisVal[1])));
                            break;
                        case 2://left
                            if (grid[x + 1, y].getPower() > thisVal[0])
                                thisVal[1] = ComponentTypes.powerOutput(ComponentTypes.RisingEdge) - thisVal[1];
                            thisVal[0] = grid[x + 1, y].getPower();
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,thisVal[1])));
                            break;
                        case 3://up
                            if (grid[x, y + 1].getPower() > thisVal[0])
                                thisVal[1] = ComponentTypes.powerOutput(ComponentTypes.RisingEdge) - thisVal[1];
                            thisVal[0] = grid[x, y + 1].getPower();
                            outputs.Add(new PointFP(x, y,fingerprint(x,y,thisVal[1])));
                            break;
                        default://right
                            if (grid[x, y].getPower() > thisVal[0])
                                thisVal[1] = ComponentTypes.powerOutput(ComponentTypes.RisingEdge) - thisVal[1];
                            thisVal[0] = grid[x, y].getPower();
                            outputs.Add(new PointFP(x + 1, y,fingerprint(x+1,y,thisVal[1])));
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

                #region Seven Segment Displays

                case ComponentTypes.SevSeg:
                    tempVal = 0;
                    for (int yy = 0; yy < size.Y; yy++)
                        sevseginputs[yy] = grid[x, y + yy].getPower();
                    break;

                #endregion

                #region Chip

                case ComponentTypes.Chip:
                    int state = 0;
                    double maxpower = 1;
                    for (int yy = 0; yy < Math.Sqrt(truthtable.Count()); yy++)
                    {
                        if (grid[x, y + yy].getPower() > 0)
                            state += (int)(Math.Pow(2, yy));
                        if (grid[x, y + yy].getPower() > maxpower)
                            maxpower = grid[x, y + yy].getPower();
                    }
                    for (int yy = 0; yy < truthtable[state].Count(); yy++)
                    {
                        double power = truthtable[state][yy] == 1 ? maxpower : 0;
                        outputs.Add(new PointFP(x + 1, y + yy,fingerprint(x+1,y+yy,power)));
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
                    case ComponentTypes.Diode:
                    case ComponentTypes.Inverter:
                    case ComponentTypes.RisingEdge:
                        switch (dir)
                        {
                            case 0:
                            case 2:
                                g.DrawImage(image, new RectangleF(scrx, scry, (zoomlevel - 1) * 2 + 1, zoomlevel), new RectangleF(0, 20 * dir, 80, 40), GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, (zoomlevel - 1) * 2 + 1), new RectangleF(dir == 1 ? 80 : 120, 0, 40, 80), GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                    case ComponentTypes.Power1:
                    case ComponentTypes.Power2:
                    case ComponentTypes.Power3:
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel));
                        break;
                    case ComponentTypes.And:
                    case ComponentTypes.Or:
                    case ComponentTypes.XOr:
                    case ComponentTypes.DLatch:
                        g.DrawImage(image, new RectangleF(scrx, scry, (zoomlevel - 1) * 2 + 1, (zoomlevel - 1) * 2 + 1), new RectangleF(80 * (dir % 2), (dir < 2 ? 0 : 80), 80, 80), GraphicsUnit.Pixel);
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
                            g.FillRectangle(b, scrx, scry, (zoomlevel - 1) * 2 + 1, (zoomlevel - 1) * 2 + 1);
                        g.DrawImage(image, new RectangleF(scrx, scry, (zoomlevel - 1) * 2 + 1, (zoomlevel - 1) * 2 + 1), new RectangleF(0, 0, 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.Toggle:
                        if (thisVal.Count() == 0)
                            thisVal.Add(0);
                        g.DrawImage(image, new RectangleF(scrx, scry, zoomlevel, zoomlevel), new RectangleF(thisVal[0] == 0 ? 0 : 40, 0, 40, 40), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.BigToggle:
                        if (thisVal.Count() == 0)
                            thisVal.Add(0);
                        g.DrawImage(image, new RectangleF(scrx, scry, (zoomlevel - 1) * 2 + 1, (zoomlevel - 1) * 2 + 1), new RectangleF(thisVal[0] == 0 ? 0 : 80, 0, 80, 80), GraphicsUnit.Pixel);
                        break;
                    case ComponentTypes.SevSeg:
                        b.Color = Color.FromArgb(0, 0, 0);
                        using (Bitmap i2 = new Bitmap(160, 280))
                        {
                            Graphics g2 = Graphics.FromImage(i2);
                            g2.FillRectangle(b, 0, 0, 160, 280);
                            List<Rectangle> toDraw = new List<Rectangle>();
                            toDraw.Add(new Rectangle(28, 13, 104, 19));
                            toDraw.Add(new Rectangle(128, 28, 19, 106));
                            toDraw.Add(new Rectangle(128, 145, 19, 105));
                            toDraw.Add(new Rectangle(28, 246, 104, 19));
                            toDraw.Add(new Rectangle(13, 145, 19, 105));
                            toDraw.Add(new Rectangle(13, 28, 19, 106));
                            toDraw.Add(new Rectangle(28, 130, 104, 19));
                            for (int xx = 0; xx < 7; xx++)
                            {
                                b.Color = Color.FromArgb((int)(Math.Min(Math.Max(Tile.baseColor + sevseginputs[xx] * (255 - Tile.baseColor), 0), 255)),
                                                         (int)(Math.Min(Math.Max((sevseginputs[xx] - 1) * 255, 0), 255)),
                                                         (int)(Math.Min(Math.Max((sevseginputs[xx] - 2) * 255, 0), 255)));
                                if (sevseginputs[xx] > 0)
                                    g2.FillRectangle(b, toDraw[xx]);
                            }
                            g2.DrawImage(image, 0, 0);
                            g.DrawImage(i2, new RectangleF(scrx, scry, (zoomlevel - 1) * 4 + 1, (zoomlevel - 1) * 7 + 1));
                        }
                        break;
                    case ComponentTypes.Chip:
                        if (thisVal.Count() == 0)
                            thisVal.Add(0);
                        if (thisVal[0] == 1)
                        {
                            b.Color = Color.FromArgb(255, 255, 0);
                            g.FillRectangle(b, scrx + 1, scry + 1, (zoomlevel - 2) * size.X - 1 + size.X, (zoomlevel - 2) * size.Y - 1 + size.Y);
                        }
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
                    default:
                        throw new Exception();
                }
                image.Dispose();
            }
            catch (Exception e)
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
                case ComponentTypes.RisingEdge:
                case ComponentTypes.And:
                case ComponentTypes.Or:
                case ComponentTypes.XOr:
                    dir = (dir + 1) % 4;
                    makeSize();
                    break;
                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                    if (thisVal.Count() == 0)
                        thisVal.Add(0);
                    thisVal[0] = ComponentTypes.powerOutput(type) - thisVal[0];
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
