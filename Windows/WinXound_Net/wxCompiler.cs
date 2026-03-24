using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace WinXound_Net
{
    class wxCompiler
    {

        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        //''WINSOUND COMPILER
        public void WinsoundCompiler(string FileName, string CompilerPath)
        {
            try
            {
                //Check for another running CSoundAV executable 
                foreach (Process p in Process.GetProcesses())
                {

                    if (p.ProcessName.ToLower().Contains("csound5gui"))
                    {
                        MessageBox.Show(@"It seems that there is another running 
                                    instance of CSound5GUI\n Please close it 
                                    before to run CSound5GUI compiler",
                                        "WinXound Info",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }
                }


                Process mtProc = new Process();
                mtProc.StartInfo.FileName = CompilerPath; //mGlobalSettings.Directory.Winsound;
                mtProc.StartInfo.Arguments = "\"" + FileName + "\"";
                //Proc.StartInfo.UseShellExecute = False 
                //Proc.StartInfo.RedirectStandardOutput = True 
                //Proc.StartInfo.CreateNoWindow = True 
                //Proc.EnableRaisingEvents = True 
                mtProc.Start();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCompiler - WinsoundCompiler");
            }

        }


        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        //'CSOUNDAV COMPILER 
        public void CSoundAVCompiler(string FileName, string CompilerPath)
        {
            try
            {
                //Check for another running CSoundAV executable 
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.ProcessName.ToLower().Contains("csoundav_win"))
                    {
                        MessageBox.Show(@"It seems that there is another running 
                                    instance of CSoundAV \n Please close it 
                                    before to run CSoundAV compiler",
                                        "WinXound Info",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }
                }


                Process mtProc = new Process();
                mtProc.StartInfo.FileName = CompilerPath; // mGlobalSettings.Directory.CSoundAV;
                mtProc.StartInfo.Arguments = "\"" + FileName + "\"";
                //Proc.StartInfo.UseShellExecute = False 
                //Proc.StartInfo.RedirectStandardOutput = True 
                //Proc.StartInfo.CreateNoWindow = True 
                //Proc.EnableRaisingEvents = True 
                mtProc.Start();
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCompiler - CSoundAVCompiler");
            }
        }



        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        //'EXTERNAL COMPILER (For CSound and also Python)
        public void ExternalCompiler(string FileName,
                                     string CompilerPath,
                                     string Arguments)
        {
            ExternalCompiler(FileName, "", CompilerPath, Arguments);
        }

        public void ExternalCompiler(string FileName1,
                                     string FileName2,
                                     string CompilerPath,
                                     string Arguments)
        {
            try
            {
                ////Check for another running Python executable 
                //foreach (Process p in Process.GetProcesses())
                //{
                //    if (p.ProcessName.ToLower().Contains("python.exe"))
                //    {
                //        MessageBox.Show(@"It seems that there is another running 
                //                    instance of Python \n Please close it 
                //                    before to run CSoundAV compiler",
                //                        "WinXound Info",
                //                        MessageBoxButtons.OK,
                //                        MessageBoxIcon.Information);
                //        return;
                //    }
                //}


                //if (!string.IsNullOrEmpty(mSettings.Directory.PythonConsolePath))
                //    System.Environment.SetEnvironmentVariable("Path",
                //        System.Environment.GetEnvironmentVariable("Path").TrimEnd(";".ToCharArray()) +
                //        ";" + mSettings.Directory.PythonConsolePath + ";");



                ////& = Use to separate multiple commands on one command line
                ////&& = Use to run the command following && only if the command 
                ////preceding the symbol is successful. 
                ////Cmd.exe runs the first command, and then runs the second command 
                ////only if the first command completed successfully.
                // Inject OPCODEDIR and OPCODEDIR64 into the external terminal session.
                string envSetup = "";
                string opcodedir = wxGlobal.Settings.Directory.OPCODEDIR;
                if (!string.IsNullOrEmpty(opcodedir))
                {
                    envSetup = "SET \"OPCODE7DIR=" + opcodedir + "\" && " +
                               "SET \"OPCODE7DIR64=" + opcodedir + "\" && ";
                }

                Process mtProc = new Process();
                mtProc.StartInfo.FileName = "cmd.exe";
                //With Python send temp files to WorkingDirectory
                mtProc.StartInfo.WorkingDirectory = Directory.GetParent(FileName1).ToString();
                mtProc.StartInfo.Arguments = "/K cls && " + envSetup +
                                             "\"" + CompilerPath + "\" " +
                                             Arguments + " " +
                                             "\"" + FileName1 + "\""; 

                if (!string.IsNullOrEmpty(FileName2))
                {
                    mtProc.StartInfo.Arguments += " \"" + FileName2 + "\"";
                }

                System.Diagnostics.Debug.WriteLine(mtProc.StartInfo.Arguments);

                mtProc.Start();


                //System.Diagnostics.Process.Start("cmd.exe", @"/K cls && " +
                //                                  "\"" + CompilerPath + "\" " +
                //                                  Arguments + " " +
                //                                  "\"" + FileName + "\"");


            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxCompiler - ExternalCompiler");
            }
        }




    }




}
