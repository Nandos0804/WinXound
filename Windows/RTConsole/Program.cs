using System;
using System.Runtime.InteropServices;
using System.Text;



namespace WinXound_RTConsole
{
    class Program
    {


        [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
        private static extern bool PeekNamedPipe(IntPtr handle,
            byte[] buffer, uint nBufferSize, ref uint bytesRead,
            ref uint bytesAvail, ref uint BytesLeftThisMessage);

        [DllImport("coredll.dll", EntryPoint = "memset", SetLastError = false)]
        private static extern void memset(IntPtr dest, int c, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr handle,
            byte[] buffer, uint toRead, ref uint read, IntPtr lpOverLapped);

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
           ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        //[StructLayout(LayoutKind.Sequential)]
        //internal struct PROCESS_INFORMATION
        //{
        //    public IntPtr hProcess;
        //    public IntPtr hThread;
        //    public int dwProcessId;
        //    public int dwThreadId;
        //}


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }


        [Flags]
        public enum STARTF : uint
        {
            STARTF_USESHOWWINDOW = 0x00000001,
            STARTF_USESIZE = 0x00000002,
            STARTF_USEPOSITION = 0x00000004,
            STARTF_USECOUNTCHARS = 0x00000008,
            STARTF_USEFILLATTRIBUTE = 0x00000010,
            STARTF_RUNFULLSCREEN = 0x00000020,  // ignored for non-x86 platforms
            STARTF_FORCEONFEEDBACK = 0x00000040,
            STARTF_FORCEOFFFEEDBACK = 0x00000080,
            STARTF_USESTDHANDLES = 0x00000100,
        }

        public enum ShowState : int
        {

            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11


        }

        private int WAIT_TIMEOUT = 0x00000102;
        private int WAIT_OBJECT_0 = 0;
        private int WAIT_ABANDONED = 0x00000080;



        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName,
           string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
           uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
           [In] ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateConsoleScreenBuffer(uint dwDesiredAccess,
           uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, uint dwFlags,
           IntPtr lpScreenBufferData);

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const int FILE_SHARE_READ = 1;
        const int FILE_SHARE_WRITE = 0x00000002;
        const int CONSOLE_TEXTMODE_BUFFER = 1;
        const Int64 INVALID_HANDLE_VALUE = -1;

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {

            public short X;
            public short Y;

        }

        const int ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;

        public struct SMALL_RECT
        {

            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

        }

        public struct CONSOLE_SCREEN_BUFFER_INFO
        {

            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;

        }


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleScreenBufferSize(
            IntPtr hConsoleOutput,
            COORD dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            [In] ref SMALL_RECT lpConsoleWindow);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FillConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            char cCharacter,
            uint nLength,
            COORD dwWriteCoord,
            out int lpNumberOfCharsWritten); //uint


        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetStdHandle(
            uint nStdHandle,
            IntPtr hHandle);



        const int STARTF_FORCEOFFFEEDBACK = 0x00000080;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleCursorPosition(
            IntPtr hConsoleOutput,
           COORD dwCursorPosition);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] StringBuilder lpCharacter,
            uint nLength,
            COORD dwReadCoord,
            out int lpNumberOfCharsRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] Byte[] lpCharacter,
            uint nLength,
            COORD dwReadCoord,
            out int lpNumberOfCharsRead);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);


        const int SLEEP_TIME = 1;



        [DllImport("kernel32.dll")]
        static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer,
            uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten,
            IntPtr lpOverlapped);
        //[In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        static extern bool WriteFile(IntPtr hFile, System.Text.StringBuilder lpBuffer,
            uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten,
            IntPtr lpOverlapped);





        static void Main(string[] args)
        {
            string commandLine = "";

            COORD origin = new COORD(); // { 0, 0 };
            origin.X = 0; origin.Y = 0;


            bool bSuccess = false;
            // get pipe/console to output to
            IntPtr hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
            Int32 dwDummy;

            ////parse command line : skip to RTconsole's arguments
            ////string[] args = System.Environment.GetCommandLineArgs();
            ////check if args command line is present
            try
            {
                if (args.Length > 0)
                {
                    commandLine = "\"" + args[0].Trim() + "\" "; //Compiler name

                    if (args[0].ToLower().Contains("csound"))
                    {
                        for (Int32 i = 1; i < args.Length; i++)
                        {
                            if (args[i].StartsWith("-o"))
                            {
                                string tFilename = args[i].Replace("-o", "");
                                args[i] = "-o\"" + tFilename + "\"";
                                break;
                            }
                        }
                    }

                    //Now we must check if this is an analysis tool call !!!
                    if (args.Length > 1)
                    {
                        //Console.WriteLine(args[1]);
                        //Orc/Sco compiler: two filenames (we have to delete the [ORCSCO] arg)
                        if (args[1].Trim() == "[ORCSCO]")
                        {
                            //Console.WriteLine("[ORCSCO]");
                            for (int i = 2; i < args.Length - 2; i++)
                            {
                                commandLine += args[i] + " ";  //Flags
                            }
                            commandLine += "\"" + args[args.Length - 2] + "\"" + " "; //Input Filename
                            commandLine += "\"" + args[args.Length - 1] + "\""; //Output Filename
                        }
                        //Analysis compiler: two filenames
                        else if (args[1].Trim() == "[ANALYSIS]")
                        {
                            //Console.WriteLine("[ANALYSIS]");
                            for (int i = 2; i < args.Length - 2; i++)
                            {
                                commandLine += args[i] + " ";  //Flags
                            }
                            commandLine += "\"" + args[args.Length - 2] + "\"" + " "; //Input Filename
                            commandLine += "\"" + args[args.Length - 1] + "\""; //Output Filename
                        }
                        //Normal compiler: one filename
                        else
                        {
                            //Console.WriteLine("[NORMAL]");
                            for (int i = 1; i < args.Length - 1; i++)
                            {
                                commandLine += args[i] + " ";  //Flags
                            }
                            commandLine += "\"" + args[args.Length - 1] + "\""; //Filename
                        }
                    }

                }
                else
                    return;
            }
            catch (Exception ex)
            {
                return;
            }

            //Print the Commandline info
            Console.WriteLine("WinXound Commandline Info:");
            Console.WriteLine(commandLine);
            Console.WriteLine("");
            Console.WriteLine("------- Compiler Start -------");

            // prepare the console window & inherited screen buffer
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = 1; //TRUE

            IntPtr hConsole =
                CreateConsoleScreenBuffer(GENERIC_READ | GENERIC_WRITE,
                                          FILE_SHARE_READ | FILE_SHARE_WRITE,
                                          ref sa, CONSOLE_TEXTMODE_BUFFER, IntPtr.Zero);
            if (hConsole == IntPtr.Zero)
                return;

            bSuccess = SetConsoleMode(hConsole, ENABLE_WRAP_AT_EOL_OUTPUT);
            if (!bSuccess)
                return;

            COORD dim = GetLargestConsoleWindowSize(hConsole);

            bSuccess = SetConsoleScreenBufferSize(hConsole, dim);
            if (!bSuccess)
                return;

            SMALL_RECT sr = new SMALL_RECT(); //{ 0, 0, dim.X - 1, dim.Y - 1 };
            sr.Left = 0;
            sr.Top = 0;
            sr.Right = (short)(dim.X - 1);
            sr.Bottom = (short)(dim.Y - 1);

            bSuccess = SetConsoleWindowInfo(hConsole, true, ref sr);
            if (!bSuccess)
                return;

            // fill screen buffer with zeroes
            bSuccess =
                FillConsoleOutputCharacter(hConsole, '\0',
                                           (uint)(dim.X * dim.Y),
                                           origin, out dwDummy);
            if (!bSuccess)
                return;

            //STD_OUTPUT_HANDLE
            bSuccess = SetStdHandle(unchecked((uint)STD_OUTPUT_HANDLE), hConsole); // to be inherited by child process
            if (!bSuccess)
                return;

            // start the subprocess
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            //ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));
            STARTUPINFO si = new STARTUPINFO();
            //ZeroMemory(&si, sizeof(STARTUPINFO));
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));
            si.dwFlags = STARTF_FORCEOFFFEEDBACK; // we don't want the "app starting" cursor


            //commandLine = _T("csound \"C:\\Users\\Teto\\Desktop\\File Esempio CSound\\trapped.csd\"");


            SECURITY_ATTRIBUTES saTemp = new SECURITY_ATTRIBUTES();
            // all other default options are already good : we want subprocess to share the same console and to inherit our STD handles
            if (!CreateProcess(null, commandLine,
                               ref saTemp, ref saTemp, true, 0, IntPtr.Zero, null,
                               ref si, out pi))
            {
                CloseHandle(hConsole);
                return;
            }
            CloseHandle(pi.hThread); // always close the hThread after a CreateProcess


            COORD dimensions = new COORD();  //(80, 4000);
            dimensions.X = 80; dimensions.Y = 4000;
            SetConsoleScreenBufferSize(hConsole, dimensions);



            // X : columns, Y : rows
            COORD lastpos = new COORD(); //{ 0, 0 };
            lastpos.X = 0; lastpos.Y = 0;

            CONSOLE_SCREEN_BUFFER_INFO csbi;
            short sLimitVert = (short)((dim.Y / 3) * 2);
            bool exitNow = false;
            do
            {
                if (WaitForSingleObject(pi.hProcess, 0) != 0x00000102) //WAIT_TIMEOUT
                    exitNow = true; // exit after this last iteration

                // get screen buffer state
                bSuccess = GetConsoleScreenBufferInfo(hConsole, out csbi);
                if (!bSuccess)
                    break;

                if ((csbi.dwCursorPosition.X == lastpos.X) && (csbi.dwCursorPosition.Y == lastpos.Y))
                    //Sleep(SLEEP_TIME); // text cursor did not move, sleep a while
                    System.Threading.Thread.Sleep(SLEEP_TIME);
                else
                {
                    Int32 count = 0;

                    if (csbi.dwCursorPosition.Y > lastpos.Y)
                        count = dim.X - lastpos.X;

                    if (csbi.dwCursorPosition.Y >= sLimitVert)
                    {
                        // Until the end of the buffer
                        SetConsoleCursorPosition(hConsole, origin);

                        count += (dim.Y - lastpos.Y - 1) * dim.X;
                    }
                    else
                    {
                        // Until the cursor
                        if (csbi.dwCursorPosition.Y > lastpos.Y)
                        {
                            count += csbi.dwCursorPosition.X;
                            count += (csbi.dwCursorPosition.Y - lastpos.Y - 1) * dim.X;
                        }
                        else
                        {
                            count += csbi.dwCursorPosition.X - lastpos.X;
                        }
                    }

                    // read newly output characters starting from last cursor position
                    //StringBuilder buffer = new StringBuilder();//(count * Marshal.SizeOf(typeof(StringBuilder)));
                    //(LPTSTR)LocalAlloc(0, count * sizeof(TCHAR));
                    byte[] buffer = new byte[count * sizeof(byte)];
                    if (buffer == null)
                        break;

                    bSuccess =
                        ReadConsoleOutputCharacter(hConsole, buffer,
                                    (uint)count, lastpos, out count);
                    if (!bSuccess)
                        break;

                    // fill screen buffer with zeroes
                    bSuccess =
                        FillConsoleOutputCharacter(hConsole, '\0',
                                    (uint)count, lastpos, out dwDummy);
                    if (!bSuccess)
                        break;

                    // Update lastpos
                    if (csbi.dwCursorPosition.Y >= sLimitVert)
                        lastpos = origin;
                    else
                        lastpos = csbi.dwCursorPosition;

                    // scan screen buffer and transmit character to real output handle
                    //StringBuilder scan = buffer;
                    //byte[] scan = new byte[4000];
                    byte[] scan = buffer;
                    //int index = 1;
                    do
                    {
                        if (scan != null)
                        {
                            Int32 len = 1;
                            //while (scan[len] !='\0' && (len < count))
                            while ((len < count))
                                len++;

                            WriteFile(hOutput, scan,
                                      (uint)len, out dwDummy,
                                      IntPtr.Zero);

                            //scan += len;
                            count -= len;
                        }
                        else
                        {
                            Int32 len = 1;
                            while (!(scan[len] == '\0') && (len < count))
                                len++;

                            //scan += len;
                            count -= len;
                        }
                    } while (count > 0);

                    //LocalFree(buffer);
                    buffer = null;
                }

                // loop until end of subprocess
            } while (!exitNow);

            CloseHandle(hConsole);

            // release subprocess handle
            UInt32 exitCode;
            if (!GetExitCodeProcess(pi.hProcess, out exitCode))
                exitCode = unchecked((uint)-3);
            CloseHandle(pi.hProcess);

            ////Console.ReadLine();
        }




    }
}
