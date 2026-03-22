using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace WinXound_Net
{
    public partial class FormFindLine : Form
    {
        public FormFindLine()
        {
            InitializeComponent();
        }


        private Int32 mMaxLineNumbers = 0;

        public delegate void OnButtonFind(Int32 linenumber);
        public event OnButtonFind ButtonFindClick;


        private void ButtonFind_Click(object sender, EventArgs e)
        {
            try
            {
                Int32 tmpNumber = (int)(mNumber.Value) - 1;

                if (ButtonFindClick != null)
                    ButtonFindClick(tmpNumber);

                ButtonExit.PerformClick();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form FindLine - ButtonFind_Click Error");
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FormFindLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ButtonExit.PerformClick();
            }
            else if (e.KeyCode == Keys.Return)
            {
                ButtonFind.PerformClick();
            }
        }

        private void FormFindLine_Load(object sender, EventArgs e)
        {
            //SetMaxLineNumbers(mMaxLineNumbers);
        }

        public void SetMaxLineNumbers(Int32 linenumbers)
        {
            mMaxLineNumbers = linenumbers;
            mNumber.Select(0, mNumber.Value.ToString().Length);
            mNumber.Maximum = mMaxLineNumbers;
            mNumber.Focus();
        }
    }
}
