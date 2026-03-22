using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class wxIntelliTip : UserControl
    {
        public wxIntelliTip()
        {
            InitializeComponent();
        }


        private string mParams = "";

        public void ShowTip(string Title, string Params)
        {
            //Me.BringToFront() 

            //Const Offset = 3 

            mParams = Params;

            LabelTitle.Text = Title;
            LabelParams.Text = Params;


        }

        public void SetYellowColors()
        {
            LabelTitle.BackColor = Color.FromArgb(255, 255, 192);
            LabelParams.BackColor = Color.LightYellow;
        }

        public void SetGreyColors()
        {
            LabelTitle.BackColor = Color.FromArgb(215, 215, 215);
            LabelParams.BackColor = Color.FromArgb(230, 230, 230); ;
        }

        
    }
}
