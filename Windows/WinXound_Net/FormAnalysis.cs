using System;
using System.IO;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormAnalysis : Form
    {

        string newline = System.Environment.NewLine;


        public FormAnalysis()
        {
            InitializeComponent();
            this.Name = "FormAnalysis";
        }


        private void FormAnalysis_Load(object sender, EventArgs e)
        {
            WxUtilityConsole1.ButtonFindError.Visible = false;

            this.Text = "CSound Analysis";

            try
            {
                string mFile = Application.StartupPath + "\\Help\\atsa.html";
                if (File.Exists(mFile))
                {
                    WebBrowserHelp.Navigate(mFile);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form Analysis - FormAnalysis_Load");
            }

            //Prepare data tabs 
            SetDefaultValues("ATSA");
            SetDefaultValues("CVANAL");
            SetDefaultValues("HETRO");
            SetDefaultValues("LPANAL");
            SetDefaultValues("PVANAL");


            WxUtilityConsole1.ButtonFindError.Visible = false;
            WxUtilityConsole1.OnCompilerCompleted +=
                new wxCompilerConsole.CompilerCompleted(WxUtilityConsole1_OnCompilerCompleted);
            WxUtilityConsole1.OnButtonStopBatch +=
                new EventHandler(buttonStopBatch_Click);
        }



        private void SetDefaultValues(string mUtilityName)
        {

            switch (mUtilityName)
            {

                case "ATSA":
                    b_Atsa.Value = 0;
                    e_Atsa.Value = 0;
                    l_Atsa.Value = 20;
                    Max_Atsa.Value = 20000;
                    d_Atsa.Value = (decimal)0.1;
                    c_Atsa.Value = 4;
                    w_Atsa.SelectedIndex = 1;
                    F_File_Atsa.SelectedIndex = 3;
                    h_Atsa.Value = (decimal)0.25;
                    m_Atsa.Value = -60;
                    t_Atsa.Value = 3;
                    s_Atsa.Value = 3;
                    g_Atsa.Value = 3;
                    T_SMR_Atsa.Value = 30;
                    S_SMR_Atsa.Value = 60;
                    P_Peak_Atsa.Value = 0;
                    M_SMR_Atsa.Value = (decimal)0.5;
                    break;

                case "CVANAL":
                    s_Cvanal.Value = 10000;
                    c_Cvanal.Value = 0;
                    b_Cvanal.Value = 0;
                    d_Cvanal.Value = 0;
                    break;

                case "HETRO":
                    s_Hetro.Value = 10000;
                    c_Hetro.Value = 1;
                    b_Hetro.Value = 0;
                    d_Hetro.Value = 0;
                    f_Hetro.Value = 100;
                    Max_Hetro.Value = 32767;
                    n_Hetro.Value = 256;
                    h_Hetro.Value = 10;
                    min_Hetro.Value = 64;
                    l_Hetro.Value = 0;
                    checkBoxSDIF.Checked = false;
                    break;

                case "LPANAL":
                    s_Lpanal.Value = 10000;
                    c_Lpanal.Value = 1;
                    b_Lpanal.Value = 0;
                    d_Lpanal.Value = 0;
                    p_Lpanal.Value = 34;
                    Min_Lpanal.Value = 70;
                    Max_Lpanal.Value = 200;
                    h_Lpanal.Value = 200;
                    v_Lpanal.SelectedIndex = 0;
                    a_Lpanal.Checked = false;
                    Comments_Lpanal.Text = "";
                    break;

                case "PVANAL":
                    s_Pvanal.Value = 10000;
                    c_Pvanal.Value = 1;
                    b_Pvanal.Value = 0;
                    d_Pvanal.Value = 0;
                    n_Pvanal.Text = "2048";
                    w_Pvanal.SelectedIndex = 0;
                    h_Pvanal.Text = "128";
                    window_Pvanal.Text = "Von Hann";
                    checkBoxPvocEx.Checked = false;
                    break;

            }

        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            switch (TabControl1.SelectedTab.Name.Substring(3))
            {
                case "Atsa":
                    SetDefaultValues("ATSA");
                    break;
                case "Cvanal":
                    SetDefaultValues("CVANAL");
                    break;
                case "Hetro":
                    SetDefaultValues("HETRO");
                    break;
                case "Lpanal":
                    SetDefaultValues("LPANAL");
                    break;
                case "Pvanal":
                    SetDefaultValues("PVANAL");
                    break;

                default:
                    break;
            }
        }


        private void ButtonStart_Click(object sender, EventArgs e)
        {

            //if (TabControl1.SelectedTab.Name == "TB_Help") return;

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

            StartCompiler();

        }



        private string ATSA()
        {
            string tFlags = "";

            if (b_Atsa.Value > 0)
            {
                tFlags += "-b" + b_Atsa.Value + " ";
            }
            if (e_Atsa.Value > 0)
            {
                tFlags += "-e" + e_Atsa.Value + " ";
            }
            if (l_Atsa.Value != 20)
            {
                tFlags += "-l" + l_Atsa.Value + " ";
            }
            if (Max_Atsa.Value != 20000)
            {
                tFlags += "-H" + Max_Atsa.Value + " ";
            }
            if ((double)d_Atsa.Value != 0.1)
            {
                tFlags += "-d" + d_Atsa.Value + " ";
            }
            if (c_Atsa.Value != 4)
            {
                tFlags += "-c" + c_Atsa.Value + " ";
            }
            if (w_Atsa.SelectedIndex != 1)
            {
                tFlags += "-w" + w_Atsa.SelectedIndex + " ";
            }
            if ((double)h_Atsa.Value != 0.25)
            {
                tFlags += "-h" + h_Atsa.Value + " ";
            }
            if (m_Atsa.Value != -60)
            {
                tFlags += "-m" + m_Atsa.Value + " ";
            }
            if (t_Atsa.Value != 3)
            {
                tFlags += "-t" + t_Atsa.Value + " ";
            }
            if (s_Atsa.Value != 3)
            {
                tFlags += "-s" + s_Atsa.Value + " ";
            }
            if (g_Atsa.Value != 3)
            {
                tFlags += "-g" + g_Atsa.Value + " ";
            }
            if (T_SMR_Atsa.Value != 30)
            {
                tFlags += "-T" + T_SMR_Atsa.Value + " ";
            }
            if (S_SMR_Atsa.Value != 60)
            {
                tFlags += "-S" + S_SMR_Atsa.Value + " ";
            }
            if (P_Peak_Atsa.Value > 0)
            {
                tFlags += "-P" + P_Peak_Atsa.Value + " ";
            }
            if ((double)M_SMR_Atsa.Value != 0.5)
            {
                tFlags += "-M" + M_SMR_Atsa.Value + " ";
            }
            if (F_File_Atsa.SelectedIndex != 3)
            {
                tFlags += "-F" + F_File_Atsa.SelectedIndex + 1 + " ";
            }

            return tFlags;
        }


        private string CVANAL()
        {
            string tFlags = "";

            if (s_Cvanal.Value != 10000)
            {
                tFlags += "-s" + s_Cvanal.Value + " ";
            }
            if (c_Cvanal.Value > 0)
            {
                tFlags += "-c" + c_Cvanal.Value + " ";
            }
            if (b_Cvanal.Value > 0)
            {
                tFlags += "-b" + b_Cvanal.Value + " ";
            }
            if (d_Cvanal.Value > 0)
            {
                tFlags += "-d" + d_Cvanal.Value + " ";
            }

            return tFlags;
        }


        private string HETRO()
        {
            string tFlags = "";

            if (s_Hetro.Value != 10000)
            {
                tFlags += "-s" + s_Hetro.Value + " ";
            }
            if (c_Hetro.Value != 1)
            {
                tFlags += "-c" + c_Hetro.Value + " ";
            }
            if (b_Hetro.Value > 0)
            {
                tFlags += "-b" + b_Hetro.Value + " ";
            }
            if (d_Hetro.Value > 0)
            {
                tFlags += "-d" + d_Hetro.Value + " ";
            }

            if (f_Hetro.Value != 100)
            {
                tFlags += "-f" + f_Hetro.Value + " ";
            }
            if (h_Hetro.Value != 10)
            {
                tFlags += "-h" + h_Hetro.Value + " ";
            }
            if (Max_Hetro.Value != 32767)
            {
                tFlags += "-M" + Max_Hetro.Value + " ";
            }
            if (min_Hetro.Value != 64)
            {
                tFlags += "-m" + min_Hetro.Value + " ";
            }
            if (n_Hetro.Value != 256)
            {
                tFlags += "-n" + n_Hetro.Value + " ";
            }
            if (l_Hetro.Value > 0)
            {
                tFlags += "-l" + l_Hetro.Value + " ";
            }

            return tFlags;
        }


        private string LPANAL()
        {
            string tFlags = "";

            if (s_Lpanal.Value != 10000)
            {
                tFlags += "-s" + s_Lpanal.Value + " ";
            }
            if (c_Lpanal.Value != 1)
            {
                tFlags += "-c" + c_Lpanal.Value + " ";
            }
            if (b_Lpanal.Value > 0)
            {
                tFlags += "-b" + b_Lpanal.Value + " ";
            }
            if (d_Lpanal.Value > 0)
            {
                tFlags += "-d" + d_Lpanal.Value + " ";
            }

            if (p_Lpanal.Value != 34)
            {
                tFlags += "-p" + p_Lpanal.Value + " ";
            }
            if (h_Lpanal.Value != 200)
            {
                tFlags += "-h" + h_Lpanal.Value + " ";
            }
            if (Max_Lpanal.Value != 200)
            {
                tFlags += "-Q" + Max_Lpanal.Value + " ";
            }
            if (Min_Lpanal.Value != 70)
            {
                tFlags += "-P" + Min_Lpanal.Value + " ";
            }
            if (a_Lpanal.Checked)
            {
                tFlags += "-a ";
            }
            if (!v_Lpanal.Text.Contains("none"))
            {
                tFlags += "-v" + v_Lpanal.SelectedIndex + " ";
            }
            if (Comments_Lpanal.Text.Length > 0)
            {
                tFlags += "-C" + Comments_Lpanal.Text + " ";
            }

            return tFlags;
        }


        private string PVANAL()
        {
            string tFlags = "";

            if (s_Pvanal.Value != 10000)
            {
                tFlags += "-s" + s_Pvanal.Value + " ";
            }
            if (c_Pvanal.Value != 1)
            {
                tFlags += "-c" + c_Pvanal.Value + " ";
            }
            if (b_Pvanal.Value > 0)
            {
                tFlags += "-b" + b_Pvanal.Value + " ";
            }
            if (d_Pvanal.Value > 0)
            {
                tFlags += "-d" + d_Pvanal.Value + " ";
            }

            tFlags += "-n" + n_Pvanal.Text + " ";

            if (w_Pvanal.Text.Contains("Hop"))
            {
                tFlags += "-h" + h_Pvanal.Text + " ";
            }
            else if (w_Pvanal.Text != "4")
            {
                tFlags += "-w" + w_Pvanal.Text + " ";
            }

            if (!window_Pvanal.Text.Contains("Hann"))
            {
                if (window_Pvanal.Text.Contains("Hamming"))
                {
                    tFlags += "-H ";
                }
                else
                {
                    tFlags += "-K ";
                }
            }

            return tFlags;
        }




        private void StartCompiler()//string arguments)
        {

            ComboBox input = InputFile; // GetInput();
            TextBox output = OutputPath; // GetOutput();

            if (input != null)
            {
                if (string.IsNullOrEmpty(input.Text))
                {
                    wxGlobal.wxMessageError("Input file not specified", "Error");
                    return;
                }
            }
            if (output != null)
            {
                if (string.IsNullOrEmpty(output.Text))
                {
                    wxGlobal.wxMessageError("Output path not specified", "Error");
                    return;
                }
            }


            //Select the current index of input text
            input.SelectedIndex = mCurrentIndex;


            //CHECK FOR OUTPUT DIRECTORY PERMISSION
            FileAccessRights rights = new FileAccessRights(output.Text);
            if (!rights.canWrite())
            {
                MessageBox.Show(
                    "Your current user account allows read-only access to the selected 'Output' directory." + newline +
                    "Please select a valid 'Output path' where you have full write permissions.",
                    "Output path - Write access denied!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }




            string flags = "";
            //arguments = " -U atsa ";
            string arguments = "[ANALYSIS] -U " +
                               TabControl1.SelectedTab.Name.Substring(3).ToLower() +
                               " ";
            switch (TabControl1.SelectedTab.Name.Substring(3))
            {
                case "Atsa":
                    flags = ATSA();
                    break;
                case "Cvanal":
                    flags = CVANAL();
                    break;
                case "Hetro":
                    flags = HETRO();
                    break;
                case "Lpanal":
                    flags = LPANAL();
                    break;
                case "Pvanal":
                    flags = PVANAL();
                    break;
            }

            arguments += flags + "\"" + input.Text.Trim() + "\"";

            if (output != null && TabControl1.SelectedIndex < 5)
            {
                arguments += " " + "\"" + output.Text.Trim();

                string filename = Path.GetFileName(input.Text.Trim());
                string o = "";
                if (filename.Contains("."))
                {
                    o = filename.Substring(0, filename.LastIndexOf("."));
                }
                else
                {
                    o = filename;
                }

                //Output filename extension (check for pvanal and hetro tools)
                if (TabControl1.SelectedTab.Name.Substring(3, 3).ToLower() == "pva")
                {
                    if (checkBoxPvocEx.Checked) //Extended format (pvoc-ex)
                        o += ".pvx";
                    else
                        o += ".pv"; //Simple format

                }
                else if (TabControl1.SelectedTab.Name.Substring(3, 3).ToLower() == "het" &&
                         checkBoxSDIF.Checked) //Check for sdif extension
                {
                    o += ".sdif";
                }
                else
                {
                    o += "." + TabControl1.SelectedTab.Name.Substring(3, 3).ToLower();
                }


                if (!output.Text.Trim().EndsWith(@"\"))
                {
                    arguments += "\\";
                }
                arguments += o + "\"";
            }


            System.Diagnostics.Debug.WriteLine(arguments);



            //CHECK CONTROLS APPEARANCE
            TabControl2.SelectedIndex = 1; //Select compiler tab
            TabControl1.Enabled = false;
            InputFile.Enabled = false;
            OutputPath.Enabled = false;
            ButtonInput.Enabled = false;
            ButtonOutput.Enabled = false;
            buttonClearInputField.Enabled = false;
            ButtonReset.Enabled = false;

            ButtonStart.Enabled = false;
            if (input.Items.Count > 1)
            {
                mStopBatch = false;
                WxUtilityConsole1.buttonStopBatch.Text = "Stop batch process";
                WxUtilityConsole1.buttonStopBatch.Visible = true;
                WxUtilityConsole1.Title =
                    string.Format("Batch processing info:  {0}  [{1} of {2}]",
                                  input.SelectedItem.ToString(),
                                  mCurrentIndex + 1,
                                  input.Items.Count);
            }

            //START COMPILER
            //Replace "," with "." inside arguments string 
            arguments = arguments.Replace(",", ".");
            //Call csound compiler [Utility -U]
            //Example: " -U cvanal [FLAGS] inputfile outputfile" 
            WxUtilityConsole1.Filename = wxGlobal.Settings.Directory.CSoundConsole;
            WxUtilityConsole1.Arguments = arguments;

            //If we use batch file, we pass false to ClearPreviousText parameter
            //to compiler to preserve the old content
            WxUtilityConsole1.StartCompiler(true, mCurrentIndex < 1);

        }



        private Int32 mCurrentIndex = 0;
        private bool mStopBatch = true;

        void WxUtilityConsole1_OnCompilerCompleted(string mErrorLine, string mWaveFile)
        {

            ComboBox input = InputFile; //GetInput();
            if (input == null) return;

            mCurrentIndex++;

            if (mCurrentIndex < input.Items.Count && mStopBatch == false)
            {
                //buttonStopBatch.Visible = true;
                input.SelectedIndex = mCurrentIndex;
                StartCompiler();
                return;
            }
            else
            {
                mStopBatch = true;
                mCurrentIndex = 0;
                TabControl1.Enabled = true;
                InputFile.Enabled = true;
                OutputPath.Enabled = true;
                ButtonStart.Enabled = true;
                ButtonInput.Enabled = true;
                ButtonOutput.Enabled = true;
                buttonClearInputField.Enabled = true;
                ButtonReset.Enabled = true;

                WxUtilityConsole1.buttonStopBatch.Visible = false;
            }
        }

        private void buttonStopBatch_Click(object sender, EventArgs e)
        {
            WxUtilityConsole1.buttonStopBatch.Text = "Stopping batch (Wait current) ...";
            //WxUtilityConsole1.ButtonStopCompiler.PerformClick();
            mStopBatch = true;
        }






        //INPUT BUTTONS
        private void ButtonInput_Click(object sender, EventArgs e)
        {
            ComboBox input = InputFile; //GetInput();
            TextBox output = OutputPath; //GetOutput();
            //if (input == null || output == null) return;


            //Notify user that there are other files into the list
            if (input.Items.Count > 0)
            {
                DialogResult r =
                    MessageBox.Show("There are one or more files in the input list." + newline +
                                    "Do you want to keep them (new files will be added)?",
                                    "WinXound",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                if (r == DialogResult.Cancel) return;
                else if (r == DialogResult.No)
                {
                    input.Items.Clear();
                    output.Clear();
                }
            }


            string[] ret = InputFileDialog(true);
            if (ret == null) return;

            // Add file(s) to list
            string o = "";
            foreach (string s in ret)
            {
                input.Items.Add(s);
                o = Directory.GetParent(s).ToString();
                //output.Items.Add(o);
            }

            input.SelectedIndex = 0;

            if (output.Text.Length == 0)
            {
                output.Text = o;
            }

        }


        /*
        private void ButtonInputSndinfo_Click(object sender, EventArgs e)
        {
            string[] ret = InputFileDialog(false);
            if (ret == null) return;

            if (string.IsNullOrEmpty(ret[0])) return;

            InputSndinfo.Text = ret[0];
        }
        */


        //OUTPUT BUTTONS
        private void ButtonOutput_Click(object sender, EventArgs e)
        {
            //string s = OutputFileDialog(GetOutput().Text);
            //GetOutput().Text = s;

            string s = OutputFileDialog(OutputPath.Text);
            OutputPath.Text = s;

        }


        private string[] InputFileDialog(bool multiselection)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.Multiselect = multiselection;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                return null;
            }

            return openFileDialog1.FileNames;

        }

        private string OutputFileDialog(string mFileName)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.SelectedPath = mFileName;
            if (fbd.ShowDialog() != DialogResult.OK ||
                string.IsNullOrEmpty(fbd.SelectedPath))
            {
                return mFileName;
            }

            return fbd.SelectedPath;
        }


        private void FormAnalysis_KeyDown(object sender, KeyEventArgs e)
        {
            if (TabControl1.Enabled == false) return;

            if (e.KeyCode == Keys.Escape)
            {
                if (WxUtilityConsole1.ProcessActive)
                {
                    WxUtilityConsole1.StopCompiler();
                }
                else
                {
                    this.Close();
                }
            }

            switch (e.KeyCode)
            {
                case Keys.F1:
                    TabControl1.SelectedIndex = 0;
                    break;

                case Keys.F2:
                    TabControl1.SelectedIndex = 1;
                    break;

                case Keys.F3:
                    TabControl1.SelectedIndex = 2;
                    break;

                case Keys.F4:
                    TabControl1.SelectedIndex = 3;
                    break;

                case Keys.F5:
                    TabControl1.SelectedIndex = 4;
                    break;

                case Keys.F6:
                    TabControl1.SelectedIndex = 5;
                    break;

            }
        }

        private void WxUtilityConsole1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                FormAnalysis_KeyDown(this, e);
            }
        }

        private void WebBrowserHelp_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (TabControl1.Enabled == false) return;

            this.Focus();
            FormAnalysis_KeyDown(this, new KeyEventArgs(e.KeyData));
            //if (e.KeyCode == Keys.F5)
            //{
            //    TabControl1.SelectedIndex = 4;
            //}
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabControl1.Enabled == false) return;

            if (TabControl1.SelectedIndex == 5) //SndInfo
            {
                OutputPath.Enabled = false;
                ButtonOutput.Enabled = false;
                ButtonReset.Enabled = false;
            }
            else
            {
                OutputPath.Enabled = true;
                ButtonOutput.Enabled = true;
                ButtonReset.Enabled = true;
            }

            TabControl2.SelectedIndex = 0;

            string UtilityName = TabControl1.SelectedTab.Name.Substring(3);

            try
            {
                string mFile = Application.StartupPath + "\\Help\\" +
                               UtilityName + ".html";
                if (File.Exists(mFile))
                {
                    WebBrowserHelp.Navigate(mFile);
                }
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                    "Form Analysis - TabControl1_SelectedIndexChanged");
            }

        }

        private void buttonClearInputField_Click(object sender, EventArgs e)
        {
            InputFile.Items.Clear();
            InputFile.Text = "";
        }





        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        //OLD REMMED STUFFS!!!
        //OLD: TO REMOVE!
        /*
        private ComboBox GetInput()
        {
            //TB_Atsa
            //Search for Input combobox
            foreach (Control c in TabControl1.SelectedTab.Controls)
            {
                System.Diagnostics.Debug.WriteLine(c.Name);

                if (c.GetType() == typeof(ComboBox))
                {
                    if (c.Name ==
                        "Input" + TabControl1.SelectedTab.Name.Substring(3))
                    {
                        return (ComboBox)c;
                    }
                }
            }
            return null;
        }
        private TextBox GetOutput()
        {
            //TB_Atsa
            //Search for Output combobox
            foreach (Control c in TabControl1.SelectedTab.Controls)
            {
                System.Diagnostics.Debug.WriteLine(c.Name);

                if (c.GetType() == typeof(TextBox))
                {
                    if (c.Name ==
                        "Output" + TabControl1.SelectedTab.Name.Substring(3))
                    {
                        return (TextBox)c;
                    }
                }
            }
            return null;
        }
        */


    }

}
