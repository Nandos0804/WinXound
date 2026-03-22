using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace OpcodesParser
{
    class wxGlobal
    {

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



        ///////////////////////////////////////////////////////////////////////////////
        //Useful method to retrieve internal resources
        public static StreamReader GetResource(string filename)
        {
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

    }
}
