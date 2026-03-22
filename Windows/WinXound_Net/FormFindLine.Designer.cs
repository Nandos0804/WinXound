namespace WinXound_Net
{
    partial class FormFindLine
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
            this.mNumber = new System.Windows.Forms.NumericUpDown();
            this.ButtonExit = new System.Windows.Forms.Button();
            this.ButtonFind = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // mNumber
            // 
            this.mNumber.Location = new System.Drawing.Point(6, 6);
            this.mNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.mNumber.Name = "mNumber";
            this.mNumber.Size = new System.Drawing.Size(51, 20);
            this.mNumber.TabIndex = 0;
            this.mNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ButtonExit
            // 
            this.ButtonExit.Location = new System.Drawing.Point(144, 5);
            this.ButtonExit.Name = "ButtonExit";
            this.ButtonExit.Size = new System.Drawing.Size(75, 22);
            this.ButtonExit.TabIndex = 2;
            this.ButtonExit.Text = "E&xit";
            this.ButtonExit.UseVisualStyleBackColor = true;
            this.ButtonExit.Click += new System.EventHandler(this.ButtonExit_Click);
            // 
            // ButtonFind
            // 
            this.ButtonFind.Location = new System.Drawing.Point(63, 5);
            this.ButtonFind.Name = "ButtonFind";
            this.ButtonFind.Size = new System.Drawing.Size(75, 22);
            this.ButtonFind.TabIndex = 1;
            this.ButtonFind.Text = "&Find";
            this.ButtonFind.UseVisualStyleBackColor = true;
            this.ButtonFind.Click += new System.EventHandler(this.ButtonFind_Click);
            // 
            // FormFindLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(225, 32);
            this.ControlBox = false;
            this.Controls.Add(this.mNumber);
            this.Controls.Add(this.ButtonExit);
            this.Controls.Add(this.ButtonFind);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "FormFindLine";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Find Line";
            this.Load += new System.EventHandler(this.FormFindLine_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFindLine_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.mNumber)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.NumericUpDown mNumber;
        internal System.Windows.Forms.Button ButtonExit;
        internal System.Windows.Forms.Button ButtonFind;
    }
}