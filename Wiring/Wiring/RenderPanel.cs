using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Wiring {
    public class RenderPanel:Panel {
        public System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        public long lasttime = 0;
        private WireGrid blueprint;
        public RenderPanel(Size sz) {
            Size = sz;
            Location = new Point(0,0);
            DoubleBuffered = true;
            Paint += new PaintEventHandler(paintEvent);
            MouseMove += new MouseEventHandler(mouseMove);
            MouseClick += new MouseEventHandler(click);
            blueprint = new WireGrid();
            st.Start();
            Invalidate();
        }
        public void paintEvent(object sender,PaintEventArgs e) {
            long time = st.ElapsedMilliseconds - lasttime;
            lasttime += time;
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            SolidBrush b = new SolidBrush(Color.FromArgb(0,0,0));
            g.FillRectangle(b,0,0,Size.Width,Size.Height);

            blueprint.render(g,Size);

            Invalidate();
        }
        private void mouseMove(object sender,MouseEventArgs e) {
            blueprint.MouseMove(new Point(e.X,e.Y),Size);
        }
        private void click(object sender,MouseEventArgs e) {
            blueprint.MouseClick(new Point(e.X,e.Y),Size,e.Button==MouseButtons.Right?false:true);
        }
    }
}
