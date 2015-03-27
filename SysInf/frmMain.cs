using System;
using System.Windows.Forms;
using SystemInfo;

namespace SysInf
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            string output = ServerSystem.GetMachinePerformance();
            lblSysInf.Text = output;
            //MessageBox.Show(output);
        }
    }
}
