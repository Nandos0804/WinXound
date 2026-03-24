/* Special Thanks to the authors of ScintillaNet project for various useful tips:
 * ChrisRickard
 * fearthecowboy
 * gserack
 * jacobslusser
 * http://scintillanet.codeplex.com/
 */


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace ScintillaTextEditor
{
    public partial class TextView : Control //UserControl
    {

        //private IntPtr mLib;
        private IntPtr mDirectPointer;

        private string mFontName;
        private float mFontSize;
        private bool mFocus = false;
        private Keys mKeyPress;
        private bool mIsSecondaryView = false;
        private bool mRemoveKeyDown = false;

        public Int32 SCI_WIDTH_MARGINS = 53;



        #region " STRUCTURES FOR SCINTILLA "
        //STRUCTURES FOR SCINTILLA
        [StructLayout(LayoutKind.Sequential)]
        public struct NotifyHeader
        {
            public IntPtr hwndFrom;	// environment specific window handle/pointer
            public IntPtr idFrom;	// CtrlID of the window issuing the notification
            public uint code;		// The SCN_* notification code
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCNotification
        {
            public NotifyHeader nmhdr;
            public int position;			// SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND, 
            // SCN_CALLTIPCLICK, SCN_HOTSPOTCLICK, SCN_HOTSPOTDOUBLECLICK
            public char ch;					// SCN_CHARADDED, SCN_KEY
            public int modifiers;			// SCN_KEY
            public int modificationType;	// SCN_MODIFIED
            public IntPtr text;				// SCN_MODIFIED
            public int length;				// SCN_MODIFIED
            public int linesAdded;			// SCN_MODIFIED
            public int message;				// SCN_MACRORECORD
            public IntPtr wParam;			// SCN_MACRORECORD
            public IntPtr lParam;			// SCN_MACRORECORD
            public int line;				// SCN_MODIFIED
            public int foldLevelNow;		// SCN_MODIFIED
            public int foldLevelPrev;		// SCN_MODIFIED
            public int margin;				// SCN_MARGINCLICK
            public int listType;			// SCN_USERLISTSELECTION
            public int x;					// SCN_DWELLSTART, SCN_DWELLEND
            public int y;					// SCN_DWELLSTART, SCN_DWELLEND
            public int token;		        // SCN_MODIFIED with SC_MOD_CONTAINER
            public int annotationLinesAdded;    //SC_MOD_CHANGEANNOTATION
        }

        public struct CharacterRange
        {
            public Int32 cpMin;
            public Int32 cpMax;
        }

        public struct CurrentLine
        {
            public string Text;
            public Int32 CurLinePos;
        }

        public struct TextRange
        {
            public CharacterRange chrg;
            public Int32 lpstrText;
        }

        public struct TextToFind
        {
            public CharacterRange chrg;
            public Int32 lpstrText;
            public CharacterRange chrgText;
        }

        public struct PrintRectangle
        {
            /// <summary>
            /// Left X Bounds Coordinate
            /// </summary>
            public int Left;
            /// <summary>
            /// Top Y Bounds Coordinate
            /// </summary>
            public int Top;
            /// <summary>
            /// Right X Bounds Coordinate
            /// </summary>
            public int Right;
            /// <summary>
            /// Bottom Y Bounds Coordinate
            /// </summary>
            public int Bottom;

            public PrintRectangle(int iLeft, int iTop, int iRight, int iBottom)
            {
                Left = iLeft;
                Top = iTop;
                Right = iRight;
                Bottom = iBottom;
            }
        }

        public struct RangeToFormat
        {
            /// <summary>
            /// The HDC (device context) we print to
            /// </summary>
            public IntPtr hdc;
            /// <summary>
            /// The HDC we use for measuring (may be same as hdc)
            /// </summary>
            public IntPtr hdcTarget;
            /// <summary>
            /// Rectangle in which to print
            /// </summary>
            public PrintRectangle rc;
            /// <summary>
            /// Physically printable page size
            /// </summary>
            public PrintRectangle rcPage;
            /// <summary>
            /// Range of characters to print
            /// </summary>
            public CharacterRange chrg;
        }
        #endregion



        //EVENTS
        public delegate void OnFileDragDrop(object sender, string[] filename);
        public event OnFileDragDrop SCI_FileDragDrop;
        public event EventHandler SCI_SavePointReached;
        public event EventHandler SCI_SavePointLeft;

        //public event EventHandler SCI_ModContainer;
        public delegate void OnSciModContainer(object sender, Int32 token);
        public event OnSciModContainer SCI_ModContainer;

        //public event EventHandler SCI_GotFocus;
        //public event EventHandler SCI_LostFocus;
        public event MouseEventHandler SCI_MouseEvent;
        public event MouseEventHandler SCI_MouseWheel;
        public event EventHandler SCI_UpdateUI;
        //public event EventHandler SCI_Modified;
        public delegate void OnSCI_Modified(object sender, Int32 position, Int32 length);
        public event OnSCI_Modified SCI_Modified;
        public event MouseEventHandler SCI_MouseZoom;
        public event KeyEventHandler KeyAction;

        //EVENT for Autocomplete
        public delegate void OnAutocomplete(object sender, string command);
        public event OnAutocomplete SCI_AutoComplete;




        //TextView Class Constructor
        public TextView()
        {
            //InitializeComponent();
            this.SuspendLayout();
            // 
            // TextView
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "TextView";
            this.Size = new System.Drawing.Size(200, 200);
            //this.Load += new System.EventHandler(this.TextView_Load);
            //this.Resize += new System.EventHandler(this.TextView_Resize);
            this.Cursor = Cursors.IBeam;
            this.ResumeLayout(false);
            //components = new System.ComponentModel.Container();



            //We retrieve the direct pointer of Scintilla to send commands directly 
            mDirectPointer = WinApi.SendMessage(Handle,
                                                SciConst.SCI_GETDIRECTPOINTER,
                                                0, 0);

            ///'Font Settings
            //this.Font = new Font("Courier New", 10);
            this.SetFontName("Courier New");
            this.SetFontSize(10);


            //Margins Settings 
            SCI_WIDTH_MARGINS = this.MeasureTextWidth("999999");

            //NUMBERS
            this.SetMarginTypeN(0, 1);
            ////NON FOLDER SYMBOLS 
            this.SetMarginTypeN(1, 0);
            ////FOLDER SYMBOLS 
            this.SetMarginTypeN(2, 0);

            this.SetMarginWidthN(0, SCI_WIDTH_MARGINS);
            this.SetMarginWidthN(1, 0);
            this.SetMarginWidthN(2, 0);

            //Set context menu to false because we provide our one
            this.UsePopup(false);

            //Set CodePage (Unicode UTF-8: 65001)
            this.SetCodePage(SciConst.SC_CP_UTF8);

            //Clear some Command Keys (CTRL+S)
            this.ClearCmdKey(Keys.S.GetHashCode() | Keys.Control.GetHashCode());
            this.ClearCmdKey(Keys.J.GetHashCode() | Keys.Control.GetHashCode());

            //Set Scintilla ModeEventMask (to filter SCN_MODIFIED event)
            this.SetModeEventMask((int)SciConst.SC_MOD_INSERTTEXT |
                                  (int)SciConst.SC_MOD_DELETETEXT |
                                  //(int)SciConst.SC_PERFORMED_REDO |
                                  //(int)SciConst.SC_PERFORMED_UNDO |
                                  (int)SciConst.SC_MOD_CONTAINER);

            //Set Eol conversion on Paste (and on drag_and_drop)
            this.SetPasteConvertEndings(true);

            //Accept DragDrop Files 
            WinApi.DragAcceptFiles(this.Handle, true);

            this.Resize += new EventHandler(TextView_Resize);
        }

        void TextView_Resize(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("RESIZE");
            if (mRemoveKeyDown)
            {
                if (SCI_AutoComplete != null)
                    SCI_AutoComplete(this, "hide");
            }
        }


        //Various class overrides
        bool sciLexerLoaded = false;
        protected override CreateParams CreateParams
        {
            get
            {
                SetStyle(ControlStyles.UserPaint, false);
                SetStyle(ControlStyles.CacheText, true);

                if (!sciLexerLoaded)
                {
                    /*mLib =*/
                    WinApi.LoadLibrary("SciLexer.dll");
                }

                CreateParams cp = base.CreateParams;
                cp.ClassName = "Scintilla";
                cp.Style = (int)WinApi.WS_CHILD | (int)WinApi.WS_VISIBLE | (int)WinApi.WS_TABSTOP;

                sciLexerLoaded = true;

                return cp;
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && IsHandleCreated)
            {
                //	wi11811 2008-07-28 Chris Rickard
                //	Since we eat the destroy message in WndProc
                //	we have to manually let Scintilla know to
                //	clean up its resources.
                Message destroyMessage = new Message();
                destroyMessage.Msg = WinApi.WM_DESTROY;
                destroyMessage.HWnd = Handle;
                base.DefWndProc(ref destroyMessage);

                System.Diagnostics.Debug.WriteLine("TextView: Dispose");
            }

            //if (disposing && (components != null))
            //{
            //    components.Dispose();
            //}

            base.Dispose(disposing);
        }



        #region " SCINTILLA PINVOKE LIBRARIES "
        //PINVOKE LIBRARY TO CALL SCINTILLA FUNCTIONS
        //, CharSet = CharSet.Auto, SetLastError = false) ???
        [DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        private static extern Int32 Perform(
                    IntPtr directPointer,
                    UInt32 message,
                    UInt32 wParam,
                    UInt32 lParam);

        [DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        private static extern Int32 Perform(
                    IntPtr directPointer,
                    UInt32 message,
                    Int32 wParam,
                    Int32 lParam);

        [DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        private static extern Int32 Perform(
                    IntPtr directPointer,
                    UInt32 message,
                    IntPtr wParam,
                    IntPtr lParam);

        [DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        private static extern Int32 Perform(
                    IntPtr directPointer,
                    UInt32 message,
                    Int32 wParam,
                    byte[] lParam);


        //These two DllImport create some problems between .Net strings and Scintilla
        //UTF-8 string types (strange chars will appear in the text). MarshalAs ???
        //Don't use them!!! (maybe in the future???)
        //[DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        //private static extern Int32 Perform(
        //            IntPtr directPointer,
        //            UInt32 message,
        //            Int32 wParam,
        //            [MarshalAs(UnmanagedType.LPStr)] string lParam);
        //[DllImport("SciLexer.dll", EntryPoint = "Scintilla_DirectFunction")]
        //private static extern Int32 Perform(
        //            IntPtr directPointer,
        //            UInt32 message,
        //            Int32 wParam,
        //            [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);



        ////Other method to send commands (thanks to ScintillaNet for the tip)
        //private Int32 Perform(IntPtr directPointer, uint msg, IntPtr wParam, IntPtr lParam)
        //{
        //    if (!this.IsDisposed)
        //    {
        //        Message m = new Message();
        //        m.Msg = (int)msg;
        //        m.WParam = wParam;
        //        m.LParam = lParam;
        //        m.HWnd = Handle;

        //        //  DefWndProc is the Window Proc associated with the window
        //        //  class for this control created by Windows Forms. It will
        //        //  in turn call Scintilla's DefWndProc Directly. This has 
        //        //  the same net effect as using Scintilla's DirectFunction
        //        //  in that SendMessage isn't used to get the message to 
        //        //  Scintilla but requires 1 less PInvoke and I don't have
        //        //  to maintain the FunctionPointer and "this" reference
        //        DefWndProc(ref m);

        //        return m.Result.ToInt32();
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}
        #endregion



        //[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public override string Text
        {
            get
            {
                return this.GetText();
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    this.ClearAll();
                else
                    this.SetText(value);
            }
        }

        private Int32 MeasureTextWidth(string text)
        {
            return TextRenderer.MeasureText(text, new Font(mFontName, mFontSize)).Width;
            //return this.TextWidth(32, text);
        }

        private string ZeroTerminated(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "\0";
            else if (!text.EndsWith("\0"))
                return text + "\0";

            return text;
        }

        //Useful methods to retrieve Word from WndProc message
        private short HiWord(int pDWord)
        {
            return ((short)(((pDWord) >> 16) & 0xFFFF));
        }
        private short LoWord(int pDWord)
        {
            return ((short)pDWord);
        }




        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);
        //    ControlPaint.DrawBorder3D(e.Graphics, 0, 0, this.Width, this.Height);
        //}

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //  Thanks to ScintillaNet authors for the 'WM_DESTROY' tip:
                //	wi11811 2008-07-28 Chris Rickard
                //	If we get a destroy message we make this window
                //	a message-only window so that it doesn't actually
                //	get destroyed, causing Scintilla to wipe out all
                //	its settings associated with this window handle.
                //	We do send a WM_DESTROY message to Scintilla in the
                //	Dispose() method so that it does clean up its 
                //	resources when this control is actually done with.
                //	Credit (blame :) goes to tom103 for figuring this 
                //	one out.
                case WinApi.WM_DESTROY:
                    if (this.IsHandleCreated)
                    {
                        WinApi.SetParent(this.Handle, WinApi.HWND_MESSAGE);
                        System.Diagnostics.Debug.WriteLine("TextView: WM_DESTROY");
                        return;
                    }
                    break;

                case WinApi.WM_PAINT:
                    base.WndProc(ref m);
                    //OnPaint(new PaintEventArgs(CreateGraphics(), this.ClientRectangle));
                    return;


                //case WinApi.WM_COMMAND:
                //    switch (HiWord(m.WParam.ToInt32()))
                //    {
                //        case (int)SciConst.SCEN_SETFOCUS:
                //            if (SCI_GotFocus != null)
                //                SCI_GotFocus(this, null);
                //            mFocus = true;
                //            break;

                //        case (int)SciConst.SCEN_KILLFOCUS:
                //            if (SCI_LostFocus != null)
                //                SCI_LostFocus(this, null);
                //            mFocus = false;
                //            break;

                //        case (int)SciConst.SCEN_CHANGE:
                //            //if (KeyAction != null)
                //            //    KeyAction(this, new KeyEventArgs();
                //            break;
                //    }
                //    break;


                case WinApi.WM_CONTEXTMENU:
                    if (SCI_MouseEvent != null)
                        SCI_MouseEvent(this,
                                       new MouseEventArgs(MouseButtons.Right, 0,
                                                          LoWord(m.LParam.ToInt32()),
                                                          HiWord(m.LParam.ToInt32()),
                                                          0));
                    return;


                case WinApi.WM_DROPFILES:

                    //Get number of files dropped (0xffffffff // (UINT)-1)
                    Int32 nfiles = WinApi.DragQueryFileA(m.WParam, 0xffffffff, (IntPtr)null, 0);

                    List<string> files = new List<string>();
                    Int32 fileNameLength = 0;

                    for (uint i = 0; i < nfiles; i++)
                    {
                        fileNameLength =
                            WinApi.DragQueryFileA(m.WParam, i, (IntPtr)null, 0) + 1;

                        //byte[] buffer = new byte[1024];
                        //WinApi.DragQueryFileA(m.WParam, i, (IntPtr)VarPtr(buffer), 1024);
                        //files.Add(Marshal.PtrToStringAnsi((IntPtr)(VarPtr(buffer))));

                        //StringBuilder buffer = new StringBuilder(1024);
                        //WinApi.DragQueryFileA(m.WParam, i, buffer, 1024);
                        //files.Add(buffer.ToString());

                        StringBuilder buffer = new StringBuilder(fileNameLength);
                        WinApi.DragQueryFileA(m.WParam, i, buffer, fileNameLength);
                        files.Add(buffer.ToString().Trim());

                    }
                    WinApi.DragFinish(m.WParam);

                    if (SCI_FileDragDrop != null)
                    {
                        SCI_FileDragDrop(this, files.ToArray());
                    }

                    break;

                case WinApi.WM_VSCROLL:
                    //System.Diagnostics.Debug.WriteLine("WM_HSCROLL/WM_VSCROLL");
                    if (mRemoveKeyDown)
                    {
                        if (SCI_AutoComplete != null)
                            SCI_AutoComplete(this, "hide");
                    }
                    break;

            }


            if ((m.Msg ^ 0x2000) != WinApi.WM_NOTIFY)
            {
                switch (m.Msg)
                {
                    //case WinApi.WM_HSCROLL:
                    //case WinApi.WM_VSCROLL:
                    //    System.Diagnostics.Debug.WriteLine("WM_HSCROLL/WM_VSCROLL");
                    //    base.WndProc(ref m);
                    //    return;

                    case WinApi.WM_MOUSEWHEEL:

                        if (mRemoveKeyDown)
                        {
                            if (SCI_AutoComplete != null)
                                SCI_AutoComplete(this, "hide");
                        }

                        if (SCI_MouseWheel != null)
                            SCI_MouseWheel(this,
                                           new MouseEventArgs(MouseButtons.None, 0, 0,
                                                              0, HiWord(m.WParam.ToInt32())));
                        //We deny Scintilla to use the zoom function and provide our one
                        //(CTRL+MouseWheel)
                        if (ModifierKeys == Keys.Control ||
                            ModifierKeys == (Keys.Control | Keys.Alt))
                        {
                            if (SCI_MouseZoom != null)
                                SCI_MouseZoom(this,
                                              new MouseEventArgs(MouseButtons.None, 0, 0,
                                                                 0, HiWord(m.WParam.ToInt32())));
                        }
                        else
                        {
                            base.WndProc(ref m);
                        }
                        return;

                    case WinApi.WM_LBUTTONDOWN:
                        //System.Diagnostics.Debug.WriteLine("WM_LBUTTONDOWN");
                        //System.Diagnostics.Debug.WriteLine(ModifierKeys);
                        if (mRemoveKeyDown)
                        {
                            if (SCI_AutoComplete != null)
                                SCI_AutoComplete(this, "hide");
                        }

                        //Scintilla BUG (2.01 version or major)
                        //If we click on margins area we must deselect any rectangular selection
                        if (this.SelectionIsRectangle() &&
                            LoWord(m.LParam.ToInt32()) <= SCI_WIDTH_MARGINS)
                        {
                            this.ClearSelections();
                        }

                        //Scintilla BUG (2.01 version or major)
                        //If we click to editor area to extend selection and alt modifier is false, 
                        //we must deselect any Rectangular selection
                        bool shift = (ModifierKeys & Keys.Shift) != 0;
                        bool alt = (ModifierKeys & Keys.Alt) != 0;
                        //System.Diagnostics.Debug.WriteLine(shift + "," + alt);
                        if (this.SelectionIsRectangle() &&
                            shift == true &&
                            alt == false)
                        {
                            int curpos = this.GetAnchor(); //SCI_GETANCHOR parameter:0];
                            this.ClearSelections();
                            this.SetSelectionStart(curpos);
                            this.SetSelectionEnd(curpos);
                        }


                        //If modifiers is not Control+Alt (these are needed for String selection)
                        //We send the message to our base class (Scintilla)
                        if (ModifierKeys != (Keys.Control | Keys.Alt))
                            base.WndProc(ref m);

                        //Raise SCI_MouseEvent
                        if (SCI_MouseEvent != null)
                            SCI_MouseEvent(this,
                                           new MouseEventArgs(MouseButtons.Left, 0,
                                                              LoWord(m.LParam.ToInt32()),
                                                              HiWord(m.LParam.ToInt32()),
                                                              0));

                        return;

                }

                base.WndProc(ref m);
                return;
            }


            //if (m.Msg == WinApi.WM_NOTIFY)
            if ((m.Msg ^ 0x2000) == WinApi.WM_NOTIFY)
            {
                SCNotification scn = (SCNotification)Marshal.PtrToStructure(m.LParam, typeof(SCNotification));
                //NativeScintillaEventArgs nsea = new NativeScintillaEventArgs(m, scn);

                switch (scn.nmhdr.code)
                {
                    case SciConst.SCN_URIDROPPED:
                        break;

                    case SciConst.SCN_AUTOCSELECTION:
                        break;

                    case SciConst.SCN_CALLTIPCLICK:
                        break;

                    case SciConst.SCN_CHARADDED:
                        //System.Diagnostics.Debug.WriteLine("SCN_CHARADDED");
                        Perform(mDirectPointer, SciConst.SCI_BEGINUNDOACTION, 0, 0);
                        Perform(mDirectPointer, SciConst.SCI_ENDUNDOACTION, 0, 0);
                        break;

                    case SciConst.SCN_DOUBLECLICK:
                        break;

                    case SciConst.SCN_DWELLEND:
                        //System.Diagnostics.Debug.WriteLine("SCN_DWELLEND");
                        break;

                    case SciConst.SCN_DWELLSTART:
                        break;

                    case SciConst.SCN_HOTSPOTCLICK:
                        break;

                    case SciConst.SCN_HOTSPOTDOUBLECLICK:
                        break;

                    case SciConst.SCN_INDICATORCLICK:
                        break;

                    case SciConst.SCN_INDICATORRELEASE:
                        break;

                    case SciConst.SCN_KEY:
                        break;

                    case SciConst.SCN_MACRORECORD:
                        break;

                    case SciConst.SCN_MARGINCLICK:
                        //System.Media.SystemSounds.Beep.Play();
                        break;

                    case SciConst.SCN_MODIFIED:

                        if (mIsSecondaryView == false)
                        {
                            //if ((scn.modificationType & (int)SciConst.SC_PERFORMED_UNDO) != 0)
                            //    System.Diagnostics.Debug.WriteLine("SC_PERFORMED_UNDO");

                            //if ((scn.modificationType & (int)SciConst.SC_PERFORMED_REDO) != 0)
                            //    System.Diagnostics.Debug.WriteLine("SC_PERFORMED_REDO");

                            if ((scn.modificationType & (int)SciConst.SC_MOD_CONTAINER) != 0)
                            {
                                //System.Diagnostics.Debug.WriteLine("SC_MOD_CONTAINER:CONVERT_EOL!!! -> TOKEN: " + scn.token);
                                if (SCI_ModContainer != null)
                                    SCI_ModContainer(this, scn.token);
                            }
                        }

                        if (scn.text != IntPtr.Zero && mIsSecondaryView == false)
                        {
                            //System.Diagnostics.Debug.WriteLine(
                            //        Marshal.PtrToStringAnsi(scn.text));
                            //System.Diagnostics.Debug.WriteLine("SCN_MODIFIED");
                            if (SCI_Modified != null)
                                SCI_Modified(this, scn.position, scn.length);
                        }
                        break;

                    case SciConst.SCN_MODIFYATTEMPTRO:
                        break;

                    case SciConst.SCN_NEEDSHOWN:
                        //System.Diagnostics.Debug.WriteLine("SCN_NEEDSHOWN");
                        break;

                    case SciConst.SCN_PAINTED:
                        //System.Diagnostics.Debug.WriteLine("SCN_PAINTED");
                        break;

                    case SciConst.SCN_SAVEPOINTLEFT:
                        if (mIsSecondaryView == false)
                        {
                            //System.Diagnostics.Debug.WriteLine("SCN_SAVEPOINTLEFT");
                            if (SCI_SavePointLeft != null)
                                SCI_SavePointLeft(this, null);
                        }
                        break;

                    case SciConst.SCN_SAVEPOINTREACHED:
                        if (mIsSecondaryView == false)
                        {
                            //System.Diagnostics.Debug.WriteLine("SCN_SAVEPOINTREACHED");
                            if (SCI_SavePointReached != null)
                                SCI_SavePointReached(this, null);
                        }
                        break;

                    case SciConst.SCN_STYLENEEDED:
                        //System.Diagnostics.Debug.WriteLine("SCN_STYLENEEDED");
                        break;

                    case SciConst.SCN_UPDATEUI:
                        //System.Diagnostics.Debug.WriteLine("SCN_UPDATEUI");
                        if (SCI_UpdateUI != null)
                            SCI_UpdateUI(this, null);
                        break;

                    case SciConst.SCN_USERLISTSELECTION:
                        break;

                    case SciConst.SCN_ZOOM:
                        break;

                }
            }

            base.WndProc(ref m);
        }


        public override bool PreProcessMessage(ref Message msg)
        {

            if (mRemoveKeyDown)
            {
                if ((Keys)msg.WParam.ToInt32() == Keys.Escape)
                {
                    if (SCI_AutoComplete != null)
                        SCI_AutoComplete(this, "hide");
                    return true; //We eat the message
                    ////return base.PreProcessMessage(ref msg);
                }
            }

            mKeyPress = Keys.None;

            switch (msg.Msg)
            {
                case WinApi.WM_KEYDOWN:

                    if (Control.ModifierKeys == Keys.None)
                    {
                        mKeyPress = (Keys)msg.WParam.ToInt32();
                        switch (mKeyPress)
                        {
                            case Keys.F1:
                            case Keys.F2:
                            case Keys.F3:
                            case Keys.F4:
                            case Keys.F5:
                            case Keys.F6:
                            case Keys.F7:
                            case Keys.F8:
                            case Keys.F9:
                            case Keys.F10:
                            case Keys.F11:
                            case Keys.F12:
                                return base.PreProcessMessage(ref msg);

                            case Keys.Escape:
                                //if (KeyAction != null)
                                //    KeyAction(this, new KeyEventArgs(Keys.Escape));
                                return base.PreProcessMessage(ref msg);

                            case Keys.Tab:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "tab");
                                    return true;
                                }
                                return false;

                            case Keys.Up:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "up");
                                    return true;
                                }
                                return false;

                            case Keys.Down:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "down");
                                    return true;
                                }
                                return false;

                            case Keys.Left:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "left");
                                }
                                return false;

                            case Keys.Right:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "right");
                                }
                                return false;

                            case Keys.Enter:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "enter");
                                    return true;
                                }
                                //return base.PreProcessMessage(ref msg);
                                break;

                            case Keys.Space:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "space");
                                    return true;
                                }
                                //return base.PreProcessMessage(ref msg);
                                break;

                        }
                    }
                    //ALT
                    //if (Control.ModifierKeys == Keys.Alt)
                    //{
                    //}
                    //SHIFT
                    //if (Control.ModifierKeys == Keys.Shift)
                    //{
                    //}
                    //CONTROL 
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        //Todo: Use Select Case to allow only good keys 
                        mKeyPress = (Keys)msg.WParam.ToInt32();
                        //System.Diagnostics.Debug.WriteLine(mKeyPress);
                        switch (mKeyPress)
                        {
                            //case Keys.Q:
                            //case Keys.H:
                            //case Keys.J:
                            case Keys.OemBackslash:
                            case Keys.Oem1:
                                return true;

                            case Keys.Enter:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "ctrl+shift+enter");
                                    return true;
                                }
                                if (SCI_AutoComplete != null)
                                    SCI_AutoComplete(this, "ctrl+enter");
                                return true;

                        }
                        return base.PreProcessMessage(ref msg);
                    }

                    //CONTROL + SHIFT 
                    if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                    {
                        //Todo: Use Select Case to allow only good keys 
                        mKeyPress = (Keys)msg.WParam.ToInt32();
                        switch (mKeyPress)
                        {
                            case Keys.D1:
                            case Keys.D2:
                            case Keys.D3:
                            case Keys.D4:
                            case Keys.D5:
                            case Keys.B:
                            case Keys.R:
                            case Keys.I:
                            case Keys.Right:
                            case Keys.Left:
                            case Keys.OemBackslash:
                            case Keys.K:
                                return base.PreProcessMessage(ref msg);

                                //case Keys.Enter:
                                //    if (mRemoveKeyDown)
                                //    {
                                //        if (SCI_AutoComplete != null)
                                //            SCI_AutoComplete(this, "ctrl+shift+enter");
                                //        return true;
                                //    }
                                //    break;

                        }

                        //Return MyBase.PreProcessMessage(msg) 
                        return true;
                    }

                    //CONTROL + ALT (ALTGR)
                    if (Control.ModifierKeys == (Keys.Control | Keys.Alt))
                    {
                        mKeyPress = (Keys)msg.WParam.ToInt32();
                        switch (mKeyPress)
                        {
                            case Keys.Enter:
                                if (mRemoveKeyDown)
                                {
                                    if (SCI_AutoComplete != null)
                                        SCI_AutoComplete(this, "ctrl+shift+enter");
                                    return true;
                                }
                                if (SCI_AutoComplete != null)
                                    SCI_AutoComplete(this, "ctrl+enter");
                                return true;
                        }
                        return base.PreProcessMessage(ref msg);
                    }
                    break;
            }

            return base.PreProcessMessage(ref msg);
        }


        public void RemoveArrowKeys()
        {
            System.Diagnostics.Debug.WriteLine("RemoveArrowKeys");
            //this.ClearCmdKey(Keys.Up.GetHashCode());
            //this.ClearCmdKey(Keys.Down.GetHashCode());
            mRemoveKeyDown = true;
        }
        public void RestoreArrowKeys()
        {
            System.Diagnostics.Debug.WriteLine("RestoreArrowKeys");
            //this.AssignCmdKey(Keys.Up.GetHashCode(),(int)SciConst.SCI_LINEUP);
            //this.AssignCmdKey(Keys.Down.GetHashCode(), (int)SciConst.SCI_LINEDOWN);
            mRemoveKeyDown = false;
        }


        //FOCUS STUFFS
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            mFocus = false;

            //////if (mRemoveKeyDown)
            //////{
            //////    if (SCI_AutoComplete != null)
            //////        SCI_AutoComplete(this, "hide");
            //////}
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.SetFocus();
            mFocus = true;
        }

        public new bool Focused
        {
            get { return mFocus; }
        }

        public new void Focus()
        {
            this.SetFocus();
        }

        public void SetFocus()
        {
            //bool focus = true;
            Perform(mDirectPointer, SciConst.SCI_SETFOCUS, 1, 0);
            WinApi.SetFocus(Handle); //MAYBE TO REMOVE ???
            //Perform(mDirectPointer, SciConst.SCI_GRABFOCUS, 0, 0);
        }

        public void RemoveFocus()
        {
            Perform(mDirectPointer, SciConst.SCI_SETFOCUS, 0, 0);
        }



        //private void TextView_Resize(object sender, EventArgs e)
        //{
        //    //WinApi.MoveWindow(Handle, 2, 2, this.ClientSize.Width - 4, this.ClientSize.Height - 4, true);
        //    //this.Refresh();
        //}

        //public new void Move(Int32 x, Int32 y, Int32 Width, Int32 Height)
        //{
        //    this.Location = new Point(x, y);
        //    this.Size = new Size(Width, Height);
        //}

        public IntPtr SCIHandle()
        {
            //return mWnd;
            return Handle;
        }

        public void Open(string mFileName)
        {
            try
            {
                if (File.Exists(mFileName))
                {
                    //OLD:
                    //StreamReader reader = new StreamReader(mFileName, Encoding.Default);
                    //this.SetText(reader.ReadToEnd());
                    //reader.Close();

                    ////??? UTF8 = Conversion Problems!?
                    ////this.SetText(File.ReadAllText(mFileName, Encoding.UTF8));
                    this.SetText(File.ReadAllText(mFileName, Encoding.Default));

                }
                else
                {
                    MessageBox.Show("Unable to open file: File not found!", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open file!" + Environment.NewLine +
                                "Error: " + ex.Message,
                                "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save(string mFileName)
        {
            //OLD:
            //StreamWriter writer = new StreamWriter(mFileName, false, Encoding.Default);
            //writer.Write(this.GetText());
            //writer.Close();

            ////??? UTF8 = Conversion Problems!? Lua doesn't compile!!!
            ////File.WriteAllText(mFileName, this.GetText(), Encoding.UTF8);
            File.WriteAllText(mFileName, this.GetText(), Encoding.Default);
        }


        public string GetFontName()
        {
            //return mFontName;
            return this.StyleGetFont(32);
        }

        public Int32 GetFontSize()
        {
            //return (Int32)mFontSize;
            return this.StyleGetSize(32);
        }

        public void SetFontName(string fontname)
        {
            for (Int32 i = 0; i < 33; i++)
            {
                this.StyleSetFont(i, fontname);
            }
            mFontName = fontname;
        }

        public void SetFontSize(int size)
        {
            for (Int32 i = 0; i < 33; i++)
            {
                this.StyleSetSize(i, size);
            }
            mFontSize = size;
        }

        public bool IsSecondaryView
        {
            get { return mIsSecondaryView; }
            set { mIsSecondaryView = value; }
        }

        public void HideAutoCompletion()
        {
            if (mRemoveKeyDown)
            {
                if (SCI_AutoComplete != null)
                    SCI_AutoComplete(this, "hide");
            }
        }





        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////







        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        ///SCI FUNCTIONS 
        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////

        //SCI_GETTEXT(int length, char *text) 
        //This returns length-1 characters of text from the start of the 
        //document plus one terminating 0 character. To collect all the text 
        //in a document, use SCI_GETLENGTH to get the number of characters in the document (nLen), 
        //allocate a character buffer of length nLen+1 bytes, then call 
        //SCI_GETTEXT(nLen+1, char *text). If the text argument is 0 then the 
        //length that should be allocated to store the entire document is returned. 
        //If you then save the text, you should use SCI_SETSAVEPOINT to mark the text as unmodified. 
        public string GetText()
        {
            //System.Diagnostics.Debug.WriteLine(this.GetTextLength());

            //OLD:
            //Int32 TextLength = this.GetTextLength();
            //byte[] buffer = new byte[TextLength + 1];
            //Perform(mDirectPointer, SciConst.SCI_GETTEXT, TextLength + 1, VarPtr(buffer));
            ////'Return System.Text.Encoding.Default.GetString(buffer, 0, TextLength) 
            //return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);


            ////Marshal with stringbuilder
            //Int32 TextLength = this.GetTextLength();
            //StringBuilder s = new StringBuilder(TextLength + 1);
            //Perform(mDirectPointer, SciConst.SCI_GETTEXT, TextLength + 1, s);
            //return s.ToString();


            Int32 TextLength = this.GetTextLength();
            byte[] buffer = new byte[TextLength + 1];
            Perform(mDirectPointer, SciConst.SCI_GETTEXT, TextLength + 1, buffer);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);


        }


        //SCI_GETLINE(int line, char *text)
        //The buffer is not terminated by a 0 character. 
        //It is up to you to make sure that the buffer is long enough for the text, 
        //use SCI_LINELENGTH(int line).
        public string GetLine(Int32 Line)
        {
            //OLD:
            //Int32 TextLength = this.LineLength(Line);
            //byte[] buffer = new byte[TextLength];
            //Perform(mDirectPointer, SciConst.SCI_GETLINE, Line, VarPtr(buffer));
            //string temp = System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
            //temp = temp.TrimEnd("\r\n".ToCharArray());
            //return temp;


            ////Marshal with stringbuilder (Note: marshaler add N+1 length to StringBuilder)
            //Int32 lineLength = this.LineLength(Line) - 1;
            //StringBuilder s = new StringBuilder(lineLength);
            //Perform(mDirectPointer, SciConst.SCI_GETLINE, Line, s);
            //string temp = s.ToString().TrimEnd("\r\n".ToCharArray());
            //return temp;


            Int32 lineLength = this.LineLength(Line);
            byte[] buffer = new byte[lineLength];
            Perform(mDirectPointer, SciConst.SCI_GETLINE, Line, buffer);
            string temp = System.Text.Encoding.UTF8.GetString(buffer, 0, lineLength);
            temp = temp.TrimEnd("\r\n".ToCharArray());
            return temp;

        }

        public string GetLineWithEOL(Int32 Line)
        {
            Int32 lineLength = this.LineLength(Line);
            byte[] buffer = new byte[lineLength];
            Perform(mDirectPointer, SciConst.SCI_GETLINE, Line, buffer);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, lineLength);
        }


        //SCI_GETCURLINE(int textLen, char *text)
        //Pass in char* text pointing at a buffer large enough to hold 
        //the text you wish to retrieve and a terminating 0 character
        public CurrentLine GetCurLine()
        {
            Int32 lineLength = Perform(mDirectPointer, SciConst.SCI_GETCURLINE, 0, 0);
            byte[] buffer = new byte[lineLength + 1];

            CurrentLine mData;
            mData.CurLinePos =
                Perform(mDirectPointer, SciConst.SCI_GETCURLINE, lineLength, buffer);
            mData.Text = System.Text.Encoding.UTF8.GetString(buffer, 0, lineLength);

            return mData;
        }

        //SCI_LINELENGTH(int line) 
        public Int32 LineLength(Int32 Line)
        {
            return Perform(mDirectPointer, SciConst.SCI_LINELENGTH, Line, 0);
        }

        //SCI_CLEARALL 
        public void ClearAll()
        {
            Perform(mDirectPointer, SciConst.SCI_CLEARALL, 0, 0);
        }

        //SCI_SETTEXT(<unused>, const char *text)
        //This replaces all the text in the document 
        //with the zero terminated text string you pass in.
        public void SetText(string text)
        {
            //OLD:
            //byte[] b = System.Text.Encoding.UTF8.GetBytes((text));
            //Perform(mDirectPointer, SciConst.SCI_SETTEXT, 0, VarPtr(b));


            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            Perform(mDirectPointer, SciConst.SCI_SETTEXT, 0, b);


            ////Problems with UTF-8 encoding...
            ////WITH MARSHAL [MarshalAs(UnmanagedType.LPStr)] string lParam
            ////Marshal add directly a Zero termination to the string
            //Perform(mDirectPointer, SciConst.SCI_SETTEXT, 0, text);

        }

        //SCI_INSERTTEXT(int pos, const char *text) 
        //This inserts the zero terminated text string at position pos.
        public void InsertText(Int32 pos, string text)
        {
            //OLD:
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //Perform(mDirectPointer, SciConst.SCI_INSERTTEXT, pos, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            Perform(mDirectPointer, SciConst.SCI_INSERTTEXT, pos, b);
        }

        //SCI_ADDTEXT(int length, const char *s) 
        //This inserts the first length characters from the string s at the current position. 
        //This will include any 0's in the string that you might have expected to stop 
        //the insert operation. The current position is set at the end of the inserted text, 
        //but it is not scrolled into view.
        public void AddText(string text)
        {
            //OLD:
            //Int32 Length = text.Length;
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //Perform(mDirectPointer, SciConst.SCI_ADDTEXT, Length, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(text);
            Perform(mDirectPointer, SciConst.SCI_ADDTEXT, text.Length, b);
        }

        //SCI_APPENDTEXT(int length, const char *s)
        //This adds the first length characters from the string s to the end 
        //of the document. This will include any 0's in the string that you might 
        //have expected to stop the operation. The current selection is not changed and 
        //the new text is not scrolled into view.
        public void AppendText(string text)
        {
            //Int32 Length = text.Length;
            //byte[] b = System.Text.Encoding.UTF8.GetBytes(text);
            //Perform(mDirectPointer, SciConst.SCI_APPENDTEXT, Length, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(text);
            Perform(mDirectPointer, SciConst.SCI_APPENDTEXT, text.Length, b);
        }

        //SCI_GETCHARAT(int pos) 
        public char GetCharAt(Int32 Pos)
        {
            Int32 KeyDec = Perform(mDirectPointer, SciConst.SCI_GETCHARAT, Pos, 0);
            if (KeyDec > 32 & KeyDec < 128) //??? KeyDec > 0 ???
            {
                return (char)KeyDec;
            }
            else
            {
                //KeyDec Null Char '\0'
                return '\0';
            }
        }

        //SCI_GETTEXTLENGTH 
        public Int32 GetTextLength()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETTEXTLENGTH, 0, 0);
        }
        //SCI_GETLENGTH 
        public Int32 GetLength()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETLENGTH, 0, 0);
        }

        //SCI_GETLINECOUNT 
        public Int32 GetLineCount()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETLINECOUNT, 0, 0);
        }

        //SCI_GETFIRSTVISIBLELINE 
        public Int32 GetFirstVisibleLine()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETFIRSTVISIBLELINE, 0, 0);
        }

        //SCI_LINESONSCREEN 
        public Int32 LinesOnScreen()
        {
            return Perform(mDirectPointer, SciConst.SCI_LINESONSCREEN, 0, 0);
        }

        //SCI_GOTOPOS(int pos) 
        public void GotoPos(Int32 Pos)
        {
            Perform(mDirectPointer, SciConst.SCI_GOTOPOS, Pos, 0);
        }

        //SCI_GOTOLINE(int line) 
        public void GotoLine(Int32 Line)
        {
            Perform(mDirectPointer, SciConst.SCI_GOTOLINE, Line, 0);
        }

        //SCI_GETCURRENTPOS
        public Int32 GetCurrentPos()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETCURRENTPOS, 0, 0);
        }

        //SCI_SELECTALL 
        public void SelectAll()
        {
            Perform(mDirectPointer, SciConst.SCI_SELECTALL, 0, 0);
        }

        //SCI_SETCODEPAGE(int codePage) 
        public void SetCodePage(UInt32 CodePage)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCODEPAGE, CodePage, 0);
        }

        //SCI_GETMODIFY 
        public bool GetModify()
        {
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETMODIFY, 0, 0));
        }

        //SCI_SETSAVEPOINT 
        public void SetSavePoint()
        {
            Perform(mDirectPointer, SciConst.SCI_SETSAVEPOINT, 0, 0);
        }

        //SCI_EMPTYUNDOBUFFER 
        public void EmptyUndoBuffer()
        {
            Perform(mDirectPointer, SciConst.SCI_EMPTYUNDOBUFFER, 0, 0);
        }

        //SCI_STYLESETFONT(int styleNumber, const char *fontName) 
        //The fontName is a zero terminated string holding the name of a font.
        public void StyleSetFont(Int32 styleNumber, string FontName)
        {
            //byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(FontName);
            //Perform(mDirectPointer, SciConst.SCI_STYLESETFONT, styleNumber, VarPtr(buffer));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(FontName));
            Perform(mDirectPointer, SciConst.SCI_STYLESETFONT, styleNumber, b);

            mFontName = FontName;
        }

        //SCI_STYLESETSIZE(int styleNumber, int sizeInPoints) 
        public void StyleSetSize(Int32 styleNumber, Int32 sizeInPoints)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETSIZE, styleNumber, sizeInPoints);

            mFontSize = sizeInPoints;
            if (styleNumber > 31 && styleNumber < 34)
            {
                SCI_WIDTH_MARGINS = this.MeasureTextWidth("999999");
                //If margins are visible then set to new width
                if (this.GetMarginWidthN(0) > 0)
                    this.SetMarginWidthN(0, SCI_WIDTH_MARGINS);
            }
        }

        //SCI_SETZOOM(int zoomInPoints) 
        public void SetZoom(Int32 zoomInPoints)
        {
            Perform(mDirectPointer, SciConst.SCI_SETZOOM, zoomInPoints, 0);
        }

        //SCI_GETZOOM
        public Int32 GetZoom()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETZOOM, 0, 0);
        }


        //TODO:
        ////SCI_FINDTEXT(int searchFlags, TextToFind *ttf) 
        //public Int32 FindText(Int32 searchFlags, TextToFind mTextToFind)
        //{
        //    //TODO: CHECK !!!
        //    return Perform(mDirectPointer, SciConst.SCI_FINDTEXT,
        //                   searchFlags, VarPtr(mTextToFind));
        //}


        //SCI_SEARCHINTARGET(int length, const char *text) 
        //The text string is NOT zero terminated; the size is set by length
        public Int32 SearchInTarget(string text)
        {
            //OLD:
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
            //return Perform(mDirectPointer, SciConst.SCI_SEARCHINTARGET,
            //               text.Length, VarPtr(buffer));

            //With Byte[] Passed Directly
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
            return Perform(mDirectPointer, SciConst.SCI_SEARCHINTARGET,
                           text.Length, buffer);

        }

        //SCI_REPLACETARGET(int length, const char *text)
        //If length is -1, text is a zero terminated string, 
        //otherwise length sets the number of character to replace the target with
        public void ReplaceTarget(Int32 length, string text)
        {
            //OLD:
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
            //Perform(mDirectPointer, SciConst.SCI_REPLACETARGET, length, VarPtr(buffer));

            if (length == -1) text += '\0'; //Added 3.3.0
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
            Perform(mDirectPointer, SciConst.SCI_REPLACETARGET, length, buffer);

        }

        //SCI_SETTARGETSTART(int pos) 
        public void SetTargetStart(Int32 Pos)
        {
            Perform(mDirectPointer, SciConst.SCI_SETTARGETSTART, Pos, 0);
        }

        //SCI_SETTARGETEND(int pos) 
        public void SetTargetEnd(Int32 Pos)
        {
            Perform(mDirectPointer, SciConst.SCI_SETTARGETEND, Pos, 0);
        }

        //SCI_GETTARGETSTART 
        public Int32 GetTargetStart()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETTARGETSTART, 0, 0);
        }

        //SCI_GETTARGETEND 
        public Int32 GetTargetEnd()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETTARGETEND, 0, 0);
        }

        //SCI_SEARCHNEXT(int searchFlags, const char *text)
        //zero terminated text.
        public Int32 SearchNext(Int32 searchFlags, string text)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //return Perform(mDirectPointer, SciConst.SCI_SEARCHNEXT, searchFlags, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            return Perform(mDirectPointer, SciConst.SCI_SEARCHNEXT, searchFlags, b);
        }

        //SCI_SEARCHPREV(int searchFlags, const char *text) 
        //zero terminated text.
        public Int32 SearchPrev(Int32 searchFlags, string text)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //return Perform(mDirectPointer, SciConst.SCI_SEARCHPREV, searchFlags, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            return Perform(mDirectPointer, SciConst.SCI_SEARCHPREV, searchFlags, b);
        }

        //SCI_LINEFROMPOSITION(int pos) 
        public Int32 LineFromPosition(Int32 Pos)
        {
            return Perform(mDirectPointer, SciConst.SCI_LINEFROMPOSITION, Pos, 0);
        }

        //SCI_POSITIONFROMLINE(int line) 
        public Int32 PositionFromLine(Int32 Line)
        {
            return Perform(mDirectPointer, SciConst.SCI_POSITIONFROMLINE, Line, 0);
        }

        //SCI_UNDO 
        public void Undo()
        {
            Perform(mDirectPointer, SciConst.SCI_UNDO, 0, 0);
        }

        //SCI_REDO 
        public void Redo()
        {
            Perform(mDirectPointer, SciConst.SCI_REDO, 0, 0);
        }

        //SCI_CANUNDO 
        public Int32 CanUndo()
        {
            return Perform(mDirectPointer, SciConst.SCI_CANUNDO, 0, 0);
        }

        //SCI_CANREDO 
        public Int32 CanRedo()
        {
            return Perform(mDirectPointer, SciConst.SCI_CANREDO, 0, 0);
        }

        //SCI_CUT 
        public void Cut()
        {
            Perform(mDirectPointer, SciConst.SCI_CUT, 0, 0);
        }

        //SCI_COPY 
        public void Copy()
        {
            Perform(mDirectPointer, SciConst.SCI_COPY, 0, 0);
        }

        //SCI_PASTE 
        public void Paste()
        {
            Perform(mDirectPointer, SciConst.SCI_PASTE, 0, 0);
        }

        //SCI_CLEAR 
        public void Clear()
        {
            Perform(mDirectPointer, SciConst.SCI_CLEAR, 0, 0);
        }

        //SCI_CALLTIPSHOW(int posStart, const char *definition) 
        public void CallTipShow(Int32 posStart, string Definition)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Definition);
            //Perform(mDirectPointer, SciConst.SCI_CALLTIPSHOW, posStart, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(Definition));
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSHOW, posStart, b);
        }

        //SCI_CALLTIPCANCEL 
        public void CallTipCancel()
        {
            Perform(mDirectPointer, SciConst.SCI_CALLTIPCANCEL, 0, 0);
        }

        //SCI_CALLTIPACTIVE 
        public bool CallTipActive()
        {
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_CALLTIPACTIVE, 0, 0));
        }

        //SCI_CALLTIPPOSSTART 
        public Int32 CallTipPosStart()
        {
            return Perform(mDirectPointer, SciConst.SCI_CALLTIPPOSSTART, 0, 0);
        }

        //SCI_CALLTIPSETHLT(int highlightStart, int highlightEnd) 
        public void CallTipSetHLT(Int32 highlightStart, Int32 highlightEnd)
        {
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSETHLT, highlightStart, highlightEnd);
        }

        //SCI_CALLTIPSETBACK(int colour) 
        public void CallTipSetBack(Int32 Colour)
        {
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSETBACK, Colour, 0);
        }

        //SCI_CALLTIPSETFORE(int colour) 
        public void CallTipSetFore(Int32 Colour)
        {
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSETFORE, Colour, 0);
        }

        //SCI_CALLTIPSETFOREHLT(int colour) 
        public void CallTipSetForeHLT(Int32 Colour)
        {
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSETFOREHLT, Colour, 0);
        }

        //SCI_SETMOUSEDWELLTIME 
        //SCI_GETMOUSEDWELLTIME 
        //These two messages set and get the time the mouse must sit still, 
        //in milliseconds, to generate a SCN_DWELLSTART notification. 
        //If set to SC_TIME_FOREVER, the default, no dwell events are generated. 
        public void SetMouseDwellTime(Int32 DwellTime)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMOUSEDWELLTIME, DwellTime, 0);
        }

        public Int32 GetMouseDwellTime()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETMOUSEDWELLTIME, 0, 0);
        }


        //SCI_WORDSTARTPOSITION(int position, bool onlyWordCharacters) 
        public Int32 WordStartPosition(Int32 Position, bool onlyWordCharacters)
        {
            return Perform(mDirectPointer, SciConst.SCI_WORDSTARTPOSITION,
                           Position, Convert.ToInt32(onlyWordCharacters));
        }

        //SCI_WORDENDPOSITION(int position, bool onlyWordCharacters) 
        public Int32 WordEndPosition(Int32 Position, bool onlyWordCharacters)
        {
            return Perform(mDirectPointer, SciConst.SCI_WORDENDPOSITION,
                           Position, Convert.ToInt32(onlyWordCharacters));
        }


        ////TODO:
        ////SCI_GETTEXTRANGE(<unused>, TextRange *tr) 
        //public Int32 GetTextRange(TextRange mTextRange)
        //{
        //    //TODO: CHECK !!!
        //    return Perform(mDirectPointer, SciConst.SCI_GETTEXTRANGE, 0, VarPtr(mTextRange));
        //}


        //SCI_STYLESETFORE(int styleNumber, int colour) 
        public void StyleSetFore(Int32 styleNumber, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETFORE, styleNumber, colour);
        }

        //SCI_STYLESETBACK(int styleNumber, int colour) 
        public void StyleSetBack(Int32 styleNumber, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETBACK, styleNumber, colour);
        }

        //SCI_SETLEXERLANGUAGE(<unused>, const char *name) 
        public void SetLexerLanguage(string Language)
        {
            //byte[] MyLang = System.Text.UTF8Encoding.UTF8.GetBytes(Language);
            //Perform(mDirectPointer, SciConst.SCI_SETLEXERLANGUAGE, 0, VarPtr(MyLang));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(Language));
            Perform(mDirectPointer, SciConst.SCI_SETLEXERLANGUAGE, 0, b);
        }

        //SCI_SETKEYWORDS(int keyWordSet, const char *keyWordList) 
        public void SetKeyWords(Int32 keyWordSet, string KeyWordsList)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(KeyWordsList);
            //Perform(mDirectPointer, SciConst.SCI_SETKEYWORDS, keyWordSet, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(KeyWordsList));
            Perform(mDirectPointer, SciConst.SCI_SETKEYWORDS, keyWordSet, b);
        }

        //SCI_SETTABWIDTH(int widthInChars) 
        public void SetTabWidth(Int32 widthInChars)
        {
            Perform(mDirectPointer, SciConst.SCI_SETTABWIDTH, widthInChars, 0);
        }

        //SCI_GETTABWIDTH 
        public Int32 GetTabWidth()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETTABWIDTH, 0, 0);
        }

        //SCI_SETMARGINTYPEN(int margin, int iType) 
        public void SetMarginTypeN(Int32 Margin, Int32 iType)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, Margin, iType);
        }

        //SCI_GETMARGINTYPEN(int margin) 
        public Int32 GetMarginTypeN(Int32 Margin)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETMARGINTYPEN, Margin, 0);
        }

        //SCI_SETMARGINWIDTHN(int margin, int pixelWidth) 
        public void SetMarginWidthN(Int32 Margin, Int32 pixelWidth)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, Margin, pixelWidth);
        }

        //SCI_GETMARGINWIDTHN(int margin) 
        public Int32 GetMarginWidthN(Int32 Margin)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETMARGINWIDTHN, Margin, 0);
        }

        //SCI_POSITIONFROMPOINT(int x, int y) 
        public Int32 PositionFromPoint(Int32 x, Int32 y)
        {
            return Perform(mDirectPointer, SciConst.SCI_POSITIONFROMPOINT, x, y);
        }
        //SCI_POSITIONFROMPOINTCLOSE(int x, int y) 
        public Int32 PositionFromPointClose(Int32 x, Int32 y)
        {
            return Perform(mDirectPointer, SciConst.SCI_POSITIONFROMPOINTCLOSE, x, y);
        }


        //SCI_POINTXFROMPOSITION(<unused>, int pos) 
        public Int32 PointXfromPosition(Int32 Pos)
        {
            return Perform(mDirectPointer, SciConst.SCI_POINTXFROMPOSITION, 0, Pos);
        }
        //SCI_POINTYFROMPOSITION(<unused>, int pos) 
        public Int32 PointYfromPosition(Int32 Pos)
        {
            return Perform(mDirectPointer, SciConst.SCI_POINTYFROMPOSITION, 0, Pos);
        }

        //SCI_TEXTHEIGHT(int line) 
        public Int32 TextHeight(Int32 Line)
        {
            return Perform(mDirectPointer, SciConst.SCI_TEXTHEIGHT, Line, 0);
        }


        //SCI_SETWORDCHARS(<unused>, const char *chars) 
        public void SetWordChars(string Chars)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Chars);
            //Perform(mDirectPointer, SciConst.SCI_SETWORDCHARS, 0, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(Chars));
            Perform(mDirectPointer, SciConst.SCI_SETWORDCHARS, 0, b);
        }

        //SCI_SETWHITESPACECHARS(<unused>, const char *chars) 
        public void SetWhiteSpaceChars(string SpaceChars)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(SpaceChars);
            //Perform(mDirectPointer, SciConst.SCI_SETWHITESPACECHARS, 0, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(SpaceChars));
            Perform(mDirectPointer, SciConst.SCI_SETWHITESPACECHARS, 0, b);
        }

        //SCI_SETCHARSDEFAULT 
        public void SetCharsDefault()
        {
            Perform(mDirectPointer, SciConst.SCI_SETCHARSDEFAULT, 0, 0);
        }

        //SCI_SETSEARCHFLAGS(int searchFlags) 
        public void SetSearchFlags(Int32 searchFlags)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSEARCHFLAGS, searchFlags, 0);
        }

        //SCI_SETSELECTIONSTART(int pos) 
        public void SetSelectionStart(Int32 pos)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELECTIONSTART, pos, 0);
        }
        //SCI_SETSELECTIONEND(int pos) 
        public void SetSelectionEnd(Int32 pos)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELECTIONEND, pos, 0);
        }

        //SCI_GETSELECTIONSTART 
        public Int32 GetSelectionStart()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETSELECTIONSTART, 0, 0);
        }
        //SCI_GETSELECTIONEND 
        public Int32 GetSelectionEnd()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETSELECTIONEND, 0, 0);
        }

        //SCI_GETSELTEXT(<unused>, char *text)
        //This copies the currently selected text and a terminating 0 byte to the text buffer. 
        //The buffer size should be determined by calling with a NULL pointer for the text 
        //argument SCI_GETSELTEXT(0,0). This allows for rectangular and discontiguous 
        //selections as well as simple selections
        public string GetSelText()
        {
            Int32 TextLength = Perform(mDirectPointer, SciConst.SCI_GETSELTEXT, 0, 0) - 1;

            if (TextLength > 0)
            {
                //OLD:
                //byte[] buffer = new byte[TextLength + 1];
                //Perform(mDirectPointer, SciConst.SCI_GETSELTEXT, 0, VarPtr(buffer));
                //return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);

                byte[] buffer = new byte[TextLength + 1];
                Perform(mDirectPointer, SciConst.SCI_GETSELTEXT, 0, buffer);
                return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
            }
            else
            {
                return "";
            }
        }

        //SCI_GETCURSOR 
        public Int32 GetCursor()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETCURSOR, 0, 0);
        }
        //SCI_SETCURSOR(int curType) 
        public void SetCursor(Int32 curType)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCURSOR, curType, 0);
        }


        //SCI_SETMOUSEDOWNCAPTURES(bool captures) 
        public void SetMouseDownCaptures(bool captures)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMOUSEDOWNCAPTURES,
                    Convert.ToInt32(captures), 0);
        }

        //SCI_GETMOUSEDOWNCAPTURES 
        public Int32 GetMouseDownCaptures()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETMOUSEDOWNCAPTURES, 0, 0);
        }

        //SCI_SETCURRENTPOS(int pos) 
        public void SetCurrentPos(Int32 pos)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCURRENTPOS, pos, 0);
        }

        //SCI_STYLESETCHANGEABLE(int styleNumber, bool changeable) 
        public void StyleSetChangeable(Int32 styleNumber, bool changeable)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETCHANGEABLE, styleNumber,
                    Convert.ToInt32(changeable));
        }

        //SCI_USEPOPUP(bool bEnablePopup) 
        public void UsePopup(bool bEnablePopup)
        {
            Perform(mDirectPointer, SciConst.SCI_USEPOPUP,
                    Convert.ToInt32(bEnablePopup), 0);
        }

        //SCI_REPLACESEL(<unused>, const char *text)
        //The currently selected text between the anchor and the current position 
        //is replaced by the 0 terminated text string
        //Pass Zero terminated text string!!!
        public void ReplaceSel(string text)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //Perform(mDirectPointer, SciConst.SCI_REPLACESEL, 0, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            Perform(mDirectPointer, SciConst.SCI_REPLACESEL, 0, b);
        }


        //SCI_MARKERDEFINE(int markerNumber, int markerSymbols) 
        public void MarkerDefine(Int32 markerNumber, Int32 markerSymbols)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERDEFINE, markerNumber, markerSymbols);
        }


        //TODO
        //SCI_MARKERDEFINEPIXMAP(int markerNumber, const char *xpm) 


        //SCI_MARKERSETFORE(int markerNumber, int colour) 
        public void MarkerSetFore(Int32 markerNumber, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERSETFORE, markerNumber, colour);
        }

        //SCI_MARKERSETBACK(int markerNumber, int colour) 
        public void MarkerSetBack(Int32 markerNumber, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERSETBACK, markerNumber, colour);
        }

        //SCI_MARKERSETALPHA(int markerNumber, int alpha)
        public void MarkerSetAlpha(Int32 markerNumber, Int32 alpha)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERSETALPHA, markerNumber, alpha);
        }

        //SCI_MARKERADD(int line, int markerNumber) 
        public Int32 MarkerAdd(Int32 line, Int32 markerNumber)
        {
            return Perform(mDirectPointer, SciConst.SCI_MARKERADD, line, markerNumber);
        }

        //SCI_MARKERDELETE(int line, int markerNumber) 
        public void MarkerDelete(Int32 line, Int32 markerNumber)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERDELETE, line, markerNumber);
        }

        //SCI_MARKERDELETEALL(int markerNumber) [-1 to delete all] 
        public void MarkerDeleteAll(Int32 markerNumber)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERDELETEALL, markerNumber, 0);
        }

        //SCI_MARKERGET(int line) 
        public Int32 MarkerGet(Int32 line)
        {
            return Perform(mDirectPointer, SciConst.SCI_MARKERGET, line, 0);
        }

        //SCI_MARKERNEXT(int lineStart, int markerMask) 
        public Int32 MarkerNext(Int32 lineStart, Int32 markerMask)
        {
            return Perform(mDirectPointer, SciConst.SCI_MARKERNEXT, lineStart, markerMask);
        }

        //SCI_MARKERPREVIOUS(int lineStart, int markerMask) 
        public Int32 MarkerPrevious(Int32 lineStart, Int32 markerMask)
        {
            return Perform(mDirectPointer, SciConst.SCI_MARKERPREVIOUS, lineStart, markerMask);
        }

        //SCI_MARKERLINEFROMHANDLE(int handle) 
        public Int32 MarkerLineFromHandle(Int32 handle)
        {
            return Perform(mDirectPointer, SciConst.SCI_MARKERLINEFROMHANDLE, handle, 0);
        }

        //SCI_MARKERDELETEHANDLE(int handle) 
        public void MarkerDeleteHandle(Int32 handle)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERDELETEHANDLE, handle, 0);
        }


        //SCI_ASSIGNCMDKEY(int keyDefinition, int sciCommand) 
        public void AssignCmdKey(Int32 keyDefinition, Int32 sciCommand)
        {
            Perform(mDirectPointer, SciConst.SCI_ASSIGNCMDKEY, keyDefinition, sciCommand);
        }

        //SCI_CLEARCMDKEY(int keyDefinition) 
        public void ClearCmdKey(Int32 keyDefinition)
        {
            Perform(mDirectPointer, SciConst.SCI_CLEARCMDKEY, keyDefinition, 0);
        }

        //SCI_CLEARALLCMDKEYS 
        public void ClearAllCmdKeys()
        {
            Perform(mDirectPointer, SciConst.SCI_CLEARALLCMDKEYS, 0, 0);
        }

        //SCI_POSITIONBEFORE(int position) 
        public Int32 PositionBefore(Int32 position)
        {
            return Perform(mDirectPointer, SciConst.SCI_POSITIONBEFORE, position, 0);
        }

        //SCI_POSITIONAFTER(int position) 
        public Int32 PositionAfter(Int32 position)
        {
            return Perform(mDirectPointer, SciConst.SCI_POSITIONAFTER, position, 0);
        }

        //SCI_SETSELFORE(bool useSelectionForeColour, int colour) 
        public void SetSelFore(bool useSelectionForeColour, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELFORE,
                    Convert.ToInt32(useSelectionForeColour), colour);
        }

        //SCI_SETSELBACK(bool useSelectionBackColour, int colour) 
        public void SetSelBack(bool useSelectionForeColour, Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELBACK,
                    Convert.ToInt32(useSelectionForeColour), colour);
        }

        //SCI_SHOWLINES(int lineStart, int lineEnd) 
        public void ShowLines(Int32 lineStart, Int32 lineEnd)
        {
            Perform(mDirectPointer, SciConst.SCI_SHOWLINES, lineStart, lineEnd);
        }

        //SCI_HIDELINES(int lineStart, int lineEnd) 
        public void HideLines(Int32 lineStart, Int32 lineEnd)
        {
            Perform(mDirectPointer, SciConst.SCI_HIDELINES, lineStart, lineEnd);
        }

        //SCI_LINESCROLL(int column, int line) 
        public void LineScroll(Int32 column, Int32 line)
        {
            Perform(mDirectPointer, SciConst.SCI_LINESCROLL, column, line);
        }

        //SCI_SCROLLCARET 
        public void ScrollCaret()
        {
            Perform(mDirectPointer, SciConst.SCI_SCROLLCARET, 0, 0);
        }

        //SCI_SETREADONLY(bool readOnly) 
        public void SetReadOnly(bool status)
        {
            Perform(mDirectPointer, SciConst.SCI_SETREADONLY,
                    Convert.ToInt32(status), 0);
        }

        //SCI_GETREADONLY 
        public bool GetReadOnly()
        {
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETREADONLY, 0, 0));
        }

        //SCI_SETENDATLASTLINE(bool endAtLastLine) 
        public void SetEndAtLastLine(bool endAtLastLine)
        {
            Perform(mDirectPointer, SciConst.SCI_SETENDATLASTLINE,
                    Convert.ToInt32(endAtLastLine), 0);
        }
        //SCI_GETENDATLASTLINE 
        public bool GetEndAtLastLine()
        {
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETENDATLASTLINE, 0, 0));
        }

        //SCI_GETSTYLEAT 
        public Int32 GetStyleAt(Int32 position)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETSTYLEAT, position, 0);
        }

        //SCI_SETWRAPMODE(int wrapMode) 
        public void SetWrapMode(Int32 mType)
        {
            Perform(mDirectPointer, SciConst.SCI_SETWRAPMODE, mType, 0);
        }

        //SCI_SETWRAPVISUALFLAGS(int wrapVisualFlags) 
        public void SetWrapVisualFlags(Int32 mType)
        {
            Perform(mDirectPointer, SciConst.SCI_SETWRAPVISUALFLAGS, mType, 0);
        }

        //SCI_GETLINEENDPOSITION(int line) 
        public Int32 GetLineEndPosition(Int32 line)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETLINEENDPOSITION, line, 0);
        }

        //SCI_SETCARETFORE(int color) 
        public void SetCaretFore(Int32 color)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCARETFORE, color, 0);
        }

        //SCI_STYLESETBOLD(int styleNumber, bool bold) 
        public void StyleSetBold(Int32 styleNumber, bool bold)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETBOLD, styleNumber,
                    Convert.ToInt32(bold));
        }

        //SCI_STYLESETITALIC(int styleNumber, bool italic) 
        public void StyleSetItalic(Int32 styleNumber, bool italic)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETITALIC, styleNumber,
                    Convert.ToInt32(italic));
        }


        //SCI_STYLEGETFONT(int styleNumber, char *fontName) 
        //The fontName is a zero terminated string holding the name of a font.
        public string StyleGetFont(Int32 styleNumber)
        {
            //OLD:
            //Int32 length = Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, 0, 0);
            //byte[] buffer = new byte[length]; //new byte[33];
            //Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, styleNumber, VarPtr(buffer));
            //return Marshal.PtrToStringAnsi((IntPtr)VarPtr(buffer));

            Int32 length = Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, 0, 0);
            byte[] buffer = new byte[length + 1];
            Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, styleNumber, buffer);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        }


        //SCI_STYLEGETSIZE(int styleNumber) 
        public Int32 StyleGetSize(Int32 styleNumber)
        {
            return Perform(mDirectPointer, SciConst.SCI_STYLEGETSIZE, styleNumber, 0);
        }


        //SCI_SETVIEWWS(int wsMode) 
        public void SetViewWS(Int32 wsMode)
        {
            Perform(mDirectPointer, SciConst.SCI_SETVIEWWS, wsMode, 0);
        }

        //SCI_GETVIEWWS 
        public Int32 GetViewWS()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETVIEWWS, 0, 0);
        }

        //SCI_SETWHITESPACEFORE<(bool useWhitespaceForeColour, int colour) 
        public void SetWhiteSpaceFore(bool mUse, Int32 mColor)
        {
            Perform(mDirectPointer, SciConst.SCI_SETWHITESPACEFORE,
                    Convert.ToInt32(mUse), mColor);
        }
        //SCI_SETWHITESPACEBACK(bool useWhitespaceBackColour, int colour) 
        public void SetWhiteSpaceBack(bool mUse, Int32 mColor)
        {
            Perform(mDirectPointer, SciConst.SCI_SETWHITESPACEBACK,
                    Convert.ToInt32(mUse), mColor);
        }

        //SCI_SETSELECTIONMODE(int mode) 
        public void SetSelectionMode(Int32 mode)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELECTIONMODE, mode, 0);
        }

        //SCI_STYLESETHOTSPOT(int styleNumber, bool hotspot) 
        public void StyleSetHotSpot(Int32 styleNumber, bool hotspot)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETHOTSPOT, styleNumber,
                    Convert.ToInt32(hotspot));
        }

        //SCI_TEXTWIDTH(int styleNumber, const char *text) 
        public Int32 TextWidth(Int32 styleNumber, string text)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            //return Perform(mDirectPointer, SciConst.SCI_TEXTWIDTH, styleNumber, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(text));
            return Perform(mDirectPointer, SciConst.SCI_TEXTWIDTH, styleNumber, b);
        }


        //SCI_SETSELALPHA(int alpha) 
        public void SetSelAlpha(Int32 alpha)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSELALPHA, alpha, 0);
        }

        //SCI_GETSELALPHA 
        public Int32 GetSelAlpha()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETSELALPHA, 0, 0);
        }


        //SCI_SETCARETLINEVISIBLE(bool show) 
        public void SetCaretLineVisible(bool show)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCARETLINEVISIBLE,
                    Convert.ToInt32(show), 0);
        }

        //SCI_GETCARETLINEVISIBLE 
        public bool GetCaretLineVisible()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETCARETLINEVISIBLE, 0, 0));
        }

        //SCI_SETCARETLINEBACK(int colour) 
        public void SetCaretLineBack(Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCARETLINEBACK, colour, 0);
        }

        //SCI_GETCARETLINEBACK 
        public Int32 GetCaretLineBack()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETCARETLINEBACK, 0, 0);
        }

        //SCI_SETCARETLINEBACKALPHA(int alpha) 
        public void SetCaretLineBackAlpha(Int32 alpha)
        {
            Perform(mDirectPointer, SciConst.SCI_SETCARETLINEBACKALPHA, alpha, 0);
        }

        //SCI_GETCARETLINEBACKALPHA 
        public Int32 GetCaretLineBackAlpha()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETCARETLINEBACKALPHA, 0, 0);
        }


        //THIS IS NEEDED FOR PRINT!!!
        //SCI_FORMATRANGE(bool bDraw, RangeToFormat *pfr)
        public Int32 FormatRange(bool bDraw, RangeToFormat pfr)
        {
            return (Int32)Perform(mDirectPointer, SciConst.SCI_FORMATRANGE,
                                  Convert.ToInt32(bDraw), VarPtr(pfr));
        }



        //SCI_SETHIGHLIGHTGUIDE(int column)
        public void SetHighlightGuide(Int32 column)
        {
            Perform(mDirectPointer, SciConst.SCI_SETHIGHLIGHTGUIDE, column, 0);
        }

        //SCI_GETHIGHLIGHTGUIDE
        public Int32 GetHighlightGuide()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETHIGHLIGHTGUIDE, 0, 0);
        }

        //SCI_STYLEGETBOLD(int styleNumber)
        public bool StyleGetBold(Int32 styleNumber)
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_STYLEGETBOLD, styleNumber, 0));
        }

        //SCI_STYLEGETITALIC(int styleNumber)
        public bool StyleGetItalic(Int32 styleNumber)
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_STYLEGETITALIC, styleNumber, 0));
        }

        //SCI_STYLEGETFORE(int styleNumber)
        public Int32 StyleGetFore(Int32 styleNumber)
        {
            return Perform(mDirectPointer, SciConst.SCI_STYLEGETFORE, styleNumber, 0);
        }

        //SCI_STYLEGETBACK(int styleNumber)
        public Int32 StyleGetBack(Int32 styleNumber)
        {
            return Perform(mDirectPointer, SciConst.SCI_STYLEGETBACK, styleNumber, 0);
        }

        //SCI_SETEDGEMODE(int edgeMode)
        public void SetEdgeMode(Int32 edgeMode)
        {
            Perform(mDirectPointer, SciConst.SCI_SETEDGEMODE, edgeMode, 0);
        }

        //SCI_GETEDGEMODE
        public Int32 GetEdgeMode()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETEDGEMODE, 0, 0);
        }

        //SCI_SETEDGECOLUMN(int column)
        public void SetEdgeColumn(Int32 column)
        {
            Perform(mDirectPointer, SciConst.SCI_SETEDGECOLUMN, column, 0);
        }

        //SCI_GETEDGECOLUMN
        public Int32 GetEdgeColumn()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETEDGECOLUMN, 0, 0);
        }

        //SCI_SETEDGECOLOUR(int colour)
        public void SetEdgeColour(Int32 colour)
        {
            Perform(mDirectPointer, SciConst.SCI_SETEDGECOLOUR, colour, 0);
        }

        //SCI_GETEDGECOLOUR
        public Int32 GetEdgeColour()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETEDGECOLOUR, 0, 0);
        }

        //SCI_LOADLEXERLIBRARY(<unused>, const char *path)
        public void LoadLexerLibrary(string path)
        {
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(path);
            //Perform(mDirectPointer, SciConst.SCI_LOADLEXERLIBRARY,
            //                        0, VarPtr(b));

            //With Byte[] Passed Directly
            byte[] b = System.Text.Encoding.UTF8.GetBytes(ZeroTerminated(path));
            Perform(mDirectPointer, SciConst.SCI_LOADLEXERLIBRARY, 0, b);
        }

        //SCI_GETLEXER
        public Int32 GetLexer()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETLEXER, 0, 0);
        }

        //SCI_SETLEXER
        public void SetLexer(Int32 lexer)
        {
            Perform(mDirectPointer, SciConst.SCI_SETLEXER, lexer, 0);
        }

        //SCI_GETENDSTYLED
        public Int32 GetEndStyled()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETENDSTYLED, 0, 0);
        }

        //SCI_STARTSTYLING(int pos, int mask)
        public void StartStyling(Int32 pos, Int32 mask)
        {
            Perform(mDirectPointer, SciConst.SCI_STARTSTYLING, pos, mask);
        }

        //SCI_SETSTYLING(int length, int style)
        public void SetStyling(Int32 length, Int32 style)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSTYLING, length, style);
        }

        //SCI_STYLECLEARALL
        public void StyleClearAll()
        {
            Perform(mDirectPointer, SciConst.SCI_STYLECLEARALL, 0, 0);
        }

        //SCI_COLOURISE(int startPos, int endPos)
        public void Colourise(Int32 startPos, Int32 endPos)
        {
            Perform(mDirectPointer, SciConst.SCI_COLOURISE, startPos, endPos);
        }

        //SCI_SETEOLMODE(int eolMode)
        public void SetEOLMode(Int32 eolMode)
        {
            Perform(mDirectPointer, SciConst.SCI_SETEOLMODE, eolMode, 0);
        }

        //SCI_GETEOLMODE
        public Int32 GetEOLMode()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETEOLMODE, 0, 0);
        }

        //SCI_CONVERTEOLS(int eolMode)
        public void ConvertEOLS(Int32 eolMode)
        {
            Perform(mDirectPointer, SciConst.SCI_CONVERTEOLS, eolMode, 0);
        }

        //SCI_SETVIEWEOL(bool visible)
        public void SetViewEOL(bool visible)
        {
            Perform(mDirectPointer, SciConst.SCI_SETVIEWEOL, Convert.ToInt32(visible), 0);
        }

        //SCI_GETVIEWEOL
        public bool GetViewEOL()
        {
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETVIEWEOL, 0, 0));
        }

        //SCI_SETPASTECONVERTENDINGS(bool convert)
        public void SetPasteConvertEndings(bool convert)
        {
            Perform(mDirectPointer, SciConst.SCI_SETPASTECONVERTENDINGS,
                    Convert.ToInt32(convert), 0);
        }

        //SCI_GETPASTECONVERTENDINGS
        public bool GetPasteConvertEndings()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETPASTECONVERTENDINGS, 0, 0));
        }

        //SCI_BRACEMATCH(int pos, int maxReStyle)
        public Int32 BraceMatch(Int32 pos, Int32 maxReStyle)
        {
            return Perform(mDirectPointer, SciConst.SCI_BRACEMATCH, pos, maxReStyle);
        }

        //SCI_BRACEHIGHLIGHT(int pos1, int pos2)
        public void BraceHiglight(Int32 pos1, Int32 pos2)
        {
            Perform(mDirectPointer, SciConst.SCI_BRACEHIGHLIGHT, pos1, pos2);
        }

        //SCI_GETCOLUMN(int pos)
        public Int32 GetColumn(Int32 pos)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETCOLUMN, pos, 0);
        }

        //SCI_SETMODEVENTMASK(int eventMask)
        public void SetModeEventMask(Int32 eventMask)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMODEVENTMASK, eventMask, 0);
        }

        //SCI_SETSCROLLWIDTHTRACKING(bool tracking)
        public void SetScrollWidthTracking(bool tracking)
        {
            Perform(mDirectPointer, SciConst.SCI_SETSCROLLWIDTHTRACKING,
                    Convert.ToInt32(tracking), 0);
        }

        //SCI_GETSCROLLWIDTHTRACKING
        public bool GetScrollWidthTracking()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETSCROLLWIDTHTRACKING, 0, 0));
        }


        //SCI_BEGINUNDOACTION
        public void BeginUndoAction()
        {
            Perform(mDirectPointer, SciConst.SCI_BEGINUNDOACTION, 0, 0);
        }

        //SCI_ENDUNDOACTION
        public void EndUndoAction()
        {
            Perform(mDirectPointer, SciConst.SCI_ENDUNDOACTION, 0, 0);
        }


        //SCI_SETVIRTUALSPACEOPTIONS(int virtualSpaceOptions)
        public void SetVirtualSpaceOptions(Int32 virtualSpaceOptions)
        {
            Perform(mDirectPointer, SciConst.SCI_SETVIRTUALSPACEOPTIONS, virtualSpaceOptions, 0);
        }

        //SCI_GETVIRTUALSPACEOPTIONS
        public Int32 GetVirtualSpaceOptions()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETVIRTUALSPACEOPTIONS, 0, 0);
        }

        //SCI_SETADDITIONALCARETSVISIBLE(bool additionalCaretsVisible)
        public void SetAdditionalCaretsVisible(bool additionalCaretsVisible)
        {
            Perform(mDirectPointer, SciConst.SCI_SETADDITIONALCARETSVISIBLE,
                   Convert.ToInt32(additionalCaretsVisible), 0);
        }

        //SCI_GETADDITIONALCARETSVISIBLE
        public bool GetAdditionalCaretsVisible()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETADDITIONALCARETSVISIBLE, 0, 0));
        }

        //SCI_SETMULTIPLESELECTION(bool multipleSelection)
        public void SetMultipleSelection(bool multipleSelection)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMULTIPLESELECTION,
                    Convert.ToInt32(multipleSelection), 0);
        }

        //SCI_GETMULTIPLESELECTION
        public Int32 GetMultipleSelection()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETMULTIPLESELECTION, 0, 0);
        }


        //SCI_SETMARGINSENSITIVEN(int margin, bool sensitive)
        public void SetMarginSensitiveN(Int32 margin, bool sensitive)
        {
            Perform(mDirectPointer, SciConst.SCI_SETMARGINSENSITIVEN,
                    margin, Convert.ToInt32(sensitive));
        }

        //SCI_GETMARGINSENSITIVEN(int margin)
        public bool GetMarginSensitiveN(Int32 margin)
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETMARGINSENSITIVEN, margin, 0));
        }


        //SCI_SELECTIONISRECTANGLE
        public bool SelectionIsRectangle()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_SELECTIONISRECTANGLE, 0, 0));
        }

        //SCI_CLEARSELECTIONS
        public void ClearSelections()
        {
            Perform(mDirectPointer, SciConst.SCI_CLEARSELECTIONS, 0, 0);
        }

        //GetAnchor(); //SCI_GETANCHOR parameter:0];
        public Int32 GetAnchor()
        {
            return Perform(mDirectPointer, SciConst.SCI_GETANCHOR, 0, 0);
        }

        //SCI_SETUNDOCOLLECTION(bool collectUndo)
        public void SetUndoCollection(bool collectUndo)
        {
            Perform(mDirectPointer, SciConst.SCI_SETUNDOCOLLECTION,
                    Convert.ToInt32(collectUndo), 0);
        }

        //SCI_GETUNDOCOLLECTION
        public bool GetUndoCollection()
        {
            return Convert.ToBoolean(
                Perform(mDirectPointer, SciConst.SCI_GETUNDOCOLLECTION, 0, 0));
        }

        //SCI_ADDUNDOACTION(int token, int flags)
        public void AddUndoAction(Int32 token, Int32 flags)
        {
            Perform(mDirectPointer, SciConst.SCI_ADDUNDOACTION,
                    token, flags);
        }

        //SCI_MOVECARETINSIDEVIEW
        public void MoveCaretInsideView()
        {
            Perform(mDirectPointer, SciConst.SCI_MOVECARETINSIDEVIEW, 0, 0);
        }

        //SCI_SETFIRSTVISIBLELINE
        public void SetFirstVisibleLine(Int32 line)
        {
            Perform(mDirectPointer, SciConst.SCI_SETFIRSTVISIBLELINE, line, 0);
        }


        //////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////// 
        //SCI_SETDOCPOINTER 
        public void SetDocPointer(IntPtr DocPointer)
        {
            WinApi.SendMessage(Handle, SciConst.SCI_SETDOCPOINTER, IntPtr.Zero, DocPointer);
            ////Perform(mDirectPointer, SciConst.SCI_SETDOCPOINTER, IntPtr.Zero, DocPointer);
        }

        //SCI_GETDOCPOINTER 
        public IntPtr GetDocPointer()
        {
            return WinApi.SendMessage(Handle, SciConst.SCI_GETDOCPOINTER, 0, 0);
            ////return (IntPtr)Perform(mDirectPointer, SciConst.SCI_GETDOCPOINTER, IntPtr.Zero, IntPtr.Zero);
        }
        //////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////// 






        //OLD Version !!! Don't use... Possible memory null reference!
        static Int32 VarPtr(object o)
        {
            GCHandle GCH = GCHandle.Alloc(o, GCHandleType.Pinned);
            Int32 ret = GCH.AddrOfPinnedObject().ToInt32();
            GCH.Free();
            return ret;
        }


        //NEW Version !!!
        private class PtrByte : IDisposable
        {
            private GCHandle GCH;
            private byte[] buffer;

            public PtrByte(Int32 length)
            {
                buffer = new byte[length];
                GCH = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            }

            public void Dispose()
            {
                if (GCH.IsAllocated)
                {
                    GCH.Free();
                    System.Diagnostics.Debug.WriteLine("PtrByte: Dispose");

                    if (buffer.Length > 3145728) //6MB:6291456 3MB: 3145728 1MB:1048576
                    {
                        buffer = new byte[0];
                        GC.Collect();
                        System.Diagnostics.Debug.WriteLine("PtrByte: GC.Collect()");
                    }

                }
            }

            public Int32 Handle
            {
                get
                {
                    if (GCH.IsAllocated)
                        return GCH.AddrOfPinnedObject().ToInt32();

                    return 0;
                }
            }

            public override string ToString()
            {
                return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
        }




    }
}





#region " WINAPI "

namespace ScintillaTextEditor
{
    public static class WinApi
    {

        public const int WM_GETMINMAXINFO = 0x24;
        public const int WM_VSCROLL = 0x115;
        public const int WM_COMMAND = 0x111;
        public const int WM_NOTIFY = 0x4e;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_MOVE = 0x3;
        public const int WM_MOUSEHOVER = 0x2a1;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x10;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_CONTEXTMENU = 0x7b;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_MOUSEHWHEEL = 0x20E;


        public const int VK_TAB = 0x9;

        public const uint CW_USEDEFAULT = 0x80000000;

        public const int SW_NORMAL = 1;
        public const int SW_SHOW = 5;



        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_TABSTOP = 0x00010000;

        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;



        public const uint WS_TILED = WS_OVERLAPPED;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_SIZEBOX = WS_THICKFRAME;
        public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;
        public const uint WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);


        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;
        public const int SWP_NOSIZE = 0x1;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_SHOWWINDOW = 0x40;
        public const int SWP_NOREDRAW = 0x8;
        public const int HWND_TOP = 0;

        public const int GW_CHILD = 5;
        public const int GW_HWNDNEXT = 2;

        public const int WM_GETTEXT = 0xd;
        public const int WM_GETTEXTLENGTH = 0xe;

        public const int BM_SETSTATE = 0xf3;
        public const int WM_LBUTTONUP = 0x202;



        public const int WM_DROPFILES = 0x0233;
        public const int WM_PAINT = 0x000F;
        public const int WM_HSCROLL = 0x114;
        public const int WM_DESTROY = 0x02;
        public const int ERROR_MOD_NOT_FOUND = 126;
        public static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);


        //[DllImport("user32.dll")]
        //internal static extern IntPtr CreateWindowEx(
        //    int dwExStyle, //uint
        //    string lpClassName,
        //    string lpWindowName,
        //    int dwStyle, //uint
        //    int x, int y,
        //    int width, int height,
        //    IntPtr hWndParent,
        //    int hMenu,
        //    IntPtr hInstance,
        //    string lpParam);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd,
                                               int X, int Y,
                                               int nWidth, int nHeight,
                                               bool bRepaint);




        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool GetUpdateRect(IntPtr hWnd, Int32 lpRect, bool bErase);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, Int32 lpRect, bool bErase);


        [DllImport("shell32.dll")]
        public static extern int DragQueryFileA(
                IntPtr hDrop,
                uint idx,
                IntPtr buff,
                int sz
        );

        [DllImport("shell32.dll")]
        public static extern int DragQueryFileA(
                IntPtr hDrop,
                uint idx,
                byte[] buff,
                int sz
        );

        [DllImport("shell32.dll")]
        public static extern int DragQueryFileA(
                IntPtr hDrop,
                uint idx,
                StringBuilder buff,
                int sz
        );


        [DllImport("shell32.dll")]
        public static extern int DragFinish(
                IntPtr hDrop
        );

        [DllImport("shell32.dll")]
        public static extern void DragAcceptFiles(
                IntPtr hwnd,
                bool accept
        );

        [DllImport("user32.dll")]
        public static extern Int32 SetFocus(IntPtr hwnd);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                  UInt32 message,
                                                  IntPtr wParam,
                                                  IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                 UInt32 message,
                                                 Int32 wParam,
                                                 Int32 lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessageStr(IntPtr hWnd,
                                                    UInt32 message,
                                                    Int32 wParam,
                                                    System.Text.StringBuilder lParam);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);





        //PROCESS-THREAD LIBRARIES
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle,
                                               uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

    }
}
#endregion



#region " SCINTILLA CONST "

namespace ScintillaTextEditor
{
    public class SciConst
    {
        public const int INVALID_POSITION = -1;
        public const uint SCI_START = 2000;
        public const uint SCI_OPTIONAL_START = 3000;
        public const uint SCI_LEXER_START = 4000;
        public const uint SCI_ADDTEXT = 2001;
        public const uint SCI_ADDSTYLEDTEXT = 2002;
        public const uint SCI_INSERTTEXT = 2003;
        public const uint SCI_CLEARALL = 2004;
        public const uint SCI_CLEARDOCUMENTSTYLE = 2005;
        public const uint SCI_GETLENGTH = 2006;
        public const uint SCI_GETCHARAT = 2007;
        public const uint SCI_GETCURRENTPOS = 2008;
        public const uint SCI_GETANCHOR = 2009;
        public const uint SCI_GETSTYLEAT = 2010;
        public const uint SCI_REDO = 2011;
        public const uint SCI_SETUNDOCOLLECTION = 2012;
        public const uint SCI_SELECTALL = 2013;
        public const uint SCI_SETSAVEPOINT = 2014;
        public const uint SCI_GETSTYLEDTEXT = 2015;
        public const uint SCI_CANREDO = 2016;
        public const uint SCI_MARKERLINEFROMHANDLE = 2017;
        public const uint SCI_MARKERDELETEHANDLE = 2018;
        public const uint SCI_GETUNDOCOLLECTION = 2019;
        public const uint SCWS_INVISIBLE = 0;
        public const uint SCWS_VISIBLEALWAYS = 1;
        public const uint SCWS_VISIBLEAFTERINDENT = 2;
        public const uint SCI_GETVIEWWS = 2020;
        public const uint SCI_SETVIEWWS = 2021;
        public const uint SCI_POSITIONFROMPOINT = 2022;
        public const uint SCI_POSITIONFROMPOINTCLOSE = 2023;
        public const uint SCI_GOTOLINE = 2024;
        public const uint SCI_GOTOPOS = 2025;
        public const uint SCI_SETANCHOR = 2026;
        public const uint SCI_GETCURLINE = 2027;
        public const uint SCI_GETENDSTYLED = 2028;
        public const uint SC_EOL_CRLF = 0;
        public const uint SC_EOL_CR = 1;
        public const uint SC_EOL_LF = 2;
        public const uint SCI_CONVERTEOLS = 2029;
        public const uint SCI_GETEOLMODE = 2030;
        public const uint SCI_SETEOLMODE = 2031;
        public const uint SCI_STARTSTYLING = 2032;
        public const uint SCI_SETSTYLING = 2033;
        public const uint SCI_GETBUFFEREDDRAW = 2034;
        public const uint SCI_SETBUFFEREDDRAW = 2035;
        public const uint SCI_SETTABWIDTH = 2036;
        public const uint SCI_GETTABWIDTH = 2121;
        public const uint SC_CP_UTF8 = 65001;
        public const uint SC_CP_DBCS = 1;
        public const uint SCI_SETCODEPAGE = 2037;
        public const uint SCI_SETUSEPALETTE = 2039;
        public const uint MARKER_MAX = 31;
        public const uint SC_MARK_CIRCLE = 0;
        public const uint SC_MARK_ROUNDRECT = 1;
        public const uint SC_MARK_ARROW = 2;
        public const uint SC_MARK_SMALLRECT = 3;
        public const uint SC_MARK_SHORTARROW = 4;
        public const uint SC_MARK_EMPTY = 5;
        public const uint SC_MARK_ARROWDOWN = 6;
        public const uint SC_MARK_MINUS = 7;
        public const uint SC_MARK_PLUS = 8;
        public const uint SC_MARK_VLINE = 9;
        public const uint SC_MARK_LCORNER = 10;
        public const uint SC_MARK_TCORNER = 11;
        public const uint SC_MARK_BOXPLUS = 12;
        public const uint SC_MARK_BOXPLUSCONNECTED = 13;
        public const uint SC_MARK_BOXMINUS = 14;
        public const uint SC_MARK_BOXMINUSCONNECTED = 15;
        public const uint SC_MARK_LCORNERCURVE = 16;
        public const uint SC_MARK_TCORNERCURVE = 17;
        public const uint SC_MARK_CIRCLEPLUS = 18;
        public const uint SC_MARK_CIRCLEPLUSCONNECTED = 19;
        public const uint SC_MARK_CIRCLEMINUS = 20;
        public const uint SC_MARK_CIRCLEMINUSCONNECTED = 21;
        public const uint SC_MARK_BACKGROUND = 22;
        public const uint SC_MARK_DOTDOTDOT = 23;
        public const uint SC_MARK_ARROWS = 24;
        public const uint SC_MARK_PIXMAP = 25;
        public const uint SC_MARK_FULLRECT = 26;
        public const uint SC_MARK_CHARACTER = 10000;
        public const int SC_MARKNUM_FOLDEREND = 25;
        public const int SC_MARKNUM_FOLDEROPENMID = 26;
        public const int SC_MARKNUM_FOLDERMIDTAIL = 27;
        public const int SC_MARKNUM_FOLDERTAIL = 28;
        public const int SC_MARKNUM_FOLDERSUB = 29;
        public const int SC_MARKNUM_FOLDER = 30;
        public const int SC_MARKNUM_FOLDEROPEN = 31;
        public const int SC_MASK_FOLDERS = -33554432;
        public const uint SCI_MARKERDEFINE = 2040;
        public const uint SCI_MARKERSETFORE = 2041;
        public const uint SCI_MARKERSETBACK = 2042;
        public const uint SCI_MARKERSETALPHA = 2476;
        public const uint SCI_MARKERADD = 2043;
        public const uint SCI_MARKERDELETE = 2044;
        public const uint SCI_MARKERDELETEALL = 2045;
        public const uint SCI_MARKERGET = 2046;
        public const uint SCI_MARKERNEXT = 2047;
        public const uint SCI_MARKERPREVIOUS = 2048;
        public const uint SCI_MARKERDEFINEPIXMAP = 2049;
        public const uint SCI_MARKERADDSET = 2466;
        public const uint SC_MARGIN_SYMBOL = 0;
        public const uint SC_MARGIN_NUMBER = 1;
        public const uint SCI_SETMARGINTYPEN = 2240;
        public const uint SCI_GETMARGINTYPEN = 2241;
        public const uint SCI_SETMARGINWIDTHN = 2242;
        public const uint SCI_GETMARGINWIDTHN = 2243;
        public const uint SCI_SETMARGINMASKN = 2244;
        public const uint SCI_GETMARGINMASKN = 2245;
        public const uint SCI_SETMARGINSENSITIVEN = 2246;
        public const uint SCI_GETMARGINSENSITIVEN = 2247;
        public const int STYLE_DEFAULT = 32;
        public const int STYLE_LINENUMBER = 33;
        public const int STYLE_BRACELIGHT = 34;
        public const int STYLE_BRACEBAD = 35;
        public const int STYLE_CONTROLCHAR = 36;
        public const int STYLE_INDENTGUIDE = 37;
        public const int STYLE_CALLTIP = 38;
        public const int STYLE_LASTPREDEFINED = 39;
        public const int STYLE_MAX = 127;
        public const int SC_CHARSET_ANSI = 0;
        public const int SC_CHARSET_DEFAULT = 1;
        public const int SC_CHARSET_BALTIC = 186;
        public const int SC_CHARSET_CHINESEBIG5 = 136;
        public const int SC_CHARSET_EASTEUROPE = 238;
        public const int SC_CHARSET_GB2312 = 134;
        public const int SC_CHARSET_GREEK = 161;
        public const int SC_CHARSET_HANGUL = 129;
        public const int SC_CHARSET_MAC = 77;
        public const int SC_CHARSET_OEM = 255;
        public const int SC_CHARSET_RUSSIAN = 204;
        public const int SC_CHARSET_CYRILLIC = 1251;
        public const int SC_CHARSET_SHIFTJIS = 128;
        public const int SC_CHARSET_SYMBOL = 2;
        public const int SC_CHARSET_TURKISH = 162;
        public const int SC_CHARSET_JOHAB = 130;
        public const int SC_CHARSET_HEBREW = 177;
        public const int SC_CHARSET_ARABIC = 178;
        public const int SC_CHARSET_VIETNAMESE = 163;
        public const int SC_CHARSET_THAI = 222;
        public const int SC_CHARSET_8859_15 = 1000;
        public const uint SCI_STYLECLEARALL = 2050;
        public const uint SCI_STYLESETFORE = 2051;
        public const uint SCI_STYLESETBACK = 2052;
        public const uint SCI_STYLESETBOLD = 2053;
        public const uint SCI_STYLESETITALIC = 2054;
        public const uint SCI_STYLESETSIZE = 2055;
        public const uint SCI_STYLESETFONT = 2056;
        public const uint SCI_STYLESETEOLFILLED = 2057;
        public const uint SCI_STYLERESETDEFAULT = 2058;
        public const uint SCI_STYLESETUNDERLINE = 2059;
        public const uint SC_CASE_MIXED = 0;
        public const uint SC_CASE_UPPER = 1;
        public const uint SC_CASE_LOWER = 2;
        public const uint SCI_STYLESETCASE = 2060;
        public const uint SCI_STYLESETCHARACTERSET = 2066;
        public const uint SCI_STYLESETHOTSPOT = 2409;
        public const uint SCI_SETSELFORE = 2067;
        public const uint SCI_SETSELBACK = 2068;
        public const uint SCI_SETCARETFORE = 2069;
        public const uint SCI_ASSIGNCMDKEY = 2070;
        public const uint SCI_CLEARCMDKEY = 2071;
        public const uint SCI_CLEARALLCMDKEYS = 2072;
        public const uint SCI_SETSTYLINGEX = 2073;
        public const uint SCI_STYLESETVISIBLE = 2074;
        public const uint SCI_GETCARETPERIOD = 2075;
        public const uint SCI_SETCARETPERIOD = 2076;
        public const uint SCI_SETWORDCHARS = 2077;
        public const uint SCI_BEGINUNDOACTION = 2078;
        public const uint SCI_ENDUNDOACTION = 2079;
        public const uint INDIC_MAX = 31;
        public const uint INDIC_PLAIN = 0;
        public const uint INDIC_SQUIGGLE = 1;
        public const uint INDIC_TT = 2;
        public const uint INDIC_DIAGONAL = 3;
        public const uint INDIC_STRIKE = 4;
        public const uint INDIC_HIDDEN = 5;
        public const uint INDIC_BOX = 6;
        public const uint INDIC_ROUNDBOX = 7;
        public const int INDIC0_MASK = 0x20;
        public const int INDIC1_MASK = 0x40;
        public const int INDIC2_MASK = 0x80;
        public const int INDICS_MASK = 0xE0;
        public const uint SCI_INDICSETSTYLE = 2080;
        public const uint SCI_INDICGETSTYLE = 2081;
        public const uint SCI_INDICSETFORE = 2082;
        public const uint SCI_INDICGETFORE = 2083;
        public const uint SCI_SETWHITESPACEFORE = 2084;
        public const uint SCI_SETWHITESPACEBACK = 2085;
        public const uint SCI_SETSTYLEBITS = 2090;
        public const uint SCI_GETSTYLEBITS = 2091;
        public const uint SCI_SETLINESTATE = 2092;
        public const uint SCI_GETLINESTATE = 2093;
        public const uint SCI_GETMAXLINESTATE = 2094;
        public const uint SCI_GETCARETLINEVISIBLE = 2095;
        public const uint SCI_SETCARETLINEVISIBLE = 2096;
        public const uint SCI_GETCARETLINEBACK = 2097;
        public const uint SCI_SETCARETLINEBACK = 2098;
        public const uint SCI_STYLESETCHANGEABLE = 2099;
        public const uint SCI_AUTOCSHOW = 2100;
        public const uint SCI_AUTOCCANCEL = 2101;
        public const uint SCI_AUTOCACTIVE = 2102;
        public const uint SCI_AUTOCPOSSTART = 2103;
        public const uint SCI_AUTOCCOMPLETE = 2104;
        public const uint SCI_AUTOCSTOPS = 2105;
        public const uint SCI_AUTOCSETSEPARATOR = 2106;
        public const uint SCI_AUTOCGETSEPARATOR = 2107;
        public const uint SCI_AUTOCSELECT = 2108;
        public const uint SCI_AUTOCSETCANCELATSTART = 2110;
        public const uint SCI_AUTOCGETCANCELATSTART = 2111;
        public const uint SCI_AUTOCSETFILLUPS = 2112;
        public const uint SCI_AUTOCSETCHOOSESINGLE = 2113;
        public const uint SCI_AUTOCGETCHOOSESINGLE = 2114;
        public const uint SCI_AUTOCSETIGNORECASE = 2115;
        public const uint SCI_AUTOCGETIGNORECASE = 2116;
        public const uint SCI_USERLISTSHOW = 2117;
        public const uint SCI_AUTOCSETAUTOHIDE = 2118;
        public const uint SCI_AUTOCGETAUTOHIDE = 2119;
        public const uint SCI_AUTOCSETDROPRESTOFWORD = 2270;
        public const uint SCI_AUTOCGETDROPRESTOFWORD = 2271;
        public const uint SCI_REGISTERIMAGE = 2405;
        public const uint SCI_CLEARREGISTEREDIMAGES = 2408;
        public const uint SCI_AUTOCGETTYPESEPARATOR = 2285;
        public const uint SCI_AUTOCSETTYPESEPARATOR = 2286;
        public const uint SCI_AUTOCSETMAXWIDTH = 2208;
        public const uint SCI_AUTOCGETMAXWIDTH = 2209;
        public const uint SCI_AUTOCSETMAXHEIGHT = 2210;
        public const uint SCI_AUTOCGETMAXHEIGHT = 2211;
        public const uint SCI_SETINDENT = 2122;
        public const uint SCI_GETINDENT = 2123;
        public const uint SCI_SETUSETABS = 2124;
        public const uint SCI_GETUSETABS = 2125;
        public const uint SCI_SETLINEINDENTATION = 2126;
        public const uint SCI_GETLINEINDENTATION = 2127;
        public const uint SCI_GETLINEINDENTPOSITION = 2128;
        public const uint SCI_GETCOLUMN = 2129;
        public const uint SCI_SETHSCROLLBAR = 2130;
        public const uint SCI_GETHSCROLLBAR = 2131;
        public const uint SCI_SETINDENTATIONGUIDES = 2132;
        public const uint SCI_GETINDENTATIONGUIDES = 2133;
        public const uint SCI_SETHIGHLIGHTGUIDE = 2134;
        public const uint SCI_GETHIGHLIGHTGUIDE = 2135;
        public const uint SCI_GETLINEENDPOSITION = 2136;
        public const uint SCI_GETCODEPAGE = 2137;
        public const uint SCI_GETCARETFORE = 2138;
        public const uint SCI_GETUSEPALETTE = 2139;
        public const uint SCI_GETREADONLY = 2140;
        public const uint SCI_SETCURRENTPOS = 2141;
        public const uint SCI_SETSELECTIONSTART = 2142;
        public const uint SCI_GETSELECTIONSTART = 2143;
        public const uint SCI_SETSELECTIONEND = 2144;
        public const uint SCI_GETSELECTIONEND = 2145;
        public const uint SCI_SETPRINTMAGNIFICATION = 2146;
        public const uint SCI_GETPRINTMAGNIFICATION = 2147;
        public const uint SC_PRINT_NORMAL = 0;
        public const uint SC_PRINT_INVERTLIGHT = 1;
        public const uint SC_PRINT_BLACKONWHITE = 2;
        public const uint SC_PRINT_COLOURONWHITE = 3;
        public const uint SC_PRINT_COLOURONWHITEDEFAULTBG = 4;
        public const uint SCI_SETPRINTCOLOURMODE = 2148;
        public const uint SCI_GETPRINTCOLOURMODE = 2149;
        public const uint SCFIND_WHOLEWORD = 2;
        public const uint SCFIND_MATCHCASE = 4;
        public const uint SCFIND_WORDSTART = 0x00100000;
        public const uint SCFIND_REGEXP = 0x00200000;
        public const uint SCFIND_POSIX = 0x00400000;
        public const uint SCI_FINDTEXT = 2150;
        public const uint SCI_FORMATRANGE = 2151;
        public const uint SCI_GETFIRSTVISIBLELINE = 2152;
        public const uint SCI_GETLINE = 2153;
        public const uint SCI_GETLINECOUNT = 2154;
        public const uint SCI_SETMARGINLEFT = 2155;
        public const uint SCI_GETMARGINLEFT = 2156;
        public const uint SCI_SETMARGINRIGHT = 2157;
        public const uint SCI_GETMARGINRIGHT = 2158;
        public const uint SCI_GETMODIFY = 2159;
        public const uint SCI_SETSEL = 2160;
        public const uint SCI_GETSELTEXT = 2161;
        public const uint SCI_GETTEXTRANGE = 2162;
        public const uint SCI_HIDESELECTION = 2163;
        public const uint SCI_POINTXFROMPOSITION = 2164;
        public const uint SCI_POINTYFROMPOSITION = 2165;
        public const uint SCI_LINEFROMPOSITION = 2166;
        public const uint SCI_POSITIONFROMLINE = 2167;
        public const uint SCI_LINESCROLL = 2168;
        public const uint SCI_SCROLLCARET = 2169;
        public const uint SCI_REPLACESEL = 2170;
        public const uint SCI_SETREADONLY = 2171;
        public const uint SCI_NULL = 2172;
        public const uint SCI_CANPASTE = 2173;
        public const uint SCI_CANUNDO = 2174;
        public const uint SCI_EMPTYUNDOBUFFER = 2175;
        public const uint SCI_UNDO = 2176;
        public const uint SCI_CUT = 2177;
        public const uint SCI_COPY = 2178;
        public const uint SCI_PASTE = 2179;
        public const uint SCI_CLEAR = 2180;
        public const uint SCI_SETTEXT = 2181;
        public const uint SCI_GETTEXT = 2182;
        public const uint SCI_GETTEXTLENGTH = 2183;
        public const uint SCI_GETDIRECTFUNCTION = 2184;
        public const uint SCI_GETDIRECTPOINTER = 2185;
        public const uint SCI_SETOVERTYPE = 2186;
        public const uint SCI_GETOVERTYPE = 2187;
        public const uint SCI_SETCARETWIDTH = 2188;
        public const uint SCI_GETCARETWIDTH = 2189;
        public const uint SCI_SETTARGETSTART = 2190;
        public const uint SCI_GETTARGETSTART = 2191;
        public const uint SCI_SETTARGETEND = 2192;
        public const uint SCI_GETTARGETEND = 2193;
        public const uint SCI_REPLACETARGET = 2194;
        public const uint SCI_REPLACETARGETRE = 2195;
        public const uint SCI_SEARCHINTARGET = 2197;
        public const uint SCI_SETSEARCHFLAGS = 2198;
        public const uint SCI_GETSEARCHFLAGS = 2199;
        public const uint SCI_CALLTIPSHOW = 2200;
        public const uint SCI_CALLTIPCANCEL = 2201;
        public const uint SCI_CALLTIPACTIVE = 2202;
        public const uint SCI_CALLTIPPOSSTART = 2203;
        public const uint SCI_CALLTIPSETHLT = 2204;
        public const uint SCI_CALLTIPSETBACK = 2205;
        public const uint SCI_CALLTIPSETFORE = 2206;
        public const uint SCI_CALLTIPSETFOREHLT = 2207;
        public const uint SCI_CALLTIPUSESTYLE = 2212;
        public const uint SCI_VISIBLEFROMDOCLINE = 2220;
        public const uint SCI_DOCLINEFROMVISIBLE = 2221;
        public const uint SCI_WRAPCOUNT = 2235;
        public const uint SC_FOLDLEVELBASE = 0x400;
        public const uint SC_FOLDLEVELWHITEFLAG = 0x1000;
        public const uint SC_FOLDLEVELHEADERFLAG = 0x2000;
        public const uint SC_FOLDLEVELBOXHEADERFLAG = 0x4000;
        public const uint SC_FOLDLEVELBOXFOOTERFLAG = 0x8000;
        public const uint SC_FOLDLEVELCONTRACTED = 0x10000;
        public const uint SC_FOLDLEVELUNINDENT = 0x20000;
        public const uint SC_FOLDLEVELNUMBERMASK = 0x0FFF;
        public const uint SCI_SETFOLDLEVEL = 2222;
        public const uint SCI_GETFOLDLEVEL = 2223;
        public const uint SCI_GETLASTCHILD = 2224;
        public const uint SCI_GETFOLDPARENT = 2225;
        public const uint SCI_SHOWLINES = 2226;
        public const uint SCI_HIDELINES = 2227;
        public const uint SCI_GETLINEVISIBLE = 2228;
        public const uint SCI_SETFOLDEXPANDED = 2229;
        public const uint SCI_GETFOLDEXPANDED = 2230;
        public const uint SCI_TOGGLEFOLD = 2231;
        public const uint SCI_ENSUREVISIBLE = 2232;
        public const uint SC_FOLDFLAG_LINEBEFORE_EXPANDED = 0x0002;
        public const uint SC_FOLDFLAG_LINEBEFORE_CONTRACTED = 0x0004;
        public const uint SC_FOLDFLAG_LINEAFTER_EXPANDED = 0x0008;
        public const uint SC_FOLDFLAG_LINEAFTER_CONTRACTED = 0x0010;
        public const uint SC_FOLDFLAG_LEVELNUMBERS = 0x0040;
        public const uint SC_FOLDFLAG_BOX = 0x0001;
        public const uint SCI_SETFOLDFLAGS = 2233;
        public const uint SCI_ENSUREVISIBLEENFORCEPOLICY = 2234;
        public const uint SCI_SETTABINDENTS = 2260;
        public const uint SCI_GETTABINDENTS = 2261;
        public const uint SCI_SETBACKSPACEUNINDENTS = 2262;
        public const uint SCI_GETBACKSPACEUNINDENTS = 2263;
        public const uint SC_TIME_FOREVER = 10000000;
        public const uint SCI_SETMOUSEDWELLTIME = 2264;
        public const uint SCI_GETMOUSEDWELLTIME = 2265;
        public const uint SCI_WORDSTARTPOSITION = 2266;
        public const uint SCI_WORDENDPOSITION = 2267;
        public const uint SC_WRAP_NONE = 0;
        public const uint SC_WRAP_WORD = 1;
        public const uint SC_WRAP_CHAR = 2;
        public const uint SCI_SETWRAPMODE = 2268;
        public const uint SCI_GETWRAPMODE = 2269;
        public const uint SC_WRAPVISUALFLAG_NONE = 0x0000;
        public const uint SC_WRAPVISUALFLAG_END = 0x0001;
        public const uint SC_WRAPVISUALFLAG_START = 0x0002;
        public const uint SCI_SETWRAPVISUALFLAGS = 2460;
        public const uint SCI_GETWRAPVISUALFLAGS = 2461;
        public const uint SC_WRAPVISUALFLAGLOC_DEFAULT = 0x0000;
        public const uint SC_WRAPVISUALFLAGLOC_END_BY_TEXT = 0x0001;
        public const uint SC_WRAPVISUALFLAGLOC_START_BY_TEXT = 0x0002;
        public const uint SCI_SETWRAPVISUALFLAGSLOCATION = 2462;
        public const uint SCI_GETWRAPVISUALFLAGSLOCATION = 2463;
        public const uint SCI_SETWRAPSTARTINDENT = 2464;
        public const uint SCI_GETWRAPSTARTINDENT = 2465;
        public const uint SC_CACHE_NONE = 0;
        public const uint SC_CACHE_CARET = 1;
        public const uint SC_CACHE_PAGE = 2;
        public const uint SC_CACHE_DOCUMENT = 3;
        public const uint SCI_SETLAYOUTCACHE = 2272;
        public const uint SCI_GETLAYOUTCACHE = 2273;
        public const uint SCI_SETSCROLLWIDTH = 2274;
        public const uint SCI_GETSCROLLWIDTH = 2275;
        public const uint SCI_TEXTWIDTH = 2276;
        public const uint SCI_SETENDATLASTLINE = 2277;
        public const uint SCI_GETENDATLASTLINE = 2278;
        public const uint SCI_TEXTHEIGHT = 2279;
        public const uint SCI_SETVSCROLLBAR = 2280;
        public const uint SCI_GETVSCROLLBAR = 2281;
        public const uint SCI_APPENDTEXT = 2282;
        public const uint SCI_GETTWOPHASEDRAW = 2283;
        public const uint SCI_SETTWOPHASEDRAW = 2284;
        public const uint SCI_TARGETFROMSELECTION = 2287;
        public const uint SCI_LINESJOIN = 2288;
        public const uint SCI_LINESSPLIT = 2289;
        public const uint SCI_SETFOLDMARGINCOLOUR = 2290;
        public const uint SCI_SETFOLDMARGINHICOLOUR = 2291;
        public const uint SCI_LINEDOWN = 2300;
        public const uint SCI_LINEDOWNEXTEND = 2301;
        public const uint SCI_LINEUP = 2302;
        public const uint SCI_LINEUPEXTEND = 2303;
        public const uint SCI_CHARLEFT = 2304;
        public const uint SCI_CHARLEFTEXTEND = 2305;
        public const uint SCI_CHARRIGHT = 2306;
        public const uint SCI_CHARRIGHTEXTEND = 2307;
        public const uint SCI_WORDLEFT = 2308;
        public const uint SCI_WORDLEFTEXTEND = 2309;
        public const uint SCI_WORDRIGHT = 2310;
        public const uint SCI_WORDRIGHTEXTEND = 2311;
        public const uint SCI_HOME = 2312;
        public const uint SCI_HOMEEXTEND = 2313;
        public const uint SCI_LINEEND = 2314;
        public const uint SCI_LINEENDEXTEND = 2315;
        public const uint SCI_DOCUMENTSTART = 2316;
        public const uint SCI_DOCUMENTSTARTEXTEND = 2317;
        public const uint SCI_DOCUMENTEND = 2318;
        public const uint SCI_DOCUMENTENDEXTEND = 2319;
        public const uint SCI_PAGEUP = 2320;
        public const uint SCI_PAGEUPEXTEND = 2321;
        public const uint SCI_PAGEDOWN = 2322;
        public const uint SCI_PAGEDOWNEXTEND = 2323;
        public const uint SCI_EDITTOGGLEOVERTYPE = 2324;
        public const uint SCI_CANCEL = 2325;
        public const uint SCI_DELETEBACK = 2326;
        public const uint SCI_TAB = 2327;
        public const uint SCI_BACKTAB = 2328;
        public const uint SCI_NEWLINE = 2329;
        public const uint SCI_FORMFEED = 2330;
        public const uint SCI_VCHOME = 2331;
        public const uint SCI_VCHOMEEXTEND = 2332;
        public const uint SCI_ZOOMIN = 2333;
        public const uint SCI_ZOOMOUT = 2334;
        public const uint SCI_DELWORDLEFT = 2335;
        public const uint SCI_DELWORDRIGHT = 2336;
        public const uint SCI_LINECUT = 2337;
        public const uint SCI_LINEDELETE = 2338;
        public const uint SCI_LINETRANSPOSE = 2339;
        public const uint SCI_LINEDUPLICATE = 2404;
        public const uint SCI_LOWERCASE = 2340;
        public const uint SCI_UPPERCASE = 2341;
        public const uint SCI_LINESCROLLDOWN = 2342;
        public const uint SCI_LINESCROLLUP = 2343;
        public const uint SCI_DELETEBACKNOTLINE = 2344;
        public const uint SCI_HOMEDISPLAY = 2345;
        public const uint SCI_HOMEDISPLAYEXTEND = 2346;
        public const uint SCI_LINEENDDISPLAY = 2347;
        public const uint SCI_LINEENDDISPLAYEXTEND = 2348;
        public const uint SCI_HOMEWRAP = 2349;
        public const uint SCI_HOMEWRAPEXTEND = 2450;
        public const uint SCI_LINEENDWRAP = 2451;
        public const uint SCI_LINEENDWRAPEXTEND = 2452;
        public const uint SCI_VCHOMEWRAP = 2453;
        public const uint SCI_VCHOMEWRAPEXTEND = 2454;
        public const uint SCI_LINECOPY = 2455;
        public const uint SCI_MOVECARETINSIDEVIEW = 2401;
        public const uint SCI_LINELENGTH = 2350;
        public const uint SCI_BRACEHIGHLIGHT = 2351;
        public const uint SCI_BRACEBADLIGHT = 2352;
        public const uint SCI_BRACEMATCH = 2353;
        public const uint SCI_GETVIEWEOL = 2355;
        public const uint SCI_SETVIEWEOL = 2356;
        public const uint SCI_GETDOCPOINTER = 2357;
        public const uint SCI_SETDOCPOINTER = 2358;
        public const uint SCI_SETMODEVENTMASK = 2359;
        public const uint EDGE_NONE = 0;
        public const uint EDGE_LINE = 1;
        public const uint EDGE_BACKGROUND = 2;
        public const uint SCI_GETEDGECOLUMN = 2360;
        public const uint SCI_SETEDGECOLUMN = 2361;
        public const uint SCI_GETEDGEMODE = 2362;
        public const uint SCI_SETEDGEMODE = 2363;
        public const uint SCI_GETEDGECOLOUR = 2364;
        public const uint SCI_SETEDGECOLOUR = 2365;
        public const uint SCI_SEARCHANCHOR = 2366;
        public const uint SCI_SEARCHNEXT = 2367;
        public const uint SCI_SEARCHPREV = 2368;
        public const uint SCI_LINESONSCREEN = 2370;
        public const uint SCI_USEPOPUP = 2371;
        public const uint SCI_SELECTIONISRECTANGLE = 2372;
        public const uint SCI_SETZOOM = 2373;
        public const uint SCI_GETZOOM = 2374;
        public const uint SCI_CREATEDOCUMENT = 2375;
        public const uint SCI_ADDREFDOCUMENT = 2376;
        public const uint SCI_RELEASEDOCUMENT = 2377;
        public const uint SCI_GETMODEVENTMASK = 2378;
        public const uint SCI_SETFOCUS = 2380;
        public const uint SCI_GETFOCUS = 2381;
        public const uint SCI_SETSTATUS = 2382;
        public const uint SCI_GETSTATUS = 2383;
        public const uint SCI_SETMOUSEDOWNCAPTURES = 2384;
        public const uint SCI_GETMOUSEDOWNCAPTURES = 2385;
        public const int SC_CURSORNORMAL = -1;
        public const int SC_CURSORWAIT = 4;
        public const uint SCI_SETCURSOR = 2386;
        public const uint SCI_GETCURSOR = 2387;
        public const uint SCI_SETCONTROLCHARSYMBOL = 2388;
        public const uint SCI_GETCONTROLCHARSYMBOL = 2389;
        public const uint SCI_WORDPARTLEFT = 2390;
        public const uint SCI_WORDPARTLEFTEXTEND = 2391;
        public const uint SCI_WORDPARTRIGHT = 2392;
        public const uint SCI_WORDPARTRIGHTEXTEND = 2393;
        public const uint VISIBLE_SLOP = 0x01;
        public const uint VISIBLE_STRICT = 0x04;
        public const uint SCI_SETVISIBLEPOLICY = 2394;
        public const uint SCI_DELLINELEFT = 2395;
        public const uint SCI_DELLINERIGHT = 2396;
        public const uint SCI_SETXOFFSET = 2397;
        public const uint SCI_GETXOFFSET = 2398;
        public const uint SCI_CHOOSECARETX = 2399;
        public const uint SCI_GRABFOCUS = 2400;
        public const uint CARET_SLOP = 0x01;
        public const uint CARET_STRICT = 0x04;
        public const uint CARET_JUMPS = 0x10;
        public const uint CARET_EVEN = 0x08;
        public const uint SCI_SETXCARETPOLICY = 2402;
        public const uint SCI_SETYCARETPOLICY = 2403;
        public const uint SCI_SETPRINTWRAPMODE = 2406;
        public const uint SCI_GETPRINTWRAPMODE = 2407;
        public const uint SCI_SETHOTSPOTACTIVEFORE = 2410;
        public const uint SCI_SETHOTSPOTACTIVEBACK = 2411;
        public const uint SCI_SETHOTSPOTACTIVEUNDERLINE = 2412;
        public const uint SCI_SETHOTSPOTSINGLELINE = 2421;
        public const uint SCI_PARADOWN = 2413;
        public const uint SCI_PARADOWNEXTEND = 2414;
        public const uint SCI_PARAUP = 2415;
        public const uint SCI_PARAUPEXTEND = 2416;
        public const uint SCI_POSITIONBEFORE = 2417;
        public const uint SCI_POSITIONAFTER = 2418;
        public const uint SCI_COPYRANGE = 2419;
        public const uint SCI_COPYTEXT = 2420;
        public const uint SC_SEL_STREAM = 0;
        public const uint SC_SEL_RECTANGLE = 1;
        public const uint SC_SEL_LINES = 2;
        public const uint SCI_SETSELECTIONMODE = 2422;
        public const uint SCI_GETSELECTIONMODE = 2423;
        public const uint SCI_GETLINESELSTARTPOSITION = 2424;
        public const uint SCI_GETLINESELENDPOSITION = 2425;
        public const uint SCI_LINEDOWNRECTEXTEND = 2426;
        public const uint SCI_LINEUPRECTEXTEND = 2427;
        public const uint SCI_CHARLEFTRECTEXTEND = 2428;
        public const uint SCI_CHARRIGHTRECTEXTEND = 2429;
        public const uint SCI_HOMERECTEXTEND = 2430;
        public const uint SCI_VCHOMERECTEXTEND = 2431;
        public const uint SCI_LINEENDRECTEXTEND = 2432;
        public const uint SCI_PAGEUPRECTEXTEND = 2433;
        public const uint SCI_PAGEDOWNRECTEXTEND = 2434;
        public const uint SCI_STUTTEREDPAGEUP = 2435;
        public const uint SCI_STUTTEREDPAGEUPEXTEND = 2436;
        public const uint SCI_STUTTEREDPAGEDOWN = 2437;
        public const uint SCI_STUTTEREDPAGEDOWNEXTEND = 2438;
        public const uint SCI_WORDLEFTEND = 2439;
        public const uint SCI_WORDLEFTENDEXTEND = 2440;
        public const uint SCI_WORDRIGHTEND = 2441;
        public const uint SCI_WORDRIGHTENDEXTEND = 2442;
        public const uint SCI_SETWHITESPACECHARS = 2443;
        public const uint SCI_SETCHARSDEFAULT = 2444;
        public const uint SCI_AUTOCGETCURRENT = 2445;
        public const uint SCI_ALLOCATE = 2446;
        public const uint SCI_TARGETASUTF8 = 2447;
        public const uint SCI_SETLENGTHFORENCODE = 2448;
        public const uint SCI_ENCODEDFROMUTF8 = 2449;
        public const uint SCI_FINDCOLUMN = 2456;
        public const uint SCI_GETCARETSTICKY = 2457;
        public const uint SCI_SETCARETSTICKY = 2458;
        public const uint SCI_TOGGLECARETSTICKY = 2459;
        public const uint SCI_SETPASTECONVERTENDINGS = 2467;
        public const uint SCI_GETPASTECONVERTENDINGS = 2468;
        public const uint SCI_SELECTIONDUPLICATE = 2469;
        public const uint SC_ALPHA_TRANSPARENT = 0;
        public const uint SC_ALPHA_OPAQUE = 255;
        public const uint SC_ALPHA_NOALPHA = 256;
        public const uint SCI_SETCARETLINEBACKALPHA = 2470;
        public const uint SCI_GETCARETLINEBACKALPHA = 2471;
        public const uint SCI_STARTRECORD = 3001;
        public const uint SCI_STOPRECORD = 3002;
        public const uint SCI_SETLEXER = 4001;
        public const uint SCI_GETLEXER = 4002;
        public const uint SCI_COLOURISE = 4003;
        public const uint SCI_SETPROPERTY = 4004;
        public const uint KEYWORDSET_MAX = 8;
        public const uint SCI_SETKEYWORDS = 4005;
        public const uint SCI_SETLEXERLANGUAGE = 4006;
        public const uint SCI_LOADLEXERLIBRARY = 4007;
        public const uint SCI_GETPROPERTY = 4008;
        public const uint SCI_GETPROPERTYEXPANDED = 4009;
        public const uint SCI_GETPROPERTYINT = 4010;
        public const uint SCI_GETSTYLEBITSNEEDED = 4011;
        public const uint SC_MOD_INSERTTEXT = 0x1;
        public const uint SC_MOD_DELETETEXT = 0x2;
        public const uint SC_MOD_CHANGESTYLE = 0x4;
        public const uint SC_MOD_CHANGEFOLD = 0x8;
        public const uint SC_PERFORMED_USER = 0x10;
        public const uint SC_PERFORMED_UNDO = 0x20;
        public const uint SC_PERFORMED_REDO = 0x40;
        public const uint SC_MULTISTEPUNDOREDO = 0x80;
        public const uint SC_LASTSTEPINUNDOREDO = 0x100;
        public const uint SC_MOD_CHANGEMARKER = 0x200;
        public const uint SC_MOD_BEFOREINSERT = 0x400;
        public const uint SC_MOD_BEFOREDELETE = 0x800;
        public const uint SC_MULTILINEUNDOREDO = 0x1000;
        public const int SC_MODEVENTMASKALL = 0x6FFF;
        public const uint SCEN_CHANGE = 768;
        public const uint SCEN_SETFOCUS = 512;
        public const uint SCEN_KILLFOCUS = 256;
        public const uint SCK_DOWN = 300;
        public const uint SCK_UP = 301;
        public const uint SCK_LEFT = 302;
        public const uint SCK_RIGHT = 303;
        public const uint SCK_HOME = 304;
        public const uint SCK_END = 305;
        public const uint SCK_PRIOR = 306;
        public const uint SCK_NEXT = 307;
        public const uint SCK_DELETE = 308;
        public const uint SCK_INSERT = 309;
        public const uint SCK_ESCAPE = 7;
        public const uint SCK_BACK = 8;
        public const uint SCK_TAB = 9;
        public const uint SCK_RETURN = 13;
        public const uint SCK_ADD = 310;
        public const uint SCK_SUBTRACT = 311;
        public const uint SCK_DIVIDE = 312;
        public const uint SCMOD_NORM = 0;
        public const uint SCMOD_SHIFT = 1;
        public const uint SCMOD_CTRL = 2;
        public const uint SCMOD_ALT = 4;
        public const uint SCN_STYLENEEDED = 2000;
        public const uint SCN_CHARADDED = 2001;
        public const uint SCN_SAVEPOINTREACHED = 2002;
        public const uint SCN_SAVEPOINTLEFT = 2003;
        public const uint SCN_MODIFYATTEMPTRO = 2004;
        public const uint SCN_KEY = 2005;
        public const uint SCN_DOUBLECLICK = 2006;
        public const uint SCN_UPDATEUI = 2007;
        public const uint SCN_MODIFIED = 2008;
        public const uint SCN_MACRORECORD = 2009;
        public const uint SCN_MARGINCLICK = 2010;
        public const uint SCN_NEEDSHOWN = 2011;
        public const uint SCN_PAINTED = 2013;
        public const uint SCN_USERLISTSELECTION = 2014;
        public const uint SCN_URIDROPPED = 2015;
        public const uint SCN_DWELLSTART = 2016;
        public const uint SCN_DWELLEND = 2017;
        public const uint SCN_ZOOM = 2018;
        public const uint SCN_HOTSPOTCLICK = 2019;
        public const uint SCN_HOTSPOTDOUBLECLICK = 2020;
        public const uint SCN_CALLTIPCLICK = 2021;
        public const uint SCN_AUTOCSELECTION = 2022;
        public const uint SCI_STYLEGETFORE = 2481;
        public const uint SCI_STYLEGETBACK = 2482;
        public const uint SCI_STYLEGETBOLD = 2483;
        public const uint SCI_STYLEGETITALIC = 2484;
        public const uint SCI_STYLEGETSIZE = 2485;
        public const uint SCI_STYLEGETFONT = 2486;
        public const uint SCI_STYLEGETEOLFILLED = 2487;
        public const uint SCI_STYLEGETUNDERLINE = 2488;
        public const uint SCI_STYLEGETCASE = 2489;
        public const uint SCI_STYLEGETCHARACTERSET = 2490;
        public const uint SCI_STYLEGETVISIBLE = 2491;
        public const uint SCI_STYLEGETCHANGEABLE = 2492;
        public const uint SCI_STYLEGETHOTSPOT = 2493;
        public const uint INDIC_CONTAINER = 8;
        public const uint SCI_INDICSETUNDER = 2510;
        public const uint SCI_INDICGETUNDER = 2511;
        public const uint SCI_GETHOTSPOTACTIVEFORE = 2494;
        public const uint SCI_GETHOTSPOTACTIVEBACK = 2495;
        public const uint SCI_GETHOTSPOTACTIVEUNDERLINE = 2496;
        public const uint SCI_GETHOTSPOTSINGLELINE = 2497;
        public const uint CARETSTYLE_INVISIBLE = 0;
        public const uint CARETSTYLE_LINE = 1;
        public const uint CARETSTYLE_BLOCK = 2;
        public const uint SCI_SETCARETSTYLE = 2512;
        public const uint SCI_GETCARETSTYLE = 2513;
        public const uint SCI_SETINDICATORCURRENT = 2500;
        public const uint SCI_GETINDICATORCURRENT = 2501;
        public const uint SCI_SETINDICATORVALUE = 2502;
        public const uint SCI_GETINDICATORVALUE = 2503;
        public const uint SCI_INDICATORFILLRANGE = 2504;
        public const uint SCI_INDICATORCLEARRANGE = 2505;
        public const uint SCI_INDICATORALLONFOR = 2506;
        public const uint SCI_INDICATORVALUEAT = 2507;
        public const uint SCI_INDICATORSTART = 2508;
        public const uint SCI_INDICATOREND = 2509;
        public const uint SCI_SETPOSITIONCACHE = 2514;
        public const uint SCI_GETPOSITIONCACHE = 2515;
        public const uint SC_MOD_CHANGEINDICATOR = 0x4000;
        public const uint SCN_INDICATORCLICK = 2023;
        public const uint SCN_INDICATORRELEASE = 2024;
        public const uint SCI_GETSELALPHA = 2477;
        public const uint SCI_SETSELALPHA = 2478;
        public const uint SCI_SETSCROLLWIDTHTRACKING = 2516;
        public const uint SCI_GETSCROLLWIDTHTRACKING = 2517;
        public const uint SCI_SETVIRTUALSPACEOPTIONS = 2596;
        public const uint SCI_GETVIRTUALSPACEOPTIONS = 2597;
        public const uint SCI_SETADDITIONALCARETSVISIBLE = 2608;
        public const uint SCI_GETADDITIONALCARETSVISIBLE = 2609;
        public const uint SCI_SETMULTIPLESELECTION = 2563;
        public const uint SCI_GETMULTIPLESELECTION = 2564;
        public const uint SCI_CLEARSELECTIONS = 2571;
        public const uint SCI_ADDUNDOACTION = 2560;
        public const uint SC_MOD_CONTAINER = 0x40000;
        public const uint SCI_SETFIRSTVISIBLELINE = 2613;


    }
}


#endregion



