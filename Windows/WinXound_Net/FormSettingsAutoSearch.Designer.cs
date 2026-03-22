namespace WinXound_Net
{
    partial class FormSettingsAutoSearch
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
            this.buttonApply = new System.Windows.Forms.Button();
            this.clbCSound = new System.Windows.Forms.CheckedListBox();
            this.clbCSoundManual = new System.Windows.Forms.CheckedListBox();
            this.clbPython = new System.Windows.Forms.CheckedListBox();
            this.clbPythonIdle = new System.Windows.Forms.CheckedListBox();
            this.clbLua = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelCSoundGui = new System.Windows.Forms.Label();
            this.labelCSoundManual = new System.Windows.Forms.Label();
            this.labelCSoundExe = new System.Windows.Forms.Label();
            this.clbCSoundGui = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelPythonIdle = new System.Windows.Forms.Label();
            this.labelPythonExe = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelLuaExe = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(294, 400);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(375, 400);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // clbCSound
            // 
            this.clbCSound.CheckOnClick = true;
            this.clbCSound.Enabled = false;
            this.clbCSound.FormattingEnabled = true;
            this.clbCSound.Location = new System.Drawing.Point(6, 30);
            this.clbCSound.Name = "clbCSound";
            this.clbCSound.Size = new System.Drawing.Size(433, 34);
            this.clbCSound.TabIndex = 0;
            this.clbCSound.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // clbCSoundManual
            // 
            this.clbCSoundManual.CheckOnClick = true;
            this.clbCSoundManual.Enabled = false;
            this.clbCSoundManual.FormattingEnabled = true;
            this.clbCSoundManual.Location = new System.Drawing.Point(6, 83);
            this.clbCSoundManual.Name = "clbCSoundManual";
            this.clbCSoundManual.Size = new System.Drawing.Size(433, 34);
            this.clbCSoundManual.TabIndex = 1;
            this.clbCSoundManual.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // clbPython
            // 
            this.clbPython.CheckOnClick = true;
            this.clbPython.Enabled = false;
            this.clbPython.FormattingEnabled = true;
            this.clbPython.Location = new System.Drawing.Point(6, 32);
            this.clbPython.Name = "clbPython";
            this.clbPython.Size = new System.Drawing.Size(432, 34);
            this.clbPython.TabIndex = 0;
            this.clbPython.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // clbPythonIdle
            // 
            this.clbPythonIdle.CheckOnClick = true;
            this.clbPythonIdle.Enabled = false;
            this.clbPythonIdle.FormattingEnabled = true;
            this.clbPythonIdle.Location = new System.Drawing.Point(6, 85);
            this.clbPythonIdle.Name = "clbPythonIdle";
            this.clbPythonIdle.Size = new System.Drawing.Size(432, 34);
            this.clbPythonIdle.TabIndex = 1;
            this.clbPythonIdle.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // clbLua
            // 
            this.clbLua.CheckOnClick = true;
            this.clbLua.Enabled = false;
            this.clbLua.FormattingEnabled = true;
            this.clbLua.Location = new System.Drawing.Point(6, 33);
            this.clbLua.Name = "clbLua";
            this.clbLua.Size = new System.Drawing.Size(432, 34);
            this.clbLua.TabIndex = 0;
            this.clbLua.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelCSoundGui);
            this.groupBox1.Controls.Add(this.clbCSoundManual);
            this.groupBox1.Controls.Add(this.labelCSoundManual);
            this.groupBox1.Controls.Add(this.clbCSound);
            this.groupBox1.Controls.Add(this.labelCSoundExe);
            this.groupBox1.Controls.Add(this.clbCSoundGui);
            this.groupBox1.Location = new System.Drawing.Point(5, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(445, 178);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CSound";
            // 
            // labelCSoundGui
            // 
            this.labelCSoundGui.AutoSize = true;
            this.labelCSoundGui.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelCSoundGui.Location = new System.Drawing.Point(3, 120);
            this.labelCSoundGui.Name = "labelCSoundGui";
            this.labelCSoundGui.Size = new System.Drawing.Size(124, 13);
            this.labelCSoundGui.TabIndex = 7;
            this.labelCSoundGui.Text = "CSound GUI: (not found)";
            this.labelCSoundGui.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCSoundManual
            // 
            this.labelCSoundManual.AutoSize = true;
            this.labelCSoundManual.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelCSoundManual.Location = new System.Drawing.Point(3, 67);
            this.labelCSoundManual.Name = "labelCSoundManual";
            this.labelCSoundManual.Size = new System.Drawing.Size(140, 13);
            this.labelCSoundManual.TabIndex = 6;
            this.labelCSoundManual.Text = "CSound Manual: (not found)";
            this.labelCSoundManual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCSoundExe
            // 
            this.labelCSoundExe.AutoSize = true;
            this.labelCSoundExe.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelCSoundExe.Location = new System.Drawing.Point(3, 16);
            this.labelCSoundExe.Name = "labelCSoundExe";
            this.labelCSoundExe.Size = new System.Drawing.Size(122, 13);
            this.labelCSoundExe.TabIndex = 5;
            this.labelCSoundExe.Text = "CSound exe: (not found)";
            this.labelCSoundExe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // clbCSoundGui
            // 
            this.clbCSoundGui.CheckOnClick = true;
            this.clbCSoundGui.Enabled = false;
            this.clbCSoundGui.FormattingEnabled = true;
            this.clbCSoundGui.Location = new System.Drawing.Point(6, 136);
            this.clbCSoundGui.Name = "clbCSoundGui";
            this.clbCSoundGui.Size = new System.Drawing.Size(433, 34);
            this.clbCSoundGui.TabIndex = 2;
            this.clbCSoundGui.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clb_ItemCheck);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelPythonIdle);
            this.groupBox2.Controls.Add(this.labelPythonExe);
            this.groupBox2.Controls.Add(this.clbPythonIdle);
            this.groupBox2.Controls.Add(this.clbPython);
            this.groupBox2.Location = new System.Drawing.Point(5, 186);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(445, 127);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Python";
            // 
            // labelPythonIdle
            // 
            this.labelPythonIdle.AutoSize = true;
            this.labelPythonIdle.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelPythonIdle.Location = new System.Drawing.Point(3, 69);
            this.labelPythonIdle.Name = "labelPythonIdle";
            this.labelPythonIdle.Size = new System.Drawing.Size(117, 13);
            this.labelPythonIdle.TabIndex = 8;
            this.labelPythonIdle.Text = "Python Idle: (not found)";
            this.labelPythonIdle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPythonExe
            // 
            this.labelPythonExe.AutoSize = true;
            this.labelPythonExe.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelPythonExe.Location = new System.Drawing.Point(3, 16);
            this.labelPythonExe.Name = "labelPythonExe";
            this.labelPythonExe.Size = new System.Drawing.Size(117, 13);
            this.labelPythonExe.TabIndex = 7;
            this.labelPythonExe.Text = "Python exe: (not found)";
            this.labelPythonExe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelLuaExe);
            this.groupBox3.Controls.Add(this.clbLua);
            this.groupBox3.Location = new System.Drawing.Point(5, 319);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(445, 75);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Lua";
            // 
            // labelLuaExe
            // 
            this.labelLuaExe.AutoSize = true;
            this.labelLuaExe.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelLuaExe.Location = new System.Drawing.Point(3, 17);
            this.labelLuaExe.Name = "labelLuaExe";
            this.labelLuaExe.Size = new System.Drawing.Size(102, 13);
            this.labelLuaExe.TabIndex = 9;
            this.labelLuaExe.Text = "Lua exe: (not found)";
            this.labelLuaExe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormSettingsAutoSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(456, 428);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "FormSettingsAutoSearch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WinXound Settings - Compilers paths";
            this.Load += new System.EventHandler(this.FormSettingsAutoSearch_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSettingsAutoSearch_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.CheckedListBox clbCSound;
        private System.Windows.Forms.CheckedListBox clbCSoundManual;
        private System.Windows.Forms.CheckedListBox clbPython;
        private System.Windows.Forms.CheckedListBox clbPythonIdle;
        private System.Windows.Forms.CheckedListBox clbLua;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckedListBox clbCSoundGui;
        private System.Windows.Forms.Label labelCSoundGui;
        private System.Windows.Forms.Label labelCSoundManual;
        private System.Windows.Forms.Label labelCSoundExe;
        private System.Windows.Forms.Label labelPythonIdle;
        private System.Windows.Forms.Label labelPythonExe;
        private System.Windows.Forms.Label labelLuaExe;
    }
}