using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;


namespace ScintillaTextEditor
{
    public partial class TextEditor : UserControl
    {

        public TextEditor()
        {
            InitializeComponent();

            //Set DocPointer for secondary view
            textView2.SetDocPointer(textView1.GetDocPointer());
            textView2.IsSecondaryView = true;
            oldFocusedEditor = textView1;
            textView1.SetScrollWidthTracking(true);
            textView2.SetScrollWidthTracking(true);

            textView1.SetVirtualSpaceOptions(1);
            textView2.SetVirtualSpaceOptions(1);
            textView1.SetAdditionalCaretsVisible(false);
            textView2.SetAdditionalCaretsVisible(false);
            textView1.SetMultipleSelection(false);
            textView2.SetMultipleSelection(false);

            //textView1.SetMarginSensitiveN(0, true);
            //textView2.SetMarginSensitiveN(0, true);


        }


        private string mFileName = "";
        private PrintDocument mPrintDocument = null;
        private TextView oldFocusedEditor = null;
        private bool mShowMatchingBracket = false;




        ///////////////////////////////
        //EVENTS:
        public delegate void OnFileDragDrop(object sender, string[] filename);
        public event OnFileDragDrop TextEditorFileDropped;

        public delegate void OnSciModContainer(object sender, Int32 token);
        public event OnSciModContainer TextEditorModContainer;

        public event EventHandler TextEditorUpdateUI;
        //public event EventHandler TextEditorTextHasChanged;
        public event TextView.OnSCI_Modified TextEditorTextHasChanged;
        public event EventHandler TextEditorFontHasChanged;
        public event EventHandler TextEditorSavePointReached;
        public event EventHandler TextEditorSavePointLeft;
        //public event EventHandler TextEditorModContainer;
        public event EventHandler TextEditorError;
        public event KeyEventHandler TextEditorKeyAction;
        public event MouseEventHandler TextEditorMouseDown;
        public event MouseEventHandler TextEditorMouseZoom;

        public event EventHandler EditorError;

        ///////////////////////////////
        //PROPERTIES:
        //Font TextEditorFont { get; set;}
        public Font TextEditorFont
        {
            get
            {
                return this.GetTextFont();
            }
            set
            {
                this.SetTextFont(value);
            }
        }
        //bool ShowSpaces { get; set; }
        public bool ShowSpaces
        {
            get
            {
                return Convert.ToBoolean(textView1.GetViewWS());
            }
            set
            {
                if (value == true)
                {
                    textView1.SetViewWS(1);
                    textView2.SetViewWS(1);
                }
                else
                {
                    textView1.SetViewWS(0);
                    textView2.SetViewWS(0);
                }
            }
        }
        //bool ShowEOLMarker { get; set; }
        public bool ShowEOLMarker
        {
            get { return textView1.GetViewEOL(); }
            set
            {
                textView1.SetViewEOL(value);
                textView2.SetViewEOL(value);
            }
        }
        //bool AllowCaretBeyondEOL { get; set; }
        public bool AllowCaretBeyondEOL
        {
            get { return textView1.GetEndAtLastLine(); }
            set
            {
                textView1.SetEndAtLastLine(!value);
                textView2.SetEndAtLastLine(!value);
            }
        }
        //bool ShowMatchingBracket { get; set; }
        public bool ShowMatchingBracket
        {
            //STYLE_BRACELIGHT //
            //get { return textView1.StyleGetBold(SciConst.STYLE_BRACELIGHT); }
            get { return mShowMatchingBracket; }
            set
            {
                this.BraceHiglight(-1, -1);
                if (TextEditorUpdateUI != null)
                {
                    TextEditorUpdateUI(this, new EventArgs());
                }
                mShowMatchingBracket = value;
            }
        }
        //bool CanUndo { get; }
        public bool CanUndo
        {
            get { return Convert.ToBoolean(GetFocusedEditor.CanUndo()); }
        }
        //bool CanRedo { get; }
        public bool CanRedo
        {
            get { return Convert.ToBoolean(GetFocusedEditor.CanRedo()); }
        }
        //int TabIndent { get; set; }
        public int TabIndent
        {
            get { return textView1.GetTabWidth(); }
            set
            {
                textView1.SetTabWidth(value);
                textView2.SetTabWidth(value);
            }
        }
        //bool ShowLineNumbers { get; set; }
        public bool ShowLineNumbers
        {
            get
            {
                //return Convert.ToBoolean(textView1.GetMarginWidthN(0)); 
                return (textView1.GetMarginWidthN(0) > 0 ? true : false);
            }
            set
            {
                if (value == true)
                {
                    textView1.SetMarginWidthN(0, textView1.SCI_WIDTH_MARGINS);
                    textView2.SetMarginWidthN(0, textView2.SCI_WIDTH_MARGINS);
                }
                else
                {
                    textView1.SetMarginWidthN(0, 0);
                    textView2.SetMarginWidthN(0, 0);
                }

            }
        }
        //bool MarkCaretLine { get; set; }
        public bool MarkCaretLine
        {
            //wxGlobal.Preferences.Syntax.MarkCaretLineColor
            get
            {
                return textView1.GetCaretLineVisible();
            }
            set
            {
                textView1.SetCaretLineVisible(value);
                textView2.SetCaretLineVisible(value);
            }
        }
        //bool ShowVerticalRuler { get; set; }
        public bool ShowVerticalRuler
        {
            get { return Convert.ToBoolean(textView1.GetEdgeMode()); }
            set
            {
                if (value == true)
                {
                    //wxGlobal.Preferences.Syntax.VerticalRulerColor
                    textView1.SetEdgeColumn(80);
                    textView2.SetEdgeColumn(80);
                    textView1.SetEdgeMode(1);
                    textView2.SetEdgeMode(1);
                }
                else
                {
                    textView1.SetEdgeMode(0);
                    textView2.SetEdgeMode(0);
                }

            }
        }
        //bool ReadOnly { get; set; }
        public bool ReadOnly
        {
            get { return textView1.GetReadOnly(); }
            set
            {
                textView1.SetReadOnly(value);
                textView2.SetReadOnly(value);
            }
        }
        //TextView PrimaryView
        public TextView PrimaryView
        {
            get { return textView1; }
        }
        //TextView SecondaryView
        public TextView SecondaryView
        {
            get { return textView2; }
        }

        public string NewLine
        {
            get
            {
                UInt32 eolMode = (UInt32)textView1.GetEOLMode();
                string newEol = "\r\n";

                switch (eolMode)
                {
                    case SciConst.SC_EOL_CRLF:
                        newEol = "\r\n";
                        break;

                    case SciConst.SC_EOL_CR:
                        newEol = "\r";
                        break;

                    case SciConst.SC_EOL_LF:
                        newEol = "\n";
                        break;
                }

                return newEol;
            }
        }

        public Int32 SplitterDistanceY
        {
            get
            {
                if (this.splitContainer1.Panel2Collapsed == false)
                {
                    if (this.splitContainer1.Orientation == Orientation.Horizontal)
                    {
                        return this.splitContainer1.SplitterDistance +
                               this.splitContainer1.SplitterWidth;
                    }
                }
                return 0;
            }
        }

        public Int32 SplitterDistanceX
        {
            get
            {
                if (this.splitContainer1.Panel2Collapsed == false)
                {
                    if (this.splitContainer1.Orientation == Orientation.Vertical)
                    {
                        return this.splitContainer1.SplitterDistance +
                               this.splitContainer1.SplitterWidth;
                    }
                }
                return 0;
            }
        }



        private void TextEditor_Load(object sender, EventArgs e)
        {
            mPrintDocument = new TextPrinter(this.textView1);


            textView1.SCI_MouseEvent +=
                new MouseEventHandler(textViews_SCI_MouseEvent);
            textView2.SCI_MouseEvent +=
                new MouseEventHandler(textViews_SCI_MouseEvent);

            textView1.SCI_Modified +=
                new TextView.OnSCI_Modified(textViews_TextChanged);
            textView2.SCI_Modified +=
                new TextView.OnSCI_Modified(textViews_TextChanged);

            textView1.SCI_FileDragDrop +=
                new TextView.OnFileDragDrop(textViews_SCI_FileDragDrop);
            textView2.SCI_FileDragDrop +=
                new TextView.OnFileDragDrop(textViews_SCI_FileDragDrop);

            textView1.SCI_UpdateUI +=
                new EventHandler(textViews_SCI_UpdateUI);
            textView2.SCI_UpdateUI +=
                new EventHandler(textViews_SCI_UpdateUI);

            textView1.KeyAction +=
                new KeyEventHandler(textViews_KeyAction);
            textView2.KeyAction +=
                new KeyEventHandler(textViews_KeyAction);

            textView1.GotFocus +=
                new EventHandler(textViews_SCI_GotFocus);
            textView2.GotFocus +=
                new EventHandler(textViews_SCI_GotFocus);

            textView1.LostFocus +=
                new EventHandler(textViews_SCI_LostFocus);
            textView2.LostFocus +=
                new EventHandler(textViews_SCI_LostFocus);

            textView1.SCI_SavePointReached +=
                new EventHandler(textViews_SCI_SavePointReached);
            textView2.SCI_SavePointReached +=
                new EventHandler(textViews_SCI_SavePointReached);

            textView1.SCI_SavePointLeft +=
                new EventHandler(textViews_SCI_SavePointLeft);
            textView2.SCI_SavePointLeft +=
                new EventHandler(textViews_SCI_SavePointLeft);

            textView1.SCI_ModContainer +=
                new TextView.OnSciModContainer(textViews_SCI_ModContainer);
            //new EventHandler(textViews_SCI_ModContainer);

            textView1.SCI_MouseZoom +=
                new MouseEventHandler(textViews_SCI_MouseZoom);
            textView2.SCI_MouseZoom +=
                new MouseEventHandler(textViews_SCI_MouseZoom);

        }


        void textViews_SCI_SavePointReached(object sender, EventArgs e)
        {
            if (TextEditorSavePointReached != null) TextEditorSavePointReached(sender, e);
        }

        void textViews_SCI_SavePointLeft(object sender, EventArgs e)
        {
            if (TextEditorSavePointLeft != null) TextEditorSavePointLeft(sender, e);
        }

        void textViews_SCI_ModContainer(object sender, Int32 token)
        {
            //if (TextEditorModContainer != null) TextEditorModContainer(sender, e);

            //After Undo/Redo we must check for real line endings to synchronize
            //Scintilla Eol Mode
            Int32 eolModeReal = this.GetEolModeReal();
            Int32 eolModeScintilla = this.GetEolMode();

            System.Diagnostics.Debug.WriteLine(
                    "Real Mode: " + eolModeReal +
                    " - Scintilla Mode: " + eolModeScintilla);

            if (eolModeReal != this.GetEolMode())
            {
                this.SetEolMode(eolModeReal);
            }

            if (token == 2)
            {
                //System.Diagnostics.Debug.WriteLine(
                //    "SC_MOD_CONTAINER:CONVERT_EOL!!! -> TOKEN: " + token);

                if (TextEditorModContainer != null)
                    TextEditorModContainer(this, token);
            }
        }

        void textViews_SCI_GotFocus(object sender, EventArgs e)
        {
            oldFocusedEditor = sender as TextView;
            this.OnGotFocus(e);
        }

        void textViews_SCI_LostFocus(object sender, EventArgs e)
        {
            oldFocusedEditor = sender as TextView;
            this.OnLostFocus(e);
        }

        void textViews_KeyAction(object sender, KeyEventArgs e)
        {
            if (TextEditorKeyAction != null)
                TextEditorKeyAction(this, e);
        }

        void textViews_SCI_MouseEvent(object sender, MouseEventArgs e)
        {
            if (TextEditorMouseDown != null)
                TextEditorMouseDown(this, e);
        }

        void textViews_SCI_UpdateUI(object sender, EventArgs e)
        {
            //Raise Event TextEditorUpdateUI
            if (TextEditorUpdateUI != null)
            {
                TextEditorUpdateUI(this, e);
            }
        }

        private void textViews_TextChanged(object sender, Int32 position, Int32 length)
        {
            if (TextEditorTextHasChanged != null)
            {
                TextEditorTextHasChanged(this, position, length);
            }
        }

        private void textViews_SCI_FileDragDrop(object sender, string[] filename)
        {
            if (TextEditorFileDropped != null)
            {
                TextEditorFileDropped(this, filename);
            }
        }



        void textViews_SCI_MouseZoom(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if ((int)textView1.GetFontSize() > 5)
                    SetTextFont(new Font(textView1.StyleGetFont(SciConst.STYLE_DEFAULT),
                                         textView1.StyleGetSize(SciConst.STYLE_DEFAULT) + 1));
            }
            else
            {
                if ((int)textView1.GetFontSize() > 6)
                    SetTextFont(new Font(textView1.StyleGetFont(SciConst.STYLE_DEFAULT),
                                         textView1.StyleGetSize(SciConst.STYLE_DEFAULT) - 1));
            }
            if (TextEditorMouseZoom != null) TextEditorMouseZoom(sender, e);
        }

        public string FileName
        {
            get { return mFileName; }
            set { mFileName = value; }
        }

        public override string Text
        {
            get { return textView1.GetText(); }
            set { textView1.SetText(value); }
        }

        public PrintDocument GetPrintDocument()
        {
            return mPrintDocument;
        }


        public bool Splitted
        {
            get { return !splitContainer1.Panel2Collapsed; }
        }

        public void Split()
        {
            splitContainer1.Orientation = Orientation.Horizontal;
            splitContainer1.SplitterDistance = this.Height / 2;
            splitContainer1.Panel2Collapsed = false;
        }

        public void SplitVertical()
        {
            splitContainer1.Orientation = Orientation.Vertical;
            splitContainer1.SplitterDistance = this.Width / 2;
            splitContainer1.Panel2Collapsed = false;
        }

        public void RemoveSplit()
        {
            splitContainer1.Panel2Collapsed = true;
        }

        public void LoadFile(string fileName)
        {
            textView1.Open(fileName);
            //textView1.Colourise(0, -1); ???
        }

        public void SaveFile(string fileName)
        {
            textView1.Save(fileName);
        }

        public void SetTextFont(Font f)
        {
            for (Int32 i = 0; i < 33; i++)
            {
                if (!string.IsNullOrEmpty(f.Name))
                {
                    textView1.StyleSetFont(i, f.Name);
                    textView2.StyleSetFont(i, f.Name);
                }
                textView1.StyleSetSize(i, (int)f.Size);
                textView2.StyleSetSize(i, (int)f.Size);
            }
            textView1.SetZoom(0);
            textView2.SetZoom(0);
        }

        public Font GetTextFont()
        {
            return new Font(textView1.StyleGetFont(SciConst.STYLE_DEFAULT),
                            textView1.StyleGetSize(SciConst.STYLE_DEFAULT));
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (oldFocusedEditor != null)
            {
                oldFocusedEditor.SetFocus();
            }
            else textView1.SetFocus();
        }

        private void splitContainer1_MouseDown(object sender, MouseEventArgs e)
        {
            if (textView1.Focused) oldFocusedEditor = textView1;
            else oldFocusedEditor = textView2;

            GetFocusedEditor.HideAutoCompletion();
        }

        public TextView GetFocusedEditor
        {
            get
            {
                if (textView2.Focused)
                {
                    return textView2;
                }
                else if (textView1.Focused)
                {
                    return textView1;
                }
                else
                {
                    return oldFocusedEditor;
                    //if (SCITextEditor.textView2.Visible == true)
                    //{
                    //    return oldFocusedEditor;
                    //}
                    //else return SCITextEditor.textView1;
                }
            }
        }

        public void SetFocus()
        {
            GetFocusedEditor.SetFocus();
        }

        public void RemoveFocus()
        {
            GetFocusedEditor.RemoveFocus();
        }

        //void SetFocusOnPrimaryArea();
        public void SetFocusOnPrimaryArea()
        {
            if (this.Splitted)
                textView1.SetFocus();
        }
        //void SetFocusOnSecondaryArea();
        public void SetFocusOnSecondaryArea()
        {
            if (this.Splitted)
                textView2.SetFocus();
        }

        public void SetHighlight(string LanguageName, string Path)
        {
            if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
                textView1.LoadLexerLibrary(Path);

            if (!string.IsNullOrEmpty(LanguageName))
                textView1.SetLexerLanguage(LanguageName);
            else
                textView1.SetLexer(1); //SCLEX_NULL

            //MessageBox.Show(textView1.GetLexer().ToString());
        }

        public void SetKeyWords(int KeyWordSet, string KeyWordList)
        {
            textView1.SetKeyWords(KeyWordSet, KeyWordList);
            textView2.SetKeyWords(KeyWordSet, KeyWordList);
        }

        public void SetWordChars(string WordChars)
        {
            textView1.SetWordChars(WordChars);
            textView2.SetWordChars(WordChars);
        }

        public void SetTextStyle(int StyleNumber, int StyleFore, int StyleBack,
                             bool StyleBold, bool StyleItalic)
        {
            textView1.StyleSetFore(StyleNumber, StyleFore);
            textView1.StyleSetBack(StyleNumber, StyleBack);
            textView1.StyleSetBold(StyleNumber, StyleBold);
            textView1.StyleSetItalic(StyleNumber, StyleItalic);

            textView2.StyleSetFore(StyleNumber, StyleFore);
            textView2.StyleSetBack(StyleNumber, StyleBack);
            textView2.StyleSetBold(StyleNumber, StyleBold);
            textView2.StyleSetItalic(StyleNumber, StyleItalic);
        }



        //void SetCodePage(int codepage);     //... only for Scintilla
        public void SetCodePage(int codepage)
        {
            textView1.SetCodePage((uint)codepage);
            textView2.SetCodePage((uint)codepage);
        }
        //void SetText(string text);
        public void SetText(string text)
        {
            textView1.SetText(text);
        }
        //string GetText();
        public string GetText()
        {
            return textView1.GetText();
        }
        //string GetTextLine();
        public string GetTextLine(int linenumber)
        {
            return GetFocusedEditor.GetLine(linenumber);
        }

        //int GetCurrentLineNumber();
        public int GetCurrentLineNumber()
        {
            //return GetFocusedEditor.GetCurrentPos();
            return GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetCurrentPos());
        }
        //int GetLineLength(int linenumber);
        public int GetLineLength(int linenumber)
        {
            return GetFocusedEditor.LineLength(linenumber);
        }
        //void ClearAllText();
        public void ClearAllText()
        {
            //SCIText = "";
            textView1.ClearAll();
            Refresh();
        }
        //int GetCaretPosition();
        public int GetCaretPosition()
        {
            return GetFocusedEditor.GetCurrentPos();
        }
        //void SetCaretPosition();
        public void SetCaretPosition(int position)
        {
            //GoToPos ??????
            //GetFocusedEditor.SetCurrentPos(position);
            GetFocusedEditor.GotoPos(position);
        }
        //void GoToLine(int linenumber);
        public void GoToLine(int linenumber)
        {
            GetFocusedEditor.GotoLine(linenumber);
        }

        //void InsertText(int position, string text);
        public void InsertText(int position, string text)
        {
            //textView1.InsertText(position, text);
            GetFocusedEditor.InsertText(position, text);
        }
        //void AddText(string text);     //... aggiunge il testo alla posizione corrente
        public void AddText(string text)
        {
            ////textView1.AddText(text);
            GetFocusedEditor.AddText(text);
        }
        //void AppendText(string text);     //... aggiunge il testo alla fine
        public void AppendText(string text)
        {
            //textView1.AppendText(text);
            GetFocusedEditor.AppendText(text);
        }
        //char GetCharAt(int position);
        public char GetCharAt(int position)
        {
            return textView1.GetCharAt(position);
        }
        //int GetTextLength();
        public int GetTextLength()
        {
            return textView1.GetTextLength();
        }
        //int GetLines();     //... TextEditor total number of lines	
        public int GetLines()
        {
            return textView1.GetLineCount();
        }

        //void SetFirstVisibleLine(int linenumber);
        public void SetFirstVisibleLine(int linenumber)
        {
            if (textView2.Focused)
            {
                this.SetFirstVisibleLine(linenumber, 2);
            }
            else this.SetFirstVisibleLine(linenumber, 1);
        }
        //void SetFirstVisibleLine(int linenumber, int view);
        public void SetFirstVisibleLine(int linenumber, int view)
        {
            if (view == 2)
            {
                //Old:
                //textView2.GotoLine(linenumber);
                //textView2.LineScroll(0,
                //    linenumber - textView2.GetFirstVisibleLine());

                //New:
                textView2.SetFirstVisibleLine(linenumber);

                textView2.SetFocus();
            }
            else
            {
                //Old:
                //textView1.GotoLine(linenumber);
                //textView1.LineScroll(0,
                //    linenumber - textView1.GetFirstVisibleLine());

                //New:
                textView1.SetFirstVisibleLine(linenumber);

                textView1.SetFocus();
            }

        }
        //int GetFirstVisibleLine();
        public int GetFirstVisibleLine()
        {
            return GetFocusedEditor.GetFirstVisibleLine();
        }
        //int GetFirstVisibleLine(int view);
        public int GetFirstVisibleLine(int view)
        {
            if (view == 2)
            {
                return textView2.GetFirstVisibleLine();
            }
            else
            {
                return textView1.GetFirstVisibleLine();
            }
        }
        //int LinesOnScreen();
        public int LinesOnScreen()
        {
            return GetFocusedEditor.LinesOnScreen();
        }
        //void Undo();
        public void Undo()
        {
            GetFocusedEditor.Undo();
        }
        //void Redo();
        public void Redo()
        {
            GetFocusedEditor.Redo();
        }
        //void Copy();
        public void Copy()
        {
            GetFocusedEditor.Copy();
        }
        //void Cut();
        public void Cut()
        {
            GetFocusedEditor.Cut();
        }
        //void Paste();
        public void Paste()
        {
            GetFocusedEditor.Paste();
        }
        //void Delete();
        public void Delete()
        {
            GetFocusedEditor.Clear();
        }
        //void SelectAll();
        public void SelectAll()
        {
            GetFocusedEditor.SelectAll();
        }

        //bool IsTextChanged();
        public bool IsTextChanged()
        {
            return GetFocusedEditor.GetModify();
        }
        //void SetSavePoint()
        public void SetSavePoint()
        {
            GetFocusedEditor.SetSavePoint();
        }
        //void EmptyUndoBuffer();
        public void EmptyUndoBuffer()
        {
            GetFocusedEditor.EmptyUndoBuffer();
        }
        //// void SetFont(string name, int size);     ... Si potrebbe usare direttamente la proprietà Font dell'UserControl
        //// string GetFontName();
        //// int GetFontSize();
        //void SetZoom(int zoom);
        public void SetZoom(int zoom)
        {
            textView1.SetZoom(zoom);
            textView2.SetZoom(zoom);
        }
        //int GetZoom();
        public int GetZoom()
        {
            return GetFocusedEditor.GetZoom();
        }




        //Multipurpose Text Finder
        public Int32 FindText(string text, bool MatchWholeWord,
                              bool MatchCase, bool IsBackward,
                              bool SelectText, bool ShowMessage,
                              bool SkipRem)
        {
            return FindText(text, MatchWholeWord, MatchCase, IsBackward, SelectText,
                            ShowMessage, SkipRem, -1, -1);
        }
        //void FindText(string text, string text, bool MatchWholeWord, bool MatchCase, int start, int end);
        public Int32 FindText(string text, bool MatchWholeWord,
                              bool MatchCase, bool IsBackward,
                              bool SelectText, bool ShowMessage,
                              bool SkipRem, int start, int end)
        {
            if (this.GetTextLength() > 0)
            {

                string StringToFind = text;
                //StringToFind = Regex.Escape(StringToFind);

                Int32 mStart = GetFocusedEditor.GetCurrentPos();
                Int32 mEnd = GetFocusedEditor.GetTextLength();
                Int32 mSearchFlags = 0;
                Int32 mFindPos = -1;


                if (start > -1) mStart = start;
                if (end > -1) mEnd = end;

                if (MatchWholeWord)
                {
                    mSearchFlags |= (Int32)SciConst.SCFIND_WHOLEWORD;
                }

                if (MatchCase)
                {
                    mSearchFlags |= (Int32)SciConst.SCFIND_MATCHCASE;
                }

                if (IsBackward)
                {
                    //Search backward
                    mStart = GetFocusedEditor.GetCurrentPos() - 1;
                    mEnd = 0;
                }

                //Search routine
                try
                {
                    GetFocusedEditor.SetSearchFlags(mSearchFlags);

                    do
                    {
                        GetFocusedEditor.SetTargetStart(mStart);
                        GetFocusedEditor.SetTargetEnd(mEnd);

                        mFindPos = GetFocusedEditor.SearchInTarget(StringToFind);
                        if (mFindPos > -1)
                        {
                            mStart = mFindPos + 1;
                            if (!(this.IsRemAt(mFindPos) && SkipRem == true))
                            {
                                if (SelectText == true)
                                {
                                    GetFocusedEditor.SetSelectionStart(mFindPos);
                                    GetFocusedEditor.SetSelectionEnd(mFindPos + StringToFind.Length);
                                    GetFocusedEditor.ScrollCaret();
                                    GetFocusedEditor.SetFocus();
                                    return mFindPos;
                                }
                                else if (SelectText == false)
                                {
                                    return mFindPos;
                                }
                            }
                        }
                        else
                        {
                            if (ShowMessage == true)
                                MessageBox.Show("Text not found",
                                                "Winxound Find and Replace",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                            return -1;
                        }
                    }
                    while (true);

                }

                catch (Exception ex)
                {
                    if (EditorError != null)
                    {
                        EditorError(this, new ErrorEventArgs(ex));
                    }
                    return -1;
                }

            }
            return -1;

        }

        ////ArrayList SearchText();
        public ArrayList SearchText(string text, bool MatchWholeWord,
                                    bool MatchCase, bool IsBackward, bool SkipRem)
        {
            return SearchText(text, MatchWholeWord, MatchCase, IsBackward, SkipRem, -1);
        }

        public ArrayList SearchText(string text, bool MatchWholeWord,
                                    bool MatchCase, bool IsBackward, bool SkipRem,
                                    int start)
        {
            if (this.GetTextLength() > 0)
            {

                ArrayList mMatches = new ArrayList();

                string StringToFind = text;
                Int32 mStart = 0;
                if (start > 0) mStart = start;
                Int32 mFindPos = -1;

                do
                {
                    mFindPos = this.FindText(StringToFind, MatchWholeWord, MatchCase,
                                             IsBackward, false, false, SkipRem,
                                             mStart, this.GetTextLength());
                    if (mFindPos > -1)
                    {
                        mStart = mFindPos + 1;
                        mMatches.Add(mFindPos);
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                return mMatches;

            }
            else return null;
        }
        //void ReplaceTarget(int offset, int length, string text);
        public void ReplaceTarget(int offset, int length, string ReplaceString)
        {
            GetFocusedEditor.SetTargetStart(offset);
            GetFocusedEditor.SetTargetEnd(offset + length);
            GetFocusedEditor.ReplaceTarget(-1, ReplaceString);
        }
        //void ReplaceText(int offset, int length, string text);
        public void ReplaceText(int offset, int length, string text)
        {
            //ICSTextEditor.Document.Replace(offset, length, text);
        }
        //void ReplaceText(string ReplaceString)
        public void ReplaceText(string ReplaceString)
        {
            if (!string.IsNullOrEmpty(this.GetSelectedText()))
            {
                this.SetSelectedText(ReplaceString);
                this.Refresh();
                this.SetFocus();
            }
        }
        //void ReplaceAllText(...)
        public void ReplaceAllText(string StringToFind,
                                   string ReplaceString,
                                   bool MatchWholeWord,
                                   bool MatchCase,
                                   bool FromCaretPosition,
                                   bool FCPUp)
        {
            if (this.GetTextLength() > 0)
            {

                Int32 mStart = 0;
                Int32 mEnd = this.GetTextLength();
                Int32 mFindPos = -1;
                Int32 mSearchFlags = 0;
                Int32 mTotalOcc = 0;

                if (FromCaretPosition)
                {
                    mStart = this.GetCaretPosition();
                    if (FCPUp)
                    {
                        mStart = GetFocusedEditor.GetCurrentPos() - 1;
                        mEnd = 0;
                    }
                }


                try
                {
                    do
                    {
                        GetFocusedEditor.SetTargetStart(mStart);
                        GetFocusedEditor.SetTargetEnd(mEnd);

                        if (MatchWholeWord)
                        {
                            mSearchFlags |= (Int32)SciConst.SCFIND_WHOLEWORD;
                        }
                        if (MatchCase)
                        {
                            mSearchFlags |= (Int32)SciConst.SCFIND_MATCHCASE;
                        }
                        GetFocusedEditor.SetSearchFlags(mSearchFlags);

                        mFindPos = GetFocusedEditor.SearchInTarget(StringToFind);
                        if (mFindPos > -1)
                        {
                            mStart = mFindPos + ReplaceString.Length;
                            GetFocusedEditor.SetSelectionStart(mFindPos);
                            GetFocusedEditor.SetSelectionEnd(mFindPos + StringToFind.Length);
                            GetFocusedEditor.ReplaceSel(ReplaceString);
                            mTotalOcc++;
                        }
                        else
                        {
                            if (mStart == 0)
                            {
                                MessageBox.Show("Text not found", "Winxound Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(mTotalOcc.ToString() + " occurence(s) replaced.",
                                                "Winxound Find and Replace",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                            }

                            break;
                        }
                    }

                    while (true);



                }
                catch (Exception ex)
                {
                    //wxGlobal.wxMessageError(ex.Message, "Form Find and Replace - ButtonReplaceAll_Click Error");
                }
            }
        }


        //int GetLineNumberFromPosition(int position);
        public int GetLineNumberFromPosition(int position)
        {
            return GetFocusedEditor.LineFromPosition(position);
        }
        //int GetPositionFromLineNumber(int linenumber);
        public int GetPositionFromLineNumber(int linenumber)
        {
            return GetFocusedEditor.PositionFromLine(linenumber);
        }
        //int WordStart(int position);
        public int GetWordStart(int position)
        {
            //return GetFocusedEditor.WordStartPosition(position, true);

            char c = '\0';
            Int32 StartPos = 0;

            for (StartPos = position; StartPos > 0; StartPos--)
            {
                c = GetFocusedEditor.GetCharAt(StartPos - 1);
                if (!IsValidChar(c)) break;
            }
            return StartPos;
        }
        //int WordEnd(int position);
        public int GetWordEnd(int position)
        {
            //return GetFocusedEditor.WordEndPosition(position, true);

            char c = '\0';
            Int32 EndPos = 0;

            for (EndPos = position;
                 EndPos < GetFocusedEditor.GetTextLength(); EndPos++)
            {
                // Find the end of the word. 
                c = GetFocusedEditor.GetCharAt(EndPos);
                if (!IsValidChar(c)) break;
            }

            return EndPos;
        }
        //public string GetCurrentWord()
        public string GetCurrentWord()
        {
            return this.GetWordAt(this.GetCaretPosition());
        }
        //string GetWordAt(int position);
        public string GetWordAt(int position)
        {
            string WordAt = "";
            string tempWord = "";
            char c = '\0';
            Int32 StartPos = 0;
            Int32 EndPos = 0;

            //for (StartPos = GetFocusedEditor.GetCurrentPos(); StartPos > 0; StartPos--)
            for (StartPos = position; StartPos > 0; StartPos--)
            {
                c = GetFocusedEditor.GetCharAt(StartPos - 1);
                if (IsValidChar(c))
                {
                    tempWord += c;
                }
                else break;
            }

            for (Int32 ciclo = tempWord.Length - 1; ciclo >= 0; ciclo--)
            {
                WordAt += tempWord[ciclo];
            }

            //for (EndPos = GetFocusedEditor.GetCurrentPos();
            //     EndPos < GetFocusedEditor.GetTextLength(); EndPos++)
            for (EndPos = position;
                 EndPos < GetFocusedEditor.GetTextLength(); EndPos++)
            {
                // Find the end of the word. 
                c = GetFocusedEditor.GetCharAt(EndPos);
                if (IsValidChar(c))
                {
                    WordAt += c;
                }
                else break;
            }
            return WordAt;
        }
        private bool IsValidChar(char c)
        {
            return (char.IsLetterOrDigit(c) || char.IsNumber(c) || c == '_' ||
                    c == '$' || c == '#');
        }

        public Point GetQuotesPosition(Int32 position)
        {

            Int32 mStart = 0;
            Int32 mEnd = 0;
            Int32 mCaretPosition = position; //tEditor.textEditor.GetCaretPosition();
            Int32 mCurrentLineNumber = this.GetLineNumberFromPosition(position); //tEditor.textEditor.GetCurrentLineNumber();

            for (Int32 c = mCaretPosition;
                 c >= this.GetPositionFromLineNumber(mCurrentLineNumber);
                 c--)
            {
                if (this.GetCharAt(c) == '"' ||
                    this.GetCharAt(c) == '\'')
                {
                    mStart = c + 1;
                    break;
                }
            }

            for (Int32 c = mCaretPosition;
                 c <= this.GetLineEndPosition(mCurrentLineNumber); //this.GetPositionFromLineNumber(mCurrentLineNumber);
                 c++)
            {
                if (this.GetCharAt(c) == '"' ||
                    this.GetCharAt(c) == '\'')
                {
                    mEnd = c;
                    break;
                }
            }

            return new Point(mStart, mEnd);
        }

        public string GetTextInQuotes(Int32 position)
        {
            string mString = "";

            Point p = this.GetQuotesPosition(position);

            //if ((mEnd - mStart) > 0 && mStart >= 0)
            if ((p.Y - p.X) > 0 && p.X >= 0)
            {
                //mString = this.GetText().Substring(mStart, mEnd - mStart);
                mString = this.GetText().Substring(p.X, (p.Y - p.X));
                return mString;
            }

            return "";
        }




        //void SetSelection(int start, int end);
        public void SetSelection(int start, int end)
        {
            this.SetSelectionStart(start);
            this.SetSelectionEnd(end);
        }
        //void SetSelectionStart(int start);
        public void SetSelectionStart(int start)
        {
            GetFocusedEditor.SetSelectionStart(start);
        }
        //void SetSelectionEnd(int end);
        public void SetSelectionEnd(int end)
        {
            GetFocusedEditor.SetSelectionEnd(end);
        }
        //int GetSelectionStart();
        public int GetSelectionStart()
        {
            return GetFocusedEditor.GetSelectionStart();
        }
        //int GetSelectionEnd();
        public int GetSelectionEnd()
        {
            return GetFocusedEditor.GetSelectionEnd();
        }
        //string GetSelectedText();
        public string GetSelectedText()
        {
            return GetFocusedEditor.GetSelText();
        }
        //void SetSelectedText(string text);
        public void SetSelectedText(string text)
        {
            GetFocusedEditor.ReplaceSel(text);
        }
        //void InsertRemoveBookmark();
        public void InsertRemoveBookmark()
        {
            //textView1.MarkerSetFore(0, BookmarksForeColor);
            //textView1.MarkerSetBack(0, BookmarksBackColor);
            //textView2.MarkerSetFore(0, BookmarksForeColor);
            //textView2.MarkerSetBack(0, BookmarksBackColor);
            //textView1.MarkerSetAlpha(0, BookmarksAlpha);
            //textView2.MarkerSetAlpha(0, BookmarksAlpha);

            //Int32 mLine = GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetCurrentPos());
            Int32 mLine = this.GetCurrentLineNumber();

            if (GetFocusedEditor.MarkerGet(mLine) > 0) //Bookmark already exist so we remove it
            {
                textView1.MarkerDelete(mLine, 0);
                textView2.MarkerDelete(mLine, 0);
            }
            else //Bookmark doesn't exist so we add it
            {
                textView1.MarkerAdd(mLine, 0);
                textView2.MarkerAdd(mLine, 0);
            }
            //RefreshListBox();
        }
        //void InsertBookmarkAt(Int32 lineNumber);
        public void InsertBookmarkAt(Int32 lineNumber)
        {
            textView1.MarkerAdd(lineNumber, 0);
            textView2.MarkerAdd(lineNumber, 0);
        }
        //void RemoveAllBookmarks();
        public void RemoveAllBookmarks()
        {
            textView1.MarkerDeleteAll(0);
            textView2.MarkerDeleteAll(0);
            //ListBoxBookmarks.Items.Clear();
        }
        //void GoToNextBookmark();
        public void GoToNextBookmark()
        {
            Int32 mLine = GetFocusedEditor.MarkerNext(
                this.GetCurrentLineNumber() + 1, 1);
            if (mLine > -1)
                this.GoToLine(mLine);
        }
        //void GoToPreviousBookmark();
        public void GoToPreviousBookmark()
        {
            Int32 mLine = GetFocusedEditor.MarkerPrevious(
                this.GetCurrentLineNumber() - 1, 1);
            if (mLine > -1)
                this.GoToLine(mLine);
        }
        //bool HasBookmarks();
        public bool HasBookmarks()
        {
            Int32 mLine = this.textView1.MarkerNext(0, 1); //[self MarkerNext:0 markerMask:1];
            return (mLine > -1);
        }
        //int GetStyleAt(int position);
        public int GetStyleAt(int position)
        {
            return GetFocusedEditor.GetStyleAt(position);
        }
        //public bool IsRemAt(int position)
        //{
        //    return (GetFocusedEditor.GetStyleAt(position) == 1 ||
        //            GetFocusedEditor.GetStyleAt(position) == 2);
        //}

        string mLine = "";
        public bool IsRemAt(int position)
        {
            //1. Search for single line rem ";"
            mLine = this.GetTextLine(this.GetLineNumberFromPosition(position));

            //We must replace tab chars with spaces 
            //otherwise IndexOf position doesn't correspond
            mLine = mLine.Replace("\t", new string(' ', GetFocusedEditor.GetTabWidth()));

            if (mLine.IndexOf(";") > -1)
            {
                //Int32 temp = GetFocusedEditor.GetColumn(position);
                //Int32 temp2 = mLine.IndexOf(";");
                //System.Diagnostics.Debug.WriteLine(
                //    "IsRemAt-> Scintilla:" + temp + ", IndexOf: " + temp2);

                if (mLine.IndexOf(";") < GetFocusedEditor.GetColumn(position))
                    return true;
            }


            //for the moment we skip here
            return false;


            //2. Search for multi line rem "/*" and "*/"
            Int32 mStart = -1;
            Int32 mEnd = -1;

            //Search before
            GetFocusedEditor.SetTargetStart(position);
            GetFocusedEditor.SetTargetEnd(0);
            mStart = GetFocusedEditor.SearchInTarget("/*");

            //Regex r = new Regex(@"/\*", RegexOptions.RightToLeft);
            //mStart = r.Match(GetFocusedEditor.GetText(), position).Index;

            //Search next
            GetFocusedEditor.SetTargetStart(mStart);
            GetFocusedEditor.SetTargetEnd(GetFocusedEditor.GetTextLength());
            mEnd = GetFocusedEditor.SearchInTarget("*/");

            //r = new Regex(@"\*/");
            //mEnd = r.Match(GetFocusedEditor.GetText(), mStart).Index;

            if (mStart > -1 && mEnd > -1)
            {
                if (mStart < position &&
                    position < mEnd)
                {
                    return true;
                }
            }
            else if (mStart > -1 && mEnd < 0)
            {
                return true;
            }


            //3. if not found
            return false;
        }

        //int GetLineEndPosition(int linenumber);
        public int GetLineEndPosition(int linenumber)
        {
            return GetFocusedEditor.GetLineEndPosition(linenumber);
        }
        //void Comment();
        public void Comment()
        {
            try
            {
                string languagecomment = "";
                if (this.FileName.ToLower().EndsWith(".csd"))
                    languagecomment = ";";
                else if (this.FileName.ToLower().EndsWith(".py") ||
                         this.FileName.ToLower().EndsWith(".pyw"))
                    languagecomment = "#";
                else if (this.FileName.ToLower().EndsWith(".lua"))
                    languagecomment = "--";
                else
                    return;

                Int32 mLineStart = GetFocusedEditor.LineFromPosition(
                    GetFocusedEditor.GetSelectionStart());
                Int32 mLineEnd = GetFocusedEditor.LineFromPosition(
                    GetFocusedEditor.GetSelectionEnd());

                //Disable comment if the end of the selection (caret)
                //is located at the start of the line
                if (GetFocusedEditor.GetSelText().Length > 0)
                {
                    if (mLineEnd > mLineStart) //if multiple lines selected
                    {
                        Int32 curPosition =
                            GetFocusedEditor.GetSelectionEnd() -
                            GetFocusedEditor.PositionFromLine(mLineEnd);
                        //System.Diagnostics.Debug.WriteLine(curPosition);

                        if (curPosition == 0)
                        {
                            mLineEnd -= 1;
                            if (mLineEnd < mLineStart)
                                mLineEnd = mLineStart;
                        }
                    }
                }

                for (Int32 mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
                {
                    GetFocusedEditor.InsertText(
                        GetFocusedEditor.PositionFromLine(mLine),
                        languagecomment);
                }
            }

            catch (Exception ex)
            {
                if (EditorError != null)
                {
                    EditorError(this, new ErrorEventArgs(ex));
                }
            }
        }
        //void UnComment();
        public void UnComment()
        {
            try
            {
                string languagecomment = "";
                if (this.FileName.ToLower().EndsWith(".csd"))
                    languagecomment = ";";
                else if (this.FileName.ToLower().EndsWith(".py") ||
                         this.FileName.ToLower().EndsWith(".pyw"))
                    languagecomment = "#";
                else if (this.FileName.ToLower().EndsWith(".lua"))
                    languagecomment = "--";
                else
                    return;

                Int32 mLineStart = GetFocusedEditor.LineFromPosition(
                    GetFocusedEditor.GetSelectionStart());
                Int32 mLineEnd = GetFocusedEditor.LineFromPosition(
                    GetFocusedEditor.GetSelectionEnd());

                Int32 curPos = 0;
                Int32 curLinePos = 0;
                string curText = "";

                for (Int32 mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
                {
                    curPos = GetFocusedEditor.PositionFromLine(mLine);
                    curText = GetFocusedEditor.GetLine(mLine);
                    curLinePos = curText.IndexOf(languagecomment);
                    if (curLinePos > -1)
                    {
                        GetFocusedEditor.GotoPos(curPos + curLinePos);
                        foreach (char c in languagecomment.ToCharArray())
                        {
                            GetFocusedEditor.Clear(); //Equivalent to: 'Canc' key
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                if (EditorError != null)
                {
                    EditorError(this, new ErrorEventArgs(ex));
                }
            }
        }
        ////PrintDocument GetPrintDocument();
        //public PrintDocument GetPrintDocument()
        //{
        //    return this.GetPrintDocument();
        //}
        //void SelectLine(int linenumber);
        public void SelectLine(int linenumber)
        {
            SelectLine(linenumber, false);
        }
        public void SelectLine(int linenumber, bool SetAsFirstVisibleLine)
        {
            if (SetAsFirstVisibleLine)
            {
                this.SetFirstVisibleLine(linenumber);
            }
            else GetFocusedEditor.ScrollCaret();

            Int32 mPos = GetFocusedEditor.PositionFromLine(linenumber);
            Int32 mEndPos = GetFocusedEditor.GetLineEndPosition(linenumber);
            GetFocusedEditor.SetSelectionStart(mPos);
            GetFocusedEditor.SetSelectionEnd(mEndPos);
            GetFocusedEditor.SetFocus();
        }


        //void ClearSelection();
        public void ClearSelection()
        {
            //ICSTextEditor.ActiveTextAreaControl.SelectionManager.ClearSelection();
        }




        //void SetEolMode(Int32 mode)
        public void SetEolMode(Int32 mode)
        {
            textView1.SetEOLMode(mode);
            textView2.SetEOLMode(mode);
        }
        //Int32 GetEolMode()
        public Int32 GetEolMode()
        {
            return textView1.GetEOLMode();
        }

        public Int32 GetEolModeReal()
        {
            string t = this.GetText();
            if (t.Contains("\r\n"))
            {
                return (Int32)SciConst.SC_EOL_CRLF;
            }
            else if (t.Contains("\r"))
            {
                return (Int32)SciConst.SC_EOL_CR;
            }
            else if (t.Contains("\n"))
            {
                return (Int32)SciConst.SC_EOL_LF;
            }
            else
            {
                return (Int32)SciConst.SC_EOL_CRLF;
            }


            //if (this.FindText("\r\n", false, false, false, false, false, false, 0, 100) > -1)
            //{
            //    return (Int32)SciConst.SC_EOL_CRLF;
            //}
            //else if (this.FindText("\r", false, false, false, false, false, false, 0, 100) > -1)
            //{
            //    return (Int32)SciConst.SC_EOL_CR;
            //}
            //else if (this.FindText("\n", false, false, false, false, false, false, 0, 100) > -1)
            //{
            //    return (Int32)SciConst.SC_EOL_LF;
            //}
            //else
            //{
            //    return (Int32)SciConst.SC_EOL_CRLF;
            //}


        }
        public string GetEolModeReport()
        {
            //Check Line Endings
            //SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)

            string s = this.GetText().Replace("\r\n", "");

            Int32 crlfOccurrences =
                (this.GetText().Length - s.Length) / 2;

            Int32 crOccurrences =
                s.Length - s.Replace("\r", "").Length;

            Int32 lfOccurrences =
                s.Length - s.Replace("\n", "").Length;

            string report =
                (crlfOccurrences > 0 ? "CRLF(" + crlfOccurrences + ") " : "") +
                (crOccurrences > 0 ? "CR(" + crOccurrences + ") " : "") +
                (lfOccurrences > 0 ? "LF(" + lfOccurrences + ") " : "");

            if ((crlfOccurrences > 0 && crOccurrences > 0) ||
               (crlfOccurrences > 0 && lfOccurrences > 0) ||
               (crOccurrences > 0 && lfOccurrences > 0))
                report += " (mixed eols)";

            return report;
        }
        //Convert EOL
        public void ConvertEOL(Int32 eolMode)
        {
            //OLD: (using Scintilla built-in converter)
            //GetFocusedEditor.ConvertEOLS(eolMode);

            ////if (this.GetEolMode() == eolMode) return;

            string newEol = "";
            switch ((uint)eolMode)
            {
                case SciConst.SC_EOL_CRLF:
                    newEol = "\r\n";
                    break;

                case SciConst.SC_EOL_CR:
                    newEol = "\r";
                    break;

                case SciConst.SC_EOL_LF:
                    newEol = "\n";
                    break;

                default:
                    return;
            }

            //1. convert all eols in "\n"
            string temp = this.GetText().Replace("\r\n", "\n").Replace("\r", "\n");

            //2. convert "\n" to desired format
            this.textView1.BeginUndoAction();
            this.SetText(""); //To prevent no dirty document bug we have to delete all text
            //Add this action to SCI_MOD_CONTAINER (for Undo/Redo notification)
            this.textView1.AddUndoAction(1, 0);

            if (newEol != "\n")
                this.SetText(temp.Replace("\n", newEol));
            else
                this.SetText(temp);

            this.textView1.AddUndoAction(2, 0);
            this.textView1.EndUndoAction();

            //3. set Scintilla EOL Mode
            this.SetEolMode(eolMode);

            //4. free resources
            temp = null;
            //GC.Collect();

        }

        public string ConvertEolOfString(string t)
        {
            string newEol = "\r\n";
            Int32 CurrentEolMode = textView1.GetEOLMode();

            switch ((uint)CurrentEolMode)
            {
                case SciConst.SC_EOL_CRLF:
                    newEol = "\r\n";
                    break;

                case SciConst.SC_EOL_CR:
                    newEol = "\r";
                    break;

                case SciConst.SC_EOL_LF:
                    newEol = "\n";
                    break;
            }

            //1. convert all eols in "\n"
            string temp = t.Replace("\r\n", "\n").Replace("\r", "\n");

            //2. convert "\n" to desired format
            if (newEol != "\n")
                return temp.Replace("\n", newEol);
            else
                return temp;

        }

        public bool GetViewEOL()
        {
            return GetFocusedEditor.GetViewEOL();
        }

        //SCI_BRACEMATCH(int pos, int maxReStyle)
        public int BraceMatch(int pos, int maxReStyle)
        {
            return GetFocusedEditor.BraceMatch(pos, maxReStyle);
        }
        //SCI_BRACEHIGHLIGHT(int pos1, int pos2)
        public void BraceHiglight(int pos1, int pos2)
        {
            GetFocusedEditor.BraceHiglight(pos1, pos2);
        }

        public void ScrollCaret()
        {
            GetFocusedEditor.ScrollCaret();
        }


    }
}
