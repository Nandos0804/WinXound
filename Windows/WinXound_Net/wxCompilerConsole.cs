using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Globalization;
using System.Diagnostics;



namespace WinXound_Net
{
    public partial class wxCompilerConsole : UserControl
    {
        public wxCompilerConsole()
        {
            InitializeComponent();
        }

        public delegate void CompilerCompleted(string mErrorLine, string mWaveFile);
        public event CompilerCompleted OnCompilerCompleted;

        public event EventHandler OnButtonStopBatch;

        private byte[] wxOutputBuffer = new byte[1024];
        private byte[] wxErrorBuffer = new byte[1024];

        //Delegate for SCI_EDIT AppendText
        private delegate void AppendTextDelegate(ScintillaTextEditor.TextEditor mOutputWin,
                                                 string mString);
        AppendTextDelegate d = new AppendTextDelegate(AppendText);

        private ScintillaTextEditor.TextEditor RTB_Error = new ScintillaTextEditor.TextEditor();
        private string newline = System.Environment.NewLine;
        private string mTitle = "";

        private bool mCompiledWithRT = false;
        bool mIsPaused = false;

        private Font mFont = new Font("Courier New", 10);



        private void wxCompilerConsole_Load(object sender, EventArgs e)
        {
            //RTB_Output.FontChanged += new EventHandler(RTB_Output_FontChanged);
            RTB_Output.ShowLineNumbers = false;

            //Disable Undo
            RTB_Output.PrimaryView.SetUndoCollection(false);
            RTB_Output.SecondaryView.SetUndoCollection(false);

            ConfigureOutputWindow(RTB_Output.PrimaryView);
            RTB_Output.AllowCaretBeyondEOL = false;
        }


        private void ConfigureOutputWindow(ScintillaTextEditor.TextView mSciEdit)
        {
            mSciEdit.SetZoom(0);
            mSciEdit.SetTabWidth(8);

            Int32 blue = 230;
            Int32 red = 255;


            //cpp styles
            //Set Syntax foreground colors
            mSciEdit.StyleSetFore(0, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(1, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(2, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(4, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));//Numbers
            mSciEdit.StyleSetFore(5, ColorTranslator.ToWin32(Color.FromArgb(red, 0, 0)));//KeyWord0
            mSciEdit.StyleSetFore(6, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));//Double Quoted strings
            mSciEdit.StyleSetFore(9, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(10, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(11, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(16, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));//KeyWord1
            //mSciEdit.StyleSetFore(20, ColorTranslator.ToWin32(Color.FromArgb(0, 150, 150)));//KeyWord2
            //mSciEdit.StyleSetFore(21, ColorTranslator.ToWin32(Color.FromArgb(0, 150, 150)));//KeyWord3


            /*
            //# 0 - whitespace
            //# 1, 2, 3, 4 - comments
            //# 5 - number
            //# 6, 7, 8, 9 - keywords: standard, secondary, doc keywords, typedefs and aliases
            //# 10 - string
            //# 11 - string not closed
            //# 12 - char
            //# 13 - operator
            //# 14 - identifier
            //# 15, 16, 17 - Doc comments: line doc /// or //!, doc keyword, doc keyword err
            mSciEdit.StyleSetFore(0, ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)));
            mSciEdit.StyleSetFore(5, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));
            mSciEdit.StyleSetFore(6, ColorTranslator.ToWin32(Color.FromArgb(red, 0, 0)));
            mSciEdit.StyleSetFore(7, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));
            mSciEdit.StyleSetFore(8, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));
            mSciEdit.StyleSetFore(9, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));
            mSciEdit.StyleSetFore(10, ColorTranslator.ToWin32(Color.FromArgb(0, 0, blue)));
            */

            //Set Syntax Language
            mSciEdit.SetLexerLanguage("cpp");
            //mSciEdit.SetLexerLanguage("d");

            //Set ErrorWordList list 
            string ErrorWordList = "ERROR Error error ERROR: Error: error: ";
            mSciEdit.SetKeyWords(0, ErrorWordList);

            //string TagWordList = ""; //"Csound Version version Command Python Lua ";
            //TagWordList += "Compiler arguments Start End ";
            //mSciEdit.SetKeyWords(1, TagWordList);


            //mSciEdit.SetWordChars("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_")

            mSciEdit.UsePopup(false);
            mSciEdit.SetReadOnly(true);
        }



        public string Title
        {
            get { return mTitle; }
            set
            {
                mTitle = value;
                groupBox1.Text = mTitle;
            }
        }

        public string Filename
        {
            get
            {
                return process1.StartInfo.FileName;
            }
            set
            {
                process1.StartInfo.FileName = value;
            }
        }

        public string Arguments
        {
            get
            {
                return process1.StartInfo.Arguments;
            }
            set
            {
                process1.StartInfo.Arguments = value;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return process1.StartInfo.WorkingDirectory;
            }
            set
            {
                process1.StartInfo.WorkingDirectory = value;
            }
        }


        public bool ProcessActive
        {
            get
            {
                try { return (!process1.HasExited); }
                catch { return false; }
            }
        }

        public Int32 GetTextLength()
        {
            return RTB_Output.GetTextLength();
        }

        public void SetEnvironment(string NameValue)
        {
            if(!process1.StartInfo.EnvironmentVariables.ContainsKey("SFDIR"))
                process1.StartInfo.EnvironmentVariables.Add("SFDIR", NameValue);
        }

        public string OutputText
        {
            get
            {
                try
                {
                    //string mString = "Compiler Output:" + newline + newline +
                    //                 RTB_Output.Text + newline + newline +
                    //                 "=============================================" + newline + newline +
                    //                 "Compiler Info:" + newline + newline +
                    //                 RTB_Error.Text;
                    string mString = "Compiler Output:" + newline + newline +
                                     RTB_Output.Text;
                    return mString;
                }
                catch { return ""; }
            }
        }

        public bool ShowLineNumbers
        {
            set { RTB_Output.ShowLineNumbers = value; }
        }

        public bool ReadOnly
        {
            get { return RTB_Output.ReadOnly; }
            set { RTB_Output.ReadOnly = value; }
        }

        public void SetCompilerFont(Font font)
        {
            RTB_Output.SetTextFont(font);
            //RTB_Output.ZoomFactor = 1;
            mFont = font;
        }

        public void ClearText()
        {
            try
            {
                RTB_Output.ReadOnly = false;
                RTB_Output.ClearAllText();
                RTB_Output.ReadOnly = true;
                RTB_Error.ReadOnly = false;
                RTB_Error.ClearAllText();
                RTB_Error.ReadOnly = true;
            }
            catch { }
        }

        public void StopCompiler()
        {
            try
            {
                if (mCompiledWithRT)
                {
                    //Retrieve RTconsole.exe pid
                    Int32 rtconsolePID = 0;
                    foreach (Process p in Process.GetProcesses())
                    {
                        if (p.ProcessName.ToLower().Contains("rtconsole"))
                        {
                            rtconsolePID = p.Id;
                        }
                    }

                    foreach (Process p in Process.GetProcesses())
                    {
                        if (p.ProcessName.ToLower().Contains("python") ||
                           p.ProcessName.ToLower().Contains("lua") ||
                           p.ProcessName.ToLower().Contains("csound"))
                        {
                            //if (GetParentProcess(p.ProcessName) == rtconsolePID)
                            //    p.Kill();

                            if (GetParentProcess(p.Id) == rtconsolePID)
                                p.Kill();
                        }

                    }
                }

                //FINALLY
                if (!process1.HasExited) process1.Kill();
                mCompiledWithRT = false;
                ButtonPauseCompiler.Text = "Pause";

            }
            catch (Exception Exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCompilerConsole - StopCompiler error: " + Exception.Message);
            }
        }



        private void ButtonPauseCompiler_Click(object sender, EventArgs e)
        {
            this.PauseCompiler();
        }
        public void PauseCompiler()
        {
            try
            {
                if (process1.HasExited) return;
            }
            catch { }


            bool _paused = mIsPaused;

            Process[] procs = Process.GetProcessesByName("csound");
            if (procs.Length < 1) return;

            IntPtr handle = IntPtr.Zero;
            foreach (Process p in procs)
            {
                //Check for the right process
                if (GetParentProcess(p.Id) != process1.Id)
                    continue;

                try
                {
                    if (p.Threads[0].ThreadState == ThreadState.Wait)
                    {
                        System.Diagnostics.Debug.WriteLine(p.Threads[0].WaitReason);
                        _paused = (p.Threads[0].WaitReason == ThreadWaitReason.Suspended);
                    }
                }
                catch (Exception ex)
                {
                    _paused = mIsPaused;
                }
                

                foreach (ProcessThread thread in p.Threads)
                {
                    try
                    {
                        handle = ScintillaTextEditor.WinApi.OpenThread(
                            2, false, (uint)thread.Id);
                        if (_paused == true)
                            ScintillaTextEditor.WinApi.ResumeThread(handle);
                        else
                            ScintillaTextEditor.WinApi.SuspendThread(handle);
                    }
                    finally
                    {
                        if (handle != IntPtr.Zero)
                            ScintillaTextEditor.WinApi.CloseHandle(handle);

                        ButtonPauseCompiler.Text = "Pause";
                        mIsPaused = false;
                    }
                }
            }

            mIsPaused = !_paused;
            if (mIsPaused == true)
                ButtonPauseCompiler.Text = "Resume";
            else
                ButtonPauseCompiler.Text = "Pause";

            /*
            foreach (ProcessThread thread in process1.Threads)
            {
                IntPtr handles = ScintillaTextEditor.WinApi.OpenThread(2, false, (uint)thread.Id);
                ScintillaTextEditor.WinApi.SuspendThread(handles);

            }
            */


            
        }


        public bool StartCompiler(bool mVerbose)
        {
            return StartCompiler(mVerbose, true);
        }


        public bool StartCompiler(bool mVerbose, bool mClearPreviousText)
        {
            RTB_Output.ShowLineNumbers = false;
            try
            {
                //Check for other compiler running process
                if (!process1.HasExited)
                {
                    //MessageBox.Show("Compiler is already running!" + newline +
                    //                "Stop it manually and retry",
                    //                "WinXound Info - Compiler busy",
                    //                MessageBoxButtons.OK,
                    //                MessageBoxIcon.Information);
                    return false;
                }
            }
            catch { }

            this.StopCompiler();

            if (mClearPreviousText || this.GetTextLength() >= Int32.MaxValue)
                this.ClearText();

            /*
            if (mVerbose)
            {
                this.Invoke(d,
                    new Object[] { RTB_Output, 
                        "Compiler arguments: [ " + 
                        this.Arguments.Replace("[ORCSCO]","").Trim() + " ]" + 
                        newline + newline });
            }
            */

            //this.Invoke(d, new Object[] { RTB_Output, 
            //            "------- Compiler Start -------" + newline });

            ButtonStopCompiler.Enabled = true;
            ButtonPauseCompiler.Enabled = true;
            ButtonPauseCompiler.Text = "Pause";
            ButtonSaveCompiler.Enabled = false;
            ButtonFindError.Text = "&Go to error ->";
            ButtonFindError.BackColor = Color.Transparent;
            ButtonFindError.Enabled = false;

            mCompiledWithRT = false;
            mIsPaused = false;

            try
            {
                //NOTES:
                //For python and lua we need to use an external RTconsole.exe
                //to capture the console output in real time
                //Thanks to Olivier Marcoux for this great solution
                //http://www.codeproject.com/KB/threads/RTconsole.aspx
                //Now this system is also needed for the 5.12.1 version of
                //CSound
                //if (process1.StartInfo.FileName.Contains("python") ||
                //   process1.StartInfo.FileName.Contains("lua"))
                {
                    process1.StartInfo.Arguments =
                        "\"" + process1.StartInfo.FileName + "\"" + " " +
                        process1.StartInfo.Arguments;

                    process1.StartInfo.FileName =
                        wxGlobal.RTConsoleName;   //"WinXound_RTConsole.exe";

                    System.Diagnostics.Debug.WriteLine(
                        "Compiled with RTConsole.exe: " +
                        process1.StartInfo.Arguments);

                    mCompiledWithRT = true;

                    if (!process1.StartInfo.Arguments.Contains("csound"))
                        ButtonPauseCompiler.Enabled = false;
                }


                bool ret = process1.Start();
                if (ret)
                {
                    process1.StandardInput.AutoFlush = true;
                    process1.StandardOutput.BaseStream.BeginRead(wxOutputBuffer,
                                                             0, wxOutputBuffer.Length,
                                                             new AsyncCallback(ReadCallback),
                                                             null);
                    process1.StandardError.BaseStream.BeginRead(wxErrorBuffer,
                                                            0, wxErrorBuffer.Length,
                                                            new AsyncCallback(ErrorCallback),
                                                            null);
                }
                return true;
            }

            catch { return false; }

        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                this.Invoke(d, new Object[] { RTB_Output, Encoding.ASCII.GetString(wxOutputBuffer, 0, process1.StandardOutput.BaseStream.EndRead(ar)) });

                if (process1.HasExited)
                {
                    this.Invoke(d, new Object[] { RTB_Output, process1.StandardOutput.ReadToEnd() });
                }
                else
                {
                    process1.StandardOutput.BaseStream.BeginRead(wxOutputBuffer,
                                                                 0, wxOutputBuffer.Length,
                                                                 new AsyncCallback(ReadCallback),
                                                                 null);
                }
            }
            catch
            {//wxGlobal.wxMessageError(ex.Message, "wxCompilerConsole - ReadCallback Error")
            }

        }

        private void ErrorCallback(IAsyncResult ar)
        {
            try
            {
                this.Invoke(d, new Object[] { RTB_Error, Encoding.ASCII.GetString(wxErrorBuffer, 0, process1.StandardError.BaseStream.EndRead(ar)) });

                if (process1.HasExited)
                {
                    this.Invoke(d, new Object[] { RTB_Error, process1.StandardError.ReadToEnd() });
                }
                else
                {
                    process1.StandardError.BaseStream.BeginRead(wxErrorBuffer,
                                                                 0, wxErrorBuffer.Length,
                                                                 new AsyncCallback(ErrorCallback),
                                                                 null);
                }
            }
            catch
            {//wxGlobal.wxMessageError(ex.Message, "wxCompilerConsole - ErrorCallback Error")
            }
        }


        private static void AppendText(ScintillaTextEditor.TextEditor mOutputWin, string mString)
        {
            try
            {
                mOutputWin.ReadOnly = false;
                {
                    mOutputWin.SetCaretPosition(mOutputWin.GetTextLength() - 1);
                    mString = mString.Replace("\0", ""); //Erase the [NUL] chars
                    mOutputWin.AddText(mString); //???AppendText???
                    //mOutputWin.AppendText(mString);
                    mOutputWin.SetCaretPosition(mOutputWin.GetTextLength() - 1);
                }
                mOutputWin.ReadOnly = true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCompilerConsole.AppendText Error");
            }

        }


        private static int GetParentProcess(int Id)
        {
            int parentPid = 0;
            using (ManagementObject mo =
                   new ManagementObject("win32_process.handle='" +
                                        Id.ToString(CultureInfo.InvariantCulture)
                                        + "'"))
            {
                try
                {
                    mo.Get();
                }
                catch (ManagementException)
                {
                    return -1;
                }
                parentPid = Convert.ToInt32(mo["ParentProcessId"],
                                            CultureInfo.InvariantCulture);
            }
            return parentPid;
        }




        private void ButtonStopCompiler_Click(object sender, EventArgs e)
        {
            this.StopCompiler();
        }

        private void process1_Exited(object sender, EventArgs e)
        {
            ButtonStopCompiler.Enabled = false;
            ButtonPauseCompiler.Enabled = false;
            ButtonSaveCompiler.Enabled = true;

            //Compiler completed --
            this.Invoke(d, new Object[] { RTB_Output, "------- Compiler End -------" + newline + newline });

            if (RTB_Error.Text.Length > 0)
            {
                string mString = @"Compiler Info/Warnings/Errors:" + newline +
                                 RTB_Error.Text.Trim() + newline;

                this.Invoke(d, new Object[] { RTB_Output, mString });
            }

            string mError = FindError(RTB_Error);
            string mWave = FindSounds(RTB_Output);

            try
            {
                this.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCompilerConsole - process1_Exited Error: " + ex.Message);
            }

            if (OnCompilerCompleted != null)
                OnCompilerCompleted(mError, mWave);


        }

        private string FindError(ScintillaTextEditor.TextEditor mOutputWin)
        {
            string StringToFind = "error:";
            Int32 mFindPos = -1;

            try
            {
                //mFindPos = mOutputWin.Find(StringToFind,
                //           RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                mFindPos = mOutputWin.FindText(StringToFind, true, true, false,
                                               false, false, false, 0, -1);

                if (mFindPos > -1)
                {
                    //Int32 mLineNumber = mOutputWin.GetLineFromCharIndex(mFindPos);
                    Int32 mLineNumber = mOutputWin.GetLineNumberFromPosition(mFindPos);
                    //string mLine = mOutputWin.Lines[mLineNumber - 1].Trim();
                    string mLine = mOutputWin.GetTextLine(mLineNumber);

                    mFindPos = mLine.IndexOf("line");
                    if (mFindPos > 1)
                    {
                        //return mOutputWin.Lines[mLineNumber].TrimEnd();
                        return mOutputWin.GetTextLine(mLineNumber + 1).TrimEnd();
                    }
                    else return "";

                }
                else return "";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCompilerConsole - FindError: " + ex.Message);
                return "";
            }
        }



        private string FindSounds(ScintillaTextEditor.TextEditor mOutputWin)
        {
            string StringToFind = ".wav";
            Int32 mStart = 0;
            Int32 mEnd = mOutputWin.GetTextLength();
            Int32 mFindPos = -1;

            try
            {
                //mFindPos = mOutputWin.Find(StringToFind, mStart, mEnd, RichTextBoxFinds.Reverse);
                mFindPos = mOutputWin.FindText(StringToFind, false, true, true,
                                               false, false, false, mStart, mEnd);

                if (mFindPos > -1)
                {
                    //Int32 mLineNumber = mOutputWin.GetLineFromCharIndex(mFindPos);
                    Int32 mLineNumber = mOutputWin.GetLineNumberFromPosition(mFindPos);
                    //string mLine = mOutputWin.Lines[mLineNumber].Trim();
                    string mLine = mOutputWin.GetTextLine(mLineNumber);
                    //mLine = mLine.ToLower();

                    if (!mLine.ToLower().Contains("written"))
                    {
                        if (!mLine.Contains("writing")) return "";
                    }

                    mFindPos = mLine.ToLower().IndexOf(".wav") - 1;
                    string tempString = "";
                    if (mFindPos > 1)
                    {
                        Int32 mFindStart = -1;
                        mFindStart = mLine.ToLower().IndexOf("written to");
                        if (mFindStart > 0)
                        {

                            mLine = mLine.Remove(mFindPos + 1);
                            tempString = mLine.Substring(mFindStart + 11);
                        }

                        tempString += ".wav";

                        //System.Diagnostics.Debug.WriteLine("FINDSOUNDS: " + tempString);
                        return tempString;

                    }
                    else return "";

                }
                else return "";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxCompilerConsole - FindSounds: " + ex.Message);
                return "";
            }


        }

        private void ButtonSaveCompiler_Click(object sender, EventArgs e)
        {
            //Save CompilerOutput Text 
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;


                //Try to set the output filename
                try
                {
                    if (process1 != null)
                    {
                        string temp = process1.StartInfo.Arguments;
                        temp = temp.Remove(0, temp.IndexOf("\"") + 1);
                        temp = temp.Remove(temp.IndexOf("\"", 1));
                        temp = System.IO.Path.GetFileNameWithoutExtension(temp);
                        saveFileDialog1.FileName = temp + ".output.txt";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "wxCompilerConsole - ButtonSaveCompiler_Click - OutputFileName error: "
                        + ex.Message);

                    saveFileDialog1.FileName = "CSound_Compiler_Output.txt";
                }


                if (!(saveFileDialog1.ShowDialog() == DialogResult.OK) ||
                    string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    return;
                }

                System.IO.File.WriteAllText(saveFileDialog1.FileName, this.OutputText);
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCompilerConsole - ButtonSaveCompiler_Click Error");
            }


        }

        private void buttonStopBatch_Click(object sender, EventArgs e)
        {
            if (OnButtonStopBatch != null) OnButtonStopBatch(sender, e);
        }










    }
}
