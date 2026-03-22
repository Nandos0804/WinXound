using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WinXound_Net
{
    public partial class wxCodeRepositoryModifyForm : Form
    {
        public wxCodeRepositoryModifyForm()
        {
            InitializeComponent();
            UDOTextEditor.Text = "";
        }

        private string mText = "";
        private string mFileName = "";

        public void SetText(string text)
        {
            mText = text;
            UDOTextEditor.Text = mText;
        }

        public void SetFile(string filename)
        {
            mFileName = filename;
            try
            {
                groupBoxFile.Text = Path.GetFileName(filename);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepositoryModifyForm - SetFile");
            }
        }

        private void wxCodeRepositoryModifyForm_Load(object sender, EventArgs e)
        {

        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonSaveExit_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(mFileName, UDOTextEditor.Text);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepositoryModifyForm - buttonSaveExit_Click");
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
