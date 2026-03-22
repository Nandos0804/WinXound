namespace WinXound_Net
{
    partial class wxCodeRepositoryModifyForm
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSaveExit = new System.Windows.Forms.Button();
            this.UDOTextEditor = new System.Windows.Forms.RichTextBox();
            this.groupBoxFile = new System.Windows.Forms.GroupBox();
            this.groupBoxFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.Location = new System.Drawing.Point(3, 242);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(107, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSaveExit
            // 
            this.buttonSaveExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSaveExit.Location = new System.Drawing.Point(112, 242);
            this.buttonSaveExit.Name = "buttonSaveExit";
            this.buttonSaveExit.Size = new System.Drawing.Size(107, 23);
            this.buttonSaveExit.TabIndex = 2;
            this.buttonSaveExit.Text = "&Save and Exit";
            this.buttonSaveExit.UseVisualStyleBackColor = true;
            this.buttonSaveExit.Click += new System.EventHandler(this.buttonSaveExit_Click);
            // 
            // UDOTextEditor
            // 
            this.UDOTextEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UDOTextEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UDOTextEditor.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UDOTextEditor.Location = new System.Drawing.Point(3, 16);
            this.UDOTextEditor.Name = "UDOTextEditor";
            this.UDOTextEditor.Size = new System.Drawing.Size(642, 216);
            this.UDOTextEditor.TabIndex = 0;
            this.UDOTextEditor.Text = "";
            this.UDOTextEditor.WordWrap = false;
            // 
            // groupBoxFile
            // 
            this.groupBoxFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFile.Controls.Add(this.UDOTextEditor);
            this.groupBoxFile.Location = new System.Drawing.Point(3, 3);
            this.groupBoxFile.Name = "groupBoxFile";
            this.groupBoxFile.Size = new System.Drawing.Size(648, 235);
            this.groupBoxFile.TabIndex = 4;
            this.groupBoxFile.TabStop = false;
            // 
            // wxCodeRepositoryModifyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 268);
            this.Controls.Add(this.groupBoxFile);
            this.Controls.Add(this.buttonSaveExit);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "wxCodeRepositoryModifyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Code Repository Text Editor";
            this.Load += new System.EventHandler(this.wxCodeRepositoryModifyForm_Load);
            this.groupBoxFile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSaveExit;
        private System.Windows.Forms.RichTextBox UDOTextEditor;
        private System.Windows.Forms.GroupBox groupBoxFile;
    }
}