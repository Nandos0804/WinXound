using ScintillaTextEditor;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


namespace WinXound_Net
{
    class wxFindCodeStructure : IDisposable
    {
        // From Microsoft MSDN Example:
        // Track whether Dispose has been called.
        private bool disposed = false;

        private TextEditor mTextEditor = null;
        //private Hashtable mHashTable = new Hashtable();
        private TreeView mTreeView = new TreeView();
        private Control mOwner = null;
        private bool mAbort;


        Thread t = null;

        //Types for FindStructure:
        //string StringToFind = @".*instr.+[0123456789]+.*";
        //string StringToFind = @"instr.+[0123456789]+.*";
        //string StringToFind = @"instr";
        string mFsKey = null;
        string mFsValue = null;
        Int32 mFsSearchFlags = 0;
        Int32 mFsStart = 0;
        Int32 mFsEnd = 0;
        Int32 mFsFindPos = -1;

        //public delegate void OnWorkCompleted(object sender, Hashtable hashtable);
        public delegate void OnWorkCompleted(object sender, TreeView treeView);
        public event OnWorkCompleted WorkCompleted;


        public wxFindCodeStructure(Control owner, ref TextEditor te)
        {
            try
            {
                mTextEditor = te;
                mOwner = owner;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - wxFindCodeStructure: " + ex.Message);
            }
        }

        ~wxFindCodeStructure()
        {
            try
            {
                // Do not re-create Dispose clean-up code here.
                // Calling Dispose(false) is optimal in terms of
                // readability and maintainability.
                Dispose(false);
                System.Diagnostics.Debug.WriteLine("~wxFindCodeStructure");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - ~wxFindCodeStructure: " + ex.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Dispose");
                Dispose(true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SupressFinalize to
                // take this object off the finalization queue 
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - Dispose: " + ex.Message);
            }
        }

        private void Dispose(bool disposing)
        {
            try
            {
                // Check to see if Dispose has already been called.
                if (!this.disposed)
                {
                    // If disposing equals true, dispose all managed 
                    // and unmanaged resources.
                    if (disposing)
                    {
                        // Dispose managed resources.
                        if (t != null) t.Abort();
                    }
                    // Call the appropriate methods to clean up 
                    // unmanaged resources here.
                    // If disposing is false, 
                    // only the following code is executed.

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - Dispose: " + ex.Message);
            }

            disposed = true;
        }




        public void Abort()
        {
            mAbort = true;
        }

        public void Start()
        {
            try
            {
                //Thread t = new Thread(FindStructure);
                t = new Thread(FindStructure);
                t.IsBackground = true;
                t.Name = "FindStructure";
                t.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - Start: " + ex.Message);
            }
        }

        public ThreadState State
        {
            get
            {
                try
                {
                    if (t != null) return t.ThreadState;
                    return ThreadState.Stopped;
                    //return Thread.CurrentThread.ThreadState;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "wxFindCodeStructure - State: " + ex.Message);
                    return ThreadState.Stopped;
                }
            }
        }


        private void CheckAbort()
        {
            try
            {
                if (mAbort == true)
                    //if (t != null)
                    //t.Abort();
                    Thread.CurrentThread.Abort();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - CheckAbort: " + ex.Message);
            }
        }


        private void FindStructure()
        {
            try
            {
                //mHashTable.Clear();
                mTreeView.Nodes.Clear();
                //mTreeView = new TreeView();


                //Create rootnode structure
                TreeNode Options = new TreeNode("<CsOptions>");
                Options.Name = "<CsOptions>";
                TreeNode Instruments = new TreeNode("<CsInstruments>");
                Instruments.Name = "<CsInstruments>";
                TreeNode Score = new TreeNode("<CsScore>");
                Score.Name = "<CsScore>";

                mTreeView.Nodes.Add("<CsoundSynthesizer>", "<CsoundSynthesizer>");
                mTreeView.Nodes.Add(Options);
                mTreeView.Nodes.Add(Instruments);
                mTreeView.Nodes.Add(Score);


                //CSOPTIONS
                //if([[wxDefaults valueForKey:@"ExplorerShowOptions"] boolValue])
                if (wxGlobal.Settings.EditorProperties.ExplorerShowOptions)
                {
                    Int32 mStartCsOptions = mTextEditor.FindText("<CsOptions>",
                        true, true, false, false, false, true, 0, -1);


                    if (mStartCsOptions > -1)
                    {
                        mStartCsOptions += 11;

                        Int32 mEndCsOptions = mTextEditor.FindText("</CsOptions>",
                                 true, true, false, false, false, true, 0, -1);

                        if (mEndCsOptions > -1)
                        {
                            Int32 startLine = mTextEditor.GetLineNumberFromPosition(mStartCsOptions) + 1;
                            Int32 endLine = mTextEditor.GetLineNumberFromPosition(mEndCsOptions);
                            Int32 mFindRem = -1;
                            string textOfLine = "";

                            for (Int32 i = startLine; i < endLine; i++)
                            {
                                //SKIP INVALID LINES (REM)
                                textOfLine = mTextEditor.GetTextLine(i);

                                mFindRem = textOfLine.Trim().IndexOf(";");

                                if (mFindRem == 0)
                                {
                                    continue;
                                }
                                else if (textOfLine.Length > 0)
                                {
                                    mFsValue = textOfLine.Trim();
                                    mFsKey = this.ParseLine(mFsValue);
                                    Options.Nodes.Add(mFsValue, mFsKey);
                                    break;
                                }
                            }
                        }
                    }
                }




                ////CSINTRUMENTS: MACROS (#DEFINE)
                //if([[wxDefaults valueForKey:@"ExplorerShowInstrMacros"] boolValue])
                //    [self findString:@"#define" inText:_text withNode:CsInstruments isScore:false];	
                if (wxGlobal.Settings.EditorProperties.ExplorerShowInstrMacros)
                    this.findString(@"#define", Instruments, false);

                ////CSINTRUMENTS: OPCODE
                //if([[wxDefaults valueForKey:@"ExplorerShowInstrOpcodes"] boolValue])
                //    [self findString:@"opcode" inText:_text withNode:CsInstruments isScore:false];	
                if (wxGlobal.Settings.EditorProperties.ExplorerShowInstrOpcodes)
                    this.findString(@"opcode", Instruments, false);

                ////CSINTRUMENTS: INSTR
                //if([[wxDefaults valueForKey:@"ExplorerShowInstrInstruments"] boolValue])
                //    [self findString:@"instr" inText:_text withNode:CsInstruments isScore:false];		
                //TODO: string named instruments!!!
                if (wxGlobal.Settings.EditorProperties.ExplorerShowInstrInstruments)
                    //this.findString(@"instr\s+\d+", Instruments, false);
                    this.findString(@"instr", Instruments, false);


                ////CSSCORE: Functions
                //if([[wxDefaults valueForKey:@"ExplorerShowScoreFunctions"] boolValue])
                //    [self findStringInScore:@"f" inText:_text withNode:CsScore];
                if (wxGlobal.Settings.EditorProperties.ExplorerShowScoreFunctions)
                    this.findStringInScore("f", Score);

                ////CSSCORE: Macros
                //if([[wxDefaults valueForKey:@"ExplorerShowScoreMacros"] boolValue])
                //    [self findString:@"#define" inText:_text withNode:CsScore isScore:true];
                if (wxGlobal.Settings.EditorProperties.ExplorerShowScoreMacros)
                    this.findString(@"#define", Score, true);

                ////CSSCORE: Sections
                //if([[wxDefaults valueForKey:@"ExplorerShowScoreSections"] boolValue])
                //    [self findStringInScore:@"s" inText:_text withNode:CsScore];
                if (wxGlobal.Settings.EditorProperties.ExplorerShowScoreSections)
                    this.findStringInScore("s", Score);






                if (WorkCompleted != null)
                    mOwner.Invoke(WorkCompleted, new Object[] { this, mTreeView });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - FindStructure: " + ex.Message);
            }

            return;
        }

        //private string ParseLine(string Text)
        //{
        //    try
        //    {
        //        Text = Regex.Replace(Text, @"(\s+)", " ");
        //        Match m = Regex.Match(Text, @"\d+");

        //        return Text.Substring(0, m.Index).TrimStart() +
        //               m.Value.PadLeft(3, '0') +
        //               Text.Substring((m.Index + m.Length));
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(
        //            "wxFindCodeStructure - ParseLine: " + ex.Message);
        //        return Text;
        //    }
        //}

        private string ParseLine(string Text)
        {
            try
            {
                Text = Regex.Replace(Text, @"(\s+)", " ");
                //Text = Text.TrimEnd("\n\r".ToCharArray());
                return Text.Trim();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxFindCodeStructure - ParseLine: " + ex.Message);
                return Text;
            }
        }


        private bool findStringInScore(string stringToFind,
                                       TreeNode passedNode)
        {
            //CSSCORE

            //if ([[NSThread currentThread] isCancelled]) return false;

            mFsKey = null;
            mFsValue = null;
            mFsSearchFlags = 0;
            mFsStart = 0;
            mFsEnd = mTextEditor.GetTextLength();
            mFsFindPos = -1;
            mAbort = false;
            Int32 mFindRem = 0;



            Int32 mStartCsScore = mTextEditor.FindText("<CsScore>",
                true, true, false, false, false, true, 0, -1);

            //New: for Orc and Sco
            if (mTextEditor.FileName.EndsWith(".sco"))
                mStartCsScore = 0;

            if (mStartCsScore > -1)
            {
                mStartCsScore += 8;
                Int32 mEndCsScore = mTextEditor.FindText("</CsScore>",
                    true, true, false, false, false, true, 0, -1);

                if (mEndCsScore == -1) mEndCsScore = mFsEnd;

                if (mEndCsScore > -1)
                {

                    Int32 f = -1;
                    Int32 index = 1;
                    Int32 startLine = mTextEditor.GetLineNumberFromPosition(mStartCsScore);
                    Int32 endLine = mTextEditor.GetLineNumberFromPosition(mEndCsScore);
                    string textOfLine = "";

                    for (Int32 i = startLine; i < endLine; i++)
                    {
                        //SKIP INVALID LINES (REM)
                        textOfLine = mTextEditor.GetTextLine(i);

                        mFindRem = textOfLine.Trim().IndexOf(";");

                        if (mFindRem == 0)
                        {
                            continue;
                        }
                        else if (textOfLine.Length > 0)
                        {

                            f = textOfLine.Trim().IndexOf(stringToFind);
                            if (f == 0)
                            {
                                mFsValue = textOfLine.Trim();
                                mFsKey = this.ParseLine(mFsValue); //[self parseString:mFsValue];

                                if (stringToFind == "s")
                                {
                                    mFsKey += " " + index.ToString();
                                    index++;
                                }

                                passedNode.Nodes.Add(mFsValue, mFsKey);
                            }
                        }
                    }
                }
            }

            return true;
        }



        //- (BOOL) findString:(NSString*)stringToFind 
        //inText:(NSString*) _text withNode:(wxNode*)passedNode isScore:(BOOL)isScore
        private bool findString(string stringToFind,
                                TreeNode passedNode,
                                bool isScore)
        {
            mFsKey = null;
            mFsValue = null;
            mFsSearchFlags = 0;
            mFsStart = 0;
            mFsEnd = mTextEditor.GetTextLength();
            mFsFindPos = -1;
            mAbort = false;



            if (isScore) //Score
            {
                mFsStart = mTextEditor.FindText("<CsScore>",
                                                true, true, false, false, false, true,
                                                0, -1);
                mFsEnd = mTextEditor.FindText("</CsScore>",
                                                true, true, false, false, false, true,
                                                0, -1);
            }
            else //Instruments
            {
                mFsStart = mTextEditor.FindText("<CsInstruments>",
                                                true, true, false, false, false, true,
                                                0, -1);
                mFsEnd = mTextEditor.FindText("</CsInstruments>",
                                               true, true, false, false, false, true,
                                               0, -1);
            }


            //New: for Orc and Sco
            if (mTextEditor.FileName.EndsWith(".orc") && isScore == true)
                return false;

            if (mTextEditor.FileName.EndsWith(".orc") ||
                mTextEditor.FileName.EndsWith(".sco"))
            {
                mFsStart = 0;
                mFsEnd = mTextEditor.GetTextLength();
            }



            mFsSearchFlags |= (Int32)SciConst.SCFIND_WHOLEWORD;
            mFsSearchFlags |= (Int32)SciConst.SCFIND_MATCHCASE;
            //mFsSearchFlags |= (Int32)SciConst.SCFIND_REGEXP;

            do
            {
                CheckAbort();
                //System.Diagnostics.Debug.WriteLine(Thread.CurrentThread.Name);

                mTextEditor.PrimaryView.SetTargetStart(mFsStart);
                mTextEditor.PrimaryView.SetTargetEnd(mFsEnd);

                mTextEditor.PrimaryView.SetSearchFlags(mFsSearchFlags);

                //mFsFindPos = tv.PrimaryView.SearchInTarget(@"instr");
                //mFsFindPos = tv.PrimaryView.SearchInTarget(@".*[^;]instr\s+\d+");
                //mFsFindPos = mTextEditor.PrimaryView.SearchInTarget(@"instr\s+\d+");
                mFsFindPos = mTextEditor.PrimaryView.SearchInTarget(stringToFind);
                if (mFsFindPos > -1)
                {
                    mFsStart = mFsFindPos + 5;

                    if (mTextEditor.IsRemAt(mFsFindPos) == false)
                    {
                        mFsValue = mTextEditor.PrimaryView.GetLine(
                                    mTextEditor.PrimaryView.LineFromPosition(mFsFindPos));
                        mFsKey = this.ParseLine(mFsValue);
                        //if (!TempHashTable.Contains(mFsKey))
                        //if (!mHashTable.Contains(mFsKey))
                        {
                            //TempHashTable.Add(mFsKey, mFsValue);
                            //mHashTable.Add(mFsKey, mFsValue);
                            passedNode.Nodes.Add(mFsValue, mFsKey);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            while (true);


            return true;
        }































        //private void FindUserOpcodes(BackgroundWorker worker, DoWorkEventArgs e)
        //{
        //    //RETRIEVE NEW USER OPCODES
        //    TextEditor tv = e.Argument as TextEditor;

        //    //Int32 mStart = -1;
        //    //Int32 mEnd = -1;

        //    mUserOpcodes.Clear();
        //    mUserOpcodesAdded = false;
        //    mFindOpcodeName = "";


        //    //1.
        //    ////Example: opcode  msrosc, a,iii
        //    //MatchCollection mc = Regex.Matches(tv.GetText(), @"\bopcode\b");
        //    //foreach (Match m in mc)
        //    //{
        //    //    if (worker.CancellationPending)
        //    //    {
        //    //        e.Cancel = true;
        //    //        return;
        //    //    }

        //    //    //Skip remmed instruments
        //    //    if (tv.IsRemAt(m.Index)) continue;

        //    //    mFindOpcodeName = tv.GetTextLine(tv.GetLineNumberFromPosition(m.Index));

        //    //    mStart = mFindOpcodeName.IndexOf("opcode");
        //    //    if (mStart == -1) continue;

        //    //    mFindOpcodeName = mFindOpcodeName.Substring(mStart + 6);

        //    //    mEnd = mFindOpcodeName.IndexOf(",");
        //    //    if (mEnd == -1) continue;

        //    //    mFindOpcodeName = mFindOpcodeName.Remove(mEnd).Trim();

        //    //    //System.Diagnostics.Debug.WriteLine(mFindOpcodeName);

        //    //    if (!mUserOpcodes.Contains(mFindOpcodeName))
        //    //    {
        //    //        mUserOpcodes.Add(mFindOpcodeName);
        //    //        mUserOpcodesAdded = true;
        //    //    }

        //    //}


        //    ////2.
        //    ////Example: opcode  msrosc, a,iii
        //    //MatchCollection mc = Regex.Matches(tv.GetText(), @"\bopcode\b\s+\w+\s*,");
        //    //foreach (Match m in mc)
        //    //{
        //    //    if (worker.CancellationPending)
        //    //    {
        //    //        e.Cancel = true;
        //    //        return;
        //    //    }

        //    //    //Skip remmed instruments
        //    //    if (tv.IsRemAt(m.Index)) continue;

        //    //    mFindOpcodeName = Regex.Replace(m.Value, @"(\s+)|(opcode)|(,)", "");

        //    //    //System.Diagnostics.Debug.WriteLine(mFindOpcodeName);

        //    //    if (!mUserOpcodes.Contains(mFindOpcodeName))
        //    //    {
        //    //        mUserOpcodes.Add(mFindOpcodeName);
        //    //        mUserOpcodesAdded = true;
        //    //    }
        //    //}


        //    //3.
        //    mFsValue = "";
        //    mFsKey = "";
        //    mFsSearchFlags = 0;
        //    mFsStart = 0;
        //    mFsEnd = 0;
        //    mFsFindPos = -1;
        //    do
        //    {
        //        if (worker.CancellationPending)
        //        {
        //            e.Cancel = true;
        //            break;
        //        }

        //        tv.PrimaryView.SetTargetStart(mFsStart);
        //        tv.PrimaryView.SetTargetEnd(tv.GetTextLength());

        //        mFsSearchFlags |= (Int32)SciConst.SCFIND_WHOLEWORD;
        //        mFsSearchFlags |= (Int32)SciConst.SCFIND_MATCHCASE;
        //        mFsSearchFlags |= (Int32)SciConst.SCFIND_REGEXP;
        //        tv.PrimaryView.SetSearchFlags(mFsSearchFlags);

        //        //@"\bopcode\b\s+\w+\s*,"
        //        mFsFindPos = tv.PrimaryView.SearchInTarget(@"opcode\s+\w+\s*,");
        //        //TextView.TextToFind text = new TextView.TextToFind();
        //        //text.chrg.cpMin = mFsStart;
        //        //text.chrg.cpMax = tv.GetTextLength();
        //        //text.lpstrText = Marshal.StringToHGlobalAnsi(@"opcode\s+\w+\s*,").ToInt32();
        //        //mFsFindPos = tv.PrimaryView.FindText(mFsSearchFlags, text);
        //        if (mFsFindPos > -1)
        //        {
        //            mFsStart = mFsFindPos + 6;

        //            if (tv.IsRemAt(mFsFindPos) == false)
        //            {
        //                //mFsValue = tv.PrimaryView.GetLine(
        //                //           tv.PrimaryView.LineFromPosition(mFsFindPos));
        //                //mFsValue = tv.GetText().Substring(text.chrgText.cpMin,
        //                //                                  text.chrgText.cpMax - text.chrgText.cpMin);
        //                mFsValue = tv.GetText().Substring(mFsFindPos,
        //                           tv.GetLineEndPosition(
        //                           tv.GetLineNumberFromPosition(mFsFindPos)) -
        //                           mFsFindPos);

        //                //mFindOpcodeName = Regex.Replace(mFsValue, @"(\s+)|(opcode)|(,)", "");
        //                //mFindOpcodeName = Regex.Split(mFsValue,
        //                //                 @"(\s+)|(,)").GetValue(2).ToString();
        //                mFindOpcodeName = mFsValue.Remove(mFsValue.IndexOf(","));
        //                mFindOpcodeName = mFindOpcodeName.TrimStart().Substring(7).Trim();


        //                System.Diagnostics.Debug.WriteLine(mFindOpcodeName);

        //                if (!mUserOpcodes.Contains(mFindOpcodeName))
        //                {
        //                    mUserOpcodes.Add(mFindOpcodeName);
        //                    mUserOpcodesAdded = true;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    while (true);





        //    //If founded add user opcodes to syntax highlight
        //    //'UserOpcodes (SCE_C_WORD3 = 20)
        //    if (mUserOpcodes.Count > 0 && mUserOpcodesAdded == true)
        //    {
        //        string mOpcodes = "";
        //        foreach (string m in mUserOpcodes)
        //        {
        //            mOpcodes += m + " ";
        //        }
        //        tv.SetKeyWords(2, mOpcodes); //2
        //        tv.PrimaryView.Colourise(0, 0);
        //        tv.SecondaryView.Colourise(0, 0);
        //    }

        //}





        #region " OLD CODE REMOVED "

        ////2. 
        //// \b = wholeword
        //MatchCollection mc = Regex.Matches(mTextEditor.GetText(), @"\binstr\b");
        //foreach (Match m in mc)
        //{
        //    CheckAbort();

        //    //Skip remmed instruments
        //    if (mTextEditor.IsRemAt(m.Index)) continue;

        //    mFsValue = mTextEditor.PrimaryView.GetLine(
        //               mTextEditor.PrimaryView.LineFromPosition(m.Index));
        //    mFsKey = this.ParseLine(mFsValue);
        //    //if (!mHashTable.Contains(mKey))
        //    if (!mHashTable.Contains(mFsKey))
        //    {
        //        ////Limit the number of instruments
        //        //if (mHashTable.Count > 1000) break;
        //        //mHashTable.Add(mKey, mValue);
        //        mHashTable.Add(mFsKey, mFsValue);
        //    }
        //}
        ////int oldCacheSize = Regex.CacheSize;
        ////Regex.CacheSize = 0;
        ////GC.Collect();
        ////Regex.CacheSize = oldCacheSize; 

        #endregion

    }
}
