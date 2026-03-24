using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace WinXound_Net
{
    class wxRegistry
    {

        private const Int32 SHCNE_ASSOCCHANGED = 0x8000000;
        private const Int32 SHCNF_IDLIST = (int)0x0L;

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(HChangeNotifyEventID wEventId,
                                          HChangeNotifyFlags uFlags,
                                          IntPtr dwItem1,
                                          IntPtr dwItem2);



        //mRegister.RegisterExtension("CSoundCSD", ".csd", "CSound CSD file",
        //                            Application.StartupPath + "\\Icons\\csd.ico",
        //                            "WinXound_Net.exe", null);
        public void RegisterExtensionAdmin(string Identifier,
                                           string FileExtension,
                                           string FileType,
                                           string IconFileName,
                                           string OpenWith,
                                           string Parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(Identifier)) return;

                string sKeyName = null;
                // Holds Key Name in registry. 
                string sKeyValue = null;
                // Holds Key Value in registry. 
                //Dim ret As Int32 ' Holds error status if any from API calls. 
                //Dim lphKey As Int32 ' Holds key handle from RegCreateKey. 
                string path = null;
                RegistryKey RK = default(RegistryKey);

                //path = App.path 
                path = Application.StartupPath;
                //if (Strings.Right(path, 1) != "\\")
                if (!path.EndsWith("\\"))
                {
                    path = path + "\\";
                }

                // This creates a Root entry that called as the string of AppTitle. 
                //sKeyName = AppTitle 
                //sKeyValue = FileType 
                //ret& = RegCreateKey&(HKEY_CLASSES_ROOT, sKeyName, lphKey&) 
                //ret& = RegSetValue&(lphKey&, "", REG_SZ, sKeyValue, 0&) 

                RK = Registry.ClassesRoot.CreateSubKey(Identifier);
                RK.SetValue("", FileType);


                // This creates a Root entry called as the string of FileExtension associated with AppTitle. 
                //sKeyName = FileExtension 
                //sKeyValue = AppTitle 
                //ret& = RegCreateKey&(HKEY_CLASSES_ROOT, sKeyName, lphKey&) 
                //ret& = RegSetValue&(lphKey&, "", REG_SZ, sKeyValue, 0&) 

                RK = Registry.ClassesRoot.CreateSubKey(FileExtension);
                RK.SetValue("", Identifier);
                RK.Close();


                // This sets the command line for AppTitle. 
                sKeyName = Identifier;
                if (!string.IsNullOrEmpty(Parameters))
                {
                    sKeyValue = "\"" + path + OpenWith + "\"" + " " + Parameters.Trim();
                }
                else
                {
                    //'sKeyValue = path & OpenWith & " %1" 
                    sKeyValue = "\"" + path + OpenWith + "\"" + " " + "\"" + "%1" + "\"";
                }
                //ret& = RegCreateKey&(HKEY_CLASSES_ROOT, sKeyName, lphKey&) 
                //ret& = RegSetValue&(lphKey&, "shell\open\command", REG_SZ, _ 
                // sKeyValue, MAX_PATH) 

                RK = Registry.ClassesRoot.OpenSubKey(Identifier, true);
                RK.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", sKeyValue);
                //RK.SetValue("shell\open\command", sKeyValue) 


                // This sets the icon for the file extension 
                if (!string.IsNullOrEmpty(IconFileName))
                {
                    sKeyName = Identifier;
                    sKeyValue = IconFileName;
                    //ret& = RegCreateKey&(HKEY_CLASSES_ROOT, sKeyName, lphKey&) 
                    //ret& = RegSetValue&(lphKey&, "DefaultIcon", REG_SZ, sKeyValue, MAX_PATH) 
                    //RK = Registry.ClassesRoot.OpenSubKey(AppTitle, True) 
                    RK.CreateSubKey("DefaultIcon").SetValue("", sKeyValue);
                }

                RK.Close();


                // This notifies the shell that the icon has changed 
                //SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED,
                               HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxRegistry - RegisterExtension");
            }
        }



        //mRegister.UnRegisterExtension("CSoundCSD", ".csd");
        public void UnRegisterExtensionAdmin(string Identifier, string FileExtension)
        {
            try
            {
                //Delete all keys 
                Registry.ClassesRoot.DeleteSubKey(FileExtension);
                Registry.ClassesRoot.DeleteSubKey(Identifier + "\\DefaultIcon");
                Registry.ClassesRoot.DeleteSubKey(Identifier + "\\Shell\\Open\\Command");
                Registry.ClassesRoot.DeleteSubKey(Identifier + "\\Shell\\Open");
                Registry.ClassesRoot.DeleteSubKey(Identifier + "\\Shell");
                Registry.ClassesRoot.DeleteSubKey(Identifier);

                //Notify shell on the delete and refresh the icons 
                SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED,
                    HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxRegistry - UnRegisterExtension");
            }
        }







        //mRegister.RegisterExtension("CSoundCSD", ".csd", "CSound CSD file",
        //                            Application.StartupPath + "\\Icons\\csd.ico",
        //                            "WinXound_Net.exe", null);
        public void RegisterExtensionUser(string Identifier,
                                          string FileExtension,
                                          string FileType,
                                          string IconFileName,
                                          string OpenWith,
                                          string Parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(Identifier)) return;

                string sKeyName = "";
                string sKeyValue = "";
                string path = "";
                RegistryKey RK = default(RegistryKey);


                //Application path 
                path = Application.StartupPath;
                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }


                // This creates an entry for FileExtension 
                // associated with Identifier. 
                // example= .xyz
                RK = Registry.CurrentUser.CreateSubKey(
                            @"Software\Classes\" + FileExtension);
                RK.SetValue("", Identifier);
                RK.Close();


                // This creates an entry for Identifier. 
                RK = Registry.CurrentUser.CreateSubKey(
                            @"Software\Classes\" + Identifier);
                RK.SetValue("", FileType); //Description of the file


                // This sets the command line for AppTitle. 
                sKeyName = Identifier;
                if (!string.IsNullOrEmpty(Parameters))
                {
                    sKeyValue = "\"" + path + OpenWith + "\"" + " " +
                                Parameters.Trim();
                }
                else
                {
                    sKeyValue = "\"" + path + OpenWith + "\"" + " " +
                                "\"" + "%1" + "\"";
                }

                RK = Registry.CurrentUser.OpenSubKey(
                            @"Software\Classes\" + Identifier, true);
                RK.CreateSubKey("shell").CreateSubKey("open")
                    .CreateSubKey("command").SetValue("", sKeyValue);
                //RK.SetValue("shell\open\command", sKeyValue) 


                // This sets the icon for the file extension 
                if (!string.IsNullOrEmpty(IconFileName))
                {
                    RK.CreateSubKey("DefaultIcon").SetValue("", IconFileName);
                }

                RK.Close();  //Close Identifier Registry Class


                // This notifies the shell that the icon has changed 
                //SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED,
                               HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxRegistry - RegisterExtensionUser");
            }
        }


        //mRegister.UnRegisterExtension("CSoundCSD", ".csd");
        public void UnRegisterExtensionUser(string Identifier, string FileExtension)
        {
            try
            {
                //Delete all keys 
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + FileExtension);
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + Identifier + "\\DefaultIcon");
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + Identifier + "\\Shell\\Open\\Command");
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + Identifier + "\\Shell\\Open");
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + Identifier + "\\Shell");
                Registry.CurrentUser.DeleteSubKey(
                    @"Software\Classes\" + Identifier);

                //Notify shell on the delete and refresh the icons 
                SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED,
                    HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "wxRegistry - UnRegisterExtensionUser");
            }
        }




        #region enum HChangeNotifyEventID
        /// <summary>
        /// Describes the event that has occurred. 
        /// Typically, only one event is specified at a time. 
        /// If more than one event is specified, the values contained 
        /// in the <i>dwItem1</i> and <i>dwItem2</i> 
        /// parameters must be the same, respectively, for all specified events. 
        /// This parameter can be one or more of the following values. 
        /// </summary>
        /// <remarks>
        /// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index 
        /// in the system image list that has changed. 
        /// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
        /// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index 
        /// in the system image list that has changed. 
        /// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
        /// </remarks>
        [Flags]
        enum HChangeNotifyEventID
        {
            /// <summary>
            /// All events have occurred. 
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,

            /// <summary>
            /// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> 
            /// must be specified in the <i>uFlags</i> parameter. 
            /// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>. 
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,

            /// <summary>
            /// The attributes of an item or folder have changed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the item or folder that has changed. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,

            /// <summary>
            /// A nonfolder item has been created. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the item that was created. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A nonfolder item has been deleted. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the item that was deleted. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_DELETE = 0x00000004,

            /// <summary>
            /// A drive has been added. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive that was added. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,

            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive that was added. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,

            /// <summary>
            /// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,

            /// <summary>
            /// Not currently used. 
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,

            /// <summary>
            /// The amount of free space on a drive has changed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive on which the free space changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,

            /// <summary>
            /// Storage media has been inserted into a drive. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive that contains the new media. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,

            /// <summary>
            /// Storage media has been removed from a drive. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the root of the drive from which the media was removed. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,

            /// <summary>
            /// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> 
            /// or <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the folder that was created. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_MKDIR = 0x00000008,

            /// <summary>
            /// A folder on the local computer is being shared via the network. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the folder that is being shared. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,

            /// <summary>
            /// A folder on the local computer is no longer being shared via the network. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the folder that is no longer being shared. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,

            /// <summary>
            /// The name of a folder has changed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder. 
            /// <i>dwItem2</i> contains the new PIDL or name of the folder. 
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,

            /// <summary>
            /// The name of a nonfolder item has changed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the previous PIDL or name of the item. 
            /// <i>dwItem2</i> contains the new PIDL or name of the item. 
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,

            /// <summary>
            /// A folder has been removed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the folder that was removed. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_RMDIR = 0x00000010,

            /// <summary>
            /// The computer has disconnected from a server. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the server from which the computer was disconnected. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,

            /// <summary>
            /// The contents of an existing folder have changed, 
            /// but the folder still exists and has not been renamed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or 
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>. 
            /// <i>dwItem1</i> contains the folder that has changed. 
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>. 
            /// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or 
            /// SHCNE_RENAMEFOLDER, respectively, instead. 
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,

            /// <summary>
            /// An image in the system image list has changed. 
            /// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>. 
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000,

        }
        #endregion // enum HChangeNotifyEventID

        #region public enum HChangeNotifyFlags
        /// <summary>
        /// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters. 
        /// The uFlags parameter must be one of the following values.
        /// </summary>
        [Flags]
        public enum HChangeNotifyFlags
        {
            /// <summary>
            /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values. 
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that 
            /// represent the item(s) affected by the change. 
            /// Each ITEMIDLIST must be relative to the desktop folder. 
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of 
            /// maximum length MAX_PATH that contain the full path names 
            /// of the items affected by the change. 
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of 
            /// maximum length MAX_PATH that contain the full path names 
            /// of the items affected by the change. 
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that 
            /// represent the friendly names of the printer(s) affected by the change. 
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that 
            /// represent the friendly names of the printer(s) affected by the change. 
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// The function should not return until the notification 
            /// has been delivered to all affected components. 
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components 
            /// but should return as soon as the notification process has begun. 
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }
        #endregion // enum HChangeNotifyFlags


    }
}
