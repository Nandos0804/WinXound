using System;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormInfo : Form
    {
        public FormInfo()
        {
            InitializeComponent();
        }

        private void FormInfo_Load(object sender, EventArgs e)
        {
            LabelVer.Text = "WinXound.Net (ver. " + wxGlobal.WINXOUND_VERSION + ")";
        }

        private void LabelInfo_TextChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void FormInfo_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                ControlPaint.DrawBorder3D(e.Graphics, 0, 0, this.Width, this.Height, Border3DStyle.Flat);
            }
            catch { }
        }
    }
}
