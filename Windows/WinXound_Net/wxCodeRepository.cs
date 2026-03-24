using ScintillaTextEditor;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace WinXound_Net
{
    public partial class wxCodeRepository : UserControl
    {
        private struct TextEditorSyntax
        {
            public string DefaultFontName;
            public Int32 DefaultFontSize;
            public Int32 DefaultTabSize;
            public bool ShowVerticalRuler;
            public bool ShowMatchingBracket;
            public bool ShowLineNumbers;
            public bool ShowIntelliTip;
            public bool ShowExplorer;
            public bool MarkCaretLine;

            public Int32 TextForeColor;
            public Int32 TextBackColor;
            public Int32 TextSelectionForeColor;
            public Int32 TextSelectionBackColor;

            public Int32 VerticalRulerColor;
            public Int32 MarkCaretLineColor;

            public Int32 OpcodesForeColor;
            public Int32 OpcodesBackColor;
            public bool OpcodesBold;
            public bool OpcodesItalic;

            public Int32 TagsForeColor;
            public Int32 TagsBackColor;
            public bool TagsBold;
            public bool TagsItalic;

            public Int32 MacrosForeColor;
            public Int32 MacrosBackColor;
            public bool MacrosBold;
            public bool MacrosItalic;

            public Int32 InstrForeColor;
            public Int32 InstrBackColor;
            public bool InstrBold;
            public bool InstrItalic;

            public Int32 StringsForeColor;
            public Int32 StringsBackColor;
            public bool StringsBold;
            public bool StringsItalic;

            public Int32 RemindersForeColor;
            public Int32 RemindersBackColor;
            public bool RemindersBold;
            public bool RemindersItalic;

            public Int32 MarginsForeColor;
            public Int32 MarginsBackColor;

            public Int32 BookmarksForeColor;
            public Int32 BookmarksBackColor;
            public Int32 BookmarksAlpha;
        }
        private TextEditorSyntax mSyntax;





        public wxCodeRepository()
        {
            InitializeComponent();


            SetDefaultSyntax();

            TreeViewUserData.GotFocus +=
                new EventHandler(TreeViewUserData_GotFocus);

            //TextBoxUdo.PrimaryView.SetCursor(2);
            TextBoxUdo.SetHighlight("winxound", "");

            TextBoxUdo.AllowCaretBeyondEOL = false;

        }

        private string newline = System.Environment.NewLine;


        public void SetFocus()
        {
            TextBoxUdo.Focus();
            //TreeViewUserData.Focus();
        }


        private void TreeViewUserData_GotFocus(object sender, EventArgs e)
        {
            TextBoxUdo.SetFocus();
        }

        private void wxCodeRepository_Load(object sender, EventArgs e)
        {
            try
            {
                if (TreeViewUserData.Nodes.Count > 0)
                {
                    TreeViewUserData.ExpandAll();
                    TreeViewUserData.TopNode = TreeViewUserData.Nodes[0];
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - wxCodeRepository_Load");
            }
        }

        public void ShowFirstUDO()
        {
            try
            {
                if (TreeViewUserData.Nodes.Count > 0)
                    TreeViewUserData.SelectedNode = TreeViewUserData.Nodes[0].FirstNode;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - ShowFirstUDO");
            }
        }


        private void UDO_ShowInfo()
        {
            try
            {
                if (TreeViewUserData.SelectedNode.Parent == null)
                {
                    TextBoxUdo.ReadOnly = false;
                    TextBoxUdo.ClearAllText();
                    TextBoxUdo.ReadOnly = true;
                    TextBoxUdo.Focus();
                    return;
                }

                string tempFile = Application.StartupPath + "\\CodeRepository\\" +
                                  TreeViewUserData.SelectedNode.Parent.Text + "\\" +
                                  TreeViewUserData.SelectedNode.Text + ".udo";

                if (!File.Exists(tempFile)) return;

                if (TreeViewUserData.SelectedNode.Text == "Instructions")
                {
                    TextBoxUdo.SetHighlight("none", "");
                }
                else TextBoxUdo.SetHighlight("winxound", "");


                TextBoxUdo.ReadOnly = false;
                TextBoxUdo.ClearAllText();
                TextBoxUdo.SetText(File.ReadAllText(tempFile));

                TextBoxUdo.ReadOnly = true;
                TextBoxUdo.Focus();
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - UDO_ShowInfo");
            }


        }

        public void UpdateUdoList()
        {
            //Fill TreeViewUserData with Udo and other personal code 
            try
            {
                TreeViewUserData.Nodes.Clear();

                TreeViewUserData.BeginUpdate();

                Int32 mIndex = 0;
                string mRootPath = Application.StartupPath + "\\CodeRepository\\";
                string mOpcodes = "";

                if (Directory.Exists(mRootPath))
                {
                    foreach (string dir in Directory.GetDirectories(mRootPath))
                    {
                        string tempDir = Path.GetFileName(dir);
                        TreeViewUserData.Nodes.Add(tempDir);

                        string[] mFiles = Directory.GetFiles(dir, "*.udo");
                        if (mFiles.Length > 0)
                        {
                            foreach (string file in mFiles)
                            {
                                string tempFile = file.Remove(file.Length - 4);
                                tempFile = tempFile.Substring(dir.Length + 1);
                                TreeViewUserData.Nodes[mIndex].Nodes.Add(tempFile);
                                mOpcodes += tempFile + " ";
                            }
                        }
                        mIndex++;
                    }


                    TextBoxUdo.SetKeyWords(2, mOpcodes); //2 Personal Opcodes
                    TextBoxUdo.PrimaryView.Colourise(0, -1);

                    TreeViewUserData.EndUpdate();
                    TreeViewUserData.ExpandAll();
                    TreeViewUserData.SelectedNode = TreeViewUserData.Nodes[0].FirstNode;

                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - UpdateUdoList");
            }

        }

        private void TextBoxUdo_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetData(DataFormats.Text).ToString() == TextBoxUdo.Text) return;
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - TextBoxUdo_DragEnter");
            }
        }


        public void InsertText(string text)
        {
            try
            {
                RichTextBox tempRTB = new RichTextBox();


                wxCodeRepositoryInputBox ib =
                    new wxCodeRepositoryInputBox(" Save User Code as ...", "FileName:", true);

                ArrayList nodes = new ArrayList();
                foreach (TreeNode tn in TreeViewUserData.Nodes)
                {
                    nodes.Add(tn.Text);
                }
                ib.SetNodeList(nodes);
                ib.ShowDialog(this);

                wxCodeRepositoryInputBox.CodeRepositoryInputBoxReturnValue ret =
                    ib.ReturnValue;
                ib.Close();

                if (string.IsNullOrEmpty(ret.FileName)) return;
                if (string.IsNullOrEmpty(ret.Node)) return;

                string tempFile = Application.StartupPath + "\\CodeRepository\\" +
                           ret.Node + "\\" + ret.FileName + ".udo";

                if (File.Exists(tempFile))
                {
                    DialogResult resp =
                        MessageBox.Show("Udo code already exists. Do you want to continue?",
                                        "File already exists",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
                    if (resp == DialogResult.No)
                    {
                        TextBoxUdo.Focus();
                        return;
                    }

                }

                //Udo description
                string desc = ret.Description;

                if (desc.Length > 0)
                {
                    desc = "; " + desc;
                }

                //Add Text to tempRTB and save it to file
                if (desc.Length > 0) tempRTB.AppendText(desc + newline);
                tempRTB.AppendText(text);
                tempRTB.SaveFile(tempFile, RichTextBoxStreamType.PlainText);


                //Add new Udo and Refresh TreeViewUserData 
                UpdateUdoList();
                TreeViewUserData.ExpandAll();
                this.SelectNode(TreeViewUserData, ret.Node, ret.FileName);

                UDO_ShowInfo();
                TextBoxUdo.Focus();
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - InsertText");
            }
        }



        private void TextBoxUdo_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.Text).ToString() == TextBoxUdo.Text) return;

            this.InsertText(e.Data.GetData(DataFormats.Text).ToString());
        }

        private void SelectNode(TreeView tv, string ParentNodeText, string ChildNodeText)
        {
            try
            {
                foreach (TreeNode tn in tv.Nodes)
                {
                    if (tn.Text == ParentNodeText)
                    {
                        tn.Expand();
                        foreach (TreeNode tn_child in tn.Nodes)
                        {
                            if (tn_child.Text == ChildNodeText)
                            {
                                tv.SelectedNode = tn_child;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - SelectNode");
            }
        }

        private void TextBoxUdo_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    TextBoxUdo.DoDragDrop(TextBoxUdo.Text, DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - TextBoxUdo_MouseMove");
            }
        }

        private void TextBoxUdo_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }



        private void ContextModifyUDO_Click(object sender, EventArgs e)
        {
            try
            {
                string tempFile = Application.StartupPath + "\\CodeRepository\\" +
                                      TreeViewUserData.SelectedNode.Parent.Text + "\\" +
                                      TreeViewUserData.SelectedNode.Text + ".udo";

                if (!File.Exists(tempFile)) return;

                wxCodeRepositoryModifyForm mCodeModify = new wxCodeRepositoryModifyForm();
                mCodeModify.SetText(File.ReadAllText(tempFile));
                mCodeModify.SetFile(tempFile);

                if (mCodeModify.ShowDialog(this) == DialogResult.Cancel) return;

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - ContextModifyUDO_Click");
            }

            UpdateUdoList();
        }

        private void ContextDeleteUDO_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult ret = MessageBox.Show("Are you sure?", "Delete UDO",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Warning);

                if (ret == DialogResult.Yes)
                {
                    string tempFile = Application.StartupPath + "\\CodeRepository\\" +
                                      TreeViewUserData.SelectedNode.Parent.Text + "\\" +
                                      TreeViewUserData.SelectedNode.Text + ".udo";

                    Int32 RootNodeIndex = TreeViewUserData.SelectedNode.Parent.Index;
                    Int32 ChildNodeIndex = TreeViewUserData.SelectedNode.Index;

                    File.Delete(tempFile);

                    UpdateUdoList();
                    if (TreeViewUserData.Nodes[RootNodeIndex].Nodes.Count == 0)
                    {
                        TreeViewUserData.SelectedNode =
                            TreeViewUserData.Nodes[RootNodeIndex];
                    }
                    else if (ChildNodeIndex < TreeViewUserData.Nodes[RootNodeIndex].Nodes.Count)
                    {
                        TreeViewUserData.SelectedNode =
                            TreeViewUserData.Nodes[RootNodeIndex].Nodes[ChildNodeIndex];
                    }
                    else if (ChildNodeIndex >= TreeViewUserData.Nodes[RootNodeIndex].Nodes.Count)
                    {
                        TreeViewUserData.SelectedNode =
                            TreeViewUserData.Nodes[RootNodeIndex].LastNode;
                    }

                    TreeViewUserData.ExpandAll();

                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - ContextDeleteUDO_Click");
            }
        }

        private void ContextRenameUDO_Click(object sender, EventArgs e)
        {
            try
            {
                FormInputBox ib = new FormInputBox(" Rename as ...", "Enter new name: ");
                ib.ShowDialog(this);

                if (string.IsNullOrEmpty(ib.ReturnValue)) return;

                string newFile = Application.StartupPath + "\\CodeRepository\\" +
                                 TreeViewUserData.SelectedNode.Parent.Text + "\\" +
                                 ib.ReturnValue + ".udo";

                string oldFile = Application.StartupPath + "\\CodeRepository\\" +
                                 TreeViewUserData.SelectedNode.Parent.Text + "\\" +
                                 TreeViewUserData.SelectedNode.Text + ".udo";

                Int32 RootNodeIndex = TreeViewUserData.SelectedNode.Parent.Index;
                Int32 ChildNodeIndex = TreeViewUserData.SelectedNode.Index;

                File.Move(oldFile, newFile);

                UpdateUdoList();
                TreeViewUserData.ExpandAll();
                TreeViewUserData.SelectedNode =
                    TreeViewUserData.Nodes[RootNodeIndex].Nodes[ChildNodeIndex];
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - ContextRenameUDO_Click");
            }
        }


        private void TreeViewUserData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UDO_ShowInfo();
        }

        private void TreeViewUserData_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                TreeNode selectedNode = TreeViewUserData.GetNodeAt(e.X, e.Y);
                TreeViewUserData.SelectedNode = selectedNode;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form CodeRepository - TreeViewUserData_MouseDown");
            }
        }

        private void splitContainer1_Resize(object sender, EventArgs e)
        {
            try
            {
                if (splitContainer1.Panel1.Width >
                    splitContainer1.Parent.Width)
                {
                    splitContainer1.SplitterDistance = 200;
                }
            }
            catch //(Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, 
                //                        "Form CodeRepository - splitContainer1_Resize");
            }
        }













        ///////////////////////////////////////////////////////////////////////////////
        // CONFIGURE SCINTILLA STYLES
        ///////////////////////////////////////////////////////////////////////////////

        //TODO: CHANGE Syntax styles according to wxGlobal ...

        //void ConfigureEditor(Hashtable Opcodes)
        public void ConfigureEditor(Hashtable Opcodes)
        {
            //Set Keywords list
            string KeyWordList = "";
            foreach (DictionaryEntry de in Opcodes)
            {
                if (de.Key.ToString() != "instr" && de.Key.ToString() != "endin")
                {
                    KeyWordList += de.Key + " ";
                }
            }
            TextBoxUdo.SetKeyWords(0, KeyWordList);

            string TagWordList = "<CsVersion> </CsVersion> " +
                                    "<CsoundSynthesizer> </CsoundSynthesizer> " +
                                    "<CsOptions> </CsOptions> " +
                                    "<CsInstruments> </CsInstruments> " +
                                    "<CsScore> </CsScore> ";
            TextBoxUdo.SetKeyWords(1, TagWordList);

            TextBoxUdo.SetKeyWords(3, " instr endin ");

            TextBoxUdo.SetWordChars(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_");

            this.SciEditSetFontsAndStyles(TextBoxUdo.PrimaryView);
            this.SciEditSetFontsAndStyles(TextBoxUdo.SecondaryView);
        }

        private void SciEditSetFontsAndStyles(ScintillaTextEditor.TextView mSciEdit)
        {
            //'FROM SCINTILLA (SCILEXER.H)
            //'#define SCE_C_DEFAULT 0
            //'#define SCE_C_COMMENT 1
            //'#define SCE_C_COMMENTLINE 2
            //'#define SCE_C_COMMENTDOC 3
            //'#define SCE_C_NUMBER 4
            //'#define SCE_C_WORD 5
            //'#define SCE_C_STRING 6
            //'#define SCE_C_CHARACTER 7
            //'#define SCE_C_UUID 8
            //'#define SCE_C_PREPROCESSOR 9
            //'#define SCE_C_OPERATOR 10
            //'#define SCE_C_IDENTIFIER 11
            //'#define SCE_C_STRINGEOL 12
            //'#define SCE_C_VERBATIM 13
            //'#define SCE_C_REGEX 14
            //'#define SCE_C_COMMENTLINEDOC 15
            //'#define SCE_C_WORD2 16
            //'#define SCE_C_COMMENTDOCKEYWORD 17
            //'#define SCE_C_COMMENTDOCKEYWORDERROR 18
            //'#define SCE_C_GLOBALCLASS 19

            for (Int32 i = 0; i < 33; i++)
            {
                mSciEdit.StyleSetFont(i, mSyntax.DefaultFontName);
                mSciEdit.StyleSetSize(i, mSyntax.DefaultFontSize);
            }
            mSciEdit.SetZoom(0);
            mSciEdit.SetTabWidth(mSyntax.DefaultTabSize);
            //mSciEdit.SetPasteConvertEndings(true);   //???

            mSciEdit.SetCaretLineBack(mSyntax.MarkCaretLineColor);
            mSciEdit.SetEdgeColour(mSyntax.VerticalRulerColor);
            mSciEdit.MarkerSetFore(0, mSyntax.BookmarksForeColor);
            mSciEdit.MarkerSetBack(0, mSyntax.BookmarksBackColor);
            mSciEdit.MarkerSetAlpha(0, mSyntax.BookmarksAlpha);


            //'DEFAULT TEXT COLORS - (SCE_C_DEFAULT 0)
            mSciEdit.StyleSetFore(0, mSyntax.TextForeColor);
            mSciEdit.StyleSetBack(0, mSyntax.TextBackColor);

            //'(SCE_C_IDENTIFIER 11)
            mSciEdit.StyleSetFore(11, mSyntax.TextForeColor);
            mSciEdit.StyleSetBack(11, mSyntax.TextBackColor);

            //'Comment Multi /**/ - (SCE_C_COMMENT 1)
            mSciEdit.StyleSetFore(1, mSyntax.RemindersForeColor);
            mSciEdit.StyleSetBack(1, mSyntax.RemindersBackColor);
            mSciEdit.StyleSetBold(1, mSyntax.RemindersBold);
            mSciEdit.StyleSetItalic(1, mSyntax.RemindersItalic);

            //'Comment Line ";" - (SCE_C_COMMENTLINE 2)
            mSciEdit.StyleSetFore(2, mSyntax.RemindersForeColor);
            mSciEdit.StyleSetBack(2, mSyntax.RemindersBackColor);
            mSciEdit.StyleSetBold(2, mSyntax.RemindersBold);
            mSciEdit.StyleSetItalic(2, mSyntax.RemindersItalic);

            //'Numbers color - (SCE_C_NUMBER 4)
            mSciEdit.StyleSetFore(4, mSyntax.TextForeColor);
            mSciEdit.StyleSetBack(4, mSyntax.TextBackColor);

            //'Keyword(0): OPCODES - (SCE_C_WORD 5)
            mSciEdit.StyleSetFore(5, mSyntax.OpcodesForeColor);
            mSciEdit.StyleSetBack(5, mSyntax.OpcodesBackColor);
            mSciEdit.StyleSetBold(5, mSyntax.OpcodesBold);
            mSciEdit.StyleSetItalic(5, mSyntax.OpcodesItalic);

            //'Double quoted string - (SCE_C_STRING 6)
            mSciEdit.StyleSetFore(6, mSyntax.StringsForeColor);
            mSciEdit.StyleSetBack(6, mSyntax.StringsBackColor);
            mSciEdit.StyleSetBold(6, mSyntax.StringsBold);
            mSciEdit.StyleSetItalic(6, mSyntax.StringsItalic);

            //'Preprocessor and Macro - (SCE_C_PREPROCESSOR 9)
            mSciEdit.StyleSetFore(9, mSyntax.MacrosForeColor);
            mSciEdit.StyleSetBack(9, mSyntax.MacrosBackColor);
            mSciEdit.StyleSetBold(9, mSyntax.MacrosBold);
            mSciEdit.StyleSetItalic(9, mSyntax.MacrosItalic);

            //'Operator [=] - (SCE_C_OPERATOR 10)
            mSciEdit.StyleSetFore(10, mSyntax.OpcodesForeColor);
            mSciEdit.StyleSetBack(10, mSyntax.OpcodesBackColor);

            //'CSD Tags, Keyword(1) - (SCE_C_WORD2 16)
            mSciEdit.StyleSetFore(16, mSyntax.TagsForeColor);
            mSciEdit.StyleSetBack(16, mSyntax.TagsBackColor);
            mSciEdit.StyleSetBold(16, mSyntax.TagsBold);
            mSciEdit.StyleSetItalic(16, mSyntax.TagsItalic);

            //'UserOpcodes - (SCE_C_WORD3 20)
            mSciEdit.StyleSetFore(20, mSyntax.MacrosForeColor);
            mSciEdit.StyleSetBack(20, mSyntax.MacrosBackColor);
            mSciEdit.StyleSetBold(20, mSyntax.MacrosBold);
            mSciEdit.StyleSetItalic(20, mSyntax.MacrosItalic);

            //'INSTR/ENDIN color - Keyword(3) - (SCE_WXOUND_WORD4 21)
            mSciEdit.StyleSetFore(21, mSyntax.InstrForeColor);
            mSciEdit.StyleSetBack(21, mSyntax.InstrBackColor);
            mSciEdit.StyleSetBold(21, mSyntax.InstrBold);
            mSciEdit.StyleSetItalic(21, mSyntax.InstrItalic);

            //'DEFAULT STYLE "32"
            mSciEdit.StyleSetFore(32, mSyntax.TextForeColor);
            mSciEdit.StyleSetBack(32, mSyntax.TextBackColor);

            //'DEFAULT STYLE "33" STYLE_NUMBERS_MARGINS
            mSciEdit.StyleSetFore(SciConst.STYLE_LINENUMBER,
                mSyntax.MarginsForeColor);
            //mSyntax.TextForeColor);
            mSciEdit.StyleSetBack(SciConst.STYLE_LINENUMBER,
                mSyntax.MarginsBackColor);
            //mSyntax.TextBackColor);

            //'CARET COLOR
            mSciEdit.SetCaretFore(mSyntax.TextForeColor);

            //'TEXT SELECTION
            mSciEdit.SetSelFore(true, mSyntax.TextSelectionForeColor);       //'Fore Selection color
            mSciEdit.SetSelBack(true, mSyntax.TextSelectionBackColor);       //'Back Selection color

        }


        private void SetDefaultSyntax()
        {
            mSyntax.DefaultFontName = "Courier New";
            mSyntax.DefaultFontSize = 10;
            mSyntax.DefaultTabSize = 8;
            mSyntax.ShowMatchingBracket = true;
            mSyntax.ShowVerticalRuler = false;
            mSyntax.MarkCaretLine = false;

            mSyntax.TextForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0));
            mSyntax.TextBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.TextSelectionForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0));
            mSyntax.TextSelectionBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(192, 192, 192));

            mSyntax.OpcodesForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255));
            mSyntax.OpcodesBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.OpcodesBold = false;
            mSyntax.OpcodesItalic = false;

            mSyntax.TagsForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(220, 0, 110));
            mSyntax.TagsBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.TagsBold = false;
            mSyntax.TagsItalic = false;

            mSyntax.MacrosForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(160, 50, 160));
            mSyntax.MacrosBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.MacrosBold = false;
            mSyntax.MacrosItalic = false;

            mSyntax.InstrForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255));
            mSyntax.InstrBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.InstrBold = false;
            mSyntax.InstrItalic = false;

            mSyntax.StringsForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0));
            mSyntax.StringsBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.StringsBold = false;
            mSyntax.StringsItalic = false;

            mSyntax.RemindersForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0));
            mSyntax.RemindersBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.RemindersBold = false;
            mSyntax.RemindersItalic = false;

            mSyntax.VerticalRulerColor =
                ColorTranslator.ToWin32(Color.FromArgb(192, 192, 192));
            mSyntax.MarkCaretLineColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 100));

            mSyntax.BookmarksForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255));
            mSyntax.BookmarksBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255));
            mSyntax.BookmarksAlpha = 40;

            mSyntax.MarginsForeColor =
                ColorTranslator.ToWin32(Color.FromArgb(130, 130, 130));
            mSyntax.MarginsBackColor =
                ColorTranslator.ToWin32(Color.FromArgb(250, 250, 250));

        }

    }
}
