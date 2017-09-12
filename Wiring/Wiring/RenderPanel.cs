using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Wiring
{
    public class RenderPanel : Panel
    {
        public System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        public long lasttime = 0;
        WireGrid blueprint;
        public RenderPanel(Size sz)
        {
            Size = sz;
            Location = new Point(0, 0);
            DoubleBuffered = true;
            Paint += new PaintEventHandler(paintEvent);
            blueprint = new WireGrid();
            st.Start();
            Invalidate();
        }
        public void paintEvent(object sender, PaintEventArgs e)
        {
            long time = st.ElapsedMilliseconds - lasttime;
            lasttime += time;
            Graphics g = e.Graphics;
            SolidBrush b = new SolidBrush(Color.FromArgb(0, 0, 0));
            g.FillRectangle(b, 0, 0, Size.Width, Size.Height);

            blueprint.render(g,Size);

            Invalidate();
        }
    }
}
