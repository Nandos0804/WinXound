using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

using ScintillaTextEditor;


namespace WinXound_Net
{
    class wxImportExport
    {

        private string newline = System.Environment.NewLine;

        private string mWorkingDirectory = null;
        public void SetWorkingDirectory(string WorkingDirectory)
        {
            mWorkingDirectory = WorkingDirectory;
        }

        public wxImportExport(string WorkingDirectory)
        {
            mWorkingDirectory = WorkingDirectory;
        }



        ////////////////////////////////////////////////////////////////////////////////////
        //IMPORT CLASSES

        //ORC/SCO
        public string ImportORCSCO(ref string SciEditFileName)
        {
            try
            {
                string tempOrc = "";
                string tempSco = "";

                string OrcFileName = SciEditFileName.Remove(SciEditFileName.Length - 4, 4) + ".orc";
                string ScoFileName = SciEditFileName.Remove(SciEditFileName.Length - 4, 4) + ".sco";

                //Verify that files exists and load them 
                if (File.Exists(OrcFileName))
                {
                    tempOrc = File.ReadAllText(OrcFileName, System.Text.Encoding.Default);
                }
                if (File.Exists(ScoFileName))
                {
                    tempSco = File.ReadAllText(ScoFileName, System.Text.Encoding.Default);
                }

                string tempString = "<CsoundSynthesizer>" + newline + newline +
                                    "<CsOptions>" + newline +
                                    "</CsOptions>" + newline + newline +
                                    "<CsInstruments>" + newline + newline +
                                    tempOrc + newline + newline +
                                    "</CsInstruments>" + newline + newline +
                                    "<CsScore>" + newline + newline +
                                    tempSco + newline + newline +
                                    "</CsScore>" + newline + newline +
                                    "</CsoundSynthesizer>" + newline + newline;

                ////SciEditFileName = SciEditFileName.Remove(SciEditFileName.Length - 4, 4) + ".csd";
                SciEditFileName = Path.GetFileNameWithoutExtension(SciEditFileName) + ".csd";

                return tempString;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxImportExport - ImportORCSCO");
                return "";
            }

        }

        //ORC
        public void ImportORC(TextEditor textEditor, string OrcFileName)
        {
            try
            {
                string tempOrc = File.ReadAllText(OrcFileName, System.Text.Encoding.Default);
                if (string.IsNullOrEmpty(tempOrc)) return;

                tempOrc = tempOrc.Insert(0, newline + newline);
                tempOrc += newline;

                Int32 startInstr = 0;
                Int32 endInstr = 0;

                //Search for <CsInstruments> section: start and end
                //Find <CsInstruments> 
                startInstr = textEditor.FindText("<CsInstruments>", true, true, false, false, false, true, 0, -1);
                if (startInstr == -1)
                {
                    MessageBox.Show("<CsInstruments> tag not found!" + newline +
                                    "Please insert it in the code and retry.",
                                    "Import Orc Error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                    //tempOrc.Insert(0, "<CsInstruments>");
                    //startInstr = textEditor.GetCaretPosition();
                }

                //Find </CsInstruments> 
                endInstr = textEditor.FindText("</CsInstruments>", true, true, false, false, false, true, 0, -1);
                if (endInstr == -1)
                {
                    MessageBox.Show("</CsInstruments> tag not found!" + newline +
                                    "Please insert it in the code and retry.",
                                    "Import Orc Error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                    //tempOrc += "</CsInstruments>";
                    //endInstr = startInstr;
                }

                textEditor.SetSelection(startInstr + "<CsInstruments>".Length, endInstr);

                //Replace/Insert text
                //textEditor.PrimaryView.BeginUndoAction();
                textEditor.SetSelectedText(textEditor.ConvertEolOfString(tempOrc));
                //textEditor.ConvertEOL(textEditor.GetEolMode());
                //textEditor.PrimaryView.EndUndoAction();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxImportExport - ImportORC");
            }

        }


        //SCO
        public void ImportSCO(TextEditor textEditor, string ScoFileName)
        {
            try
            {
                string tempSco = File.ReadAllText(ScoFileName, System.Text.Encoding.Default);
                if (string.IsNullOrEmpty(tempSco)) return;

                tempSco = tempSco.Insert(0, newline + newline);
                tempSco += newline;

                Int32 startScore = 0;
                Int32 endScore = 0;

                //Search for <CsScore> section: start and end
                //Find <CsScore> 
                startScore = textEditor.FindText("<CsScore>", true, true, false, false, false, true, 0, -1);
                if (startScore == -1)
                {
                    MessageBox.Show("<CsScore> tag not found!" + newline +
                                    "Please insert it in the code and retry.",
                                    "Import Sco Error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                //Find </CsScore> 
                endScore = textEditor.FindText("</CsScore>", true, true, false, false, false, true, 0, -1);
                if (endScore == -1)
                {
                    MessageBox.Show("</CsScore> tag not found!" + newline +
                                    "Please insert it in the code and retry.",
                                    "Import Sco Error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                textEditor.SetSelection(startScore + "<CsScore>".Length, endScore);

                //Replace/Insert text
                //textEditor.PrimaryView.BeginUndoAction();
                textEditor.SetSelectedText(textEditor.ConvertEolOfString(tempSco));
                //textEditor.ConvertEOL(textEditor.GetEolMode());
                //textEditor.PrimaryView.EndUndoAction();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxImportExport - ImportSCO");
            }
        }




        ////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////




        ////////////////////////////////////////////////////////////////////////////////////
        // EXPORT CLASSES
        public void ExportOrcSco(string mText, string SciEditFileName)
        {

            int InizioOrchestra = 0;
            int FineOrchestra = 0;
            int InizioScore = 0;
            int FineScore = 0;

            RichTextBox RichTextBox1 = new RichTextBox();
            RichTextBox RichTextBox2 = new RichTextBox();
            RichTextBox RichTextBox3 = new RichTextBox();

            RichTextBox1.Text = mText;
            RichTextBox2.Text = "";
            RichTextBox3.Text = "";

            //Find <CsInstruments> tags 
            try
            {
                InizioOrchestra = RichTextBox1.Find("<CsInstruments>", 0, RichTextBox1.Text.Length,
                                  RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
                if (!(InizioOrchestra > 0))
                    throw new Exception("Unable to find <CsInstruments> section");

                FineOrchestra = RichTextBox1.Find("</CsInstruments>", 0, RichTextBox1.Text.Length,
                                RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
                if (!(FineOrchestra > 0))
                    throw new Exception("Unable to find </CsInstruments> section");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                RichTextBox1.Dispose();
                RichTextBox2.Dispose();
                RichTextBox3.Dispose();
                return;
            }

            RichTextBox1.SelectionStart = InizioOrchestra + "<CsInstruments>".Length;
            RichTextBox1.SelectionLength = FineOrchestra - RichTextBox1.SelectionStart;

            //Set Text in RTB2 
            RichTextBox2.AppendText(RichTextBox1.SelectedText.Trim());

            //Deselect text in richtextbox 
            RichTextBox1.SelectionLength = 0;
            RichTextBox1.SelectionStart = 0;


            //Find <CsScore> tags 
            try
            {
                InizioScore = RichTextBox1.Find("<CsScore>", 0, RichTextBox1.Text.Length,
                                                RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
                if (!(InizioScore > 0))
                    throw new Exception("Unable to find <CsScore> section");

                FineScore = RichTextBox1.Find("</CsScore>", 0, RichTextBox1.Text.Length,
                                                RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
                if (!(FineScore > 0))
                    throw new Exception("Unable to find </CsScore> section");

                RichTextBox1.SelectionStart = InizioScore + "<CsScore>".Length;
                RichTextBox1.SelectionLength = FineScore - RichTextBox1.SelectionStart;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                RichTextBox1.Dispose();
                RichTextBox2.Dispose();
                RichTextBox3.Dispose();
                return;
            }

            //Set Text in RTB3 
            RichTextBox3.AppendText(RichTextBox1.SelectedText.Trim());



            //Save Files as ORC and SCO 
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(SciEditFileName);
            saveFileDialog1.RestoreDirectory = true;

            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) &&
                (saveFileDialog1.FileName.Length) > 0)
            {

                RichTextBox2.SaveFile(saveFileDialog1.FileName + ".orc", RichTextBoxStreamType.PlainText);
                RichTextBox3.SaveFile(saveFileDialog1.FileName + ".sco", RichTextBoxStreamType.PlainText);

                MessageBox.Show("Export done!", "Export file ...",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }

            RichTextBox1.Dispose();
            RichTextBox2.Dispose();
            RichTextBox3.Dispose();

        }


        public void ExportORC(string mText, string OrcFileName)
        {

            RichTextBox tempOrc = new RichTextBox();
            RichTextBox tempEditor = new RichTextBox();
            tempEditor.Text = mText;

            string StringToFind = null;
            Int32 mFindPos = -1;
            Int32 mFindPosStart = 0;
            Int32 mFindPosEnd = 0;

            //Find <CsInstruments> 
            StringToFind = "<CsInstruments>";
            mFindPos = tempEditor.Find(StringToFind, 0, tempEditor.Text.Length,
                                       RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
            if (mFindPos > -1)
            {
                mFindPosStart = mFindPos + StringToFind.Length;
            }
            else
            {
                MessageBox.Show("Unable to find <CsInstruments> section", "Error",
                                MessageBoxButtons.OK);
                return;
            }

            //Find </CsInstruments> 
            mFindPos = -1;
            StringToFind = "</CsInstruments>";
            mFindPos = tempEditor.Find(StringToFind, 0, tempEditor.Text.Length,
                                       RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
            if (mFindPos > -1)
            {
                mFindPosEnd = mFindPos;
            }
            else
            {
                MessageBox.Show("Unable to find </CsInstruments> section", "Error",
                                MessageBoxButtons.OK);
                return;
            }


            tempOrc.AppendText(tempEditor.Text.Substring(mFindPosStart, mFindPosEnd - mFindPosStart).Trim());


            //Save Files as ORC
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(OrcFileName);
            saveFileDialog1.Filter = "CSound Orc Files (*.orc)|*.orc|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) &&
                (saveFileDialog1.FileName.Length) > 0)
            {

                tempOrc.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                MessageBox.Show("Export done!", "Export file ...",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }

            tempOrc.Dispose();
            tempEditor.Dispose();

        }


        public void ExportSCO(string mText, string ScoFileName)
        {

            RichTextBox tempSco = new RichTextBox();
            RichTextBox tempEditor = new RichTextBox();
            tempEditor.Text = mText;

            string StringToFind = null;
            Int32 mFindPos = -1;
            Int32 mFindPosStart = 0;
            Int32 mFindPosEnd = 0;

            //Find <CsInstruments> 
            StringToFind = "<CsScore>";
            mFindPos = tempEditor.Find(StringToFind, 0, tempEditor.Text.Length,
                                       RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
            if (mFindPos > -1)
            {
                mFindPosStart = mFindPos + StringToFind.Length;
            }
            else
            {
                MessageBox.Show("Unable to find <CsScore> section", "Error",
                                MessageBoxButtons.OK);
                return;
            }

            //Find </CsInstruments> 
            mFindPos = -1;
            StringToFind = "</CsScore>";
            mFindPos = tempEditor.Find(StringToFind, 0, tempEditor.Text.Length,
                                       RichTextBoxFinds.WholeWord | RichTextBoxFinds.MatchCase);
            if (mFindPos > -1)
            {
                mFindPosEnd = mFindPos;
            }
            else
            {
                MessageBox.Show("Unable to find </CsScore> section", "Error",
                                MessageBoxButtons.OK);
                return;
            }


            tempSco.AppendText(tempEditor.Text.Substring(mFindPosStart, mFindPosEnd - mFindPosStart).Trim());


            //Save Files as ORC
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(ScoFileName);
            saveFileDialog1.Filter = "CSound Sco Files (*.sco)|*.sco|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) &&
                (saveFileDialog1.FileName.Length) > 0)
            {

                tempSco.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                MessageBox.Show("Export done!", "Export file ...",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }

            tempSco.Dispose();
            tempEditor.Dispose();

        }



    }//End (Class)

}
