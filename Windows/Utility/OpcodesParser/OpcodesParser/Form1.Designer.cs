namespace OpcodesParser
{
    partial class Form1
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
            this.RTB_Output = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.ButtonStopCompiler = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RTB_Output
            // 
            this.RTB_Output.BackColor = System.Drawing.Color.Ivory;
            this.RTB_Output.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RTB_Output.Location = new System.Drawing.Point(12, 54);
            this.RTB_Output.Multiline = true;
            this.RTB_Output.Name = "RTB_Output";
            this.RTB_Output.ReadOnly = true;
            this.RTB_Output.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.RTB_Output.Size = new System.Drawing.Size(667, 206);
            this.RTB_Output.TabIndex = 19;
            this.RTB_Output.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.DarkRed;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(477, 39);
            this.label1.TabIndex = 18;
            this.label1.Text = "Notes:\r\nImproper use of this utility may cause WinXound to become unstable.\r\nPlea" +
                "se read carefully the instructions on how to use this utility (Chapter \"Create N" +
                "ew Opcodes File\").";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 295);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(669, 23);
            this.progressBar1.TabIndex = 16;
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(266, 271);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(28, 13);
            this.labelInfo.TabIndex = 15;
            this.labelInfo.Text = "Info:";
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStart.Location = new System.Drawing.Point(12, 266);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(109, 23);
            this.buttonStart.TabIndex = 14;
            this.buttonStart.Text = "&Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // ButtonStopCompiler
            // 
            this.ButtonStopCompiler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonStopCompiler.Enabled = false;
            this.ButtonStopCompiler.Location = new System.Drawing.Point(127, 266);
            this.ButtonStopCompiler.Name = "ButtonStopCompiler";
            this.ButtonStopCompiler.Size = new System.Drawing.Size(109, 23);
            this.ButtonStopCompiler.TabIndex = 13;
            this.ButtonStopCompiler.Text = "&Abort";
            this.ButtonStopCompiler.UseVisualStyleBackColor = true;
            this.ButtonStopCompiler.Click += new System.EventHandler(this.ButtonStopCompiler_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 330);
            this.Controls.Add(this.RTB_Output);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.ButtonStopCompiler);
            this.Name = "Form1";
            this.Text = "WinXound Opcodes Parser";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox RTB_Output;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelInfo;
        public System.Windows.Forms.Button buttonStart;
        public System.Windows.Forms.Button ButtonStopCompiler;
    }
}

