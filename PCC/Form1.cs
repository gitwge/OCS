using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = true;
            
        }
        DBtest dd = new DBtest();
        private void timer1_Tick(object sender, EventArgs e)
        {
            dd.testdata2();
        }
    }
}
