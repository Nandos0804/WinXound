namespace WinXound_Net
{
    partial class FormAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            this.LinkLabel3 = new System.Windows.Forms.LinkLabel();
            this.Label2 = new System.Windows.Forms.Label();
            this.LinkLabel2 = new System.Windows.Forms.LinkLabel();
            this.LinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.labelMail = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LinkLabel3
            // 
            this.LinkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkLabel3.AutoSize = true;
            this.LinkLabel3.BackColor = System.Drawing.Color.Transparent;
            this.LinkLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinkLabel3.Location = new System.Drawing.Point(87, 377);
            this.LinkLabel3.Name = "LinkLabel3";
            this.LinkLabel3.Size = new System.Drawing.Size(187, 16);
            this.LinkLabel3.TabIndex = 40;
            this.LinkLabel3.TabStop = true;
            this.LinkLabel3.Text = "http://winxound.codeplex.com/";
            this.LinkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel3_LinkClicked);
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label2.AutoSize = true;
            this.Label2.BackColor = System.Drawing.Color.Transparent;
            this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(20, 377);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(64, 16);
            this.Label2.TabIndex = 39;
            this.Label2.Text = "Website: ";
            // 
            // LinkLabel2
            // 
            this.LinkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkLabel2.AutoSize = true;
            this.LinkLabel2.BackColor = System.Drawing.Color.Transparent;
            this.LinkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinkLabel2.Location = new System.Drawing.Point(165, 353);
            this.LinkLabel2.Name = "LinkLabel2";
            this.LinkLabel2.Size = new System.Drawing.Size(151, 16);
            this.LinkLabel2.TabIndex = 32;
            this.LinkLabel2.TabStop = true;
            this.LinkLabel2.Text = "stefano_bonetti@alice.it";
            this.LinkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2_LinkClicked);
            // 
            // LinkLabel1
            // 
            this.LinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkLabel1.AutoSize = true;
            this.LinkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.LinkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinkLabel1.Location = new System.Drawing.Point(20, 353);
            this.LinkLabel1.Name = "LinkLabel1";
            this.LinkLabel1.Size = new System.Drawing.Size(135, 16);
            this.LinkLabel1.TabIndex = 31;
            this.LinkLabel1.TabStop = true;
            this.LinkLabel1.Text = "stefano_bonetti@tin.it";
            this.LinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // labelMail
            // 
            this.labelMail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMail.AutoSize = true;
            this.labelMail.BackColor = System.Drawing.Color.Transparent;
            this.labelMail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMail.Location = new System.Drawing.Point(20, 335);
            this.labelMail.Name = "labelMail";
            this.labelMail.Size = new System.Drawing.Size(280, 16);
            this.labelMail.TabIndex = 41;
            this.labelMail.Text = "For any problem or suggestion please Mail to:";
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(624, 405);
            this.Controls.Add(this.labelMail);
            this.Controls.Add(this.LinkLabel3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.LinkLabel2);
            this.Controls.Add(this.LinkLabel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " About WinXound.Net";
            this.Load += new System.EventHandler(this.FormAbout_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormAbout_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAbout_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.LinkLabel LinkLabel3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.LinkLabel LinkLabel2;
        internal System.Windows.Forms.LinkLabel LinkLabel1;
        internal System.Windows.Forms.Label labelMail;

    }
}
