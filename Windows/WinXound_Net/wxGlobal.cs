using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;


namespace WinXound_Net
{
    class wxGlobal
    {
        //////////////////////////////////////////////////////////////////
        //THIS CLASS STORE INFORMATION GLOBALLY FOR ALL WINXOUND CLASSES//
        //////////////////////////////////////////////////////////////////

        /////////////////////////
        //WINXOUND VERSION
        //Version.Major & "." & _
        //Version.Minor & "." & _
        //Version.Build
        public static string WINXOUND_VERSION =
            Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
            Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." +
            Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();

        public static string TITLE = "WinXound.Net " + WINXOUND_VERSION;


        /////////////////////////
        //SCINTILLA VERSION
        //Useful method to retrieve the current version of Scintilla library
        public static string GetScintillaVersion()
        {
            string filename = Application.StartupPath + "\\SciLexer.dll";
            if (File.Exists(filename))
            {
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(filename).FileVersion;
            }
            else return "...";
        }


        //LASTWORDSEARCH (used in FormMain)
        public static string LastWordSearch = "";
        //LASTWAVEFILE (used in FormMain)
        public static string LastWaveFile = "";


        //METHOD TO VISUALIZE GENERAL ERRORS IN WINXOUND
        public static void wxMessageError(string mErrorMessage, string mTitle)
        {
            try
            {
                DialogResult ret =
                    MessageBox.Show(
                        mErrorMessage + Environment.NewLine + Environment.NewLine +
                        "If you would like to send this error message to the" + Environment.NewLine +
                        "author please click the 'OK' button.",
                        mTitle,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                if (ret == DialogResult.OK)
                {
                    //mailto:info@xxx.com?body=yyy&subject=wwwwww
                    String EmailLink = @"mailto:stefano_bonetti@tin.it?" +
                                       @"body=" +
                                       mTitle + @"%0a" +  //%0a=linefeed
                                       Environment.OSVersion.ToString() + @"%0a" +
                                       mErrorMessage +
                                       @"&subject=WinXound Error";
                    System.Diagnostics.Process.Start(EmailLink);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxGlobal - wxMessageError: " + ex.Message);
            }
        }


        //OLD
        //public enum CHECK_MODIFY
        //{
        //    mContinue = 1,
        //    mStop = 0
        //}


        /////////////////////////
        //GLOBAL SETTINGS
        public static wxSettings Settings = new wxSettings();


        /////////////////////////
        //Various names
        public static string RTConsoleName
        {
            get { return Application.StartupPath + "\\Utility\\WinXound_RTConsole.exe"; }
        }
        //public static string SettingsFileName = "WinXoundSettings";

        /////////////////////////
        //LOAD OPCODES METHOD
        public static Hashtable LoadOpcodes()
        {
            //Load opcodes from "opcodes.txt" file into Opcodes_Hashtable 

            //NO MORE NEEDED: WinXound now include opcdes.txt as embedded resource //RESTORED 29/03/2015
            //Check if file exists; if not use the default one included inside resources.
            if (!File.Exists(Application.StartupPath + "\\Utility\\opcodes.txt"))
            {
                using (StreamReader sr = GetResource("Resources.opcodes.txt"))
                {
                    string text = sr.ReadToEnd();
                    File.WriteAllText(Application.StartupPath + "\\Utility\\opcodes.txt", text);
                }
            }


            Hashtable Opcodes = new Hashtable();
            //OLD: new StreamReader(Application.StartupPath + "\\Utility\\opcodes.txt"))
            //using (StreamReader reader = GetResource("Resources.opcodes.txt"))
            using (StreamReader reader = new StreamReader(Application.StartupPath + "\\Utility\\opcodes.txt"))
            {
                string mString = null;

                while (!(reader.Peek() == -1))
                {
                    mString = reader.ReadLine();
                    if (string.IsNullOrEmpty(mString)) continue;

                    string[] split = mString.Split(";".ToCharArray());
                    if (!(split.Length == 4)) continue;

                    for (byte i = 0; i < 4; i++)
                    {
                        if (split[i].Length > 0)
                        {
                            split[i] = split[i].Trim();
                            split[i] = split[i].Remove(0, 1);
                            split[i] = split[i].Remove(split[i].Length - 1, 1);
                        }
                    }
                    if (!Opcodes.Contains(split[0]))
                    {
                        Opcodes.Add(split[0],
                                    split[1] + "|" +
                                    split[2] + "|" +
                                    split[3]);
                    }
                }

                //No more needed. This is called by "using" statement on StreamReader.Dispose
                //reader.Close();
            }


            //Error has occurred?
            if (Opcodes == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxGlobal - LoadOpcodes error: OPCODES == NULL");
                //wxGlobal.wxMessageError(ex.Message, "wxGlobal - LoadOpcodes");
                //return null;
            }

            return Opcodes;

        }



        public static byte BoolToByte(bool mBool)
        {
            if (mBool == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public static bool CheckAllowedFileTypes(string mFileName)
        {
            if (mFileName.ToLower().EndsWith(".csd") ||
                mFileName.ToLower().EndsWith(".orc") ||
                mFileName.ToLower().EndsWith(".sco") ||
                mFileName.ToLower().EndsWith(".txt") ||
                mFileName.ToLower().EndsWith(".inc") ||
                mFileName.ToLower().EndsWith(".h"))
            {
                return true;
            }

            return false;
        }

        public static bool CheckAllowedFileTypes(string[] list)
        {
            try
            {
                if (list.GetValue(0).ToString().ToLower().EndsWith(".csd") ||
                    list.GetValue(0).ToString().ToLower().EndsWith(".orc") ||
                    list.GetValue(0).ToString().ToLower().EndsWith(".sco") ||
                    list.GetValue(0).ToString().ToLower().EndsWith(".txt") ||
                    list.GetValue(0).ToString().ToLower().EndsWith(".inc") ||
                    list.GetValue(0).ToString().ToLower().EndsWith(".h"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxGlobal - CheckAllowedFileTypes: " + ex.Message);
                return false;
            }

            return false;
        }


        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve internal resources
        public static StreamReader GetResource(string filename)
        {
            //Usage: GetResource("Resources.opcodes.txt")

            Assembly _assembly;
            StreamReader _StreamReader;

            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                string _AssemblyName = _assembly.GetName().Name;
                string _Resource = _AssemblyName + "." + filename;
                _StreamReader = new StreamReader(_assembly.GetManifestResourceStream(_Resource));
                return _StreamReader;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxGlobal - GetResource: " + ex.Message);
                return null;
            }

        }

        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve the pointer of an object
        public static int VarPtr(object o)
        {
            try
            {
                System.Runtime.InteropServices.GCHandle GC =
                System.Runtime.InteropServices.GCHandle.Alloc(o,
                System.Runtime.InteropServices.GCHandleType.Pinned);
                int ret = GC.AddrOfPinnedObject().ToInt32();

                return ret;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxGlobal - GetResource: " + ex.Message);
                return 0;
            }
        }


        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to create Desktop shortcut
        public static void CreateDesktopShortcut()
        {
            try
            {

                string mString = "";
                string newline = System.Environment.NewLine;

                mString += "'FORCE EXPLICIT VARIABLE DECLARATION" + newline +
                           "option explicit" + newline +
                           "'STEP OVER ERRORS FOR CUSTOM ERROR REPORTING" + newline +
                           "on error resume next" + newline +
                           "'DECLARE VARIABLES" + newline +
                           "dim shell, desktopPath, link" + newline +
                           "'INSTANTIATE THE WINDOWS SCRIPT HOST SHELL OBJECT" + newline +
                           "Set shell = WScript.CreateObject(\"WScript.shell\")" + newline +
                           "'SET THE PATH TO THE WINDOWS DESKTOP FOLDER & MY DOCUMENTS FOLDER" + newline +
                           "desktopPath = shell.SpecialFolders(\"Desktop\")" + newline +
                           "'CREATE A SHORTCUT ON THE USER'S DESKTOP" + newline +
                    //"Set link = shell.CreateShortcut(desktopPath & "\WinXound.lnk")
                           "Set link = shell.CreateShortcut(desktopPath & \"\\" + TITLE + ".lnk\")" + newline +
                           "'SET THE PROPERTIES FOR THE SHORTCUT" + newline +
                           "link.Description = \"WinXound\"" + newline +
                           "link.TargetPath = \"" + Application.ExecutablePath + "\"" + newline +
                           "link.WindowStyle = 0" + newline +
                           "link.WorkingDirectory = desktopPath" + newline +
                           "link.Save" + newline +
                           "'CLEANUP OBJECTS" + newline +
                           "set shell = nothing" + newline +
                           "'LET THE USER KNOW IF THERE WAS AN ERROR AND WHAT IT WAS" + newline +
                           "'OTHERWISE CONFIRM SHORCUT CREATION" + newline +
                           "if err.number <> 0 then" + newline +
                           "msgbox \"There was an error creating your shortcut.\" & vbCrLf & err.description & vbCrLf & err.source, vbOKOnly-vbExclamation, \"WinXound\"" + newline +
                           "else" + newline +
                           "msgBox \"Your new shortcut has been created!\" & vbCrLf & \"Please check your Windows Desktop.\", vbOKOnly-vbInformation, \"WinXound\"" + newline +
                           "end if";


                File.WriteAllText(Application.StartupPath + "\\Settings\\WinXound.vbs",
                                  mString);

                if (File.Exists(Application.StartupPath + "\\Settings\\WinXound.vbs"))
                {
                    System.Diagnostics.Process.Start(
                        Application.StartupPath + "\\Settings\\WinXound.vbs");
                }
                else
                {
                    MessageBox.Show("Unable to create the desktop shortcut",
                                    "WinXound Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                };
            }
            catch (Exception ex)
            {
                wxMessageError(ex.Message, "wxGlobal - CreateDesktopShortcut");
            }
        }


        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve String occurrences
        public static int CountStringOccurrences(string text, string pattern)
        {
            int count = text.Length - text.Replace(pattern, "").Length;
            return count;
        }


        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve if the file is with Syntax Highlight
        public static bool isSyntaxType(wxEditor editor)
        {
            if (editor != null)
            {
                if (editor.FileName.ToLower().EndsWith(".csd") ||
                    editor.FileName.ToLower().EndsWith(".inc") ||
                    editor.FileName.ToLower().EndsWith(".udo") ||
                    editor.FileName.ToLower().EndsWith(".py") ||
                    editor.FileName.ToLower().EndsWith(".pyw") ||
                    editor.FileName.ToLower().EndsWith(".lua") ||
                    editor.FileName.ToLower().EndsWith(".orc") ||
                    editor.FileName.ToLower().EndsWith(".sco"))
                {
                    return true;
                }
            }
            return false;
        }


        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve if the Os version is Vista or Major
        public static bool IsWinVistaOrMajor()
        {
            OperatingSystem OS = Environment.OSVersion;
            return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
        }



        ///////////////////////////////////////////////////////////////////////////////
        //Useful methods to retrieve the Program Files directory on 32 or 64 bit environment
        public static string GetProgramsFolder32()
        {
            string ProgramsFolder =
                System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            if (string.IsNullOrEmpty(ProgramsFolder))
            {
                ProgramsFolder =
                    System.Environment.GetFolderPath(
                        Environment.SpecialFolder.ProgramFiles);
                if(!ProgramsFolder.EndsWith("(x86)"))
                    ProgramsFolder += "(x86)";
            }

            if(string.IsNullOrEmpty(ProgramsFolder))
                return "";

            return ProgramsFolder;
        }

        public static string GetProgramsFolder64()
        {
            string ProgramsFolder =
                System.Environment.GetEnvironmentVariable("ProgramW6432");

            if (string.IsNullOrEmpty(ProgramsFolder))
            {
                ProgramsFolder =
                    System.Environment.GetFolderPath(
                        Environment.SpecialFolder.ProgramFiles);
                if (ProgramsFolder.EndsWith("(x86)"))
                    ProgramsFolder.Replace("(x86)", "");
            }

            if (string.IsNullOrEmpty(ProgramsFolder))
                return "";

            return ProgramsFolder;
        }


        ////////////////////////////////////////////////
        //USEFUL METHOD TO RETRIEVE THE CURRENT LOCAL IP
        public static string GetLocalIP()
        {
            System.Net.IPHostEntry h = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName());
            return ((System.Net.IPAddress)h.AddressList.GetValue(0)).ToString();
        }



        #region " OLD REMMED STUFFS "

        //public static string ParseLine(string Text)
        //{

        //    string dChars = "\\s{1,}";
        //    ////'Dim RegExp As New Regex("\s{1,}\d\s{1,}") 
        //    Regex RegExp = new Regex("\\d{1,3}");
        //    //string ParsedText = RegExp.Replace(Text, dChars, " ");
        //    string ParsedText = Regex.Replace(Text, dChars, " ");
        //    ////'Dim ParsedText As String = Regex.Replace(Text, dChars, " ") 

        //    ParsedText = ParsedText.Trim();// Trim(" ");

        //    Match MC = RegExp.Match(ParsedText);
        //    if (MC.Success)
        //    {
        //        if (MC.Length == 1)
        //        {
        //            return ParsedText.Insert(MC.Index, "00");
        //        }
        //        else if (MC.Length == 2)
        //        {
        //            return ParsedText.Insert(MC.Index, "0");
        //        }
        //        else
        //        {
        //            return ParsedText;
        //        }
        //    }
        //    else
        //    {
        //        return ParsedText;
        //    }

        //}




        //#region Assembly Attribute
        //public string AssemblyTitle
        //{
        //    get
        //    {
        //        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        //        if (attributes.Length > 0)
        //        {
        //            AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
        //            if (titleAttribute.Title != "")
        //            {
        //                return titleAttribute.Title;
        //            }
        //        }
        //        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        //    }
        //}

        //public string AssemblyVersion
        //{
        //    get
        //    {
        //        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //    }
        //}

        //public string AssemblyDescription
        //{
        //    get
        //    {
        //        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
        //        if (attributes.Length == 0)
        //        {
        //            return "";
        //        }
        //        return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        //    }
        //}

        //public string AssemblyProduct
        //{
        //    get
        //    {
        //        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        //        if (attributes.Length == 0)
        //        {
        //            return "";
        //        }
        //        return ((AssemblyProductAttribute)attributes[0]).Product;
        //    }
        //}

        //public string AssemblyCopyright
        //{
        //    get
        //    {
        //        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        //        if (attributes.Length == 0)
        //        {
        //            return "";
        //        }
        //        return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        //    }
        //}

        //public string AssemblyCompany
        //{
        //    get
        //    {
        //        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        //        if (attributes.Length == 0)
        //        {
        //            return "";
        //        }
        //        return ((AssemblyCompanyAttribute)attributes[0]).Company;
        //    }
        //}
        //#endregion
        #endregion

    }

}
