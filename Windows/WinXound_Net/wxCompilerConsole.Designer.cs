namespace WinXound_Net
{
    partial class wxCompilerConsole
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.process1 = new System.Diagnostics.Process();
            this.ButtonStopCompiler = new System.Windows.Forms.Button();
            this.ButtonSaveCompiler = new System.Windows.Forms.Button();
            this.ButtonFindError = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RTB_Output = new ScintillaTextEditor.TextEditor();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.ButtonPauseCompiler = new System.Windows.Forms.Button();
            this.buttonStopBatch = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // process1
            // 
            this.process1.StartInfo.CreateNoWindow = true;
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.ErrorDialog = true;
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.RedirectStandardError = true;
            this.process1.StartInfo.RedirectStandardInput = true;
            this.process1.StartInfo.RedirectStandardOutput = true;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.UserName = "";
            this.process1.StartInfo.UseShellExecute = false;
            this.process1.SynchronizingObject = this;
            this.process1.Exited += new System.EventHandler(this.process1_Exited);
            // 
            // ButtonStopCompiler
            // 
            this.ButtonStopCompiler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonStopCompiler.Enabled = false;
            this.ButtonStopCompiler.Location = new System.Drawing.Point(0, 1);
            this.ButtonStopCompiler.Name = "ButtonStopCompiler";
            this.ButtonStopCompiler.Size = new System.Drawing.Size(95, 23);
            this.ButtonStopCompiler.TabIndex = 5;
            this.ButtonStopCompiler.Text = "S&top (Esc)";
            this.ButtonStopCompiler.UseVisualStyleBackColor = true;
            this.ButtonStopCompiler.Click += new System.EventHandler(this.ButtonStopCompiler_Click);
            // 
            // ButtonSaveCompiler
            // 
            this.ButtonSaveCompiler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSaveCompiler.Enabled = false;
            this.ButtonSaveCompiler.Location = new System.Drawing.Point(443, 1);
            this.ButtonSaveCompiler.Name = "ButtonSaveCompiler";
            this.ButtonSaveCompiler.Size = new System.Drawing.Size(109, 23);
            this.ButtonSaveCompiler.TabIndex = 6;
            this.ButtonSaveCompiler.Text = "&Save Output as ...";
            this.ButtonSaveCompiler.UseVisualStyleBackColor = true;
            this.ButtonSaveCompiler.Click += new System.EventHandler(this.ButtonSaveCompiler_Click);
            // 
            // ButtonFindError
            // 
            this.ButtonFindError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonFindError.Enabled = false;
            this.ButtonFindError.Location = new System.Drawing.Point(487, 1);
            this.ButtonFindError.Name = "ButtonFindError";
            this.ButtonFindError.Size = new System.Drawing.Size(68, 23);
            this.ButtonFindError.TabIndex = 7;
            this.ButtonFindError.Text = "&Go to error";
            this.ButtonFindError.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RTB_Output);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(555, 270);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compiler Output";
            // 
            // RTB_Output
            // 
            this.RTB_Output.AllowCaretBeyondEOL = true;
            this.RTB_Output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RTB_Output.FileName = "";
            this.RTB_Output.Location = new System.Drawing.Point(3, 16);
            this.RTB_Output.MarkCaretLine = false;
            this.RTB_Output.Name = "RTB_Output";
            this.RTB_Output.ReadOnly = false;
            this.RTB_Output.ShowEOLMarker = false;
            this.RTB_Output.ShowLineNumbers = false;
            this.RTB_Output.ShowMatchingBracket = true;
            this.RTB_Output.ShowSpaces = false;
            this.RTB_Output.ShowVerticalRuler = false;
            this.RTB_Output.Size = new System.Drawing.Size(549, 251);
            this.RTB_Output.TabIndent = 8;
            this.RTB_Output.TabIndex = 0;
            this.RTB_Output.TextEditorFont = new System.Drawing.Font("Courier New", 10F);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.ButtonPauseCompiler);
            this.panelButtons.Controls.Add(this.ButtonFindError);
            this.panelButtons.Controls.Add(this.buttonStopBatch);
            this.panelButtons.Controls.Add(this.ButtonStopCompiler);
            this.panelButtons.Controls.Add(this.ButtonSaveCompiler);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 270);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(555, 25);
            this.panelButtons.TabIndex = 1;
            // 
            // ButtonPauseCompiler
            // 
            this.ButtonPauseCompiler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonPauseCompiler.Enabled = false;
            this.ButtonPauseCompiler.Location = new System.Drawing.Point(101, 1);
            this.ButtonPauseCompiler.Name = "ButtonPauseCompiler";
            this.ButtonPauseCompiler.Size = new System.Drawing.Size(95, 23);
            this.ButtonPauseCompiler.TabIndex = 10;
            this.ButtonPauseCompiler.Text = "Pause";
            this.ButtonPauseCompiler.UseVisualStyleBackColor = true;
            this.ButtonPauseCompiler.Click += new System.EventHandler(this.ButtonPauseCompiler_Click);
            // 
            // buttonStopBatch
            // 
            this.buttonStopBatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStopBatch.Location = new System.Drawing.Point(202, 1);
            this.buttonStopBatch.Name = "buttonStopBatch";
            this.buttonStopBatch.Size = new System.Drawing.Size(188, 23);
            this.buttonStopBatch.TabIndex = 9;
            this.buttonStopBatch.Text = "Stop batch process";
            this.buttonStopBatch.UseVisualStyleBackColor = true;
            this.buttonStopBatch.Visible = false;
            this.buttonStopBatch.Click += new System.EventHandler(this.buttonStopBatch_Click);
            // 
            // wxCompilerConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelButtons);
            this.Name = "wxCompilerConsole";
            this.Size = new System.Drawing.Size(555, 295);
            this.Load += new System.EventHandler(this.wxCompilerConsole_Load);
            this.groupBox1.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Diagnostics.Process process1;
        public System.Windows.Forms.Button ButtonSaveCompiler;
        public System.Windows.Forms.Button ButtonStopCompiler;
        public System.Windows.Forms.Button ButtonFindError;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelButtons;
        private ScintillaTextEditor.TextEditor RTB_Output;
        public System.Windows.Forms.Button buttonStopBatch;
        private System.Windows.Forms.Button ButtonPauseCompiler;
    }
}
