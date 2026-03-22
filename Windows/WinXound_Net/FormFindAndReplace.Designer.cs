namespace WinXound_Net
{
    partial class FormFindAndReplace
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ButtonFindPrevious = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.CheckBoxMatchCase = new System.Windows.Forms.CheckBox();
            this.CheckBoxMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.rbUp = new System.Windows.Forms.RadioButton();
            this.rbDown = new System.Windows.Forms.RadioButton();
            this.CheckBoxReplaceFromCaret = new System.Windows.Forms.CheckBox();
            this.ButtonReplaceAll = new System.Windows.Forms.Button();
            this.ButtonReplace = new System.Windows.Forms.Button();
            this.ButtonFindNext = new System.Windows.Forms.Button();
            this.TextBoxReplace = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.TextBoxFind = new System.Windows.Forms.TextBox();
            this.ButtonExit = new System.Windows.Forms.Button();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonFindPrevious
            // 
            this.ButtonFindPrevious.Location = new System.Drawing.Point(329, 50);
            this.ButtonFindPrevious.Name = "ButtonFindPrevious";
            this.ButtonFindPrevious.Size = new System.Drawing.Size(99, 21);
            this.ButtonFindPrevious.TabIndex = 3;
            this.ButtonFindPrevious.Text = "&Find Previous";
            this.ButtonFindPrevious.UseVisualStyleBackColor = true;
            this.ButtonFindPrevious.Click += new System.EventHandler(this.ButtonFindPrevious_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.CheckBoxMatchCase);
            this.GroupBox1.Controls.Add(this.CheckBoxMatchWholeWord);
            this.GroupBox1.Location = new System.Drawing.Point(12, 69);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(311, 39);
            this.GroupBox1.TabIndex = 1;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Find Options";
            // 
            // CheckBoxMatchCase
            // 
            this.CheckBoxMatchCase.AutoSize = true;
            this.CheckBoxMatchCase.Location = new System.Drawing.Point(138, 19);
            this.CheckBoxMatchCase.Name = "CheckBoxMatchCase";
            this.CheckBoxMatchCase.Size = new System.Drawing.Size(82, 17);
            this.CheckBoxMatchCase.TabIndex = 1;
            this.CheckBoxMatchCase.Text = "Match case";
            this.CheckBoxMatchCase.UseVisualStyleBackColor = true;
            // 
            // CheckBoxMatchWholeWord
            // 
            this.CheckBoxMatchWholeWord.AutoSize = true;
            this.CheckBoxMatchWholeWord.Location = new System.Drawing.Point(6, 19);
            this.CheckBoxMatchWholeWord.Name = "CheckBoxMatchWholeWord";
            this.CheckBoxMatchWholeWord.Size = new System.Drawing.Size(113, 17);
            this.CheckBoxMatchWholeWord.TabIndex = 0;
            this.CheckBoxMatchWholeWord.Text = "Match whole word";
            this.CheckBoxMatchWholeWord.UseVisualStyleBackColor = true;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.rbUp);
            this.GroupBox2.Controls.Add(this.rbDown);
            this.GroupBox2.Controls.Add(this.CheckBoxReplaceFromCaret);
            this.GroupBox2.Location = new System.Drawing.Point(12, 176);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(311, 46);
            this.GroupBox2.TabIndex = 5;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Replace All Options";
            // 
            // rbUp
            // 
            this.rbUp.AutoSize = true;
            this.rbUp.Enabled = false;
            this.rbUp.Location = new System.Drawing.Point(226, 18);
            this.rbUp.Name = "rbUp";
            this.rbUp.Size = new System.Drawing.Size(39, 17);
            this.rbUp.TabIndex = 16;
            this.rbUp.TabStop = true;
            this.rbUp.Text = "Up";
            this.rbUp.UseVisualStyleBackColor = true;
            // 
            // rbDown
            // 
            this.rbDown.AutoSize = true;
            this.rbDown.Checked = true;
            this.rbDown.Enabled = false;
            this.rbDown.Location = new System.Drawing.Point(167, 18);
            this.rbDown.Name = "rbDown";
            this.rbDown.Size = new System.Drawing.Size(53, 17);
            this.rbDown.TabIndex = 15;
            this.rbDown.TabStop = true;
            this.rbDown.Text = "Down";
            this.rbDown.UseVisualStyleBackColor = true;
            // 
            // CheckBoxReplaceFromCaret
            // 
            this.CheckBoxReplaceFromCaret.AutoSize = true;
            this.CheckBoxReplaceFromCaret.Location = new System.Drawing.Point(6, 19);
            this.CheckBoxReplaceFromCaret.Name = "CheckBoxReplaceFromCaret";
            this.CheckBoxReplaceFromCaret.Size = new System.Drawing.Size(155, 17);
            this.CheckBoxReplaceFromCaret.TabIndex = 0;
            this.CheckBoxReplaceFromCaret.Text = "Replace from caret position";
            this.CheckBoxReplaceFromCaret.UseVisualStyleBackColor = true;
            this.CheckBoxReplaceFromCaret.CheckedChanged += new System.EventHandler(this.CheckBoxReplaceFromCaret_CheckedChanged);
            // 
            // ButtonReplaceAll
            // 
            this.ButtonReplaceAll.Location = new System.Drawing.Point(329, 160);
            this.ButtonReplaceAll.Name = "ButtonReplaceAll";
            this.ButtonReplaceAll.Size = new System.Drawing.Size(99, 21);
            this.ButtonReplaceAll.TabIndex = 7;
            this.ButtonReplaceAll.Text = "Replace &All";
            this.ButtonReplaceAll.UseVisualStyleBackColor = true;
            this.ButtonReplaceAll.Click += new System.EventHandler(this.ButtonReplaceAll_Click);
            // 
            // ButtonReplace
            // 
            this.ButtonReplace.Location = new System.Drawing.Point(329, 133);
            this.ButtonReplace.Name = "ButtonReplace";
            this.ButtonReplace.Size = new System.Drawing.Size(99, 21);
            this.ButtonReplace.TabIndex = 6;
            this.ButtonReplace.Text = "&Replace";
            this.ButtonReplace.UseVisualStyleBackColor = true;
            this.ButtonReplace.Click += new System.EventHandler(this.ButtonReplace_Click);
            // 
            // ButtonFindNext
            // 
            this.ButtonFindNext.Location = new System.Drawing.Point(329, 23);
            this.ButtonFindNext.Name = "ButtonFindNext";
            this.ButtonFindNext.Size = new System.Drawing.Size(99, 21);
            this.ButtonFindNext.TabIndex = 2;
            this.ButtonFindNext.Text = "&Find Next";
            this.ButtonFindNext.UseVisualStyleBackColor = true;
            this.ButtonFindNext.Click += new System.EventHandler(this.ButtonFindNext_Click);
            // 
            // TextBoxReplace
            // 
            this.TextBoxReplace.Location = new System.Drawing.Point(12, 133);
            this.TextBoxReplace.Multiline = true;
            this.TextBoxReplace.Name = "TextBoxReplace";
            this.TextBoxReplace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxReplace.Size = new System.Drawing.Size(311, 37);
            this.TextBoxReplace.TabIndex = 4;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(10, 117);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(72, 13);
            this.Label2.TabIndex = 14;
            this.Label2.Text = "Replace with:";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(9, 8);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(30, 13);
            this.Label1.TabIndex = 11;
            this.Label1.Text = "Find:";
            // 
            // TextBoxFind
            // 
            this.TextBoxFind.Location = new System.Drawing.Point(12, 24);
            this.TextBoxFind.Multiline = true;
            this.TextBoxFind.Name = "TextBoxFind";
            this.TextBoxFind.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxFind.Size = new System.Drawing.Size(311, 39);
            this.TextBoxFind.TabIndex = 0;
            // 
            // ButtonExit
            // 
            this.ButtonExit.Location = new System.Drawing.Point(329, 199);
            this.ButtonExit.Name = "ButtonExit";
            this.ButtonExit.Size = new System.Drawing.Size(99, 23);
            this.ButtonExit.TabIndex = 8;
            this.ButtonExit.Text = "E&xit";
            this.ButtonExit.UseVisualStyleBackColor = true;
            this.ButtonExit.Click += new System.EventHandler(this.ButtonExit_Click);
            // 
            // FormFindAndReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(438, 230);
            this.ControlBox = false;
            this.Controls.Add(this.ButtonFindPrevious);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.ButtonReplaceAll);
            this.Controls.Add(this.ButtonReplace);
            this.Controls.Add(this.ButtonFindNext);
            this.Controls.Add(this.TextBoxReplace);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TextBoxFind);
            this.Controls.Add(this.ButtonExit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "FormFindAndReplace";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = " Find and Replace";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormFindAndReplace_Load);
            this.Shown += new System.EventHandler(this.FormFindAndReplace_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormFindAndReplace_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFindAndReplace_KeyDown);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button ButtonFindPrevious;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.CheckBox CheckBoxMatchCase;
        internal System.Windows.Forms.CheckBox CheckBoxMatchWholeWord;
        internal System.Windows.Forms.GroupBox GroupBox2;
        internal System.Windows.Forms.CheckBox CheckBoxReplaceFromCaret;
        internal System.Windows.Forms.Button ButtonReplaceAll;
        internal System.Windows.Forms.Button ButtonReplace;
        internal System.Windows.Forms.Button ButtonFindNext;
        internal System.Windows.Forms.TextBox TextBoxReplace;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button ButtonExit;
        private System.Windows.Forms.RadioButton rbUp;
        private System.Windows.Forms.RadioButton rbDown;
        public System.Windows.Forms.TextBox TextBoxFind;
    }
}