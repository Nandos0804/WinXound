using System;
using System.Collections;
using System.Text.RegularExpressions;


namespace WinXound_Net
{
    class wxCodeFormatter
    {

        private ScintillaTextEditor.TextEditor mEditor =
                        null; //new ScintillaTextEditor.TextEditor();

        private Hashtable KeyWords = new Hashtable();

        string newline = System.Environment.NewLine;


        //Constructor
        public wxCodeFormatter(ScintillaTextEditor.TextEditor editor,
                               Hashtable keywords)
        {
            mEditor = editor;
            KeyWords = keywords;
        }



        //public string FormatCode()
        public void FormatCode()
        {
            //return FormatCode(0, mEditor.GetLines());
            this.FormatCode(0, mEditor.GetLines());
        }

        //public string FormatCode(Int32 start, Int32 end)
        public void FormatCode(Int32 start, Int32 end)
        {
            try
            {
                string textline = "";
                string tempString = "";
                bool isSinglelineRem = false;
                bool isMultilineRem = false;
                Int32 remIndex = 0;
                //bool isOrchestra = false;
                bool isInstrument = false;
                bool isScore = false;
                bool isReplaced = false;
                Int32 startpos = 0;
                char[] delimiter = new char[] { ' ', '\t', '\r', '\n' };


                //SET START - END 
                //Remember: passed arguments (start-end) are expressed as line number
                Int32 mStart = start;
                Int32 mEnd = end;

                //If the textEditor has some selected text we must
                //look before to find the right csound section (Score or Instruments)
                if (mStart > 0)
                {
                    Int32 tempRet = mEditor.FindText("<CsScore>",
                                                     true,
                                                     true,
                                                     true,
                                                     false,
                                                     false,
                                                     true);

                    if (tempRet > -1)
                        isScore = true;
                    else
                    {
                        tempRet = mEditor.FindText("instr",
                                                   true,
                                                   true,
                                                   true,
                                                   false,
                                                   false,
                                                   true);

                        if (tempRet > -1)
                            isInstrument = true;
                    }
                }




                Int32 tempindex = 0;
                Int32 lineLength = 0;

                //mEditor.TabIndent = mCodeFormat.TabIndent;

                mEditor.GetFocusedEditor.BeginUndoAction();

                for (Int32 i = mStart; i <= mEnd; i++)
                {
                    //Real line length without LineEndings chars = 
                    //SCI_GETLINEENDPOSITION(line) - SCI_POSITIONFROMLINE(line)
                    //Skip empty lines
                    lineLength = mEditor.GetLineEndPosition(i) - mEditor.GetPositionFromLineNumber(i);
                    if (lineLength == 0)
                        continue;


                    //Retrieve the text of the line
                    textline = mEditor.GetTextLine(i).TrimStart();

                    //Skip line without chars (after TrimStart())
                    if (string.IsNullOrEmpty(textline)) continue;

                    isSinglelineRem = false;
                    isReplaced = false;
                    startpos = mEditor.GetPositionFromLineNumber(i);

                    foreach (string word in textline.Split(delimiter))
                    {
                        //Skip empty words
                        if (string.IsNullOrEmpty(word)) continue;

                        //Check for single line rem
                        if (word.Contains(";") && isMultilineRem == false)
                        {
                            isSinglelineRem = true;
                            remIndex = textline.IndexOf(";");
                            break;
                        }

                        //Check for Multiline rem ('/*' and '*/')
                        if (word.Contains(@"/*") && isSinglelineRem == false)
                        {
                            isMultilineRem = true;
                        }
                        if (word.Contains(@"*/") && isMultilineRem == true /*&& 
                        isSinglelineRem == false*/
                                                      )
                        {
                            isMultilineRem = false;
                        }
                        if (isMultilineRem == true) continue;

                        ////Check for CsInstruments section
                        //if (word == "<CsInstruments>" && isSinglelineRem == false 
                        //    && isMultilineRem == false)
                        //    isOrchestra = true;
                        //if (word == "</CsInstruments>" && isSinglelineRem == false 
                        //    && isMultilineRem == false)
                        //    isOrchestra = false;

                        //Check for CsScore section
                        if (word == "<CsScore>" && isSinglelineRem == false
                            && isMultilineRem == false)
                            isScore = true;
                        if (word == "</CsScore>" && isSinglelineRem == false
                            && isMultilineRem == false)
                            isScore = false;
                        //If is CsScore section skip opcodes check
                        if (isScore == true) break;


                        //Check for opcodes
                        if (word == "instr" ||
                            word == "endin" ||
                            word == "opcode" ||
                            word == "endop")
                        {
                            //Set isInstruments for further opcodes search
                            if (word == "instr" && isSinglelineRem == false && isMultilineRem == false)
                                isInstrument = true;
                            if (word == "endin" && isSinglelineRem == false && isMultilineRem == false)
                                isInstrument = false;

                            if (wxGlobal.Settings.CodeFormat.FormatInstruments)
                            {
                                //Style 1: Add tab to start
                                if (wxGlobal.Settings.CodeFormat.InstrumentsType == 0)
                                {
                                    textline = "\t" + textline;
                                    mEditor.ReplaceTarget(startpos, lineLength, textline);
                                }
                                //Style 2: No tab space at start
                                else
                                {
                                    mEditor.ReplaceTarget(startpos, lineLength, textline);
                                }
                            }
                            isReplaced = true;
                            break;
                        }

                        else if (word.Contains("sr") ||
                                 word.Contains("kr") ||
                                 word.Contains("ksmps") ||
                                 word.Contains("nchnls") ||
                                 word.Contains("0dbfs"))
                        //else if (word == "sr" ||
                        //         word == "kr" ||
                        //         word == "ksmps" ||
                        //         word == "nchnls" ||
                        //         word == "0dbfs" ||
                        //         word == "sr=" ||
                        //         word == "kr=" ||
                        //         word == "ksmps=" ||
                        //         word == "nchnls=" ||
                        //         word == "0dbfs=")
                        {
                            if (wxGlobal.Settings.CodeFormat.FormatHeader)
                            {
                                textline = Regex.Replace(textline, @"\s+", " ");// +newline;

                                tempindex = textline.IndexOf("=");
                                if (tempindex > -1)
                                    textline = "\t" + textline.Substring(0, tempindex).Trim() +
                                               "\t" + textline.Substring(tempindex).Trim();// + newline;

                                mEditor.ReplaceTarget(startpos, lineLength, textline);
                            }
                            isReplaced = true;
                            break;
                        }

                        else if (word == "if" ||
                                 word == "elseif" ||
                                 word == "endif")
                        {
                            mEditor.ReplaceTarget(startpos, lineLength, textline);
                            isReplaced = true;
                            break;
                        }

                        else if (KeyWords.Contains(word) || word == "=")
                        {
                            //Replace multispace with one space
                            textline = Regex.Replace(textline, @"\s+", " ");

                            //Split line in single words
                            string[] split = textline.Split(delimiter);
                            string mCompose = "";

                            //Check inside instr/endin section
                            if (wxGlobal.Settings.CodeFormat.FormatInstruments &&
                                isInstrument == true)
                            {
                                if (wxGlobal.Settings.CodeFormat.InstrumentsType == 0)
                                {
                                    for (Int32 n = 0; n < split.Length; n++)
                                    {
                                        if (split[n] == word)
                                        {
                                            mCompose += "\t" + split[n] + "\t";
                                        }
                                        else mCompose += split[n] + " ";
                                    }
                                }
                                else
                                {
                                    mCompose = "\t" + textline;
                                }

                                //isReplaced = true;
                                //break;
                            }

                            else if (isInstrument == true)
                            {
                                isReplaced = false;
                                break;
                            }

                            //check outside (but inside CsOrchestra)
                            else if (isInstrument == false)
                            {
                                for (Int32 n = 0; n < split.Length; n++)
                                {
                                    if (split[n] == word)
                                    {
                                        //opcode is after the first word
                                        if (n > 0)
                                        {
                                            mCompose += "\t"; //+ split[n] + "\t";
                                        }
                                        mCompose += split[n] + "\t";
                                    }
                                    else mCompose += split[n] + " ";
                                }
                            }

                            //Finally replace the line
                            mEditor.ReplaceTarget(
                                startpos, lineLength, mCompose.TrimEnd());// + newline);

                            isReplaced = true;
                            break;
                        }
                    }

                    if (isSinglelineRem == true &&
                        isMultilineRem == false &&
                        isScore == true)
                    {

                        if (CheckScoreForGoodLine(textline) == false) continue;

                        remIndex = textline.IndexOf(";");
                        tempString = textline.Substring(0, remIndex);
                        textline = Regex.Replace(tempString, @"\s+", " ") +
                                    textline.Substring(remIndex);


                        remIndex = textline.IndexOf(";");
                        tempString = textline.Substring(0, remIndex);
                        textline = Regex.Replace(tempString, " ", "\t") +
                                                  textline.Substring(remIndex);
                    }
                    else if (isSinglelineRem == false &&
                             isMultilineRem == false &&
                             isScore == true)
                    {
                        if (CheckScoreForGoodLine(textline) == false) continue;

                        textline = Regex.Replace(textline, @"\s+", " ");// + newline;
                        textline = Regex.Replace(textline, " ", "\t");
                    }

                    //Finally replace line
                    if (isReplaced == false)
                    {
                        mEditor.ReplaceTarget(startpos, lineLength, textline);
                    }

                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCodeFormatter - FormatCode");
            }

            mEditor.GetFocusedEditor.EndUndoAction();

            //return mEditor.GetText();

        }

        private bool CheckScoreForGoodLine(string textline)
        {
            if (textline.StartsWith("f") &&
                wxGlobal.Settings.CodeFormat.FormatFunctions == false)
                return false;

            if (textline.StartsWith("i") &&
                wxGlobal.Settings.CodeFormat.FormatScoreInstruments == false)
                return false;

            if (textline.StartsWith("t") &&
                wxGlobal.Settings.CodeFormat.FormatTempo == false)
                return false;

            return true;
        }





        /////////////////////////////////////////////////////////////////////////////////////
        //For the moment unused
        private ArrayList FindBlockOfRems()
        {
            ArrayList tempArray = new ArrayList();

            string pattern = @"/\*";
            MatchCollection mcStart = Regex.Matches(mEditor.GetText(), pattern);

            pattern = @"\*/";
            MatchCollection mcEnd = Regex.Matches(mEditor.GetText(), pattern);

            foreach (Match m in mcStart)
            {
                tempArray.Add(m.Index.ToString() + ",");
            }

            for (Int32 i = 0; i < tempArray.Count; i++)
            {
                if (mcEnd[i] != null)
                    tempArray[i] = tempArray[i].ToString() + mcEnd[i].Index.ToString();
            }

            return tempArray;

        }



    }
}
