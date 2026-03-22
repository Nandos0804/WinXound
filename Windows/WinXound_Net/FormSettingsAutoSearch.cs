using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WinXound_Net
{
    public partial class FormSettingsAutoSearch : Form
    {

        public delegate void OnReturnValue(object sender, string[] ReturnValue);
        public event OnReturnValue ReturnValueEv;

        private List<string> csound_paths = new List<string>();
        private List<string> csound_manual_paths = new List<string>();
        private List<string> csound_gui_paths = new List<string>();
        private List<string> python_paths = new List<string>();
        private List<string> python_idle_paths = new List<string>();
        private List<string> lua_paths = new List<string>();


        public FormSettingsAutoSearch()
        {
            InitializeComponent();
        }

        private void FormSettingsAutoSearch_Load(object sender, EventArgs e)
        {
            this.Show();

            AutoSearchCSound();
            AutoSearchPython();
            AutoSearchLua();

            try
            {
                foreach (string s in csound_paths)
                {
                    clbCSound.Items.Add(s);
                    clbCSound.Enabled = true;
                    clbCSound.SetItemChecked(0, true);
                    labelCSoundExe.Text = "CSound exe: (found)";
                    labelCSoundExe.ForeColor = Color.DarkGreen;
                }
                foreach (string s in csound_manual_paths)
                {
                    clbCSoundManual.Items.Add(s);
                    clbCSoundManual.Enabled = true;
                    clbCSoundManual.SetItemChecked(0, true);
                    labelCSoundManual.Text = "CSound Manual: (found)";
                    labelCSoundManual.ForeColor = Color.DarkGreen;
                }
                foreach (string s in csound_gui_paths)
                {
                    clbCSoundGui.Items.Add(s);
                    clbCSoundGui.Enabled = true;
                    clbCSoundGui.SetItemChecked(0, true);
                    labelCSoundGui.Text = "CSound GUI: (found)";
                    labelCSoundGui.ForeColor = Color.DarkGreen;
                }
                foreach (string s in python_paths)
                {
                    clbPython.Items.Add(s);
                    clbPython.Enabled = true;
                    clbPython.SetItemChecked(0, true);
                    labelPythonExe.Text = "Python exe: (found)";
                    labelPythonExe.ForeColor = Color.DarkGreen;
                }
                foreach (string s in python_idle_paths)
                {
                    clbPythonIdle.Items.Add(s);
                    clbPythonIdle.Enabled = true;
                    clbPythonIdle.SetItemChecked(0, true);
                    labelPythonIdle.Text = "Python Idle: (found)";
                    labelPythonIdle.ForeColor = Color.DarkGreen;
                }
                foreach (string s in lua_paths)
                {
                    clbLua.Items.Add(s);
                    clbLua.Enabled = true;
                    clbLua.SetItemChecked(0, true);
                    labelLuaExe.Text = "Lua exe: (found)";
                    labelLuaExe.ForeColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormSettingsAutoSearch_Load: " + ex.Message);
            }

        }


        private void AutoSearchCSound()
        {
            //We try to retrieve the CSound path from [CSOUNDRC] and search for executables and manuals
            string mPath = "";
            string CSoundPath = "";

            try
            {
                mPath = System.Environment.GetEnvironmentVariable("CSOUNDRC");

                if (string.IsNullOrEmpty(mPath))
                {
                    mPath = System.Environment.GetEnvironmentVariable("RAWWAVE_PATH");
                    mPath = Path.GetDirectoryName(mPath);
                }

                if (string.IsNullOrEmpty(mPath)) //PATH NOT FOUND
                {
                    mPath = wxGlobal.GetProgramsFolder32();
                    if (!string.IsNullOrEmpty(mPath))
                    {
                        CSoundPath = mPath + "\\Csound";
                        if (!Directory.Exists(CSoundPath))
                        {
                            mPath = wxGlobal.GetProgramsFolder64();
                            if (!string.IsNullOrEmpty(mPath))
                            {
                                CSoundPath = mPath + "\\Csound";
                                if (!Directory.Exists(CSoundPath))
                                {
                                    CSoundPath = "C:\\Csound";
                                }
                            }
                        }
                    }
                }
                else //PATH FOUND
                    CSoundPath = Directory.GetParent(mPath).ToString();




                if (File.Exists(CSoundPath + "\\bin\\csound.exe"))
                {
                    csound_paths.Add(
                        CSoundPath + "\\bin\\csound.exe");
                }
                if (File.Exists(CSoundPath + "\\doc\\manual\\index.html"))
                {
                    csound_manual_paths.Add(
                        CSoundPath + "\\doc\\manual\\index.html");
                }

                //CSound5gui and QuteCSound
                if (File.Exists(CSoundPath + "\\bin\\csound5gui.exe"))
                {
                    csound_gui_paths.Add(
                        CSoundPath + "\\bin\\csound5gui.exe");
                }
                if (File.Exists(CSoundPath + "\\bin\\qutecsound.exe"))
                {
                    csound_gui_paths.Add(
                        CSoundPath + "\\bin\\qutecsound.exe");
                }
                if (File.Exists(CSoundPath + "\\bin\\qutecsoundf.exe"))
                {
                    csound_gui_paths.Add(
                        CSoundPath + "\\bin\\qutecsoundf.exe");
                }
                if (File.Exists(CSoundPath + "\\bin\\qutecsound-d.exe"))
                {
                    csound_gui_paths.Add(
                        CSoundPath + "\\bin\\qutecsound-d.exe");
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

        private void AutoSearchPython()
        {
            //We try to retrieve the Python path and search for executables

            string mPath = "";
            string PythonPath = "";

            try
            {
                mPath = System.Environment.GetEnvironmentVariable("PYTHONPATH");
                if (mPath == null) //PYTHONPATH DOES NOT EXIST
                {
                    mPath = System.Environment.GetEnvironmentVariable("Path");
                    if (mPath == null) //Path DOES NOT EXIST
                        return;
                }
                else //PYTHONPATH EXIST
                {
                    if (!mPath.ToLower().Contains(@"\python"))
                    {
                        mPath = System.Environment.GetEnvironmentVariable("Path");
                        if (mPath == null) //Path DOES NOT EXIST
                            return;
                    }
                }

                //Check if mPath contains some python paths
                if (mPath.ToLower().Contains(@"\python"))
                {
                    char[] delimiter = ";".ToCharArray();
                    string[] split = null;
                    split = mPath.Split(delimiter);
                    if (split == null) return;

                    foreach (string s in split)
                    {
                        if (s.ToLower().Contains("python"))
                        {
                            if (wxGlobal.CountStringOccurrences(s, "\\") == 1)
                            {
                                PythonPath = s.Trim();
                                break;
                            }
                        }
                    }
                }
                
                else //Look in root path
                {
                    //Retrieve System Folder
                    string temp = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    string root = Directory.GetDirectoryRoot(temp);

                    foreach (string path in Directory.GetDirectories(root))
                    {
                        //System.Diagnostics.Debug.WriteLine(path);
                        if (path.ToLower().Contains("python"))
                        {
                            PythonPath = path.Trim();
                            break;
                        }
                    }
                }
                
                if (File.Exists(PythonPath + "\\python.exe"))
                {
                    python_paths.Add(
                        PythonPath + "\\python.exe");
                }

                if (File.Exists(PythonPath + "\\Lib\\idlelib\\idle.pyw"))
                {
                    python_idle_paths.Add(
                        PythonPath + "\\Lib\\idlelib\\idle.pyw");
                }

            }

            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, 
                //    "FormSettingsAutoSearch - AutoSearchPython");
                System.Diagnostics.Debug.WriteLine(
                    "FormSettingsAutoSearch - AutoSearchPython: " + ex.Message);
            }

        }

        private void AutoSearchLua()
        {
            //We try to retrieve the CSound path from [CSOUNDRC] and 
            //search for bin directory with luajit.exe
            string mPath = "";
            string luapath = "";

            try
            {
                mPath = System.Environment.GetEnvironmentVariable("CSOUNDRC");

                if (string.IsNullOrEmpty(mPath))
                {
                    mPath = System.Environment.GetEnvironmentVariable("RAWWAVE_PATH");
                    mPath = Path.GetDirectoryName(mPath);
                    //mPath = Path.GetDirectoryName(mPath);
                }

                if (mPath == null) return;

                //if (File.Exists(mPath))
                if (Directory.Exists(mPath))
                {
                    luapath = Directory.GetParent(mPath).ToString();
                    if (File.Exists(luapath + "\\bin\\luajit.exe"))
                    {
                        lua_paths.Add(
                            luapath + "\\bin\\luajit.exe");
                    }
                }
            }

            catch (Exception ex)
            {
                //wxGlobal.wxMessageError(ex.Message, 
                //    "FormSettingsAutoSearch - AutoSearchLua");
                System.Diagnostics.Debug.WriteLine(
                    "FormSettingsAutoSearch - AutoSearchLua: " + ex.Message);
            }
        }


        private void buttonApply_Click(object sender, EventArgs e)
        {
            List<string> finalpaths = new List<string>();

            if (clbCSound.CheckedItems.Count > 0)
                finalpaths.Add(
                    "csound.exe|" + clbCSound.CheckedItems[0].ToString());

            if (clbCSoundManual.CheckedItems.Count > 0)
                finalpaths.Add(
                    "csound.manual|" + clbCSoundManual.CheckedItems[0].ToString());

            if (clbCSoundGui.CheckedItems.Count > 0)
                finalpaths.Add(
                    "csound.gui|" + clbCSoundGui.CheckedItems[0].ToString());

            if (clbPython.CheckedItems.Count > 0)
                finalpaths.Add(
                    "python.exe|" + clbPython.CheckedItems[0].ToString());

            if (clbPythonIdle.CheckedItems.Count > 0)
                finalpaths.Add(
                    "python.idle|" + clbPythonIdle.CheckedItems[0].ToString());

            if (clbLua.CheckedItems.Count > 0)
                finalpaths.Add(
                    "lua.exe|" + clbLua.CheckedItems[0].ToString());

            if (ReturnValueEv != null) ReturnValueEv(this, finalpaths.ToArray());

            ////For debug
            //foreach (string s in finalpaths)
            //{
            //    System.Diagnostics.Debug.WriteLine(s);
            //}

            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void FormSettingsAutoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) buttonCancel.PerformClick();
        }

        private void clb_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                CheckedListBox clb = (sender as CheckedListBox);
                for (Int32 index = 0; index < clb.Items.Count; index++)
                {
                    if (e.Index != index)
                        clb.SetItemChecked(index, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FormSettingsAutoSearch.clb_ItemCheck Error: " +
                    ex.Message);
            }
        }



    }
}
