using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Wiring {
    public class RenderPanel:Panel {
        public System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        public long lasttime = 0;
        private WireGrid blueprint;
        
        private VScrollBar vScrollBar1;
        private HScrollBar hScrollBar1;
        private Button zoomIn;
        private Button zoomOut;

        public RenderPanel(Size sz) {

            //Initial setup
            Size = sz;
            Location = new Point(0,0);
            DoubleBuffered = true;
            blueprint = new WireGrid();

            //Events
            Paint += new PaintEventHandler(paintEvent);
            MouseMove += new MouseEventHandler(mouseMove);
            MouseClick += new MouseEventHandler(click);
            MouseWheel += new MouseEventHandler(vScroll2);

            //Windows form elements
            vScrollBar1 = new VScrollBar();
            vScrollBar1.Height = sz.Height;
            vScrollBar1.Location = new Point(sz.Width - vScrollBar1.Width,0);
            vScrollBar1.Value = 5;
            vScrollBar1.Maximum = 20;
            vScrollBar1.ValueChanged += new EventHandler(vScroll);
            vScrollBar1.Maximum = blueprint.height;
            vScrollBar1.Value = blueprint.ycenter;

            hScrollBar1 = new HScrollBar();
            hScrollBar1.Width = sz.Width-blueprint.workbenchWidth-1;
            hScrollBar1.Location = new Point(blueprint.workbenchWidth+1,sz.Height-hScrollBar1.Height);
            hScrollBar1.ValueChanged += new EventHandler(hScroll);
            hScrollBar1.Maximum = blueprint.width;
            hScrollBar1.Value = blueprint.xcenter;

            vScrollBar1.Height -= hScrollBar1.Height;
            hScrollBar1.Width -= vScrollBar1.Width;

            zoomIn = new Button();
            zoomIn.Text = "+";
            zoomIn.Size = new Size(40,40);
            zoomIn.Location = new Point(50,5);
            zoomIn.Click += new EventHandler(zoomin);

            zoomOut = new Button();
            zoomOut.Text = "-";
            zoomOut.Size = new Size(40,40);
            zoomOut.Location = new Point(5,5);
            zoomOut.Click += new EventHandler(zoomout);

            //Add controls to panel
            Controls.Add(vScrollBar1);
            Controls.Add(hScrollBar1);
            Controls.Add(zoomIn);
            Controls.Add(zoomOut);

            //Start rendering
            st.Start();
            Invalidate();
        }

        public void init() {
            Refresh();
        }

        public void paintEvent(object sender,PaintEventArgs e) {
            long time = st.ElapsedMilliseconds - lasttime;
            lasttime += time;
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            SolidBrush b = new SolidBrush(Color.FromArgb(0,0,0));
            g.FillRectangle(b,0,0,Size.Width,Size.Height);

            blueprint.render(g,Size,time);

            Invalidate();
        }

        private void mouseMove(object sender,MouseEventArgs e) {
            blueprint.MouseMove(new Point(e.X,e.Y),Size);
        }

        private void click(object sender,MouseEventArgs e) {
            blueprint.MouseClick(new Point(e.X,e.Y),Size,e.Button==MouseButtons.Left?0:e.Button == MouseButtons.Right ? 1 : 2);
        }

        private void vScroll(object sender,EventArgs e) {
            blueprint.ycenter=vScrollBar1.Value;
        }

        private void vScroll2(object sender,MouseEventArgs e) {
            var valchange = -e.Delta / SystemInformation.MouseWheelScrollDelta;
            if(valchange < 0 && vScrollBar1.Value + valchange >= vScrollBar1.Minimum)
                vScrollBar1.Value += valchange;
            else if(valchange < 0)
                vScrollBar1.Value = vScrollBar1.Minimum;
            else if(valchange > 0 && vScrollBar1.Value + valchange <= vScrollBar1.Maximum)
                vScrollBar1.Value += valchange;
            else if(valchange > 0)
                vScrollBar1.Value = vScrollBar1.Maximum;
        }

        private void hScroll(object Sender,EventArgs e) {
            blueprint.xcenter=hScrollBar1.Value;
        }

        private void zoomin(object Sender,EventArgs e) {
            blueprint.zoomIn();
        }

        private void zoomout(object Sender,EventArgs e) {
            blueprint.zoomOut();
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if(m.HWnd != Handle)
                return;
            int val;
            switch(m.Msg) {
                case 276://WM_HSCROLL
                    if((int)(m.WParam) < 2) {
                        val = (int)(m.WParam);
                        if(val == 0 && hScrollBar1.Value > hScrollBar1.Minimum)
                            hScrollBar1.Value--;
                        else if(val ==1 && hScrollBar1.Value < hScrollBar1.Maximum)
                            hScrollBar1.Value++;
                    }
                    m.Result = (IntPtr)1;
                    break;
            }
        }


    }
}
