using System;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormInputBox : Form
    {
        private string mMessage = "";
        private string mTitle = "";

        public FormInputBox(string Title, string Message)
        {
            InitializeComponent();
            mTitle = Title;
            mMessage = Message;
            this.Title = mTitle;
            this.Message = mMessage;
        }


        public string Message
        {
            set { mMessage = value; }
        }

        public string Title
        {
            set { mTitle = value; }
        }

        public string SetValue
        {
            set { textBoxInput.Text = value; }
        }

        public void ResetFocus()
        {
            textBoxInput.Focus();
        }

        public string ReturnValue
        {
            get { return textBoxInput.Text.Trim(); }
        }


        private void FormInputBox_Load(object sender, EventArgs e)
        {
            this.Text = mTitle;
            this.labelMessage.Text = mMessage;
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

        private void FormInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                buttonCancel_Click(this, null);
        }
    }
}
