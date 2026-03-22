namespace WinXound_Net
{
    partial class wxCodeFormatterForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbInstrumentsType2 = new System.Windows.Forms.RadioButton();
            this.rbInstrumentsType1 = new System.Windows.Forms.RadioButton();
            this.checkBoxFormatTempo = new System.Windows.Forms.CheckBox();
            this.checkBoxFormatInstruments = new System.Windows.Forms.CheckBox();
            this.checkBoxFormatScoreInstruments = new System.Windows.Forms.CheckBox();
            this.checkBoxFormatHeader = new System.Windows.Forms.CheckBox();
            this.checkBoxFormatFunctions = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSaveAndExit = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.textEditorExample = new ScintillaTextEditor.TextEditor();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbInstrumentsType2);
            this.groupBox1.Controls.Add(this.rbInstrumentsType1);
            this.groupBox1.Controls.Add(this.checkBoxFormatTempo);
            this.groupBox1.Controls.Add(this.checkBoxFormatInstruments);
            this.groupBox1.Controls.Add(this.checkBoxFormatScoreInstruments);
            this.groupBox1.Controls.Add(this.checkBoxFormatHeader);
            this.groupBox1.Controls.Add(this.checkBoxFormatFunctions);
            this.groupBox1.Location = new System.Drawing.Point(5, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(212, 161);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sections";
            // 
            // rbInstrumentsType2
            // 
            this.rbInstrumentsType2.AutoSize = true;
            this.rbInstrumentsType2.Location = new System.Drawing.Point(107, 65);
            this.rbInstrumentsType2.Name = "rbInstrumentsType2";
            this.rbInstrumentsType2.Size = new System.Drawing.Size(57, 17);
            this.rbInstrumentsType2.TabIndex = 3;
            this.rbInstrumentsType2.Text = "Style 2";
            this.rbInstrumentsType2.UseVisualStyleBackColor = true;
            this.rbInstrumentsType2.CheckedChanged += new System.EventHandler(this.rbInstrumentsType2_CheckedChanged);
            // 
            // rbInstrumentsType1
            // 
            this.rbInstrumentsType1.AutoSize = true;
            this.rbInstrumentsType1.Checked = true;
            this.rbInstrumentsType1.Location = new System.Drawing.Point(28, 65);
            this.rbInstrumentsType1.Name = "rbInstrumentsType1";
            this.rbInstrumentsType1.Size = new System.Drawing.Size(57, 17);
            this.rbInstrumentsType1.TabIndex = 2;
            this.rbInstrumentsType1.TabStop = true;
            this.rbInstrumentsType1.Text = "Style 1";
            this.rbInstrumentsType1.UseVisualStyleBackColor = true;
            this.rbInstrumentsType1.CheckedChanged += new System.EventHandler(this.rbInstrumentsType1_CheckedChanged);
            // 
            // checkBoxFormatTempo
            // 
            this.checkBoxFormatTempo.AutoSize = true;
            this.checkBoxFormatTempo.Checked = true;
            this.checkBoxFormatTempo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFormatTempo.Location = new System.Drawing.Point(9, 135);
            this.checkBoxFormatTempo.Name = "checkBoxFormatTempo";
            this.checkBoxFormatTempo.Size = new System.Drawing.Size(71, 17);
            this.checkBoxFormatTempo.TabIndex = 6;
            this.checkBoxFormatTempo.Text = "Tempo (t)";
            this.checkBoxFormatTempo.UseVisualStyleBackColor = true;
            this.checkBoxFormatTempo.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBoxFormatInstruments
            // 
            this.checkBoxFormatInstruments.AutoSize = true;
            this.checkBoxFormatInstruments.Checked = true;
            this.checkBoxFormatInstruments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFormatInstruments.Location = new System.Drawing.Point(9, 42);
            this.checkBoxFormatInstruments.Name = "checkBoxFormatInstruments";
            this.checkBoxFormatInstruments.Size = new System.Drawing.Size(80, 17);
            this.checkBoxFormatInstruments.TabIndex = 1;
            this.checkBoxFormatInstruments.Text = "Instruments";
            this.checkBoxFormatInstruments.UseVisualStyleBackColor = true;
            this.checkBoxFormatInstruments.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBoxFormatScoreInstruments
            // 
            this.checkBoxFormatScoreInstruments.AutoSize = true;
            this.checkBoxFormatScoreInstruments.Checked = true;
            this.checkBoxFormatScoreInstruments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFormatScoreInstruments.Location = new System.Drawing.Point(9, 112);
            this.checkBoxFormatScoreInstruments.Name = "checkBoxFormatScoreInstruments";
            this.checkBoxFormatScoreInstruments.Size = new System.Drawing.Size(91, 17);
            this.checkBoxFormatScoreInstruments.TabIndex = 5;
            this.checkBoxFormatScoreInstruments.Text = "Instruments (i)";
            this.checkBoxFormatScoreInstruments.UseVisualStyleBackColor = true;
            this.checkBoxFormatScoreInstruments.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBoxFormatHeader
            // 
            this.checkBoxFormatHeader.AutoSize = true;
            this.checkBoxFormatHeader.Checked = true;
            this.checkBoxFormatHeader.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFormatHeader.Location = new System.Drawing.Point(9, 19);
            this.checkBoxFormatHeader.Name = "checkBoxFormatHeader";
            this.checkBoxFormatHeader.Size = new System.Drawing.Size(61, 17);
            this.checkBoxFormatHeader.TabIndex = 0;
            this.checkBoxFormatHeader.Text = "Header";
            this.checkBoxFormatHeader.UseVisualStyleBackColor = true;
            this.checkBoxFormatHeader.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBoxFormatFunctions
            // 
            this.checkBoxFormatFunctions.AutoSize = true;
            this.checkBoxFormatFunctions.Checked = true;
            this.checkBoxFormatFunctions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFormatFunctions.Location = new System.Drawing.Point(9, 89);
            this.checkBoxFormatFunctions.Name = "checkBoxFormatFunctions";
            this.checkBoxFormatFunctions.Size = new System.Drawing.Size(84, 17);
            this.checkBoxFormatFunctions.TabIndex = 4;
            this.checkBoxFormatFunctions.Text = "Functions (f)";
            this.checkBoxFormatFunctions.UseVisualStyleBackColor = true;
            this.checkBoxFormatFunctions.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(5, 346);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(105, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSaveAndExit
            // 
            this.buttonSaveAndExit.Location = new System.Drawing.Point(112, 346);
            this.buttonSaveAndExit.Name = "buttonSaveAndExit";
            this.buttonSaveAndExit.Size = new System.Drawing.Size(105, 23);
            this.buttonSaveAndExit.TabIndex = 9;
            this.buttonSaveAndExit.Text = "&Save and Exit";
            this.buttonSaveAndExit.UseVisualStyleBackColor = true;
            this.buttonSaveAndExit.Click += new System.EventHandler(this.buttonSaveAndExit_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(5, 173);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(212, 23);
            this.buttonReset.TabIndex = 7;
            this.buttonReset.Text = "&Reset to Default";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textEditorExample);
            this.groupBox4.Location = new System.Drawing.Point(223, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(398, 363);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Example";
            // 
            // textEditorExample
            // 
            this.textEditorExample.AllowCaretBeyondEOL = true;
            this.textEditorExample.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditorExample.FileName = "";
            this.textEditorExample.Location = new System.Drawing.Point(3, 16);
            this.textEditorExample.MarkCaretLine = false;
            this.textEditorExample.Name = "textEditorExample";
            this.textEditorExample.ReadOnly = false;
            this.textEditorExample.ShowEOLMarker = false;
            this.textEditorExample.ShowLineNumbers = true;
            this.textEditorExample.ShowMatchingBracket = false;
            this.textEditorExample.ShowSpaces = false;
            this.textEditorExample.ShowVerticalRuler = false;
            this.textEditorExample.Size = new System.Drawing.Size(392, 344);
            this.textEditorExample.TabIndent = 8;
            this.textEditorExample.TabIndex = 0;
            this.textEditorExample.TabStop = false;
            this.textEditorExample.TextEditorFont = new System.Drawing.Font("Courier New", 10F);
            // 
            // wxCodeFormatterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(625, 373);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonSaveAndExit);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "wxCodeFormatterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Code Formatter Options";
            this.Load += new System.EventHandler(this.wxCodeFormatterForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.wxCodeFormatterForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxFormatInstruments;
        private System.Windows.Forms.CheckBox checkBoxFormatHeader;
        private System.Windows.Forms.CheckBox checkBoxFormatTempo;
        private System.Windows.Forms.CheckBox checkBoxFormatScoreInstruments;
        private System.Windows.Forms.CheckBox checkBoxFormatFunctions;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSaveAndExit;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rbInstrumentsType2;
        private System.Windows.Forms.RadioButton rbInstrumentsType1;
        private ScintillaTextEditor.TextEditor textEditorExample;
    }
}