using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace WinXound_Net
{
    [Serializable]
    public class wxSettings
    {

        private string d = "|";
        //private const Int32 SC_ALPHA_NOALPHA = 256;

        public List<string> RecentFiles;
        public List<string> LastSessionFiles;

        private const Int32 MAX_RECENT_FILES = 9;

        private string newline = System.Environment.NewLine;

        public string VERSION = "3";

        //Styles format reminders
        //Styles 0-255 for Scintilla
        //Style 256 = TextSelection
        //Style 257 = Bookmarks
        //Style 258 = VerticalRuler
        //Style 259 = MarkCaretLine

        /// <summary>
        /// GENERAL STRUCTURE
        /// </summary>
        public struct mGeneral
        {
            //General
            public bool ShowUtilitiesMessage;
            public bool ShowImportOrcScoMessage;
            public bool ShowReadOnlyFileMessage;
            public bool BringWinXoundToFrontForCabbage;
            public FormWindowState WindowState;
            public Point WindowSize;
            public Point WindowPosition;
            public Point CompilerWindowSize;
            public Point CompilerWindowPosition;
            public bool FirstStart;
            public bool FindWholeWord;
            public bool FindMatchCase;
            public bool ReplaceFromCaret;
            public bool ShowToolbar;
            public List<bool> ToolBarItems;
            public string CompilerFontName;
            public Int32 DefaultWavePlayer;
            public Int32 CompilerFontSize;
            public Int32 StartupAction;
            public Int32 OrcScoImport;
            public bool UseWinXoundFlags;
            //CSound
            public string CSoundDefaultFlags;
            public string CSoundOrcScoFlags;
            public List<string> CSoundAdditionalFlags;
            public Int32 OpenSoundFileWith;
            //Python
            public string PythonDefaultFlags;
            //Lua
            public string LuaDefaultFlags;

        }
        public mGeneral General;


        /// <summary>
        /// DIRECTORY STRUCTURE
        /// </summary>
        public struct mDirectory
        {
            //General
            public string WaveEditor;
            public string WorkingDir;
            //CSound
            public string CSoundConsole;
            public string Winsound;
            public string CSoundAV;
            public string CSoundAVHelp;
            public string CSoundHelpHTML;
            public string SFDIR;
            public string SSDIR;
            public string SADIR;
            public string MFDIR;
            public string INCDIR;
            public string OPCODEDIR;
            public bool UseSFDIR;
            //Python
            public string PythonConsolePath;
            public string PythonIdlePath;
            //Lua
            public string LuaConsolePath;
            public string LuaGuiPath;
            //Cabbage
            public string CabbagePath;

        }
        public mDirectory Directory;


        /// <summary>
        /// EDITOR PROPERTIES STRUCTURE
        /// </summary>
        //[XmlAttribute("EditorProperties")]
        public struct mEditorProperties
        {
            //General
            public string DefaultFontName;
            public Int32 DefaultFontSize;
            public Int32 DefaultTabSize;
            public bool ShowVerticalRuler;
            public bool ShowMatchingBracket;
            public bool ShowLineNumbers;
            public bool ShowIntelliTip;
            public bool ShowExplorer;
            public bool MarkCaretLine;
            public bool SaveBookmarks;
            public bool ExplorerShowOptions;
            public bool ExplorerShowInstrMacros;
            public bool ExplorerShowInstrOpcodes;
            public bool ExplorerShowInstrInstruments;
            public bool ExplorerShowScoreFunctions;
            public bool ExplorerShowScoreMacros;
            public bool ExplorerShowScoreSections;
            public Int32 ExplorerFontSize;

            //Language specific style values
            //0=StyleNumber // 1=ForeColor // 2=BackColor // 3=Bold
            //4=Italic // 5=Alpha // 6=EolFilled // 7=FriendlyName
            public List<string> CSoundStyles;
            public List<string> PythonStyles;
            public bool UseMixedPython;
            public List<string> LuaStyles;

            #region " Useful methods for styles "

            private List<string> tempList;

            //Useful Methods
            public string StyleGetFriendlyName(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList != null)
                {
                    foreach (string s in tempList)
                    {
                        if (GetValues(s)[0] == Convert.ToString(stylenumber))
                        {
                            return GetValues(s)[7];
                        }
                    }
                }
                return "";
            }
            public Int32 StyleGetForeColor(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList != null)
                {
                    foreach (string s in tempList)
                    {
                        if (GetValues(s)[0] == Convert.ToString(stylenumber))
                        {
                            return Convert.ToInt32(GetValues(s)[1]);
                        }
                    }
                    //32 = Default Scintilla Style
                    return StyleGetForeColor(language, 32);
                }
                return 0; //Default Fore color (Black)  
            }
            public Int32 StyleGetBackColor(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList != null)
                {
                    foreach (string s in tempList)
                    {
                        if (GetValues(s)[0] == Convert.ToString(stylenumber))
                        {
                            return Convert.ToInt32(GetValues(s)[2]);
                        }
                    }
                    return StyleGetBackColor(language, 32);
                }
                return 16777215; //Default Back color (White);
            }
            public bool StyleGetBold(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList != null)
                {
                    foreach (string s in tempList)
                    {
                        if (GetValues(s)[0] == Convert.ToString(stylenumber))
                        {
                            return Convert.ToBoolean(GetValues(s)[3]);
                        }
                    }
                    return StyleGetBold(language, 32);
                }
                return false;
            }
            public bool StyleGetItalic(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList != null)
                {
                    foreach (string s in tempList)
                    {
                        if (GetValues(s)[0] == Convert.ToString(stylenumber))
                        {
                            return Convert.ToBoolean(GetValues(s)[4]);
                        }
                    }
                    return StyleGetItalic(language, 32);
                }
                return false;
            }
            public Int32 StyleGetAlpha(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList == null) return 40;

                foreach (string s in tempList)
                {
                    if (GetValues(s)[0] == Convert.ToString(stylenumber))
                    {
                        return Convert.ToInt32(GetValues(s)[5]);
                    }
                }
                return 40; //Default Alpha for Bookmarks;
            }
            public bool StyleGetEolFilled(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList == null) return false;

                foreach (string s in tempList)
                {
                    if (GetValues(s)[0] == Convert.ToString(stylenumber))
                    {
                        return Convert.ToBoolean(GetValues(s)[6]);
                    }
                }
                return false;
            }
            public Int32 StyleGetListPosition(string language, Int32 stylenumber)
            {
                tempList = SelectLanguage(language);
                if (tempList == null) return -1;

                Int32 index = 0;
                foreach (string s in tempList)
                {
                    if (GetValues(s)[0] == Convert.ToString(stylenumber))
                    {
                        return index;
                    }
                    index++;
                }
                return -1;
            }

            private List<string> SelectLanguage(string language)
            {
                switch (language)
                {
                    case "csound":
                        return CSoundStyles;
                    case "python":
                        return PythonStyles;
                    case "lua":
                        return LuaStyles;
                }
                return null;
            }

            private string[] GetValues(string s)
            {
                try
                {
                    return s.Split(",".ToCharArray());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "wxSettings.EditorProperties.GetValues Error: " + ex.Message);
                    return new string[] { "" };
                }
            }
            #endregion
        }
        public mEditorProperties EditorProperties;


        /// <summary>
        /// CODEFORMAT STRUCTURE
        /// </summary>
        public struct mCodeFormat
        {
            //CSound
            public bool FormatHeader;
            public bool FormatInstruments;
            public bool FormatFunctions;
            public bool FormatScoreInstruments;
            public bool FormatTempo;
            public Int32 InstrumentsType;
            public Int32 TabIndent;
        }
        public mCodeFormat CodeFormat;

        public struct mTemplates
        {
            public string CSound;
            public string Python;
            public string Lua;
            public string Cabbage;
        }
        public mTemplates Templates;


        ///////////////////////////////////////////////////////////////////////////////
        //CONSTRUCTOR
        ///////////////////////////////////////////////////////////////////////////////
        public wxSettings()
        {
            //Initialize List's and Create the default settings
            this.RecentFiles = new List<string>(MAX_RECENT_FILES);
            this.LastSessionFiles = new List<string>();
            this.General.ToolBarItems = new List<bool>();
            this.General.CSoundAdditionalFlags = new List<string>();
            CreateDefaultWinXoundSettings();
        }


        ///////////////////////////////////////////////////////////////////////////////
        // LOAD SETTINGS
        ///////////////////////////////////////////////////////////////////////////////
        public bool LoadSettings(bool readOnly)
        {
            return this.LoadSettings(Application.StartupPath +
                                     "\\Settings\\WinXoundSettings.txt", readOnly);
        }

        public bool LoadSettings(string filename, bool readOnly)
        {
            //Read datas from file
            string[] input = null;

            try
            {
                if (File.Exists(filename))
                {
                    input = File.ReadAllLines(filename);
                }
                else
                {
                    this.General.FirstStart = true;
                    this.CreateDefaultWinXoundSettings();
                    if (readOnly == false)
                        this.SaveSettings();
                }
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(
                //   "LoadSettings - Settings Error : " + ex.Message);
                wxGlobal.wxMessageError(ex.Message,
                    "WinXound - wxSettings.LoadSettings Error");

                this.CreateDefaultWinXoundSettings();
            }


            if (input != null)
            {
                //this.General.ToolBarItems = new List<bool>();
                this.General.ToolBarItems.Clear();
                this.General.CSoundAdditionalFlags.Clear();

                string field = "";
                string value = "";

                foreach (string s in input)
                {
                    field = GetField(s);
                    value = GetValue(s);
                    if (String.IsNullOrEmpty(value)) continue;

                    try
                    {
                        switch (field)
                        {
                            case "VERSION":
                                this.VERSION = value;
                                break;

                            //GENERAL
                            case "General.ShowUtilitiesMessage":
                                this.General.ShowUtilitiesMessage =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.ShowImportOrcScoMessage":
                                this.General.ShowImportOrcScoMessage =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.ShowReadOnlyFileMessage":
                                this.General.ShowReadOnlyFileMessage =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.BringWinXoundToFrontForCabbage":
                                this.General.BringWinXoundToFrontForCabbage =
                                    Convert.ToBoolean(value);
                                break;



                            case "General.WindowState":
                                this.General.WindowState =
                                   (FormWindowState)Enum.Parse(typeof(FormWindowState),
                                   value);
                                break;

                            case "General.WindowSize":
                                Point ps = GetPointValue(value);
                                if (ps.X != 0 && ps.Y != 0)
                                    this.General.WindowSize = ps;
                                break;

                            case "General.WindowPosition":
                                Point pp = GetPointValue(value);
                                if (pp.X != 0 && pp.Y != 0)
                                    this.General.WindowPosition = pp;
                                break;

                            case "General.CompilerWindowSize":
                                Point psw = GetPointValue(value);
                                if (psw.X != 0 && psw.Y != 0)
                                    this.General.CompilerWindowSize = psw;
                                break;

                            case "General.CompilerWindowPosition":
                                Point ppw = GetPointValue(value);
                                if (ppw.X != 0 && ppw.Y != 0)
                                    this.General.CompilerWindowPosition = ppw;
                                break;

                            case "General.FirstStart":
                                this.General.FirstStart =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.FindWholeWord":
                                this.General.FindWholeWord =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.FindMatchCase":
                                this.General.FindMatchCase =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.ReplaceFromCaret":
                                this.General.ReplaceFromCaret =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.ShowToolbar":
                                this.General.ShowToolbar =
                                    Convert.ToBoolean(value);
                                break;

                            case "General.StartupAction":
                                this.General.StartupAction =
                                    Convert.ToInt32(value);
                                break;

                            case "General.OrcScoImport":
                                this.General.OrcScoImport =
                                    Convert.ToInt32(value);
                                break;



                            //TOOLBAR ELEMENTS
                            case "General.ToolBarItems":
                                this.General.ToolBarItems.Add(
                                    Convert.ToBoolean(value));
                                break;

                            case "General.CSoundDefaultFlags":
                                this.General.CSoundDefaultFlags = value;
                                break;

                            case "General.CSoundOrcScoFlags":
                                this.General.CSoundOrcScoFlags = value;
                                break;

                            case "General.CSoundAdditionalFlags":
                                if (!this.General.CSoundAdditionalFlags.Contains(value))
                                    this.General.CSoundAdditionalFlags.Add(value);
                                break;

                            case "General.OpenSoundFileWith":
                                this.General.OpenSoundFileWith =
                                    Convert.ToInt32(value);
                                break;

                            case "General.PythonDefaultFlags":
                                this.General.PythonDefaultFlags = value;
                                break;

                            case "General.CompilerFontName":
                                this.General.CompilerFontName = value;
                                break;

                            case "General.DefaultWavePlayer":
                                this.General.DefaultWavePlayer =
                                    Convert.ToInt32(value);
                                break;

                            case "General.CompilerFontSize":
                                this.General.CompilerFontSize =
                                    Convert.ToInt32(value);
                                break;

                            case "General.LuaDefaultFlags":
                                this.General.LuaDefaultFlags = value;
                                break;

                            case "General.UseWinXoundFlags":
                                this.General.UseWinXoundFlags =
                                    Convert.ToBoolean(value);
                                break;

                            //DIRECTORY
                            case "Directory.WaveEditor":
                                this.Directory.WaveEditor = value;
                                break;

                            case "Directory.WorkingDir":
                                this.Directory.WorkingDir = value;
                                break;

                            case "Directory.CSoundConsole":
                                this.Directory.CSoundConsole = value;
                                break;

                            case "Directory.Winsound":
                                this.Directory.Winsound = value;
                                break;

                            case "Directory.CSoundAV":
                                this.Directory.CSoundAV = value;
                                break;

                            case "Directory.CSoundAVHelp":
                                this.Directory.CSoundAVHelp = value;
                                break;

                            case "Directory.CSoundHelpHTML":
                                this.Directory.CSoundHelpHTML = value;
                                break;

                            case "Directory.SFDIR":
                                this.Directory.SFDIR = value;
                                break;

                            case "Directory.SSDIR":
                                this.Directory.SSDIR = value;
                                break;

                            case "Directory.SADIR":
                                this.Directory.SADIR = value;
                                break;

                            case "Directory.MFDIR":
                                this.Directory.MFDIR = value;
                                break;

                            case "Directory.INCDIR":
                                this.Directory.INCDIR = value;
                                break;

                            case "Directory.OPCODEDIR":
                                this.Directory.OPCODEDIR = value;
                                break;

                            case "Directory.UseSFDIR":
                                this.Directory.UseSFDIR =
                                    Convert.ToBoolean(value);
                                break;

                            case "Directory.PythonConsolePath":
                                this.Directory.PythonConsolePath = value;
                                break;

                            case "Directory.PythonIdlePath":
                                this.Directory.PythonIdlePath = value;
                                break;

                            case "Directory.LuaConsolePath":
                                this.Directory.LuaConsolePath = value;
                                break;

                            case "Directory.LuaGuiPath":
                                this.Directory.LuaGuiPath = value;
                                break;

                            case "Directory.CabbagePath":
                                this.Directory.CabbagePath = value;
                                break;



                            //EDITORPROPERTIES
                            case "EditorProperties.DefaultFontName":
                                this.EditorProperties.DefaultFontName = value;
                                break;

                            case "EditorProperties.DefaultFontSize":
                                this.EditorProperties.DefaultFontSize =
                                    Convert.ToInt32(value);
                                break;

                            case "EditorProperties.DefaultTabSize":
                                this.EditorProperties.DefaultTabSize =
                                    Convert.ToInt32(value);
                                break;

                            case "EditorProperties.ShowVerticalRuler":
                                this.EditorProperties.ShowVerticalRuler =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ShowMatchingBracket":
                                this.EditorProperties.ShowMatchingBracket =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ShowLineNumbers":
                                this.EditorProperties.ShowLineNumbers =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ShowIntelliTip":
                                this.EditorProperties.ShowIntelliTip =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ShowExplorer":
                                this.EditorProperties.ShowExplorer =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.MarkCaretLine":
                                this.EditorProperties.MarkCaretLine =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.SaveBookmarks":
                                this.EditorProperties.SaveBookmarks =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.UseMixedPython":
                                this.EditorProperties.UseMixedPython =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowOptions":
                                this.EditorProperties.ExplorerShowOptions =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowInstrMacros":
                                this.EditorProperties.ExplorerShowInstrMacros =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowInstrOpcodes":
                                this.EditorProperties.ExplorerShowInstrOpcodes =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowInstrInstruments":
                                this.EditorProperties.ExplorerShowInstrInstruments =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowScoreFunctions":
                                this.EditorProperties.ExplorerShowScoreFunctions =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowScoreMacros":
                                this.EditorProperties.ExplorerShowScoreMacros =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerShowScoreSections":
                                this.EditorProperties.ExplorerShowScoreSections =
                                    Convert.ToBoolean(value);
                                break;

                            case "EditorProperties.ExplorerFontSize":
                                this.EditorProperties.ExplorerFontSize =
                                   Convert.ToInt32(value);
                                break;

                            //STYLES
                            case "EditorProperties.CSoundStyles":
                                try
                                {
                                    Int32 StyleNumber =
                                        Convert.ToInt32(value.Split(",".ToCharArray())[0]);
                                    Int32 ListIndexOfOldStyle =
                                        EditorProperties.StyleGetListPosition(
                                            "csound", StyleNumber);

                                    if (ListIndexOfOldStyle > -1)
                                    {
                                        this.EditorProperties.CSoundStyles[ListIndexOfOldStyle] =
                                            value;
                                    }
                                    else this.EditorProperties.CSoundStyles.Add(value);
                                }
                                catch { }
                                break;

                            case "EditorProperties.PythonStyles":
                                try
                                {
                                    Int32 StyleNumber =
                                        Convert.ToInt32(value.Split(",".ToCharArray())[0]);
                                    Int32 ListIndexOfOldStyle =
                                        EditorProperties.StyleGetListPosition(
                                            "python", StyleNumber);

                                    if (ListIndexOfOldStyle > -1)
                                    {
                                        this.EditorProperties.PythonStyles[ListIndexOfOldStyle] =
                                            value;
                                    }
                                    else this.EditorProperties.PythonStyles.Add(value);
                                }
                                catch { }
                                break;

                            case "EditorProperties.LuaStyles":
                                try
                                {
                                    Int32 StyleNumber =
                                        Convert.ToInt32(value.Split(",".ToCharArray())[0]);
                                    Int32 ListIndexOfOldStyle =
                                        EditorProperties.StyleGetListPosition(
                                            "lua", StyleNumber);

                                    if (ListIndexOfOldStyle > -1)
                                    {
                                        this.EditorProperties.LuaStyles[ListIndexOfOldStyle] =
                                            value;
                                    }
                                    else this.EditorProperties.LuaStyles.Add(value);
                                }
                                catch { }
                                break;

                            //CODEFORMAT
                            case "CodeFormat.FormatHeader":
                                this.CodeFormat.FormatHeader =
                                    Convert.ToBoolean(value);
                                break;

                            case "CodeFormat.FormatInstruments":
                                this.CodeFormat.FormatInstruments =
                                    Convert.ToBoolean(value);
                                break;

                            case "CodeFormat.FormatFunctions":
                                this.CodeFormat.FormatFunctions =
                                    Convert.ToBoolean(value);
                                break;

                            case "CodeFormat.FormatScoreInstruments":
                                this.CodeFormat.FormatScoreInstruments =
                                    Convert.ToBoolean(value);
                                break;

                            case "CodeFormat.FormatTempo":
                                this.CodeFormat.FormatTempo =
                                    Convert.ToBoolean(value);
                                break;

                            case "CodeFormat.InstrumentsType":
                                this.CodeFormat.InstrumentsType =
                                    Convert.ToInt32(value);
                                break;

                            case "CodeFormat.TabIndent":
                                this.CodeFormat.TabIndent =
                                    Convert.ToInt32(value);
                                break;

                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "wxSettings.LoadSettings." + field +
                            " (Value: " + value + ") " +
                            "Error: " + ex.Message);
                    }
                }

            }


            //Additional CSound Flags Check
            if (this.General.CSoundAdditionalFlags.Count < 1)
                CreateAdditionalCSoundFlags();


            //LOAD RECENT FILES LIST
            try
            {
                this.RecentFiles = new List<string>();

                if (File.Exists(Application.StartupPath + "\\Settings\\RecentFiles.txt"))
                {
                    string[] files = File.ReadAllLines(
                        Application.StartupPath + "\\Settings\\RecentFiles.txt");
                    if (files != null)
                    {
                        foreach (string s in files)
                        {
                            if (File.Exists(s))
                                this.RecentFiles.Add(s);
                        }
                    }
                }
                else //Create new RecentList file
                {
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\RecentFiles.txt",
                            "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.LoadSettings - RecentFiles Error : " + ex.Message);
            }



            //LOAD LAST SESSION FILES
            try
            {
                this.LastSessionFiles = new List<string>();

                if (File.Exists(Application.StartupPath + "\\Settings\\LastSessionFiles.txt"))
                {
                    string[] files = File.ReadAllLines(
                        Application.StartupPath + "\\Settings\\LastSessionFiles.txt");
                    if (files != null)
                    {
                        foreach (string s in files)
                        {
                            if (File.Exists(s))
                                this.LastSessionFiles.Add(s);
                        }
                    }
                }
                else //Create new LastSessionFiles file
                {
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\LastSessionFiles.txt",
                            "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.LoadSettings - LastSessionFiles Error : " + ex.Message);
            }




            //SET ENVIRONMENT VARIABLES
            this.SetEnvironmentVariables();


            //LOAD TEMPLATES
            this.CreateDefaultTemplates();
            try
            {
                //Csound template
                if (File.Exists(Application.StartupPath + "\\Settings\\CSoundTemplate.txt"))
                {
                    this.Templates.CSound = File.ReadAllText(
                        Application.StartupPath + "\\Settings\\CSoundTemplate.txt");
                }
                else
                {
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\CSoundTemplate.txt",
                            this.Templates.CSound);
                }

                //Python Template
                if (File.Exists(Application.StartupPath + "\\Settings\\PythonTemplate.txt"))
                {
                    this.Templates.Python = File.ReadAllText(
                        Application.StartupPath + "\\Settings\\PythonTemplate.txt");
                }
                else
                {
                    //this.Templates.Python = "";
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\PythonTemplate.txt",
                            "");
                }


                //Lua Template
                if (File.Exists(Application.StartupPath + "\\Settings\\LuaTemplate.txt"))
                {
                    this.Templates.Lua = File.ReadAllText(
                        Application.StartupPath + "\\Settings\\LuaTemplate.txt");
                }
                else
                {
                    //this.Templates.Lua = "";
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\LuaTemplate.txt",
                            "");
                }

                //Cabbage Template
                if (File.Exists(Application.StartupPath + "\\Settings\\CabbageTemplate.txt"))
                {
                    this.Templates.Cabbage = File.ReadAllText(
                        Application.StartupPath + "\\Settings\\CabbageTemplate.txt");
                }
                else
                {
                    if (readOnly == false)
                        File.WriteAllText(
                            Application.StartupPath + "\\Settings\\CabbageTemplate.txt",
                            this.Templates.Cabbage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.LoadSettings - Templates Error : " + ex.Message);
            }

            return true;

        }

        //Useful methods for LoadSettings
        private string GetField(string s)
        {
            try
            {
                return s.Split(d.ToCharArray())[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings - GetField Error: " + ex.Message);
                return "";
            }

        }
        private string GetValue(string s)
        {
            try
            {
                return s.Split(d.ToCharArray())[1];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.GetValue Error: " + ex.Message);
                return "";
            }
        }
        private Point GetPointValue(string s)
        {
            try
            {
                s = s.ToLower();
                s = s.Replace("{", "");
                s = s.Replace("x=", "");
                s = s.Replace("y=", "");
                s = s.Replace("}", "");

                Int32 x, y;
                x = Convert.ToInt32(s.Split(",".ToCharArray())[0]);
                y = Convert.ToInt32(s.Split(",".ToCharArray())[1]);
                return new Point(x, y);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.GetPointValue Error : " + ex.Message);
                return new Point(0, 0);
            }


        }


        ///////////////////////////////////////////////////////////////////////////////
        // SAVE SETTINGS
        ///////////////////////////////////////////////////////////////////////////////
        public bool SaveSettings()
        {
            return SaveSettings(Application.StartupPath +
                                "\\Settings\\WinXoundSettings.txt");
        }

        public bool SaveSettings(string filename)
        {

            //NEW METHOD
            List<string> output = new List<string>();
            output.Add("WinXoundSettings" + d);

            //VERSION
            output.Add("VERSION" + d + this.VERSION);

            //GENERAL
            output.Add("General.ShowUtilitiesMessage" + d + this.General.ShowUtilitiesMessage);
            output.Add("General.ShowImportOrcScoMessage" + d + this.General.ShowImportOrcScoMessage);
            output.Add("General.ShowReadOnlyFileMessage" + d + this.General.ShowReadOnlyFileMessage);
            output.Add("General.BringWinXoundToFrontForCabbage" + d + this.General.BringWinXoundToFrontForCabbage);
            output.Add("General.WindowState" + d + this.General.WindowState);
            output.Add("General.WindowSize" + d + this.General.WindowSize);
            output.Add("General.WindowPosition" + d + this.General.WindowPosition);
            output.Add("General.CompilerWindowSize" + d + this.General.CompilerWindowSize);
            output.Add("General.CompilerWindowPosition" + d + this.General.CompilerWindowPosition);
            output.Add("General.FirstStart" + d + this.General.FirstStart);
            output.Add("General.FindWholeWord" + d + this.General.FindWholeWord);
            output.Add("General.FindMatchCase" + d + this.General.FindMatchCase);
            output.Add("General.ReplaceFromCaret" + d + this.General.ReplaceFromCaret);
            output.Add("General.ShowToolbar" + d + this.General.ShowToolbar);
            output.Add("General.StartupAction" + d + this.General.StartupAction);
            output.Add("General.OrcScoImport" + d + this.General.OrcScoImport);
            //ToolBar Items
            foreach (bool b in this.General.ToolBarItems)
            {
                output.Add("General.ToolBarItems" + d + b);
            }
            output.Add("General.CSoundDefaultFlags" + d + this.General.CSoundDefaultFlags);
            output.Add("General.CSoundOrcScoFlags" + d + this.General.CSoundOrcScoFlags);
            //Additional flags
            foreach (string s in this.General.CSoundAdditionalFlags)
            {
                output.Add("General.CSoundAdditionalFlags" + d + s);
            }
            output.Add("General.OpenSoundFileWith" + d + this.General.OpenSoundFileWith);
            output.Add("General.PythonDefaultFlags" + d + this.General.PythonDefaultFlags);
            output.Add("General.CompilerFontName" + d + this.General.CompilerFontName);
            output.Add("General.DefaultWavePlayer" + d + this.General.DefaultWavePlayer);
            output.Add("General.CompilerFontSize" + d + this.General.CompilerFontSize);
            output.Add("General.LuaDefaultFlags" + d + this.General.LuaDefaultFlags);
            output.Add("General.UseWinXoundFlags" + d + this.General.UseWinXoundFlags);

            //DIRECTORY
            output.Add("Directory.WaveEditor" + d + this.Directory.WaveEditor);
            output.Add("Directory.WorkingDir" + d + this.Directory.WorkingDir);
            output.Add("Directory.CSoundConsole" + d + this.Directory.CSoundConsole);
            output.Add("Directory.Winsound" + d + this.Directory.Winsound);
            output.Add("Directory.CSoundAV" + d + this.Directory.CSoundAV);
            output.Add("Directory.CSoundAVHelp" + d + this.Directory.CSoundAVHelp);
            output.Add("Directory.CSoundHelpHTML" + d + this.Directory.CSoundHelpHTML);
            output.Add("Directory.SFDIR" + d + this.Directory.SFDIR);
            output.Add("Directory.SSDIR" + d + this.Directory.SSDIR);
            output.Add("Directory.SADIR" + d + this.Directory.SADIR);
            output.Add("Directory.MFDIR" + d + this.Directory.MFDIR);
            output.Add("Directory.INCDIR" + d + this.Directory.INCDIR);
            output.Add("Directory.OPCODEDIR" + d + this.Directory.OPCODEDIR);
            output.Add("Directory.UseSFDIR" + d + this.Directory.UseSFDIR);
            output.Add("Directory.PythonConsolePath" + d + this.Directory.PythonConsolePath);
            output.Add("Directory.PythonIdlePath" + d + this.Directory.PythonIdlePath);
            output.Add("Directory.LuaConsolePath" + d + this.Directory.LuaConsolePath);
            output.Add("Directory.LuaGuiPath" + d + this.Directory.LuaGuiPath);
            output.Add("Directory.CabbagePath" + d + this.Directory.CabbagePath);

            //EDITORPROPERTIES
            output.Add("EditorProperties.DefaultFontName" + d + this.EditorProperties.DefaultFontName);
            output.Add("EditorProperties.DefaultFontSize" + d + this.EditorProperties.DefaultFontSize);
            output.Add("EditorProperties.DefaultTabSize" + d + this.EditorProperties.DefaultTabSize);
            output.Add("EditorProperties.ShowVerticalRuler" + d + this.EditorProperties.ShowVerticalRuler);
            output.Add("EditorProperties.ShowMatchingBracket" + d + this.EditorProperties.ShowMatchingBracket);
            output.Add("EditorProperties.ShowLineNumbers" + d + this.EditorProperties.ShowLineNumbers);
            output.Add("EditorProperties.ShowIntelliTip" + d + this.EditorProperties.ShowIntelliTip);
            output.Add("EditorProperties.ShowExplorer" + d + this.EditorProperties.ShowExplorer);
            output.Add("EditorProperties.MarkCaretLine" + d + this.EditorProperties.MarkCaretLine);
            output.Add("EditorProperties.SaveBookmarks" + d + this.EditorProperties.SaveBookmarks);
            output.Add("EditorProperties.UseMixedPython" + d + this.EditorProperties.UseMixedPython);
            output.Add("EditorProperties.ExplorerShowOptions" + d + this.EditorProperties.ExplorerShowOptions);
            output.Add("EditorProperties.ExplorerShowInstrMacros" + d + this.EditorProperties.ExplorerShowInstrMacros);
            output.Add("EditorProperties.ExplorerShowInstrOpcodes" + d + this.EditorProperties.ExplorerShowInstrOpcodes);
            output.Add("EditorProperties.ExplorerShowInstrInstruments" + d + this.EditorProperties.ExplorerShowInstrInstruments);
            output.Add("EditorProperties.ExplorerShowScoreFunctions" + d + this.EditorProperties.ExplorerShowScoreFunctions);
            output.Add("EditorProperties.ExplorerShowScoreMacros" + d + this.EditorProperties.ExplorerShowScoreMacros);
            output.Add("EditorProperties.ExplorerShowScoreSections" + d + this.EditorProperties.ExplorerShowScoreSections);
            output.Add("EditorProperties.ExplorerFontSize" + d + this.EditorProperties.ExplorerFontSize);

            //CSound Styles
            foreach (string s in this.EditorProperties.CSoundStyles)
            {
                output.Add("EditorProperties.CSoundStyles" + d + s);
            }
            //Python Styles
            foreach (string s in this.EditorProperties.PythonStyles)
            {
                output.Add("EditorProperties.PythonStyles" + d + s);
            }
            //Lua Styles
            foreach (string s in this.EditorProperties.LuaStyles)
            {
                output.Add("EditorProperties.LuaStyles" + d + s);
            }
            //CODEFORMAT
            output.Add("CodeFormat.FormatHeader" + d + this.CodeFormat.FormatHeader);
            output.Add("CodeFormat.FormatInstruments" + d + this.CodeFormat.FormatInstruments);
            output.Add("CodeFormat.FormatFunctions" + d + this.CodeFormat.FormatFunctions);
            output.Add("CodeFormat.FormatScoreInstruments" + d + this.CodeFormat.FormatScoreInstruments);
            output.Add("CodeFormat.FormatTempo" + d + this.CodeFormat.FormatTempo);
            output.Add("CodeFormat.InstrumentsType" + d + this.CodeFormat.InstrumentsType);
            output.Add("CodeFormat.TabIndent" + d + this.CodeFormat.TabIndent);

            //SAVE SETTINGS
            try
            {
                File.WriteAllLines(filename, output.ToArray());
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(
                //    "SaveSettings - SETTINGS Error : " + ex.Message);
                wxGlobal.wxMessageError(ex.Message,
                    "WinXound - wxSettings.SaveSettings Error");
            }

            //SAVE RECENT FILES LIST
            try
            {
                File.WriteAllLines(
                    Application.StartupPath + "\\Settings\\RecentFiles.txt",
                    RecentFiles.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "SaveSettings - RecentFiles Error : " + ex.Message);
            }


            //SAVE LAST SESSION FILES LIST
            try
            {
                File.WriteAllLines(
                    Application.StartupPath + "\\Settings\\LastSessionFiles.txt",
                    LastSessionFiles.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "SaveSettings - LastSessionFiles Error : " + ex.Message);
            }


            //SAVE TEMPLATES
            try
            {
                File.WriteAllText(
                    Application.StartupPath + "\\Settings\\CSoundTemplate.txt",
                    this.Templates.CSound);

                File.WriteAllText(
                    Application.StartupPath + "\\Settings\\PythonTemplate.txt",
                    this.Templates.Python);

                File.WriteAllText(
                    Application.StartupPath + "\\Settings\\LuaTemplate.txt",
                    this.Templates.Lua);

                File.WriteAllText(
                    Application.StartupPath + "\\Settings\\CabbageTemplate.txt",
                    this.Templates.Cabbage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "SaveSettings - Templates Error : " + ex.Message);
            }

            return true;

        }



        ///////////////////////////////////////////////////////////////////////////////
        // CREATE DEFAULT SETTINGS
        ///////////////////////////////////////////////////////////////////////////////
        public void CreateDefaultWinXoundSettings()
        {
            //VERSION
            this.VERSION = "3";

            //GENERAL
            this.General.ShowUtilitiesMessage = false;
            this.General.ShowImportOrcScoMessage = false;
            this.General.ShowReadOnlyFileMessage = true;
            this.General.BringWinXoundToFrontForCabbage = true;
            this.General.WindowState = FormWindowState.Normal;
            this.General.WindowSize = new Point(800, 600);
            this.General.WindowPosition = new Point(0, 0);
            this.General.CompilerWindowSize = new Point(700, 400);
            this.General.CompilerWindowPosition = new Point(0, 0);
            //this.General.FirstStart = true;
            this.General.FindWholeWord = false;
            this.General.FindMatchCase = false;
            this.General.ReplaceFromCaret = false;
            this.General.ShowToolbar = true;
            this.General.StartupAction = 0;
            this.General.OrcScoImport = 0;
            ////ToolBar Items (checked in FormMain)
            this.General.CompilerFontName = "Courier New";
            this.General.DefaultWavePlayer = 0;
            this.General.CompilerFontSize = 10;
            this.General.CSoundDefaultFlags = "-B4096 --displays --asciidisplay";
            this.General.CSoundOrcScoFlags = "-W -odac";
            //Additional flags
            CreateAdditionalCSoundFlags();
            this.General.OpenSoundFileWith = 1;
            this.General.PythonDefaultFlags = "-u";
            this.General.LuaDefaultFlags = "";
            this.General.UseWinXoundFlags = true;

            //DIRECTORY
            this.Directory.WaveEditor = "";
            this.Directory.WorkingDir = "";
            this.Directory.CSoundConsole = "";
            this.Directory.Winsound = "";
            this.Directory.CSoundAV = "";
            this.Directory.CSoundAVHelp = "";
            this.Directory.CSoundHelpHTML = "";
            this.Directory.SFDIR = "";
            this.Directory.SSDIR = "";
            this.Directory.SADIR = "";
            this.Directory.MFDIR = "";
            this.Directory.INCDIR = "";
            this.Directory.OPCODEDIR = "";
            this.Directory.UseSFDIR = true;
            this.Directory.PythonConsolePath = "";
            this.Directory.PythonIdlePath = "";
            this.Directory.LuaConsolePath = "";
            this.Directory.LuaGuiPath = "";
            this.Directory.CabbagePath = "";

            //EDITORPROPERTIES
            this.EditorProperties.DefaultFontName = "Courier New";
            this.EditorProperties.DefaultFontSize = 10;
            this.EditorProperties.DefaultTabSize = 8;
            this.EditorProperties.ShowVerticalRuler = false;
            this.EditorProperties.ShowMatchingBracket = true;
            this.EditorProperties.ShowLineNumbers = true;
            this.EditorProperties.ShowIntelliTip = true;
            this.EditorProperties.ShowExplorer = true;
            this.EditorProperties.MarkCaretLine = false;
            this.EditorProperties.SaveBookmarks = true;
            this.EditorProperties.UseMixedPython = true;
            this.EditorProperties.ExplorerShowOptions = true;
            this.EditorProperties.ExplorerShowInstrMacros = true;
            this.EditorProperties.ExplorerShowInstrOpcodes = true;
            this.EditorProperties.ExplorerShowInstrInstruments = true;
            this.EditorProperties.ExplorerShowScoreFunctions = true;
            this.EditorProperties.ExplorerShowScoreMacros = true;
            this.EditorProperties.ExplorerShowScoreSections = true;
            this.EditorProperties.ExplorerFontSize = 0;

            //CODEFORMAT
            this.CodeFormat.FormatHeader = true;
            this.CodeFormat.FormatInstruments = true;
            this.CodeFormat.FormatFunctions = true;
            this.CodeFormat.FormatScoreInstruments = true;
            this.CodeFormat.FormatTempo = true;
            this.CodeFormat.InstrumentsType = 0;
            this.CodeFormat.TabIndent = 8;

            //EditorProperties Styles
            this.CreateDefaultCSoundStyles();
            this.CreateDefaultPythonStyles();
            this.CreateDefaultLuaStyles();

            //TEMPLATES
            CreateDefaultTemplates();



        }

        public void CreateAdditionalCSoundFlags()
        {
            //Additional flags
            this.General.CSoundAdditionalFlags.Clear();
            this.General.CSoundAdditionalFlags.Add("[Realtime output]: -odac");
            this.General.CSoundAdditionalFlags.Add("[Render to filename]: -W -o\"CompilerOutput.wav\"");
            this.General.CSoundAdditionalFlags.Add("[Render to file using csd/orc/sco name]: -W -o\"*.wav\"");
            this.General.CSoundAdditionalFlags.Add("[Render to file asking for its name]: -W -o\"?.wav\"");

        }

        public string GetAdditionalCSoundFlags()
        {
            return "[Realtime output]: -odac" + newline +
                   "[Render to filename]: -W -o\"CompilerOutput.wav\"" + newline +
                   "[Render to file using csd/orc/sco name]: -W -o\"*.wav\"" + newline +
                   "[Render to file asking for its name]: -W -o\"?.wav\"";
        }


        public void CreateDefaultTemplates()
        {
            this.Templates.CSound =
                    "<CsoundSynthesizer>" + newline + newline + newline +
                    "<CsOptions>" + newline + newline +
                    "</CsOptions>" + newline + newline + newline +
                    "<CsInstruments>" + newline + newline +
                    "</CsInstruments>" + newline + newline + newline +
                    "<CsScore>" + newline + newline +
                    "</CsScore>" + newline + newline + newline +
                    "</CsoundSynthesizer>";

            this.Templates.Python = "";
            this.Templates.Lua = "";

            this.Templates.Cabbage =
                    "<Cabbage>" + newline +
                    "</Cabbage>" + newline + newline +
                    "<CsoundSynthesizer>" + newline + newline +
                    "<CsOptions>" + newline +
                    "-d -n" + newline +
                    "</CsOptions>" + newline + newline +
                    "<CsInstruments>" + newline +
                    "sr = 44100" + newline +
                    "ksmps = 32" + newline +
                    "nchnls = 2" + newline + newline +
                    "instr 1" + newline +
                    "endin" + newline +
                    "</CsInstruments>" + newline + newline +
                    "<CsScore>" + newline +
                    "i1 0 1000" + newline +
                    "</CsScore>" + newline + newline +
                    "</CsoundSynthesizer>";


        }

        public void CreateDefaultCSoundStyles()
        {
            //CSOUND DEFAULT STYLES
            //int StyleNumber, int Fore, int Back, 
            //bool Bold, bool Italic, int Alpha, 
            //bool EolFilled, string FriendlyName
            //CSOUND:
            this.EditorProperties.CSoundStyles = new List<string>();

            //DEFAULT STYLE "32"
            this.EditorProperties.CSoundStyles.Add(
                "32," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,DefaultStyle");

            //WHITE SPACE - (SCE_C_DEFAULT 0)
            this.EditorProperties.CSoundStyles.Add(
                "0," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,WhiteSpace");

            //Comment Multi /**/ - (SCE_C_COMMENT 1)   
            this.EditorProperties.CSoundStyles.Add(
                "1," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentMulti");

            //Comment Line ";" - (SCE_C_COMMENTLINE 2)
            this.EditorProperties.CSoundStyles.Add(
                "2," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentLine");

            //Numbers color - (SCE_C_NUMBER 4)
            this.EditorProperties.CSoundStyles.Add(
                "4," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Numbers");

            //Keyword(0): OPCODES - (SCE_C_WORD 5)
            this.EditorProperties.CSoundStyles.Add(
                "5," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Opcodes");

            //Double quoted string - (SCE_C_STRING 6)
            this.EditorProperties.CSoundStyles.Add(
                "6," +
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,DoubleQuotedString");

            //Preprocessor and Macro - (SCE_C_PREPROCESSOR 9)
            this.EditorProperties.CSoundStyles.Add(
                "9," +
                ColorTranslator.ToWin32(Color.FromArgb(160, 50, 160)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,PreprocessorAndMacro");

            //Operator [=] - (SCE_C_OPERATOR 10)
            this.EditorProperties.CSoundStyles.Add(
                 "10," +
                 ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                 ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                 "False,False,256,False,Operator=");

            //CSD Tags, Keyword(1) - (SCE_C_WORD2 16)
            this.EditorProperties.CSoundStyles.Add(
                "16," +
                ColorTranslator.ToWin32(Color.FromArgb(220, 0, 110)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CsdTags");

            //UserOpcodes - (SCE_C_WORD3 20)
            this.EditorProperties.CSoundStyles.Add(
                "20," +
                ColorTranslator.ToWin32(Color.FromArgb(160, 50, 160)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,UserOpcodes");

            //INSTR-ENDIN color - Keyword(3) - (SCE_WXOUND_WORD4 21)
            this.EditorProperties.CSoundStyles.Add(
                "21," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,InstrEndin");

            //DEFAULT STYLE "33" STYLE_NUMBERS_MARGINS
            this.EditorProperties.CSoundStyles.Add(
                "33," +
                ColorTranslator.ToWin32(Color.FromArgb(130, 130, 130)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(250, 250, 250)).ToString() + "," +
                "False,False,256,False,Margins");

            //Styles 0-255 for Scintilla
            //Style 256 = Text Selection
            //Style 257 = Bookmarks
            //Style 258 = Vertical Ruler
            //Style 259 = Caret Line Marker
            this.EditorProperties.CSoundStyles.Add(
                "256," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(210, 210, 210)).ToString() + "," +
                "False,False,256,False,TextSelection");
            this.EditorProperties.CSoundStyles.Add(
                "257," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                "False,False,40,False,Bookmarks");
            this.EditorProperties.CSoundStyles.Add(
                "258," +
                ColorTranslator.ToWin32(Color.FromArgb(192, 192, 192)).ToString() + "," +
                "0,False,False,256,False,VerticalRuler");
            this.EditorProperties.CSoundStyles.Add(
                "259," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 100)).ToString() + "," +
                "0,False,False,256,False,CaretLineMarker");

        }

        public void CreateDefaultPythonStyles()
        {
            //DEFAULT PYTHON STYLES
            //int StyleNumber, int Fore, int Back, 
            //bool Bold, bool Italic, int Alpha, 
            //bool EolFilled, string FriendlyName
            //PYTHON:
            this.EditorProperties.PythonStyles = new List<string>();

            //DEFAULT STYLE "32"
            this.EditorProperties.PythonStyles.Add(
                "32," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,DefaultStyle");

            //White Space - (SCE_P_DEFAULT 0)
            this.EditorProperties.PythonStyles.Add(
                "0," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,WhiteSpace");

            //Comment Line "#"  - (SCE_P_COMMENTLINE 1)
            this.EditorProperties.PythonStyles.Add(
                "1," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentLine");

            //Numbers - (SCE_P_NUMBER 2)
            this.EditorProperties.PythonStyles.Add(
                "2," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 127, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Numbers");

            //Strings "'" - (SCE_P_STRING 3)
            this.EditorProperties.PythonStyles.Add(
                "3," +
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Strings");

            //Single quoted string - (SCE_P_CHARACTER 4)
            this.EditorProperties.PythonStyles.Add(
                "4," +
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,StringsSingle");

            //Keyword(0) - (SCE_P_WORD 5) - 0,0,127 + BOLD
            this.EditorProperties.PythonStyles.Add(
                "5," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "True,False,256,False,Keywords");

            //Triple quoted string - (SCE_P_TRIPLE 6)
            this.EditorProperties.PythonStyles.Add(
                "6," +
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,StringsTriple");

            //TripleDouble quoted string - (SCE_P_TRIPLEDOUBLE 7)
            this.EditorProperties.PythonStyles.Add(
                "7," +
                ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,StringsTripleD");

            //Class name definition - (SCE_P_CLASSNAME 8) - 0,0,255 + BOLD
            this.EditorProperties.PythonStyles.Add(
                "8," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "True,False,256,False,Classes");

            //Function or method name definition - (SCE_P_DEFNAME 9) -  0,127,127 + BOLD
            this.EditorProperties.PythonStyles.Add(
                "9," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 127, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "True,False,256,False,Functions");

            //Operators - (SCE_P_OPERATOR 10) - TextForeColor + BOLD
            this.EditorProperties.PythonStyles.Add(
                 "10," +
                 ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                 ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                 "True,False,256,False,Operators");

            //all words - ( SCE_P_IDENTIFIER 11)
            this.EditorProperties.PythonStyles.Add(
                "11," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Words");

            //Comment-blocks - ( SCE_P_COMMENTBLOCK 12)
            this.EditorProperties.PythonStyles.Add(
                "12," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentBlocks");

            //End of line where string is not closed - ( SCE_P_STRINGEOL 13)
            this.EditorProperties.PythonStyles.Add(
                "13," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(224, 192, 224)).ToString() + "," +
                "False,False,256,False,EolStringNotClosed");

            //Highlighted identifiers - ( SCE_P_WORD2 14)
            this.EditorProperties.PythonStyles.Add(
                "14," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Identifiers");

            //Decorators @dec1 - (SCE_C_WORD2 15)
            this.EditorProperties.PythonStyles.Add(
                "15," +
                ColorTranslator.ToWin32(Color.FromArgb(128, 80, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Decorators");



            //DEFAULT STYLE "33" STYLE_NUMBERS_MARGINS
            this.EditorProperties.PythonStyles.Add(
                "33," +
                ColorTranslator.ToWin32(Color.FromArgb(130, 130, 130)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(250, 250, 250)).ToString() + "," +
                "False,False,256,False,Margins");

            //Styles 0-255 for Scintilla
            //Style 256 = Text Selection
            //Style 257 = Bookmarks
            //Style 258 = Vertical Ruler
            //Style 259 = Caret Line Marker
            //Style 260 = Line Numbers
            this.EditorProperties.PythonStyles.Add(
                "256," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(210, 210, 210)).ToString() + "," +
                "False,False,256,False,TextSelection");
            this.EditorProperties.PythonStyles.Add(
                "257," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                "False,False,40,False,Bookmarks");
            this.EditorProperties.PythonStyles.Add(
                "258," +
                ColorTranslator.ToWin32(Color.FromArgb(192, 192, 192)).ToString() + "," +
                "0,False,False,256,False,VerticalRuler");
            this.EditorProperties.PythonStyles.Add(
                "259," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 100)).ToString() + "," +
                "0,False,False,256,False,CaretLineMarker");

        }

        public void CreateDefaultLuaStyles()
        {
            //DEFAULT LUA STYLES
            //int StyleNumber, int Fore, int Back, 
            //bool Bold, bool Italic, int Alpha, 
            //bool EolFilled, string FriendlyName
            //LUA:
            this.EditorProperties.LuaStyles = new List<string>();

            //DEFAULT STYLE "32"
            this.EditorProperties.LuaStyles.Add(
                "32," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,DefaultStyle");

            //White Space
            this.EditorProperties.LuaStyles.Add(
                "0," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,WhiteSpace");

            //Comment Block
            this.EditorProperties.LuaStyles.Add(
                "1," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentBlock");

            //Comment Line
            this.EditorProperties.LuaStyles.Add(
                "2," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 150, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,CommentLine");

            ////Doc Comment
            //this.EditorProperties.LuaStyles.Add(
            //    "3," +
            //    ColorTranslator.ToWin32(Color.FromArgb(150, 0, 0)).ToString() + "," +
            //    ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
            //    "False,False,256,False,DocComment");

            //Numbers
            this.EditorProperties.LuaStyles.Add(
                "4," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 127, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Numbers");

            //Keyword(0) - 0,0,127 + BOLD
            this.EditorProperties.LuaStyles.Add(
                "5," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 230)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Double quoted string //127,0,127
            this.EditorProperties.LuaStyles.Add(
                "6," +
                ColorTranslator.ToWin32(Color.FromArgb(127, 0, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Strings");

            //Single quoted string //127,0,127
            this.EditorProperties.LuaStyles.Add(
                "7," +
                ColorTranslator.ToWin32(Color.FromArgb(127, 0, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,StringsSingle");

            //Literal string  //fore: 127,0,127 - back: 200,255,255
            this.EditorProperties.LuaStyles.Add(
                "8," +
                ColorTranslator.ToWin32(Color.FromArgb(127, 0, 127)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(200, 255, 255)).ToString() + "," +
                "False,False,256,False,LiteralString");

            //Preprocessor -  //127,127,0
            this.EditorProperties.LuaStyles.Add(
                "9," +
                ColorTranslator.ToWin32(Color.FromArgb(127, 127, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "True,False,256,False,Preprocessor");

            //Operators - TextForeColor
            this.EditorProperties.LuaStyles.Add(
                 "10," +
                 ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                 ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                 "False,False,256,False,Operators");

            //all words
            this.EditorProperties.LuaStyles.Add(
                "11," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                "False,False,256,False,Words");

            //End of line where string is not closed
            this.EditorProperties.LuaStyles.Add(
                "12," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(224, 192, 224)).ToString() + "," +
                "False,False,256,False,EolStringNotClosed");

            //Other Keywords
            //style.lua.13=$(style.lua.5),back:#F5FFF5
            //style.lua.14=$(style.lua.5),back:#F5F5FF
            //style.lua.15=$(style.lua.5),back:#FFF5F5
            //style.lua.16=$(style.lua.5),back:#FFF5FF
            //style.lua.17=$(style.lua.5),back:#FFFFF5
            //style.lua.18=$(style.lua.5),back:#FFA0A0
            //style.lua.19=$(style.lua.5),back:#FFF5F5

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "13," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(245, 255, 245)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "14," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(245, 245, 255)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "15," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 245, 245)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "16," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 245, 255)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "17," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 245)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "18," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 160, 160)).ToString() + "," +
                "False,False,256,False,Keywords");

            //Keywords
            this.EditorProperties.LuaStyles.Add(
                "19," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 245, 245)).ToString() + "," +
                "False,False,256,False,Keywords");


            //////////////////////////////////////////
            //DEFAULT STYLE "33" STYLE_NUMBERS_MARGINS
            this.EditorProperties.LuaStyles.Add(
                "33," +
                ColorTranslator.ToWin32(Color.FromArgb(130, 130, 130)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(250, 250, 250)).ToString() + "," +
                "False,False,256,False,Margins");

            //Styles 0-255 for Scintilla
            //Style 256 = Text Selection
            //Style 257 = Bookmarks
            //Style 258 = Vertical Ruler
            //Style 259 = Caret Line Marker
            //Style 260 = Line Numbers
            this.EditorProperties.LuaStyles.Add(
                "256," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 0)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(210, 210, 210)).ToString() + "," +
                "False,False,256,False,TextSelection");
            this.EditorProperties.LuaStyles.Add(
                "257," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 255)).ToString() + "," +
                ColorTranslator.ToWin32(Color.FromArgb(0, 0, 255)).ToString() + "," +
                "False,False,40,False,Bookmarks");
            this.EditorProperties.LuaStyles.Add(
                "258," +
                ColorTranslator.ToWin32(Color.FromArgb(192, 192, 192)).ToString() + "," +
                "0,False,False,256,False,VerticalRuler");
            this.EditorProperties.LuaStyles.Add(
                "259," +
                ColorTranslator.ToWin32(Color.FromArgb(255, 255, 100)).ToString() + "," +
                "0,False,False,256,False,CaretLineMarker");

        }
        ///////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////





        ///////////////////////////////////////////////////////////////////////////////
        // RECENT FILES
        ///////////////////////////////////////////////////////////////////////////////
        private List<string> RecentFilesCheckList(List<string> list)
        {
            try
            {
                foreach (string s in list)
                {
                    if (!File.Exists(s))
                    {
                        list.Remove(s);
                    }
                }

                RecentFilesRemoveExcess(ref list);

                return list;
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(
                    ex.Message, "wxSettings.RecentFilesCheckList");
                return new List<string>();
            }
        }

        private void RecentFilesRemoveExcess(ref List<string> list)
        {
            try
            {
                if (list.Count > MAX_RECENT_FILES)
                {
                    Int32 count = list.Count - MAX_RECENT_FILES;
                    list.RemoveRange(MAX_RECENT_FILES, count);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxSettings.RecentFilesRemoveExcess error: " +
                    ex.Message);
            }
        }

        public void RecentFilesInsert(string mFileName)
        {
            try
            {
                if (File.Exists(mFileName))
                {

                    if (RecentFiles.Contains(mFileName))
                    {
                        RecentFiles.Remove(mFileName);
                    }

                    RecentFiles.Insert(0, mFileName);
                    RecentFilesRemoveExcess(ref RecentFiles);
                }
            }

            catch (Exception ex)
            {
                wxGlobal.wxMessageError(
                      ex.Message, "wxSettings.RecentFilesInsert");
            }
        }



        ///////////////////////////////////////////////////////////////////////////////
        // LAST SESSION FILES
        ///////////////////////////////////////////////////////////////////////////////
        public void LastSessionFilesClear()
        {
            LastSessionFiles.Clear();
        }
        public void LastSessionFilesInsert(string mFileName)
        {
            try
            {
                /*
                if(Glib::file_test(mFileName, Glib::FILE_TEST_EXISTS))
                {
                    std::string temp = mFileName;
                    LastSessionFiles.insert(LastSessionFiles.begin(), temp);
                }
                */
                if (File.Exists(mFileName))
                {
                    LastSessionFiles.Add(mFileName);
                }

            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                           "wxSettings.LastSessionFilesInsert error: " +
                           ex.Message);
            }
        }








        ///////////////////////////////////////////////////////////////////////////////
        // ENVIRONMENT VARIABLES
        ///////////////////////////////////////////////////////////////////////////////
        public void SetEnvironmentVariables()
        {
            //SET ENVIRONMENT VARIABLES
            try
            {
                System.Environment.SetEnvironmentVariable(
                    "SFDIR", this.Directory.SFDIR);
                System.Environment.SetEnvironmentVariable(
                    "SSDIR", this.Directory.SSDIR);
                System.Environment.SetEnvironmentVariable(
                    "SADIR", this.Directory.SADIR);
                System.Environment.SetEnvironmentVariable(
                    "MFDIR", this.Directory.MFDIR);
                System.Environment.SetEnvironmentVariable(
                    "INCDIR", this.Directory.INCDIR);

                if (!string.IsNullOrEmpty(this.Directory.OPCODEDIR))
                {
                    System.Environment.SetEnvironmentVariable(
                        "OPCODEDIR", this.Directory.OPCODEDIR);
                    System.Environment.SetEnvironmentVariable(
                        "OPCODEDIR64", this.Directory.OPCODEDIR);
                }

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message,
                        "wxSettings.SetEnvironmentVariable Error");
            }
        }






    }
}
