namespace WinXound_Net
{
    partial class FormInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInfo));
            this.Label8 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.LabelInfo = new System.Windows.Forms.Label();
            this.LabelVer = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.BackColor = System.Drawing.Color.Transparent;
            this.Label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(133, 62);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(218, 20);
            this.Label8.TabIndex = 45;
            this.Label8.Text = "developed by Stefano Bonetti";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.BackColor = System.Drawing.Color.Transparent;
            this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(61, 42);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(363, 20);
            this.Label3.TabIndex = 44;
            this.Label3.Text = "an open source editor for CSound and CSoundAV";
            // 
            // LabelInfo
            // 
            this.LabelInfo.BackColor = System.Drawing.Color.Transparent;
            this.LabelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelInfo.Location = new System.Drawing.Point(57, 90);
            this.LabelInfo.Name = "LabelInfo";
            this.LabelInfo.Size = new System.Drawing.Size(371, 16);
            this.LabelInfo.TabIndex = 43;
            this.LabelInfo.Text = "LabelInfo";
            this.LabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LabelInfo.TextChanged += new System.EventHandler(this.LabelInfo_TextChanged);
            // 
            // LabelVer
            // 
            this.LabelVer.BackColor = System.Drawing.Color.Transparent;
            this.LabelVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelVer.Location = new System.Drawing.Point(13, 9);
            this.LabelVer.Name = "LabelVer";
            this.LabelVer.Size = new System.Drawing.Size(458, 37);
            this.LabelVer.TabIndex = 42;
            this.LabelVer.Text = "WinXound.Net";
            this.LabelVer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(485, 120);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.LabelInfo);
            this.Controls.Add(this.LabelVer);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormInfo";
            this.Load += new System.EventHandler(this.FormInfo_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormInfo_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label3;
        public System.Windows.Forms.Label LabelInfo;
        internal System.Windows.Forms.Label LabelVer;
    }
}