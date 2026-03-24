using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinXound_Net
{
    public partial class FormPlayer : Form
    {

        private wxPlayer mPlayer = new wxPlayer();
        private Int32 HH, MM, SS;
        private string mTitle = " WinXound Player";
        private bool bPlaying = false;
        private string mLastFileName = "";


        public FormPlayer()
        {
            InitializeComponent();
        }

        public string FileName
        {
            get { return mLastFileName; }
            set { mLastFileName = value; }
        }

        private void FormPlayer_Load(object sender, EventArgs e)
        {
            labelFilename.Text = "";
            mPlayer.Player_TimerTick +=
                new wxPlayer.Player_TimerTickEventHandler(mPlayer_Player_TimerTick);
            mPlayer.Player_Paused +=
                new wxPlayer.Player_PausedEventHandler(mPlayer_Player_Paused);
            mPlayer.Player_Stopped +=
                new wxPlayer.Player_StoppedEventHandler(mPlayer_Player_Stopped);

            FormPlayerShow();
        }

        public void FormPlayerShow()
        {
            if (!string.IsNullOrEmpty(mLastFileName))
            {
                bool ret = mPlayer.OpenFile(mLastFileName);
                if (ret)
                {
                    labelFilename.Text = mLastFileName;
                    this.Text = mTitle + " [Total time: " + DisplayTime(mPlayer.Duration) + "]";
                    trackBarPosition.Maximum = mPlayer.Duration;
                    trackBarPosition.TickFrequency = trackBarPosition.Maximum * 10 / 100;
                    buttonPlay.Text = "Play (P)";
                    buttonPlay.Enabled = true;
                    //buttonPause.Enabled = false;
                    buttonStop.Enabled = false;
                    trackBarPosition.Enabled = true;
                    buttonWaveEditor.Enabled = true;
                    return;
                }
            }

            trackBarPosition.Value = 0;
            trackBarPosition.Maximum = mPlayer.Duration;
            trackBarPosition.TickFrequency = trackBarPosition.Maximum * 10 / 100;

            labelFilename.Text = "";
            trackBarPosition.Enabled = false;
            buttonPlay.Text = "Play (P)";
            buttonPlay.Enabled = false;
            //buttonPause.Enabled = false;
            buttonStop.Enabled = false;
            buttonWaveEditor.Enabled = false;
            this.Text = mTitle;
            trackBarPosition.Maximum = 1;
            trackBarPosition.TickFrequency = 0;
            trackBarPosition.Text = "";

        }

        void mPlayer_Player_TimerTick()
        {
            labelTime.Text = DisplayTime(mPlayer.Position);
            trackBarPosition.Value = mPlayer.Position;
        }

        void mPlayer_Player_Stopped()
        {
            labelTime.Text = "00:00:00"; //DisplayTime(mPlayer.Position);
            trackBarPosition.Value = 0;

            //buttonPlay.Enabled = true;
            //buttonPause.Enabled = false;
            buttonPlay.Text = "Play (P)";
            buttonStop.Enabled = false;
        }

        void mPlayer_Player_Paused()
        {
            //buttonPause.Enabled = false;
            //buttonPlay.Enabled = true;
            buttonPlay.Text = "Play (P)";
            buttonStop.Enabled = true;
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (mPlayer.IsPlaying())
            {
                mPlayer.PauseSound();
            }
            else
            {
                mPlayer.PlaySound();

                //buttonPlay.Enabled = false;
                //buttonPause.Enabled = true;
                buttonPlay.Text = "Pause (P)";
                buttonStop.Enabled = true;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            mPlayer.StopSound();
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            mPlayer.PauseSound();
        }

        private void buttonRewind_Click(object sender, EventArgs e)
        {
            mPlayer.StopSound();
            mPlayer.Position = 0;
            trackBarPosition.Value = 0;
        }


        private string DisplayTime(Int32 mMilliseconds)
        {
            HH = mMilliseconds / 1000 / 60 / 60;
            MM = mMilliseconds / 1000 / 60;
            SS = mMilliseconds / 1000 % 60;

            string tempString = String.Format("{0:00}", HH) + ":" +
                                String.Format("{0:00}", MM) + ":" +
                                String.Format("{0:00}", SS);


            return tempString;
        }

        private void FormPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                buttonExit.PerformClick();
            }
            else if (e.KeyCode == Keys.P)
            {
                buttonPlay.PerformClick();
            }
            /*else if (e.KeyCode == Keys.U)
            {
                buttonPause.PerformClick();
            }*/
            else if (e.KeyCode == Keys.S)
            {
                buttonStop.PerformClick();
            }
            else if (e.KeyCode == Keys.W)
            {
                buttonWaveEditor.PerformClick();
            }
            else if (e.KeyCode == Keys.B)
            {
                buttonOpen.PerformClick();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            mPlayer.StopSound();
            mPlayer.CloseAll();

            this.DialogResult = DialogResult.OK;

            //mPlayer.Dispose();
            //this.Close();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Supported files (*.wav)|*.wav|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (!(openFileDialog1.ShowDialog() == DialogResult.OK) ||
                string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                return;
            }

            //mPlayer.OpenFile(openFileDialog1.FileName);
            labelFilename.Text = openFileDialog1.FileName;
            mLastFileName = openFileDialog1.FileName;
            FormPlayerShow();
            buttonPlay.Focus();
        }

        private void buttonWaveEditor_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WaveEditor))
                {
                    if (System.IO.File.Exists(wxGlobal.Settings.Directory.WaveEditor))
                    {
                        if (!string.IsNullOrEmpty(mLastFileName))
                        {
                            if (System.IO.File.Exists(mLastFileName))
                            {
                                System.Diagnostics.Process.Start(
                                    wxGlobal.Settings.Directory.WaveEditor, "\"" +
                                    mLastFileName + "\"");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Process.Start(
                                wxGlobal.Settings.Directory.WaveEditor);
                        }
                        buttonExit.PerformClick();
                    }
                    else
                    {
                        throw new System.Exception("");
                    }
                }
                else
                {
                    throw new System.Exception("");
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Unable to start the Wave Editor executable!\n" +
                                "Please check WinXound Settings (Directories->Wave Editor Executable field)",
                                "WinXound Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }


        }


        private void trackBarPosition_MouseDown(object sender, MouseEventArgs e)
        {
            if (mPlayer.IsPlaying()) bPlaying = true;
            //buttonPause.PerformClick();
            mPlayer.PauseSound();

        }

        private void trackBarPosition_Scroll(object sender, EventArgs e)
        {
            mPlayer.Position = trackBarPosition.Value;
            labelTime.Text = DisplayTime(mPlayer.Position);
        }

        private void trackBarPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (bPlaying)
                buttonPlay.PerformClick();

            bPlaying = false;
        }

        public void ClosePlayer()
        {
            mPlayer.CloseAll();
        }

    }





    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////





    public class wxPlayer
    {
        [DllImport("winmm.dll", EntryPoint = "mciGetErrorStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciGetErrorString(int dwError,
                                                    string lpstrBuffer,
                                                    int uLength);

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        //Get the error message of the mcidevice if any
        private static extern int mciSendString(string lpstrCommand,
                                                StringBuilder lpstrReturnString, //string lpstrReturnString, 
                                                int uReturnLength,
                                                int hwndCallback);


        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        //Get the error message of the mcidevice if any
        private static extern int mciSendString(string lpstrCommand,
                                                int lpstrReturnString,
                                                int uReturnLength,
                                                int hwndCallback);

        //Send command strings to the mci device
        //'Private Declare Function getTickCount Lib "kernel32" Alias "GetTickCount" () As Integer

        private Timer withEventsField_mTimer = new Timer();
        private Timer mTimer
        {
            get { return withEventsField_mTimer; }
            set
            {
                if (withEventsField_mTimer != null)
                {
                    withEventsField_mTimer.Tick -= mTimer_Tick;
                }
                withEventsField_mTimer = value;
                if (withEventsField_mTimer != null)
                {
                    withEventsField_mTimer.Tick += mTimer_Tick;
                }
            }
        }


        //private String retString = new String(' ', 128);
        private StringBuilder retString = new StringBuilder(128);

        private Int32 mDuration = 0;
        public event Player_TimerTickEventHandler Player_TimerTick;
        public delegate void Player_TimerTickEventHandler();
        public event Player_StoppedEventHandler Player_Stopped;
        public delegate void Player_StoppedEventHandler();
        public event Player_PausedEventHandler Player_Paused;
        public delegate void Player_PausedEventHandler();


        public wxPlayer()
        {
            //'
            mTimer.Tick += new EventHandler(mTimer_Tick);
        }


        public bool OpenFile(string Filename)
        {
            bool functionReturnValue = false;
            try
            {
                if (!System.IO.File.Exists(Filename))
                    return false;

                Filename = "\"" + Filename + "\"";

                mciSendString("close mysound", 0, 0, 0);
                mciSendString("open " + Filename + " type waveaudio alias mysound", 0, 0, 0);

                mciSendString("seek mysound to end", 0, 0, 0);
                mDuration = this.Position;
                mciSendString("seek mysound to start", 0, 0, 0);

                functionReturnValue = true;

            }
            catch (Exception)
            {
                return false;
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
            }

            return functionReturnValue;

        }

        public void StopSound()
        {
            try
            {
                mciSendString("stop mysound", 0, 0, 0);
                mciSendString("seek mysound to start", 0, 0, 0);
                StopTimer();
                if (Player_Stopped != null)
                {
                    Player_Stopped();
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                System.Diagnostics.Debug.WriteLine(
                    "wxPlayer StopSound Error: " + ex.Message);
            }
        }

        public void PauseSound()
        {
            try
            {
                mciSendString("pause mysound", 0, 0, 0);
                StopTimer();
                if (Player_Paused != null)
                {
                    Player_Paused();
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                System.Diagnostics.Debug.WriteLine(
                    "wxPlayer PauseSound Error: " + ex.Message);
            }
        }

        public void PlaySound()
        {
            try
            {
                //'mciSendString("seek mysound to start", 0, 0, 0)
                mciSendString("play mysound", 0, 0, 0);
                StartTimer();

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                System.Diagnostics.Debug.WriteLine(
                    "wxPlayer PlaySound Error: " + ex.Message);
            }
        }

        public bool IsPlaying()
        {
            //

            try
            {
                mciSendString("status mysound mode", retString, 128, 0);
                //retString = retString.Substring(0,7); // Strings.Left(retString, 7);
                //System.Diagnostics.Debug.WriteLine(retString);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                System.Diagnostics.Debug.WriteLine(
                    "wxPlayer IsPlaying Error: " + ex.Message);
            }

            return (retString.ToString().ToLower().Contains("playing"));

        }


        public Int32 Duration
        {
            get { return mDuration; }
        }

        public Int32 Position
        {

            get
            {
                try
                {
                    mciSendString("set mysound time format ms", 0, 0, 0);
                    mciSendString("status mysound position", retString, 128, 0);
                    //return Conversion.Val(retString);
                    return Int32.Parse(retString.ToString());
                    //System.Diagnostics.Debug.WriteLine("Position: " + retString);
                    //return 0;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                    System.Diagnostics.Debug.WriteLine(
                        "wxPlayer Position Get Error: " + ex.Message);
                }

                return 0;
            }

            set
            {
                try
                {
                    mciSendString("set movie time format ms", 0, 0, 0);
                    if (this.IsPlaying() == true)
                    {
                        mciSendString("play mysound from " + value, 0, 0, 0);
                    }
                    else
                    {
                        mciSendString("seek mysound to " + value, 0, 0, 0);
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "wxPlayer Position Set Error: " + ex.Message);
                }
            }
        }


        public void CloseAll() //Dispose()
        {
            try
            {
                mciSendString("close all", 0, 0, 0);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, " Error", MessageBoxButtons.OK);
                System.Diagnostics.Debug.WriteLine(
                    "wxPlayer CloseAll Error: " + ex.Message);
            }
        }




        ///''''''''''
        //TIMER STUFFS
        private void StartTimer()
        {
            //System.Diagnostics.Debug.WriteLine("TIMER STARTED");
            mTimer.Enabled = true;
            mTimer.Interval = 100;
            mTimer.Start();
        }

        private void StopTimer()
        {
            mTimer.Stop();
        }

        private void mTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.Position >= this.Duration)
            {
                this.StopSound();
            }
            if (this.IsPlaying() == false)
            {
                mTimer.Stop();
            }

            if (Player_TimerTick != null)
                Player_TimerTick();
        }

    }

}
