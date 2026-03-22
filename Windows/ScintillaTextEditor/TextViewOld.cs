using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.IO;

//using System.Security.Permissions;

namespace ScintillaTextEditor
{
    class TextViewOld : System.Windows.Forms.Control
    {

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


        private IntPtr mLib;
        private IntPtr mWnd;
        private IntPtr mDirectPointer;

        private string mFontName;
        private float mFontSize;
        private bool mLineNumbers = true;
        private bool mFocus = false;


        private byte mContextOffset = 2;
        private Int32 mKeyPress;

        private Point mMousePosition;


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
        }



        //STRUCTURES for SCIEDIT 
        public struct CharacterRange
        {
            public long cpMin;
            public long cpMax;
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




        public class NativeScintillaEventArgs : EventArgs
        {
            private Message _msg;
            private SCNotification _notification;

            /// <summary>
            /// Notification Message sent from the native Scintilla
            /// </summary>
            public Message Msg
            {
                get
                {
                    return _msg;
                }
            }

            /// <summary>
            /// SCNotification structure sent from Scintilla that contains the event data
            /// </summary>
            public SCNotification SCNotification
            {
                get
                {
                    return _notification;
                }
            }

            /// <summary>
            /// Initializes a new instance of the NativeScintillaEventArgs class.
            /// </summary>
            /// <param name="msg">Notification Message sent from the native Scintilla</param>
            /// <param name="notification">SCNotification structure sent from Scintilla that contains the event data</param>
            public NativeScintillaEventArgs(Message msg, SCNotification notification)
            {
                _msg = msg;
                _notification = notification;
            }
        }






        public TextViewOld()
        {
            ////System.Media.SystemSounds.Beep.Play();
            //mLib = WinApi.LoadLibrary("SciLexer.dll");
            //if (mLib == null) return;

            //mWnd = WinApi.CreateWindowEx(0,
            //                             "Scintilla",
            //                             "",
            //                             WinApi.WS_CHILD |
            //                             WinApi.WS_VISIBLE |
            //                             WinApi.WS_TABSTOP,
            //                             0, 0, this.Width, this.Height,
            //                             this.Handle, IntPtr.Zero,
            //                             IntPtr.Zero, IntPtr.Zero);

            //mDirectPointer = WinApi.SendMessage(mWnd,
            //                                    SciConst.SCI_GETDIRECTPOINTER,
            //                                    0, 0);

            /////'Font Settings 
            //byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(this.Font.Name);


            //Perform(mDirectPointer, SciConst.SCI_STYLESETFONT, 32, (uint)VarPtr(b));
            ////Perform(mDirectPointer, SciConst.SCI_STYLESETFONT,
            ////        32, Convert.ToUInt32(this.Font.Name));
            //Perform(mDirectPointer, SciConst.SCI_STYLESETSIZE,
            //        32, Convert.ToUInt32(this.Font.Size));

            //mFontName = this.Font.Name;
            //mFontSize = this.Font.Size;

            ////Margins Settings 
            //Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 0, 1);
            ////NUMBERS 
            //Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 1, 0);
            ////NON FOLDER SYMBOLS 
            //Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 2, 0);
            ////FOLDER SYMBOLS 

            //Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 0, 50);
            //Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 1, 0);
            //Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 2, 0);

            ////Set CodePage 
            //SetCodePage(SciConst.SC_CP_UTF8);
            //SetCodePage(SciConst.SC_CP_UTF8);

            ////Accept DragDrop Files 
            //WinApi.DragAcceptFiles(mWnd, true);
        }


        /// <summary>
        /// Overriden. See <see cref="Control.Dispose"/>.
        /// </summary>
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
            }

            base.Dispose(disposing);
        }


        /// <summary>
        /// Overriden. See <see cref="Control.WndProc"/>.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
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
            if (m.Msg == WinApi.WM_DESTROY)
            {
                if (this.IsHandleCreated)
                {
                    WinApi.SetParent(this.Handle, WinApi.HWND_MESSAGE);
                    return;
                }
            }
            else if ((int)m.Msg == WinApi.WM_PAINT)
            {
                //	I tried toggling the ControlStyles.UserPaint flag and sending the message
                //	to both base.WndProc and DefWndProc in order to get the best of both worlds,
                //	Scintilla Paints as normal and .NET fires the Paint Event with the proper
                //	clipping regions and such. This didn't work too well, I kept getting weird
                //	phantom paints, or sometimes the .NET paint events would seem to get painted
                //	over by Scintilla. This technique I use below seems to work perfectly.

                base.WndProc(ref m);

                //if(_isCustomPaintingEnabled)
                //{
                //    RECT r;
                //    if (!WinApi.GetUpdateRect(Handle, out r, false))
                //        r = ClientRectangle;

                //    Graphics g = CreateGraphics();
                //    g.SetClip(r);

                //    OnPaint(new PaintEventArgs(CreateGraphics(), r));
                //}
                return;
            }
            //else if ((m.Msg) == WinApi.WM_DROPFILES)
            //{
            //    handleFileDrop(m.WParam);
            //    return;
            //}

            //	Uh-oh. Code based on undocumented unsupported .NET behavior coming up!
            //	Windows Forms Sends Notify messages back to the originating
            //	control ORed with 0x2000. This is way cool becuase we can listen for
            //	WM_NOTIFY messages originating form our own hWnd (from Scintilla)
            if ((m.Msg ^ 0x2000) != WinApi.WM_NOTIFY)
            {
                switch (m.Msg)
                {
                    case WinApi.WM_HSCROLL:
                    case WinApi.WM_VSCROLL:
                        //FireScroll(ref m);

                        //	FireOnScroll calls WndProc so no need to call it again
                        return;
                }
                base.WndProc(ref m);
                return;
            }
            else if ((int)m.Msg >= 10000)
            {
                //_commands.Execute((BindableCommand)m.Msg);
                return;
            }


            SCNotification scn = (SCNotification)Marshal.PtrToStructure(m.LParam, typeof(SCNotification));
            NativeScintillaEventArgs nsea = new NativeScintillaEventArgs(m, scn);

            switch (scn.nmhdr.code)
            {

                case SciConst.SCN_AUTOCSELECTION:
                    //FireAutoCSelection(nsea);
                    break;

                case SciConst.SCN_CALLTIPCLICK:
                    //FireCallTipClick(nsea);
                    break;

                case SciConst.SCN_CHARADDED:
                    //FireCharAdded(nsea);
                    break;

                case SciConst.SCEN_CHANGE:
                    //FireChange(nsea);
                    break;

                case SciConst.SCN_DOUBLECLICK:
                    //FireDoubleClick(nsea);
                    break;

                case SciConst.SCN_DWELLEND:
                    //FireDwellEnd(nsea);
                    break;

                case SciConst.SCN_DWELLSTART:
                    //FireDwellStart(nsea);
                    break;

                case SciConst.SCN_HOTSPOTCLICK:
                    //FireHotSpotClick(nsea);
                    break;

                case SciConst.SCN_HOTSPOTDOUBLECLICK:
                    //FireHotSpotDoubleclick(nsea);
                    break;

                case SciConst.SCN_INDICATORCLICK:
                    //FireIndicatorClick(nsea);
                    break;

                case SciConst.SCN_INDICATORRELEASE:
                    //FireIndicatorRelease(nsea);
                    break;

                case SciConst.SCN_KEY:
                    //FireKey(nsea);
                    break;

                case SciConst.SCN_MACRORECORD:
                    //FireMacroRecord(nsea);
                    break;

                case SciConst.SCN_MARGINCLICK:
                    //FireMarginClick(nsea);
                    break;

                case SciConst.SCN_MODIFIED:
                    //FireModified(nsea);
                    break;

                case SciConst.SCN_MODIFYATTEMPTRO:
                    //FireModifyAttemptRO(nsea);
                    break;

                case SciConst.SCN_NEEDSHOWN:
                    //FireNeedShown(nsea);
                    break;

                case SciConst.SCN_PAINTED:
                    //FirePainted(nsea);
                    break;

                case SciConst.SCN_SAVEPOINTLEFT:
                    //FireSavePointLeft(nsea);
                    break;

                case SciConst.SCN_SAVEPOINTREACHED:
                    //FireSavePointReached(nsea);
                    break;

                case SciConst.SCN_STYLENEEDED:
                    //FireStyleNeeded(nsea);
                    break;

                case SciConst.SCN_UPDATEUI:
                    //FireUpdateUI(nsea);
                    break;

                case SciConst.SCN_URIDROPPED:
                    //FireUriDropped(nsea);
                    break;

                case SciConst.SCN_USERLISTSELECTION:
                    //FireUserListSelection(nsea);
                    break;

                case SciConst.SCN_ZOOM:
                    //FireZoom(nsea);
                    break;

            }

            base.WndProc(ref m);
        }


        private static bool _sciLexerLoaded = false;
        private const string DefaultDllName = "SciLexer.dll";

        protected override CreateParams CreateParams
        {
            //[SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                //	Otherwise Scintilla won't paint. When UserPaint is set to
                //	true the base Class (Control) eats the WM_PAINT message.
                //	Of course when this set to false we can't use the Paint
                //	events. This is why I'm relying on the Paint notification
                //	sent from scintilla to paint the Marker Arrows.
                //SetStyle(ControlStyles.UserPaint, false);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                //	Windows Forms shouldn't be holding onto the text property
                //	for a number of reasons. First off Scintilla may hold on
                //	to a huge set of text and it can cause OutOfMemoryExceptions.
                //	Also it isn't reflective of what is actually in Scintilla
                //	as the text constantly changes and will get out of sync with
                //	what gets set in the Text property setter.
                SetStyle(ControlStyles.CacheText, true);

                //	Registers the Scintilla Window Class
                //	I'm relying on the fact that a version specific renamed
                //	SciLexer exists either in the Current Dir or a global path
                //	(See LoadLibrary Windows API Search Rules)

                //	{wi15726} 2008-07-28 Chris Rickard 
                //	As milang pointed out there were some improvements to be made
                //	to this section of code. Now LoadLibrary is only called once
                //	per process (well, appdomain) and a better exception is thrown
                //	if it can't be loaded.
                //	Lastly I took out the whole concept of using an alternate name
                //	for SciLexer.dll. This is a breaking change but I don't think
                //	ANYONE has ever used this feature. If people complain I'll put
                //	it back but it completely avoids the weird behavoir described 
                //	in {wi15726}.
                //	Exception handling by jacobslusser
                if (!_sciLexerLoaded)
                {
                    if (WinApi.LoadLibrary(DefaultDllName) == IntPtr.Zero)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == WinApi.ERROR_MOD_NOT_FOUND)
                        {
                            // //Couldn't find the SciLexer library. Provider a friendlier error message.
                            //string message = String.Format(

                            //    @"The Scintilla library could not be found. Please place the library " +
                            //    @"in a searchable path such as the application or '{0}' directory.",
                            //    Environment.SystemDirectory);

                            //throw new FileNotFoundException(message, new Win32Exception(errorCode));
                        }

                        throw new Win32Exception(errorCode);
                    }
                    else
                    {
                        _sciLexerLoaded = true;
                    }

                }


                //	Tell Windows Forms to create a Scintilla
                //	derived Window Class for this control
                CreateParams cp = base.CreateParams;
                cp.ClassName = "Scintilla";

                return cp;
            }
        }

        private BitVector32 _state;
        private static readonly int _modifiedState = BitVector32.CreateMask();
        private static readonly int _acceptsReturnState = BitVector32.CreateMask(_modifiedState);
        private static readonly int _acceptsTabState = BitVector32.CreateMask(_acceptsReturnState);
        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Shift) != Keys.None)
                keyData ^= Keys.Shift;

            switch (keyData)
            {
                case Keys.Tab:
                    return _state[_acceptsTabState];
                case Keys.Enter:
                    return _state[_acceptsReturnState];
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.F:

                    return true;
            }

            return base.IsInputKey(keyData);
        }


        protected override bool ProcessKeyMessage(ref Message m)
        {
            //	For some reason IsInputKey isn't working for
            //	Key.Enter. This seems to make it work as expected
            if ((int)m.WParam == (int)Keys.Enter && !_state[_acceptsReturnState])
            {
                return true;
            }
            else
            {
                return base.ProcessKeyMessage(ref m);
            }
        }

        /// <summary>
        /// Overriden. See <see cref="Control.OnLostFocus"/>.
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            //if(Selection.HideSelection)
            //	_ns.HideSelection(true);

            //_ns.SetSelBack(true, Utilities.ColorToRgb(Selection.BackColorUnfocused));
            //_ns.SetSelFore(true, Utilities.ColorToRgb(Selection.ForeColorUnfocused));

            base.OnLostFocus(e);
        }

        /// <summary>
        /// Overriden. See <see cref="Control.OnGotFocus"/>.
        /// </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            //if (!Selection.Hidden)
            //	_ns.HideSelection(false);

            //_ns.SetSelBack(true, Utilities.ColorToRgb(Selection.BackColor));
            //_ns.SetSelFore(true, Utilities.ColorToRgb(Selection.ForeColor));

            base.OnGotFocus(e);
        }















        public IntPtr SCIHandle()
        {
            return mWnd;
        }

        public new void Move(Int32 x, Int32 y, Int32 Width, Int32 Height)
        {
            this.Location = new Point(x, y);
            this.Size = new Size(Width, Height);
        }

        public void SetFocus()
        {
            WinApi.SetFocus(mWnd);
        }

        public void Open(string mFileName)
        {
            if (File.Exists(mFileName))
            {
                StreamReader reader = new StreamReader(mFileName, Encoding.Default);
                this.SetText(reader.ReadToEnd());
                reader.Close();
            }
            else
            {
                MessageBox.Show(null, "Unable to open file", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save(string mFileName)
        {
            StreamWriter writer = new StreamWriter(mFileName, false, Encoding.Default);
            writer.Write(this.GetText());
            writer.Close();
        }

        public string GetFontName()
        {
            return mFontName;
        }

        public Int32 GetFontSize()
        {
            return (Int32)mFontSize;
        }

        public bool LineNumbers
        {
            get { return mLineNumbers; }
            set
            {
                if (value == true)
                {
                    //'Show Numbers 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 0, 1);
                    //NUMBERS 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 1, 0);
                    //NON FOLDER SYMBOLS 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 2, 0);
                    //FOLDER SYMBOLS 

                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 0, 50);
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 1, 0);
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 2, 0);
                    mLineNumbers = true;
                }
                else
                {
                    //'Hide Numbers 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 0, 1);
                    //NUMBERS 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 1, 0);
                    //NON FOLDER SYMBOLS 
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINTYPEN, 2, 0);
                    //FOLDER SYMBOLS 

                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 0, 0);
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 1, 0);
                    Perform(mDirectPointer, SciConst.SCI_SETMARGINWIDTHN, 2, 0);
                    mLineNumbers = false;

                }
            }
        }

        public new Point MousePosition
        {
            get { return mMousePosition; }
        }

        public new bool Focused
        {
            get { return mFocus; }
        }

        public new void Focus()
        {
            this.SetFocus();
        }















        ///'SCI FUNCTIONS 

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
            Int32 TextLength = this.GetTextLength();
            byte[] buffer = new byte[TextLength + 1];

            Perform(mDirectPointer, SciConst.SCI_GETTEXT, TextLength + 1, VarPtr(buffer));
            //'Return System.Text.Encoding.Default.GetString(buffer, 0, TextLength) 

            return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
        }

        //SCI_GETLINE(int line, char *text) 
        public string GetLine(Int32 Line)
        {
            Int32 TextLength = this.LineLength(Line);
            byte[] buffer = new byte[TextLength + 1];

            Perform(mDirectPointer, SciConst.SCI_GETLINE, Line, VarPtr(buffer));
            //'Return System.Text.Encoding.Default.GetString(buffer, 0, TextLength) 

            return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
        }

        //SCI_GETCURLINE(int textLen, char *text) 
        public CurrentLine GetCurLine()
        {
            Int32 TextLength = Perform(mDirectPointer, SciConst.SCI_GETCURLINE, 0, 0);
            byte[] buffer = new byte[TextLength + 1];
            CurrentLine mData = default(CurrentLine);

            mData.CurLinePos = Perform(mDirectPointer, SciConst.SCI_GETCURLINE, TextLength, VarPtr(buffer));
            mData.Text = System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
            //'Return System.Text.Encoding.Default.GetString(buffer, 0, TextLength) 

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
        public void SetText(string Text)
        {
            //Try 
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            //Catch 
            //End Try 
            Perform(mDirectPointer, SciConst.SCI_SETTEXT, 0, VarPtr(b));
        }

        //SCI_INSERTTEXT(int pos, const char *text) 
        public void InsertText(Int32 Pos, string Text)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            Perform(mDirectPointer, SciConst.SCI_INSERTTEXT, Pos, VarPtr(b));
        }

        //SCI_ADDTEXT(int length, const char *s) 
        public void AddText(string Text)
        {
            Int32 Length = Text.Length;
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            Perform(mDirectPointer, SciConst.SCI_ADDTEXT, Length, VarPtr(b));
        }

        //SCI_APPENDTEXT(int length, const char *s) 
        public void AppendText(string Text)
        {
            //Try 
            Int32 Length = Text.Length;
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            //Catch 
            //End Try 
            Perform(mDirectPointer, SciConst.SCI_APPENDTEXT, Length, VarPtr(b));
        }

        //SCI_GETCHARAT(int pos) 
        public string GetCharAt(Int32 Pos)
        {
            Int32 KeyDec = Perform(mDirectPointer, SciConst.SCI_GETCHARAT, Pos, 0);
            if (KeyDec > 32 & KeyDec < 128)
            {
                return Convert.ToString((char)KeyDec);
            }
            else
            {
                //KeyDec 
                return "";
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
        public void StyleSetFont(Int32 styleNumber, string FontName)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(FontName);
            Perform(mDirectPointer, SciConst.SCI_STYLESETFONT, styleNumber, VarPtr(b));
            mFontName = FontName;
        }

        //SCI_STYLESETSIZE(int styleNumber, int sizeInPoints) 
        public void StyleSetSize(Int32 styleNumber, Int32 sizeInPoints)
        {
            Perform(mDirectPointer, SciConst.SCI_STYLESETSIZE, styleNumber, sizeInPoints);
            mFontSize = sizeInPoints;
        }

        //SCI_SETZOOM(int zoomInPoints) 
        public void SetZoom(Int32 zoomInPoints)
        {
            Perform(mDirectPointer, SciConst.SCI_SETZOOM, zoomInPoints, 0);
        }


        //SCI_FINDTEXT(int searchFlags, TextToFind *ttf) 
        public Int32 FindText(Int32 searchFlags, TextToFind mTextToFind)
        {
            return Perform(mDirectPointer, SciConst.SCI_FINDTEXT, searchFlags, VarPtr(mTextToFind));
        }

        //SCI_SEARCHINTARGET(int length, const char *text) 
        public Int32 SearchInTarget(string Text)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            return Perform(mDirectPointer, SciConst.SCI_SEARCHINTARGET, Text.Length, VarPtr(b));
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
        public Int32 SearchNext(Int32 searchFlags, string Text)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            return Perform(mDirectPointer, SciConst.SCI_SEARCHNEXT, searchFlags, VarPtr(b));
        }

        //SCI_SEARCHPREV(int searchFlags, const char *text) 
        public Int32 SearchPrev(Int32 searchFlags, string Text)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            return Perform(mDirectPointer, SciConst.SCI_SEARCHPREV, searchFlags, VarPtr(b));
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
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Definition);
            Perform(mDirectPointer, SciConst.SCI_CALLTIPSHOW, posStart, VarPtr(b));
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
            return Perform(mDirectPointer, SciConst.SCI_WORDSTARTPOSITION, Position, Convert.ToInt32(onlyWordCharacters));
        }

        //SCI_WORDENDPOSITION(int position, bool onlyWordCharacters) 
        public Int32 WordEndPosition(Int32 Position, bool onlyWordCharacters)
        {
            return Perform(mDirectPointer, SciConst.SCI_WORDENDPOSITION, Position, Convert.ToInt32(onlyWordCharacters));
        }

        //SCI_GETTEXTRANGE(<unused>, TextRange *tr) 
        public Int32 GetTextRange(TextRange mTextRange)
        {
            return Perform(mDirectPointer, SciConst.SCI_GETTEXTRANGE, 0, VarPtr(mTextRange));
        }


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
            byte[] MyLang = System.Text.UTF8Encoding.UTF8.GetBytes(Language);
            Perform(mDirectPointer, SciConst.SCI_SETLEXERLANGUAGE, 0, VarPtr(MyLang));
        }

        //SCI_SETKEYWORDS(int keyWordSet, const char *keyWordList) 
        public void SetKeyWords(Int32 keyWordSet, string KeyWordsList)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(KeyWordsList);
            Perform(mDirectPointer, SciConst.SCI_SETKEYWORDS, keyWordSet, VarPtr(b));
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
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Chars);
            Perform(mDirectPointer, SciConst.SCI_SETWORDCHARS, 0, VarPtr(b));
        }

        //SCI_SETWHITESPACECHARS(<unused>, const char *chars) 
        public void SetWhiteSpaceChars(string SpaceChars)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(SpaceChars);
            Perform(mDirectPointer, SciConst.SCI_SETWHITESPACECHARS, 0, VarPtr(b));
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
        public string GetSelText()
        {
            //If Me.GetSelectionStart() = Me.GetSelectionEnd() Then Return vbNullString 

            Int32 TextLength = Perform(mDirectPointer, SciConst.SCI_GETSELTEXT, 0, 0) - 1;

            if (TextLength > 0)
            {

                byte[] buffer = new byte[TextLength + 1];

                Perform(mDirectPointer, SciConst.SCI_GETSELTEXT, 0, VarPtr(buffer));

                return System.Text.Encoding.UTF8.GetString(buffer, 0, TextLength);
            }
            else
            {

                //'vbnullstring 
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
        public void ReplaceSel(string Text)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Text);
            Perform(mDirectPointer, SciConst.SCI_REPLACESEL, 0, VarPtr(b));
        }


        //SCI_MARKERDEFINE(int markerNumber, int markerSymbols) 
        public void MarkerDefine(Int32 markerNumber, Int32 markerSymbols)
        {
            Perform(mDirectPointer, SciConst.SCI_MARKERDEFINE, markerNumber, markerSymbols);
        }

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
            //Try 
            //Catch 
            //End Try 
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
            //Try 
            //Catch 
            //End Try 
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
        public string StyleGetFont(Int32 styleNumber)
        {
            int length = Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, 0, 0);
            byte[] buffer = new byte[length];//new byte[33];
            Perform(mDirectPointer, SciConst.SCI_STYLEGETFONT, styleNumber, VarPtr(buffer));
            return Marshal.PtrToStringAnsi((IntPtr)VarPtr(buffer));
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
        public Int32 TextWidth(Int32 styleNumber, string mText)
        {
            byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(mText);
            return Perform(mDirectPointer, SciConst.SCI_TEXTWIDTH, styleNumber, VarPtr(b));
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
            return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETCARETLINEVISIBLE, 0, 0));
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





        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        //SCI_SETDOCPOINTER 
        public void SetDocPointer(Int32 DocPointer)
        {
            WinApi.SendMessage(mWnd, SciConst.SCI_SETDOCPOINTER, 0, DocPointer);
        }

        //SCI_GETDOCPOINTER 
        public IntPtr GetDocPointer()
        {
            return WinApi.SendMessage(mWnd, SciConst.SCI_GETDOCPOINTER, 0, 0);
        }








        public Int32 VarPtr(object o)
        {

            System.Runtime.InteropServices.GCHandle GC = System.Runtime.InteropServices.GCHandle.Alloc(o, System.Runtime.InteropServices.GCHandleType.Pinned);

            int ret = GC.AddrOfPinnedObject().ToInt32();

            GC.Free();


            return ret;
        }



    }
}




#region " WINAPI "

namespace ScintillaTextEditor
{
    internal static class WinApi
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
        internal static extern IntPtr CreateWindowEx(
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
        internal static extern bool MoveWindow(IntPtr hWnd,
                                               int X, int Y,
                                               int nWidth, int nHeight,
                                               bool bRepaint);




        [DllImport("user32.dll")]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        internal static extern bool GetUpdateRect(IntPtr hWnd, out Rectangle lpRect, bool bErase);

        [DllImport("shell32.dll")]
        internal static extern int DragQueryFileA(
                IntPtr hDrop,
                uint idx,
                IntPtr buff,
                int sz
        );

        [DllImport("shell32.dll")]
        internal static extern int DragFinish(
                IntPtr hDrop
        );

        [DllImport("shell32.dll")]
        internal static extern void DragAcceptFiles(
                IntPtr hwnd,
                bool accept
        );

        [DllImport("user32.dll")]
        internal static extern Int32 SetFocus(IntPtr hwnd);


        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpLibFileName);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd,
                                                  UInt32 message,
                                                  IntPtr wParam,
                                                  IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd,
                                                 UInt32 message,
                                                 Int32 wParam,
                                                 Int32 lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessageStr(IntPtr hWnd,
                                                    UInt32 message,
                                                    Int32 wParam,
                                                    System.Text.StringBuilder lParam);

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
    }
}


#endregion

