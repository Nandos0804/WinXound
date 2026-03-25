using ScintillaTextEditor;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace WinXound_Net
{
    public partial class wxEditor : UserControl //, IEditor
    {

        private bool mShowExplorer = true;
        private bool mShowIntelliTip = true;
        private wxPosition mPosition = new wxPosition();
        private ArrayList mUserOpcodes = new ArrayList();
        private bool mFileIsReadOnly = false;

        private Int32 mTextChangePosition = 0;
        private Int32 mTextChangeLength = 0;

        private string mLanguage = "";

        private wxFindCodeStructure FindCodeStructure;

        private ImageList mImageList = new ImageList();

        private bool firstLaunch = true;
        private bool mBookmarksOnLoad = false;

        //private List<string> OrcScoFilesList = new List<string>();


        ///////////////////////////////
        //EVENTS:
        public event EventHandler CaretPositionChanged;
        public delegate void OnFileDragDrop(object sender, string[] filename);
        public event OnFileDragDrop FileDropped;
        //public event EventHandler TextHasChanged;
        public event TextView.OnSCI_Modified TextHasChanged;
        public event EventHandler FontHasChanged;
        public event EventHandler SavePointReached;
        public event EventHandler SavePointLeft;
#pragma warning disable CS0067
        public event EventHandler EditorError;
#pragma warning restore CS0067
        public event EventHandler OrcScoShowList;
        public event EventHandler OrcScoSwitchRequest;
        public event MouseEventHandler TextEditorMouseDown;
        public event KeyEventHandler TextEditorKeyAction;



        //Constructor
        public wxEditor()
        {
            InitializeComponent();

            try
            {
                //LabelOrcScoName.AutoSize = false;
                //LabelOrcScoName.Width = 350;
                //toolStripTextBoxOrcSco.AutoSize = false;
                //toolStripTextBoxOrcSco.Width = 100;
                LabelOrcScoName.Text = "";

                FindCodeStructure = new wxFindCodeStructure(this, ref textEditor);
                FindCodeStructure.WorkCompleted +=
                    new wxFindCodeStructure.OnWorkCompleted(FindCodeStructure_WorkCompleted);


                this.FillImageList();
                mImageList.ColorDepth = ColorDepth.Depth24Bit;
                mImageList.ImageSize = new Size(12, 12);
                TreeViewStructure.ImageList = mImageList;

                Int32 fontSize =
                    (wxGlobal.Settings.EditorProperties.ExplorerFontSize * 2) + 8;
                TreeViewStructure.Font =
                    new Font(TreeViewStructure.Font.Name, fontSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - wxEditor(Constructor): " + ex.Message);
            }
        }




        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            textEditor.SetFocus();
        }

        private void wxEditor_Load(object sender, EventArgs e)
        {

            textEditor.TextEditorMouseDown +=
                new MouseEventHandler(SCITextEditor_MouseDown);

            textEditor.TextEditorTextHasChanged +=
                new TextView.OnSCI_Modified(SCITextEditor_TextChanged);

            textEditor.TextEditorFileDropped +=
                new TextEditor.OnFileDragDrop(SCITextEditor_FileDragDrop);

            textEditor.TextEditorUpdateUI +=
                new EventHandler(SCITextEditor_UpdateUI);

            textEditor.TextEditorKeyAction +=
                new KeyEventHandler(SCITextEditor_KeyAction);

            textEditor.TextEditorSavePointReached +=
                new EventHandler(SCITextEditor_SavePointReached);

            textEditor.TextEditorSavePointLeft +=
               new EventHandler(SCITextEditor_SavePointLeft);

            textEditor.TextEditorMouseZoom +=
                new MouseEventHandler(SCITextEditor_MouseZoom);

            TreeViewStructure.GotFocus +=
                new EventHandler(TreeViewStructure_GotFocus);

            TreeViewStructure.Sorted = false;


        }

        void SCITextEditor_MouseZoom(object sender, MouseEventArgs e)
        {
            if (FontHasChanged != null) FontHasChanged(this, e);
        }

        void SCITextEditor_SavePointReached(object sender, EventArgs e)
        {
            if (SavePointReached != null) SavePointReached(sender, e);
        }

        void SCITextEditor_SavePointLeft(object sender, EventArgs e)
        {
            if (SavePointLeft != null) SavePointLeft(sender, e);
        }

        void SCITextEditor_KeyAction(object sender, KeyEventArgs e)
        {
            if (TextEditorKeyAction != null) TextEditorKeyAction(this, e);
        }

        void SCITextEditor_MouseDown(object sender, MouseEventArgs e)
        {
            //Store cursor position
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                mPosition.StoreCursorPos(textEditor.GetCaretPosition());
                //System.Diagnostics.Debug.WriteLine(textEditor.GetCaretPosition());
            }
            if (TextEditorMouseDown != null)
                TextEditorMouseDown(this, e);
        }

        void SCITextEditor_UpdateUI(object sender, EventArgs e)
        {
            //TODO ??? Inserire curword qui???
            //Raise Event CaretPositionChanged
            if (CaretPositionChanged != null) CaretPositionChanged(this, e);
        }

        private void SCITextEditor_TextChanged(object sender, Int32 position, Int32 length)
        {
            if (TextHasChanged != null) TextHasChanged(this, position, length);

            mTextChangePosition = position;
            mTextChangeLength = length;

            TimerSearch.Stop();
            TimerSearch.Start();
        }

        private void SCITextEditor_FileDragDrop(object sender, string[] filename)
        {
            if (FileDropped != null) FileDropped(this, filename);
        }

        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////




        //////////////////////////////////////////////////////////////////////////
        //EDITOR INTERFACE
        //////////////////////////////////////////////////////////////////////////

        //PROPERTIES:
        public ArrayList UserOpcodes
        {
            get { return mUserOpcodes; }
        }

        public string CurrentSyntaxLanguage
        {
            get { return mLanguage; }
        }

        public bool ShowExplorer
        {
            get { return mShowExplorer; }
            set
            {
                mShowExplorer = value;
                if (value == true)
                {
                    splitContainer2.Visible = true;
                    splitContainerBase.Panel1Collapsed = false;
                }
                else
                {
                    splitContainer2.Visible = false;
                    splitContainerBase.Panel1Collapsed = true;
                }

            }
        }

        public bool ShowIntelliTip
        {
            get { return mShowIntelliTip; }
            set
            {
                mShowIntelliTip = value;
                wxIntelliTip1.Visible = value;
            }

        }

        public bool ShowOrcScoPanel
        {
            get { return toolStripOrcSco.Visible; }
            set
            {
                toolStripOrcSco.Visible = value;
            }

        }

        public string FileName
        {
            get { return textEditor.FileName; }
            set { textEditor.FileName = value; }
        }

        public string FileType
        {
            get
            {
                //Set Highlight language
                if (textEditor.FileName.ToLower().EndsWith(".py") ||
                    textEditor.FileName.ToLower().EndsWith(".pyw"))
                {
                    //Python
                    return "python";

                }
                else if (textEditor.FileName.ToLower().EndsWith(".lua"))
                {
                    //Lua
                    return "lua";
                }
                else if (textEditor.FileName.ToLower().EndsWith(".csd") ||
                         textEditor.FileName.ToLower().EndsWith(".orc") ||
                         textEditor.FileName.ToLower().EndsWith(".sco"))
                {
                    //Csound
                    return "csound";
                }

                return "none";
            }
        }
        ////Control TextEditor { get; }
        //public Control TextEditor
        //{
        //    get { return textEditor; }
        //}
        public bool FileIsReadOnly
        {
            get { return mFileIsReadOnly; }
            set { mFileIsReadOnly = value; }
        }

        public bool BookmarksOnLoad
        {
            get { return mBookmarksOnLoad; }
            set { mBookmarksOnLoad = value; }
        }

        public Int32 SplitterDistanceX
        {
            get
            {
                if (this.splitContainerBase.Panel1Collapsed == false)
                    return this.splitContainerBase.SplitterDistance +
                           this.splitContainerBase.SplitterWidth;
                else
                    return 0;
            }
        }

        ///////////////////////////////
        //METHODS:
        public void LoadFile(string filename)
        {
            textEditor.LoadFile(filename);
            //TimerSearch.Start();
        }

        public void StartExplorerTimer()
        {
            TimerSearch.Start();
        }

        public void SaveFile(string filename)
        {
            textEditor.SaveFile(filename);
            this.FileName = filename;
        }

        public void RefreshTextEditor()
        {
            textEditor.Refresh();
        }

        public void SetFocus()
        {
            textEditor.SetFocus();
        }

        public void SetFocusOnPrimaryView()
        {
            textEditor.PrimaryView.SetFocus();
        }

        public void SetFocusOnSecondaryView()
        {
            textEditor.SecondaryView.SetFocus();
        }

        public void SetHighlight(string LanguageName, string Path)
        {
            textEditor.SetHighlight(LanguageName, Path);
        }



        public string GetCurrentOrcScoFile()
        {
            return LabelOrcScoName.Text.Trim();
        }
        public void SetCurrentOrcScoFile(string OrcScoFilename)
        {
            LabelOrcScoName.Text = OrcScoFilename;
        }



        public void ConfigureEditorForLua()
        {
            try
            {
                mLanguage = "lua";

                //Set Keywords list
                string keywords = "and break do else elseif " +
                                  "end false for function if " +
                                  "in local nil not or repeat return " +
                                  "then true until while ";

                string keywords2 = "_VERSION assert collectgarbage dofile " +
                                   "error gcinfo loadfile loadstring " +
                                   "print rawget rawset require tonumber " +
                                   "tostring type unpack ";

                string keywords3 = "";
                string keywords4 = "";

                //Add Lua 4:
                keywords2 += "_ALERT _ERRORMESSAGE _INPUT _PROMPT _OUTPUT " +
                             "_STDERR _STDIN _STDOUT call dostring foreach " +
                             "foreachi getn globals newtype" +
                             "sort tinsert tremove ";

                keywords3 += "abs acos asin atan atan2 ceil cos deg exp " +
                             "floor format frexp gsub ldexp log log10 max " +
                             "min mod rad random randomseed " +
                             "sin sqrt strbyte strchar strfind strlen " +
                             "strlower strrep strsub strupper tan ";

                keywords4 += "openfile closefile readfrom writeto appendto " +
                             "remove rename flush seek tmpfile tmpname read write " +
                             "clock date difftime execute exit getenv setlocale time ";

                //Add Lua 5:
                keywords2 += "_G getfenv getmetatable ipairs loadlib next pairs pcall " +
                             "rawequal setfenv setmetatable xpcall " +
                             "string table math coroutine io os debug " +
                             "load module select ";

                keywords3 += "string.byte string.char string.dump string.find " +
                              "string.len string.lower string.rep string.sub " +
                              "string.upper string.format string.gfind string.gsub " +
                              "table.concat table.foreach table.foreachi table.getn " +
                              "table.sort table.insert table.remove table.setn " +
                              "math.abs math.acos math.asin math.atan math.atan2 " +
                              "math.ceil math.cos math.deg math.exp " +
                              "math.floor math.frexp math.ldexp math.log " +
                              "math.log10 math.max math.min math.mod " +
                              "math.pi math.pow math.rad math.random math.randomseed " +
                              "math.sin math.sqrt math.tan string.gmatch string.match " +
                              "string.reverse table.maxn math.cosh math.fmod " +
                              "math.modf math.sinh math.tanh math.huge ";

                keywords4 += "coroutine.create coroutine.resume coroutine.status " +
                             "coroutine.wrap coroutine.yield io.close io.flush " +
                             "io.input io.lines io.open io.output io.read io.tmpfile " +
                             "io.type io.write io.stdin io.stdout io.stderr " +
                             "os.clock os.date os.difftime os.execute os.exit " +
                             "os.getenv os.remove os.rename " +
                             "os.setlocale os.time os.tmpname " +
                             "coroutine.running package.cpath package.loaded " +
                             "package.loadlib package.path package.preload " +
                             "package.seeall io.popen ";


                textEditor.SetKeyWords(0, keywords);
                textEditor.SetKeyWords(1, keywords2);
                textEditor.SetKeyWords(2, keywords3);
                textEditor.SetKeyWords(3, keywords4);

                this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
                this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - ConfigureEditorForLua: " + ex.Message);
            }
        }

        public void ConfigureEditorForPython()
        {
            try
            {
                mLanguage = "python";

                //Set Keywords list
                string keywords = "and as assert break class continue def del elif " +
                                  "else except exec finally for from global if import " +
                                  "in is lambda None not or pass print raise return " +
                                  "try while with yield";
                textEditor.SetKeyWords(0, keywords);

                this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
                this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - ConfigureEditorForPython: " + ex.Message);
            }
        }

        public void ConfigureEditorForPythonMixed(Hashtable Opcodes)
        {
            try
            {
                mLanguage = "python";

                //Set Keywords list
                string keywords = "and as assert break class continue def del elif " +
                                  "else except exec finally for from global if import " +
                                  "in is lambda None not or pass print raise return " +
                                  "try while with yield";
                textEditor.SetKeyWords(0, keywords);

                if (Opcodes != null)
                {
                    //Set Keywords list
                    string KeyWordList = "";
                    foreach (DictionaryEntry de in Opcodes)
                    {
                        KeyWordList += de.Key + " ";

                    }
                    textEditor.SetKeyWords(1, KeyWordList);
                }

                this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
                this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - ConfigureEditorForPythonMixed: " + ex.Message);
            }
        }

        public void ConfigureEditorForCSound(Hashtable Opcodes)
        {
            try
            {
                mLanguage = "csound";

                if (Opcodes != null)
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

                    //CABBAGE
                    string CabbageWidgets = "form rslider hslider vslider " +
                                            "button checkbox combobox groupbox keyboard";
                    string CabbageWords = "channel pos caption size value  " +
                                          "onoffCaption items colour ";

                    textEditor.SetKeyWords(0, KeyWordList + CabbageWords);
                    textEditor.SetKeyWords(2, CabbageWidgets);

                    string TagWordList = "<CsVersion> </CsVersion> " +
                                            "<CsoundSynthesizer> </CsoundSynthesizer> " +
                                            "<CsOptions> </CsOptions> " +
                                            "<CsInstruments> </CsInstruments> " +
                                            "<CsScore> </CsScore> " +
                                            "<CsVersion> </CsVersion> " +
                                            "<CsLicence> </CsLicence> " +
                                            "<Cabbage> </Cabbage> ";
                    textEditor.SetKeyWords(1, TagWordList);

                    textEditor.SetKeyWords(3, " instr endin ");




                }

                textEditor.SetWordChars(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_");

                this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
                this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - ConfigureEditor: " + ex.Message);
            }
        }

        public void ConfigureEditorForText()
        {
            mLanguage = "none";
        }

        private void SciEditSetFontsAndStyles(ScintillaTextEditor.TextView mSciEdit)
        {
            if (mLanguage == "none") return;
            //???
            //mSciEdit.StyleSetFont(32, wxGlobal.Settings.EditorProperties.DefaultFontName);
            //mSciEdit.StyleSetSize(32, wxGlobal.Settings.EditorProperties.DefaultFontSize);
            //mSciEdit.StyleClearAll(); 
            //???

            //Set Default Font (name and size) to styles 0 -> 32
            for (Int32 i = 0; i < 33; i++)
            {
                mSciEdit.StyleSetFont(i,
                    wxGlobal.Settings.EditorProperties.DefaultFontName);
                mSciEdit.StyleSetSize(i,
                    wxGlobal.Settings.EditorProperties.DefaultFontSize);
            }

            mSciEdit.SetZoom(0);
            mSciEdit.SetTabWidth(wxGlobal.Settings.EditorProperties.DefaultTabSize);
            //mSciEdit.SetPasteConvertEndings(true);   //???

            //Set styles from 0 to 33
            //STYLE_NUMBERS_MARGINS = 33;
            for (int mIndex = 0; mIndex < 34; mIndex++)
            {
                mSciEdit.StyleSetFore(
                    mIndex,
                    wxGlobal.Settings.EditorProperties.StyleGetForeColor(mLanguage, mIndex));

                mSciEdit.StyleSetBack(
                    mIndex,
                    wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, mIndex));

                mSciEdit.StyleSetBold(
                    mIndex,
                    wxGlobal.Settings.EditorProperties.StyleGetBold(mLanguage, mIndex));

                mSciEdit.StyleSetItalic(
                    mIndex,
                    wxGlobal.Settings.EditorProperties.StyleGetItalic(mLanguage, mIndex));
            }


            //DEFAULT STYLE "34" STYLE_BRACELIGHT
            mSciEdit.StyleSetFore(SciConst.STYLE_BRACELIGHT,
                                  mSciEdit.StyleGetFore(32)); //TextForeColor
            mSciEdit.StyleSetBack(SciConst.STYLE_BRACELIGHT,
                                  wxGlobal.Settings.EditorProperties.StyleGetBackColor(
                                  mLanguage, 256));   //TextSelectionBackColor

            //////DEFAULT STYLE "35" STYLE_BRACEBAD
            ////mSciEdit.StyleSetFore(SciConst.STYLE_BRACEBAD,
            ////                      wxGlobal.Settings.EditorProperties.OpcodesForeColor);
            ////mSciEdit.StyleSetBack(SciConst.STYLE_BRACEBAD,
            ////                      wxGlobal.Settings.EditorProperties.TagsForeColor);


            //TEXT SELECTION (style 256)
            mSciEdit.SetSelFore(true,
                                wxGlobal.Settings.EditorProperties.StyleGetForeColor(
                                mLanguage, 256));
            mSciEdit.SetSelBack(true,
                                wxGlobal.Settings.EditorProperties.StyleGetBackColor(
                                mLanguage, 256));


            //BOOKMARKS (style 257)
            //mSciEdit.MarkerSetFore(0, wxGlobal.Settings.EditorProperties.StyleGetForeColor(
            //                       mLanguage, 257));
            mSciEdit.MarkerSetBack(0, wxGlobal.Settings.EditorProperties.StyleGetBackColor(
                                   mLanguage, 257));
            mSciEdit.MarkerSetAlpha(0, wxGlobal.Settings.EditorProperties.StyleGetAlpha(
                                   mLanguage, 257));


            //VERTICAL RULER (style 258)
            mSciEdit.SetEdgeColour(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
                                   mLanguage, 258));


            //CARET LINE MARKER (style 259)
            mSciEdit.SetCaretLineBack(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
                                      mLanguage, 259));

            //CARET COLOR (Same as Text Fore Color)
            mSciEdit.SetCaretFore(mSciEdit.StyleGetFore(32));



        }




        public void InsertRemoveBookmark()
        {
            textEditor.InsertRemoveBookmark();
            RefreshListBoxBookmarks();
        }

        public void RemoveAllBookmarks()
        {
            textEditor.RemoveAllBookmarks();
            ListBoxBookmarks.Items.Clear();
        }

        public void GoToNextBookmark()
        {
            textEditor.GoToNextBookmark();
            //textEditor.SetFocus();
        }

        public void GoToPreviousBookmark()
        {
            textEditor.GoToPreviousBookmark();
            //textEditor.SetFocus();
        }

        public void SetIntelliTip(string title, string parameters)
        {
            wxIntelliTip1.ShowTip(title, parameters);
        }

        public PrintDocument GetPrintDocument()
        {
            return textEditor.GetPrintDocument();
        }

        public void StoreCursorPos()
        {
            mPosition.StoreCursorPos(textEditor.GetCaretPosition());
        }

        public void StoreCursorPos(int position)
        {
            mPosition.StoreCursorPos(position);
        }

        public void GoToPreviousPos()
        {
            textEditor.SetCaretPosition(mPosition.PreviousPos());
            textEditor.SetFocus();
        }

        public void GoToNextPos()
        {
            textEditor.SetCaretPosition(mPosition.NextPos());
            textEditor.SetFocus();
        }

        public void SetFont(Font f)
        {
            textEditor.SetTextFont(f);
        }

        public Font GetFont()
        {
            return textEditor.GetTextFont();
        }

        //END EDITOR INTERFACE
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////

        private void splitContainerBase_MouseDown(object sender, MouseEventArgs e)
        {
            textEditor.GetFocusedEditor.HideAutoCompletion();
        }

        private void splitContainerBase_Resize(object sender, EventArgs e)
        {
            try
            {
                if (splitContainerBase.Panel1.Width >
                    splitContainerBase.Parent.Width)
                {
                    splitContainerBase.SplitterDistance = 200;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - splitContainerBase_Resize: " + ex.Message);
            }
        }

        private void splitContainerBase_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.SetFocus();
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.SetFocus();
        }

        private void wxIntelliTip1_VisibleChanged(object sender, EventArgs e)
        {
            //TODO: Bug non aggiorna le dimensioni del texteditor ???
            this.splitContainerBase.Refresh();
        }


        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////
        //BACKGROUND WORKER FOR FINDSTUCTURE
        private Hashtable InstrSearch = new Hashtable();

        //void FindCodeStructure_WorkCompleted(object sender, Hashtable hashtable)
        void FindCodeStructure_WorkCompleted(object sender, TreeView treeView)
        {
            try
            {

                TreeViewStructure.BeginUpdate();

                //foreach (TreeNode t in treeView.Nodes)
                for (Int32 i = 0; i < treeView.Nodes.Count; i++)
                {
                    TreeNode node = TreeViewStructure.Nodes[i];
                    node.Nodes.Clear();

                    TreeNode t = treeView.Nodes[i];

                    TreeNode clonedNode = (TreeNode)t.Clone();
                    clonedNode.ImageIndex = 0;
                    clonedNode.SelectedImageIndex = 0;

                    if (clonedNode.Nodes.Count > 0)
                    {
                        foreach (TreeNode childNode in clonedNode.Nodes)
                        {
                            if (childNode.Parent.Name == "<CsOptions>")
                            {
                                childNode.ImageIndex = 1;
                                childNode.SelectedImageIndex = 1;
                            }
                            else if (childNode.Parent.Name == "<CsScore>")
                            {
                                if (childNode.Text.StartsWith("f"))
                                {
                                    childNode.ImageIndex = 5;
                                    childNode.SelectedImageIndex = 5;
                                }
                                else if (childNode.Text.StartsWith("#def"))
                                {
                                    childNode.ImageIndex = 6;
                                    childNode.SelectedImageIndex = 6;
                                }
                                else if (childNode.Text.StartsWith("s"))
                                {
                                    childNode.ImageIndex = 2;
                                    childNode.SelectedImageIndex = 2;
                                }
                            }
                            else if (childNode.Text.StartsWith("instr"))
                            {
                                childNode.ImageIndex = 4;
                                childNode.SelectedImageIndex = 4;
                            }
                            else if (childNode.Text.StartsWith("#def"))
                            {
                                childNode.ImageIndex = 6;
                                childNode.SelectedImageIndex = 6;
                            }
                            else if (childNode.Text.StartsWith("opc"))
                            {
                                childNode.ImageIndex = 3;
                                childNode.SelectedImageIndex = 3;
                            }

                            node.Nodes.Add(childNode);
                        }
                    }
                }

                TreeViewStructure.ExpandAll();
                TreeViewStructure.EndUpdate();

                if (firstLaunch)
                {
                    TreeViewStructure.TopNode = TreeViewStructure.Nodes[0];
                    firstLaunch = false;
                }

                //TreeViewStructure.Refresh();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxEditor - FindCodeStructure_WorkCompleted: " + ex.Message);
            }
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {

            TimerSearch.Stop();

            //using (FindCodeStructure)
            //{
            //System.Diagnostics.Debug.WriteLine("Before: " + FindCodeStructure.State);

            if (!wxGlobal.isSyntaxType(this)) return;

            FindCodeStructure.Start();
            //System.Diagnostics.Debug.WriteLine("After: " + FindCodeStructure.State);
            //}

        }

        public void RefreshExplorer()
        {
            TimerSearch_Tick(this, null);
            Int32 fontSize =
                (wxGlobal.Settings.EditorProperties.ExplorerFontSize * 2) + 8;
            TreeViewStructure.Font =
                new Font(TreeViewStructure.Font.Name, fontSize);
        }

        private void TreeViewStructure_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (!textEditor.Visible) return;

                string StringToFind = e.Node.Name;
                bool isTag = false;

                Int32 gStart = 0;
                Int32 gEnd = textEditor.GetTextLength();

                //ROOT NODE
                if (e.Node.Level == 0)
                {
                    string mName = e.Node.Text;

                    if (mName == "1. <CsoundSynthesizer>")
                    {
                        StringToFind = "<CsoundSynthesizer>";
                    }
                    else if (mName == "2. <CsOptions>")
                    {
                        StringToFind = "<CsOptions>";
                    }
                    else if (mName == "3. <CsInstruments>")
                    {
                        StringToFind = "<CsInstruments>";
                    }
                    else if (mName == "4. <CsScore>")
                    {
                        StringToFind = "<CsScore>";
                    }

                    isTag = true;
                }
                else if (e.Node.Parent.Name == "<CsScore>")
                {
                    if (e.Node.Text.StartsWith("s"))
                    {
                        Int32 cycles = Int32.Parse(e.Node.Text.Substring(1));
                        Int32 mStart = 0;
                        Int32 ret = -1;

                        for (Int32 c = 0; c < cycles; c++)
                        {
                            ret = textEditor.FindText("s",
                                                      true,
                                                      true,
                                                      false,
                                                      false,
                                                      false,
                                                      true,
                                                      mStart,
                                                      -1);
                            if (ret > -1)
                            {
                                mStart = ret + 2;
                            }
                            else break;
                        }
                        if (ret > -1)
                        {
                            //[textEditor GoToLine:[textEditor getLineNumberFromPosition:ret]];
                            textEditor.GoToLine(textEditor.GetLineNumberFromPosition(ret));
                            if (ret > -1)
                                textEditor.SelectLine(
                                    textEditor.GetLineNumberFromPosition(ret), true);
                            return;
                        }
                    }
                    else
                    {
                        //Backward search
                        gStart = gEnd;
                        gEnd = 0;
                    }
                }

                else
                {
                    //StringToFind = InstrSearch[mName].ToString();
                    StringToFind = e.Node.Name;
                }

                System.Diagnostics.Debug.WriteLine(StringToFind);

                //Find and select line
                Int32 position =
                    textEditor.FindText(StringToFind, isTag, true, false,
                                        false, false, true, gStart, gEnd);
                if (position > -1)
                    textEditor.SelectLine(
                        textEditor.GetLineNumberFromPosition(position), true);

            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "wxEditor - TreeViewStructure_NodeMouseClick");
            }

        }

        void TreeViewStructure_GotFocus(object sender, EventArgs e)
        {
            this.SetFocus();
        }




        public void RefreshListBoxBookmarks()
        {

            try
            {
                Int32 mBookLine = 0;
                Int32 CurLine = 0;
                Int32 mIndex = 0;
                string mText = "";

                ListBoxBookmarks.Items.Clear();
                ListBoxBookmarks.BeginUpdate();
                do
                {
                    mBookLine = textEditor.PrimaryView.MarkerNext(CurLine, 1);
                    if (mBookLine == -1) break;

                    mIndex += 1;
                    mText = textEditor.PrimaryView.GetLine(mBookLine);
                    mText = Regex.Replace(mText, @"\s+", " ").Trim();
                    ListBoxBookmarks.Items.Add(String.Format("{0:G}", mIndex) + ". " + mText);
                    CurLine = mBookLine + 1;
                }
                while (true);
                ListBoxBookmarks.EndUpdate();

                ListBoxBookmarks.Refresh();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "wxEditor - RefreshListBox");

            }
        }

        private void ListBoxBookmarks_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Int32 CurLine = 0;
                Int32 BookLine = 0;
                Int32 mIndex = ListBoxBookmarks.SelectedIndex;

                for (Int32 Ciclo = 0; Ciclo < mIndex + 1; Ciclo++)
                {
                    BookLine = textEditor.GetFocusedEditor.MarkerNext(CurLine, 1);
                    CurLine = BookLine + 1;
                }

                ListBoxBookmarks.SelectedIndex = -1;
                textEditor.GetFocusedEditor.GotoLine(BookLine);
                textEditor.GetFocusedEditor.SetFocus();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "wxEditor - ListBoxBookmarks_SelectedIndexChanged");

            }
        }






        private void FillImageList()
        {
            //Create Bitmap images in memory and then add them to the ImageList
            Int32 mWidth = 8;
            Int32 mHeight = 8;
            Int32 x = (mImageList.ImageSize.Width / 2) - (mWidth / 2);
            Int32 y = (mImageList.ImageSize.Height / 2) - (mHeight / 2);

            //For Windows Xp we must set the bitmap rectangle to 9x9
            if (!wxGlobal.IsWinVistaOrMajor())
            {
                mWidth = 9;
                mHeight = 9;
            }

            //BLUE
            Bitmap bmpBlue = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpBlue.MakeTransparent(Color.Black);
            bmpBlue.MakeTransparent(Color.Black);
            Graphics gBmpBlue = Graphics.FromImage(bmpBlue);
            gBmpBlue.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpBlue.FillRectangle(new SolidBrush(Color.FromArgb(12, 93, 136)),
                                   new Rectangle(x, y, mWidth, mHeight));

            //GREEN
            Bitmap bmpGreen = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpGreen.MakeTransparent(Color.Black);
            Graphics gBmpGreen = Graphics.FromImage(bmpGreen);
            gBmpGreen.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpGreen.FillRectangle(new SolidBrush(Color.FromArgb(0, 120, 5)),
                                    new Rectangle(x, y, mWidth, mHeight));

            //GREY
            Bitmap bmpGrey = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpGrey.MakeTransparent(Color.Black);
            Graphics gBmpGrey = Graphics.FromImage(bmpGrey);
            gBmpGrey.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpGrey.FillRectangle(new SolidBrush(Color.FromArgb(190, 38, 38)),
                                   new Rectangle(x, y, mWidth, mHeight));

            //ORANGE
            Bitmap bmpOrange = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpOrange.MakeTransparent(Color.Black);
            Graphics gBmpOrange = Graphics.FromImage(bmpOrange);
            gBmpOrange.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpOrange.FillRectangle(new SolidBrush(Color.FromArgb(255, 108, 0)),
                                     new Rectangle(x, y, mWidth, mHeight));

            //RED
            Bitmap bmpRed = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpRed.MakeTransparent(Color.Black);
            Graphics gBmpRed = Graphics.FromImage(bmpRed);
            gBmpRed.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpRed.FillRectangle(new SolidBrush(Color.FromArgb(143, 0, 0)),
                                  new Rectangle(x, y, mWidth, mHeight));

            //SAND
            Bitmap bmpSand = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpSand.MakeTransparent(Color.Black);
            Graphics gBmpSand = Graphics.FromImage(bmpSand);
            gBmpSand.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpSand.FillRectangle(new SolidBrush(Color.FromArgb(216, 163, 101)),
                                   new Rectangle(x, y, mWidth, mHeight));

            //VIOLET
            Bitmap bmpViolet = new Bitmap(mImageList.ImageSize.Width, mImageList.ImageSize.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmpViolet.MakeTransparent(Color.Black);
            Graphics gBmpViolet = Graphics.FromImage(bmpViolet);
            gBmpViolet.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gBmpViolet.FillRectangle(new SolidBrush(Color.FromArgb(190, 66, 188)),
                                     new Rectangle(x, y, mWidth, mHeight));


            mImageList.Images.Add(bmpRed);
            mImageList.Images.Add(bmpGreen);
            mImageList.Images.Add(bmpGrey);
            mImageList.Images.Add(bmpOrange);
            mImageList.Images.Add(bmpBlue);
            mImageList.Images.Add(bmpSand);
            mImageList.Images.Add(bmpViolet);


        }

        private void toolStripButtonSwitchOrcSco_Click(object sender, EventArgs e)
        {
            //Raise switch event
            if (OrcScoSwitchRequest != null) OrcScoSwitchRequest(this, e);
        }

        private void toolStripButtonClearOrcSco_Click(object sender, EventArgs e)
        {
            LabelOrcScoName.Text = "";
        }

        private void toolStripButtonBrowseOrcSco_Click(object sender, EventArgs e)
        {
            try
            {
                //Open document 
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                string mExtension =
                    this.FileName.ToLower().EndsWith(".orc") ? "*.sco" : "*.orc";

                openFileDialog1.Filter =
                    "Csound files (" + mExtension + ")" + "|" + mExtension + "|" +
                    "All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = false;
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(this.FileName);


                if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    textEditor.SetFocus();
                    return;
                }

                LabelOrcScoName.Text = openFileDialog1.FileName;
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "wxEditor - toolStripButtonBrowseOrcSco_Click");
            }
        }

        private void toolStripButtonShowList_Click(object sender, EventArgs e)
        {
            if (OrcScoShowList != null) OrcScoShowList(sender, e);
        }






    }
}
