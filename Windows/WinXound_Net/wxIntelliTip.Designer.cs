namespace WinXound_Net
{
    partial class wxIntelliTip
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
            this.LabelParams = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelParams
            // 
            this.LabelParams.AutoEllipsis = true;
            this.LabelParams.BackColor = System.Drawing.Color.LightYellow;
            this.LabelParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelParams.ForeColor = System.Drawing.Color.MidnightBlue;
            this.LabelParams.Location = new System.Drawing.Point(0, 18);
            this.LabelParams.Name = "LabelParams";
            this.LabelParams.Size = new System.Drawing.Size(540, 18);
            this.LabelParams.TabIndex = 3;
            this.LabelParams.Text = "Parameters";
            // 
            // LabelTitle
            // 
            this.LabelTitle.AutoEllipsis = true;
            this.LabelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.LabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.LabelTitle.Location = new System.Drawing.Point(0, 0);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(540, 18);
            this.LabelTitle.TabIndex = 2;
            this.LabelTitle.Text = "Opcode";
            // 
            // wxIntelliTip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LabelParams);
            this.Controls.Add(this.LabelTitle);
            this.DoubleBuffered = true;
            this.Name = "wxIntelliTip";
            this.Size = new System.Drawing.Size(540, 36);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label LabelParams;
        internal System.Windows.Forms.Label LabelTitle;
    }
}
