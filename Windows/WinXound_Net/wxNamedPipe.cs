using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace WinXound_Net
{
    class wxNamedPipe
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
           String pipeName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplate);

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;

        public const int BUFFER_SIZE = 4096;

        string pipeName;
        private FileStream stream;
        private SafeFileHandle handle;
        Thread readThread;
        bool connected;


        public wxNamedPipe()
        {

        }

        public void Disconnect()
        {
            this.stream.Close();
            this.handle.Close();
            this.connected = false;
        }

        public void Dispose()
        {
            try
            {
                Disconnect();
                System.Diagnostics.Debug.WriteLine("wxNamedPipe disposed");
            }
            catch (Exception ex)
            {
            }
        }

        ~wxNamedPipe()
        {
            Dispose();
        }

        public bool Connected
        {
            get { return this.connected; }
        }

        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        /// <summary>
        /// Connects to the server
        /// </summary>
        //public void Connect()
        public bool Connect()
        {
            this.handle =
               CreateFile(
                  this.pipeName,
                  GENERIC_READ | GENERIC_WRITE,
                  0,
                  IntPtr.Zero,
                  OPEN_EXISTING,
                  FILE_FLAG_OVERLAPPED,
                  IntPtr.Zero);

            //could not create handle - server probably not running
            if (this.handle.IsInvalid)
                return false;

            this.connected = true;

            //start listening for messages
            this.readThread = new Thread(new ThreadStart(Read));
            this.readThread.Start();

            return true;
        }

        /// <summary>
        /// Reads data from the server
        /// </summary>
        public void Read()
        {
            this.stream = new FileStream(this.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] readBuffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = this.stream.Read(readBuffer, 0, BUFFER_SIZE);
                    System.Diagnostics.Debug.WriteLine(
                        "wxNamedPipe - Data Received");
                }
                catch (Exception ex)
                {
                    //read error occurred
                    System.Diagnostics.Debug.WriteLine(
                        "wxNamedPipe - Read Error: " + ex.Message);
                    break;
                }

                //server has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                if (this.MessageReceived != null)
                {
                    //this.MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
                    string strContent = null;
                    strContent = Encoding.Default.GetString(readBuffer, 8, bytesRead - 8);
                    this.MessageReceived(strContent);
                }
            }


            //clean up resource
            this.stream.Close();
            this.handle.Close();
            this.connected = false;
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        public bool SendMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            //byte[] messageBuffer = encoder.GetBytes(message);

            //"2C-9E-B4-F2-01-00-00-00" : two UINT32 (4 byte + 4 byte)
            byte[] magic1 = BitConverter.GetBytes(Convert.ToUInt32(4071923244)); //L
            byte[] magic2 = BitConverter.GetBytes(Convert.ToUInt32(message.Length));
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(message);
            byte[] messageBuffer = new byte[magic1.Length + magic2.Length + sendBytes.Length];

            Int32 index = 0;
            for (Int32 i = 0; i <= magic1.Length - 1; i++)
            {
                messageBuffer[index] = magic1[i];
                index += 1;
            }
            for (Int32 i = 0; i <= magic2.Length - 1; i++)
            {
                messageBuffer[index] = magic2[i];
                index += 1;
            }
            for (Int32 i = 0; i <= sendBytes.Length - 1; i++)
            {
                messageBuffer[index] = sendBytes[i];
                index += 1;
            }

            try
            {
                this.stream.Write(messageBuffer, 0, messageBuffer.Length);
                this.stream.Flush();
            }
            catch (Exception ex)
            {
                //clean up resource
                try
                {
                    this.stream.Close();
                    this.handle.Close();
                    this.connected = false;
                }
                catch (Exception ex_int)
                {
                }

                System.Diagnostics.Debug.WriteLine(
                    "wxNamedPipe - SendMessage Error: " + ex.Message);

                return false;
            }

            return true;

        }
    }
}
