using System;
using System.Drawing;
using System.Windows.Forms;


namespace WinXound_Net
{
    public partial class FormFindAndReplace : Form
    {
        public FormFindAndReplace()
        {
            InitializeComponent();
        }

        public delegate void OnButtonFind(string StringToFind, bool MatchWholeWord,
                                              bool MatchCase, bool IsBackward);
        public event OnButtonFind ButtonFindClick;

        public delegate void OnButtonReplace(string StringToFind, string ReplaceString,
                                             bool MatchWholeWord, bool MatchCase,
                                             bool FromCaretPosition,
                                             bool FCPUp, bool ReplaceAll);
        public event OnButtonReplace ButtonReplaceClick;



        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            //this.Close();
            FormFindAndReplace_FormClosing(this, null);
            //if (FormHidden != null) FormHidden(this, null);
            this.Owner.Focus();
            this.Hide();

        }

        private void FormFindAndReplace_Load(object sender, EventArgs e)
        {
            try
            {
                this.Location = new Point(this.Owner.Right - this.Width - 5,
                                          this.Owner.Bottom - this.Height - 5);
            }
            catch { }


            CheckBoxMatchWholeWord.Checked =
                wxGlobal.Settings.General.FindWholeWord;
            CheckBoxMatchCase.Checked =
                wxGlobal.Settings.General.FindMatchCase;
            CheckBoxReplaceFromCaret.Checked =
                wxGlobal.Settings.General.ReplaceFromCaret;

        }

        private void FormFindAndReplace_Shown(object sender, EventArgs e)
        {
            FormFindAndReplace_Load(this, null);
        }

        private void FormFindAndReplace_FormClosing(object sender, FormClosingEventArgs e)
        {
            wxGlobal.Settings.General.FindWholeWord =
                CheckBoxMatchWholeWord.Checked;
            wxGlobal.Settings.General.FindMatchCase =
                CheckBoxMatchCase.Checked;
            wxGlobal.Settings.General.ReplaceFromCaret =
                CheckBoxReplaceFromCaret.Checked;
            wxGlobal.LastWordSearch = TextBoxFind.Text;
        }

        private void FormFindAndReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ButtonExit.PerformClick();
            }
        }

        private void ButtonFindNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (TextBoxFind.Text.Length > 0)
                {

                    string StringToFind = TextBoxFind.Text.Trim();
                    //StringToFind = Regex.Escape(StringToFind);

                    if (ButtonFindClick != null)
                        ButtonFindClick(StringToFind, CheckBoxMatchWholeWord.Checked,
                                     CheckBoxMatchCase.Checked, false);

                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Find - ButtonFindNext_Click");
            }
        }


        private void ButtonFindPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (TextBoxFind.Text.Length > 0)
                {

                    string StringToFind = TextBoxFind.Text.Trim();
                    //StringToFind = Regex.Escape(StringToFind);

                    if (ButtonFindClick != null)
                        ButtonFindClick(StringToFind, CheckBoxMatchWholeWord.Checked,
                                     CheckBoxMatchCase.Checked, true);

                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Find - ButtonFindPrevious_Click");
            }
        }


        private void ButtonReplace_Click(object sender, EventArgs e)
        {
            try
            {
                if (ButtonReplaceClick != null)
                    ButtonReplaceClick("", TextBoxReplace.Text, false,
                                       false, false, false, false);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Find - ButtonReplace_Click");
            }
        }

        private void ButtonReplaceAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (TextBoxFind.Text.Length > 0)
                {
                    string StringToFind = TextBoxFind.Text.Trim();
                    if (ButtonReplaceClick != null)
                        ButtonReplaceClick(StringToFind, TextBoxReplace.Text,
                                           CheckBoxMatchWholeWord.Checked,
                                           CheckBoxMatchCase.Checked,
                                           CheckBoxReplaceFromCaret.Checked,
                                           rbUp.Checked, true);

                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Find - ButtonReplace_Click");
            }
        }

        private void CheckBoxReplaceFromCaret_CheckedChanged(object sender, EventArgs e)
        {
            rbDown.Enabled = CheckBoxReplaceFromCaret.Checked;
            rbUp.Enabled = CheckBoxReplaceFromCaret.Checked;
        }




    }
}
