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

        public Component(int xi = 0,int yi = 0,int t = 0,int orientation = 0) {
            x = xi;
            y = yi;
            type = t;
            dir = orientation % 4;//0 = right, 1 = down, 2 = left, 3 = up - in 0, inputs on left, outputs on right
        }

        public List<Point> eval(Tile[,] grid,int width,int height) {
            List<Point> outputs = new List<Point>();
            switch(type) {
                case 1:
                    grid[x,y].power(1);
                    outputs.Add(new Point(x,y));
                    break;
                default://0, diode
                    switch(dir) {
                        case 1://down
                            if(y < height - 2) {
                                grid[x,y + 1].power(grid[x,y].signal);
                                outputs.Add(new Point(x,y + 1));
                            }
                            break;
                        case 2://left
                            if(x < width - 2) {
                                grid[x,y].power(grid[x + 1,y].signal);
                                outputs.Add(new Point(x,y));
                            }
                            break;
                        case 3://up
                            if(y < height - 2) {
                                grid[x,y].power(grid[x,y + 1].signal);
                                outputs.Add(new Point(x,y));
                            }
                            break;
                        default://right
                            if(x < width - 2) {
                                grid[x + 1,y].power(grid[x,y].signal);
                                outputs.Add(new Point(x + 1,y));
                            }
                            break;
                    }
                    break;
            }
            return outputs;
        }

        #region Component class

        public bool isInput() {
            switch(type) {
                case 1:
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
                    case 1:
                        throw new Exception();
                        break;
                    default://diode
                        image = (Bitmap)Image.FromFile("images\\diode.png");
                        switch(dir) {
                            case 0:
                            case 2:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel * 2,zoomlevel),new RectangleF(0,20 * dir,80,40),GraphicsUnit.Pixel);
                                break;
                            case 1:
                            case 3:
                                g.DrawImage(image,new RectangleF(scrx,scry,zoomlevel,zoomlevel * 2),new RectangleF(dir==1?80:120,0,80,40),GraphicsUnit.Pixel);
                                break;
                        }
                        break;
                }
            }
            catch(Exception) {
                SolidBrush b = new SolidBrush(Color.FromArgb(0,255,255));
                switch(type) {
                    case 1:
                        g.FillRectangle(b,scrx+1,scry+1,zoomlevel-2,zoomlevel-2);
                        break;
                    default:
                        g.FillRectangle(b,scrx+1,scry+1,zoomlevel * (dir == 0 || dir == 2 ? 2 : 1)-2,zoomlevel * (dir == 0 || dir == 2 ? 1 : 2)-2);
                        break;
                }
            }
        }
    }
}
