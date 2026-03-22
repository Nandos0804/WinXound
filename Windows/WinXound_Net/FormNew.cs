using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormNew : Form
    {

        public string mReturnValue = "csound";

        public FormNew()
        {
            InitializeComponent();
        }


        public delegate void OnReturnValue(object sender, string ReturnValue);
        public event OnReturnValue ReturnValueEv;

        private Pen pen = new Pen(Color.FromArgb(80, 80, 80));
        private Color SelectedColor = Color.FromArgb(203, 216, 237);

        private void FormNew_Load(object sender, EventArgs e)
        {

        }

        void panel_Paint(object sender, PaintEventArgs e)
        {
            Panel temp = (sender as Panel);
            if (!temp.BackColor.Equals(this.BackColor))
                e.Graphics.DrawRectangle(pen, 0, 0,
                    temp.Width - 1, temp.Height - 1);
        }



        private void FormNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.C)
            {
                pictureBoxCSound_Click(this, null);
            }
            else if (e.KeyCode == Keys.O)
            {
                pictureBoxCSoundOrc_Click(this, null);
            }
            else if (e.KeyCode == Keys.S)
            {
                pictureBoxCSoundSco_Click(this, null);
            }
            else if (e.KeyCode == Keys.P)
            {
                pictureBoxPython_Click(this, null);
            }
            else if (e.KeyCode == Keys.L)
            {
                pictureBoxLua_Click(this, null);
            }
            else if (e.KeyCode == Keys.B)
            {
                pictureBoxCabbage_Click(this, null);
            }


        }

        private void pictureBoxCSound_Click(object sender, EventArgs e)
        {
            mReturnValue = "csound";
            if (ReturnValueEv != null) ReturnValueEv(this, "csound");
            this.DialogResult = DialogResult.OK;
        }

        private void pictureBoxCSoundOrc_Click(object sender, EventArgs e)
        {
            mReturnValue = "orc";
            if (ReturnValueEv != null) ReturnValueEv(this, "orc");
            this.DialogResult = DialogResult.OK;
        }

        private void pictureBoxCSoundSco_Click(object sender, EventArgs e)
        {
            mReturnValue = "sco";
            if (ReturnValueEv != null) ReturnValueEv(this, "sco");
            this.DialogResult = DialogResult.OK;
        }

        private void pictureBoxPython_Click(object sender, EventArgs e)
        {
            mReturnValue = "python";
            if (ReturnValueEv != null) ReturnValueEv(this, "python");
            this.DialogResult = DialogResult.OK;
        }

        private void pictureBoxLua_Click(object sender, EventArgs e)
        {
            mReturnValue = "lua";
            if (ReturnValueEv != null) ReturnValueEv(this, "lua");
            this.DialogResult = DialogResult.OK;
        }

        private void pictureBoxCabbage_Click(object sender, EventArgs e)
        {
            mReturnValue = "cabbage";
            if (ReturnValueEv != null) ReturnValueEv(this, "cabbage");
            this.DialogResult = DialogResult.OK;
        }



        private void Control_MouseEnter_CSound_CSD(object sender, EventArgs e)
        {
            //if ((sender as PictureBox).Name.ToLower().Contains("csound"))
            labelCSound.BackColor = SelectedColor;
            panelCSound.BackColor = SelectedColor;
        }

        private void Control_MouseEnter_CSound_ORC(object sender, EventArgs e)
        {
            //if ((sender as PictureBox).Name.ToLower().Contains("csound"))
            labelCSoundOrc.BackColor = SelectedColor;
            panelCSoundOrc.BackColor = SelectedColor;
        }

        private void Control_MouseEnter_CSound_SCO(object sender, EventArgs e)
        {
            //if ((sender as PictureBox).Name.ToLower().Contains("csound"))
            labelCSoundSco.BackColor = SelectedColor;
            panelCSoundSco.BackColor = SelectedColor;
        }

        private void Control_MouseEnter_Python(object sender, EventArgs e)
        {
            labelPython.BackColor = SelectedColor;
            panelPython.BackColor = SelectedColor;
        }

        private void Control_MouseEnter_Lua(object sender, EventArgs e)
        {
            labelLua.BackColor = SelectedColor;
            panelLua.BackColor = SelectedColor;
        }

        private void Control_MouseEnter_Cabbage(object sender, EventArgs e)
        {
            labelCabbage.BackColor = SelectedColor;
            panelCabbage.BackColor = SelectedColor;
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            labelCSound.BackColor = this.BackColor;
            labelCSoundOrc.BackColor = this.BackColor;
            labelCSoundSco.BackColor = this.BackColor;
            labelPython.BackColor = this.BackColor;
            labelLua.BackColor = this.BackColor;
            labelCabbage.BackColor = this.BackColor;

            panelCSound.BackColor = this.BackColor;
            panelCSoundOrc.BackColor = this.BackColor;
            panelCSoundSco.BackColor = this.BackColor;
            panelPython.BackColor = this.BackColor;
            panelLua.BackColor = this.BackColor;
            panelCabbage.BackColor = this.BackColor;
        }






    }
}
