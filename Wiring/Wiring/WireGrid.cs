using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private List<Component> workbenchComponents;
        private List<int> componentsToCheck;
        private Component tempComponent;
        private Point mouse;
        private Point clickstart;
        private int clickstate;
        private string filename;
        public int workbenchWidth;
        public int width;
        public int height;
        public int zoomlevel;
        public int xcenter;
        public int ycenter;

        public WireGrid() {
            width = 101;
            height = 101;
            xcenter = width / 2;
            ycenter = height / 2;
            zoomlevel = 40;
            filename = "";
            grid = new Tile[width,height];
            tempgrid = new Tile[width,height];
            components = new List<Component>();
            workbenchComponents = new List<Component>();
            componentsToCheck = new List<int>();
            tempComponent = new Component(0,0,0,0,-1,0);
            workbenchWidth = 190;
            mouse = new Point(0,0);
            clickstate = 0;
            for(int x = 0;x < width;x++)
                for(int y = 0;y < height;y++)
                    grid[x,y] = new Tile();

            //Setting up the workbench
            for(int x = 0;x <= ComponentTypes.numTypes;x++)
                if(x != ComponentTypes.Chip)
                    workbenchComponents.Add(new Component(0,0,0,0,x));
            var chips = Directory.GetFiles("Chips","*.chp");
            for(int x = 0;x < chips.Length;x++) {
                workbenchComponents.Add(new Component(0,0,0,0,ComponentTypes.Chip));
                workbenchComponents[workbenchComponents.Count() - 1].Load(chips[x]);
                if(workbenchComponents[workbenchComponents.Count() - 1].thisVal.Count() == 0)
                    workbenchComponents[workbenchComponents.Count() - 1].thisVal.Add(0);
                workbenchComponents[workbenchComponents.Count() - 1].thisVal[0] = 1;
            }

            int spacing = 5;
            int mintop = 60;
            int left = spacing;
            int top = spacing + mintop;
            int topbuffer = top;
            int maxwidth = workbenchWidth - 2 * spacing;
            for(int x = 0;x < workbenchComponents.Count();x++) {
                if(left > spacing && workbenchComponents[x].size.X * 20 + left > maxwidth) {
                    left = spacing;
                    top = topbuffer + spacing;
                }
                workbenchComponents[x].x = left;
                workbenchComponents[x].y = top;
                left += spacing + 20 * workbenchComponents[x].size.X;
                if(top + 20 * workbenchComponents[x].size.Y > topbuffer)
                    topbuffer = top + 20 * workbenchComponents[x].size.Y;
            }
        }

        #region Renders

        public void render(Graphics g,Size sz,long time) {
            SolidBrush b = new SolidBrush(Color.FromArgb(63,63,63));
            Point offset = center(sz);
            int leftedge = Math.Max((-zoomlevel - offset.X) / (zoomlevel - 1),0);
            int rightedge = Math.Min((sz.Width + zoomlevel - offset.X) / (zoomlevel - 1),width - 1);
            int topedge = Math.Max((-zoomlevel - offset.Y) / (zoomlevel - 1),0);
            int bottomedge = Math.Min((sz.Height + zoomlevel - offset.Y) / (zoomlevel - 1),height - 1);
            if(clickstate != 1 && clickstate != 2) {
                for(int x = leftedge;x <= rightedge;x++)
                    for(int y = topedge;y <= bottomedge;y++)
                        grid[x,y].render(g,x * (zoomlevel - 1) + offset.X,y * (zoomlevel - 1) + offset.Y,zoomlevel);
            }
            else if(clickstate < 3) {
                for(int x = 0;x < width;x++)
                    for(int y = 0;y < height;y++)
                        tempgrid[x,y] = new Tile(grid[x,y]);
                if(mouse.X > clickstart.X) {
                    tempgrid[clickstart.X,clickstart.Y].changeDir(2,clickstate == 1);
                    for(int x = clickstart.X + 1;x <= mouse.X;x++) {
                        tempgrid[x,clickstart.Y].changeDir(0,clickstate == 1);
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
                components[x].render(g,sz,components[x].x * (zoomlevel - 1) + offset.X,components[x].y * (zoomlevel - 1) + offset.Y,zoomlevel);
            }
            for(int x = 0;x < width;x++)
                for(int y = 0;y < height;y++)
                    grid[x,y].reset();
            tempComponent.x = mouse.X;
            tempComponent.y = mouse.Y;
            if(tempComponent.type >= 0 && clickstate == 3)
                tempComponent.render(g,sz,mouse.X * (zoomlevel - 1) + offset.X,mouse.Y * (zoomlevel - 1) + offset.Y,zoomlevel);
            evaluatePower(time);
            renderWorkbench(g,sz);
            /*Font f = new Font("Arial",12);
            b.Color = Color.FromArgb(255,255,255);
            for(int x = leftedge;x < rightedge;x++)
                for(int y = topedge;y < bottomedge;y++)
                    grid[x,y].renderNum(g,grid[x,y].canDir(y%4)?1:0,x * (zoomlevel - 1) + offset.X,y * (zoomlevel - 1) + offset.Y,zoomlevel);/**/

        }

        private void renderWorkbench(Graphics g,Size sz) {
            SolidBrush b = new SolidBrush(Color.FromArgb(63,63,63));
            g.FillRectangle(b,0,0,workbenchWidth + 1,sz.Height);
            b.Color = Color.FromArgb(0,0,0);
            g.FillRectangle(b,0,0,workbenchWidth,sz.Height);
            for(int x = 0;x < workbenchComponents.Count();x++)
                workbenchComponents[x].render(g,sz,workbenchComponents[x].x,workbenchComponents[x].y,20);
        }

        #endregion

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

        public void MouseClick(Point p,Size sz,int clickType) {
            Point offset = center(sz);
            Point orig = new Point(p.X,p.Y);
            p.X -= offset.X;
            p.Y -= offset.Y;
            p.X /= zoomlevel - 1;
            p.Y /= zoomlevel - 1;
            p.X = Math.Max(Math.Min(p.X,width - 1),0);
            p.Y = Math.Max(Math.Min(p.Y,height - 1),0);
            mouse = new Point(p.X,p.Y);
            if(orig.X > workbenchWidth) {
                int comp = -1;
                if(clickstate == 0) {
                    for(int x = 0;x < components.Count();x++)
                        if(p.X >= components[x].x && p.X < components[x].x + components[x].size.X &&
                            p.Y >= components[x].y && p.Y < components[x].y + components[x].size.Y) {
                            if(clickType == 0 && ComponentTypes.isClickable(components[x].type))
                                comp = x;
                            else if(clickType == 1) {
                                components.RemoveAt(x);
                                x--;
                                clickType = -1;
                            }
                        }

                }
                if(clickstate == 0 && comp >= 0) {
                    components[comp].click();
                }
                else if(clickstate == 0 && grid[p.X,p.Y].type < 15) {
                    if(clickType == 0)
                        clickstate = 1;
                    else if(clickType == 1)
                        clickstate = 2;
                    clickstart = new Point(p.X,p.Y);
                }
                else if(clickstate == 0) {
                    grid[p.X,p.Y].type = 1 - (grid[p.X,p.Y].type - 15) + 15;
                }
                else if(clickstate == 3) {
                    if(clickType == 0) {
                        components.Add(new Component(width,height,mouse.X,mouse.Y,tempComponent.type,0));
                        if(tempComponent.type == ComponentTypes.Chip)
                            components[components.Count() - 1].Load(tempComponent);
                    }
                    tempComponent.type = -1;
                    clickstate = 0;
                }
                else {
                    clickstate = 0;
                    for(int x = 0;x < width;x++)
                        for(int y = 0;y < height;y++)
                            grid[x,y] = new Tile(tempgrid[x,y]);
                }
            }
            else {
                if(clickstate == 0) {
                    for(int x = 0;x < workbenchComponents.Count();x++) {
                        Point loc = new Point(workbenchComponents[x].x,workbenchComponents[x].y);
                        Point size = workbenchComponents[x].size;
                        if(orig.X >= loc.X && orig.X < loc.X + size.X * 20 && orig.Y >= loc.Y && orig.Y < loc.Y + size.Y * 20) {
                            tempComponent = new Component(0,0,0,0,workbenchComponents[x].type,0);
                            if(workbenchComponents[x].type == ComponentTypes.Chip)
                                tempComponent.Load(workbenchComponents[x]);
                            clickstate = 3;
                            break;
                        }
                    }
                }
            }
        }

        public Point center(Size sz) {
            int xoffset = -xcenter * (zoomlevel - 1) - zoomlevel / 2 + (sz.Width - workbenchWidth) / 2 + workbenchWidth;
            int yoffset = -ycenter * (zoomlevel - 1) - zoomlevel / 2 + sz.Height / 2;
            return new Point(xoffset,yoffset);
        }

        #endregion

        #region Power evaluation

        public void evaluatePower(long time) {
            for(int x = 0;x < components.Count();x++) {
                components[x].clip(grid,width,height);
                if(ComponentTypes.isInput(components[x].type))
                    componentsToCheck.Add(x);
            }
            List<Point3FP> outputs = new List<Point3FP>();
            do {
                while(componentsToCheck.Count() > 0) {
                    var outputBuf = components[componentsToCheck[0]].eval(grid,width,height,time);
                    while(outputBuf.Count() > 0) {
                        outputs.Add(new Point3FP(outputBuf[0],componentsToCheck[0]));
                        outputBuf.RemoveAt(0);
                    }
                    componentsToCheck.RemoveAt(0);
                }
                while(outputs.Count() > 0) {
                    grid[outputs[0].X,outputs[0].Y].power(outputs[0].Fingerprint);
                    eval(outputs[0]);
                    outputs.RemoveAt(0);
                }
            } while(componentsToCheck.Count() > 0 || outputs.Count() > 0);
            for(int x = 0;x < components.Count();x++)
                if(ComponentTypes.isOutput(components[x].type))
                    components[x].eval(grid,width,height,time);
        }

        public void eval(Point3FP pf,int sourceAxis = -1) {
            for(int z = 0;z < components.Count();z++)
                if(pf.X >= components[z].x && pf.X < components[z].x + components[z].size.X &&
                   pf.Y >= components[z].y && pf.Y < components[z].y + components[z].size.Y &&
                   pf.Source != z && !ComponentTypes.isInput(components[z].type))
                    componentsToCheck.Add(z);
            List<string> blanks = new List<string>();
            if(grid[pf.X,pf.Y].type != 16 || sourceAxis != 1) {
                if(pf.X > 0)
                    if(grid[pf.X,pf.Y].canDir(0) && !grid[pf.X - 1,pf.Y].hasFingerprint(pf.Fingerprint,0)) {
                        grid[pf.X - 1,pf.Y].power("","",pf.Fingerprint,"");
                        eval(new Point3FP(pf.X - 1,pf.Y,pf.Fingerprint,pf.Source),0);
                    }
                if(pf.X < width - 2)
                    if(grid[pf.X,pf.Y].canDir(2) && !grid[pf.X + 1,pf.Y].hasFingerprint(pf.Fingerprint,0)) {
                        grid[pf.X + 1,pf.Y].power(pf.Fingerprint,"","","");
                        eval(new Point3FP(pf.X + 1,pf.Y,pf.Fingerprint,pf.Source),0);
                    }
            }
            if(grid[pf.X,pf.Y].type != 16 || sourceAxis != 0) {
                if(pf.Y > 0)
                    if(grid[pf.X,pf.Y].canDir(1) && !grid[pf.X,pf.Y - 1].hasFingerprint(pf.Fingerprint,1)) {
                        grid[pf.X,pf.Y - 1].power("","","",pf.Fingerprint);
                        eval(new Point3FP(pf.X,pf.Y - 1,pf.Fingerprint,pf.Source),1);
                    }
                if(pf.Y < height - 2)
                    if(grid[pf.X,pf.Y].canDir(3) && !grid[pf.X,pf.Y + 1].hasFingerprint(pf.Fingerprint,1)) {
                        grid[pf.X,pf.Y + 1].power("",pf.Fingerprint,"","");
                        eval(new Point3FP(pf.X,pf.Y + 1,pf.Fingerprint,pf.Source),1);
                    }
            }
        }

        #endregion

        #region Scrolls

        public void vScroll(int val) {
            ycenter += val;
            if(ycenter < 0)
                ycenter = 0;
            if(ycenter > height - 1)
                ycenter = height - 1;
        }

        public void hScroll(int val) {
            xcenter += val;
            if(xcenter < 0)
                xcenter = 0;
            if(xcenter > width - 1)
                xcenter = width - 1;
        }

        #endregion

        #region Zooms

        public void zoomIn() {
            if(zoomlevel < 80)
                zoomlevel += 10;
        }

        public void zoomOut() {
            if(zoomlevel > 10)
                zoomlevel -= 10;
        }

        #endregion

        #region Eval

        public void evaluate(RenderPanel parent) {
            List<Component> inputs = new List<Component>();
            int newInputsStart = 0;
            List<List<int>> newInputIndices = new List<List<int>>();
            int numInputs = 0;
            int numOutputs = 0;
            for(int x = 0;x < components.Count();x++) {
                if(ComponentTypes.isInput(components[x].type)) {
                    numInputs++;
                    inputs.Add(components[x]);
                    components.RemoveAt(x);
                    x--;
                }
                else if(ComponentTypes.isOutput(components[x].type))
                    numOutputs++;
            }
            newInputsStart = components.Count();
            for(int z = 0;z < inputs.Count();z++) {
                newInputIndices.Add(new List<int>());
                for(int x = inputs[z].x;x < inputs[z].x + inputs[z].size.X;x++)
                    for(int y = inputs[z].y;y < inputs[z].y + inputs[z].size.Y;y++) {
                        components.Add(new Component(width,height,x,y,ComponentTypes.Toggle,0));
                        newInputIndices[z].Add(components.Count() - 1);
                        components[components.Count() - 1].click();
                    }
            }
            int numstates = (int)(Math.Pow(2,newInputIndices.Count()));
            int[,] truthtable = new int[numstates,numOutputs];
            for(int state = 0;state < numstates;state++) {
                for(int x = 0;x < width;x++)
                    for(int y = 0;y < height;y++)
                        grid[x,y].reset();
                for(int inp = 0;inp < newInputIndices.Count();inp++) {
                    if(state % (Math.Pow(2,inp)) == 0) {
                        for(int x = 0;x < newInputIndices[inp].Count();x++)
                            components[newInputIndices[inp][x]].click();
                    }
                }
                evaluatePower(0);
                int thisOutput = 0;
                for(int x = 0;x < components.Count();x++) {
                    if(ComponentTypes.isOutput(components[x].type)) {
                        if(components[x].tempVal > 0)
                            truthtable[state,thisOutput] = 1;
                        thisOutput++;
                    }
                }
            }
            if(filename == "") {
                SaveFileDialog save = new SaveFileDialog();
                if(!Directory.Exists("Chips"))
                    Directory.CreateDirectory("Chips");
                save.Title = "Save Circuit As";
                save.DefaultExt = "chp";
                save.InitialDirectory = "Chips";
                save.Filter = "Chip files (*.chp)|*.chp";
                if(save.ShowDialog(parent) == DialogResult.OK) {
                    filename = save.FileName;
                }
            }
            if(filename != "" && numInputs > 0 && numOutputs > 0) {
                string output = "";
                for(int y = 0;y < numstates;y++) {
                    for(int x = 0;x < numOutputs;x++) {
                        output = string.Format("{0}{1}{2}",output,truthtable[y,x],x == numOutputs - 1 ? "" : " ");
                    }
                    if(y < numstates - 1)
                        output = string.Format("{0}\r\n",output);
                }
                File.WriteAllText(filename,output);
            }
            components.RemoveRange(newInputsStart,components.Count() - newInputsStart);
            components.AddRange(inputs);
        }

        #endregion

    }
}
