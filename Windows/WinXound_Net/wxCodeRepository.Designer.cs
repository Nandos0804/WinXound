namespace WinXound_Net
{
    partial class wxCodeRepository
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TreeViewUserData = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ContextModifyUDO = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextDeleteUDO = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextRenameUDO = new System.Windows.Forms.ToolStripMenuItem();
            this.TextBoxUdo = new ScintillaTextEditor.TextEditor();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TreeViewUserData);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.TextBoxUdo);
            this.splitContainer1.Size = new System.Drawing.Size(630, 381);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Resize += new System.EventHandler(this.splitContainer1_Resize);
            // 
            // TreeViewUserData
            // 
            this.TreeViewUserData.AllowDrop = true;
            this.TreeViewUserData.ContextMenuStrip = this.contextMenuStrip1;
            this.TreeViewUserData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewUserData.HideSelection = false;
            this.TreeViewUserData.Location = new System.Drawing.Point(0, 0);
            this.TreeViewUserData.Name = "TreeViewUserData";
            this.TreeViewUserData.Size = new System.Drawing.Size(200, 381);
            this.TreeViewUserData.TabIndex = 0;
            this.TreeViewUserData.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxUdo_DragDrop);
            this.TreeViewUserData.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewUserData_AfterSelect);
            this.TreeViewUserData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TextBoxUdo_MouseMove);
            this.TreeViewUserData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewUserData_MouseDown);
            this.TreeViewUserData.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxUdo_DragEnter);
            this.TreeViewUserData.DragOver += new System.Windows.Forms.DragEventHandler(this.TextBoxUdo_DragOver);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContextModifyUDO,
            this.ContextDeleteUDO,
            this.toolStripSeparator1,
            this.ContextRenameUDO});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(114, 76);
            // 
            // ContextModifyUDO
            // 
            this.ContextModifyUDO.Name = "ContextModifyUDO";
            this.ContextModifyUDO.Size = new System.Drawing.Size(113, 22);
            this.ContextModifyUDO.Text = "Modify";
            this.ContextModifyUDO.Click += new System.EventHandler(this.ContextModifyUDO_Click);
            // 
            // ContextDeleteUDO
            // 
            this.ContextDeleteUDO.Name = "ContextDeleteUDO";
            this.ContextDeleteUDO.Size = new System.Drawing.Size(113, 22);
            this.ContextDeleteUDO.Text = "Delete";
            this.ContextDeleteUDO.Click += new System.EventHandler(this.ContextDeleteUDO_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(110, 6);
            // 
            // ContextRenameUDO
            // 
            this.ContextRenameUDO.Name = "ContextRenameUDO";
            this.ContextRenameUDO.Size = new System.Drawing.Size(113, 22);
            this.ContextRenameUDO.Text = "Rename";
            this.ContextRenameUDO.Click += new System.EventHandler(this.ContextRenameUDO_Click);
            // 
            // TextBoxUdo
            // 
            this.TextBoxUdo.AllowCaretBeyondEOL = false;
            this.TextBoxUdo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBoxUdo.FileName = "";
            this.TextBoxUdo.Location = new System.Drawing.Point(0, 0);
            this.TextBoxUdo.MarkCaretLine = false;
            this.TextBoxUdo.Name = "TextBoxUdo";
            this.TextBoxUdo.ReadOnly = false;
            this.TextBoxUdo.ShowEOLMarker = false;
            this.TextBoxUdo.ShowLineNumbers = true;
            this.TextBoxUdo.ShowMatchingBracket = false;
            this.TextBoxUdo.ShowSpaces = false;
            this.TextBoxUdo.ShowVerticalRuler = false;
            this.TextBoxUdo.Size = new System.Drawing.Size(426, 381);
            this.TextBoxUdo.TabIndent = 8;
            this.TextBoxUdo.TabIndex = 0;
            this.TextBoxUdo.TextEditorFont = new System.Drawing.Font("Courier New", 10F);
            // 
            // wxCodeRepository
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "wxCodeRepository";
            this.Size = new System.Drawing.Size(630, 381);
            this.Load += new System.EventHandler(this.wxCodeRepository_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ContextDeleteUDO;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ContextRenameUDO;
        private System.Windows.Forms.TreeView TreeViewUserData;
        private ScintillaTextEditor.TextEditor TextBoxUdo;
        private System.Windows.Forms.ToolStripMenuItem ContextModifyUDO;

    }
}
