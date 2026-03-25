using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class wxCodeFormatterForm : Form
    {

        string newline = System.Environment.NewLine;



        public wxCodeFormatterForm()
        {
            InitializeComponent();
        }


        private void buttonReset_Click(object sender, EventArgs e)
        {
            //CheckBox Settings
            checkBoxFormatHeader.Checked = true;
            checkBoxFormatInstruments.Checked = true;
            checkBoxFormatFunctions.Checked = true;
            checkBoxFormatScoreInstruments.Checked = true;
            checkBoxFormatTempo.Checked = true;

            //Format Type Settings
            rbInstrumentsType1.Checked = true;
            //rbTabIndent1.Checked = true;

            //rbTabIndent1.Checked = true;
            //NudTabIndent.Value = 8;
            //rbTabIndent1.Text = "Default (" +
            //                    wxGlobal.Settings.EditorProperties.DefaultTabSize +
            //                    ")";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonSaveAndExit_Click(object sender, EventArgs e)
        {
            ApplySettings();
            //this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void wxCodeFormatterForm_Load(object sender, EventArgs e)
        {

            //CheckBox Settings
            checkBoxFormatHeader.Checked =
                wxGlobal.Settings.CodeFormat.FormatHeader;
            checkBoxFormatInstruments.Checked =
                wxGlobal.Settings.CodeFormat.FormatInstruments;
            checkBoxFormatFunctions.Checked =
                wxGlobal.Settings.CodeFormat.FormatFunctions;
            checkBoxFormatScoreInstruments.Checked =
                wxGlobal.Settings.CodeFormat.FormatScoreInstruments;
            checkBoxFormatTempo.Checked =
                wxGlobal.Settings.CodeFormat.FormatTempo;

            if (wxGlobal.Settings.CodeFormat.InstrumentsType == 0)
                rbInstrumentsType1.Checked = true;
            else
                rbInstrumentsType2.Checked = true;

            //if (wxGlobal.Settings.CodeFormat.TabIndent ==
            //    wxGlobal.Settings.EditorProperties.DefaultTabSize)
            //    rbTabIndent1.Checked = true;
            //else
            //    rbTabIndent2.Checked = true;

            //if (wxGlobal.Settings.CodeFormat.TabIndent != 0)
            //    NudTabIndent.Value = wxGlobal.Settings.CodeFormat.TabIndent;
            //else
            //    NudTabIndent.Value = wxGlobal.Settings.EditorProperties.DefaultTabSize;

            //rbTabIndent1.Text = "Default (" +
            //                    wxGlobal.Settings.EditorProperties.DefaultTabSize +
            //                    ")";

            try
            {
                textEditorExample.SetTextFont(new Font("Courier New", 8));
                textEditorExample.AllowCaretBeyondEOL = false;
                textEditorExample.ShowLineNumbers = false;
                textEditorExample.SetHighlight("winxound", "");

                //Set reduced Keywords list
                string KeyWordList = "instr endin oscili out sr kr ksmps nchnls ";
                textEditorExample.SetKeyWords(0, KeyWordList);

                string TagWordList = "<CsVersion> </CsVersion> " +
                                     "<CsoundSynthesizer> </CsoundSynthesizer> " +
                                     "<CsOptions> </CsOptions> " +
                                     "<CsInstruments> </CsInstruments> " +
                                     "<CsScore> </CsScore> ";
                textEditorExample.SetKeyWords(1, TagWordList);
                textEditorExample.SetKeyWords(3, " instr endin ");

                //'Comment Line ";" - (SCE_C_COMMENTLINE 2)
                textEditorExample.PrimaryView.StyleSetFore(2,
                    ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)));
                textEditorExample.PrimaryView.StyleSetBack(2,
                    ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)));
                //'Keyword(0): OPCODES - (SCE_C_WORD 5)
                textEditorExample.PrimaryView.StyleSetFore(5,
                    ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)));
                textEditorExample.PrimaryView.StyleSetBack(5,
                    ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)));
                //'CSD Tags, Keyword(1) - (SCE_C_WORD2 16)
                textEditorExample.PrimaryView.StyleSetFore(16,
                    ColorTranslator.ToWin32(Color.FromArgb(220, 0, 110)));
                textEditorExample.PrimaryView.StyleSetBack(16,
                    ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCodeFormatterForm - wxCodeFormatterForm_Load: " + ex.Message);
            }

            FormatCode();
        }


        private void FormatCode()
        {
            textEditorExample.ReadOnly = false;
            textEditorExample.ClearAllText();

            //if (rbTabIndent1.Checked)
            textEditorExample.TabIndent = wxGlobal.Settings.EditorProperties.DefaultTabSize;
            //else
            //    textEditorExample.TabIndent = (Int32)NudTabIndent.Value;


            textEditorExample.AppendText(
                "<CsoundSynthesizer>" + newline +
                "<CsOptions>" + newline +
                "-W -odac" + newline +
                "</CsOptions>" + newline +
                "<CsInstruments>" + newline +
                ";Simple Oscili" + newline);

            if (checkBoxFormatHeader.Checked)
            {
                textEditorExample.AppendText(
                    "\tsr     = 44100" + newline +
                    "\tkr     = 4410" + newline +
                    "\tksmps  = 10" + newline +
                    "\tnchnls = 1" + newline + newline);
            }
            else
            {
                textEditorExample.AppendText(
                    "sr = 44100" + newline +
                    "kr = 4410" + newline +
                    "ksmps = 10" + newline +
                    "nchnls = 1" + newline + newline);
            }

            if (checkBoxFormatInstruments.Checked)
            {
                if (rbInstrumentsType1.Checked)
                {
                    textEditorExample.AppendText(
                        "\tinstr 1" + newline +
                        "a1\toscili\t10000, 440, 1" + newline +
                        "\tout\ta1" + newline +
                        "\tendin" + newline);
                }
                else
                {
                    textEditorExample.AppendText(
                        "instr 1" + newline +
                        "\ta1 oscili 10000, 440, 1" + newline +
                        "\tout a1" + newline +
                        "endin" + newline);
                }
            }
            else
            {
                textEditorExample.AppendText(
                    "instr 1" + newline +
                    "a1 oscili 10000, 440, 1" + newline +
                    "out a1" + newline +
                    "endin" + newline);
            }

            textEditorExample.AppendText(
                "</CsInstruments>" + newline +
                "<CsScore>" + newline);


            if (checkBoxFormatFunctions.Checked)
            {
                textEditorExample.AppendText(
                    "f1\t0\t4096\t10\t1" + newline);
            }
            else
            {
                textEditorExample.AppendText(
                    "f1 0 4096 10 1" + newline);
            }

            if (checkBoxFormatScoreInstruments.Checked)
            {
                textEditorExample.AppendText(
                    "i1\t0\t3" + newline);
            }
            else
            {
                textEditorExample.AppendText(
                    "i1 0 3" + newline);
            }

            if (checkBoxFormatTempo.Checked)
            {
                textEditorExample.AppendText(
                    "t\t0\t60\t40\t60\t45\t30" + newline);
            }
            else
            {
                textEditorExample.AppendText(
                    "t 0 60 40 60 45 30" + newline);
            }

            textEditorExample.AppendText(
                "</CsScore>" + newline +
                "</CsoundSynthesizer>");

            textEditorExample.ReadOnly = true;

        }

        //private void rbCustomTabIndent_CheckedChanged(object sender, EventArgs e)
        //{
        //    NudTabIndent.Enabled = rbTabIndent2.Checked;
        //}

        private void ApplySettings()
        {
            //CheckBox Settings
            wxGlobal.Settings.CodeFormat.FormatHeader =
                    checkBoxFormatHeader.Checked;
            wxGlobal.Settings.CodeFormat.FormatInstruments =
                    checkBoxFormatInstruments.Checked;
            wxGlobal.Settings.CodeFormat.FormatFunctions =
                    checkBoxFormatFunctions.Checked;
            wxGlobal.Settings.CodeFormat.FormatScoreInstruments =
                    checkBoxFormatScoreInstruments.Checked;
            wxGlobal.Settings.CodeFormat.FormatTempo =
                    checkBoxFormatTempo.Checked;

            wxGlobal.Settings.CodeFormat.InstrumentsType =
                (rbInstrumentsType2.Checked ? 1 : 0);

            //wxGlobal.Settings.CodeFormat.TabIndent =
            //    (rbTabIndent2.Checked ? 1 : 0);

            //if (rbTabIndent1.Checked == true)
            //    wxGlobal.Settings.CodeFormat.TabIndent =
            //        wxGlobal.Settings.EditorProperties.DefaultTabSize;
            //else
            //    wxGlobal.Settings.CodeFormat.TabIndent =
            //        (Int32)NudTabIndent.Value;

            //wxSettings2 settings = new wxSettings2();
            wxGlobal.Settings.SaveSettings();

        }

        private void checkBoxes_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.Name == "checkBoxFormatInstruments")
                {
                    rbInstrumentsType1.Enabled = cb.Checked;
                    rbInstrumentsType2.Enabled = cb.Checked;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCodeFormatterForm - checkBoxes_CheckedChanged: " + ex.Message);
            }

            FormatCode();
        }

        private void rbInstrumentsType1_CheckedChanged(object sender, EventArgs e)
        {
            FormatCode();
        }

        private void rbInstrumentsType2_CheckedChanged(object sender, EventArgs e)
        {
            FormatCode();
        }

        private void NudTabIndent_ValueChanged(object sender, EventArgs e)
        {
            FormatCode();
        }

        private void rbTabIndent1_CheckedChanged(object sender, EventArgs e)
        {
            FormatCode();
        }

        private void wxCodeFormatterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonSaveAndExit.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                buttonCancel.PerformClick();
            }
        }





    }
}
