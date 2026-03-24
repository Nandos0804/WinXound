using System;
using System.Windows.Forms;




namespace WinXound_Net
{
    public partial class FormInsertHeader : Form
    {
        public FormInsertHeader()
        {
            InitializeComponent();
        }

        public delegate void OnButtonClicked(string TextToInsert, bool AtStart);
        public event OnButtonClicked ButtonClicked;

        private string newline = System.Environment.NewLine;

        private void FormInsertHeader_Load(object sender, EventArgs e)
        {
            cbSampleRate.Text = "44100";
            //RadioButtonCurrent.Text +=
            //    string.Format(" (Line: {0}, Col: {1})",
            //                  mSender.ActiveTextAreaControl.Caret.Position.Line + 1,
            //                  mSender.ActiveTextAreaControl.Caret.Position.Column + 1);
        }

        private void cbSampleRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            nControlRate.Value = Convert.ToDecimal(Convert.ToDouble(cbSampleRate.Text) * 0.1);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }



        private void ButtonInsert_Click(object sender, EventArgs e)
        {
            try
            {
                Int32 ksmps = Convert.ToInt32(cbSampleRate.Text) /
                              Convert.ToInt32(nControlRate.Text);

                //string mString = "\t" + "sr = " + cbSampleRate.Text + newline +
                //                 "\t" + "kr = " + nControlRate.Text + newline +
                //                 "\t" + "ksmps = " + ksmps + newline +
                //                 "\t" + "nchnls = " + nChannels.Text + newline;

                string mString = "sr = " + cbSampleRate.Text + newline +
                                 "kr = " + nControlRate.Text + newline +
                                 "ksmps = " + ksmps + newline +
                                 "nchnls = " + nChannels.Text + newline;

                if (ButtonClicked != null)
                    ButtonClicked(mString, RadioButtonStart.Checked);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form InsertHeader - ButtonInsert_Click");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();


        }

        private void FormInsertHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) ButtonCancel.PerformClick();
        }





    }
}
