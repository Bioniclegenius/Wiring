using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring {
    public class Component {
        private int dir;
        private double thisVal;
        public int type;
        public int x, y;
        public Point size;
        public bool Touched;

        public Component(int width,int height,int xi = 0,int yi = 0,int t = 0,int orientation = 0) {
            x = xi;
            y = yi;
            type = t;
            thisVal = 0;
            dir = orientation % 4;//0 = right, 1 = down, 2 = left, 3 = up - in 0, inputs on left, outputs on right
            makeSize();
            if(x + size.X >= width)
                x = width - size.X - 1;
            if(y + size.Y >= height)
                y = height - size.Y - 1;
            if(x < 0)
                x = 0;
            if(y < 0)
                y = 0;
        }

        private void makeSize() {
            switch(type) {
                case 0://Diode
                case 6://Inverter
                    if(dir == 0 || dir == 2)
                        size = new Point(2,1);
                    else
                        size = new Point(1,2);
                    break;
                case 4://Or gate
                case 5://And gate
                case 8://2x2 lamp
                    size = new Point(2,2);
                    break;
                default://Anything else
                    size = new Point(1,1);
                    break;
            }
        }

        public void clip(Tile[,] grid,int width,int height) {
            for(int xx = 0;xx < size.X;xx++)
                for(int yy = 0;yy < size.Y;yy++) {
                    if(xx > 0)
                        grid[xx + x,yy + y].subDir(0);
                    if(yy > 0)
                        grid[xx + x,yy + y].subDir(1);
                    if(xx < size.X - 1)
                        grid[xx + x,yy + y].subDir(2);
                    if(yy < size.Y - 1)
                        grid[xx + x,yy + y].subDir(3);
                }
        }

        public List<Point> eval(Tile[,] grid,int width,int height) {
            List<Point> outputs = new List<Point>();
            Touched = true;
            double val;
            thisVal = 0;
            switch(type) {

                #region Power sources

                case 1://power source, power level 1
                case 2://power source, power level 2
                case 3://power source, power level 3
                    grid[x,y].power(type);
                    outputs.Add(new Point(x,y));
                    break;

                #endregion

                #region Or gate

                case 4:
                    switch(dir) {
                        case 1://down
                            grid[x + 1,y + 1].power(Math.Max(grid[x,y].getPower(),grid[x + 1,y].getPower()));
                            outputs.Add(new Point(x + 1,y + 1));
                            break;
                        case 2://left
                            grid[x,y + 1].power(Math.Max(grid[x + 1,y].getPower(),grid[x + 1,y + 1].getPower()));
                            outputs.Add(new Point(x,y + 1));
                            break;
                        case 3://up
                            grid[x,y].power(Math.Max(grid[x,y + 1].getPower(),grid[x + 1,y + 1].getPower()));
                            outputs.Add(new Point(x,y));
                            break;
                        default://right
                            grid[x + 1,y].power(Math.Max(grid[x,y].getPower(),grid[x,y + 1].getPower()));
                            outputs.Add(new Point(x + 1,y));
                            break;
                    }
                    break;

                #endregion

                #region And gate

                case 5:
                    switch(dir) {
                        case 1://down
                            grid[x + 1,y + 1].power(Math.Min(grid[x,y].getPower(),grid[x + 1,y].getPower()));
                            outputs.Add(new Point(x + 1,y + 1));
                            break;
                        case 2://left
                            grid[x,y + 1].power(Math.Min(grid[x + 1,y].getPower(),grid[x + 1,y + 1].getPower()));
                            outputs.Add(new Point(x,y + 1));
                            break;
                        case 3://up
                            grid[x,y].power(Math.Min(grid[x,y + 1].getPower(),grid[x + 1,y + 1].getPower()));
                            outputs.Add(new Point(x,y));
                            break;
                        default://right
                            grid[x + 1,y].power(Math.Min(grid[x,y].getPower(),grid[x,y + 1].getPower()));
                            outputs.Add(new Point(x + 1,y));
                            break;
                    }
                    break;

                #endregion

                #region Inverter

                case 6:
                    switch(dir) {
                        case 1://down
                            val = grid[x,y].getPower();
                            if(val == 0)
                                grid[x,y + 1].power(1);
                            outputs.Add(new Point(x,y + 1));
                            break;
                        case 2://left
                            val = grid[x + 1,y].getPower();
                            if(val == 0)
                                grid[x,y].power(1);
                            outputs.Add(new Point(x,y));
                            break;
                        case 3://up
                            val = grid[x,y + 1].getPower();
                            if(val == 0)
                                grid[x,y].power(1);
                            outputs.Add(new Point(x,y));
                            break;
                        default://right
                            val = grid[x,y].getPower();
                            if(val == 0)
                                grid[x + 1,y].power(1);
                            outputs.Add(new Point(x + 1,y));
                            break;
                    }
                    break;

                #endregion

                #region 1x1 Lamps

                case 7:
                    thisVal = grid[x,y].getPower();
                    break;

                #endregion

                #region 2x2 Lamps

                case 8:
                    thisVal = Math.Max(Math.Max(grid[x,y].getPower(),grid[x + 1,y].getPower()),
                                       Math.Max(grid[x,y + 1].getPower(),grid[x + 1,y + 1].getPower()));
                    break;

                #endregion

                #region Diode/default

                default:
                    switch(dir) {
                        case 1://down
                            grid[x,y + 1].power(grid[x,y].getPower());
                            outputs.Add(new Point(x,y + 1));
                            break;
                        case 2://left
                            grid[x,y].power(grid[x + 1,y].getPower());
                            outputs.Add(new Point(x,y));
                            break;
                        case 3://up
                            grid[x,y].power(grid[x,y + 1].getPower());
                            outputs.Add(new Point(x,y));
                            break;
                        default://right
                            grid[x + 1,y].power(grid[x,y].getPower());
                            outputs.Add(new Point(x + 1,y));
                            break;
                    }
                    break;

                    #endregion

            }
            return outputs;
        }

        #region Component class

        public bool isInput() {
            switch(type) {
                case 1:
                case 2:
                case 3:
                    return true;
                default:
                    return false;
            }
        }

        public bool isOutput() {
            switch(type) {
                case 7:
                case 8:
                    return true;
                default:
                    return false;
            }
        }

        public bool isClickable() {
            switch(type) {
                case 0:
                case 4:
                case 5:
                case 6:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        public void render(Graphics g,Size sz,int scrx,int scry,int zoomlevel) {
            Bitmap image;
            SolidBrush b = new SolidBrush(Color.FromArgb(0,0,0));
            try {
                switch(type) {
                    case 0://diode
                        image = (Bitmap)Image.FromFile("images\\Diode.png");
                        switch(dir) {
                            case 0:
                            case 2:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel),new RectangleF(0,20 * dir,80,40),GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel,zoomlevel * 2),new RectangleF(dir == 1 ? 80 : 120,0,40,80),GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                    case 1://power supply, power level 1
                    case 2://power supply, power level 2
                    case 3://power supply, power level 3
                        image = (Bitmap)Image.FromFile(string.Format("images\\Power{0}.png",type));
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel,zoomlevel));
                        break;
                    case 4://or gate
                        image = (Bitmap)Image.FromFile("images\\Or.png");
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel * 2),new RectangleF(80 * (dir % 2),(dir < 2 ? 0 : 80),80,80),GraphicsUnit.Pixel);
                        break;
                    case 5://and gate
                        image = (Bitmap)Image.FromFile("images\\And.png");
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel * 2),new RectangleF(80 * (dir % 2),(dir < 2 ? 0 : 80),80,80),GraphicsUnit.Pixel);
                        break;
                    case 6://inverter
                        image = (Bitmap)Image.FromFile("images\\Inverter.png");
                        switch(dir) {
                            case 0:
                            case 2:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel),new RectangleF(0,20 * dir,80,40),GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel,zoomlevel * 2),new RectangleF(dir == 1 ? 80 : 120,0,40,80),GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                    case 7://1x1 lamps
                        b.Color = Color.FromArgb((int)(Math.Min(Math.Max(Tile.baseColor + thisVal * (255 - Tile.baseColor),0),255)),
                                                 (int)(Math.Min(Math.Max((thisVal - 1) * 255,0),255)),
                                                 (int)(Math.Min(Math.Max((thisVal - 2) * 255,0),255)));
                        if(thisVal != 0)
                            g.FillRectangle(b,scrx,scry,zoomlevel,zoomlevel);
                        image = (Bitmap)Image.FromFile("images\\1x1Lamp.png");
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel,zoomlevel),new RectangleF(0,0,40,40),GraphicsUnit.Pixel);
                        break;
                    case 8://2x2 lamps
                        b.Color = Color.FromArgb((int)(Math.Min(Math.Max(Tile.baseColor + thisVal * (255 - Tile.baseColor),0),255)),
                                                 (int)(Math.Min(Math.Max((thisVal - 1) * 255,0),255)),
                                                 (int)(Math.Min(Math.Max((thisVal - 2) * 255,0),255)));
                        if(thisVal != 0)
                            g.FillRectangle(b,scrx,scry,zoomlevel*2,zoomlevel*2);
                        image = (Bitmap)Image.FromFile("images\\2x2Lamp.png");
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel*2,zoomlevel*2),new RectangleF(0,0,80,80),GraphicsUnit.Pixel);
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch(Exception) {
                b.Color=Color.FromArgb(0,255,255);
                g.FillRectangle(b,scrx + 1,scry + 1,zoomlevel * size.X - 2,zoomlevel * size.Y - 2);
            }
            /*Font f = new Font("Arial",12);
            b.Color = Color.FromArgb(255,255,255);
            g.DrawString(string.Format("{0}",thisVal),f,b,new PointF(scrx,scry));*/
        }

        public void click() {
            switch(type) {
                case 0:
                case 4:
                case 5:
                case 6:
                    dir = (dir + 1) % 4;
                    makeSize();
                    break;
                default:
                    break;
            }
        }

    }
}
