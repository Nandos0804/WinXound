using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Xml;

namespace WinXound_Net
{
    public partial class FormSettings : Form
    {
        string newline = System.Environment.NewLine;

        public FormSettings()
        {
            InitializeComponent();
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }


        private void CreateSyntaxPanelItems(Panel mainpanel, List<string> list)
        {
            mainpanel.Controls.Clear();

            //int DescX = 0;
            int DescY = 20;
            Label lItem = new Label();
            lItem.Text = " Item:";
            lItem.Location = new Point(3, DescY);
            lItem.AutoSize = true;
            Label lFore = new Label();
            lFore.Text = " Fore:";
            lFore.Location = new Point(160, DescY);
            lFore.AutoSize = true;
            Label lBack = new Label();
            lBack.Text = " Back:";
            lBack.Location = new Point(200, DescY);
            lBack.AutoSize = true;
            Label lBold = new Label();
            lBold.Text = " Bold:";
            lBold.Location = new Point(240, DescY);
            lBold.AutoSize = true;
            Label lItalic = new Label();
            lItalic.Text = " Italic:";
            lItalic.Location = new Point(280, DescY);
            lItalic.AutoSize = true;
            Label lAlpha = new Label();
            lAlpha.Text = " Alpha:";
            lAlpha.Location = new Point(320, DescY);
            lAlpha.AutoSize = true;
            Label lEol = new Label();
            lEol.Text = " Eol filled:";
            lEol.Location = new Point(400, DescY);
            lEol.AutoSize = true;

            mainpanel.Parent.Controls.Add(lItem);
            mainpanel.Parent.Controls.Add(lFore);
            mainpanel.Parent.Controls.Add(lBack);
            mainpanel.Parent.Controls.Add(lBold);
            mainpanel.Parent.Controls.Add(lItalic);
            mainpanel.Parent.Controls.Add(lAlpha);
            //mainpanel.Parent.Controls.Add(lEol);

            int y = 5;
            //int x = 0;
            int height = 15;
            string[] split;

            foreach (string s in list)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    split = s.Split(",".ToCharArray());

                    Panel SubPanel = new Panel();
                    SubPanel.Name = split[0];
                    SubPanel.Size = new Size(mainpanel.Width,
                                             height + 3);
                    SubPanel.Location = new Point(0, y);

                    Label lName = new Label();
                    lName.Name = "FriendlyName";
                    lName.Text = split[split.Length - 1];
                    lName.AutoSize = false;
                    lName.Size = new Size(150, height);
                    lName.Location = new Point(0, 0);
                    SubPanel.Controls.Add(lName);


                    if (lName.Text != "Bookmarks")
                    {
                        Label lForeColor = new Label();
                        lForeColor.Name = "Fore";
                        lForeColor.BackColor =
                            ColorTranslator.FromWin32(Convert.ToInt32(split[1]));
                        lForeColor.AutoSize = false;
                        lForeColor.Size = new Size(30, lName.Height);
                        lForeColor.Location =
                            new Point(lFore.Location.X, 0);
                        lForeColor.Click += new EventHandler(PickColorLabels_Click);
                        lForeColor.Cursor = Cursors.Hand;
                        lForeColor.BorderStyle = BorderStyle.FixedSingle;
                        SubPanel.Controls.Add(lForeColor);
                    }


                    if (lName.Text != "VerticalRuler" &&
                        lName.Text != "CaretLineMarker")
                    {
                        Label lBackColor = new Label();
                        lBackColor.Name = "Back";
                        lBackColor.BackColor =
                            ColorTranslator.FromWin32(Convert.ToInt32(split[2]));
                        lBackColor.AutoSize = false;
                        lBackColor.Size = new Size(30, lName.Height);
                        lBackColor.Location =
                            new Point(lBack.Location.X, 0);
                        lBackColor.Click += new EventHandler(PickColorLabels_Click);
                        lBackColor.Cursor = Cursors.Hand;
                        lBackColor.BorderStyle = BorderStyle.FixedSingle;
                        SubPanel.Controls.Add(lBackColor);
                    }

                    if (lName.Text != "Margins" &&
                        lName.Text != "TextSelection" &&
                        lName.Text != "Bookmarks" &&
                        lName.Text != "VerticalRuler" &&
                        lName.Text != "CaretLineMarker")
                    {
                        CheckBox cBold = new CheckBox();
                        cBold.Name = "Bold";
                        cBold.Checked = Convert.ToBoolean(split[3]);
                        cBold.Size = new Size(30, lName.Height);
                        cBold.Location =
                            new Point(lBold.Location.X, 0);
                        SubPanel.Controls.Add(cBold);

                        CheckBox cItalic = new CheckBox();
                        cItalic.Name = "Italic";
                        cItalic.Checked = Convert.ToBoolean(split[4]);
                        cItalic.Size = new Size(30, lName.Height);
                        cItalic.Location =
                            new Point(lItalic.Location.X, 0);
                        SubPanel.Controls.Add(cItalic);
                    }

                    if (lName.Text == "Bookmarks")
                    {
                        NumericUpDown nudAlpha = new NumericUpDown();
                        nudAlpha.Name = "Alpha";
                        nudAlpha.TextAlign = HorizontalAlignment.Center;
                        nudAlpha.Maximum = 150;
                        if (!string.IsNullOrEmpty(split[5]))
                            nudAlpha.Value = Convert.ToInt32(split[5]);
                        else nudAlpha.Value = 40;
                        nudAlpha.Size = new Size(50, lName.Height + 2);
                        nudAlpha.Location =
                            new Point(lAlpha.Location.X, 0);
                        SubPanel.Controls.Add(nudAlpha);
                    }

                    mainpanel.Controls.Add(SubPanel);
                    y += SubPanel.Height + 3;
                }
            }
        }

        private List<string> RetrieveSyntaxPanelItems(Panel mainpanel)
        {
            List<string> tempList = new List<string>();
            string tempString;
            string StyleNumber;
            string ForeColor;
            string BackColor;
            string Bold;
            string Italic;
            string Alpha;
            string EOL;
            string FriendlyName;

            foreach (Panel p in mainpanel.Controls)
            {
                StyleNumber = p.Name;
                ForeColor = "0";
                BackColor = "16777215";
                Bold = "False";
                Italic = "False";
                Alpha = "256";
                EOL = "False";
                FriendlyName = "";

                foreach (Control c in p.Controls)
                {
                    switch (c.Name)
                    {
                        case "Fore":
                            ForeColor = Convert.ToString(
                                ColorTranslator.ToWin32(c.BackColor));
                            break;
                        case "Back":
                            BackColor = Convert.ToString(
                                ColorTranslator.ToWin32(c.BackColor));
                            break;
                        case "Bold":
                            Bold = Convert.ToString(((CheckBox)c).Checked);
                            break;
                        case "Italic":
                            Italic = Convert.ToString(((CheckBox)c).Checked);
                            break;
                        case "Alpha":
                            Alpha = Convert.ToString(((NumericUpDown)c).Value);
                            break;
                        case "EOL":
                            break;
                        case "FriendlyName":
                            FriendlyName = c.Text;
                            break;
                    }
                }

                tempString = StyleNumber + "," +
                             ForeColor + "," + BackColor + "," +
                             Bold + "," + Italic + "," +
                             Alpha + "," + EOL + "," +
                             FriendlyName;

                tempList.Add(tempString);

            }

            return tempList;
        }









        private void FormSettings_Load(object sender, EventArgs e)
        {
            try
            {
                //Directories
                //CSound
                LabelCSoundConsole.Text = wxGlobal.Settings.Directory.CSoundConsole;
                LabelWinsound.Text = wxGlobal.Settings.Directory.Winsound;
                LabelCSoundAV.Text = wxGlobal.Settings.Directory.CSoundAV;
                LabelCSoundHelpHTML.Text = wxGlobal.Settings.Directory.CSoundHelpHTML;
                LabelCSoundAVHelp.Text = wxGlobal.Settings.Directory.CSoundAVHelp;
                LabelWaveEditor.Text = wxGlobal.Settings.Directory.WaveEditor;
                LabelWorkingDir.Text = wxGlobal.Settings.Directory.WorkingDir;
                //Python
                LabelPythonConsole.Text = wxGlobal.Settings.Directory.PythonConsolePath;
                LabelPythonIdle.Text = wxGlobal.Settings.Directory.PythonIdlePath;
                LabelLuaConsole.Text = wxGlobal.Settings.Directory.LuaConsolePath;
                LabelLuaGUI.Text = wxGlobal.Settings.Directory.LuaGuiPath;
                //LabelCabbageExe.Text = wxGlobal.Settings.Directory.CabbagePath;


                //Environment Variables
                LabelSFDIR.Text = wxGlobal.Settings.Directory.SFDIR;
                LabelSSDIR.Text = wxGlobal.Settings.Directory.SSDIR;
                LabelSADIR.Text = wxGlobal.Settings.Directory.SADIR;
                LabelINCDIR.Text = wxGlobal.Settings.Directory.INCDIR;
                LabelMFDIR.Text = wxGlobal.Settings.Directory.MFDIR;
                LabelOPCODEDIR.Text = wxGlobal.Settings.Directory.OPCODEDIR;
                checkBoxSFDIR.Checked = wxGlobal.Settings.Directory.UseSFDIR;


                //Compiler
                TextBoxCSoundFlags.Text = wxGlobal.Settings.General.CSoundDefaultFlags;
                TextBoxPythonFlags.Text = wxGlobal.Settings.General.PythonDefaultFlags;
                TextBoxLuaFlags.Text = wxGlobal.Settings.General.LuaDefaultFlags;
                LabelCompilerDefaultFontName.Text = wxGlobal.Settings.General.CompilerFontName;
                NumericUpDownCompilerFontSize.Value = wxGlobal.Settings.General.CompilerFontSize;

                rbOpenSoundFileWithNothing.Checked =
                    (wxGlobal.Settings.General.OpenSoundFileWith == 0);
                rbOpenSoundFileWithOSPlayer.Checked =
                    (wxGlobal.Settings.General.OpenSoundFileWith == 1);
                rbOpenSoundFileWithWaveEditor.Checked =
                    (wxGlobal.Settings.General.OpenSoundFileWith == 2);


                //Text and Syntax 
                LabelDefaultFontName.Text = wxGlobal.Settings.EditorProperties.DefaultFontName;
                NumericUpDownFontSize.Value = wxGlobal.Settings.EditorProperties.DefaultFontSize;
                NumericUpDownTabSize.Value = wxGlobal.Settings.EditorProperties.DefaultTabSize;

                checkBoxShowMatchingBracket.Checked = wxGlobal.Settings.EditorProperties.ShowMatchingBracket;
                checkBoxShowVRuler.Checked = wxGlobal.Settings.EditorProperties.ShowVerticalRuler;
                checkBoxMarkCaretLine.Checked = wxGlobal.Settings.EditorProperties.MarkCaretLine;
                checkBoxUseMixedPython.Checked = wxGlobal.Settings.EditorProperties.UseMixedPython;
                checkBoxSaveBookmarks.Checked = wxGlobal.Settings.EditorProperties.SaveBookmarks;

                CreateSyntaxPanelItems(PanelCSoundColors, wxGlobal.Settings.EditorProperties.CSoundStyles);
                CreateSyntaxPanelItems(PanelPythonColors, wxGlobal.Settings.EditorProperties.PythonStyles);
                CreateSyntaxPanelItems(PanelLuaColors, wxGlobal.Settings.EditorProperties.LuaStyles);


                //GENERAL
                //Startup Action
                radioButtonSANothing.Checked =
                    (wxGlobal.Settings.General.StartupAction == 0);
                radioButtonSANewCSound.Checked =
                    (wxGlobal.Settings.General.StartupAction == 1);
                radioButtonSANewPython.Checked =
                    (wxGlobal.Settings.General.StartupAction == 2);
                radioButtonSANewLua.Checked =
                    (wxGlobal.Settings.General.StartupAction == 3);
                radioButtonSALastSessionFiles.Checked =
                    (wxGlobal.Settings.General.StartupAction == 4);
                //ORC SCO IMPORT
                radioButtonOSIAskAlways.Checked =
                    (wxGlobal.Settings.General.OrcScoImport == 0);
                radioButtonOSIConvertToCsd.Checked =
                    (wxGlobal.Settings.General.OrcScoImport == 1);
                radioButtonOSIOpenSeparately.Checked =
                    (wxGlobal.Settings.General.OrcScoImport == 2);
                //DEFAULT WAVE PLAYER
                radioButtonWavePlayerInternal.Checked =
                    (wxGlobal.Settings.General.DefaultWavePlayer == 0);
                radioButtonWavePlayerExternal.Checked =
                    (wxGlobal.Settings.General.DefaultWavePlayer == 1);
                //Templates
                RtbTemplateCSound.Text = wxGlobal.Settings.Templates.CSound;
                RtbTemplatePython.Text = wxGlobal.Settings.Templates.Python;
                RtbTemplateLua.Text = wxGlobal.Settings.Templates.Lua;
                RtbTemplateCabbage.Text = wxGlobal.Settings.Templates.Cabbage;

                checkBoxShowReadOnlyFileMessage.Checked =
                    wxGlobal.Settings.General.ShowReadOnlyFileMessage;

                //checkBoxCabbageFileUpdated.Checked =
                //    wxGlobal.Settings.General.BringWinXoundToFrontForCabbage;


                //EXPLORER
                checkBoxExplorerShowOptions.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowOptions;
                checkBoxExplorerShowInstrMacros.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowInstrMacros;
                checkBoxExplorerShowInstrOpcodes.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowInstrOpcodes;
                checkBoxExplorerShowInstrInstruments.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowInstrInstruments;
                checkBoxExplorerShowScoreFunctions.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowScoreFunctions;
                checkBoxExplorerShowScoreMacros.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowScoreMacros;
                checkBoxExplorerShowScoreSections.Checked =
                    wxGlobal.Settings.EditorProperties.ExplorerShowScoreSections;

                radioButtonExplorerFontSizeSmall.Checked =
                    (wxGlobal.Settings.EditorProperties.ExplorerFontSize == 0);
                radioButtonExplorerFontSizeMedium.Checked =
                   (wxGlobal.Settings.EditorProperties.ExplorerFontSize == 1);
                radioButtonExplorerFontSizeLarge.Checked =
                   (wxGlobal.Settings.EditorProperties.ExplorerFontSize == 2);



                //Toolbar
                Int32 mIndex = 0;
                foreach (ListViewItem li in listViewToolBar.Items)
                {
                    if (mIndex < wxGlobal.Settings.General.ToolBarItems.Count)
                        li.Checked = wxGlobal.Settings.General.ToolBarItems[mIndex];
                    else
                        li.Checked = true;
                    mIndex++;
                }

                //Additional CSound Flags
                TextBoxAlternativeCSoundFlags.Clear();
                foreach (string s in wxGlobal.Settings.General.CSoundAdditionalFlags)
                {
                    if(!string.IsNullOrEmpty(s))
                        TextBoxAlternativeCSoundFlags.AppendText(s + newline);
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormSettings_Load: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - FormSettings_Load");
            }

        }



        private void ButtonSaveExit_Click(object sender, EventArgs e)
        {
            ApplySettings();
            wxGlobal.Settings.SaveSettings();
            this.DialogResult = DialogResult.OK;
        }

        private void ApplySettings()
        {
            try
            {
                //Directories
                //CSound
                wxGlobal.Settings.Directory.CSoundConsole = LabelCSoundConsole.Text;
                wxGlobal.Settings.Directory.Winsound = LabelWinsound.Text;
                wxGlobal.Settings.Directory.CSoundAV = LabelCSoundAV.Text;
                wxGlobal.Settings.Directory.CSoundHelpHTML = LabelCSoundHelpHTML.Text;
                wxGlobal.Settings.Directory.CSoundAVHelp = LabelCSoundAVHelp.Text;
                wxGlobal.Settings.Directory.WaveEditor = LabelWaveEditor.Text;
                wxGlobal.Settings.Directory.WorkingDir = LabelWorkingDir.Text;
                //Python
                wxGlobal.Settings.Directory.PythonConsolePath = LabelPythonConsole.Text;
                wxGlobal.Settings.Directory.PythonIdlePath = LabelPythonIdle.Text;
                //Lua
                wxGlobal.Settings.Directory.LuaConsolePath = LabelLuaConsole.Text;
                wxGlobal.Settings.Directory.LuaGuiPath = LabelLuaGUI.Text;
                //wxGlobal.Settings.Directory.CabbagePath = LabelCabbageExe.Text;

                //Environment variables 
                wxGlobal.Settings.Directory.SFDIR = LabelSFDIR.Text;
                wxGlobal.Settings.Directory.SSDIR = LabelSSDIR.Text;
                wxGlobal.Settings.Directory.SADIR = LabelSADIR.Text;
                wxGlobal.Settings.Directory.INCDIR = LabelINCDIR.Text;
                wxGlobal.Settings.Directory.MFDIR = LabelMFDIR.Text;
                wxGlobal.Settings.Directory.OPCODEDIR = LabelOPCODEDIR.Text;
                wxGlobal.Settings.Directory.UseSFDIR = checkBoxSFDIR.Checked;


                //Compiler
                wxGlobal.Settings.General.CSoundDefaultFlags = TextBoxCSoundFlags.Text.Trim();
                wxGlobal.Settings.General.PythonDefaultFlags = TextBoxPythonFlags.Text.Trim();
                wxGlobal.Settings.General.LuaDefaultFlags = TextBoxLuaFlags.Text.Trim();
                wxGlobal.Settings.General.CompilerFontName = LabelCompilerDefaultFontName.Text;
                wxGlobal.Settings.General.CompilerFontSize = (int)NumericUpDownCompilerFontSize.Value;

                if (rbOpenSoundFileWithWaveEditor.Checked == true)
                    wxGlobal.Settings.General.OpenSoundFileWith = 2;
                else if (rbOpenSoundFileWithOSPlayer.Checked == true)
                    wxGlobal.Settings.General.OpenSoundFileWith = 1;
                else wxGlobal.Settings.General.OpenSoundFileWith = 0;


                //Text and Syntax 
                wxGlobal.Settings.EditorProperties.DefaultFontName = LabelDefaultFontName.Text;
                wxGlobal.Settings.EditorProperties.DefaultFontSize = (int)NumericUpDownFontSize.Value;
                wxGlobal.Settings.EditorProperties.DefaultTabSize = (int)NumericUpDownTabSize.Value;

                wxGlobal.Settings.EditorProperties.ShowMatchingBracket = checkBoxShowMatchingBracket.Checked;
                wxGlobal.Settings.EditorProperties.ShowVerticalRuler = checkBoxShowVRuler.Checked;
                wxGlobal.Settings.EditorProperties.MarkCaretLine = checkBoxMarkCaretLine.Checked;
                wxGlobal.Settings.EditorProperties.UseMixedPython = checkBoxUseMixedPython.Checked;
                wxGlobal.Settings.EditorProperties.SaveBookmarks = checkBoxSaveBookmarks.Checked;

                wxGlobal.Settings.EditorProperties.CSoundStyles.Clear();
                wxGlobal.Settings.EditorProperties.PythonStyles.Clear();
                wxGlobal.Settings.EditorProperties.LuaStyles.Clear();

                wxGlobal.Settings.EditorProperties.CSoundStyles.AddRange(
                    RetrieveSyntaxPanelItems(PanelCSoundColors));
                wxGlobal.Settings.EditorProperties.PythonStyles.AddRange(
                    RetrieveSyntaxPanelItems(PanelPythonColors));
                wxGlobal.Settings.EditorProperties.LuaStyles.AddRange(
                    RetrieveSyntaxPanelItems(PanelLuaColors));


                //GENERAL
                //Startup Action
                if (radioButtonSALastSessionFiles.Checked == true)
                    wxGlobal.Settings.General.StartupAction = 4;
                else if (radioButtonSANewLua.Checked == true)
                    wxGlobal.Settings.General.StartupAction = 3;
                else if (radioButtonSANewPython.Checked == true)
                    wxGlobal.Settings.General.StartupAction = 2;
                else if (radioButtonSANewCSound.Checked == true)
                    wxGlobal.Settings.General.StartupAction = 1;
                else 
                    wxGlobal.Settings.General.StartupAction = 0;
                //ORC SCO IMPORT
                if (radioButtonOSIOpenSeparately.Checked == true)
                    wxGlobal.Settings.General.OrcScoImport = 2;
                else if (radioButtonOSIConvertToCsd.Checked == true)
                    wxGlobal.Settings.General.OrcScoImport = 1;
                else 
                    wxGlobal.Settings.General.OrcScoImport = 0;
                //DEFAULT WAVE PLAYER
                if (radioButtonWavePlayerInternal.Checked == true)
                    wxGlobal.Settings.General.DefaultWavePlayer = 0;
                else if(radioButtonWavePlayerExternal.Checked == true)
                    wxGlobal.Settings.General.DefaultWavePlayer = 1;
                //Templates
                wxGlobal.Settings.Templates.CSound = RtbTemplateCSound.Text;
                wxGlobal.Settings.Templates.Python = RtbTemplatePython.Text;
                wxGlobal.Settings.Templates.Lua = RtbTemplateLua.Text;
                wxGlobal.Settings.Templates.Cabbage = RtbTemplateCabbage.Text;

                wxGlobal.Settings.General.ShowReadOnlyFileMessage =
                    checkBoxShowReadOnlyFileMessage.Checked;

                //wxGlobal.Settings.General.BringWinXoundToFrontForCabbage =
                //    checkBoxCabbageFileUpdated.Checked;


                //EXPLORER
                wxGlobal.Settings.EditorProperties.ExplorerShowOptions =
                    checkBoxExplorerShowOptions.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowInstrMacros =
                    checkBoxExplorerShowInstrMacros.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowInstrOpcodes =
                    checkBoxExplorerShowInstrOpcodes.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowInstrInstruments =
                    checkBoxExplorerShowInstrInstruments.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowScoreFunctions =
                    checkBoxExplorerShowScoreFunctions.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowScoreMacros =
                    checkBoxExplorerShowScoreMacros.Checked;
                wxGlobal.Settings.EditorProperties.ExplorerShowScoreSections =
                    checkBoxExplorerShowScoreSections.Checked;

                if (radioButtonExplorerFontSizeLarge.Checked == true)
                    wxGlobal.Settings.EditorProperties.ExplorerFontSize = 2;
                else if (radioButtonExplorerFontSizeMedium.Checked == true)
                    wxGlobal.Settings.EditorProperties.ExplorerFontSize = 1;
                else wxGlobal.Settings.EditorProperties.ExplorerFontSize = 0;


                //Toolbar
                Int32 mIndex = 0;
                foreach (ListViewItem li in listViewToolBar.Items)
                {
                    if (mIndex < wxGlobal.Settings.General.ToolBarItems.Count)
                        wxGlobal.Settings.General.ToolBarItems[mIndex] = li.Checked;
                    else
                        wxGlobal.Settings.General.ToolBarItems.Add(li.Checked);

                    mIndex++;
                }

                //Additional CSound Flags
                wxGlobal.Settings.General.CSoundAdditionalFlags.Clear();
                TextBoxAlternativeCSoundFlags.Text = TextBoxAlternativeCSoundFlags.Text.Trim();
                foreach(string s in TextBoxAlternativeCSoundFlags.Lines)
                {
                    if (!string.IsNullOrEmpty(s))
                        wxGlobal.Settings.General.CSoundAdditionalFlags.Add(s);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormSettings - ApplySettings: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - ApplySettings");
            }

        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void FormSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) ButtonCancel.PerformClick();
        }


        private string mLastFolder = "";
        private void ButtonBrowseClick(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog openBrowserDialog1 = new FolderBrowserDialog();
                Button mButton = (Button)sender;

                try
                {
                    openBrowserDialog1.Description =
                        mButton.Name.Remove(0, 12) + " Folder:";
                    openBrowserDialog1.SelectedPath = mLastFolder;
                }
                catch { }


                if (!(openBrowserDialog1.ShowDialog() == DialogResult.OK))
                {
                    return;
                }

                string mPath = openBrowserDialog1.SelectedPath;
                mLastFolder = mPath;

                switch (mButton.Name)
                {

                    case "ButtonBrowseSFDIR":
                        LabelSFDIR.Text = mPath;
                        break;

                    case "ButtonBrowseSSDIR":
                        LabelSSDIR.Text = mPath;
                        break;

                    case "ButtonBrowseSADIR":
                        LabelSADIR.Text = mPath;
                        break;

                    case "ButtonBrowseMFDIR":
                        LabelMFDIR.Text = mPath;
                        break;

                    case "ButtonBrowseINCDIR":
                        LabelINCDIR.Text = mPath;
                        break;

                    case "ButtonBrowseOPCODEDIR":
                        LabelOPCODEDIR.Text = mPath;
                        break;

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Settings - ButtonBrowseClick: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowseClick");
            }

        }

        private void ButtonClearClick(object sender, EventArgs e)
        {
            try
            {
                Button mButton = (Button)sender;

                switch (mButton.Name)
                {
                    //ENVIRONMENT:
                    case "ButtonClearSFDIR":
                        LabelSFDIR.Text = "";
                        break;

                    case "ButtonClearSSDIR":
                        LabelSSDIR.Text = "";
                        break;

                    case "ButtonClearSADIR":
                        LabelSADIR.Text = "";
                        break;

                    case "ButtonClearMFDIR":
                        LabelMFDIR.Text = "";
                        break;

                    case "ButtonClearINCDIR":
                        LabelINCDIR.Text = "";
                        break;

                    case "ButtonClearOPCODEDIR":
                        LabelOPCODEDIR.Text = "";
                        break;


                    //DIRECTORIES:
                    case "buttonClearCSoundExe":
                        LabelCSoundConsole.Text = "";
                        break;

                    case "buttonClearCSoundHelp":
                        LabelCSoundHelpHTML.Text = "";
                        break;

                    case "buttonClearCSoundGUI":
                        LabelWinsound.Text = "";
                        break;

                    case "buttonClearCSoundAV":
                        LabelCSoundAV.Text = "";
                        break;

                    case "buttonClearCSoundAVHelp":
                        LabelCSoundAVHelp.Text = "";
                        break;

                    case "buttonClearWaveEditor":
                        LabelWaveEditor.Text = "";
                        break;

                    case "buttonClearWorkingDir":
                        LabelWorkingDir.Text = "";
                        break;

                    case "buttonClearPythonExe":
                        LabelPythonConsole.Text = "";
                        break;

                    case "buttonClearPythonGUI":
                        LabelPythonIdle.Text = "";
                        break;

                    case "buttonClearLuaExe":
                        LabelLuaConsole.Text = "";
                        break;

                    case "buttonClearLuaGUI":
                        LabelLuaGUI.Text = "";
                        break;

                    //case "buttonClearCabbageExe":
                    //    LabelCabbageExe.Text = "";
                    //    break;



                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Settings - ButtonClearClick: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonClearClick");
            }
        }





        ///////////////////////////////////////////////////////////////////////////////
        //PICK COLORS FOR SYNTAX HIGHLIGHT
        private bool PickColor(Label lb)
        {
            try
            {
                ColorDialog cd = new ColorDialog();
                cd.AllowFullOpen = true;
                cd.AnyColor = true;
                cd.FullOpen = true;
                cd.Color = lb.BackColor;

                if ((cd.ShowDialog() == DialogResult.OK))
                {
                    lb.BackColor = cd.Color;
                    cd.Dispose();
                    //IsChangedSyntax = true;
                    return true;
                }
                cd.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Settings - PickColor: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - PickColor");
            }

            return false;
        }

        private void PickColorLabels_Click(object sender, EventArgs e)
        {
            if (PickColor((Label)sender))
            {
                Label tempLabel = (Label)sender;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////




        private void ButtonFontName_Click(object sender, EventArgs e)
        {

            FontDialog fd = new FontDialog();

            Button b = sender as Button;

            //if (b.Name == "ButtonFontName")
            //{
            //    //Hack to display all fonts types
            //    MethodInfo mi = typeof(FontDialog).GetMethod("SetOption",
            //                    BindingFlags.NonPublic | BindingFlags.Instance);
            //    mi.Invoke(fd, new object[] { 0x40000, false });
            //}

            fd.ShowEffects = false;
            fd.ShowColor = false;
            fd.FontMustExist = true;

            if (!string.IsNullOrEmpty(wxGlobal.Settings.EditorProperties.DefaultFontName))
            {
                try
                {
                    fd.Font = new Font(wxGlobal.Settings.EditorProperties.DefaultFontName,
                                       wxGlobal.Settings.EditorProperties.DefaultFontSize);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Form Settings - ButtonFontName_Click: " + ex.Message);
                }
            }

            try
            {

                if ((fd.ShowDialog() == DialogResult.OK))
                {
                    //Button b = sender as Button;
                    switch (b.Name)
                    {
                        case "ButtonFontName":
                            if (!string.IsNullOrEmpty(fd.Font.Name))
                            {
                                LabelDefaultFontName.Text = fd.Font.Name;
                            }
                            if ((int)fd.Font.Size < NumericUpDownFontSize.Minimum)
                            {
                                NumericUpDownFontSize.Value = NumericUpDownFontSize.Minimum;
                            }
                            else if ((int)fd.Font.Size > NumericUpDownFontSize.Maximum)
                            {
                                NumericUpDownFontSize.Value = NumericUpDownFontSize.Maximum;
                            }
                            else
                            {
                                NumericUpDownFontSize.Value = (decimal)fd.Font.Size;
                            }
                            break;

                        case "ButtonCompilerFontName":
                            if (!string.IsNullOrEmpty(fd.Font.Name))
                            {
                                LabelCompilerDefaultFontName.Text = fd.Font.Name;
                            }
                            if ((int)fd.Font.Size < NumericUpDownFontSize.Minimum)
                            {
                                NumericUpDownCompilerFontSize.Value = NumericUpDownCompilerFontSize.Minimum;
                            }
                            else if ((int)fd.Font.Size > NumericUpDownFontSize.Maximum)
                            {
                                NumericUpDownCompilerFontSize.Value = NumericUpDownCompilerFontSize.Maximum;
                            }
                            else
                            {
                                NumericUpDownCompilerFontSize.Value = (decimal)fd.Font.Size;
                            }
                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonFontName_Click");
            }

            fd.Dispose();

        }

        private void ButtonAutoSearch_Click(object sender, EventArgs e)
        {
            FormSettingsAutoSearch fsat = new FormSettingsAutoSearch();
            fsat.ReturnValueEv += new FormSettingsAutoSearch.OnReturnValue(fsat_ReturnValueEv);
            //if (fsat.DialogResult != DialogResult.OK) return;
            fsat.ShowDialog(this);

            fsat.ReturnValueEv -= new FormSettingsAutoSearch.OnReturnValue(fsat_ReturnValueEv);
            fsat.Dispose();
        }
        void fsat_ReturnValueEv(object sender, string[] ReturnValue)
        {
            //Apply path settings
            string name = "";
            string value = "";
            foreach (string s in ReturnValue)
            {
                name = s.Split("|".ToCharArray())[0];
                value = s.Split("|".ToCharArray())[1];
                switch (name)
                {
                    case "csound.exe":
                        LabelCSoundConsole.Text = value;
                        break;
                    case "csound.manual":
                        LabelCSoundHelpHTML.Text = value;
                        break;
                    case "csound.gui":
                        LabelWinsound.Text = value;
                        break;
                    case "python.exe":
                        LabelPythonConsole.Text = value;
                        break;
                    case "python.idle":
                        LabelPythonIdle.Text = value;
                        break;
                    case "lua.exe":
                        LabelLuaConsole.Text = value;
                        break;
                }
            }
        }




        private void ButtonsBrowseDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                Button bb = (Button)sender;
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                switch (bb.Name)
                {

                    case "ButtonBrowseCSoundConsole":
                        openFileDialog1.Filter = "CSound exe (*.exe)|*.exe|All files (*.*)|*.*";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.CSoundConsole =
                            //    openFileDialog1.FileName;
                            LabelCSoundConsole.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                    case "ButtonBrowseWinsound":
                        openFileDialog1.Filter = "CSound5 GUI exe (*.exe)|*.exe|All files (*.*)|*.*";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.Winsound =
                            //    openFileDialog1.FileName;
                            LabelWinsound.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                    case "ButtonBrowseCSoundAV":
                        openFileDialog1.Filter = "CSoundAV exe (*.exe)|*.exe|All files (*.*)|*.*";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.CSoundAV =
                            //    openFileDialog1.FileName;
                            LabelCSoundAV.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                    case "ButtonBrowseCSoundHelpHTML":
                        openFileDialog1.Filter = "CSound HTML Help (index.html)|index.html;index.htm";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.CSoundHelpHTML =
                            //    openFileDialog1.FileName;
                            LabelCSoundHelpHTML.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                    case "ButtonBrowseCSoundAVHelp":
                        openFileDialog1.Filter = "CSoundAV HTML Help (index.htm)|index.htm;index.html";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.CSoundAVHelp =
                            //    openFileDialog1.FileName;
                            LabelCSoundAVHelp.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                    case "ButtonBrowseWaveEditor":
                        openFileDialog1.Filter = "Wave Editor (*.exe)|*.exe|All files (*.*)|*.*";
                        if (ShowOpenDialog(ref openFileDialog1))
                        {
                            //wxGlobal.Settings.Directory.WaveEditor =
                            //    openFileDialog1.FileName;
                            LabelWaveEditor.Text =
                                openFileDialog1.FileName;
                        }
                        break;

                }

                openFileDialog1.Dispose();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Settings - ButtonBrowseDirectory_Click: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowseDirectory_Click");
            }

        }

        private bool ShowOpenDialog(ref OpenFileDialog ofd)
        {
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (!(ofd.ShowDialog() == DialogResult.OK) ||
                string.IsNullOrEmpty(ofd.FileName))
            {
                return false;
            }
            else return true;
        }

        private void ButtonBrowseWorkingDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();

            if (!(openFolder.ShowDialog() == DialogResult.OK) ||
                string.IsNullOrEmpty(openFolder.SelectedPath))
            {
                return;
            }

            //wxGlobal.Settings.Directory.WorkingDir = openFolder.SelectedPath;
            LabelWorkingDir.Text = openFolder.SelectedPath;
            openFolder.Dispose();

        }

        private void ButtonDefaultCsoundFlags_Click(object sender, EventArgs e)
        {
            //wxGlobal.Settings.General.CSoundDefaultFlags = 
            //    "-B4096 --displays --asciidisplay";
            TextBoxCSoundFlags.Text = "-B4096 --displays --asciidisplay"; ;

        }

        private void ButtonDefaultCompilerFont_Click(object sender, EventArgs e)
        {
            //wxGlobal.Settings.General.CompilerFontSize = 10;
            //wxGlobal.Settings.General.CompilerFontName = "Courier New";
            LabelCompilerDefaultFontName.Text = "Courier New";
            NumericUpDownCompilerFontSize.Value = 10;
        }




        ///////////////////////////////////////////////////////////////////////////////
        //ASSOCIATE EXTENSION
        private void ButtonFileAssociate_Click(object sender, EventArgs e)
        {
            try
            {
                wxRegistry mRegister = new wxRegistry();
                mRegister.RegisterExtensionUser("CSoundCSD", ".csd", "CSound CSD file",
                                            Application.StartupPath + "\\Icons\\csd.ico",
                                            "WinXound_Net.exe", null);
                mRegister.RegisterExtensionUser("CSoundORC", ".orc", "CSound ORC file",
                                            Application.StartupPath + "\\Icons\\orc.ico",
                                            "WinXound_Net.exe", null);
                mRegister.RegisterExtensionUser("CSoundSCO", ".sco", "CSound SCO file",
                                            Application.StartupPath + "\\Icons\\sco.ico",
                                            "WinXound_Net.exe", null);

                MessageBox.Show("CSound files association executed!",
                                "Files Association", MessageBoxButtons.OK);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - RegisterExtension Error");
            }
        }


        ///////////////////////////////////////////////////////////////////////////////
        //DISASSOCIATE EXTENSION
        private void ButtonFileRemove_Click(object sender, EventArgs e)
        {
            try
            {
                wxRegistry mRegister = new wxRegistry();
                mRegister.UnRegisterExtensionUser("CSoundCSD", ".csd");
                mRegister.UnRegisterExtensionUser("CSoundORC", ".orc");
                mRegister.UnRegisterExtensionUser("CSoundSCO", ".sco");

                MessageBox.Show("CSound files association removed!", "Files Association", MessageBoxButtons.OK);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - UnRegisterExtension Error");
            }
        }







        private void buttonResetToolBar_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem li in listViewToolBar.Items)
                {
                    li.Checked = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Settings - buttonResetToolBar_Click: " + ex.Message);
                //wxGlobal.wxMessageError(ex.Message, "Form Settings - buttonResetToolBar_Click");
            }
        }

        private void listViewToolBar_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            e.Item.Selected = false;
        }










        //////////////////////////////////////////////////////////////////////////////
        // EXPORT SETTINGS
        private void buttonExportSettings_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "WinXound Settings File (*.wxs)|*.wxs";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = "WinXoundSettings";

                if (!(saveFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    return;
                }

                //Apply and Save current settings
                ApplySettings();
                bool ret = wxGlobal.Settings.SaveSettings(saveFileDialog1.FileName);

                if (ret)
                {
                    MessageBox.Show("Settings exported successfully",
                                    "WinXound Information",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("An error has occurred during export!",
                                    "WinXound Information",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                        "Form Settings - buttonExportSettings_Click");
            }

        }

        //////////////////////////////////////////////////////////////////////////////
        // IMPORT SETTINGS
        private void buttonImportSettings_Click(object sender, EventArgs e)
        {
            try
            {

                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "WinXound Settings File (*.wxs)|*.wxs";
                openFileDialog1.FilterIndex = 1;
                ////openFileDialog1.FileName = "WinXoundSettings";
                openFileDialog1.RestoreDirectory = true;

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    return;
                }


                //Retrieve FileSettings Version
                Single fileSettingsVersion = 0;
                string field = "";
                string value = "";
                try
                {
                    //Read datas from file
                    string[] input = File.ReadAllLines(openFileDialog1.FileName);
                    System.Diagnostics.Debug.WriteLine(input[0]);
                    if (input[0].Contains("WinXoundSettings")) //New settings
                    {
                        foreach (string s in input)
                        {
                            field = s.Split("|".ToCharArray())[0];
                            value = s.Split("|".ToCharArray())[1];
                            if (String.IsNullOrEmpty(value)) continue;

                            if (field == "VERSION")
                            {
                                fileSettingsVersion = Convert.ToSingle(value);
                                break;
                            }
                        }
                    }
                    else fileSettingsVersion = 0;

                }
                catch (Exception ex)
                {
                    fileSettingsVersion = 0;
                    System.Diagnostics.Debug.WriteLine(
                        "Form Settings - buttonImportSettings_Click: " + ex.Message);
                }


                if (fileSettingsVersion >= 3)
                {

                    DialogResult ret = MessageBox.Show(
                        "Your current WinXound settings will be overwritten" + newline +
                        "by this new imported version." + newline +
                        "Would you like to proceed?",
                        "WinXound Settings Information",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (ret != DialogResult.Yes) return;


                    //Import version 3 or major
                    bool result =
                        wxGlobal.Settings.LoadSettings(openFileDialog1.FileName, false);

                    if (result)
                    {
                        MessageBox.Show("Settings imported successfully!",
                                        "WinXound Information",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        wxGlobal.Settings.CreateDefaultWinXoundSettings();
                        MessageBox.Show("An error has occurred during import!",
                                        "WinXound Information",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }

                    wxGlobal.Settings.SaveSettings();
                    this.DialogResult = DialogResult.OK;
                }

                else
                {
                    MessageBox.Show("Sorry, settings versions before 'WinXound 3.2.120'" + newline +
                                    "are no more supported!",
                                    "WinXound Settings Information",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Settings - buttonImportSettings_Click");
            }

        }

        private void buttonCreateDesktopShortcut_Click(object sender, EventArgs e)
        {
            wxGlobal.CreateDesktopShortcut();
        }





        private void ButtonPythonDefaultArguments_Click(object sender, EventArgs e)
        {
            TextBoxPythonFlags.Text = "-u";
        }



        private void ButtonBrowsePythonConsole_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Python exe (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    return;
                }

                //wxGlobal.Settings.Directory.PythonConsolePath =
                //    openFileDialog1.FileName;
                LabelPythonConsole.Text =
                    openFileDialog1.FileName;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowsePythonConsole_Click");
            }

        }

        private void ButtonBrowsePythonIdle_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Python Idle (*.pyw)|*.pyw|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    return;
                }

                //wxGlobal.Settings.Directory.PythonIdlePath =
                //    openFileDialog1.FileName;
                LabelPythonIdle.Text =
                    openFileDialog1.FileName;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowsePythonIdle_Click");
            }
        }





        private void ButtonDefaultEditorFont_Click(object sender, EventArgs e)
        {
            LabelDefaultFontName.Text = "Courier New";
            NumericUpDownFontSize.Value = 10; ;
            NumericUpDownTabSize.Value = 8;
        }

        private void ButtonDefaultEditorProperties_Click(object sender, EventArgs e)
        {
            checkBoxShowMatchingBracket.Checked = true;
            checkBoxShowVRuler.Checked = false;
            checkBoxMarkCaretLine.Checked = false;
            checkBoxSaveBookmarks.Checked = true;
        }

        private void ButtonDefaultCSoundStyles_Click(object sender, EventArgs e)
        {
            wxGlobal.Settings.CreateDefaultCSoundStyles();
            CreateSyntaxPanelItems(PanelCSoundColors,
                                   wxGlobal.Settings.EditorProperties.CSoundStyles);
        }

        private void ButtonDefaultPythonStyles_Click(object sender, EventArgs e)
        {
            wxGlobal.Settings.CreateDefaultPythonStyles();
            CreateSyntaxPanelItems(PanelPythonColors,
                                   wxGlobal.Settings.EditorProperties.PythonStyles);
        }

        private void ButtonDefaultLuaStyles_Click(object sender, EventArgs e)
        {
            wxGlobal.Settings.CreateDefaultLuaStyles();
            CreateSyntaxPanelItems(PanelLuaColors,
                                   wxGlobal.Settings.EditorProperties.LuaStyles);
        }

        private void ButtonLuaDefaultArguments_Click(object sender, EventArgs e)
        {
            TextBoxLuaFlags.Text = "";
        }

        private void ButtonBrowseLuaConsole_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Lua exe (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    return;
                }

                LabelLuaConsole.Text =
                    openFileDialog1.FileName;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowsePythonConsole_Click");
            }
        }

        private void ButtonBrowseLuaGUI_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Lua GUI exe (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    return;
                }

                //wxGlobal.Settings.Directory.PythonConsolePath =
                //    openFileDialog1.FileName;
                LabelLuaGUI.Text =
                    openFileDialog1.FileName;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowsePythonConsole_Click");
            }
        }

        private void buttonDefaultTemplates_Click(object sender, EventArgs e)
        {
            wxGlobal.Settings.CreateDefaultTemplates();

            RtbTemplateCSound.Text = "";
            RtbTemplateCSound.AppendText(wxGlobal.Settings.Templates.CSound);

            RtbTemplatePython.Text = "";
            RtbTemplatePython.AppendText(wxGlobal.Settings.Templates.Python);

            RtbTemplateLua.Text = "";
            RtbTemplateLua.AppendText(wxGlobal.Settings.Templates.Lua);

            RtbTemplateCabbage.Text = "";
            RtbTemplateCabbage.AppendText(wxGlobal.Settings.Templates.Cabbage);
        }

        private void ButtonBrowseCabbageExe_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //    openFileDialog1.Filter = "Cabbage exe (*.exe)|*.exe|All files (*.*)|*.*";
            //    openFileDialog1.FilterIndex = 1;
            //    openFileDialog1.RestoreDirectory = true;

            //    if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
            //        string.IsNullOrEmpty(openFileDialog1.FileName))
            //    {
            //        return;
            //    }

            //    LabelCabbageExe.Text =
            //        openFileDialog1.FileName;
            //}
            //catch (Exception ex)
            //{
            //    wxGlobal.wxMessageError(ex.Message, "Form Settings - ButtonBrowseCabbageExe_Click");
            //}

        }





        ///////////////////////////////////////////////////////////////////////////////////////////
        // ALTERNATIVE CSOUND FLAGS
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void ButtonDefaultAlternativeCSoundFlags_Click(object sender, EventArgs e)
        {
            TextBoxAlternativeCSoundFlags.Text = 
                wxGlobal.Settings.GetAdditionalCSoundFlags();

            TextBoxAlternativeCSoundFlags.Focus();
        }





    }
}
