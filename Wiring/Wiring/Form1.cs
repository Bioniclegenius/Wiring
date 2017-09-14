using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wiring
{
    public partial class Form1 : Form
    {
        private RenderPanel rp;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(800, 600);
            rp = new RenderPanel(ClientSize);
            Controls.Add(rp);
            BeginInvoke((MethodInvoker)delegate {
                rp.init();
            });
        }
    }
}
