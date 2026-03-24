using ScintillaTextEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace WinXound_Net
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private string mUntitled = "Untitled";
        private string mImported = "Imported: ";
        private string mWinXound_Untitled = "WinXound_Untitled";

        private bool mWorkingDir = false;
        private bool mWinXoundIsReadOnly = false;
        private string newline = System.Environment.NewLine;


        //private wxSettings wxGlobal.Settings = new wxSettings();
        private wxCompiler mCompiler = new wxCompiler();
        private Hashtable Opcodes = new Hashtable();

        FormFindAndReplace mFormFindAndReplace = new FormFindAndReplace();
        FormFindLine mFindLine = new FormFindLine();
        FormPlayer mPlayer = new FormPlayer();

        private wxEditor mCompilerEditor = null;
        Form compilerWindow = null;

        private Int32 oldLine = 0;
        private Int32 curLine = 0;


        ///////////////////////////////////////////////////////////////////////////////
        //FORM LOAD - STARTUP METHOD
        ///////////////////////////////////////////////////////////////////////////////
        private void FormMain_Load(object sender, EventArgs e)
        {

            this.Hide();

            //////////////////////////////////////////////////////////////////////////////
            //Check if WinXound.exe is correctly located in a 
            //Full Read-Write Access folder (aka NON-ADMINISTRATIVE folder)
            try
            {
                FileAccessRights rights =
                    new FileAccessRights(Application.ExecutablePath);

                if (!rights.canWrite())
                {
                    mWinXoundIsReadOnly = true;
                    throw new UnauthorizedAccessException("Unauthorized Access Exception");
                }

            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show(
                    uae.Message + newline + newline +
                    "It seems that you have saved WinXound inside a " + newline +
                    "path with a restricted access for your account!" + newline +
                    "In this operating system files stored in 'Program Files' path and" + newline +
                    "in your root 'C' drive are not writable by default for" + newline +
                    "non-administrative programs." + newline +
                    "Please locate WinXound folder into a path where you have" + newline +
                    "full Read and Write permissions, " + newline +
                    "for example in your User personal folder." + newline + newline +
                    "WinXound will now work in Read-Only mode (but it may be unstable).",
                    "WinXound Folder Unauthorized Access",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                mWinXoundIsReadOnly = true;
            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message,
                System.Diagnostics.Debug.WriteLine(
                    "FormMain Load - AccessControl.FileSecurity Error");
            }
            ///////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////


            //Set main title
            this.Text = wxGlobal.TITLE;

            //Load WinXound Settings (WinXoundSettings.txt)
            wxGlobal.Settings.LoadSettings(mWinXoundIsReadOnly);


            //Set Window position and size 
            try
            {
                if (wxGlobal.Settings.General.FirstStart == false)
                {
                    if ((int)(wxGlobal.Settings.General.WindowState) >= 0 &&
                        (int)(wxGlobal.Settings.General.WindowState) < 3)
                    {
                        this.WindowState = wxGlobal.Settings.General.WindowState;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                        wxGlobal.Settings.General.WindowState = FormWindowState.Normal;
                    }
                    if (this.WindowState != FormWindowState.Maximized)
                    {
                        Rectangle mScreen = Screen.GetWorkingArea(this.Location);
                        if (mScreen.Contains(wxGlobal.Settings.General.WindowPosition))
                        {
                            this.Location = wxGlobal.Settings.General.WindowPosition;
                        }
                        else
                        {
                            this.Location = new Point(0, 0);
                        }
                        if (wxGlobal.Settings.General.WindowSize.X < 320 &&
                           wxGlobal.Settings.General.WindowSize.Y < 200)
                        {
                            wxGlobal.Settings.General.WindowSize.X = 800;
                            wxGlobal.Settings.General.WindowSize.Y = 600;
                        }
                        this.Width = wxGlobal.Settings.General.WindowSize.X;
                        this.Height = wxGlobal.Settings.General.WindowSize.Y;
                    }
                }
                else //FIRST START
                {
                    //Check current screen dimensions
                    Screen scrn = Screen.FromControl(this);
                    if (scrn.WorkingArea.Width >= 900 &&
                       scrn.WorkingArea.Height >= 700)
                    {
                        this.WindowState = FormWindowState.Normal;
                        this.Size = new Size(900, 700);
                        this.CenterToScreen();
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormMain Load - Set Window position Error");
            }


            //Configure Toolbar and menu settings
            ConfigureToolBar();

            //Fill Listbox2 with Udo and other personal code
            wxCodeRepository1.UpdateUdoList();
            wxCodeRepository1.ShowFirstUDO();

            //Set UseCSoundDefaultFlags
            MenuToolsUseDefaultFlags.Checked =
                    wxGlobal.Settings.General.UseWinXoundFlags;
            MenuToolsUseDefaultFlags_CheckedChanged(this, null);

            //Set Menu and Toolbar states (enabled or disabled)
            CheckMenuConditions();
            ToolStripCSoundConsole.ToolTipText =
                "Compile\n" +
                "(Ctrl+Click to Compile in Terminal)\n" +
                "(Ctrl+Alt+L to show the Additioal Flags List";

            //Show FormMain to user
            this.Show();



            //OPCODES DATABASE LOAD!!!
            //Load opcodes (from "opcodes.tzt" file) into the Opcodes_Hashtable
            //and at the same time fill the CSoundOpcodesRepository TreeView
            ////Opcodes = wxGlobal.LoadOpcodes(); //OLD Method
            ////wxCSoundOpcodesRepository1.SetOpcodes(Opcodes); //OLD Method
            Opcodes = wxCSoundOpcodesRepository1.FillTreeViewAndReturnOpcodes();
            this.Refresh();



            ////Set UseSFDIR
            //wxGlobal.Settings.Directory.UseSFDIR = true;

            //Set Working Directory
            if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WorkingDir))
            {
                mWorkingDir = true;
            }

            //CSound Default Compiler Flags
            if (string.IsNullOrEmpty(wxGlobal.Settings.General.CSoundDefaultFlags))
            {
                wxGlobal.Settings.General.CSoundDefaultFlags =
                    "-B4096 --displays --asciidisplay";
            }


            //Fill FileMenu with the recent files list
            try
            {
                MenuFileRecentFiles.DropDownItems.Clear();
                for (Int32 i = 0; i < wxGlobal.Settings.RecentFiles.Count; i++)
                {
                    MenuFileRecentFiles.DropDownItems.Add(
                        "&" + Convert.ToString(i + 1) +
                        " " + wxGlobal.Settings.RecentFiles[i]);
                }
                foreach (ToolStripMenuItem tsmi in MenuFileRecentFiles.DropDownItems)
                {
                    tsmi.Click += new EventHandler(MenuRecentFile_Click);
                }
                if (MenuFileRecentFiles.DropDownItems.Count > 0)
                    MenuFileRecentFiles.Enabled = true;
                else MenuFileRecentFiles.Enabled = false;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormMain Load - FileMenu LastFiles Error");
            }


            //Set Default font size on Menu (checked) 
            try
            {
                for (Int32 Ciclo = 0; Ciclo < MenuFontSize.DropDown.Items.Count; Ciclo++)
                {
                    ToolStripMenuItem mMenuFontSize =
                        (ToolStripMenuItem)MenuFontSize.DropDown.Items[Ciclo];
                    ToolStripMenuItem mToolStripFontSize =
                        (ToolStripMenuItem)ToolStripButtonFontSize.DropDown.Items[Ciclo];
                    if (Convert.ToInt32(MenuFontSize.DropDown.Items[Ciclo].Text) ==
                                        wxGlobal.Settings.EditorProperties.DefaultFontSize)
                    {
                        mMenuFontSize.Checked = true;
                        mToolStripFontSize.Checked = true;
                    }
                    else
                    {
                        mMenuFontSize.Checked = false;
                        mToolStripFontSize.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormMain Load - Menu Font Size Error");
            }

            editor_FontHasChanged(null, null);


            //Add events for the compiler console
            wxCompilerConsole1.OnCompilerCompleted +=
                new wxCompilerConsole.CompilerCompleted(wxCompilerConsole1_CompilerCompleted);
            wxCompilerConsole1.ButtonFindError.Click +=
                new EventHandler(ButtonFindError_Click);

            //Add other events
            wxTabCode.GotFocus += new EventHandler(tab_Enter);
            AddEventForFormFindAndReplace();
            mFindLine.ButtonFindClick +=
                new FormFindLine.OnButtonFind(mFindLine_ButtonFindClick);


            //Set Compiler font
            try
            {
                wxCompilerConsole1.SetCompilerFont(
                            new Font(wxGlobal.Settings.General.CompilerFontName,
                                     wxGlobal.Settings.General.CompilerFontSize));
                wxCompilerConsole1.ShowLineNumbers = false;
                wxCompilerConsole1.ReadOnly = true;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormMain Load - SetCompilerFont Error");

                wxGlobal.Settings.General.CompilerFontName = "Courier New";
                wxGlobal.Settings.General.CompilerFontSize = 10;
                wxCompilerConsole1.SetCompilerFont(
                    new Font(wxGlobal.Settings.General.CompilerFontName,
                             wxGlobal.Settings.General.CompilerFontSize));
            }


            //Hide wxCompilerConsole1.ButtonFindError
            wxCompilerConsole1.ButtonFindError.Visible = false;

            //Set opcodes for Code Repository
            wxCodeRepository1.ConfigureEditor(Opcodes);

            //Set environment variables
            wxGlobal.Settings.SetEnvironmentVariables();



            //Add events for HTML window buttons
            ////Load WinXound HTML Help - SlowDown startup process!!!
            //if (File.Exists(Application.StartupPath + "\\Help\\winxound_help.html"))
            //{
            //    HelpBrowser.Navigate(Application.StartupPath + "\\Help\\winxound_help.html");
            //}
            HelpBrowser.CanGoBackChanged +=
                new EventHandler(HelpBrowser_CanGoBackChanged);
            HelpBrowser.CanGoForwardChanged +=
                new EventHandler(HelpBrowser_CanGoForwardChanged);
            HelpBrowser.Navigating +=
                new WebBrowserNavigatingEventHandler(HelpBrowser_Navigating);



            //Create Autocompletion list
            ArrayList al = new ArrayList();
            foreach (DictionaryEntry de in Opcodes)
            {
                al.Add(de.Key);
            }
            al.Sort();
            ListBoxAutoComplete.Items.AddRange(al.ToArray());
            ListBoxAutoComplete.VisibleChanged += new EventHandler(ListBoxAutoComplete_VisibleChanged);



            //TODO:CABBAGE - For the moment we disable all the Cabbage tools
            //MenuToolsCabbage.Visible = false;
            //ToolStripCabbage.Visible = false;



            //Create the compilerWindow (to detach the compiler tabs)
            CreateCompilerWindow();


            //Check for First Start 
            if (wxGlobal.Settings.General.FirstStart)
            {
                //this.FirstStart();
                FirstStartTimer.Start();
            }




            ///////////////////////////////////
            //Finally check for Files to open
            //Check for command line arguments 
            try
            {
                string[] args = System.Environment.GetCommandLineArgs();
                if (args != null)
                {
                    if (args.Length > 1)
                    {
                        for (Int32 i = 1; i < args.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(args[i]))
                            {
                                AddNewEditor(args[i]);
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormMain Load - Command line arguments Loading Error");
            }

            ////Prepare a template document if a StartupAction is defined
            //MenuFileNew.PerformClick();
            if (wxGlobal.Settings.General.StartupAction == 1)
                AddNewEditor(mUntitled + ".csd");
            else if (wxGlobal.Settings.General.StartupAction == 2)
                AddNewEditor(mUntitled + ".py");
            else if (wxGlobal.Settings.General.StartupAction == 3)
                AddNewEditor(mUntitled + ".lua");
            else if (wxGlobal.Settings.General.StartupAction == 4)
                OpenLastSessionFiles();


        }
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////







        void HelpBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.AbsolutePath.ToLower().EndsWith(".csd") ||
                    e.Url.AbsolutePath.ToLower().EndsWith(".orc") ||
                    e.Url.AbsolutePath.ToLower().EndsWith(".sco"))
                {
                    e.Cancel = true;
                    //System.Diagnostics.Debug.WriteLine("HelpBrowser_Navigating: " + e.Url.ToString());
                    string filename = e.Url.ToString().Replace("file:///", "");
                    if (File.Exists(filename))
                    {
                        AddNewEditor(filename);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - HelpBrowser_Navigating: " +
                    ex.Message);
            }
        }


        private void AddRecentFilesToMenu(string filename)
        {
            if (MenuFileRecentFiles.DropDownItems.Count > 0)
            {
                foreach (ToolStripMenuItem tsmi in MenuFileRecentFiles.DropDownItems)
                {
                    tsmi.Click -= new EventHandler(MenuRecentFile_Click);
                }
            }

            wxGlobal.Settings.RecentFilesInsert(filename);

            try
            {
                MenuFileRecentFiles.DropDownItems.Clear();
                for (Int32 i = 0; i < wxGlobal.Settings.RecentFiles.Count; i++)
                {
                    MenuFileRecentFiles.DropDownItems.Add(
                        "&" + Convert.ToString(i + 1) +
                        " " + wxGlobal.Settings.RecentFiles[i]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - AddRecentFilesToMenu: " +
                    ex.Message);
            }

            foreach (ToolStripMenuItem tsmi in MenuFileRecentFiles.DropDownItems)
            {
                tsmi.Click += new EventHandler(MenuRecentFile_Click);
            }

            if (MenuFileRecentFiles.DropDownItems.Count > 0)
                MenuFileRecentFiles.Enabled = true;
        }



        private void AddEventForFormFindAndReplace()
        {
            mFormFindAndReplace.ButtonFindClick +=
                new FormFindAndReplace.OnButtonFind(mFormFindAndReplace_ButtonFindClick);
            mFormFindAndReplace.ButtonReplaceClick +=
                new FormFindAndReplace.OnButtonReplace(mFormFindAndReplace_ButtonReplaceClick);
            //mFormFindAndReplace.FormHidden +=
            //    new EventHandler(mFormFindAndReplace_FormHidden);
        }

        //void mFormFindAndReplace_FormHidden(object sender, EventArgs e)
        //{
        //    this.Focus();
        //}

        private void RemoveEventForFormFindAndReplace()
        {
            mFormFindAndReplace.ButtonFindClick -=
                new FormFindAndReplace.OnButtonFind(mFormFindAndReplace_ButtonFindClick);
            mFormFindAndReplace.ButtonReplaceClick -=
                new FormFindAndReplace.OnButtonReplace(mFormFindAndReplace_ButtonReplaceClick);
            //mFormFindAndReplace.FormHidden -=
            //    new EventHandler(mFormFindAndReplace_FormHidden);
        }



        void mFormFindAndReplace_ButtonFindClick(string StringToFind,
                                                  bool MatchWholeWord,
                                                  bool MatchCase,
                                                  bool IsBackward)
        {
            if (ActiveEditor == null) return;
            ActiveEditor.textEditor.FindText(StringToFind, MatchWholeWord, MatchCase,
                                             IsBackward, true, true, false);
        }


        void mFormFindAndReplace_ButtonReplaceClick(string StringToFind,
                                                    string ReplaceString,
                                                    bool MatchWholeWord,
                                                    bool MatchCase,
                                                    bool FromCaretPosition,
                                                    bool FCPUp, bool ReplaceAll)
        {
            if (ActiveEditor == null) return;
            if (ReplaceAll)
            {
                ActiveEditor.textEditor.ReplaceAllText(
                    StringToFind, ReplaceString, MatchWholeWord,
                    MatchCase, FromCaretPosition, FCPUp);
            }
            else
            {
                ActiveEditor.textEditor.ReplaceText(ReplaceString);
            }
        }


        private void ConfigureToolBar()
        {
            try
            {
                //Configure Toolbar
                Int32 toolIndex = 0;
                if (wxGlobal.Settings.General.ToolBarItems == null ||
                    wxGlobal.Settings.General.ToolBarItems.Count == 0 ||
                    wxGlobal.Settings.General.ToolBarItems.Count != ToolStrip1.Items.Count)
                {
                    wxGlobal.Settings.General.ToolBarItems = new List<bool>();
                    for (toolIndex = 0; toolIndex < ToolStrip1.Items.Count; toolIndex++)
                    {
                        wxGlobal.Settings.General.ToolBarItems.Add(true);
                    }
                }

                toolIndex = 0;
                foreach (bool b in wxGlobal.Settings.General.ToolBarItems)
                {
                    ToolStrip1.Items[toolIndex].Visible = b;
                    toolIndex++;
                }

                //TODO:CABBAGE - FOR THE MOMENT WE HIDE THE CABBAGE ICON
                //ToolStripCabbage.Visible = false;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - ConfigureToolBar");
            }
        }


        private wxEditor ActiveEditor
        {
            get
            {
                try
                {
                    if (wxTabCode.TabPages.Count == 0) return null;
                    //return wxTabCode.SelectedTab.Controls.OfType<TextEditorControl>().FirstOrDefault();
                    return (wxEditor)wxTabCode.SelectedTab.Controls[0];
                }
                catch { return null; }
            }
        }




        private void AddNewEditor(string filename) //void or wxEditor?
        {
            AddNewEditor(filename, "", false);
        }
        private void AddNewEditor(string filename, string linkFile) //void or wxEditor?
        {
            AddNewEditor(filename, linkFile, false);
        }
        private void AddNewEditor(string filename, string linkFile, bool importOperation)
        {

            bool IsNewUntitledFile = false;

            if (filename == mUntitled + ".csd" ||
                filename == mUntitled + ".orc" ||
                filename == mUntitled + ".sco" ||
                filename == mUntitled + ".py" ||
                filename == mUntitled + ".lua" ||
                filename == mUntitled + ".cabbage")
            {
                IsNewUntitledFile = true;
            }


            //1.
            //Check if the loaded file is already open in another tab
            try
            {
                if (IsNewUntitledFile == false)
                {
                    wxEditor tempEditor = null;
                    if (wxTabCode.TabPages.Count > 0)
                    {
                        foreach (TabPage tp in wxTabCode.TabPages)
                        {
                            tempEditor = (wxEditor)tp.Controls[0];
                            if (tempEditor.FileName == filename &&
                                importOperation == false)
                            {
                                //Select the tab
                                wxTabCode.SelectedTab = tp;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - AddNewEditor - Check File Already Open: " +
                    ex.Message);
            }


            //2.
            //Check Find&Replace window status
            try
            {
                if (mFormFindAndReplace != null)
                {
                    if (mFormFindAndReplace.Visible)
                    {
                        ////RemoveEventForFormFindAndReplace();
                        mFormFindAndReplace.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - AddNewEditor - Check Find&Replace window status: " +
                    ex.Message);
            }


            //3.
            //Create new Tab and wxEditor control
            TabPage tab = new TabPage(filename);
            wxEditor editor = new wxEditor();

            if (tab == null || editor == null)
            {
                MessageBox.Show(
                    "FormMain->AddNewEditor: Unable to create editor!",
                    "WinXound - Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }


            //4.
            //Set editor dockstyle
            editor.Dock = System.Windows.Forms.DockStyle.Fill;


            //5.
            //Check File IO Permissions
            try
            {
                if (IsNewUntitledFile == false && File.Exists(filename))
                {
                    FileAccessRights rights = new FileAccessRights(filename);
                    if (!rights.canWrite())
                    {
                        editor.FileIsReadOnly = true;

                        if (wxGlobal.Settings.General.ShowReadOnlyFileMessage)
                        {
                            MessageBox.Show(
                                "Your current user account allows read-only access to this file." + newline +
                                "Generally files stored in 'Program Files' path or in " +
                                "your root 'C' drive are not writable by default for " +
                                "non-administrative programs." + newline +
                                "If you want to modify this file please save it " +
                                "to a folder where you have full read and write permissions." + newline + newline +
                                "WinXound will try to load the file in Read-Only mode." + newline + newline +
                                "To disable this warning message uncheck " + newline +
                                "'Show Read-Only File Alert'" + newline +
                                "in your WinXound Settings (menu File->Settings->General tab).",
                                "WinXound - ReadOnly File Access",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - AddNewEditor - Check File IO Permissions: " +
                    ex.Message);
            }


            //6.
            //Add wxEditor and tab Events
            editor.TextEditorKeyAction +=
                new System.Windows.Forms.KeyEventHandler(editor_TextEditorKeyAction);
            editor.CaretPositionChanged +=
                new EventHandler(editor_CaretPositionChanged);
            editor.TextEditorMouseDown +=
                new MouseEventHandler(editor_TextEditorMouseDown);
            editor.FileDropped +=
                new wxEditor.OnFileDragDrop(editor_FileDropped);
            editor.FontHasChanged +=
                new EventHandler(editor_FontHasChanged);
            editor.SavePointReached +=
                new EventHandler(editor_SavePointReached);
            editor.SavePointLeft +=
                new EventHandler(editor_SavePointLeft);
            editor.TextHasChanged +=
                new ScintillaTextEditor.TextView.OnSCI_Modified(editor_TextHasChanged);
            editor.textEditor.TextEditorModContainer +=
                new TextEditor.OnSciModContainer(textEditor_TextEditorModContainer);
            editor.OrcScoSwitchRequest +=
                new EventHandler(editor_OrcScoSwitchRequest);
            editor.OrcScoShowList +=
                new EventHandler(editor_OrcScoShowList);

            //Autocomplete
            editor.textEditor.PrimaryView.SCI_AutoComplete +=
                new TextView.OnAutocomplete(TextEditor_SCI_AutoComplete);
            editor.textEditor.SecondaryView.SCI_AutoComplete +=
                new TextView.OnAutocomplete(TextEditor_SCI_AutoComplete);

            //New EventHandler on the tab Enter event (set focus to TextEditor)
            tab.Enter += new EventHandler(tab_Enter);
            tab.GotFocus += new EventHandler(tab_Enter);


            //7.
            //Set TextEditor Preferences
            editor.textEditor.ShowSpaces = false; //inserted in MenuEditView
            editor.textEditor.ShowEOLMarker = false; //inserted in MenuEditView
            editor.textEditor.AllowCaretBeyondEOL = false; //TODO: maybe true! or insert in Settings
            editor.textEditor.ShowMatchingBracket = wxGlobal.Settings.EditorProperties.ShowMatchingBracket;
            editor.textEditor.ShowVerticalRuler = wxGlobal.Settings.EditorProperties.ShowVerticalRuler;
            editor.textEditor.MarkCaretLine = wxGlobal.Settings.EditorProperties.MarkCaretLine;
            editor.textEditor.ShowLineNumbers = wxGlobal.Settings.EditorProperties.ShowLineNumbers;
            editor.textEditor.TabIndent = wxGlobal.Settings.EditorProperties.DefaultTabSize;
            editor.textEditor.AllowDrop = true;
            editor.ShowExplorer = wxGlobal.Settings.EditorProperties.ShowExplorer;
            editor.ShowIntelliTip = wxGlobal.Settings.EditorProperties.ShowIntelliTip;
            editor.textEditor.SetTextFont(new Font(wxGlobal.Settings.EditorProperties.DefaultFontName,
                                          wxGlobal.Settings.EditorProperties.DefaultFontSize));
            editor.FileName = filename;
            editor.ShowOrcScoPanel = false;

            //8.
            //Load file(s) or create a new one
            //bool IsImported = false;
            //NEW FILES
            if (filename == mUntitled + ".csd")
            {
                //New CSound Document 
                editor.textEditor.SetText(wxGlobal.Settings.Templates.CSound);
            }
            else if (filename == mUntitled + ".orc" ||
                     filename == mUntitled + ".sco")
            {
                //New CSound Document 
                // editor.textEditor.SetText(wxGlobal.Settings.Templates.CSound);
            }
            else if (filename == mUntitled + ".py")
            {
                //New Python empty document
                editor.textEditor.SetText(wxGlobal.Settings.Templates.Python);
            }
            else if (filename == mUntitled + ".lua")
            {
                //New Lua empty document
                editor.textEditor.SetText(wxGlobal.Settings.Templates.Lua);
            }
            else if (filename == mUntitled + ".cabbage")
            {
                //New Lua empty document
                editor.textEditor.SetText(wxGlobal.Settings.Templates.Cabbage);
                filename = filename.Replace(".cabbage", ".csd");
                editor.FileName = filename;
            }
            //IMPORT FILES
            else if (filename.ToLower().EndsWith("orc") ||
                     filename.ToLower().EndsWith("sco") &&
                     IsNewUntitledFile == false)
            {
                bool OpenSeparately = false;

                if (wxGlobal.Settings.General.OrcScoImport == 2)        //Open separately
                {
                    OpenSeparately = true;
                }
                else if (wxGlobal.Settings.General.OrcScoImport == 1 || //Import to CSD
                         wxGlobal.Settings.General.OrcScoImport == 0)   //Ask always
                {
                    bool OkToImport = true;

                    if (wxGlobal.Settings.General.OrcScoImport == 0 &&
                        string.IsNullOrEmpty(linkFile) &&
                        importOperation == false) //Ask always
                    {
                        DialogResult ret = MessageBox.Show(
                                "Would you like to convert your Orc/Sco file to a new Csd document?" + newline + newline +
                                "To change the default Orc/Sco import action" + newline +
                                "please look at:" + newline +
                                "'File->Settings->General->Orc/Sco Import field'",
                                "WinXound - Orc/Sco Import",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                        if (ret != DialogResult.Yes) OkToImport = false;
                    }

                    if (OkToImport && string.IsNullOrEmpty(linkFile))
                        OpenSeparately = false;
                    else
                        OpenSeparately = true;
                }

                if (OpenSeparately == true &&
                    importOperation == false) //Open separately
                {
                    //Show the OrcSco Panel in the editor and load the file
                    editor.ShowOrcScoPanel = true;
                    editor.LoadFile(filename);
                    if (string.IsNullOrEmpty(linkFile))
                        LinkOrcScoFiles(editor);
                    else
                        editor.SetCurrentOrcScoFile(linkFile);
                }
                else
                {
                    //Import Orc and Sco
                    wxImportExport mImport =
                        new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                    editor.textEditor.SetText(mImport.ImportORCSCO(ref filename));
                    editor.FileName = mImported + filename;
                    //IsImported = true;
                    IsNewUntitledFile = true;

                }

            }
            //LOAD OTHER FILES
            else
            {
                //All other Files 
                editor.LoadFile(filename);
            }


            //9. If this is a new untitled file we must set and check the default line endings
            if (IsNewUntitledFile)
            {
                //NEW FILES
                //Convert (templates) and Set line endings to CRLF (Windows default)
                editor.textEditor.ConvertEOL((int)ScintillaTextEditor.SciConst.SC_EOL_CRLF);
                editor.textEditor.SetEolMode((int)ScintillaTextEditor.SciConst.SC_EOL_CRLF);
            }
            //Check Line Endings for loaded file (to get Eols and to check Eols coherence)
            else
            {
                //LOADED FILES
                CheckLineEndings(editor);
            }


            //10.
            //if (IsImported == false)
            if (IsNewUntitledFile == false)
            {
                tab.Text = Path.GetFileName(filename);
                AddRecentFilesToMenu(filename);

                //Try to load bookmarks
                LoadBookmarks(editor);
            }
            else
            {
                tab.Text = Path.GetFileName(filename);
            }


            //11.
            //Set Scintilla Save Point and Empty the Undo Buffer
            editor.textEditor.SetSavePoint();
            editor.textEditor.EmptyUndoBuffer();


            //12.
            //Check for ReadOnly files (in order to add a notification to Tab Title)
            if (editor.FileIsReadOnly) tab.Text += " (Read Only)";


            //13.
            //Set Default Highlight
            SetHighlightLanguage(editor, false);
            editor.RefreshTextEditor();


            //14. Finally add all controls to the main TabControl
            //Add Tab and wxEditor control
            tab.Controls.Add(editor);
            wxTabCode.Controls.Add(tab);


            //Set focus and selected tab
            editor.SetFocus();
            wxTabCode.SelectedTab = tab;

            //Set Undo and Redo conditions (+ Title)
            CheckMenuConditions();

            //Reset the focus ???
            this.Focus();
            editor.SetFocus();

            ////??? really needed ???
            //editor.textEditor.ShowLineNumbers =
            //    wxGlobal.Settings.EditorProperties.ShowLineNumbers;


            //Start Explorer Structure Timer
            editor.StartExplorerTimer();


        }


        ////ORCSCO FILES STUFFS:
        void LinkOrcScoFiles(wxEditor editor)
        {
            //Look for OrcSco links
            string file = Path.GetFileNameWithoutExtension(editor.FileName);
            foreach (string s in LookForOrcScoFiles(editor))
            {
                if (s.Contains(file))
                {
                    editor.SetCurrentOrcScoFile(s.Trim());
                    break;
                }
            }
        }

        void editor_OrcScoSwitchRequest(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            string filename = ActiveEditor.GetCurrentOrcScoFile();
            if (File.Exists(filename))
                AddNewEditor(filename, ActiveEditor.FileName);
        }

        void editor_OrcScoShowList(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            FormOrcScoLinks fosl = new FormOrcScoLinks();
            fosl.ListBoxOrcSco.Items.Clear();
            fosl.ListBoxOrcSco.Items.AddRange(LookForOrcScoFiles(ActiveEditor));


            if (fosl.ListBoxOrcSco.Items.Contains(ActiveEditor.GetCurrentOrcScoFile()))
                fosl.ListBoxOrcSco.SelectedItem = ActiveEditor.GetCurrentOrcScoFile();
            else
                fosl.ListBoxOrcSco.SelectedIndex = 0;


            DialogResult ret = fosl.ShowDialog(this);
            if (ret == DialogResult.OK)
                ActiveEditor.SetCurrentOrcScoFile(fosl.RetValue);

            fosl.Dispose();

        }

        public string[] LookForOrcScoFiles(wxEditor editor)
        {
            if (editor == null) return new string[] { "" };

            if (!editor.FileName.ToLower().EndsWith(".orc") &&
                !editor.FileName.ToLower().EndsWith(".sco"))
                return new string[] { "" };

            string mPredicate =
                editor.FileName.ToLower().EndsWith(".orc") ? "*.sco" : "*.orc";
            string mPath = Path.GetDirectoryName(editor.FileName);


            List<string> OrcScoFilesList = new List<string>();

            if (!string.IsNullOrEmpty(editor.GetCurrentOrcScoFile()))
                OrcScoFilesList.Add(editor.GetCurrentOrcScoFile());

            //Look in the current file directory
            foreach (string s in Directory.GetFiles(mPath, mPredicate))
            {
                //System.Diagnostics.Debug.WriteLine(s);
                if (!OrcScoFilesList.Contains(s))
                    OrcScoFilesList.Add(s);
            }

            //if (OrcScoFilesList.Count > 0)
            //    OrcScoFilesList.Add("---------");

            //Look in the current opened files
            string extension = mPredicate.TrimStart("*".ToCharArray());
            foreach (TabPage tp in wxTabCode.TabPages)
            {
                wxEditor temp = (wxEditor)tp.Controls[0];
                if (!String.IsNullOrEmpty(temp.FileName))
                {
                    if (temp.FileName.ToLower().EndsWith(extension) &&
                        !OrcScoFilesList.Contains(temp.FileName))
                        OrcScoFilesList.Add(temp.FileName);
                }
            }

            return OrcScoFilesList.ToArray();
        }



        /////////////////////////////////////////////////////////////////
        //CheckLineEndings: check for line endings and for eols coherence
        private void CheckLineEndings(wxEditor editor)
        {
            if (editor == null) return;

            //Check Line Endings
            //SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)

            bool FileIsNotConsistent = false;

            string s = editor.textEditor.GetText().Replace("\r\n", "");

            Int32 crlfOccurrences = (editor.textEditor.GetTextLength() - s.Length) / 2;
            Int32 crOccurrences = 0;
            Int32 lfOccurrences = 0;

            //Check if CRLF correspond to TextEditor lines - 1
            //If not check also for LF
            if (crlfOccurrences != editor.textEditor.GetLines() - 1)
            {
                //Check if LF correspond to TextEditor lines - 1
                //If not check also for CR
                lfOccurrences = s.Length - s.Replace("\n", "").Length;
                if (lfOccurrences != editor.textEditor.GetLines() - 1)
                {
                    crOccurrences = s.Length - s.Replace("\r", "").Length;
                }
            }


            //string report =
            //    "CRLF:\t" + crlfOccurrences + newline +
            //    "CR:\t" + crOccurrences + newline +
            //    "LF:\t" + lfOccurrences;


            string report =
                "Filename: " + Path.GetFileName(editor.FileName) + newline +
                (crlfOccurrences > 0 ? "CRLF:\t" + crlfOccurrences + newline : "") +
                (crOccurrences > 0 ? "CR:\t" + crOccurrences + newline : "") +
                (lfOccurrences > 0 ? "LF:\t" + lfOccurrences : "");


            System.Diagnostics.Debug.WriteLine(report);


            //CRLF
            //if (editor.textEditor.GetText().Contains("\r\n"))
            if (crlfOccurrences > crOccurrences &&
                crlfOccurrences > lfOccurrences)
            {
                //Set EOL mode to CRLF
                editor.textEditor.SetEolMode((int)SciConst.SC_EOL_CRLF);

                //Check for CRLF coherence
                //string crlf = editor.textEditor.GetText().Replace("\r\n", "");
                //if (crlf.Contains("\r") || crlf.Contains("\n"))
                if (crOccurrences > 0 || lfOccurrences > 0)
                {
                    //Incoherent EOLS!!! Convert all to CRLF
                    //DialogResult ret = ShowEolAlertMessage("CRLF", report);

                    //if (ret == DialogResult.Yes)
                    editor.textEditor.ConvertEOL((int)SciConst.SC_EOL_CRLF);
                }
            }

            //CR
            //else if (editor.textEditor.GetText().Contains("\r"))
            else if (crOccurrences > crlfOccurrences &&
                     crOccurrences > lfOccurrences)
            {
                //Set EOL mode to CR
                editor.textEditor.SetEolMode((int)SciConst.SC_EOL_CR);

                //Check for CR coherence
                //string cr = editor.textEditor.GetText().Replace("\r", "");
                //if (cr.Contains("\r\n") || cr.Contains("\n"))
                if (crlfOccurrences > 0 || lfOccurrences > 0)
                {
                    //Incoherent EOLS!!! Convert all to CR
                    //DialogResult ret = ShowEolAlertMessage("CR", report);

                    //if (ret == DialogResult.Yes)
                    editor.textEditor.ConvertEOL((int)SciConst.SC_EOL_CR);
                }
            }

            //LF
            //else if (editor.textEditor.GetText().Contains("\n"))
            else if (lfOccurrences > crlfOccurrences &&
                     lfOccurrences > crOccurrences)
            {
                //Set EOL mode to LF
                editor.textEditor.SetEolMode((int)SciConst.SC_EOL_LF);

                //Check for LF coherence
                //string lf = editor.textEditor.GetText().Replace("\n", "");
                //if (lf.Contains("\r\n") || lf.Contains("\r"))
                if (crlfOccurrences > 0 || crOccurrences > 0)
                {
                    //Incoherent EOLS!!! Convert all to LF
                    //DialogResult ret = ShowEolAlertMessage("LF", report);

                    //if (ret == DialogResult.Yes)
                    editor.textEditor.ConvertEOL((int)SciConst.SC_EOL_LF);
                }
            }

            else
            {
                //Convert and Set default CRLF mode
                editor.textEditor.ConvertEOL((int)SciConst.SC_EOL_CRLF);
                editor.textEditor.SetEolMode((int)SciConst.SC_EOL_CRLF);
            }

            if (FileIsNotConsistent)
            {
                //Save the file after conversion ???
            }

        }

        private DialogResult ShowEolAlertMessage(string eol, string report)
        {
            return MessageBox.Show(
                       "The line endings in the file are not consistent!" + newline + newline +
                       report + newline + newline +
                       "WinXound will normalize all line endings to " + eol + ".",
                       "WinXound Information",
                       //MessageBoxButtons.YesNo,
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Information);
        }




        private void CheckUndoRedo()
        {
            if (ActiveEditor == null) return;
            ToolStripUndo.Enabled = ActiveEditor.textEditor.CanUndo;
            MenuEditUndo.Enabled = ActiveEditor.textEditor.CanUndo;
            ToolStripRedo.Enabled = ActiveEditor.textEditor.CanRedo;
            MenuEditRedo.Enabled = ActiveEditor.textEditor.CanRedo;
        }

        private void CheckImportExportMenu()
        {
            if (ActiveEditor == null)
            {
                MenuFileImport.Enabled = true;
                MenuFileImportOrcScoToExistingCsd.Enabled = false;
                MenuFileImportOrcToExistingCsd.Enabled = false;
                MenuFileImportScoToExistingCsd.Enabled = false;

                MenuFileExport.Enabled = false;
                MenuFileExportOrcSco.Enabled = false;
                MenuFileExportOrc.Enabled = false;
                MenuFileExportSco.Enabled = false;
                return;
            }

            if (ActiveEditor.FileName.ToLower().EndsWith(".csd"))
            {
                //Import
                MenuFileImport.Enabled = true;
                MenuFileImportOrcScoToExistingCsd.Enabled = true;
                MenuFileImportOrcToExistingCsd.Enabled = true;
                MenuFileImportScoToExistingCsd.Enabled = true;
                //Export
                MenuFileExport.Enabled = true;
                MenuFileExportOrcSco.Enabled = true;
                MenuFileExportOrc.Enabled = true;
                MenuFileExportSco.Enabled = true;
            }
            else
            {
                MenuFileImport.Enabled = true;
                MenuFileImportOrcScoToExistingCsd.Enabled = false;
                MenuFileImportOrcToExistingCsd.Enabled = false;
                MenuFileImportScoToExistingCsd.Enabled = false;

                MenuFileExport.Enabled = false;
                MenuFileExportOrcSco.Enabled = false;
                MenuFileExportOrc.Enabled = false;
                MenuFileExportSco.Enabled = false;
            }
        }

        private void CheckMenuConditions()
        {
            if (wxTabCode.TabPages.Count == 0)
            {
                this.Text = wxGlobal.TITLE;

                ToolStripUndo.Enabled = false;
                MenuEditUndo.Enabled = false;
                ToolStripRedo.Enabled = false;
                MenuEditRedo.Enabled = false;

                //////////////////////////////
                MenuFileSave.Enabled = false;
                ToolStripSave.Enabled = false;
                MenuFileSaveAs.Enabled = false;
                ToolStripSaveAs.Enabled = false;
                MenuFileSaveAll.Enabled = false;
                MenuFileClose.Enabled = false;
                toolStripClose.Enabled = false;
                MenuFileCloseAll.Enabled = false;
                MenuFileImport.Enabled = false;
                MenuFileExport.Enabled = false;
                MenuFilePageSetup.Enabled = false;
                MenuFilePrint.Enabled = false;
                MenuFilePrintPreview.Enabled = false;
                MenuFileInfo.Enabled = false;

                MenuEditCut.Enabled = false;
                MenuEditCopy.Enabled = false;
                MenuEditPaste.Enabled = false;
                MenuEditDelete.Enabled = false;
                MenuEditSelectAll.Enabled = false;

                MenuEditFind.Enabled = false;
                ToolStripFind.Enabled = false;
                MenuEditFindLine.Enabled = false;
                ToolStripFindLine.Enabled = false;

                MenuEditFindBase.Enabled = false;
                FindLastWordPREV.Enabled = false;
                FindLastWordNEXT.Enabled = false;
                FindJumpToSelection.Enabled = false;

                MenuEditShowHideWS.Enabled = false;
                MenuEditShowHideEOL.Enabled = false;
                MenuEditEOLConversion.Enabled = false;
                MenuEolCRLF.Enabled = false;
                MenuEolCR.Enabled = false;
                MenuEolLF.Enabled = false;

                MenuEditCodeFormatting.Enabled = true;
                MenuEditFormatCode.Enabled = false;
                MenuEditFormatAllCode.Enabled = false;
                MenuEditFormatOptions.Enabled = true;

                MenuEditStoreSelectedText.Enabled = false;

                MenuEditComments.Enabled = false;
                MenuEditComment.Enabled = false;
                MenuEditRemoveComment.Enabled = false;

                MenuEditListOpcodes.Enabled = false;

                MenuBookmarks.Enabled = false;
                MenuInsertBookmark.Enabled = false;
                MenuRemoveAllBookmark.Enabled = false;
                MenuInsertGoToNextBookmark.Enabled = false;
                MenuInsertGoToPreviousBookmark.Enabled = false;

                MenuViewPrevious.Enabled = false;
                MenuViewNext.Enabled = false;

                MenuViewFullCode.Enabled = false;
                MenuViewSplitHorizontal.Enabled = false;
                MenuViewSplitHorizontalOrcSco.Enabled = false;
                MenuViewSplitVertical.Enabled = false;
                MenuViewSplitVerticalOrcSco.Enabled = false;
                tsCode.Enabled = false;

                MenuViewFocus1.Enabled = false;
                MenuViewFocus2.Enabled = false;

                MenuFontSize.Enabled = false;
                MenuFontReset.Enabled = false;
                ToolStripCharReset.Enabled = false;
                ToolStripCharDown.Enabled = false;
                ToolStripCharUp.Enabled = false;
                ToolStripButtonFontSize.Enabled = false;

                MenuToolsCompile.Enabled = false;
                MenuToolsCompileWithAdditionalOptions.Enabled = false;
                MenuToolsExternalCompileWithAdditionalOptions.Enabled = false;
                ToolStripCSoundConsole.Enabled = false;
                MenuToolsExternalCompile.Enabled = false;
                MenuToolsRunExternalEditor.Enabled = false;
                ToolStripExternalGUIEditor.Enabled = false;
                MenuToolsRunCSoundAV.Enabled = false;
                ToolStripCSoundAV.Enabled = false;
                //MenuToolsCabbage.Enabled = false;
                //ToolStripCabbage.Enabled = false;
                //MenuToolsRunPythonIdle.Enabled = false;
                //MenuToolsUseDefaultFlags.Enabled = false;

                MenuInsertHeader.Enabled = false;
                MenuItemCsdTags.Enabled = false;
                MenuInsertOptions.Enabled = false;

                CheckImportExportMenu();

                return;
            }

            if (ActiveEditor == null) return;

            this.Text = wxGlobal.TITLE + " - " + ActiveEditor.FileName;
            CheckUndoRedo();

            MenuFileSave.Enabled = ActiveEditor.textEditor.IsTextChanged();
            ToolStripSave.Enabled = ActiveEditor.textEditor.IsTextChanged();
            MenuFileSaveAs.Enabled = true;
            ToolStripSaveAs.Enabled = true;
            MenuFileSaveAll.Enabled = true;
            MenuFileClose.Enabled = true;
            toolStripClose.Enabled = true;
            MenuFileCloseAll.Enabled = true;
            MenuFileExport.Enabled = true;
            MenuFilePageSetup.Enabled = true;
            MenuFilePrint.Enabled = true;
            MenuFilePrintPreview.Enabled = true;
            MenuFileInfo.Enabled = true;

            MenuEditCut.Enabled = true;
            MenuEditCopy.Enabled = true;
            MenuEditPaste.Enabled = true;
            MenuEditDelete.Enabled = true;
            MenuEditSelectAll.Enabled = true;

            MenuEditFind.Enabled = true;
            ToolStripFind.Enabled = true;
            MenuEditFindLine.Enabled = true;
            ToolStripFindLine.Enabled = true;

            MenuEditFindBase.Enabled = true;
            FindLastWordPREV.Enabled = true;
            FindLastWordNEXT.Enabled = true;
            FindJumpToSelection.Enabled = true;

            MenuEditComments.Enabled = true;
            MenuEditComments.Enabled = true;
            MenuEditComment.Enabled = true;
            MenuEditRemoveComment.Enabled = true;

            MenuEditListOpcodes.Enabled = true;

            MenuBookmarks.Enabled = true;
            MenuInsertBookmark.Enabled = true;
            MenuRemoveAllBookmark.Enabled = true;
            MenuInsertGoToNextBookmark.Enabled = true;
            MenuInsertGoToPreviousBookmark.Enabled = true;

            MenuViewPrevious.Enabled = true;
            MenuViewNext.Enabled = true;

            MenuEditShowHideWS.Enabled = true;
            MenuEditShowHideEOL.Enabled = true;
            MenuEditEOLConversion.Enabled = true;
            MenuEolCRLF.Enabled = true;
            MenuEolCR.Enabled = true;
            MenuEolLF.Enabled = true;

            MenuEditCodeFormatting.Enabled = true;
            MenuEditFormatCode.Enabled = true;
            MenuEditFormatAllCode.Enabled = true;
            MenuEditFormatOptions.Enabled = true;

            MenuEditStoreSelectedText.Enabled = true;

            MenuViewFullCode.Enabled = true;
            MenuViewSplitHorizontal.Enabled = true;
            MenuViewSplitHorizontalOrcSco.Enabled = true;
            MenuViewSplitVertical.Enabled = true;
            MenuViewSplitVerticalOrcSco.Enabled = true;
            tsCode.Enabled = true;

            MenuViewFocus1.Enabled = true;
            MenuViewFocus2.Enabled = true;

            MenuFontSize.Enabled = true;
            MenuFontReset.Enabled = true;
            ToolStripCharReset.Enabled = true;
            ToolStripCharDown.Enabled = true;
            ToolStripCharUp.Enabled = true;
            ToolStripButtonFontSize.Enabled = true;

            MenuToolsCompile.Enabled = true;
            MenuToolsCompileWithAdditionalOptions.Enabled = true;
            MenuToolsExternalCompileWithAdditionalOptions.Enabled = true;
            ToolStripCSoundConsole.Enabled = true;
            MenuToolsExternalCompile.Enabled = true;
            MenuToolsRunExternalEditor.Enabled = true;
            ToolStripExternalGUIEditor.Enabled = true;
            MenuToolsRunCSoundAV.Enabled = true;
            ToolStripCSoundAV.Enabled = true;
            //MenuToolsCabbage.Enabled = true;
            //ToolStripCabbage.Enabled = true;
            //MenuToolsRunPythonIdle.Enabled = true;
            //MenuToolsUseDefaultFlags.Enabled = true;

            MenuInsertHeader.Enabled = true;
            MenuItemCsdTags.Enabled = true;
            MenuInsertOptions.Enabled = true;

            CheckImportExportMenu();
        }

        private void MenuEditEOLConversion_DropDownOpening(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            //Check Line Endings
            MenuEolCRLF.Checked = false;
            MenuEolCR.Checked = false;
            MenuEolLF.Checked = false;

            MenuEolCRLF.Text = "Convert to &CRLF (Windows)";
            MenuEolCR.Text = "Convert to C&R (Mac)";
            MenuEolLF.Text = "Convert to &LF (Unix/OsX)";

            switch (ActiveEditor.textEditor.GetEolMode())
            {
                case (int)ScintillaTextEditor.SciConst.SC_EOL_CRLF:
                    MenuEolCRLF.Checked = true;
                    MenuEolCRLF.Text = "&CRLF (Windows)";
                    break;

                case (int)ScintillaTextEditor.SciConst.SC_EOL_CR:
                    MenuEolCR.Checked = true;
                    MenuEolCR.Text = "C&R (Mac)";
                    break;

                case (int)ScintillaTextEditor.SciConst.SC_EOL_LF:
                    MenuEolLF.Checked = true;
                    MenuEolLF.Text = "&LF (Unix/OsX)";
                    break;
            }
        }

        void editor_TextHasChanged(object sender, Int32 position, Int32 length)
        {
            CheckUndoRedo();
            //CheckMenuConditions();
        }

        void editor_SavePointLeft(object sender, EventArgs e)
        {
            ToolStripSave.Enabled = true;
            MenuFileSave.Enabled = true;
        }

        void editor_SavePointReached(object sender, EventArgs e)
        {
            ToolStripSave.Enabled = false;
            MenuFileSave.Enabled = false;
        }

        void textEditor_TextEditorModContainer(object sender, Int32 token)
        {
            //System.Diagnostics.Debug.WriteLine("TextEditorModContainer called");
            TimerMod.Start();
        }

        private void TimerMod_Tick(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("TimerMod_Tick called");
            CheckUndoRedo();
            TimerMod.Stop();
        }

        void tab_Enter(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            ActiveEditor.SetFocus();
            editor_FontHasChanged(ActiveEditor, null);
            CheckMenuConditions();
        }


        private void MenuFontReset_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;
                ActiveEditor.textEditor.TextEditorFont =
                    new Font(wxGlobal.Settings.EditorProperties.DefaultFontName,
                             wxGlobal.Settings.EditorProperties.DefaultFontSize);
                editor_FontHasChanged(ActiveEditor, null);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuFontReset_Click");
            }
        }

        private void MenuFontSizeNumber_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;
                ToolStripMenuItem mTemp = (ToolStripMenuItem)sender;
                ActiveEditor.textEditor.TextEditorFont =
                    new Font(ActiveEditor.textEditor.TextEditorFont.Name,
                             Convert.ToInt32(mTemp.Text));
                editor_FontHasChanged(ActiveEditor, null);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuFontSizeNumber_Click");
            }

        }


        void editor_FontHasChanged(object sender, EventArgs e)
        {
            try
            {
                wxEditor tEditor = (wxEditor)sender;
                Int32 mFontSize = 0;

                if (tEditor != null)
                    mFontSize = (Int32)tEditor.textEditor.TextEditorFont.Size;
                else
                    mFontSize = wxGlobal.Settings.EditorProperties.DefaultFontSize;

                ToolStripButtonFontSize.Text = mFontSize.ToString();

                ToolStripMenuItem mMenuFontSize = default(ToolStripMenuItem);
                ToolStripMenuItem mToolStripFontSize = default(ToolStripMenuItem);

                //clear all checks
                for (Int32 Ciclo = 0; Ciclo < MenuFontSize.DropDown.Items.Count; Ciclo++)
                {
                    mMenuFontSize =
                        (ToolStripMenuItem)MenuFontSize.DropDown.Items[Ciclo];
                    mMenuFontSize.Checked = false;

                    mToolStripFontSize =
                        (ToolStripMenuItem)ToolStripButtonFontSize.DropDown.Items[Ciclo];
                    mToolStripFontSize.Checked = false;
                }

                if (mFontSize > 5 && mFontSize < 21)
                {
                    Int32 mIndex = (int)(mFontSize - 6);
                    mMenuFontSize =
                        (ToolStripMenuItem)MenuFontSize.DropDown.Items[mIndex];
                    mMenuFontSize.Checked = true;

                    mToolStripFontSize =
                        (ToolStripMenuItem)ToolStripButtonFontSize.DropDown.Items[mIndex];
                    mToolStripFontSize.Checked = true;
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - editor_FontHasChanged");
            }
        }

        void editor_FileDropped(object sender, string[] filename)
        {
            try
            {
                //bool mod = (ModifierKeys & Keys.Control) != 0;
                //if (mod)
                //{
                //    if (string.IsNullOrEmpty(filename[0])) return;

                //    wxEditor temp = (wxEditor)sender;
                //    string s = "\"" + filename[0].Replace(@"\", @"/") + "\"";

                //    temp.textEditor.InsertText(
                //        temp.textEditor.GetCaretPosition(), s);
                //}
                //else
                {
                    //AddNewEditor(filename);
                    foreach (string f in filename)
                    {
                        if (File.Exists(f))
                        {
                            AddNewEditor(f);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Could not open the file!",
                                "WinXound error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        void editor_TextEditorMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ModifierKeys == Keys.None)
            {
                return;
            }

            //SELECT QUOTED STRING
            if (e.Button == MouseButtons.Left && ModifierKeys == (Keys.Control | Keys.Alt))
            {
                try
                {
                    wxEditor tEditor = (wxEditor)sender;

                    Int32 curPos = tEditor.textEditor.GetFocusedEditor.PositionFromPoint(e.X, e.Y);
                    tEditor.textEditor.GetFocusedEditor.GotoPos(curPos);

                    //Style Strings
                    if ((tEditor.textEditor.GetStyleAt(curPos) == 6 && tEditor.CurrentSyntaxLanguage == "csound") ||
                        (tEditor.textEditor.GetStyleAt(curPos) == 6 && tEditor.CurrentSyntaxLanguage == "lua") ||
                        (tEditor.textEditor.GetStyleAt(curPos) == 3 && tEditor.CurrentSyntaxLanguage == "python") ||
                        (tEditor.textEditor.GetStyleAt(curPos) == 4 && tEditor.CurrentSyntaxLanguage == "python"))
                    {
                        Point quotesPos = tEditor.textEditor.GetQuotesPosition(curPos);
                        if (quotesPos.X > 0 && quotesPos.Y > 0)
                        {
                            tEditor.textEditor.SetSelection(quotesPos.X - 1, quotesPos.Y + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //wxGlobal.wxMessageError(ex.Message,
                    //    "Form Main - editor_TextEditorMouseDown - SELECT QUOTED STRING");
                    System.Diagnostics.Debug.WriteLine(
                            "Form Main - editor_TextEditorMouseDown - SELECT QUOTED STRING: " +
                            ex.Message);
                }

                return;
            }


            //HYPERLINKS
            if (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control)
            {
                ContextOpenFile_Click(this, null);
                return;
            }


            //CONTEXT MENU
            if (e.Button == MouseButtons.Right)
            {
                wxEditor tEditor = (wxEditor)sender;
                Int32 mCurPos = tEditor.textEditor.GetCaretPosition();

                try
                {
                    mContextMenu.Items[13].Enabled = false;
                    //MenuInfoOpcodeHelp.Enabled = false;
                    mContextMenu.Items[13].Text = "Opcode Help";
                    //MenuInfoOpcodeHelp.Text = "Opcode Help";

                    //OPCODES DEFINITIONS
                    if (Opcodes.Contains(gCurWord))
                    {
                        mContextMenu.Items[13].Enabled = true;
                        //MenuInfoOpcodeHelp.Enabled = true;
                        mContextMenu.Items[13].Text = gCurWord + " -> Help";
                        //MenuInfoOpcodeHelp.Text = gCurWord + " -> Help";

                        mContextMenu.Items[8].Text = "Go to definition of ...";
                        mContextMenu.Items[9].Text = "Go to reference of ...";
                    }
                    else if (!string.IsNullOrEmpty(gCurWord))
                    {
                        string mcWord = Regex.Replace(gCurWord, "\\s", "");
                        if (!string.IsNullOrEmpty(mcWord))
                        {
                            mContextMenu.Items[8].Text = "Go to definition of " + mcWord;
                            mContextMenu.Items[9].Text = "Go to reference of " + mcWord;
                        }
                        ///'Me.Text = ">" & mcWord & "<" 
                        //gCurWord = mcWord 
                        else
                        {
                            mContextMenu.Items[8].Text = "Go to definition of ...";
                            mContextMenu.Items[9].Text = "Go to reference of ...";
                        }
                        //gCurWord = vbNullString 
                        //mContextMenu.Items[13].Text = "CSound -> Help";
                    }

                    else
                    {
                        mContextMenu.Items[8].Text = "Go to definition of ...";
                        mContextMenu.Items[9].Text = "Go to reference of ...";
                        //mContextMenu.Items[13].Text = "CSound -> Help";
                    }


                    //HYPERLINKS
                    if ((tEditor.textEditor.GetStyleAt(mCurPos) == 6 && tEditor.CurrentSyntaxLanguage == "csound") ||
                        (tEditor.textEditor.GetStyleAt(mCurPos) == 6 && tEditor.CurrentSyntaxLanguage == "lua") ||
                        (tEditor.textEditor.GetStyleAt(mCurPos) == 3 && tEditor.CurrentSyntaxLanguage == "python") ||
                        (tEditor.textEditor.GetStyleAt(mCurPos) == 4 && tEditor.CurrentSyntaxLanguage == "python"))
                    {
                        //string mString = tEditor.textEditor.GetTextInQuotes(mCurPos);
                        //if (!string.IsNullOrEmpty(mString))
                        //{
                        //    //mContextMenu.Items[11].Text = "Open \"" + mString + "\"";
                        //}
                        mContextMenu.Items[11].Enabled = true;
                    }
                    else
                    {
                        //mContextMenu.Items[11].Text = "Open File";
                        mContextMenu.Items[11].Enabled = false;
                    }

                    //Finally show the context menu
                    mContextMenu.Show(e.Location);
                }

                catch (Exception ex)
                {
                    //wxGlobal.wxMessageError(ex.Message,
                    //    "Form Main - editor_TextEditorMouseDown - RightMouseDown");
                    System.Diagnostics.Debug.WriteLine(
                        "Form Main - editor_TextEditorMouseDown - RightMouseDown: " +
                        ex.Message);
                }
            }
        }

        private void OpenHyperLinks(string mString)
        {
            if (ActiveEditor == null) return;

            try
            {
                //Search for current file directory
                if (File.Exists(Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString))
                {
                    mString = Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString;
                }
                //Search for full path
                else if (File.Exists(mString))
                {
                    //check for slash
                    mString = mString.Replace("/", "\\");
                }
                //Search for INCDIR
                else if (File.Exists(wxGlobal.Settings.Directory.INCDIR + "\\" + mString))
                {
                    mString = wxGlobal.Settings.Directory.INCDIR + "\\" + mString;
                }
                //Search for SFDIR
                else if (File.Exists(wxGlobal.Settings.Directory.SFDIR + "\\" + mString))
                {
                    mString = wxGlobal.Settings.Directory.SFDIR + "\\" + mString;
                }
                //Search for SSDIR
                else if (File.Exists(wxGlobal.Settings.Directory.SSDIR + "\\" + mString))
                {
                    mString = wxGlobal.Settings.Directory.SSDIR + "\\" + mString;
                }
                //Search for SADIR
                else if (File.Exists(wxGlobal.Settings.Directory.SADIR + "\\" + mString))
                {
                    mString = wxGlobal.Settings.Directory.SADIR + "\\" + mString;
                }
                //Search for MFDIR
                else if (File.Exists(wxGlobal.Settings.Directory.MFDIR + "\\" + mString))
                {
                    mString = wxGlobal.Settings.Directory.MFDIR + "\\" + mString;
                }

                System.Diagnostics.Debug.WriteLine(mString);

                if (!File.Exists(mString)) return;

                //OPEN AUDIO FILES
                if (mString.ToLower().EndsWith(".wav") ||
                    mString.ToLower().EndsWith(".aif") ||
                    mString.ToLower().EndsWith(".aiff"))
                {
                    if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WaveEditor))
                    {
                        if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
                        {
                            System.Diagnostics.Process.Start(
                                wxGlobal.Settings.Directory.WaveEditor,
                                "\"" + mString + "\"");
                        }
                        else
                        {
                            MessageBox.Show("Cannot find External Wave Editor Path!" + newline +
                                            "Please select a valid path in File->Settings->Wave Editor executable",
                                            "Compiler error!",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
                    }
                }
                //OPEN MIDI FILES
                else if (mString.ToLower().EndsWith(".mid"))
                {
                    System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
                }
                //TRY TO OPEN OTHER FILES
                else
                {
                    AddNewEditor(mString);
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - OpenHyperLinks");
            }
        }


        void editor_TextEditorKeyAction(object sender, KeyEventArgs e)
        {
            FormMain_KeyDown(sender, e);
        }


        void editor_CaretPositionChanged(object sender, EventArgs e)
        {
            wxEditor tEditor = (wxEditor)sender;
            if (tEditor.textEditor.GetTextLength() <= 0) return;
            if (tEditor.textEditor.GetCaretPosition() <= 0) return;
            if (!wxGlobal.isSyntaxType(tEditor)) return;


            if (wxGlobal.Settings.EditorProperties.ShowMatchingBracket == true)
                CheckForBracket(tEditor);

            try
            {

                //Retrieve the current word and search inside Opcode Database 
                gCurWord =
                    tEditor.textEditor.GetWordAt(tEditor.textEditor.GetCaretPosition());

                //if (Opcodes.Contains(gCurWord))
                if (gCurWord.Length > 1 && Opcodes.Contains(gCurWord))
                {
                    string[] split = Opcodes[gCurWord].ToString().Split("|".ToCharArray());
                    tEditor.SetIntelliTip("[" + gCurWord + "] - " + split[1], split[2]);
                }


                //Autocompletion check (line change)
                curLine = tEditor.textEditor.GetLineNumberFromPosition(
                                tEditor.textEditor.GetCaretPosition());
                if (curLine != oldLine)
                {
                    oldLine = curLine;
                    if (ListBoxAutoComplete.Visible)
                        ListBoxAutoComplete.Hide();
                }

                //If Autocompletion is visible find current word in ListBox
                if (ListBoxAutoComplete.Visible)
                {
                    int index = ListBoxAutoComplete.FindString(gCurWord);
                    // Determine if a valid index is returned. Select the item if it is valid.
                    if (index != -1)
                        ListBoxAutoComplete.SetSelected(index, true);

                }
            }

            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, 
                //  "CRITICAL ERROR - Document_CaretPositionChanged Error");
                System.Diagnostics.Debug.WriteLine(
                        "Form Main - editor_CaretPositionChanged: " + ex.Message);
            }
        }

        private void CheckForBracket(wxEditor tEditor)
        {
            try
            {
                Int32 caretpos = tEditor.textEditor.GetCaretPosition();

                if (tEditor.textEditor.GetCharAt(caretpos - 1) == ')')
                {
                    Int32 pos1 = tEditor.textEditor.BraceMatch(caretpos - 1, 0);
                    if (pos1 < 0)
                    {
                        tEditor.textEditor.BraceHiglight(-1, -1);
                        return;
                    }

                    tEditor.textEditor.BraceHiglight(pos1, caretpos - 1);
                }
                else if (tEditor.textEditor.GetCharAt(caretpos) == '(')
                {
                    Int32 pos1 = tEditor.textEditor.BraceMatch(caretpos, 0);
                    if (pos1 < 0) return;

                    tEditor.textEditor.BraceHiglight(pos1, caretpos);
                }
                else
                {
                    tEditor.textEditor.BraceHiglight(-1, -1);
                }
            }

            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, 
                //  "CRITICAL ERROR - Document_CaretPositionChanged Error");
                System.Diagnostics.Debug.WriteLine(
                        "Form Main - CheckForBracket: " + ex.Message);
            }
        }


        private void ContextOpenFile_Click(object sender, EventArgs e)
        {
            //HYPERLINKS
            try
            {
                if (ActiveEditor == null) return;

                wxEditor tEditor = ActiveEditor;
                Int32 curPos = tEditor.textEditor.GetCaretPosition();

                //Style Strings
                if ((tEditor.textEditor.GetStyleAt(curPos) == 6 && tEditor.CurrentSyntaxLanguage == "csound") ||
                    (tEditor.textEditor.GetStyleAt(curPos) == 6 && tEditor.CurrentSyntaxLanguage == "lua") ||
                    (tEditor.textEditor.GetStyleAt(curPos) == 3 && tEditor.CurrentSyntaxLanguage == "python") ||
                    (tEditor.textEditor.GetStyleAt(curPos) == 4 && tEditor.CurrentSyntaxLanguage == "python"))
                {
                    string mString = tEditor.textEditor.GetTextInQuotes(curPos);

                    if (string.IsNullOrEmpty(mString)) return;

                    OpenHyperLinks(mString);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - editor_TextEditorMouseDown - ContextOpenFile_Click");
            }

            return;
        }

        private void ContextGoToDefinition_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(gCurWord)) return;
            if (ActiveEditor == null) return;

            wxEditor tEditor = ActiveEditor;
            string Definition = gCurWord.Trim();

            try
            {
                if (Definition.EndsWith("."))
                {
                    Definition = Definition.TrimEnd('.');
                }

                if (!ActiveEditor.UserOpcodes.Contains(Definition))
                {
                    ////MACRO SEARCH 
                    if (Definition.StartsWith("g") ||
                        Definition.StartsWith("$"))
                    {
                        if (Definition.StartsWith("$"))
                        {
                            Definition = Definition.TrimStart('$');
                        }
                        //Global Type: search all text from start
                        Int32 mPos =
                            ActiveEditor.textEditor.FindText(
                                Definition, true, true,
                                false, true, false, true,
                                0, ActiveEditor.textEditor.GetTextLength());
                        if (mPos > -1)
                            ActiveEditor.StoreCursorPos(mPos);
                        return;

                    }

                    ////LOCAL_TYPE SEARCH: search inside "instr" -> "endin" 
                    else
                    {
                        Int32 posINSTR = -1;
                        Int32 curPos = ActiveEditor.textEditor.GetCaretPosition();
                        Int32 mFindPos = -1;

                        //Search INSTR 
                        mFindPos = ActiveEditor.textEditor.FindText(
                            "instr", true, true, true,
                            false, false, true);
                        if (mFindPos > -1)
                        {
                            posINSTR = mFindPos;
                        }
                        else return;

                        mFindPos =
                            ActiveEditor.textEditor.FindText(
                                Definition, true, true,
                                false, true, false, true,
                                posINSTR, curPos - 1); //Definition.Length);
                        if (mFindPos > -1)
                            ActiveEditor.StoreCursorPos(mFindPos);
                    }
                }

                ////USER OPCODE SEARCH
                else if (ActiveEditor.UserOpcodes.Contains(Definition))
                {
                    ArrayList mc =
                        ActiveEditor.textEditor.SearchText(
                            Definition, true, true, false, true);
                    foreach (Int32 mPos in mc)
                    {
                        string mLine = ActiveEditor.textEditor.GetTextLine(
                                       ActiveEditor.textEditor.GetLineNumberFromPosition(
                                       mPos));
                        Int32 mDefinitionPos = mLine.IndexOf(Definition);
                        Int32 mOpcodePos = mLine.IndexOf("opcode");
                        if (mOpcodePos > -1 &&
                            mOpcodePos < mDefinitionPos)
                        {
                            ActiveEditor.textEditor.SetCaretPosition(mPos);
                            ActiveEditor.textEditor.SetSelectionEnd(mPos + Definition.Length);
                            if (mPos > -1)
                                ActiveEditor.StoreCursorPos(mPos);
                            break;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "FormMain - ContextGoToDefinition_Click");
            }


        }



        private void ContextGoToReference_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(gCurWord)) return;
            if (ActiveEditor == null) return;

            wxEditor tEditor = ActiveEditor;
            string Definition = gCurWord.Trim();

            try
            {
                if (!ActiveEditor.UserOpcodes.Contains(Definition))
                {
                    ////MACRO TYPE: search all text from current position
                    if (ActiveEditor.textEditor.GetTextLine(
                        ActiveEditor.textEditor.GetCurrentLineNumber()).Contains("#define"))
                    {
                        Definition = "$" + Definition;

                        Int32 mPos =
                            ActiveEditor.textEditor.FindText(
                                  Definition, true, true, false,
                                  true, false, true,
                                  ActiveEditor.textEditor.GetCaretPosition() + 1,
                                  ActiveEditor.textEditor.GetTextLength());
                        if (mPos > -1)
                            ActiveEditor.StoreCursorPos(mPos);
                        return;
                    }

                    ////GLOBAL TYPE: search all text from current position
                    else if (Definition.StartsWith("g") ||
                             Definition.StartsWith("$"))
                    {

                        Int32 mPos =
                            ActiveEditor.textEditor.FindText(
                                  Definition, true, true, false,
                                  true, false, true,
                                  ActiveEditor.textEditor.GetCaretPosition() + 1,
                                  ActiveEditor.textEditor.GetTextLength());
                        if (mPos > -1)
                            ActiveEditor.StoreCursorPos(mPos);
                        return;

                    }

                    ////LOCAL TYPE: search inside "instr" -> "endin" 
                    else
                    {

                        Int32 posENDIN = -1;
                        Int32 curPos = ActiveEditor.textEditor.GetCaretPosition();
                        Int32 mFindPos = -1;

                        //Search ENDIN 
                        mFindPos = ActiveEditor.textEditor.FindText(
                                                         "endin", true, true,
                                                         false, false, false, true);
                        if (mFindPos > -1)
                        {
                            posENDIN = mFindPos;
                        }
                        else return;

                        //Search reference
                        mFindPos = ActiveEditor.textEditor.FindText(
                                 Definition, true, true,
                                 false, true, false, true,
                                 curPos + 1, posENDIN);
                        if (mFindPos > -1)
                            ActiveEditor.StoreCursorPos(mFindPos);
                    }
                }

                //USER OPCODES SEARCH
                else if (ActiveEditor.UserOpcodes.Contains(Definition))
                {
                    Int32 mFindPos = ActiveEditor.textEditor.FindText(
                                            Definition, true, true,
                                            false, true, false, true);
                    if (mFindPos > -1)
                        ActiveEditor.StoreCursorPos(mFindPos);
                }

            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "FormMain - ContextGoToReference_Click");
            }


        }








        private void MenuFileNew_Click(object sender, EventArgs e)
        {
            FormNew fn = new FormNew();
            fn.ReturnValueEv += new FormNew.OnReturnValue(fn_ReturnValueEv);

            fn.ShowDialog(this);
            fn.ReturnValueEv -= new FormNew.OnReturnValue(fn_ReturnValueEv);
            fn.Dispose();

            try { this.wxTabCode.SelectedTab.Focus(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain - MenuFileNew_Click Error: " + ex.Message);
            }

        }

        void fn_ReturnValueEv(object sender, string ReturnValue)
        {
            if (ReturnValue == "csound")
            {
                AddNewEditor(mUntitled + ".csd");
            }
            else if (ReturnValue == "orc")
            {
                AddNewEditor(mUntitled + ".orc");
            }
            else if (ReturnValue == "sco")
            {
                AddNewEditor(mUntitled + ".sco");
            }
            else if (ReturnValue == "python")
            {
                AddNewEditor(mUntitled + ".py");
            }
            else if (ReturnValue == "lua")
            {
                AddNewEditor(mUntitled + ".lua");
            }
            else if (ReturnValue == "cabbage")
            {
                AddNewEditor(mUntitled + ".cabbage");
            }
        }



        private void MenuFileOpen_Click(object sender, EventArgs e)
        {
            try
            {
                //Open document 
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.Filter =
                    "Supported files (*.csd;*.orc;*.sco;*.py;*.pyw;*.lua)|*.csd;*.orc;*.sco;*.py;*.pyw;*.lua|" +
                    "CSound Files (*.csd;*.orc;*.sco)|*.csd;*.orc;*.sco|" +
                    "Python files (*.py;*.pyw)|*.py;*.pyw|" +
                    "Lua files (*.lua)|*.lua|" +
                    "All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = true;
                openFileDialog1.RestoreDirectory = true;
                if (mWorkingDir)
                {
                    openFileDialog1.InitialDirectory = wxGlobal.Settings.Directory.WorkingDir;
                    mWorkingDir = false;
                }

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    if (ActiveEditor != null) ActiveEditor.SetFocus();
                    return;
                }

                // Open file(s)
                //OpenFiles(openFileDialog1.FileNames);
                foreach (string fn in openFileDialog1.FileNames)
                {
                    AddNewEditor(fn);
                }

                if (ActiveEditor != null) ActiveEditor.SetFocus();
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileOpen_Click");
            }
        }








        private void MenuFileClose_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null)
                RemoveEditor(ActiveEditor);
        }

        private bool RemoveEditor(wxEditor editor)
        {
            if (editor != null)
            {
                bool savebookmarks = true;

                try
                {
                    //Check text changes
                    if (editor.textEditor.IsTextChanged() == true &&
                        editor.FileIsReadOnly == false) //skip ReadOnly files
                    {
                        DialogResult ret = MessageBox.Show(
                                                       "Document '" + editor.FileName +
                                                       "' has changed!" + newline +
                                                       "Do you want to save it?",
                                                       "WinXound",
                                                       MessageBoxButtons.YesNoCancel,
                                                       MessageBoxIcon.Information);

                        if (ret == DialogResult.Yes)
                        {
                            MenuFileSave.PerformClick();
                            //If the user has cancelled the SaveFileDialog we must return false
                            //So we check IsTextChanged() to see if it's not saved
                            if (editor.textEditor.IsTextChanged() == true &&
                                editor.FileIsReadOnly == false)
                                return false;
                        }
                        else if (ret == DialogResult.No)
                        {
                            savebookmarks = false;
                        }
                        else if (ret == DialogResult.Cancel)
                        {
                            editor.SetFocus();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    wxGlobal.wxMessageError(ex.Message,
                        "FormMain - RemoveEditor - Check Save method");
                }


                try
                {
                    //Save Bookmarks
                    if (savebookmarks &&
                        wxGlobal.Settings.EditorProperties.SaveBookmarks &&
                        !FileIsUntitled(editor))
                    {
                        SaveBookmarks(editor);
                    }

                    //Close editor tab and release related resources
                    if (wxTabCode.TabPages.Count > 0)
                    {
                        //Int32 tabIndex = wxTabCode.Controls.IndexOf(editor.Parent) - 1;
                        //if (tabIndex < 0) tabIndex = 0;
                        //wxTabCode.Controls.Remove(editor.Parent);
                        //wxTabCode.SelectedIndex = tabIndex;


                        Control tabToRemove = editor.Parent;

                        Int32 tabIndex = wxTabCode.Controls.IndexOf(tabToRemove) - 1;
                        if (tabIndex < 0) tabIndex = 0;

                        //Remove tab and release all resources
                        wxTabCode.Controls.Remove(tabToRemove);
                        ////editor.Dispose();
                        tabToRemove.Dispose();

                        wxTabCode.SelectedIndex = tabIndex;
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "FormMain.RemoveEditor - Close Tab method: " + ex.Message);
                }

            }

            if (wxTabCode.TabPages.Count == 0)
            {
                this.Text = wxGlobal.TITLE;
                CheckMenuConditions();
                this.Focus();

                //Don't use! Only for debug!!!
                //GC.Collect();
            }

            return true;
        }

        private bool RemoveAllEditors()
        {
            bool IsClosed = false;
            foreach (TabPage tp in wxTabCode.TabPages)
            {
                wxTabCode.SelectedTab = tp; //To prevent bug on close!!!
                IsClosed = RemoveEditor((wxEditor)tp.Controls[0]);
                if (IsClosed == false) return false;
            }

            return true;
        }

        private void MenuFileCloseAll_Click(object sender, EventArgs e)
        {
            RemoveAllEditors();
        }



        private void wxTabCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Old system: substituted by CheckMenuConditions
            //if (ActiveEditor != null)
            //    this.Text = wxGlobal.TITLE + " - " + ActiveEditor.FileName;
        }

        private void MenuEditUndo_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Undo();
        }

        private void MenuEditRedo_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Redo();
        }

        private void MenuEditCut_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Cut();
        }

        private void MenuEditCopy_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Copy();
        }

        private void MenuEditPaste_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Paste();
        }

        private void MenuEditDelete_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.Delete();
        }

        private void MenuEditSelectAll_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.textEditor.SelectAll();
        }

        private void MenuInsertHeader_Click(object sender, EventArgs e)
        {
            if (ActiveEditor != null)
            {
                FormInsertHeader mForm = new FormInsertHeader();
                mForm.ButtonClicked +=
                    new FormInsertHeader.OnButtonClicked(FormInsertHeader_ButtonClicked);

                mForm.ShowDialog(this);

                mForm.ButtonClicked -=
                    new FormInsertHeader.OnButtonClicked(FormInsertHeader_ButtonClicked);
                mForm.Dispose();
                ActiveEditor.SetFocus();
            }
        }

        void FormInsertHeader_ButtonClicked(string TextToInsert, bool AtStart)
        {
            if (ActiveEditor == null) return;

            try
            {
                string StringToInsert =
                    ActiveEditor.textEditor.NewLine + ActiveEditor.textEditor.NewLine +
                    ActiveEditor.textEditor.ConvertEolOfString(TextToInsert) +
                    ActiveEditor.textEditor.NewLine;


                if (AtStart == true)
                {
                    //Find <CsInstruments> 
                    string StringToFind = "<CsInstruments>";
                    Int32 mFindPos = -1;

                    mFindPos = ActiveEditor.textEditor.FindText(
                                                     StringToFind, true, true, false,
                                                     false, false, true, 0, -1);
                    if (mFindPos > -1)
                    {
                        ActiveEditor.textEditor.SetCaretPosition(
                            mFindPos + StringToFind.Length);
                        ActiveEditor.textEditor.InsertText(
                            ActiveEditor.textEditor.GetCaretPosition(), StringToInsert);
                        ActiveEditor.textEditor.SetCaretPosition(
                                                      mFindPos +
                                                      StringToFind.Length +
                                                      StringToInsert.Length);
                        ActiveEditor.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("Unable to find <CsInstruments> section",
                                        "Error", MessageBoxButtons.OK);
                        return;
                    }

                }

                else
                {
                    ActiveEditor.textEditor.InsertText(
                        ActiveEditor.textEditor.GetCaretPosition(), StringToInsert);
                    ActiveEditor.textEditor.SetCaretPosition(
                        ActiveEditor.textEditor.GetCaretPosition() +
                                                  StringToInsert.Length);
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - FormInsertHeader_ButtonClicked");
            }
        }

        private void MenuInfoAbout_Click(object sender, EventArgs e)
        {
            FormAbout fm = new FormAbout();
            fm.ShowDialog(this);
            fm.Dispose();
        }



        //TABCONTROLBUILD Visual Stuffs
        Int32 selIndex = 0;
        Int32 oldHeight = 0;
        private void tabControlBuild_MouseDown(object sender, MouseEventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    return;

            try
            {
                if (selIndex != tabControlBuild.SelectedIndex)
                {
                    selIndex = tabControlBuild.SelectedIndex;
                    if (oldHeight > tabControlBuild.MinimumSize.Height)
                    {
                        tabControlBuild.Size = new Size(0, oldHeight);
                    }
                    else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                    tabControlBuild.Refresh();
                }
                else
                {
                    if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
                    {
                        if (oldHeight > tabControlBuild.MinimumSize.Height)
                        {
                            tabControlBuild.Size = new Size(0, oldHeight);
                        }
                        else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                    }
                    else
                    {
                        tabControlBuild.Size = new Size(0, tabControlBuild.MinimumSize.Height);
                    }
                    tabControlBuild.Refresh();
                }

                ////tabControlBuild.SelectedTab = tabRepository;
                //if (tabControlBuild.SelectedTab == tabRepository)
                //{
                //    wxCodeRepository1.SetFocus();
                //}

                if (ActiveEditor != null) ActiveEditor.SetFocus();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - tabControlBuild_MouseDown");
            }
        }

        private void tabControlBuild_SizeChanged(object sender, EventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    return;

            try
            {
                if (tabControlBuild.Height > tabControlBuild.MinimumSize.Height)
                    oldHeight = tabControlBuild.Height;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain.tabControlBuild_SizeChanged: " + ex.Message);
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("FORM_MAIN KEY DOWN");

            if (e.KeyCode == Keys.Escape)
            {
                if (wxCompilerConsole1.ButtonStopCompiler.Enabled)
                {
                    wxCompilerConsole1.ButtonStopCompiler.PerformClick();
                    return;
                }
                if (tabControlBuild.Height > tabControlBuild.MinimumSize.Height)
                {
                    if (compilerWindow != null)
                        if (compilerWindow.Visible)
                            return;
                    tabControlBuild.Size = new Size(0, tabControlBuild.MinimumSize.Height);
                    return;
                }
                if (ListBoxAutoComplete.Visible)
                {
                    ListBoxAutoComplete.Hide();
                }
            }
            else if (e.KeyCode == Keys.L &&
                     e.Modifiers == (Keys.Control | Keys.Alt) &&
                     ActiveEditor != null)
            {
                ToolStripCSoundConsole.ShowDropDown();
            }
            //else if (e.KeyCode == Keys.Space)
            //{
            //    if (ListBoxAutoComplete.Visible)
            //        ListBoxAutoComplete.Hide();
            //}
        }

        private void MenuViewShowCode_Click(object sender, EventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    return;

            try
            {
                if (tabControlBuild.Height > tabControlBuild.MinimumSize.Height)
                {
                    tabControlBuild.Size = new Size(0, tabControlBuild.MinimumSize.Height);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuViewShowCode_Click");
            }
        }

        private void MenuViewShowCompiler_Click(object sender, EventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    compilerWindow.Focus();

            try
            {
                if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
                {
                    if (oldHeight > tabControlBuild.MinimumSize.Height)
                    {
                        tabControlBuild.Size = new Size(0, oldHeight);
                    }
                    else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                }
                tabControlBuild.SelectedTab = tabCompiler;
                selIndex = tabControlBuild.SelectedIndex;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuViewShowCompiler_Click");
            }
        }

        private void MenuViewShowHelp_Click(object sender, EventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    compilerWindow.Focus();

            try
            {
                if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
                {
                    if (oldHeight > tabControlBuild.MinimumSize.Height)
                    {
                        tabControlBuild.Size = new Size(0, oldHeight);
                    }
                    else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                }
                tabControlBuild.SelectedTab = tabHelp;
                selIndex = tabControlBuild.SelectedIndex;
                tabHelp.Focus(); HelpBrowser.Focus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuViewShowHelp_Click");
            }
        }

        private void MenuEditOpcodesMenu_Click(object sender, EventArgs e)
        {
            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    compilerWindow.Focus();

            //Popup CSound Opcodes TAB
            try
            {
                if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
                {
                    if (oldHeight > tabControlBuild.MinimumSize.Height)
                    {
                        tabControlBuild.Size = new Size(0, oldHeight);
                    }
                    else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                }
                tabControlBuild.SelectedTab = tabOpcodes;
                selIndex = tabControlBuild.SelectedIndex;
                wxCSoundOpcodesRepository1.treeViewOpcodes.Focus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditOpcodesMenu_Click");
            }
        }

        private void HelpBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyCode == Keys.Escape) MenuViewShowCode_Click(null,null);
            System.Diagnostics.Debug.WriteLine("HelpBrowser_PreviewKeyDown");

            if (compilerWindow != null)
                if (compilerWindow.Visible)
                    if (e.KeyCode == Keys.Escape)
                        this.Focus();

            try
            {
                FormMain_KeyDown(this, new KeyEventArgs(e.KeyData));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain.HelpBrowser_PreviewKeyDown: " + ex.Message);
            }

        }

        private void tabControlBuild_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("tabControlBuild_PreviewKeyDown");
            //this.Focus();

            try
            {
                FormMain_KeyDown(this, new KeyEventArgs(e.KeyData));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain.tabControlBuild_PreviewKeyDown: " + ex.Message);
            }
        }

        //TABCONTROLBUILD Visual Stuffs END



        private void MenuInfoWinXoundHelp_Click(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + "\\Help\\winxound_help.html"))
            {
                try
                {
                    HelpBrowser.Navigate(Application.StartupPath + "\\Help\\winxound_help.html");
                    MenuViewShowHelp_Click(null, null);
                }
                catch (Exception ex)
                {
                    wxGlobal.wxMessageError(ex.Message,
                        "Form Main - MenuInfoWinXoundHelp_Click");
                }
            }
            else
            {
                MessageBox.Show("Cannot find WinXound Html Help!",
                                "Compiler error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void MenuInfoCSoundHelp_Click(object sender, EventArgs e)
        {
            if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
            {
                try
                {
                    HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
                    MenuViewShowHelp_Click(null, null);
                }
                catch (Exception ex)
                {
                    wxGlobal.wxMessageError(ex.Message,
                        "Form Main - MenuInfoCSoundHelp_Click");
                }
            }
            else
            {
                MessageBox.Show("Cannot find 'CSound Html Help' Path!" + newline +
                        "Please select a valid path in File->Settings->CSound Html Help",
                        "WinXound error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }

        string gCurWord = "";
        private void MenuInfoOpcodeHelp_Click(object sender, EventArgs e)
        {
            try
            {
                if (Opcodes.Contains(gCurWord))
                {
                    if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundHelpHTML))
                    {
                        string mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\" + gCurWord + ".html";

                        //Some words manual switch
                        //0dbfs
                        if (gCurWord == "0dbfs")
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\Zerodbfs.html";
                        }

                        //tb family
                        if (gCurWord.StartsWith("tb") &&
                            char.IsDigit(gCurWord, 2))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\tb.html";
                        }
                        //PyAssign family
                        if (gCurWord.StartsWith("pyassign") ||
                            gCurWord.StartsWith("pylassign"))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\pyassign.html";
                        }
                        //PyCall family
                        if (gCurWord.StartsWith("pycall") ||
                            gCurWord.StartsWith("pylcall"))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\pycall.html";
                        }
                        //PyEval family
                        if (gCurWord.StartsWith("pyeval") ||
                            gCurWord.StartsWith("pyleval"))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\pyeval.html";
                        }
                        //PyExec family
                        if (gCurWord.StartsWith("pyexec") ||
                            gCurWord.StartsWith("pylexec"))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\pyexec.html";
                        }
                        //Pyrun family
                        if (gCurWord.StartsWith("pyrun") ||
                            gCurWord.StartsWith("pylrun"))
                        {
                            mFile =
                            Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                            "\\pyrun.html";
                        }



                        //Finally try to load the html file
                        if (File.Exists(mFile))
                        {
                            HelpBrowser.Navigate(mFile);
                            MenuViewShowHelp_Click(null, null);
                        }
                        else
                        {
                            //HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
                            HelpBrowser.Navigate(Application.StartupPath + "\\Help\\error.html");
                            MenuViewShowHelp_Click(null, null);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundHelpHTML))
                    {
                        //HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
                        HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                                             "\\PartReference.html");
                        MenuViewShowHelp_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuInfoOpcodeHelp_Click Error - F1 Key");
            }
        }



        ///////////////////////////////////////////////////////////////////////////////////////
        //CSOUND COMPILER USEFUL METHODS
        private string SaveBeforeCompile()
        {
            if (ActiveEditor == null) return "";

            //Check also for Player opened files:
            mPlayer.ClosePlayer();

            if (ActiveEditor.FileIsReadOnly == false)
            {
                if (!File.Exists(ActiveEditor.FileName))
                {
                    string tempFilename =
                        Application.StartupPath + "\\Settings\\" + mWinXound_Untitled;

                    tempFilename += Path.GetExtension(ActiveEditor.FileName);

                    File.WriteAllText(tempFilename,
                                      ActiveEditor.textEditor.GetText());
                    return tempFilename;
                }
                else
                {
                    MenuFileSave_Click(this, null);
                    return ActiveEditor.FileName;
                }
            }

            return "";
        }

        private bool CheckDisplaysFlag()
        {
            if (ActiveEditor == null) return false;

            //If --displays flag is present we must show csound windows and suppress the
            //--asciidisplay or --postscriptdisplay
            try
            {
                //Find <CsOptions> tags
                Int32 findStart = ActiveEditor.textEditor.FindText(
                                     "<CsOptions>", true, true, false,
                                     false, false, true,
                                     0, ActiveEditor.textEditor.GetTextLength());

                Int32 findEnd = ActiveEditor.textEditor.FindText(
                                     "</CsOptions>", true, true, false,
                                     false, false, true,
                                     0, ActiveEditor.textEditor.GetTextLength());

                if (findStart > -1 && findEnd > -1)
                {
                    Int32 linestart, lineend;
                    linestart = ActiveEditor.textEditor.GetLineNumberFromPosition(findStart);
                    lineend = ActiveEditor.textEditor.GetLineNumberFromPosition(findEnd);

                    for (Int32 i = linestart; i < lineend; i++)
                    {
                        string text =
                            ActiveEditor.textEditor.GetTextLine(i);
                        if (text.IndexOf(";") > -1)
                        {
                            text = text.Remove(text.IndexOf(";"));
                            if (text.Contains("--displays") /*||
                                text.Contains("--nodisplays") ||
                                text.Contains("-d")*/
                                                     )
                            {
                                if (!text.Contains("--asciidisplay") &&
                                    !text.Contains("-g") &&
                                    !text.Contains("--postscriptdisplay") &&
                                    !text.Contains("-G"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain.CheckDisplayFlag Error: " + ex.Message);
            }

            return false;
        }

        private string GetCSoundFlags(string filename)
        {
            if (ActiveEditor == null)
            {
                if (string.IsNullOrEmpty(filename))
                    return wxGlobal.Settings.General.CSoundDefaultFlags.Trim();
                else
                    return
                        wxGlobal.Settings.General.CSoundDefaultFlags.Trim() + " " +
                        "\"" + filename + "\"";
            }

            //Default WinXound flags for CSound:
            //-B4096 --displays --asciidisplay

            string flags = "";

            try
            {
                if (MenuToolsUseDefaultFlags.Checked == false)
                {
                    //User has unchecked 'UseDefaultFlags' menu, so we skip default flags
                    flags = "";
                    System.Diagnostics.Debug.Write("MenuToolsUseDefaultFlags = false - ");
                }

                //TODO: ... maybe must be tested a little more
                else if (CheckDisplaysFlag() == true)
                {
                    //Found --displays option so:
                    //temporarily remove all 'display(s)' settings from the default winxound 
                    //settings and leave the others
                    string defaultflags = wxGlobal.Settings.General.CSoundDefaultFlags.Trim();
                    defaultflags = defaultflags.Replace("--displays", "");
                    defaultflags = defaultflags.Replace("--nodisplays", "");
                    defaultflags = defaultflags.Replace("-d ", " ");
                    defaultflags = defaultflags.Replace("--asciidisplay", "");
                    defaultflags = defaultflags.Replace("-g ", " ");
                    defaultflags = defaultflags.Replace("--postscriptdisplay", "");
                    defaultflags = defaultflags.Replace("-G ", " ");
                    flags = defaultflags.Trim() + " ";

                    System.Diagnostics.Debug.Write("CheckDisplaysFlag = true - ");
                }

                else
                {
                    //USE DEFAULT WINXOUND SETTINGS FOR CSOUND
                    flags = wxGlobal.Settings.General.CSoundDefaultFlags.Trim() + " ";
                    System.Diagnostics.Debug.Write("Default Flags used - ");
                }

                //If filename is not empty we add it to compiler arguments
                if (!string.IsNullOrEmpty(filename))
                {
                    flags += "\"" + filename + "\"";
                }

                flags = flags.Trim();

                System.Diagnostics.Debug.WriteLine("Flags: " + flags);

            }

            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(filename))
                    flags = wxGlobal.Settings.General.CSoundDefaultFlags.Trim();
                else
                    flags = wxGlobal.Settings.General.CSoundDefaultFlags.Trim() + " " +
                            "\"" + filename + "\"";

                System.Diagnostics.Debug.WriteLine(
                    "FormMain.GetCSoundFlags Error: " + ex.Message);
            }

            return flags;
        }


        Int32 mCompilerErrorLine = 0;
        private void MenuToolsCompile_Click(object sender, EventArgs e)
        {
            Compile("");
        }

        private void Compile(string additionalParams)
        {

            if (ActiveEditor == null) return;

            if (wxCompilerConsole1.ProcessActive)
            {
                MessageBox.Show("Compiler is already running!" + newline +
                                "Stop it manually and retry",
                                "WinXound Info - Compiler busy",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }


            //Save file before to compile and retrieve the filename to pass to the compiler
            string TextEditorFileName = SaveBeforeCompile();


            //OLD BEHAVIOUR: Now WinXound save automatically the Untitled document
            //to a temporary file
            /* 
            if (!File.Exists(ActiveEditor.FileName))
            {
                MessageBox.Show("Please specify a valid filename before to compile.",
                                "Compiler Error - Invalid filename",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            */
            ///////


            try
            {
                //COMPILE WITH CSOUND
                if (ActiveEditor.FileName.ToLower().EndsWith(".csd") ||
                    ActiveEditor.FileName.ToLower().EndsWith(".orc") ||
                    ActiveEditor.FileName.ToLower().EndsWith(".sco"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundConsole) ||
                        !File.Exists(wxGlobal.Settings.Directory.CSoundConsole))
                    {
                        MessageBox.Show("Cannot find CSound compiler!" + newline +
                                        "Please select a valid path in File->Settings->CSound.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }



                    additionalParams = CheckForAdditionalFlags(additionalParams);
                    if (additionalParams == "[CANCEL]") return;



                    //If SFDIR is not defined output the soundfile to the filename directory
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.SFDIR) ||
                        wxGlobal.Settings.Directory.UseSFDIR == false)
                    {
                        System.Environment.SetEnvironmentVariable("SFDIR",
                            //Path.GetDirectoryName(ActiveEditor.FileName));
                            Path.GetDirectoryName(TextEditorFileName));

                        //wxCompilerConsole1.SetEnvironment(ActiveEditor.FileName);
                    }


                    //Show and start the Compiler Console
                    MenuViewShowCompiler_Click(null, null);

                    //Reset mCompilerErrorLine to 0
                    mCompilerErrorLine = 0;
                    mCompilerEditor = ActiveEditor;

                    //Call CSound compiler
                    wxCompilerConsole1.Title = ActiveEditor.FileName;
                    wxCompilerConsole1.Filename = wxGlobal.Settings.Directory.CSoundConsole;

                    //CSound compiler arguments
                    if (TextEditorFileName.ToLower().EndsWith(".orc"))
                    {
                        wxCompilerConsole1.Arguments =
                            "[ORCSCO] " +
                            additionalParams.Trim() + " " +
                            GetCSoundFlags(TextEditorFileName) + " " +
                            "\"" + ActiveEditor.GetCurrentOrcScoFile() + "\"";
                    }
                    else if (TextEditorFileName.ToLower().EndsWith(".sco"))
                    {
                        wxCompilerConsole1.Arguments =
                            "[ORCSCO] " +
                            additionalParams.Trim() + " " +
                            GetCSoundFlags(ActiveEditor.GetCurrentOrcScoFile()) + " " +
                            "\"" + TextEditorFileName + "\"";
                    }
                    else
                    {
                        wxCompilerConsole1.Arguments = additionalParams.Trim() + " " +
                                                       GetCSoundFlags(TextEditorFileName);
                    }


                    //wxCompilerConsole1.StartCompiler(false);
                    //If we use WinXound flags, we also print the csound flags in the text
                    wxCompilerConsole1.StartCompiler(MenuToolsUseDefaultFlags.Checked);

                    /*
                    //Redefine default SFDIR directory (only if SFDIR is not defined)
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.SFDIR) ||
                        wxGlobal.Settings.Directory.UseSFDIR == false)
                    {
                        System.Environment.SetEnvironmentVariable("SFDIR",
                            wxGlobal.Settings.Directory.SFDIR);
                    }
                    */

                }


                //COMPILE WITH PYTHON
                else if (ActiveEditor.FileName.ToLower().EndsWith(".py") ||
                         ActiveEditor.FileName.ToLower().EndsWith(".pyw"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.PythonConsolePath) ||
                        !File.Exists(wxGlobal.Settings.Directory.PythonConsolePath))
                    {
                        MessageBox.Show("Cannot find Python compiler!" + newline +
                                        "Please select a valid path in File->Settings->Python.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }


                    //Show and start the Compiler Console
                    MenuViewShowCompiler_Click(null, null);

                    //Reset mCompilerErrorLine to 0
                    mCompilerErrorLine = 0;
                    mCompilerEditor = ActiveEditor;

                    wxCompilerConsole1.Title = ActiveEditor.FileName;
                    string mArguments = wxGlobal.Settings.General.PythonDefaultFlags.Trim();
                    if (string.IsNullOrEmpty(mArguments))
                        mArguments = "-u";
                    wxCompilerConsole1.Filename = wxGlobal.Settings.Directory.PythonConsolePath;
                    wxCompilerConsole1.Arguments =
                        mArguments + " " + "\"" + TextEditorFileName + "\"";
                    //With Python we send the temporary files to the filename directory
                    wxCompilerConsole1.WorkingDirectory =
                        Directory.GetParent(ActiveEditor.FileName).ToString();
                    wxCompilerConsole1.StartCompiler(false);

                }


                //COMPILE WITH LUA
                else if (ActiveEditor.FileName.ToLower().EndsWith(".lua"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.LuaConsolePath) ||
                        !File.Exists(wxGlobal.Settings.Directory.LuaConsolePath))
                    {
                        MessageBox.Show("Cannot find Lua compiler!" + newline +
                                        "Please select a valid path in File->Settings->Lua.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }


                    //Show and start the Compiler Console
                    MenuViewShowCompiler_Click(null, null);

                    //Reset mCompilerErrorLine to 0
                    mCompilerErrorLine = 0;
                    mCompilerEditor = ActiveEditor;

                    wxCompilerConsole1.Title = ActiveEditor.FileName;
                    string mArguments = wxGlobal.Settings.General.LuaDefaultFlags.Trim();
                    if (string.IsNullOrEmpty(mArguments))
                        mArguments = "";
                    wxCompilerConsole1.Filename = wxGlobal.Settings.Directory.LuaConsolePath;
                    wxCompilerConsole1.Arguments =
                        mArguments + " " + "\"" + TextEditorFileName + "\"";
                    //We send the temporary files to the filename directory
                    wxCompilerConsole1.WorkingDirectory =
                        Directory.GetParent(ActiveEditor.FileName).ToString();
                    wxCompilerConsole1.StartCompiler(false);

                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsCompile_Click Error");
            }

        }

        private string CheckForAdditionalFlags(string additionalParams)
        {
            //Check for additional parameters 
            //[*=current editor filename
            //[?=ask for filename]
            if (additionalParams.Contains("*"))
            {
                if (ActiveEditor != null)
                {
                    additionalParams =
                            additionalParams.Replace(
                            "*", Path.GetFileNameWithoutExtension(ActiveEditor.FileName));
                }
            }
            else if (additionalParams.Contains("?"))
            {
                //Open document 
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName =
                    Path.GetFileNameWithoutExtension(ActiveEditor.FileName);

                if (!(saveFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    return "[CANCEL]";
                }
                else
                {
                    additionalParams =
                        additionalParams.Replace("?",   //-o"?.wav",
                                                 saveFileDialog1.FileName);
                }
            }

            return additionalParams;
        }

        private void MenuToolsExternalCompile_Click(object sender, EventArgs e)
        {
            CompileExternal("");
        }

        private void CompileExternal(string additionalParams)
        {
            if (ActiveEditor == null) return;

            //Save file before to compile and retrieve the filename
            string TextEditorFileName = SaveBeforeCompile();


            try
            {
                //COMPILE WITH CSOUND
                if (ActiveEditor.FileName.ToLower().EndsWith(".csd") ||
                    ActiveEditor.FileName.ToLower().EndsWith(".orc") ||
                    ActiveEditor.FileName.ToLower().EndsWith(".sco"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundConsole) ||
                          !File.Exists(wxGlobal.Settings.Directory.CSoundConsole))
                    {
                        MessageBox.Show("Cannot find CSound Console compiler!" + newline +
                                        "Please select a valid path in File->Settings->CSound.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }


                    additionalParams = CheckForAdditionalFlags(additionalParams);
                    if (additionalParams == "[CANCEL]") return;


                    string flags = GetCSoundFlags("");

                    if (ActiveEditor.FileName.ToLower().EndsWith(".orc"))
                    {
                        mCompiler.ExternalCompiler(
                            TextEditorFileName, //ActiveEditor.FileName,
                            ActiveEditor.GetCurrentOrcScoFile(),
                            wxGlobal.Settings.Directory.CSoundConsole,
                            flags + " " + additionalParams.Trim());
                    }
                    else if (ActiveEditor.FileName.ToLower().EndsWith(".sco"))
                    {
                        mCompiler.ExternalCompiler(
                            ActiveEditor.GetCurrentOrcScoFile(),
                            TextEditorFileName, //ActiveEditor.FileName,
                            wxGlobal.Settings.Directory.CSoundConsole,
                            flags + " " + additionalParams.Trim());
                    }
                    else
                    {
                        mCompiler.ExternalCompiler(TextEditorFileName, //ActiveEditor.FileName,
                                                   wxGlobal.Settings.Directory.CSoundConsole,
                                                   flags + " " + additionalParams.Trim());
                    }

                }


                //COMPILE WITH PYTHON
                else if (ActiveEditor.FileName.ToLower().EndsWith(".py") ||
                         ActiveEditor.FileName.ToLower().EndsWith(".pyw"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.PythonConsolePath) ||
                        !File.Exists(wxGlobal.Settings.Directory.PythonConsolePath))
                    {
                        MessageBox.Show("Cannot find Python compiler!" + newline +
                                        "Please select a valid path in File->Settings->Python.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }

                    mCompiler.ExternalCompiler(TextEditorFileName, //ActiveEditor.FileName,
                                               wxGlobal.Settings.Directory.PythonConsolePath,
                                               wxGlobal.Settings.General.PythonDefaultFlags);
                }


                //COMPILE WITH LUA
                else if (ActiveEditor.FileName.ToLower().EndsWith(".lua"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.LuaConsolePath) ||
                        !File.Exists(wxGlobal.Settings.Directory.LuaConsolePath))
                    {
                        MessageBox.Show("Cannot find Lua compiler!" + newline +
                                        "Please select a valid path in File->Settings->Lua.exe",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }

                    mCompiler.ExternalCompiler(TextEditorFileName, //ActiveEditor.FileName,
                                               wxGlobal.Settings.Directory.LuaConsolePath,
                                               wxGlobal.Settings.General.LuaDefaultFlags);
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsExternalCompile_Click Error");
            }

        }

        private void MenuToolsRunExternalEditor_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            //Save file before to compile
            SaveBeforeCompile();

            if (FileIsUntitled(ActiveEditor) ||
                !File.Exists(ActiveEditor.FileName))
            {
                MessageBox.Show("Please specify a valid filename before to call the External GUI.",
                                "External GUI Error - Invalid filename",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                //Compile with CSOUND EXTERNAL (QuteCSound)
                if (ActiveEditor.FileName.ToLower().EndsWith(".csd"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.Winsound) ||
                        !File.Exists(wxGlobal.Settings.Directory.Winsound))
                    {
                        MessageBox.Show("Cannot find CSound External GUI Editor!" + newline +
                                        "Please select a valid path in File->Settings->CSound External GUI Editor.",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }

                    mCompiler.WinsoundCompiler(ActiveEditor.FileName,
                               wxGlobal.Settings.Directory.Winsound);
                }

                //Compile with PYTHON External (IDLE)
                else if (ActiveEditor.FileName.ToLower().EndsWith(".py") ||
                         ActiveEditor.FileName.ToLower().EndsWith(".pyw"))
                {

                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.PythonIdlePath) ||
                        !File.Exists(wxGlobal.Settings.Directory.PythonIdlePath))
                    {
                        MessageBox.Show("Cannot find Python Idle (or other GUI) Editor!" + newline +
                                        "Please select a valid path in File->Settings->Python Idle.",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }

                    System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.PythonIdlePath,
                                                     "\"" + ActiveEditor.FileName + "\"");
                }

                //Compile with LUA External
                else if (ActiveEditor.FileName.ToLower().EndsWith(".lua"))
                {
                    if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.LuaGuiPath) ||
                        !File.Exists(wxGlobal.Settings.Directory.LuaGuiPath))
                    {
                        MessageBox.Show("Cannot find Lua External GUI Editor!" + newline +
                                        "Please select a valid path in File->Settings->Lua External GUI.",
                                        "Compiler error!",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                        return;
                    }

                    System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.LuaGuiPath,
                                                     "\"" + ActiveEditor.FileName + "\"");

                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsRunExternalEditor_Click Error");
            }

        }

        private void MenuToolsRunCSoundAV_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundAV) ||
                !File.Exists(wxGlobal.Settings.Directory.CSoundAV))
            {
                MessageBox.Show("Cannot find CSoundAV compiler!" + newline +
                                "Please select a valid path in File->Settings->CSoundAV",
                                "Compiler error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                return;
            }

            if (ActiveEditor == null) return;

            //Save file before to compile
            SaveBeforeCompile();

            if (!File.Exists(ActiveEditor.FileName))
            {
                MessageBox.Show("Please specify a valid filename before to compile.",
                                "Compiler Error - Invalid filename",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Compile with CSOUNDAV (Only csd files)
            if (ActiveEditor.FileName.ToLower().EndsWith(".csd") ||
                ActiveEditor.FileName.ToLower().EndsWith(".orc") ||
                ActiveEditor.FileName.ToLower().EndsWith(".sco"))
            {
                mCompiler.CSoundAVCompiler(ActiveEditor.FileName,
                                           wxGlobal.Settings.Directory.CSoundAV);
            }
        }



        private void MenuFileSettings_Click(object sender, EventArgs e)
        {
            FormSettings mForm = new FormSettings();

            if (mForm.ShowDialog(this) == DialogResult.OK)
            {
                if (wxTabCode.TabCount > 0)
                {
                    wxEditor tempEditor = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        try
                        {
                            tempEditor = (wxEditor)tp.Controls[0];

                            tempEditor.textEditor.TextEditorFont =
                                new Font(wxGlobal.Settings.EditorProperties.DefaultFontName,
                                         wxGlobal.Settings.EditorProperties.DefaultFontSize);
                            tempEditor.textEditor.TabIndent =
                                wxGlobal.Settings.EditorProperties.DefaultTabSize;
                            tempEditor.textEditor.ShowMatchingBracket =
                                wxGlobal.Settings.EditorProperties.ShowMatchingBracket;
                            tempEditor.textEditor.ShowVerticalRuler =
                                wxGlobal.Settings.EditorProperties.ShowVerticalRuler;
                            tempEditor.textEditor.MarkCaretLine =
                                wxGlobal.Settings.EditorProperties.MarkCaretLine;

                            if (tempEditor.FileName.ToLower().EndsWith(".py") ||
                                tempEditor.FileName.ToLower().EndsWith(".pyw"))
                            {
                                //Python syntax
                                if (wxGlobal.Settings.EditorProperties.UseMixedPython)
                                {
                                    tempEditor.SetHighlight("winxoundpython", "");
                                    tempEditor.ConfigureEditorForPythonMixed(null);
                                }
                                else
                                {
                                    tempEditor.SetHighlight("python", "");
                                    tempEditor.ConfigureEditorForPython();
                                }
                                //Refresh syntax
                                tempEditor.textEditor.PrimaryView.Colourise(0, -1);
                                tempEditor.textEditor.SecondaryView.Colourise(0, -1);

                            }
                            else if (tempEditor.FileName.ToLower().EndsWith(".lua"))
                            {
                                //Lua Syntax
                                tempEditor.ConfigureEditorForLua();
                            }
                            else if (tempEditor.FileName.ToLower().EndsWith(".csd"))
                            {
                                //CSoundSyntax
                                tempEditor.ConfigureEditorForCSound(null);
                            }
                            else
                                tempEditor.ConfigureEditorForText();

                            tempEditor.RefreshExplorer();
                        }

                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "FormMain - MenuFileSettings_Click: " + ex.Message);
                        }
                    }
                }

                //Set toolbar and menu settings
                ConfigureToolBar();
                editor_FontHasChanged(null, null);

                //Set environment variables
                wxGlobal.Settings.SetEnvironmentVariables();

                try
                {
                    wxCompilerConsole1.SetCompilerFont(
                            new Font(wxGlobal.Settings.General.CompilerFontName,
                                     wxGlobal.Settings.General.CompilerFontSize));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "FormMain - MenuFileSettings_Click - SetCompilerFont: " +
                        ex.Message);
                }

                mForm.Dispose();
                if (ActiveEditor != null) ActiveEditor.SetFocus();
            }

            mForm.Dispose();
        }

        private void MenuFileExit_Click(object sender, EventArgs e)
        {
            //EXIT PROGRAM
            this.Close();
        }

        private void MenuEditFind_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                if (mFormFindAndReplace == null)
                {
                    mFormFindAndReplace = new FormFindAndReplace();
                    AddEventForFormFindAndReplace();
                }

                if (!string.IsNullOrEmpty(ActiveEditor.textEditor.GetSelectedText()))
                {
                    mFormFindAndReplace.TextBoxFind.Text =
                        ActiveEditor.textEditor.GetSelectedText();
                }
                else mFormFindAndReplace.TextBoxFind.Text = wxGlobal.LastWordSearch;

                if (mFormFindAndReplace.Visible == false)
                    mFormFindAndReplace.Show(this);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditFind_Click Error");
            }

        }

        private void MenuEditFindLine_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                if (mFindLine == null)
                {
                    mFindLine = new FormFindLine();
                    mFindLine.ButtonFindClick +=
                        new FormFindLine.OnButtonFind(mFindLine_ButtonFindClick);
                }

                mFindLine.SetMaxLineNumbers(ActiveEditor.textEditor.GetLines());
                mFindLine.Focus();
                if (mFindLine.Visible == false)
                    mFindLine.ShowDialog(this);

                ActiveEditor.SetFocus();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditFindLine_Click Error");
            }
        }

        void mFindLine_ButtonFindClick(int linenumber)
        {
            if (ActiveEditor == null) return;

            try
            {
                ActiveEditor.textEditor.GoToLine(linenumber);
                ActiveEditor.textEditor.SelectLine(linenumber);
                ActiveEditor.textEditor.SetFocus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - mFindLine_ButtonFindClick Error");
            }
        }

        private void MenuToolsWinXoundPlayer_Click(object sender, EventArgs e)
        {
            try
            {
                if (wxGlobal.Settings.General.DefaultWavePlayer == 1)
                {
                    if (File.Exists(wxGlobal.LastWaveFile))
                    {
                        System.Diagnostics.Process.Start("wmplayer", "\"" + wxGlobal.LastWaveFile + "\"");
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("wmplayer");
                    }
                }
                else
                {
                    mPlayer.ShowDialog(this);
                }
            }
            catch
            {
                MessageBox.Show("Cannot find a valid sound player in your Operating System!",
                                "WinXound error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void MenuToolsWavePlayer_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WaveEditor))
                {
                    if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
                    {
                        System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.WaveEditor);
                    }
                    else
                    {
                        MessageBox.Show("Wave Editor Path not found!" + newline +
                            "Please select a valid path in File->Settings->Wave Editor Executable",
                            "WinXound error!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot find External Wave Editor Path!" + newline +
                        "Please select a valid path in File->Settings->Wave Editor executable",
                        "WinXound error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuToolsWavePlayer_Click");
            }
        }

        private void MenuToolsCalc_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("calc.exe");
            }
            catch
            {
                wxGlobal.wxMessageError("Unable to find OS Calculator",
                                        "MenuToolsCalc_Click - Error");
            }
        }

        private void MenuToolsCommandLine_Click(object sender, EventArgs e)
        {
            string arg = "";
            bool mErr = false;

            try
            {
                //Set arguments 
                if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundConsole))
                {
                    Int32 mPos = 0;
                    mPos = wxGlobal.Settings.Directory.CSoundConsole.LastIndexOf("\\");
                    arg = "/K " +
                        wxGlobal.Settings.Directory.CSoundConsole.Substring(0, 2) +
                        " && cd " + "\"" +
                        wxGlobal.Settings.Directory.CSoundConsole.Substring(0, mPos) + "\"";
                }
                //'this.Text = arg 
                else if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.Winsound))
                {
                    Int32 mPos = 0;
                    mPos = wxGlobal.Settings.Directory.Winsound.LastIndexOf("\\");
                    arg = "/K " + wxGlobal.Settings.Directory.Winsound.Substring(0, 2) +
                          " && cd " + "\"" +
                          wxGlobal.Settings.Directory.Winsound.Substring(0, mPos) + "\"";
                }
                //'this.Text = arg 
                else if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundAV))
                {
                    Int32 mPos = default(Int32);
                    mPos = wxGlobal.Settings.Directory.CSoundAV.LastIndexOf("\\");
                    arg = "/K " + wxGlobal.Settings.Directory.CSoundAV.Substring(0, 2) +
                          " && cd " + "\"" +
                          wxGlobal.Settings.Directory.CSoundAV.Substring(0, mPos) + "\"";
                }
                //'this.Text = arg 
                else
                {
                    arg = "/K cd\\";
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsCommandLine_Click");
            }

            //Start cmd.exe 
            try
            {
                System.Diagnostics.Process.Start("cmd.exe", arg);
                mErr = false;
            }
            catch
            {
                mErr = true;
            }

            //If it fault launch command.com 
            if (mErr == true)
            {
                try
                {
                    System.Diagnostics.Process.Start("command.com", arg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                                    "Unable to open command line window!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }

        private void MenuToolsCSoundAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Form form in this.OwnedForms)
                {
                    if (form.Name == "FormAnalysis")
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.Activate();
                        return;
                    }
                }

                FormAnalysis mFormAnalysis = new FormAnalysis();
                mFormAnalysis.Show(this);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsCSoundAnalysis_Click");
            }
        }

        private void MenuInfoCSoundAVHelp_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundAVHelp))
                {
                    //System.Diagnostics.Process.Start(
                    //    wxGlobal.Settings.Directory.CSoundAVHelp);
                    HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundAVHelp);
                    MenuViewShowHelp_Click(null, null);
                }
                else
                {
                    MessageBox.Show("Cannot find CSoundAV Help Path!" + newline +
                            "Please select a valid path in File->Settings->CSoundAV Help",
                            "Compiler error!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuInfoCSoundAVHelp_Click");
            }
        }




        private void MenuViewFullCode_Click(object sender, EventArgs e)
        {
            ShowFullCode();
        }

        private void MenuViewSplitHorizontal_Click(object sender, EventArgs e)
        {
            SplitHorizontal();
        }

        private void MenuViewSplitHorizontalOrcSco_Click(object sender, EventArgs e)
        {
            SplitHorizontal();
            ShowOrcSco();
        }

        private void MenuViewSplitVertical_Click(object sender, EventArgs e)
        {
            SplitVertical();
        }

        private void MenuViewSplitVerticalOrcSco_Click(object sender, EventArgs e)
        {
            SplitVertical();
            ShowOrcSco();
        }

        private void ShowFullCode()
        {
            try
            {
                if (ActiveEditor != null)
                {
                    ActiveEditor.SetFocusOnPrimaryView();
                    if (ActiveEditor.textEditor.Splitted == true)
                    {
                        ActiveEditor.textEditor.RemoveSplit();
                    }
                    ActiveEditor.SetFocus();
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - ShowFullCode");
            }
        }

        private void SplitHorizontal()
        {
            try
            {
                if (ActiveEditor != null)
                {
                    if (ActiveEditor.textEditor.Splitted == true) ShowFullCode();
                    ActiveEditor.textEditor.Split();
                    if (ActiveEditor.textEditor.Splitted == true)
                        ActiveEditor.textEditor.SetFirstVisibleLine(
                            ActiveEditor.textEditor.GetFirstVisibleLine(1), 2);
                    ActiveEditor.SetFocusOnPrimaryView();
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - SplitHorizontal");
            }
        }

        private void SplitVertical()
        {
            try
            {
                if (ActiveEditor != null)
                {
                    if (ActiveEditor.textEditor.Splitted == true) ShowFullCode();
                    ActiveEditor.textEditor.SplitVertical();
                    if (ActiveEditor.textEditor.Splitted == true)
                        ActiveEditor.textEditor.SetFirstVisibleLine(
                            ActiveEditor.textEditor.GetFirstVisibleLine(1), 2);
                    ActiveEditor.SetFocusOnPrimaryView();
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - SplitVertical");
            }
        }


        private void ShowOrcSco()
        {
            try
            {
                if (ActiveEditor == null) return;
                if (!ActiveEditor.textEditor.Splitted) return;

                Int32 mFindPos = -1;
                string StringToFind = null;

                //Find <CsInstruments> 
                StringToFind = "<CsInstruments>";
                mFindPos = ActiveEditor.textEditor.FindText(
                                                 StringToFind, true, true, false,
                                                 false, false, true,
                                                 0, ActiveEditor.textEditor.GetTextLength());
                if (mFindPos > -1)
                {
                    ActiveEditor.SetFocusOnPrimaryView();
                    ActiveEditor.textEditor.SetCaretPosition(mFindPos);
                    ActiveEditor.textEditor.SetFirstVisibleLine(
                        ActiveEditor.textEditor.GetLineNumberFromPosition(mFindPos), 1);
                }

                //Find <CsScore> 
                StringToFind = "<CsScore>";
                mFindPos = ActiveEditor.textEditor.FindText(
                                         StringToFind, true, true, false,
                                         false, false, true,
                                         0, ActiveEditor.textEditor.GetTextLength());

                if (mFindPos > -1)
                {
                    ActiveEditor.SetFocusOnSecondaryView();
                    ActiveEditor.textEditor.SetCaretPosition(mFindPos);
                    ActiveEditor.textEditor.SetFirstVisibleLine(
                        ActiveEditor.textEditor.GetLineNumberFromPosition(mFindPos), 2);
                }

                ActiveEditor.SetFocusOnPrimaryView();
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - ShowOrcSco");
            }

        }
        //FINE DA INSERIRE IN wxEditor



        private void MenuViewWindowUP_Click(object sender, EventArgs e)
        {
            try
            {
                Rectangle mRect = Screen.GetWorkingArea(this.Bounds);
                this.WindowState = FormWindowState.Normal;
                this.Bounds = new Rectangle(0, 0, mRect.Width, mRect.Height / 2);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuViewWindowUp_Click");
            }
        }

        private void MenuViewWindowDOWN_Click(object sender, EventArgs e)
        {
            try
            {
                Rectangle mRect = Screen.GetWorkingArea(this.Bounds);
                this.WindowState = FormWindowState.Normal;
                this.Bounds = new Rectangle(0, mRect.Height / 2, mRect.Width, mRect.Height / 2);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuViewWindowDOWN_Click");
            }
        }

        private void MenuViewWindowLEFT_Click(object sender, EventArgs e)
        {
            try
            {
                Rectangle mRect = Screen.GetWorkingArea(this.Bounds);
                this.WindowState = FormWindowState.Normal;
                this.Bounds = new Rectangle(0, 0, mRect.Width / 2, mRect.Height);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuViewWindowLEFT_Click");
            }
        }

        private void MenuViewWindowRIGHT_Click(object sender, EventArgs e)
        {
            try
            {
                Rectangle mRect = Screen.GetWorkingArea(this.Bounds);
                this.WindowState = FormWindowState.Normal;
                this.Bounds = new Rectangle(mRect.Width / 2, 0, mRect.Width / 2, mRect.Height);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - MenuViewWindowRIGHT_Click");
            }
        }

        private void MenuViewWindowReset_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            Rectangle mScreen = Screen.GetWorkingArea(this.Location);

            if (mScreen.Contains(wxGlobal.Settings.General.WindowPosition))
                this.Location = wxGlobal.Settings.General.WindowPosition;
            else
            {
                //this.Location = new Point(0, 0);
                //Center on screen
                this.Location =
                    new Point((mScreen.Width / 2) - (wxGlobal.Settings.General.WindowSize.X / 2),
                              (mScreen.Height / 2) - (wxGlobal.Settings.General.WindowSize.Y / 2));
            }

            this.Width = wxGlobal.Settings.General.WindowSize.X;
            this.Height = wxGlobal.Settings.General.WindowSize.Y;

        }



        private void MenuViewFocus1_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            try
            {
                RestoreAllKeys();
                ActiveEditor.SetFocusOnPrimaryView();
            }
            catch { }
        }

        private void MenuViewFocus2_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            try
            {
                RestoreAllKeys();
                ActiveEditor.SetFocusOnSecondaryView();
            }
            catch { }
        }




        private void MenuView_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                MenuViewToolbar.Checked =
                    wxGlobal.Settings.General.ShowToolbar;
                MenuViewLineNumbers.Checked =
                    wxGlobal.Settings.EditorProperties.ShowLineNumbers;
                MenuViewExplorer.Checked =
                    wxGlobal.Settings.EditorProperties.ShowExplorer;
                MenuViewOnlineHelp.Checked =
                    wxGlobal.Settings.EditorProperties.ShowIntelliTip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuView_DropDownOpening: " + ex.Message);
            }
        }

        private void MenuViewToolbar_Click(object sender, EventArgs e)
        {
            try
            {
                wxGlobal.Settings.General.ShowToolbar =
                    !wxGlobal.Settings.General.ShowToolbar;
                ToolStrip1.Visible = wxGlobal.Settings.General.ShowToolbar;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewToolbar_Click: " + ex.Message);
            }

        }

        private void MenuViewLineNumbers_Click(object sender, EventArgs e)
        {
            try
            {
                wxGlobal.Settings.EditorProperties.ShowLineNumbers =
                    !wxGlobal.Settings.EditorProperties.ShowLineNumbers;
                if (wxTabCode.TabCount > 0)
                {
                    wxEditor temp = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        temp = (wxEditor)tp.Controls[0];
                        temp.textEditor.ShowLineNumbers =
                            wxGlobal.Settings.EditorProperties.ShowLineNumbers;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewLineNumbers_Click: " + ex.Message);
            }
        }

        private void MenuViewExplorer_Click(object sender, EventArgs e)
        {
            try
            {
                wxGlobal.Settings.EditorProperties.ShowExplorer =
                    !wxGlobal.Settings.EditorProperties.ShowExplorer;
                if (wxTabCode.TabCount > 0)
                {
                    wxEditor temp = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        temp = (wxEditor)tp.Controls[0];
                        temp.ShowExplorer = wxGlobal.Settings.EditorProperties.ShowExplorer;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewExplorer_Click: " + ex.Message);
            }
        }

        private void MenuViewOnlineHelp_Click(object sender, EventArgs e)
        {
            try
            {
                wxGlobal.Settings.EditorProperties.ShowIntelliTip =
                    !wxGlobal.Settings.EditorProperties.ShowIntelliTip;
                if (wxTabCode.TabCount > 0)
                {
                    wxEditor temp = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        temp = (wxEditor)tp.Controls[0];
                        temp.ShowIntelliTip = wxGlobal.Settings.EditorProperties.ShowIntelliTip;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewOnlineHelp_Click: " + ex.Message);
            }
        }

        private void MenuViewShowAll_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStrip1.Visible = true;
                wxGlobal.Settings.General.ShowToolbar = true;
                wxGlobal.Settings.EditorProperties.ShowLineNumbers = true;
                wxGlobal.Settings.EditorProperties.ShowExplorer = true;
                wxGlobal.Settings.EditorProperties.ShowIntelliTip = true;

                if (wxTabCode.TabCount > 0)
                {
                    wxEditor temp = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        temp = (wxEditor)tp.Controls[0];
                        temp.ShowIntelliTip = true;
                        temp.ShowExplorer = true;
                        temp.textEditor.ShowLineNumbers = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewShowAll_Click: " + ex.Message);
            }
        }

        private void MenuViewHideAll_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStrip1.Visible = false;
                wxGlobal.Settings.General.ShowToolbar = false;
                wxGlobal.Settings.EditorProperties.ShowLineNumbers = false;
                wxGlobal.Settings.EditorProperties.ShowExplorer = false;
                wxGlobal.Settings.EditorProperties.ShowIntelliTip = false;

                if (wxTabCode.TabCount > 0)
                {
                    wxEditor temp = null;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        temp = (wxEditor)tp.Controls[0];
                        temp.ShowIntelliTip = false;
                        temp.ShowExplorer = false;
                        temp.textEditor.ShowLineNumbers = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewHideAll_Click: " + ex.Message);
            }
        }





        private void MenuEditComment_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.textEditor.Comment();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuEditComment_Click: " + ex.Message);
            }
        }

        private void MenuEditRemoveComment_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.textEditor.UnComment();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuEditRemoveComment_Click: " + ex.Message);
            }
        }

        private void MenuInsertBookmark_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.InsertRemoveBookmark();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuInsertBookmark_Click: " + ex.Message);
            }
        }

        private void MenuRemoveAllBookmark_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.RemoveAllBookmarks();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuRemoveAllBookmark_Click: " + ex.Message);
            }
        }

        private void MenuInsertGoToNextBookmark_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.GoToNextBookmark();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuInsertGoToNextBookmark_Click: " + ex.Message);
            }
        }

        private void MenuInsertGoToPreviousBookmark_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.GoToPreviousBookmark();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuInsertGoToPreviousBookmark_Click: " + ex.Message);
            }
        }

        private void MenuViewPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.GoToPreviousPos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewPrevious_Click: " + ex.Message);
            }
        }

        private void MenuViewNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor != null) ActiveEditor.GoToNextPos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - MenuViewNext_Click: " + ex.Message);
            }
        }

        private void ToolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "ToolStripCharDown":
                    try
                    {
                        if (ActiveEditor != null)
                        {
                            if ((int)ActiveEditor.textEditor.TextEditorFont.Size > 6)
                                ActiveEditor.textEditor.TextEditorFont =
                                    new Font(ActiveEditor.textEditor.TextEditorFont.Name,
                                             ActiveEditor.textEditor.TextEditorFont.Size - 1);
                            editor_FontHasChanged(ActiveEditor, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripCharDown: "
                            + ex.Message);
                    }
                    break;


                case "ToolStripCharUp":
                    try
                    {
                        if (ActiveEditor != null)
                        {
                            if ((int)ActiveEditor.textEditor.TextEditorFont.Size > 5)
                                ActiveEditor.textEditor.TextEditorFont =
                                    new Font(ActiveEditor.textEditor.TextEditorFont.Name,
                                             ActiveEditor.textEditor.TextEditorFont.Size + 1);
                            editor_FontHasChanged(ActiveEditor, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripCharUp: "
                            + ex.Message);
                    }
                    break;


                case "ToolStripOpcodeHelp":
                    try
                    {
                        if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
                        {
                            HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                                                 "\\PartReference.html");
                        }

                        MenuViewShowHelp_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripOpcodeHelp: "
                            + ex.Message);
                    }
                    break;


                case "ToolStripFlagsHelp":
                    try
                    {
                        if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
                        {
                            HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                                                "\\CommandFlags.html");
                        }

                        MenuViewShowHelp_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripFlagsHelp: "
                            + ex.Message);
                    }
                    break;


                case "ToolStripHome":
                    try
                    {
                        if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WorkingDir))
                        {
                            mWorkingDir = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripHome: "
                            + ex.Message);
                    }
                    break;

                /*
                case "tsCode":
                    try
                    {
                        tsCode.ShowDropDown();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - tsCode: "
                            + ex.Message);
                    }
                    break;
                */


                case "ToolStripButtonFontSize":
                    try
                    {
                        ToolStripButtonFontSize.ShowDropDown();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - ToolStrip1_ItemClicked - ToolStripButtonFontSize: "
                            + ex.Message);
                    }
                    break;

                /*
                case "ToolStripCSoundConsole":
                    //Without CTRL key we call Internal Compiler
                    if (ModifierKeys != Keys.Control)
                        MenuToolsCompile.PerformClick();
                    //With CTRL key we call External Compiler
                    else MenuToolsExternalCompile.PerformClick();
                    break;
                */

                case "ToolStripExternalGUIEditor":
                    MenuToolsRunExternalEditor.PerformClick();
                    break;

            }
        }

        private void MenuFilePageSetup_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;
                PageSetupDialog mPrinterSetup = new PageSetupDialog();
                mPrinterSetup.Document = ActiveEditor.GetPrintDocument();
                mPrinterSetup.EnableMetric = true;
                mPrinterSetup.ShowDialog();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuFilePrintPreview_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            if (ActiveEditor.textEditor.GetTextLength() > 0)
            {
                try
                {
                    PrintPreviewDialog mPreview = new PrintPreviewDialog();
                    mPreview.Document = ActiveEditor.GetPrintDocument();
                    mPreview.ShowDialog();
                    mPreview.Focus();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void MenuFilePrint_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            if (ActiveEditor.textEditor.GetTextLength() > 0)
            {
                try
                {
                    PrintDialog mPrinterDialog = new PrintDialog();
                    mPrinterDialog.Document = ActiveEditor.GetPrintDocument();
                    //mPrinterDialog.UseEXDialog = true;
                    if (mPrinterDialog.ShowDialog() == DialogResult.OK)
                    {
                        ActiveEditor.GetPrintDocument().Print();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }



        private void MenuFileImportOrcSco_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                //CSound Files (*.csd;*.orc;*.sco)|*.csd;*.orc;*.sco
                openFileDialog1.Filter = "CSound Orc Files (*.orc;*.sco)|*.orc;*.sco";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                if (mWorkingDir)
                {
                    openFileDialog1.InitialDirectory = wxGlobal.Settings.Directory.WorkingDir;
                    mWorkingDir = false;
                }

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                      string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    if (ActiveEditor != null) ActiveEditor.SetFocus();
                    return;
                }

                if (ActiveEditor != null &&
                    (sender as ToolStripMenuItem).Name.ToLower().Contains("existing"))
                {
                    wxImportExport mImport =
                        new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                    //string tempText = ActiveEditor.textEditor.GetText();

                    mImport.ImportORC(ActiveEditor.textEditor, openFileDialog1.FileName);
                    mImport.ImportSCO(ActiveEditor.textEditor,
                                      Path.ChangeExtension(openFileDialog1.FileName, ".sco"));
                }
                else
                    AddNewEditor(openFileDialog1.FileName, "", true);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileImportOrcSco_Click");
            }
        }

        private void MenuFileImportOrc_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.Filter = "CSound Orc Files (*.orc)|*.orc|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                if (mWorkingDir)
                {
                    openFileDialog1.InitialDirectory = wxGlobal.Settings.Directory.WorkingDir;
                    mWorkingDir = false;
                }

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                      string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    if (ActiveEditor != null) ActiveEditor.SetFocus();
                    return;
                }

                //If we have an already opened document, WinXound will import the ORC File
                //in the CsInstrument section
                if (ActiveEditor != null &&
                    (sender as ToolStripMenuItem).Name.ToLower().Contains("existing"))
                {

                    wxImportExport mImport =
                        new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                    //string tempText = ActiveEditor.textEditor.GetText();
                    mImport.ImportORC(ActiveEditor.textEditor, openFileDialog1.FileName);
                    //ActiveEditor.textEditor.SetText(tempText);

                }
                //else add a new document
                else
                    AddNewEditor(openFileDialog1.FileName, "", true);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileImportOrc_Click");
            }

        }

        private void MenuFileImportSco_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.Filter = "CSound Sco Files (*.sco)|*.sco|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                if (mWorkingDir)
                {
                    openFileDialog1.InitialDirectory = wxGlobal.Settings.Directory.WorkingDir;
                    mWorkingDir = false;
                }

                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                      string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    if (ActiveEditor != null) ActiveEditor.SetFocus();
                    return;
                }

                //If we have an already opened document, WinXound will import the SCO File
                //in the CsScore section
                if (ActiveEditor != null &&
                    (sender as ToolStripMenuItem).Name.ToLower().Contains("existing"))
                {
                    wxImportExport mImport =
                        new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                    //string tempText = ActiveEditor.textEditor.GetText();
                    mImport.ImportSCO(ActiveEditor.textEditor, openFileDialog1.FileName);
                    //ActiveEditor.textEditor.SetText(tempText);
                }
                //else add a new document
                else
                    AddNewEditor(openFileDialog1.FileName, "", true);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileImportSco_Click");
            }
        }

        private void MenuFileExportOrcSco_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;

                if (string.IsNullOrEmpty(ActiveEditor.textEditor.GetText()))
                {
                    MessageBox.Show("No available text to export!", "Error",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                wxImportExport mExport =
                    new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                mExport.ExportOrcSco(ActiveEditor.textEditor.GetText(),
                                     ActiveEditor.FileName);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileExportOrcSco_Click");
            }
        }

        private void MenuFileExportOrc_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;

                if (string.IsNullOrEmpty(ActiveEditor.textEditor.GetText()))
                {
                    MessageBox.Show("No available text to export!", "Error",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                wxImportExport mExport =
                    new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                mExport.ExportORC(ActiveEditor.textEditor.GetText(),
                                  ActiveEditor.FileName);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileExportOrc_Click");
            }
        }

        private void MenuFileExportSco_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;

                if (string.IsNullOrEmpty(ActiveEditor.textEditor.GetText()))
                {
                    MessageBox.Show("No available text to export!", "Error",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                wxImportExport mExport =
                    new wxImportExport(wxGlobal.Settings.Directory.WorkingDir);

                mExport.ExportSCO(ActiveEditor.textEditor.GetText(),
                                  ActiveEditor.FileName);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileExportSco_Click");
            }
        }




        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {

            //Save last session filenames
            wxGlobal.Settings.LastSessionFilesClear();
            foreach (TabPage tp in wxTabCode.TabPages)
            {
                wxEditor temp = (wxEditor)tp.Controls[0];
                if (!String.IsNullOrEmpty(temp.FileName))
                {
                    wxGlobal.Settings.LastSessionFilesInsert(temp.FileName);
                }
            }


            //Check document modifications 
            if (RemoveAllEditors() == false)
            {
                e.Cancel = true;
                return;
            }


            try
            {
                //Check and close running processes 
                wxCompilerConsole1.StopCompiler();
            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message,
                //    "FormMain_FormClosing - Compiler CloseProcess Error");
                System.Diagnostics.Debug.WriteLine(
                    "FormMain - FormClosing Error (Stop Compiler): " + ex.Message);
            }


            try
            {
                //Check and close the WavePlayer 
                mPlayer.ClosePlayer();
            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message,
                //    "FormMain_FormClosing - Compiler CloseProcess Error");
                System.Diagnostics.Debug.WriteLine(
                    "FormMain - FormClosing Error (FormPlayer close): " + ex.Message);
            }


            try
            {
                //Check for Cabbage Named Pipe connection
                if (mPipeClient != null)
                    //if (mPipeClient.Connected)
                    mPipeClient.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                   "FormMain - FormClosing Error (Cabbage disconnection): " + ex.Message);
            }


            //Set Window state preferences
            wxGlobal.Settings.General.WindowState = this.WindowState;
            wxGlobal.Settings.General.WindowSize = (Point)this.Size;
            wxGlobal.Settings.General.WindowPosition = this.Location;
            wxGlobal.Settings.General.UseWinXoundFlags = MenuToolsUseDefaultFlags.Checked;
            //Set Compiler window position and size
            wxGlobal.Settings.General.CompilerWindowSize = (Point)compilerWindow.Size;
            wxGlobal.Settings.General.CompilerWindowPosition = compilerWindow.Location;
            //Save preferences
            if (mWinXoundIsReadOnly == false)
                wxGlobal.Settings.SaveSettings();


            //Remove MenuFileRecentFiles EventHandlers
            foreach (ToolStripMenuItem tsmi in MenuFileRecentFiles.DropDownItems)
            {
                tsmi.Click -= new EventHandler(MenuRecentFile_Click);
            }


            //Search for temp files and remove them:
            //*.wav files in WinXound Directory (es. atsa.wav) 
            //Untitled temporary files (inside Settings directory)
            try
            {
                if (mWinXoundIsReadOnly == false)
                {
                    if (File.Exists(Application.StartupPath + "\\atsa_res.wav"))
                    {
                        File.Delete(Application.StartupPath + "\\atsa_res.wav");
                    }

                    string mPath = Application.StartupPath + "\\Settings\\";
                    foreach (string s in Directory.GetFiles(mPath, mWinXound_Untitled + ".*"))
                    {
                        File.Delete(s);
                    }

                    foreach (string s in Directory.GetFiles(mPath, "*.wav"))
                    {
                        File.Delete(s);
                    }
                }
            }
            catch { }
        }


        private void ShowUaeMessage(string message)
        {
            //UnauthorizedAccessException
            MessageBox.Show(
                "Windows message: " + message + newline + newline +
                "Your current user account allows read-only access to this file." + newline +
                "Generally files stored in 'Program Files' path or in " +
                "your root 'C' drive are not writable by default for " +
                "non-administrative programs." + newline + newline +
                "If you want to save changes to this file please click on 'Save As'" + newline +
                "and therefore select a folder where you have full read and write" + newline +
                "permissions." + newline + newline +
                "WinXound cannot save the file to this directory!",
                "WinXound - File Saving error - Access denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void MenuFileSave_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            if (FileIsUntitled(ActiveEditor) ||
                //string.IsNullOrEmpty(Path.GetDirectoryName(ActiveEditor.FileName)))
                !File.Exists(ActiveEditor.FileName))
            {
                MenuFileSaveAs.PerformClick();
                return;
            }

            try
            {
                ActiveEditor.SaveFile(ActiveEditor.FileName);
                ActiveEditor.textEditor.SetSavePoint();
                ActiveEditor.FileIsReadOnly = false;
            }
            catch (System.UnauthorizedAccessException uae)
            {
                ShowUaeMessage(uae.Message);
                ActiveEditor.FileIsReadOnly = true;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileSave_Click - Saving error");
            }
        }

        private void MenuFileSaveAs_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            string oldType = ActiveEditor.FileType;

            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                //saveFileDialog1.Filter = "CSound Csd File (*.csd)|*.csd|All files (*.*)|*.*";
                saveFileDialog1.Filter =
                    "Supported files (*.csd;*.orc;*.sco;*.py;*.pyw;*.lua)|*.csd;*.orc;*.sco;*.py;*.pyw;*.lua|" +
                    //"Supported files (*.csd;*.py;*.pyw;*.lua)|*.csd;*.py;*.pyw;*.lua|" +
                    "CSound Files (*.csd;*.orc;*.sco;)|*.csd;*.orc;*.sco;|" +
                    //"CSound Files (*.csd)|*.csd|" +
                    "Python files (*.py;*.pyw)|*.py;*.pyw|" +
                    "Lua files (*.lua)|*.lua|" +
                    "All files (*.*)|*.*";

                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;


                //If filename is not untitled we set the savefiledialog filename
                if (!Path.GetFileName(ActiveEditor.FileName).StartsWith(mUntitled))
                {
                    if (ActiveEditor.FileName.StartsWith(mImported))
                    {
                        saveFileDialog1.FileName =
                            Path.GetFileName(ActiveEditor.FileName.Replace(mImported, ""));
                    }
                    else
                        saveFileDialog1.FileName = Path.GetFileName(ActiveEditor.FileName);
                }


                if (mWorkingDir)
                {
                    saveFileDialog1.InitialDirectory =
                        wxGlobal.Settings.Directory.WorkingDir;
                    mWorkingDir = false;
                }

                if (!(saveFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    ActiveEditor.SetFocus();
                    return;
                }


                try
                {
                    ActiveEditor.SaveFile(saveFileDialog1.FileName);
                    ActiveEditor.textEditor.SetSavePoint();
                    ActiveEditor.FileIsReadOnly = false;
                }
                catch (System.UnauthorizedAccessException uae)
                {
                    ShowUaeMessage(uae.Message);
                    ActiveEditor.FileIsReadOnly = true;
                    return;
                }
                catch (Exception ex)
                {
                    wxGlobal.wxMessageError(ex.Message,
                        "Form Main - MenuFileSave_Click - Saving error");
                    return;
                }


                this.Text = wxGlobal.TITLE + " - " + ActiveEditor.FileName;
                ActiveEditor.Parent.Text =
                                Path.GetFileName(ActiveEditor.FileName);

                AddRecentFilesToMenu(ActiveEditor.FileName);


                //Reset highlight (if necessary)
                if (oldType != ActiveEditor.FileType)
                    SetHighlightLanguage(ActiveEditor, true);

                ActiveEditor.SetFocus();
            }
            catch (System.UnauthorizedAccessException uae)
            {
                ShowUaeMessage(uae.Message);
                ActiveEditor.FileIsReadOnly = true;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileSaveAs_Click - Saving error");
            }
        }

        private void MenuFileSaveAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (wxTabCode.TabPages.Count > 0)
                {
                    Int32 tabIndex = wxTabCode.SelectedIndex;
                    foreach (TabPage tp in wxTabCode.TabPages)
                    {
                        System.Diagnostics.Debug.WriteLine(tp.Name);
                        wxTabCode.SelectedTab = tp;
                        MenuFileSave_Click(this, null);
                    }
                    wxTabCode.SelectedIndex = tabIndex;
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuFileSaveAll_Click");
            }
        }

        private void MenuRecentFile_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;

                Int32 index = tsmi.Owner.Items.IndexOf(tsmi);

                if (File.Exists(wxGlobal.Settings.RecentFiles[index]))
                {
                    string TextEditorFileName = wxGlobal.Settings.RecentFiles[index];
                    AddNewEditor(TextEditorFileName);
                }
                else
                {
                    //Int32 tIndex = mFile.Owner.Items.IndexOf(mFile);
                    //wxGlobal.Settings.LastFiles[tIndex] = "...";
                    //MenuFileRecentFiles.DropDownItems[tIndex].Text = wxGlobal.Settings.LastFiles[tIndex];
                    //wxGlobal.Settings.SaveRecentFiles();
                    //MessageBox.Show("Unable to open file!",
                    //                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuRecentFile_Click - LastFiles loading error");
            }

        }


        private void InsertCSDTags_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActiveEditor == null) return;

                ToolStripMenuItem mi = (ToolStripMenuItem)sender;

                string mText = mi.Text.Substring(4);
                Int32 curPos = ActiveEditor.textEditor.GetCaretPosition();
                ActiveEditor.textEditor.InsertText(curPos, mText);
                ActiveEditor.textEditor.SetCaretPosition(curPos + mText.Length);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - InsertCSDTags_Click");
            }
        }

        private void MenuItemCsdTags_DropDownOpening(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            string[] StringToFind = new string[8];

            StringToFind[0] = "<CsoundSynthesizer>";
            StringToFind[1] = "</CsoundSynthesizer>";
            StringToFind[2] = "<CsOptions>";
            StringToFind[3] = "</CsOptions>";
            StringToFind[4] = "<CsInstruments>";
            StringToFind[5] = "</CsInstruments>";
            StringToFind[6] = "<CsScore>";
            StringToFind[7] = "</CsScore>";

            ArrayList mc = null;
            try
            {
                for (Int32 ciclo = 0; ciclo < 8; ciclo++)
                {
                    mc = ActiveEditor.textEditor.SearchText(
                                                 StringToFind[ciclo],
                                                 false, true, false, true);
                    if (mc.Count > 0)
                    {
                        MenuItemCsdTags.DropDown.Items[ciclo].Enabled = false;
                    }
                    else
                    {
                        MenuItemCsdTags.DropDown.Items[ciclo].Enabled = true;
                    }

                }
            }
            catch
            {
                for (Int32 ciclo = 0; ciclo < 8; ciclo++)
                {
                    MenuItemCsdTags.DropDown.Items[ciclo].Enabled = true;
                }
            }

        }


        private void wxCompilerConsole1_CompilerCompleted(string mErrorLine, string mWaveFile)
        {
            System.Diagnostics.Debug.WriteLine(
                "COMPILER CALLED - mErrorLine: " + mErrorLine +
                " - mWaveFile: " + mWaveFile);

            if (!string.IsNullOrEmpty(mErrorLine))
            {
                try
                {
                    if (mCompilerEditor == null ||
                        ActiveEditor == null) return;

                    string StringToFind = mErrorLine;
                    Int32 mFindPos = -1;

                    //mFindPos = ICSTextEditor.Text.IndexOf(StringToFind);
                    //StringToFind = String.Format("{0}{1}{0}", @"\b", StringToFind);

                    StringToFind = Regex.Escape(StringToFind);
                    MatchCollection mc = Regex.Matches(
                        mCompilerEditor.textEditor.GetText(), StringToFind);
                    StringToFind = Regex.Unescape(StringToFind);

                    //string foundedLineText = "";

                    foreach (Match p in mc)
                    {
                        //Skip remmed instruments
                        if (mCompilerEditor.textEditor.GetStyleAt(p.Index) != 2)
                        {
                            mFindPos = p.Index;
                            break;
                        }
                    }

                    if (mFindPos > -1)
                    {
                        mCompilerErrorLine =
                            mCompilerEditor.textEditor.GetLineNumberFromPosition(mFindPos);
                        //wxCompilerConsole1.ButtonFindError.Text = "&Go to error";
                        //wxCompilerConsole1.ButtonFindError.BackColor = Color.Orange;
                        //wxCompilerConsole1.ButtonFindError.Enabled = true;
                        //wxCompilerConsole1.ButtonFindError.Focus();

                        //New version - Redirecto to Error automatically
                        mCompilerEditor.textEditor.GoToLine(mCompilerErrorLine);
                        mCompilerEditor.textEditor.SelectLine(mCompilerErrorLine);
                        mCompilerEditor.Focus();
                    }
                    else
                    {
                        mCompilerErrorLine = 0;
                    }
                }
                catch (Exception ex)
                {
                    //wxGlobal.wxMessageError(ex.Message,
                    //    "Form Main - wxCompilerConsole1_CompilerCompleted - ErrorLine method");
                    System.Diagnostics.Debug.WriteLine(
                        "Form Main - wxCompilerConsole1_CompilerCompleted - ErrorLine method: " +
                        ex.Message);
                }
            }


            if (!string.IsNullOrEmpty(mWaveFile) &&
                wxGlobal.Settings.General.OpenSoundFileWith > 0)
            {
                if (File.Exists(mWaveFile))
                {
                    wxGlobal.LastWaveFile = mWaveFile;
                    try
                    {
                        if (wxGlobal.Settings.General.OpenSoundFileWith == 1)
                        {
                            if (wxGlobal.Settings.General.DefaultWavePlayer == 0)
                            {
                                mPlayer.FileName = mWaveFile;
                                mPlayer.FormPlayerShow();
                                mPlayer.ShowDialog(this);
                            }
                            else
                            {
                                System.Diagnostics.Process.Start(
                                    "wmplayer", "\"" + mWaveFile + "\"");
                            }
                        }
                        else
                        {
                            if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
                            {
                                System.Diagnostics.Process.Start(
                                    wxGlobal.Settings.Directory.WaveEditor,
                                    "\"" + mWaveFile + "\"");
                            }
                            else
                            {
                                MessageBox.Show("Wave Editor Path not found!" + newline +
                                    "Please select a valid path in File->Settings->Wave Editor Executable",
                                    "WinXound error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        wxGlobal.wxMessageError(ex.Message,
                            "Form Main - wxCompilerConsole1_CompilerCompleted - WaveFile method");
                    }
                }
            }

        }

        private void ButtonFindError_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(
            //    "ButtonFindError CALLED - Line Error: " + mCompilerErrorLine);

            try
            {
                if (mCompilerEditor == null) return;

                ShowFullCode();
                mCompilerEditor.textEditor.GoToLine(mCompilerErrorLine);

                //Select the line//
                tabControlBuild.Size = new Size(0, tabControlBuild.MinimumSize.Height);
                mCompilerEditor.textEditor.SelectLine(mCompilerErrorLine);
                mCompilerEditor.Focus();
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - ButtonFindError_Click");
            }


        }

        private void FindLastWordPREV_Click(object sender, EventArgs e)
        {
            FindLastWord(true);
        }

        private void FindLastWordNEXT_Click(object sender, EventArgs e)
        {
            FindLastWord(false);
        }

        private void FindLastWord(bool isBackward)
        {
            if (string.IsNullOrEmpty(wxGlobal.LastWordSearch)) return;
            if (ActiveEditor == null) return;


            if (ActiveEditor.textEditor.GetTextLength() > 0)
            {

                string StringToFind = wxGlobal.LastWordSearch.Trim();

                try
                {
                    ActiveEditor.textEditor.FindText(
                                          StringToFind,
                                          wxGlobal.Settings.General.FindWholeWord,
                                          wxGlobal.Settings.General.FindMatchCase,
                                          isBackward, true, true, false);
                }

                catch (Exception ex)
                {
                    wxGlobal.wxMessageError(ex.Message,
                        "Form Main - FindLastWord");
                }

            }
        }


        //OLD: PROBABLY TO REMOVE !!!
        private void MenuFontSelectFont_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    FontDialog FontDialog1 = new FontDialog();

            //    ////Hack to display all fonts types
            //    //MethodInfo mi = typeof(FontDialog).GetMethod("SetOption",
            //    //                BindingFlags.NonPublic | BindingFlags.Instance);
            //    //mi.Invoke(FontDialog1, new object[] { 0x40000, false });

            //    FontDialog1.ShowEffects = false;
            //    FontDialog1.ShowColor = false;
            //    Font currentFont =
            //        new Font(ICSTextEditor.Font.Name, ICSTextEditor.Font.Size);
            //    FontDialog1.Font = currentFont;
            //    FontDialog1.FontMustExist = true;

            //    if (!(FontDialog1.ShowDialog() == DialogResult.OK))
            //    {
            //        return;
            //    }

            //    ICSTextEditor.Font = new Font(FontDialog1.Font.Name,
            //                                  FontDialog1.Font.Size);

            //}

            //catch (Exception ex)
            //{
            //    wxGlobal.wxMessageError(ex.Message, "MenuFontSelectFont_Click - Error");
            //}

        }

        //Drag file on wxTabCode!!!
        bool isDragFile = false;
        private void wxTabCode_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] list = e.Data.GetData(DataFormats.FileDrop) as string[];
                    //if (wxGlobal.CheckAllowedFileTypes(list))
                    //{
                    isDragFile = true;
                    e.Effect = DragDropEffects.Copy;
                    //}
                    //else
                    //{
                    //    isDragFile = false;
                    //}
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - wxTabCode_DragEnter: " + ex.Message);
            }
        }

        private void wxTabCode_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && isDragFile == true)
                    e.Effect = DragDropEffects.Copy;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - wxTabCode_DragOver: " + ex.Message);
            }
        }

        private void wxTabCode_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && isDragFile == true)
                {
                    string[] list = e.Data.GetData(DataFormats.FileDrop) as string[];
                    //if (list != null && File.Exists(list.GetValue(0).ToString()))
                    //    editor_FileDropped(null, list.GetValue(0).ToString());

                    if (list == null) return;

                    foreach (string filename in list)
                    {
                        if (File.Exists(filename))
                        {
                            AddNewEditor(filename);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - wxTabCode_DragDrop: " + ex.Message);
            }
        }

        private void LineEndsConverter(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                ToolStripMenuItem mTemp = (ToolStripMenuItem)sender;
                UInt32 eolMode = ScintillaTextEditor.SciConst.SC_EOL_CRLF;

                switch (mTemp.Name)
                {
                    case "MenuEolCRLF":
                        eolMode = ScintillaTextEditor.SciConst.SC_EOL_CRLF;
                        break;

                    case "MenuEolCR":
                        eolMode = ScintillaTextEditor.SciConst.SC_EOL_CR;
                        break;

                    case "MenuEolLF":
                        eolMode = ScintillaTextEditor.SciConst.SC_EOL_LF;
                        break;
                }

                ActiveEditor.textEditor.ConvertEOL((Int32)eolMode);
                ActiveEditor.textEditor.SetEolMode((Int32)eolMode);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - LineEndsConverter");
            }
        }

        private void MenuEditShowHideEOL_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                ActiveEditor.textEditor.ShowEOLMarker = !ActiveEditor.textEditor.GetViewEOL();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditShowHideEOL_Click");
            }
        }

        private void MenuEditShowHideWS_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                ActiveEditor.textEditor.ShowSpaces = !ActiveEditor.textEditor.ShowSpaces;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditShowHideWS_Click");
            }
        }



        private void tsBrowserBack_Click(object sender, EventArgs e)
        {
            if (HelpBrowser.CanGoBack)
                HelpBrowser.GoBack();
        }

        private void tsBrowserForward_Click(object sender, EventArgs e)
        {
            if (HelpBrowser.CanGoForward)
                HelpBrowser.GoForward();
        }

        private void tsBrowserHome_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
                {
                    HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - tsBrowserHome_Click");
            }
        }

        void HelpBrowser_CanGoBackChanged(object sender, EventArgs e)
        {
            tsBrowserBack.Enabled = HelpBrowser.CanGoBack;
        }

        void HelpBrowser_CanGoForwardChanged(object sender, EventArgs e)
        {
            tsBrowserForward.Enabled = HelpBrowser.CanGoForward;
        }


        private void MenuEditFormatAllCode_Click(object sender, EventArgs e)
        {
            FormatCode(true);
        }

        private void MenuEditFormatCode_Click(object sender, EventArgs e)
        {
            FormatCode(false);
        }
        private void FormatCode(bool AllLines)
        {
            try
            {
                if (ActiveEditor != null)
                {
                    //Only csd file formatting is supported
                    if (!ActiveEditor.FileName.ToLower().EndsWith(".csd")) return;


                    Int32 currentFirstVisibleLine =
                        ActiveEditor.textEditor.GetFirstVisibleLine();

                    Int32 currentLine = ActiveEditor.textEditor.GetCurrentLineNumber();

                    wxCodeFormatter mFormatter =
                                new wxCodeFormatter(ActiveEditor.textEditor, Opcodes);


                    //Check for Bookmarks
                    List<Int32> bookmarks = new List<Int32>();
                    if (ActiveEditor.textEditor.HasBookmarks())
                    {
                        Int32 mCurLine = 0;
                        Int32 mBookLine = 0;
                        do
                        {
                            mBookLine = ActiveEditor.textEditor.PrimaryView.MarkerNext(mCurLine, 1);
                            if (mBookLine == -1) break;

                            bookmarks.Add(mBookLine);
                            mCurLine = mBookLine + 1;
                        }
                        while (true);

                        ActiveEditor.textEditor.RemoveAllBookmarks();
                    }


                    //FORMAT THE CODE
                    if (AllLines == true)
                    {
                        mFormatter.FormatCode();
                    }
                    //If no text selected: format the line
                    else if (string.IsNullOrEmpty(ActiveEditor.textEditor.GetSelectedText()))
                    {
                        //ActiveEditor.textEditor.SetText(mFormatter.FormatCode());
                        mFormatter.FormatCode(
                            ActiveEditor.textEditor.GetLineNumberFromPosition(
                                ActiveEditor.textEditor.GetSelectionStart()),
                            ActiveEditor.textEditor.GetLineNumberFromPosition(
                                ActiveEditor.textEditor.GetSelectionEnd()));
                    }
                    //If some text is selected: format only selection
                    else
                    {
                        mFormatter.FormatCode(
                            ActiveEditor.textEditor.GetLineNumberFromPosition(
                                ActiveEditor.textEditor.GetSelectionStart()),
                            ActiveEditor.textEditor.GetLineNumberFromPosition(
                                ActiveEditor.textEditor.GetSelectionEnd())
                                + 1);
                    }


                    //Restore bookmarks
                    if (bookmarks.Count > 0)
                    {
                        foreach (Int32 i in bookmarks)
                        {
                            ActiveEditor.textEditor.InsertBookmarkAt(i);
                        }

                        ActiveEditor.RefreshListBoxBookmarks();
                    }


                    //Restore caret position
                    ActiveEditor.textEditor.SetFirstVisibleLine(currentFirstVisibleLine);
                    ActiveEditor.textEditor.SetCaretPosition(
                        //ActiveEditor.textEditor.GetPositionFromLineNumber(currentLine));
                        ActiveEditor.textEditor.GetLineEndPosition(currentLine));

                    ActiveEditor.SetFocus();
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuEditFormatCode_Click");
            }
        }

        private void MenuEditFormatOptions_Click(object sender, EventArgs e)
        {
            //Show Format settings window
            wxCodeFormatterForm cff = new wxCodeFormatterForm();
            if (wxGlobal.Settings == null) System.Media.SystemSounds.Beep.Play();

            cff.ShowDialog(this);
            cff.Dispose();
        }


        //OLD: REMOVE !!!
        private void MenuToolsWinXoundOpcodesUtility_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.CSoundConsole) ||
            //    !File.Exists(wxGlobal.Settings.Directory.CSoundConsole))
            //{
            //    MessageBox.Show("Cannot find CSound compiler!" + newline +
            //                    "Please select a valid path in File->Settings->CSound.exe",
            //                    "Compiler error!",
            //                    MessageBoxButtons.OK,
            //                    MessageBoxIcon.Error);
            //    return;
            //}

            //FormCreateOpcodesFile cop =
            //    new FormCreateOpcodesFile(wxGlobal.Settings.Directory.CSoundConsole);
            //////cop.SetCSoundConsolePath(wxGlobal.Settings.Directory.CSoundConsole);

            //DialogResult res = cop.ShowDialog(this);
            //cop.Dispose();
        }

        private void MenuToolsWinXoundTest_Click(object sender, EventArgs e)
        {
            try
            {
                AddNewEditor(Application.StartupPath + "\\Help\\test.csd");

                if (ActiveEditor != null)
                    ActiveEditor.SetFocus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuToolsWinXoundTest_Click");
            }
        }

        private void MenuInsertOptionsRealtimeOutput_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                //-W -odac //-W -oc:/fm_01.wav
                string StringToInsert =
                    ActiveEditor.textEditor.NewLine + "-W -odac"; // + newline;

                //Find <CsInstruments> 
                string StringToFind = "<CsOptions>";
                Int32 mFindPos = -1;

                mFindPos = ActiveEditor.textEditor.FindText(
                                                 StringToFind, true, true, false,
                                                 false, false, true, 0, -1);
                if (mFindPos > -1)
                {
                    ActiveEditor.textEditor.SetCaretPosition(
                        mFindPos + StringToFind.Length);
                    ActiveEditor.textEditor.InsertText(
                        ActiveEditor.textEditor.GetCaretPosition(), StringToInsert);
                    ActiveEditor.textEditor.SetCaretPosition(
                                                  mFindPos +
                                                  StringToFind.Length +
                                                  StringToInsert.Length);
                    ActiveEditor.Refresh();
                }
                else
                {
                    MessageBox.Show("Unable to find <CsOptions> section",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - FormInsertHeader_ButtonClicked");
            }
        }

        private void MenuInsertOptionsFileOutput_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            try
            {
                //-W -odac //-W -oc:/fm_01.wav

                string StringToInsert = "";

                //Find <CsInstruments> 
                string StringToFind = "<CsOptions>";
                Int32 mFindPos = -1;

                mFindPos = ActiveEditor.textEditor.FindText(
                                                 StringToFind, true, true, false,
                                                 false, false, true, 0, -1);
                if (mFindPos > -1)
                {
                    FormInputBox fib = new FormInputBox("Wave Filename", "Output filename:");
                    fib.ShowDialog(this);

                    if (string.IsNullOrEmpty(fib.ReturnValue)) return;

                    StringToInsert =
                        ActiveEditor.textEditor.NewLine +
                        "-W -o" + fib.ReturnValue.Trim() + ".wav";

                    //StringToInsert = StringToInsert.Replace("\\", "/");
                    fib.Dispose();

                    ActiveEditor.textEditor.SetCaretPosition(
                        mFindPos + StringToFind.Length);
                    ActiveEditor.textEditor.InsertText(
                        ActiveEditor.textEditor.GetCaretPosition(),
                        StringToInsert);
                    ActiveEditor.textEditor.SetCaretPosition(
                                                  mFindPos +
                                                  StringToFind.Length +
                                                  StringToInsert.Length);
                    ActiveEditor.Refresh();
                }
                else
                {
                    MessageBox.Show("Unable to find <CsOptions> section",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Main - FormInsertHeader_ButtonClicked");
            }
        }



        private void MenuToolsUseDefaultFlags_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (MenuToolsUseDefaultFlags.Checked)
                {
                    tabControlBuild.TabPages[0].Text = "Compiler [WinXound flags]";
                }
                else
                {
                    tabControlBuild.TabPages[0].Text = "Compiler [CsOptions flags]";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormMain.MenuToolsUseDefaultFlags_CheckedChanged Error: " +
                    ex.Message);
            }
        }

        private void MenuViewShowRepository_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
                {
                    if (oldHeight > tabControlBuild.MinimumSize.Height)
                    {
                        tabControlBuild.Size = new Size(0, oldHeight);
                    }
                    else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
                }
                tabControlBuild.SelectedTab = tabRepository;
                selIndex = tabControlBuild.SelectedIndex;
                //wxCodeRepository1.SetFocus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - MenuViewShowRepository_Click");
            }
        }

        private void MenuEditStoreSelectedText_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            if (ActiveEditor.textEditor.GetSelectedText().Length > 0)
            {
                wxCodeRepository1.InsertText(ActiveEditor.textEditor.GetSelectedText());
            }
        }

        private void MenuEditCodeFormatting_DropDownOpening(object sender, EventArgs e)
        {
            if (ActiveEditor == null)
            {
                MenuEditFormatCode.Text = "&Format Code";
                return;
            }


            if (ActiveEditor.textEditor.GetSelectedText().Length > 0)
                MenuEditFormatCode.Text = "&Format Selected Code";
            else
                MenuEditFormatCode.Text = "&Format Code";
        }





        private void SetHighlightLanguage(wxEditor editor, bool refresh)
        {
            string filename = editor.FileName;

            //Set Highlight language
            if (filename.ToLower().EndsWith(".py") ||
                filename.ToLower().EndsWith(".pyw"))
            {
                //Python syntax
                if (wxGlobal.Settings.EditorProperties.UseMixedPython)
                {
                    editor.SetHighlight("winxoundpython", "");
                    editor.ConfigureEditorForPythonMixed(Opcodes);
                }
                else
                {
                    editor.SetHighlight("python", "");
                    editor.ConfigureEditorForPython();
                }

            }
            else if (filename.ToLower().EndsWith(".lua"))
            {
                editor.SetHighlight("lua", "");
                editor.ConfigureEditorForLua();
            }
            else if (filename.ToLower().EndsWith(".csd") ||
                     filename.ToLower().EndsWith(".inc") ||
                     filename.ToLower().EndsWith(".udo") ||
                     filename.ToLower().EndsWith(".orc") ||
                     filename.ToLower().EndsWith(".sco"))
            {
                //CSound syntax
                //editor.SetHighlight("winxound",
                //                    Application.StartupPath + "\\WinXoundLexer.dll");
                //Temporary workaround for LoadLexerLibrary (Net 2.0 does not colourize)
                //(use only with SciLexer with LexWinxound compiled inside)
                editor.SetHighlight("winxound", "");
                editor.ConfigureEditorForCSound(Opcodes);
            }
            else
            {
                editor.SetHighlight("", "");
                editor.ConfigureEditorForText();
            }


            if (refresh)
            {
                //Refresh syntax
                editor.textEditor.PrimaryView.Colourise(0, -1);
                editor.textEditor.SecondaryView.Colourise(0, -1);
            }
        }

        private void MenuInfoCSoundTutorial_Click(object sender, EventArgs e)
        {
            try
            {
                //http://www.csounds.com/resources/learning/
                //http://www.csounds.com/tutorials
                System.Diagnostics.Process.Start("http://www.csounds.com/resources/learning/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    //MessageBox.Show(noBrowser.Message);
                    MessageBox.Show("Could not open 'http://www.csounds.com/resources/learning/'!" + newline +
                                    "Internet browser not found.",
                                    "WinXound error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
            catch (System.Exception other)
            {
                MessageBox.Show("Could not open 'http://www.csounds.com/resources/learning/'!" + newline +
                                other.Message,
                                "WinXound error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }




        ////////////////////////////////////////////////////////////////////////////////////////
        //BOOKMARKS
        ////////////////////////////////////////////////////////////////////////////////////////
        private void LoadBookmarks(wxEditor editor)
        {
            //Load Bookmarks ----------------------------------------------
            //if (!wxGlobal.isSyntaxType(editor)) return;

            //Load and Save bookmarks only on syntax styled documents 
            //(we skip others documents: txt, h, inc, udo)

            string languagecomment = "";

            if (editor.FileName.ToLower().EndsWith(".csd"))
                languagecomment = @";";
            else if (editor.FileName.ToLower().EndsWith(".py") ||
                     editor.FileName.ToLower().EndsWith(".pyw"))
                languagecomment = @"#";
            else if (editor.FileName.ToLower().EndsWith(".lua"))
                languagecomment = @"--";
            else return;


            try
            {
                Int32 ret = editor.textEditor.FindText(languagecomment + "[winxound_bookmarks",
                                                       true,
                                                       true,
                                                       false,
                                                       false,
                                                       false,
                                                       false,
                                                       0,
                                                       -1);

                if (ret > -1)
                {
                    Int32 eolLength = editor.textEditor.NewLine.Length;
                    Int32 line = editor.textEditor.GetLineNumberFromPosition(ret);

                    string lineText = editor.textEditor.GetTextLine(line);

                    editor.textEditor.ReplaceTarget(
                           ret,
                           editor.textEditor.GetLineLength(line),
                           "");

                    lineText = lineText.Trim("]".ToCharArray());
                    string[] splittedNumbers = lineText.Split(",".ToCharArray());

                    for (Int32 index = 1; index < splittedNumbers.Length; index++)
                    {
                        if (!string.IsNullOrEmpty(splittedNumbers[index]))
                            editor.textEditor.InsertBookmarkAt(
                                Int32.Parse(splittedNumbers[index]));
                    }

                    editor.BookmarksOnLoad = true;
                }

                editor.RefreshListBoxBookmarks();

            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, "Form Main - LoadBookmarks");
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - LoadBookmarks error: " + ex.Message);
            }
        }


        //Save bookmarks position
        private void SaveBookmarks(wxEditor editor)
        {
            //if([[wxDefaults valueForKey:@"SaveBookmarks"] boolValue] &&
            //   [textEditor hasBookmarks])
            if (editor.FileIsReadOnly) return;


            try
            {
                //if (wxGlobal.Settings.EditorProperties.SaveBookmarks ||
                //    editor.BookmarksOnLoad == true)
                {
                    string languagecomment = "";

                    if (editor.FileName.ToLower().EndsWith(".csd"))
                        languagecomment = @";";
                    else if (editor.FileName.ToLower().EndsWith(".py") ||
                             editor.FileName.ToLower().EndsWith(".pyw"))
                        languagecomment = @"#";
                    else if (editor.FileName.ToLower().EndsWith(".lua"))
                        languagecomment = @"--";
                    else return;


                    //???
                    //Int32 ret = editor.textEditor.FindText(languagecomment + "[winxound_bookmarks",
                    //                                       true,
                    //                                       true,
                    //                                       false,
                    //                                       false,
                    //                                       false,
                    //                                       false,
                    //                                       0,
                    //                                       -1);
                    //if (ret > -1)
                    //{
                    //    editor.textEditor.ReplaceTarget(
                    //           ret - 1,
                    //           editor.textEditor.GetLineLength(editor.textEditor.GetLineNumberFromPosition(ret)) + 1,
                    //           "");
                    //}



                    if (wxGlobal.Settings.EditorProperties.SaveBookmarks &&
                        editor.textEditor.HasBookmarks())
                    {
                        //Append remmed bookmarks to the end of text
                        //[textEditor AppendText:[NSString stringWithFormat:@"\n%@[winxound_bookmarks", languagecomment]];
                        if (editor.textEditor.GetText().EndsWith("\n") ||
                           editor.textEditor.GetText().EndsWith("\r"))
                        {
                            editor.textEditor.AppendText(languagecomment + "[winxound_bookmarks");
                        }
                        else
                        {
                            editor.textEditor.AppendText(
                                editor.textEditor.NewLine + languagecomment + "[winxound_bookmarks");
                        }

                        Int32 mCurLine = 0;
                        Int32 mBookLine = 0;
                        do
                        {
                            //mBookLine = [textEditor MarkerNext:mCurLine markerMask:1];
                            mBookLine = editor.textEditor.PrimaryView.MarkerNext(mCurLine, 1);
                            if (mBookLine == -1) break;

                            //[textEditor AppendText:[NSString stringWithFormat:@",%d", mBookLine]];
                            editor.textEditor.AppendText("," + mBookLine.ToString());
                            mCurLine = mBookLine + 1;
                        }
                        while (true);

                        editor.textEditor.AppendText("]");
                        editor.SaveFile(editor.FileName);
                    }
                    else if (editor.BookmarksOnLoad == true)
                    {
                        editor.SaveFile(editor.FileName);
                    }

                }

            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, "Form Main - SaveBookmarks");
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - SaveBookmarks error: " + ex.Message);
            }

        }
        ////////////////////////////////////////////////////////////////////////////////////////
        //BOOKMARKS
        ////////////////////////////////////////////////////////////////////////////////////////






        private void FindJumpToSelection_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            ActiveEditor.textEditor.ScrollCaret();
        }


        private bool FileIsUntitled(wxEditor editor)
        {
            return (editor.FileName == mUntitled + ".csd" ||
                    editor.FileName == mUntitled + ".py" ||
                    editor.FileName == mUntitled + ".lua");
        }

        private void MenuFileInfo_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            string permissions = (ActiveEditor.FileIsReadOnly ? "Read Only" : "Read/Write");

            MessageBox.Show(
                    "FILENAME: " + Path.GetFileName(ActiveEditor.FileName) + newline +
                    "FULL PATH: " + ActiveEditor.FileName + newline + newline +
                    "FILE TYPE:\t" + Path.GetExtension(ActiveEditor.FileName) + newline +
                    "FILE RIGHTS:\t" + permissions + newline +
                    "LINE ENDINGS:\t" + ActiveEditor.textEditor.GetEolModeReport() + newline +
                    "ENCODING:\tUnicode (UTF-8)" + newline +
                    "TOTAL LINES:\t" + ActiveEditor.textEditor.GetLines().ToString() + newline +
                    "TOTAL CHARS:\t" + ActiveEditor.textEditor.GetTextLength().ToString(),
                    "File Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
        }




        ////////////////////////////////////////////////////////////////////////////////////////
        //FIRST TIME START STUFFS
        ////////////////////////////////////////////////////////////////////////////////////////
        private void FirstStartTimer_Tick(object sender, EventArgs e)
        {
            FirstStartTimer.Stop();
            if (wxGlobal.Settings.General.FirstStart)
                this.FirstStart();
        }

        private void FirstStart()
        {
            //MessageBox.Show(
            //    "It seems that this is the first time that you launch WinXound.Net." + newline +
            //    "Winxound will now open the Settings Window for you." + newline +
            //    "Please fill all required fields in order to work correctly with WinXound." + newline +
            //    "You can later modify these values from the 'File->Settings' menu.",
            //    "First Start Informations",
            //    MessageBoxButtons.OK,
            //    MessageBoxIcon.Information);
            //MenuFileSettings.PerformClick();

            wxGlobal.Settings.General.FirstStart = false;

            MessageBox.Show(
                "It seems that this is the first time that you launch WinXound.Net." + newline +
                "WinXound will now try to find CSound, Python and Lua compilers." + newline + newline +
                "If WinXound fails, please open the Settings window ('File->Settings' menu), " +
                "click on 'Directories' tab and fill all required fields." + newline + newline +
                "You can later modify these values using the 'File->Settings' menu.",
                "First Start Informations",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            FormSettingsAutoSearch fsat = new FormSettingsAutoSearch();
            fsat.ReturnValueEv += new FormSettingsAutoSearch.OnReturnValue(fsat_ReturnValueEv);

            //if (fsat.DialogResult != DialogResult.OK) return;
            fsat.ShowDialog(this);

            fsat.ReturnValueEv -= new FormSettingsAutoSearch.OnReturnValue(fsat_ReturnValueEv);
            fsat.Dispose();

        }

        private void fsat_ReturnValueEv(object sender, string[] ReturnValue)
        {
            //Apply path settings
            string name = "";
            string value = "";

            try
            {
                foreach (string s in ReturnValue)
                {
                    name = s.Split("|".ToCharArray())[0];
                    value = s.Split("|".ToCharArray())[1];
                    switch (name)
                    {
                        case "csound.exe":
                            wxGlobal.Settings.Directory.CSoundConsole = value;
                            break;
                        case "csound.manual":
                            wxGlobal.Settings.Directory.CSoundHelpHTML = value;
                            break;
                        case "csound.gui":
                            wxGlobal.Settings.Directory.Winsound = value;
                            break;
                        case "python.exe":
                            wxGlobal.Settings.Directory.PythonConsolePath = value;
                            break;
                        case "python.idle":
                            wxGlobal.Settings.Directory.PythonIdlePath = value;
                            break;
                        case "lua.exe":
                            wxGlobal.Settings.Directory.LuaConsolePath = value;
                            break;
                    }
                }

                wxGlobal.Settings.SaveSettings();

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred during compilers search!" + newline +
                                "Error: " + ex.Message,
                                "WinXound Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        //FIRST TIME START STUFFS
        ////////////////////////////////////////////////////////////////////////////////////////

























        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        //AUTOCOMPLETION STUFFS
        private void MenuEditListOpcodes_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;
            if (ListBoxAutoComplete.Visible) return;

            ////If the caret is outside the view move the caret
            //ActiveEditor.textEditor.GetFocusedEditor.MoveCaretInsideView();
            if (CaretIsInsideView() == false) return;

            ListBoxAutoComplete.Location = GetListBoxAutoCompletePosition();

            int index = ListBoxAutoComplete.FindString(gCurWord);
            // Determine if a valid index is returned. Select the item if it is valid.
            if (index > -1)
                ListBoxAutoComplete.SetSelected(index, true);

            ListBoxAutoComplete.BringToFront();
            ListBoxAutoComplete.Show();
            ActiveEditor.SetFocus();
        }

        private bool CaretIsInsideView()
        {
            if (ActiveEditor == null) return false;

            Int32 curLine = ActiveEditor.textEditor.GetCurrentLineNumber();
            Int32 firstVisibleLine = ActiveEditor.textEditor.GetFirstVisibleLine();
            Int32 lastVisibleLine = firstVisibleLine +
                ActiveEditor.textEditor.LinesOnScreen();

            return (curLine >= firstVisibleLine &&
                    curLine <= lastVisibleLine);
        }

        void ListBoxAutoComplete_VisibleChanged(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("VISIBLE CHANGED");
            if (ActiveEditor == null) return;

            if (ListBoxAutoComplete.Visible)
            {
                ActiveEditor.textEditor.PrimaryView.RemoveArrowKeys();
                ActiveEditor.textEditor.SecondaryView.RemoveArrowKeys();
            }
            else
            {
                ActiveEditor.textEditor.PrimaryView.RestoreArrowKeys();
                ActiveEditor.textEditor.SecondaryView.RestoreArrowKeys();
            }
        }

        void RestoreAllKeys()
        {
            if (ActiveEditor == null) return;
            ListBoxAutoComplete.Hide();
            ActiveEditor.textEditor.PrimaryView.RestoreArrowKeys();
            ActiveEditor.textEditor.SecondaryView.RestoreArrowKeys();
        }

        void TextEditor_SCI_AutoComplete(object sender, string command)
        {
            if (ActiveEditor == null) return;

            Int32 index = 0;

            try
            {
                switch (command)
                {
                    case "hide": //HIDE AUTOCOMPLETION LIST BOX
                        if (ListBoxAutoComplete.Visible)
                        {
                            ListBoxAutoComplete.Hide();
                            RestoreAllKeys();
                        }
                        break;

                    case "ctrl+enter": //SHOW AUTOCOMPLETION MENU
                        MenuEditListOpcodes_Click(this, null);
                        break;

                    case "ctrl+shift+enter": //INSERT COMPLETE SYNOPSIS
                        if (ListBoxAutoComplete.SelectedIndex < 0) return;

                        string mText = ListBoxAutoComplete.SelectedItem.ToString();
                        if (Opcodes.Contains(mText))
                        {
                            string[] split = Opcodes[mText].ToString().Split("|".ToCharArray());

                            string synopsis = split[2];
                            //string output = SynopsisParseSquareBrackets(synopsis);

                            System.Diagnostics.Debug.WriteLine(synopsis);
                            InsertAutocompleteString(synopsis, "", mText);
                        }
                        ListBoxAutoComplete.Hide();
                        RestoreAllKeys();
                        break;


                    case "tab": //Insert opcode with tab space after
                        if (ListBoxAutoComplete.SelectedIndex < 0) return;

                        InsertAutocompleteString(ListBoxAutoComplete.SelectedItem.ToString(),
                                                 "\t",
                                                 ListBoxAutoComplete.SelectedItem.ToString());
                        ListBoxAutoComplete.Hide();
                        RestoreAllKeys();
                        break;

                    case "space": //Insert opcode with 1 space after
                        if (ListBoxAutoComplete.SelectedIndex < 0) return;

                        InsertAutocompleteString(ListBoxAutoComplete.SelectedItem.ToString(),
                                                 " ",
                                                 ListBoxAutoComplete.SelectedItem.ToString());
                        ListBoxAutoComplete.Hide();
                        RestoreAllKeys();
                        break;

                    case "enter": //Insert opcode without space after
                        if (ListBoxAutoComplete.SelectedIndex < 0) return;

                        InsertAutocompleteString(ListBoxAutoComplete.SelectedItem.ToString(),
                                                 "",
                                                 ListBoxAutoComplete.SelectedItem.ToString());
                        ListBoxAutoComplete.Hide();
                        RestoreAllKeys();
                        break;


                    case "up": //SCROLL UP OPCODES LIST
                        index = ListBoxAutoComplete.SelectedIndex;
                        index--;
                        if (index < 0) index = 0;

                        ListBoxAutoComplete.SelectedIndex = index;
                        break;

                    case "down": //SCROLL DOWN OPCODES LIST
                        index = ListBoxAutoComplete.SelectedIndex;
                        index++;
                        if (index >= ListBoxAutoComplete.Items.Count)
                            index = ListBoxAutoComplete.Items.Count - 1;

                        ListBoxAutoComplete.SelectedIndex = index;
                        break;

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, "Form Main - TextEditor_SCI_AutoComplete");
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - TextEditor_SCI_AutoComplete error: " + ex.Message);
            }
        }


        private void InsertAutocompleteString(string text, string space, string opcode)
        {

            if (ActiveEditor == null) return;

            try
            {
                Int32 pos = ActiveEditor.textEditor.GetCaretPosition();
                if (pos < 0) pos = 0;

                Int32 start = pos;
                Int32 end = pos;

                Int32 wordStart = ActiveEditor.textEditor.GetWordStart(pos);
                Int32 wordEnd = ActiveEditor.textEditor.GetWordEnd(pos);

                if (wordStart < start)
                {
                    start -= (start - wordStart);
                }
                if (pos < wordEnd)
                {
                    end += (wordEnd - pos);
                }

                ActiveEditor.textEditor.GetFocusedEditor.BeginUndoAction();
                {
                    if (start != pos)
                    {
                        ActiveEditor.textEditor.ReplaceTarget(start, end - start, "");
                    }
                    ActiveEditor.textEditor.AddText(text + space);

                }
                ActiveEditor.textEditor.GetFocusedEditor.EndUndoAction();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Main - InsertAutocompleteString");
                //System.Diagnostics.Debug.WriteLine(
                //    "Form Main - InsertAutocompleteString error: " + ex.Message);
            }


            try
            {
                //Show synopsis in intellitip
                if (Opcodes.Contains(opcode))
                {
                    string[] split = Opcodes[opcode].ToString().Split("|".ToCharArray());
                    ActiveEditor.SetIntelliTip("[" + opcode + "] - " + split[1], split[2]);
                }

            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, "Form Main - InsertAutocompleteString");
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - InsertAutocompleteString - Intellitip error: " + ex.Message);
            }


        }

        private void ListBoxAutoComplete_Click(object sender, EventArgs e)
        {
            ActiveEditor.textEditor.SetFocus();
        }

        private void ListBoxAutoComplete_DoubleClick(object sender, EventArgs e)
        {
            TextEditor_SCI_AutoComplete(this, "enter");
        }

        private Point GetListBoxAutoCompletePosition()
        {
            if (ActiveEditor == null) return new Point(0, 0);

            try
            {
                Int32 curPos = ActiveEditor.textEditor.GetCaretPosition();
                Int32 curLine = ActiveEditor.textEditor.GetLineNumberFromPosition(curPos);
                Int32 nextPos = ActiveEditor.textEditor.GetPositionFromLineNumber(curLine + 1);
                Int32 offsetX = -2;
                Int32 offsetY = 2;

                Point editorLocation =
                    new Point(ActiveEditor.textEditor.GetFocusedEditor.PointXfromPosition(curPos) + offsetX,
                              ActiveEditor.textEditor.GetFocusedEditor.PointYfromPosition(nextPos) + offsetY);


                Point screenPoint = ActiveEditor.textEditor.GetFocusedEditor.PointToScreen(editorLocation);
                Point returnPoint = this.PointToClient(screenPoint);

                //Point returnPoint;
                //if (ActiveEditor.textEditor.GetFocusedEditor.IsSecondaryView)
                //{
                //    returnPoint =
                //        new Point(editorLocation.X + ActiveEditor.SplitterDistanceX +
                //                  ActiveEditor.textEditor.SplitterDistanceX,
                //                  editorLocation.Y + ActiveEditor.Parent.Location.Y +
                //                  ActiveEditor.textEditor.SplitterDistanceY + 50);
                //}
                //else
                //{
                //    returnPoint =
                //        new Point(editorLocation.X + ActiveEditor.SplitterDistanceX,
                //                  editorLocation.Y + ActiveEditor.Parent.Location.Y + 50);
                //}

                return returnPoint;

            }
            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, "Form Main - GetListBoxAutoCompletePosition");
                System.Diagnostics.Debug.WriteLine(
                    "Form Main - GetListBoxAutoCompletePosition: " + ex.Message);
            }

            return new Point(0, 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////



        private void MenuInfoCSoundOpcodesHelp_Click(object sender, EventArgs e)
        {
            if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
            {
                //HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
                HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                                     "\\PartReference.html");
                MenuViewShowHelp_Click(null, null);
            }
            else
            {
                MessageBox.Show("Cannot find 'CSound Html Help' Path!" + newline +
                        "Please select a valid path in File->Settings->CSound Html Help",
                        "WinXound error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }

        private void MenuInfoCSoundFlagsHelp_Click(object sender, EventArgs e)
        {
            if (File.Exists(wxGlobal.Settings.Directory.CSoundHelpHTML))
            {
                HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
                                    "\\CommandFlags.html");
                MenuViewShowHelp_Click(null, null);
            }
            else
            {
                MessageBox.Show("Cannot find 'CSound Html Help' Path!" + newline +
                        "Please select a valid path in File->Settings->CSound Html Help",
                        "WinXound error!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }


        }

        private void MenuInfoFlossManual_Click(object sender, EventArgs e)
        {
            //string link = "http://booki.flossmanuals.net/csound/"; //OLD LINK;
            string link = "http://en.flossmanuals.net/csound/";

            try
            {
                System.Diagnostics.Process.Start(link);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    //MessageBox.Show(noBrowser.Message);
                    MessageBox.Show("Could not open '" + link + "'!" + newline +
                                    "Internet browser not found.",
                                    "WinXound error!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
            catch (System.Exception other)
            {
                MessageBox.Show("Could not open '" + link + "'!" + newline +
                                other.Message,
                                "WinXound error!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }



        private void OpenLastSessionFiles()
        {
            //for (uint i=0; i < wxSETTINGS->LastSessionFiles.size(); i++ )
            foreach (string file in wxGlobal.Settings.LastSessionFiles)
            {
                if (File.Exists(file))
                {
                    AddNewEditor(file);
                }

            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        //COMPILER WINDOW STUFFS
        private void CreateCompilerWindow()
        {
            Int32 w = wxGlobal.Settings.General.CompilerWindowSize.X;
            Int32 h = wxGlobal.Settings.General.CompilerWindowSize.Y;

            compilerWindow = new Form();
            compilerWindow.StartPosition = FormStartPosition.Manual;
            compilerWindow.Text = "WinXound Compiler Output/Tools";
            compilerWindow.Size = new Size(w, h);//new Size(700, 400);

            Rectangle mRect = Screen.GetWorkingArea(this);
            if (mRect.Contains(wxGlobal.Settings.General.CompilerWindowPosition) &&
                wxGlobal.Settings.General.CompilerWindowPosition.X > 0 &&
                wxGlobal.Settings.General.CompilerWindowPosition.Y > 0)
            {
                compilerWindow.Location =
                    wxGlobal.Settings.General.CompilerWindowPosition;
            }
            else
            {
                compilerWindow.Location =
                    new Point(mRect.Width - w,
                              mRect.Height - h);
            }

            compilerWindow.Icon = this.Icon;
            //compilerWindow.ShowIcon = false;
            //compilerWindow.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            compilerWindow.KeyPreview = true;
            compilerWindow.FormClosing +=
                new FormClosingEventHandler(compilerWindow_FormClosing);
            compilerWindow.KeyDown += new KeyEventHandler(compilerWindow_KeyDown);
        }

        void compilerWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("COMPILER_KEY_DOWN");
            if (compilerWindow == null) return;
            if (!compilerWindow.Visible) return;

            if (e.KeyCode == Keys.D0 &&
                e.Modifiers == (Keys.Control | Keys.Alt))
            {
                ToolStripCompilerWindow_Click(this, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (wxCompilerConsole1.ButtonStopCompiler.Enabled)
                {
                    MenuViewShowCompiler_Click(this, null);
                    wxCompilerConsole1.ButtonStopCompiler.PerformClick();
                    return;
                }

                //Reset the focus to the main window
                this.Focus();
            }
            else if (e.KeyCode == Keys.D9 &&
                    e.Modifiers == Keys.Control)
            {
                //on_menuitemShowCompiler_Clicked();
                MenuViewShowCompiler_Click(this, null);
            }
            else if (e.KeyCode == Keys.D0 &&
                    e.Modifiers == Keys.Control)
            {
                MenuViewShowHelp_Click(this, null);
            }
            else if (e.KeyCode == Keys.D8 &&
                    e.Modifiers == Keys.Control)
            {
                //Reset the focus to the main window and show the code
                this.Focus();
                MenuViewShowCode_Click(this, null);
            }
            else if (e.KeyCode == Keys.J &&
                    e.Modifiers == Keys.Control)
            {
                MenuEditOpcodesMenu_Click(this, null);
            }
            else if (e.KeyCode == Keys.I &&
                    e.Modifiers == Keys.Control)
            {
                MenuViewShowRepository_Click(this, null);
            }

        }

        void compilerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            AttachCompilerWindow();
        }

        void AttachCompilerWindow()
        {
            //Attach the compiler tabs to the main window
            //wxTabCode.Dock = DockStyle.None;
            splitter1.Visible = true;
            tabControlBuild.Dock = DockStyle.Bottom;
            tabControlBuild.Parent = this;

            //If uncommented return to the previous state
            oldHeight = this.Bounds.Height / 2;

            compilerWindow.Hide();
        }

        void DetachCompilerWindow()
        {
            //Detach the compiler tabs
            splitter1.Visible = false;
            tabControlBuild.Parent = compilerWindow;
            tabControlBuild.Dock = DockStyle.Fill;
            compilerWindow.Show();
        }

        private void ToolStripCompilerWindow_Click(object sender, EventArgs e)
        {
            if (compilerWindow == null)
                CreateCompilerWindow();

            if (compilerWindow.Visible)
                AttachCompilerWindow();
            else
                DetachCompilerWindow();

        }

        private void MenuViewAttachDetachCompiler_Click(object sender, EventArgs e)
        {
            ToolStripCompilerWindow_Click(this, null);
        }












        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        //CABBAGE TOOLS
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////

        private wxNamedPipe mPipeClient = null;
        private void MenuToolsUpdateCabbage_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            CheckForCabbageConnection();

            //CHECK ...
            if (mPipeClient.Connected)
                System.Diagnostics.Debug.WriteLine("CONNECTED TO CABBAGE");
            else
                return;

            SaveBeforeCompile();

            mPipeClient.SendMessage(
                        "CABBAGE_UPDATE| " + ActiveEditor.FileName);

            System.Diagnostics.Debug.WriteLine("CABBAGE_UPDATE: SENT SUCCESSFULLY");
        }



        private bool CheckForCabbageConnection()
        {
            //Initialize the socket if null
            if (mPipeClient == null)
            {
                mPipeClient = new wxNamedPipe();
                mPipeClient.MessageReceived +=
                        new wxNamedPipe.MessageReceivedHandler(mPipeClient_MessageReceived);
            }


            if (!mPipeClient.Connected)
            {
                mPipeClient.PipeName = @"\\.\pipe\cabbage";

                //Try to connect to an existing Cabbage instance
                if (CheckForCabbageProcess())
                {
                    mPipeClient.Connect();
                }
                //else launch a new instance of Cabbage
                else if (File.Exists(wxGlobal.Settings.Directory.CabbagePath))
                {
                    if (ActiveEditor != null)
                    {
                        SaveBeforeCompile();

                        /*
                        System.Diagnostics.Process mtProc = new System.Diagnostics.Process();
                        mtProc.StartInfo.FileName = wxGlobal.Settings.Directory.CabbagePath;
                        mtProc.StartInfo.Arguments = ActiveEditor.FileName;
                        mtProc.Start();
                        mtProc.WaitForInputIdle(1000);
                        */

                        System.Diagnostics.Process.Start(
                            wxGlobal.Settings.Directory.CabbagePath,
                            ActiveEditor.FileName);//ActiveEditor.FileName);
                        System.Diagnostics.Debug.WriteLine("CABBAGE LAUNCHED");


                        Int32 start = Environment.TickCount;
                        while (true)
                        {
                            System.Diagnostics.Debug.WriteLine("LOOP");

                            /*
                            if (mPipeClient.Connect()) break;
                            */

                            mPipeClient.Connect();
                            System.Threading.Thread.Sleep(250);
                            if (mPipeClient.SendMessage("TEST")) break;

                            if (Environment.TickCount - start > 5000) break;

                        }

                    }
                }
            }

            return true;
        }

        private bool CheckForCabbageProcess()
        {
            //Retrieve Cabbage.exe pid
            foreach (System.Diagnostics.Process p in
                     System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.ToLower().Contains("cabbage"))
                {
                    return true;
                }
            }
            return false;
        }

        void mPipeClient_MessageReceived(string message)
        {
            //This is needed otherwise windows rises the crossthread error
            this.Invoke(new wxNamedPipe.MessageReceivedHandler(DisplayReceivedMessage),
                new object[] { message });
        }

        void DisplayReceivedMessage(string message)
        {
            if (ActiveEditor == null) return;
            System.Diagnostics.Debug.WriteLine(message);

            if (message.Contains("CABBAGE_FILE_UPDATED|"))
            {
                string filename = message.Split("|".ToCharArray())[1].Trim();
                System.Diagnostics.Debug.WriteLine(filename);

                if (File.Exists(filename))
                {
                    //Check if the loaded file is already open in another tab
                    try
                    {
                        wxEditor tempEditor = null;
                        if (wxTabCode.TabPages.Count > 0)
                        {
                            foreach (TabPage tp in wxTabCode.TabPages)
                            {
                                tempEditor = (wxEditor)tp.Controls[0];
                                if (tempEditor.FileName == filename)
                                {
                                    wxTabCode.SelectedTab = tp;
                                    tempEditor.LoadFile(filename);
                                    tempEditor.textEditor.SetSavePoint();

                                    if (wxGlobal.Settings.General.BringWinXoundToFrontForCabbage)
                                    {
                                        System.Diagnostics.Debug.WriteLine("BRING TO FRONT");
                                        //this.BringToFront();
                                        //Application.OpenForms["FormMain"].BringToFront();
                                        this.Activate();
                                    }

                                    return;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Form Main - mPipeClient_MessageReceived Error: " +
                            ex.Message);
                    }

                }
            }
        }


        //CABBAGE_EXPORT_VSTI
        //CABBAGE_EXPORT_VST
        //CABBAGE_EXPORT_AU

        private void MenuToolsCabbageExportVSTI_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            CheckForCabbageConnection();

            //CHECK ...
            if (mPipeClient.Connected)
                System.Diagnostics.Debug.WriteLine("CONNECTED TO CABBAGE");
            else
                return;

            SaveBeforeCompile();

            mPipeClient.SendMessage(
                        "CABBAGE_UPDATE| " + ActiveEditor.FileName);
            System.Threading.Thread.Sleep(250);

            mPipeClient.SendMessage(
                        "CABBAGE_EXPORT_VSTI| " + ActiveEditor.FileName);

            System.Diagnostics.Debug.WriteLine("CABBAGE_EXPORT_VSTI: SENT SUCCESSFULLY");

        }

        private void MenuToolsCabbageExportVST_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            CheckForCabbageConnection();

            //CHECK ...
            if (mPipeClient.Connected)
                System.Diagnostics.Debug.WriteLine("CONNECTED TO CABBAGE");
            else
                return;

            SaveBeforeCompile();

            mPipeClient.SendMessage(
                        "CABBAGE_UPDATE| " + ActiveEditor.FileName);
            System.Threading.Thread.Sleep(250);

            mPipeClient.SendMessage(
                        "CABBAGE_EXPORT_VST| " + ActiveEditor.FileName);

            System.Diagnostics.Debug.WriteLine("CABBAGE_EXPORT_VST: SENT SUCCESSFULLY");
        }

        private void MenuToolsCabbageExportAU_Click(object sender, EventArgs e)
        {
            if (ActiveEditor == null) return;

            CheckForCabbageConnection();

            //CHECK ...
            if (mPipeClient.Connected)
                System.Diagnostics.Debug.WriteLine("CONNECTED TO CABBAGE");
            else
                return;

            SaveBeforeCompile();

            mPipeClient.SendMessage(
                        "CABBAGE_UPDATE| " + ActiveEditor.FileName);
            System.Threading.Thread.Sleep(250);

            mPipeClient.SendMessage(
                        "CABBAGE_EXPORT_AU| " + ActiveEditor.FileName);

            System.Diagnostics.Debug.WriteLine("CABBAGE_EXPORT_AU: SENT SUCCESSFULLY");
        }


        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        private void MenuToolsCompileWithAdditionalOptions_DropDownOpening(object sender, EventArgs e)
        {
            MenuToolsCompileWithAdditionalOptions.DropDownItems.Clear();
            MenuToolsExternalCompileWithAdditionalOptions.DropDownItems.Clear();

            foreach (string i in wxGlobal.Settings.General.CSoundAdditionalFlags)
            {
                if (!string.IsNullOrEmpty(i))
                {
                    MenuToolsCompileWithAdditionalOptions.DropDownItems.Add(i);
                    MenuToolsExternalCompileWithAdditionalOptions.DropDownItems.Add(i);
                }
            }
        }

        private void ToolStripCSoundConsole_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripCSoundConsole.DropDownItems.Clear();
            foreach (string i in wxGlobal.Settings.General.CSoundAdditionalFlags)
            {
                if (!string.IsNullOrEmpty(i))
                    ToolStripCSoundConsole.DropDownItems.Add(i);
            }
        }

        private void ToolStripCSoundConsole_ButtonClick(object sender, EventArgs e)
        {
            //Without CTRL key we call Internal Compiler
            if (ModifierKeys != Keys.Control)
                MenuToolsCompile.PerformClick();
            //With CTRL key we call External Compiler
            else
                MenuToolsExternalCompile.PerformClick();
        }

        private void ToolStripCSoundConsole_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripCSoundConsole.DropDown.Close();
            MenuTools.DropDown.Close();

            try
            {
                string temp = e.ClickedItem.Text.TrimStart("[".ToCharArray());
                string options = temp.Split("]".ToCharArray())[1];
                options = options.TrimStart(":".ToCharArray()).Trim();
                //ToolStripCSoundConsole.DropDown.Close();
                //MenuTools.DropDown.Close();

                bool mExternal = false;
                if ((sender as ToolStripItem).Name ==
                    "MenuToolsExternalCompileWithAdditionalOptions")
                {
                    mExternal = true;
                }

                if (ModifierKeys == Keys.Control || mExternal == true)
                    CompileExternal(options);
                else
                    Compile(options);
            }
            catch //(Exception ex)
            {
                MessageBox.Show(
                        "Please specify a valid command ([Description]: Flags)" +
                        "in your WinXound Settings (menu File->Settings->Compiler tab).",
                        "Additional Flags Invalid syntax",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            }

        }















    }
}
