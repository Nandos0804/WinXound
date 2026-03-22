using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace OpcodesParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private List<string> CSoundOpcodes;
        private List<string> MaldonadoOpcodes;
        private List<string> FinalList;

        private bool mStop = false;
        private string newline = System.Environment.NewLine;
        private bool mFormIsClosing = false;

        private const Int32 MAX_CHARS = 255;

        private string mCSoundConsolePath = null;



        private void Form1_Load(object sender, EventArgs e)
        {
            AutoSearchCSound();

            if (!File.Exists(mCSoundConsolePath))
            {
                MessageBox.Show("CSound compiler not found!", "Error");
                this.Close();
            }

            labelInfo.Text = "CSound Path: " + mCSoundConsolePath;
        }

        private void AutoSearchCSound()
        {
            //We try to retrieve the CSound path from [CSOUNDRC] and search for executables and manuals
            string mPath = "";
            string CSoundPath = "";

            try
            {
                mPath = System.Environment.GetEnvironmentVariable("CSOUNDRC");
                if (mPath == null) return;

                if (File.Exists(mPath))
                {
                    CSoundPath = Directory.GetParent(mPath).ToString();
                    if (File.Exists(CSoundPath + "\\bin\\csound.exe"))
                    {
                        //wxGlobal.Settings.Directory.CSoundConsole =
                        //    CSoundPath + "\\bin\\csound.exe";
                        mCSoundConsolePath =
                            CSoundPath + "\\bin\\csound.exe";
                    }
                }
            }

            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message,
                //    "FormSettingsAutoSearch - AutoSearchCSound");
                System.Diagnostics.Debug.WriteLine(
                    "FormSettingsAutoSearch - AutoSearchCSound: " + ex.Message);
            }
        }

        private void ButtonStopCompiler_Click(object sender, EventArgs e)
        {
            mStop = true;
        }


        private void ProcessStopped()
        {
            mStop = false;
            ButtonStopCompiler.Enabled = false;
            buttonStart.Enabled = true;
            //buttonRestoreOpcodesFile.Enabled = true;
            progressBar1.Value = 0;

            //Restore original file
            //buttonRestoreOpcodesFile.PerformClick();

            if (mFormIsClosing == true) this.Close();
        }


        private void FormCreateOpcodesFile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ButtonStopCompiler.Enabled)
            {
                DialogResult ret =
                     MessageBox.Show("The process is still running." + newline +
                                     "Do you want to stop it and exit?",
                                     "Process running",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Stop);

                if (ret == DialogResult.Yes)
                {
                    mFormIsClosing = true;
                    this.ButtonStopCompiler.PerformClick();
                    return;
                }
                else e.Cancel = true;
            }
            mFormIsClosing = false;
        }





        /////////////////////////////////////////////////////////////////////////////////////////
        //START PROCESS
        private void buttonStart_Click(object sender, EventArgs e)
        {

            ButtonStopCompiler.Enabled = true;
            buttonStart.Enabled = false;
            //buttonRestoreOpcodesFile.Enabled = false;

            labelInfo.Text = "Searching for csound opcodes ..." + newline;

            Process mtProc = new Process();
            mtProc.StartInfo.FileName = "cmd.exe";
            mtProc.StartInfo.Arguments =
                     "/C cls && " +
                     "\"" + mCSoundConsolePath + "\" " +
                     "-z > " +
                     "\"" +
                     Application.StartupPath + "\\csound_output.txt" +
                     "\"";

            //System.Diagnostics.Debug.WriteLine(mtProc.StartInfo.Arguments);

            mtProc.StartInfo.UseShellExecute = false;
            mtProc.StartInfo.RedirectStandardOutput = true;
            mtProc.StartInfo.CreateNoWindow = true;
            mtProc.EnableRaisingEvents = true;

            mtProc.Start();
            mtProc.WaitForExit();
            ParseOpcodes();
        }





        /////////////////////////////////////////////////////////////////////////////////////////
        // PARSING PROCESSINGS
        private void ParseOpcodes()
        {

            labelInfo.Text = "Parsing opcodes (please wait)" + newline;

            //Parsing the opcodes 
            Int32 mStart = 0;
            Int32 mEnd = 0;


            //Load csound generated output file (output.txt)
            string csound_output = "";
            if (File.Exists(Application.StartupPath + "\\csound_output.txt"))
            {
                csound_output =
                   File.ReadAllText(
                       Application.StartupPath + "\\csound_output.txt");
            }

            if (string.IsNullOrEmpty(csound_output)) return;


            //Delete output.txt temporary file
            try
            {
                File.Delete(Application.StartupPath + "\\csound_output.txt");
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(
                    ex.Message, "FormCreateOpcodesFile - ParseOpcodes Error");
            }


            //Find Start and End of opcodes text from csound output
            mStart = csound_output.IndexOf("opcodes");
            mEnd = csound_output.Length;
            if (mStart > 0 && mEnd > 0)
            {
                mStart += 7;
                csound_output = csound_output.Substring(mStart).Trim();
            }


            //Delete all non-chars and non-numbers charachters
            //Regex.Replace(tempRTBproc.Text, @"[\W]", "");
            //Regex.Replace(tempRTBproc.Text, @"[^a-zA-Z0-9]", "");
            string reg = csound_output;
            reg =
                System.Text.RegularExpressions.Regex.Replace(
                    reg, @"\s+", " ");
            reg =
                System.Text.RegularExpressions.Regex.Replace(
                    reg, @"[^a-zA-Z0-9_ ]", "");


            //split string to mOpcodes (List<string>)
            CSoundOpcodes = new List<string>(reg.Split(" ".ToCharArray()));


            //Check for some missing CSound opcodes 
            string[] mStringsToFind =
                        new string[]{ 
                        "else", "elseif", "endif", 
                        "flashtxt", "if", "kr", "ksmps", 
                        "lorisread", "lorismorph", "lorisplay", 
                        "nchnls", "sr", "0dbfs", 
                        "add", "and", "div", "mod", "mul", "or", "sub"}; ///???
            bool mFound = false;
            foreach (string s in mStringsToFind)
            {
                mFound = false;
                foreach (string mString in CSoundOpcodes)
                {
                    if (mString.Trim() == s)
                    {
                        mFound = true;
                        break;
                    }
                }
                if (mFound == false) CSoundOpcodes.Add(s);

                Application.DoEvents();
                if (mStop)
                {
                    ProcessStopped();
                    return;
                }
            }


            //Add Maldonado CSoundAV Opcodes 
            StreamReader reader =
                wxGlobal.GetResource("Maldonado_Opcodes.txt");
            MaldonadoOpcodes = new List<string>();
            string mSt = "";
            while (!(reader.Peek() == -1))
            {
                mSt = reader.ReadLine();
                mSt = mSt.Trim();
                mFound = false;

                foreach (string mString in MaldonadoOpcodes)
                {
                    if (mString.Trim() == mSt) mFound = true;
                }
                if (mFound == false) MaldonadoOpcodes.Add(mSt);
            }
            reader.Close();


            //Sort all opcodes
            CSoundOpcodes.Sort();


            //Output opcodes to RTB_Output
            RTB_Output.Text = "CSound Opcodes List" + newline;
            Int32 index = 0;
            foreach (string s in CSoundOpcodes)
            {
                Application.DoEvents();
                if (mStop)
                {
                    ProcessStopped();
                    return;
                }
                index++;
                RTB_Output.AppendText(index.ToString() + ": " + s + newline);
            }

            if (mStop)
            {
                ProcessStopped();
                return;
            }

            ////return;

            //CreateWinXoundOpcodesDescription();

            CreateWinXoundOpcodesDescription2();

        }




        #region " OLD "
        private void CreateWinXoundOpcodesDescription()
        {


            //We try to retrieve the CSound path and search for executable and manual 

            Int32 pStart = default(Int32);
            Int32 pEnd = default(Int32);
            Int32 pTemp = default(Int32);
            string pOpcode = "";
            string pSyntax = "";
            string pDescription = "";
            string pField1 = "";
            string pField2 = "";
            string pField3 = "";

            string mFile = "";

            TextBox tempRTB = new TextBox();
            TextBox RTB1 = new TextBox();



            string mPath = System.Environment.GetEnvironmentVariable("CSOUNDRC");
            string ManualPath = "";

            if (File.Exists(mPath))
            {
                string CSoundPath = Directory.GetParent(mPath).ToString();
                if (File.Exists(CSoundPath + "\\doc\\manual\\index.html"))
                {
                    ManualPath = CSoundPath + "\\doc\\manual\\";
                }
            }
            else return;


            RTB1.Text = "\"" + "Opcodes" + "\"" + ";" +
                        "\"" + "Fields" + "\"" + ";" +
                        "\"" + "Description" + "\"" + newline;


            labelInfo.Text = "Parsing CSound Manual info ...";
            progressBar1.Value = 0;
            progressBar1.Maximum = CSoundOpcodes.Count;

            //For Debug: 100 or mOpcodes.Count 
            for (Int32 mIndex = 0; mIndex < CSoundOpcodes.Count; mIndex++)
            {
                Application.DoEvents();
                if (mStop)
                {
                    ProcessStopped();
                    return;
                }

                //mEx += 1;
                progressBar1.Value = mIndex;
                //if (mIndex % 10 == 1)
                //{
                //    labelInfo.Text = "CSound Manual Parsing: " + mEx.ToString();
                //}

                pOpcode = "\"" + CSoundOpcodes[mIndex] + "\"";
                pSyntax = "\"" + "(Syntax not available)" + "\"";
                pDescription = "\"" + "(Description not available)" + "\"";


                mFile = ManualPath + CSoundOpcodes[mIndex] + ".html";


                //SWITCH SOME MANUAL NAMES
                //0dbfs = Zerodbfs.html
                if (mFile == ManualPath + "0dbfs.html") mFile = ManualPath + "Zerodbfs.html";
                //TBn family
                if (CSoundOpcodes[mIndex].StartsWith("tb") &&
                    char.IsDigit(CSoundOpcodes[mIndex], 2))
                {
                    pDescription = "\"" + @"Table Read Access inside expressions." + "\"";
                    pSyntax = "\"" + "[tbN_init] ifn  ...  iout = tbN(iIndex)   or   kout = tbN(kIndex)" + "\"";
                    mFile = ""; //To skip file search
                }
                //PyAssign family
                if (CSoundOpcodes[mIndex].StartsWith("pyassign") ||
                    CSoundOpcodes[mIndex].StartsWith("pylassign"))
                {
                    pDescription = "\"" + @"Assign the value of the given Csound variable to a Python variable possibly destroying its previous content." + "\"";
                    pSyntax = "\"" + "Please see CSound manual (Click F1)" + "\"";
                    mFile = ""; //To skip file search
                }
                //PyCall family
                if (CSoundOpcodes[mIndex].StartsWith("pycall") ||
                    CSoundOpcodes[mIndex].StartsWith("pylcall"))
                {
                    pDescription = "\"" + @"Invoke the specified Python callable at k-time and i-time (i suffix), passing the given arguments." + "\"";
                    pSyntax = "\"" + "Please see CSound manual (Click F1)" + "\"";
                    mFile = ""; //To skip file search
                }
                //PyEval family
                if (CSoundOpcodes[mIndex].StartsWith("pyeval") ||
                    CSoundOpcodes[mIndex].StartsWith("pyleval"))
                {
                    pDescription = "\"" + @"Evaluate a generic Python expression and store the result in a Csound variable at k-time or i-time (i suffix)." + "\"";
                    pSyntax = "\"" + "Please see CSound manual (Click F1)" + "\"";
                    mFile = ""; //To skip file search
                }
                //PyExec family
                if (CSoundOpcodes[mIndex].StartsWith("pyexec") ||
                    CSoundOpcodes[mIndex].StartsWith("pylexec"))
                {
                    pDescription = "\"" + @"Execute a script from a file at k-time or i-time (i suffix)." + "\"";
                    pSyntax = "\"" + "Please see CSound manual (Click F1)" + "\"";
                    mFile = ""; //To skip file search
                }
                //PyRun family
                if (CSoundOpcodes[mIndex].StartsWith("pyrun") ||
                    CSoundOpcodes[mIndex].StartsWith("pylrun"))
                {
                    pDescription = "\"" + @"Execute the specified Python statement (or block of statements) at k-time (pyrun and pylrun) or i-time (pyruni and pylruni)." + "\"";
                    pSyntax = "\"" + "Please see CSound manual (Click F1)" + "\"";
                    mFile = ""; //To skip file search
                }





                //RETRIEVE INFORMATION FROM THE CSOUND HTML MANUAL
                if (File.Exists(mFile))
                {

                    tempRTB.Text = "";
                    //tempRTB.LoadFile(mFile, RichTextBoxStreamType.PlainText);
                    tempRTB.Text = File.ReadAllText(mFile);


                    //DESCRIPTION 
                    pTemp = tempRTB.Text.IndexOf("refentrytitle");
                    if (pTemp > 0)
                    {
                        pStart = tempRTB.Text.IndexOf("<p>");
                        if (pStart > 0)
                        {
                            pStart += 3;
                            pEnd = tempRTB.Text.IndexOf("</p>", pStart);
                            if (pEnd > 0)
                            {
                                pDescription = tempRTB.Text.Substring(pStart, pEnd - pStart);
                                pDescription = RemoveChars(pDescription, CSoundOpcodes[mIndex]);

                                if (pDescription.Length > MAX_CHARS) pDescription = pDescription.Remove(MAX_CHARS);
                                pDescription = "\"" + pDescription.Trim() + "\"";
                            }

                        }
                    }


                    //SYNTAX 
                    //Examples: 
                    //<pre class="synopsis"><span><b class="command">a</b></span> p1 p2 p3</pre> 
                    //<pre class="synopsis">a1, a2 <span><b class="command">babo</b></span> asig, ksrcx, ksrcy, ksrcz, irx, iry, irz [, idiff] [, ifno]</pre> 
                    pStart = tempRTB.Text.IndexOf("synopsis");
                    if (pStart > 0)
                    {
                        //pTemp = pStart + 8 ''tempRTB.Text.IndexOf("<pre", pStart + 8) 
                        //If pStart > 0 Then 
                        pTemp = tempRTB.Text.IndexOf(">", pStart);
                        if (pTemp > 0)
                        {
                            pStart = pTemp + 1;
                            pEnd = tempRTB.Text.IndexOf("<", pStart);
                            pField1 = tempRTB.Text.Substring(pStart, pEnd - pStart);
                        }

                        pTemp = tempRTB.Text.IndexOf("<strong>", pStart);
                        if (pTemp > 0)
                        {
                            pStart = pTemp + 8;
                            //'tempRTB.Text.IndexOf(">", pStart) 
                            //If pStart > 0 Then 
                            //'pStart = pTemp + 1 
                            pEnd = tempRTB.Text.IndexOf("<", pStart);
                            pField2 = tempRTB.Text.Substring(pStart, pEnd - pStart);


                            pEnd = tempRTB.Text.IndexOf("</pre>", pStart);
                            if (pEnd > 0)
                            {
                                for (Int32 ciclo = pEnd; ciclo >= pStart; ciclo += -1)
                                {
                                    if (tempRTB.Text.Substring(ciclo, 1) == ">")
                                    {
                                        pStart = ciclo + 1;
                                        pField3 = tempRTB.Text.Substring(pStart, pEnd - pStart);
                                        break; // TODO: might not be correct. Was : Exit For 
                                    }
                                }

                            }

                            pField1 = RemoveChars(pField1, null);
                            pField2 = RemoveChars(pField2, null);
                            pField3 = RemoveChars(pField3, null);

                            //pSyntax = Chr(34) & pField1 & " [" & pField2 & "] " & pField3 & Chr(34) 
                            pSyntax = pField1 + "[" + pField2 + "]" + pField3;

                            //Prepare syntax for final output 
                            pSyntax = pSyntax.Trim();
                            //Remove unwanted chars 
                            RemoveChars(pSyntax, null);
                            if (pSyntax.Length > MAX_CHARS) pSyntax = pSyntax.Remove(MAX_CHARS);
                            pSyntax = "\"" + pSyntax + "\"";
                        }


                    }



                    //Display final data 
                    RTB1.AppendText(pOpcode + ";" + pSyntax + ";" +
                                    pDescription + newline);
                }

                else
                {

                    RTB1.AppendText(pOpcode + ";" + pSyntax + ";" +
                                    pDescription + newline);
                }

            }


            labelInfo.Text = "FINISHED - Total files parsed: " + CSoundOpcodes.Count.ToString();//mEx.ToString();
            progressBar1.Value = 0;


            Application.DoEvents();
            if (mStop)
            {
                ProcessStopped();
                return;
            }

            //SAVE FILES TO "opcodes.csv"
            File.WriteAllText(
                Application.StartupPath + "\\Settings\\opcodes.csv",
                RTB1.Text);


            RTB_Output.Clear();
            RTB_Output.Text = File.ReadAllText(
                Application.StartupPath + "\\Settings\\opcodes.csv");
            RTB_Output.SelectionStart = 0;
            RTB_Output.SelectionLength = 0;


            Application.DoEvents();
            if (mStop)
            {
                ProcessStopped();
                return;
            }


            ButtonStopCompiler.Enabled = false;
            buttonStart.Enabled = true;

            MessageBox.Show("'opcodes.csv' database file updated successfully!" + newline +
                            "Please restart WinXound for the changes to take effect.",
                            "WinXound Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

            //this.DialogResult = DialogResult.OK;
            //this.Close();

        }

        #endregion




        private void CreateWinXoundOpcodesDescription2()
        {
            FinalList = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(Application.StartupPath + "\\opcodes.xml");
            FinalList = PopulateList(doc.ChildNodes);

            bool founded = false;


            ////OUTPUT ORDER
            //ParseLine(Name.Replace(@"\", "")) +
            //ParseLine(Category.Replace(@"\", "")) +
            //ParseLine(Description.Replace(@"\", "")) +
            //ParseLine(Synopsis.Replace(@"\", ""))

            string Name = "";
            string Category = "";
            string Description = "";
            string Synopsis = "";


            //ADD MaldonadoOpcodes
            for (Int32 mIndex = 0; mIndex < MaldonadoOpcodes.Count; mIndex++)
            {
                Application.DoEvents();
                if (mStop)
                {
                    ProcessStopped();
                    return;
                }

                founded = false;
                foreach (string s in FinalList)
                {
                    if (s.Contains("\"" + MaldonadoOpcodes[mIndex] + "\""))
                    {
                        founded = true;
                        break;
                    }
                }

                if (!founded)
                {
                    FinalList.Add(
                            "\"" + MaldonadoOpcodes[mIndex] + "\";" +
                            "\"CSoundAV Opcodes\";" +
                            "\"(Description not available)\";" +
                            "\"(Syntax not available)\"");
                }
            }


            progressBar1.Maximum = CSoundOpcodes.Count;
            for (Int32 mIndex = 0; mIndex < CSoundOpcodes.Count; mIndex++)
            {
                Application.DoEvents();
                if (mStop)
                {
                    ProcessStopped();
                    return;
                }
                progressBar1.Value = mIndex;

                founded = false;
                foreach (string s in FinalList)
                {
                    if (s.Contains("\"" + CSoundOpcodes[mIndex] + "\""))
                    {
                        founded = true;
                        break;
                    }
                }

                if (!founded)
                {
                    Name = "\"" + CSoundOpcodes[mIndex] + "\";";
                    Category = "\"Uncategorized\";";
                    Description = "\"(Description not available)\";";
                    Synopsis = "\"(Syntax not available)\"";
                    FinalList.Add(
                                Name +
                                Category +
                                Description +
                                Synopsis);
                }


            }



            File.WriteAllLines(
                    Application.StartupPath + "\\opcodes.txt",
                    FinalList.ToArray());


            ButtonStopCompiler.Enabled = false;
            buttonStart.Enabled = true;
            labelInfo.Text = "Finished!";

            MessageBox.Show("'opcodes.txt' database file created successfully!",
                            "WinXound Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);


            System.Diagnostics.Process.Start("notepad", Application.StartupPath + "\\opcodes.txt");
        }


        private List<string> PopulateList(XmlNodeList nodes)
        {
            //Int32 index = -1;
            string itemText = "";

            List<string> output = new List<string>();
            string Name = "";
            string Category = "";
            string Description = "";
            string Synopsis = "";

            string oldName = "";

            foreach (XmlNode parentItem in nodes[2].ChildNodes) //Opcodes node
            {
                //MenuItem mnuItemParent;
                itemText = parentItem.Attributes.Item(0).InnerText;//.Split(":".ToCharArray())[0];

                if (itemText.ToLower() == "utilities") continue;
                if (itemText == "Mathematical Operations:Arithmetic and Logic Operations") continue;
                if (itemText == "Instrument Control:Conditional Values") continue;

                //temp = "\"" + itemText + "\";";

                foreach (XmlNode OpcodeItemNode in parentItem.ChildNodes) //Opcode
                {
                    foreach (XmlNode ItemNode in OpcodeItemNode.ChildNodes) //Items
                    {
                        if (ItemNode.Name == "desc") continue;
                        if (ItemNode.HasChildNodes == false) continue;


                        //CATEGORY
                        Category = "\"" + itemText + "\";";
                        System.Diagnostics.Debug.WriteLine(Category);

                        //NAME
                        try
                        {
                            Name = "\"" + ItemNode["opcodename"].InnerText + "\";";

                            if (oldName == Name ||
                                ItemNode["opcodename"].InnerText == "=")
                            {
                                continue;
                            }
                            oldName = Name;
                        }
                        catch
                        {
                            continue;
                        }

                        //DESCRIPTION
                        Description = "\"" +
                                      ParseLine(OpcodeItemNode["desc"].InnerText)
                                      + "\";";

                        //SYNOPSIS
                        Synopsis = "\"" + 
                                   ParseLine(ItemNode.InnerText) 
                                   + "\"";


                        output.Add(Name +
                                   Category +
                                   Description +
                                   Synopsis
                                   );


                        //TO ADD MANUALLY (These are not present in opcodes.xml)
                        //Signal Generators:Basic Oscillators -> oscilx (same as osciln)
                        //Instrument Control:Sensing and Control -> sense (same as sensekey)
                        if (Name.Contains("osciln"))
                        {
                            Name = "\"oscilx\";";
                            Category = "\"Signal Generators:Basic Oscillators\";";
                            Description = "\"(same as osciln) Accesses table values at a user-defined frequency.\";";
                            Synopsis = "\"ares osciln kamp, ifrq, ifn, itimes\"";
                            output.Add(Name +
                                       Category +
                                       Description +
                                       Synopsis);
                        }
                        if (Name.Contains("sensekey"))
                        {
                            Name = "\"sense\";";
                            Category = "\"Instrument Control:Sensing and Control\";";
                            Description = "\"(same as sensekey) Returns the ASCII code of a key that has been pressed.\";";
                            Synopsis = "\"kres[, kkeydown] sensekey\"";
                            output.Add(Name +
                                       Category +
                                       Description +
                                       Synopsis);
                        }

                    }

                }
            }

            return output;

        }


        //Utility
        private string ParseLine(string Text)
        {
            try
            {
                //Delete all multispace chars
                Text = Regex.Replace(Text, @"(\s+)", " ");

                //Delete all "\" chars
                Text = Text.Replace(@"\", "");

                //Replace all ";" chars with ",". 
                //To avoid interferences with string split
                Text = Text.Replace(";", ",");

                //Text = Text.TrimEnd("\n\r".ToCharArray());
                return Text.Trim();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "ParseLine: " + ex.Message);
                return Text;
            }
        }

        private string RemoveChars(string mString, string OpcodeName)
        {

            char a = '\0';
            string tString = "";
            bool mTag = false;

            for (Int32 x = 0; x < mString.Length; x++)
            {
                a = Convert.ToChar(mString.Substring(x, 1));

                if (a == '<') mTag = true;
                if (a == '>') mTag = false;

                if ((int)a > 31 & (int)a < 127 &&
                    mTag == false && a != '>')
                {
                    tString += a;
                }
            }


            tString = RemoveSpaces(tString);
            tString = tString.Replace("\\", "");
            //tString = tString.Replace(" ", " ") 
            tString = tString.Replace(newline, " ");

            if (!string.IsNullOrEmpty(OpcodeName))
            {
                Int32 i = tString.IndexOf(OpcodeName);
                if (i > -1)
                {
                    i += OpcodeName.Length;
                    tString = tString.Substring(i);
                }
                //tString = "[" & OpcodeName & "] -" & tString.Substring(i) 
                tString = tString.Trim();
            }

            return tString;

        }

        private string RemoveSpaces(string mString)
        {

            string sString = "";
            string a = "";
            Int32 mCount = 0;

            for (Int32 x = 0; x < mString.Length; x++)
            {
                a = mString.Substring(x, 1);
                if (a == " ")
                {
                    mCount += 1;
                }
                else
                {
                    mCount = 0;
                }

                if (mCount < 2)
                {
                    sString += a;
                }
            }

            return sString;

        }


    }
}
