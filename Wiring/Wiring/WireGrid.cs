using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wiring {
    public class WireGrid {
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
        private Tile[,] tempgrid;
        private List<Component> components;
        private Point mouse;
        private Point clickstart;
        private int clickstate;
        private int width;
        private int height;
        public int zoomlevel;
        public int xcenter;
        public int ycenter;

        public WireGrid() {
            width = 101;
            height = 101;
            xcenter = width / 2;
            ycenter = height / 2;
            zoomlevel = 40;
            grid = new Tile[width,height];
            tempgrid = new Tile[width,height];
            components = new List<Component>();
            mouse = new Point(0,0);
            clickstate = 0;
            for(int x = 0;x < width;x++)
                for(int y = 0;y < height;y++) {
                    grid[x,y] = new Tile();
                }

            components.Add(new Component(new Point(width,height),48,48,4,0));
            components.Add(new Component(new Point(width,height),45,47,1,0));
            components.Add(new Component(new Point(width,height),45,48,2,0));
            components.Add(new Component(new Point(width,height),45,49,3,0));
        }

        public void render(Graphics g,Size sz) {
            SolidBrush b = new SolidBrush(Color.FromArgb(63,63,63));
            Point offset = center(sz);
            int leftedge = Math.Max((-zoomlevel - offset.X) / (zoomlevel - 1),0);
            int rightedge = Math.Min((sz.Width + zoomlevel - offset.X) / (zoomlevel - 1),width - 1);
            int topedge = Math.Max((-zoomlevel - offset.Y) / (zoomlevel - 1),0);
            int bottomedge = Math.Min((sz.Height + zoomlevel - offset.Y) / (zoomlevel - 1),height - 1);
            if(clickstate == 0) {
                for(int x = leftedge;x <= rightedge;x++)
                    for(int y = topedge;y <= bottomedge;y++)
                        grid[x,y].render(g,x * (zoomlevel - 1) + offset.X,y * (zoomlevel - 1) + offset.Y,zoomlevel);
            }
            else {
                for(int x = 0;x < width;x++)
                    for(int y = 0;y < height;y++)
                        tempgrid[x,y] = new Tile(grid[x,y]);
                if(mouse.X > clickstart.X) {
                    tempgrid[clickstart.X,clickstart.Y].changeDir(2,clickstate == 1);
                    for(int x = clickstart.X + 1;x <= mouse.X;x++) {
                        tempgrid[x,clickstart.Y].changeDir(0,clickstate==1);
                        if(x < mouse.X)
                            tempgrid[x,clickstart.Y].changeDir(2,clickstate == 1);
                    }
                }
                else if(mouse.X < clickstart.X) {
                    tempgrid[clickstart.X,clickstart.Y].changeDir(0,clickstate == 1);
                    for(int x = clickstart.X - 1;x >= mouse.X;x--) {
                        tempgrid[x,clickstart.Y].changeDir(2,clickstate == 1);
                        if(x > mouse.X)
                            tempgrid[x,clickstart.Y].changeDir(0,clickstate == 1);
                    }
                }
                if(mouse.Y > clickstart.Y) {
                    tempgrid[mouse.X,clickstart.Y].changeDir(3,clickstate == 1);
                    for(int y = clickstart.Y + 1;y <= mouse.Y;y++) {
                        tempgrid[mouse.X,y].changeDir(1,clickstate == 1);
                        if(y < mouse.Y)
                            tempgrid[mouse.X,y].changeDir(3,clickstate == 1);
                    }
                }
                else if(mouse.Y < clickstart.Y) {
                    tempgrid[mouse.X,clickstart.Y].changeDir(1,clickstate == 1);
                    for(int y = clickstart.Y - 1;y >= mouse.Y;y--) {
                        tempgrid[mouse.X,y].changeDir(3,clickstate == 1);
                        if(y > mouse.Y)
                            tempgrid[mouse.X,y].changeDir(1,clickstate == 1);
                    }
                }
                for(int x = leftedge;x <= rightedge;x++)
                    for(int y = topedge;y <= bottomedge;y++)
                        tempgrid[x,y].render(g,x * (zoomlevel - 1) + offset.X,y * (zoomlevel - 1) + offset.Y,zoomlevel);
            }
            for(int x = 0;x < components.Count();x++) {
                components[x].render(g,sz,components[x].x*(zoomlevel-1)+offset.X,components[x].y*(zoomlevel-1)+offset.Y,zoomlevel);
            }
            for(int x = 0;x < width;x++)
                for(int y = 0;y < height;y++)
                    grid[x,y].reset();
            evaluatePower();
            //grid[mouse.X,mouse.Y].renderNum(g,grid[mouse.X,mouse.Y].getPower(),mouse.X * (zoomlevel - 1) + offset.X,mouse.Y * (zoomlevel - 1) + offset.Y,zoomlevel);
            /*Font f = new Font("Arial",12);
            b.Color = Color.FromArgb(255,255,255);
            g.DrawString(string.Format("{0}",mouse.X),f,b,new PointF(5,5));*/

        }

        #region Mouse events

        public void MouseMove(Point p,Size sz) {
            Point offset = center(sz);
            p.X -= offset.X;
            p.Y -= offset.Y;
            p.X /= zoomlevel - 1;
            p.Y /= zoomlevel - 1;
            p.X = Math.Max(Math.Min(p.X,width - 1),0);
            p.Y = Math.Max(Math.Min(p.Y,height - 1),0);
            mouse = new Point(p.X,p.Y);
        }
        public void MouseClick(Point p,Size sz,bool clickType = true) {
            Point offset = center(sz);
            p.X -= offset.X;
            p.Y -= offset.Y;
            p.X /= zoomlevel - 1;
            p.Y /= zoomlevel - 1;
            p.X = Math.Max(Math.Min(p.X,width - 1),0);
            p.Y = Math.Max(Math.Min(p.Y,height - 1),0);
            mouse = new Point(p.X,p.Y);
            if(clickstate == 0 && grid[p.X,p.Y].type < 15) {
                if(clickType)
                    clickstate = 1;
                else
                    clickstate = 2;
                clickstart = new Point(p.X,p.Y);
            }
            else if(clickstate == 0) {
                grid[p.X,p.Y].type = 1 - (grid[p.X,p.Y].type - 15) + 15;
            }
            else {
                clickstate = 0;
                for(int x = 0;x < width;x++)
                    for(int y = 0;y < height;y++)
                        grid[x,y] = new Tile(tempgrid[x,y]);
            }
        }
        public Point center(Size sz) {
            int xoffset = -xcenter * (zoomlevel - 1) - zoomlevel / 2 + sz.Width / 2;
            int yoffset = -ycenter * (zoomlevel - 1) - zoomlevel / 2 + sz.Height / 2;
            return new Point(xoffset,yoffset);
        }

        #endregion

        #region Power evaluation

        public void evaluatePower() {
            for(int x = 0;x < components.Count();x++) {
                if(components[x].isInput()) {
                    var outputs=components[x].eval(grid,width,height);
                    for(int y = 0;y < outputs.Count();y++)
                        eval(outputs[y].X,outputs[y].Y);
                }
            }
            for(int x = 0;x < components.Count();x++) {
                if(!components[x].isInput()&&!components[x].isOutput()) {
                    var outputs = components[x].eval(grid,width,height);
                    for(int y = 0;y < outputs.Count();y++)
                        eval(outputs[y].X,outputs[y].Y);
                }
            }
        }
        public void eval(int x,int y) {
            if(x > 0)
                if(grid[x,y].canDir(0) && grid[x - 1,y].getPower(2) < grid[x,y].getPower(0)) {
                    grid[x - 1,y].power(0,0,grid[x,y].getPower(0),0);
                    eval(x - 1,y);
                }
            if(y > 0)
                if(grid[x,y].canDir(1) && grid[x,y - 1].getPower(3) < grid[x,y].getPower(1)) {
                    grid[x,y - 1].power(0,0,0,grid[x,y].getPower(1));
                    eval(x,y - 1);
                }
            if(x < width - 2)
                if(grid[x,y].canDir(2) && grid[x + 1,y].getPower(0) < grid[x,y].getPower(2)) {
                    grid[x + 1,y].power(grid[x,y].getPower(2),0,0,0);
                    eval(x + 1,y);
                }
            if(y < height - 2)
                if(grid[x,y].canDir(3) && grid[x,y+1].getPower(1) < grid[x,y].getPower(3)) {
                    grid[x,y+1].power(0,grid[x,y].getPower(3),0,0);
                    eval(x,y+1);
                }
        }

        #endregion

    }
}
