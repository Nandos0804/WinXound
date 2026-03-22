using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormOrcScoLinks : Form
    {
        public FormOrcScoLinks()
        {
            InitializeComponent();
        }

        public string RetValue = "";

        private void FormOrcScoLinks_Load(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            RetValue = ListBoxOrcSco.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void FormOrcScoLinks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                buttonCancel.PerformClick();
        }

        private void ListBoxOrcSco_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonOK_Click(this, null);
        }

        private void ListBoxOrcSco_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
                buttonOK_Click(this, null);
        }
    }
}
