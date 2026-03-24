using System;
//using System.Linq;
using System.Windows.Forms;

namespace WinXound_Net
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            try
            {
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                    DateTime.Now + Environment.NewLine + msg);
                MessageBox.Show(msg, "WinXound - Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
