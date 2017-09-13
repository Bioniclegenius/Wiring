using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring {
    public class Component {
        private int type;
        private int dir;
        public int x, y;
        private Point size;

        public Component(Point p,int width,int height,int xi = 0,int yi = 0,int t = 0,int orientation = 0) {
            x = xi;
            y = yi;
            if(x < 0)
                x = 0;
            if(x > p.X - 1)
                x = p.X - 1;
            if(y < 0)
                y = 0;
            if(y > p.Y - 1)
                y = p.Y - 1;
            type = t;
            switch(type) {
                case 0://diode
                    size = new Point((dir == 0 || dir == 2 ? 2 : 1),(dir == 1 || dir == 3 ? 2 : 1));
                    break;
                case 4://or/and gate
                case 5:
                    size = new Point(2,2);
                    break;
                default:
                    size = new Point(1,1);
                    break;
            }
            if(x + size.X >= width)
                x = width - size.X - 1;
            if(y + size.Y >= height)
                y = height - size.Y - 1;
            dir = orientation % 4;//0 = right, 1 = down, 2 = left, 3 = up - in 0, inputs on left, outputs on right
        }

        public void clip(Tile[,] grid,int width,int height) {
            for(int xx = 0;xx < size.X;xx++)
                for(int yy = 0;yy < size.Y;yy++) {
                    if(yy > 0)
                        grid[xx + x,yy + y].subDir(1);
                    if(xx > 0)
                        grid[xx + x,yy + y].subDir(0);
                    if(xx < size.X - 1 && xx + x < width)
                        grid[xx + x,yy + y].subDir(2);
                    if(yy < size.Y - 1 && yy + y < height)
                        grid[xx + x,yy + y].subDir(3);
                }
        }

        public List<Point> eval(Tile[,] grid,int width,int height) {
            List<Point> outputs = new List<Point>();
            clip(grid,width,height);
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
                default:
                    return false;
            }
        }

        #endregion

        public void render(Graphics g,Size sz,int scrx,int scry,int zoomlevel) {
            Bitmap image;
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
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel * 2),new RectangleF(zoomlevel * 2 * (dir % 2),(dir < 2 ? 0 : 80),80,80),GraphicsUnit.Pixel);
                        break;
                    case 5://and gate
                        image = (Bitmap)Image.FromFile("images\\And.png");
                        g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel * 2),new RectangleF(zoomlevel * 2 * (dir % 2),(dir < 2 ? 0 : 80),80,80),GraphicsUnit.Pixel);
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch(Exception) {
                SolidBrush b = new SolidBrush(Color.FromArgb(0,255,255));
                g.FillRectangle(b,scrx + 1,scry + 1,zoomlevel * size.X - 2,zoomlevel * size.Y - 2);
            }
        }
    }
}
