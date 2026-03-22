using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace WinXound_Net
{
    public partial class wxCodeRepositoryInputBox : Form
    {
        public wxCodeRepositoryInputBox(string title, string message, bool showdescription)
        {
            InitializeComponent();

            this.Text = title;
            this.labelMessage.Text = message;
            if (!showdescription)
            {
                this.textBoxDescription.Enabled = false;
                this.labelDescription.Enabled = false;
            }
        }

        private void wxCodeRepositoryInputBox_Load(object sender, EventArgs e)
        {

        }

        public struct CodeRepositoryInputBoxReturnValue
        {
            public string FileName;
            public string Description;
            public string Node;
        }

        public string Title
        {
            set { this.Text = value; }
        }

        public string Message
        {
            set { labelMessage.Text = value; }
        }

        public CodeRepositoryInputBoxReturnValue ReturnValue
        {
            get
            {
                CodeRepositoryInputBoxReturnValue rv;
                rv.FileName = textBoxFilename.Text.Trim();
                rv.Description = textBoxDescription.Text.Trim();
                rv.Node = listBoxNode.Text;
                return rv;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void SetNodeList(ArrayList nodelist)
        {
            try
            {
                listBoxNode.Items.Clear();
                foreach (string s in nodelist)
                {
                    listBoxNode.Items.Add(s);
                }
                listBoxNode.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, 
                        "Form CodeRepositoryInputBox - SetNodeList");
            }
        }

        private void wxCodeRepositoryInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) buttonCancel.PerformClick();
        }



    }
}
