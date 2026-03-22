using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace WinXound_Net
{
    partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        string newline = System.Environment.NewLine;

        private void FormAbout_Load(object sender, EventArgs e)
        {
            
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                String EmailLink = "mailto:" + LinkLabel1.Text.Trim() +
                                   @"?body=" + 
                                   @"&subject=WinXound Info";
                System.Diagnostics.Process.Start(EmailLink);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form About - LinkLabel1_LinkClicked");
            }
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                String EmailLink = "mailto:" + LinkLabel2.Text.Trim() +
                                   @"?body=" +
                                   @"&subject=WinXound Info";
                System.Diagnostics.Process.Start(EmailLink);
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form About - LinkLabel2_LinkClicked");
            }
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(LinkLabel3.Text.Trim());
            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "Form About - LinkLabel3_LinkClicked");
            }
        }

        private void FormAbout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }

        private void FormAbout_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                this.SuspendLayout();

                Int32 mLeft = labelMail.Left;

                Point mLocation = new Point(mLeft - 5, 9);

                //TITLE
                string mTitle = "WinXound.Net";
                TextRenderer.DrawText(e.Graphics, mTitle,
                          new Font("Microsoft Sans Serif", 24),
                          mLocation, Color.Black);

                Size mSizeTitle = TextRenderer.MeasureText(mTitle, new Font("Microsoft Sans Serif", 24));
                mLocation.X = mLeft - 1;
                mLocation.Y += mSizeTitle.Height;

                //DESCRIPTION
                string mDescription = "an open source editor for CSound and CSoundAV" + newline +
                                      "developed by Stefano Bonetti";
                TextRenderer.DrawText(e.Graphics, mDescription,
                                      new Font("Microsoft Sans Serif", 16),
                                      mLocation, Color.Black);

                Size mSizeDescription = TextRenderer.MeasureText(mTitle, new Font("Microsoft Sans Serif", 16));
                mLocation.X = mLeft + 1;
                mLocation.Y += (mSizeTitle.Height + mSizeDescription.Height);

                //VARIOUS
                string mVarious =
                      "The Text Editor is entirely based on Scintilla by Neil Hodgson (version " +
                      wxGlobal.GetScintillaVersion() + ")." + newline +
                      "Scintilla is Copyright by Neil Hodgson (neilh@scintilla.org)." + newline + newline +
                      "CSound is copyright by Barry Vercoe and John ffitch." + newline +
                      "CSoundAV is Copyright by Gabriel Maldonado." + newline + newline +
                      "This program is freeware and not intended for commercial use." + newline +
                      "You could use the source code freely." + newline +
                      "Altered source versions must be plainly marked as such, " + newline +
                      "and must not be misrepresented as being the original software." + newline +
                      "If you use this software in a product, an acknowledgment in " + newline +
                      "the product documentation would be appreciated.";
                //+ newline + newline +
                //"For any problem or suggestion please Mail to:";

                TextRenderer.DrawText(e.Graphics, mVarious,
                                      new Font("Microsoft Sans Serif", 10),
                                      mLocation, Color.Black);



                //VERSION
                string mVersion = "Version " + wxGlobal.WINXOUND_VERSION;
                TextFormatFlags flags = TextFormatFlags.Bottom |
                                        TextFormatFlags.Right;

                Rectangle mRect = this.ClientRectangle;
                mRect.Offset(-8, -8);

                TextRenderer.DrawText(e.Graphics, mVersion,
                                      new Font("Microsoft Sans Serif", 16),
                                      mRect, Color.Black,
                                      flags);

                this.ResumeLayout();

            }
            catch (Exception ex)
            {
                wxGlobal.wxMessageError(ex.Message, "FormAbout_Paint");
            }

        }
    }
}
