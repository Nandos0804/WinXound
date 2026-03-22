namespace WinXound_Net
{
    partial class wxEditor
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
            //FindCodeStructure.Dispose();
            //System.GC.Collect();

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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("1. <CsoundSynthesizer>");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("2. <CsOptions>");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("3. <CsInstruments>");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("4. <CsScore>");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(wxEditor));
            this.splitContainerBase = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.TreeViewStructure = new WinXound_Net.wxTreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.ListBoxBookmarks = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textEditor = new ScintillaTextEditor.TextEditor();
            this.toolStripOrcSco = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.LabelOrcScoName = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonSwitchOrcSco = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClearOrcSco = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonBrowseOrcSco = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonShowList = new System.Windows.Forms.ToolStripButton();
            this.wxIntelliTip1 = new WinXound_Net.wxIntelliTip();
            this.TimerSearch = new System.Windows.Forms.Timer(this.components);
            this.splitContainerBase.Panel1.SuspendLayout();
            this.splitContainerBase.Panel2.SuspendLayout();
            this.splitContainerBase.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStripOrcSco.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerBase
            // 
            this.splitContainerBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBase.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerBase.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBase.Name = "splitContainerBase";
            // 
            // splitContainerBase.Panel1
            // 
            this.splitContainerBase.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainerBase.Panel1MinSize = 200;
            // 
            // splitContainerBase.Panel2
            // 
            this.splitContainerBase.Panel2.Controls.Add(this.textEditor);
            this.splitContainerBase.Panel2.Controls.Add(this.toolStripOrcSco);
            this.splitContainerBase.Panel2.Controls.Add(this.wxIntelliTip1);
            this.splitContainerBase.Size = new System.Drawing.Size(795, 423);
            this.splitContainerBase.SplitterDistance = 200;
            this.splitContainerBase.TabIndex = 0;
            this.splitContainerBase.TabStop = false;
            this.splitContainerBase.Resize += new System.EventHandler(this.splitContainerBase_Resize);
            this.splitContainerBase.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainerBase_SplitterMoved);
            this.splitContainerBase.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitContainerBase_MouseDown);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.TreeViewStructure);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.ListBoxBookmarks);
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Size = new System.Drawing.Size(200, 423);
            this.splitContainer2.SplitterDistance = 296;
            this.splitContainer2.TabIndex = 3;
            this.splitContainer2.TabStop = false;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // TreeViewStructure
            // 
            this.TreeViewStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewStructure.Location = new System.Drawing.Point(0, 13);
            this.TreeViewStructure.Name = "TreeViewStructure";
            treeNode1.Name = "<CsoundSynthesizer>";
            treeNode1.Text = "1. <CsoundSynthesizer>";
            treeNode2.Name = "<CsOptions>";
            treeNode2.Text = "2. <CsOptions>";
            treeNode3.Name = "<CsInstruments>";
            treeNode3.Text = "3. <CsInstruments>";
            treeNode4.Name = "<CsScore>";
            treeNode4.Text = "4. <CsScore>";
            this.TreeViewStructure.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4});
            this.TreeViewStructure.Size = new System.Drawing.Size(200, 283);
            this.TreeViewStructure.TabIndex = 4;
            this.TreeViewStructure.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewStructure_NodeMouseClick);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LightGray;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Explorer";
            // 
            // ListBoxBookmarks
            // 
            this.ListBoxBookmarks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListBoxBookmarks.IntegralHeight = false;
            this.ListBoxBookmarks.Location = new System.Drawing.Point(0, 13);
            this.ListBoxBookmarks.Name = "ListBoxBookmarks";
            this.ListBoxBookmarks.Size = new System.Drawing.Size(200, 110);
            this.ListBoxBookmarks.TabIndex = 5;
            this.ListBoxBookmarks.TabStop = false;
            this.ListBoxBookmarks.SelectedIndexChanged += new System.EventHandler(this.ListBoxBookmarks_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.LightGray;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Bookmarks";
            // 
            // textEditor
            // 
            this.textEditor.AllowCaretBeyondEOL = true;
            this.textEditor.AllowDrop = true;
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.FileName = "";
            this.textEditor.Location = new System.Drawing.Point(0, 25);
            this.textEditor.MarkCaretLine = false;
            this.textEditor.Name = "textEditor";
            this.textEditor.ReadOnly = false;
            this.textEditor.ShowEOLMarker = false;
            this.textEditor.ShowLineNumbers = true;
            this.textEditor.ShowMatchingBracket = false;
            this.textEditor.ShowSpaces = false;
            this.textEditor.ShowVerticalRuler = false;
            this.textEditor.Size = new System.Drawing.Size(591, 362);
            this.textEditor.TabIndent = 8;
            this.textEditor.TabIndex = 19;
            this.textEditor.TextEditorFont = new System.Drawing.Font("Courier New", 10F);
            // 
            // toolStripOrcSco
            // 
            this.toolStripOrcSco.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripOrcSco.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.LabelOrcScoName,
            this.toolStripButtonSwitchOrcSco,
            this.toolStripSeparator1,
            this.toolStripButtonClearOrcSco,
            this.toolStripButtonBrowseOrcSco,
            this.toolStripButtonShowList});
            this.toolStripOrcSco.Location = new System.Drawing.Point(0, 0);
            this.toolStripOrcSco.Name = "toolStripOrcSco";
            this.toolStripOrcSco.Size = new System.Drawing.Size(591, 25);
            this.toolStripOrcSco.TabIndex = 20;
            this.toolStripOrcSco.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(64, 22);
            this.toolStripLabel1.Text = "Linked with:";
            // 
            // LabelOrcScoName
            // 
            this.LabelOrcScoName.Name = "LabelOrcScoName";
            this.LabelOrcScoName.Size = new System.Drawing.Size(39, 22);
            this.LabelOrcScoName.Text = "(none)";
            this.LabelOrcScoName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripButtonSwitchOrcSco
            // 
            this.toolStripButtonSwitchOrcSco.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonSwitchOrcSco.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSwitchOrcSco.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSwitchOrcSco.Image")));
            this.toolStripButtonSwitchOrcSco.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSwitchOrcSco.Name = "toolStripButtonSwitchOrcSco";
            this.toolStripButtonSwitchOrcSco.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSwitchOrcSco.Text = "Open/Switch Orc/Sco";
            this.toolStripButtonSwitchOrcSco.ToolTipText = "Open/Switch between Orc/Sco files";
            this.toolStripButtonSwitchOrcSco.Click += new System.EventHandler(this.toolStripButtonSwitchOrcSco_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonClearOrcSco
            // 
            this.toolStripButtonClearOrcSco.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonClearOrcSco.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClearOrcSco.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClearOrcSco.Image")));
            this.toolStripButtonClearOrcSco.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearOrcSco.Name = "toolStripButtonClearOrcSco";
            this.toolStripButtonClearOrcSco.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonClearOrcSco.Text = "Clear current Link";
            this.toolStripButtonClearOrcSco.Click += new System.EventHandler(this.toolStripButtonClearOrcSco_Click);
            // 
            // toolStripButtonBrowseOrcSco
            // 
            this.toolStripButtonBrowseOrcSco.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonBrowseOrcSco.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBrowseOrcSco.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonBrowseOrcSco.Image")));
            this.toolStripButtonBrowseOrcSco.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBrowseOrcSco.Name = "toolStripButtonBrowseOrcSco";
            this.toolStripButtonBrowseOrcSco.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonBrowseOrcSco.Text = "Browse file";
            this.toolStripButtonBrowseOrcSco.Click += new System.EventHandler(this.toolStripButtonBrowseOrcSco_Click);
            // 
            // toolStripButtonShowList
            // 
            this.toolStripButtonShowList.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonShowList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonShowList.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonShowList.Image")));
            this.toolStripButtonShowList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonShowList.Name = "toolStripButtonShowList";
            this.toolStripButtonShowList.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonShowList.Text = "Select from list";
            this.toolStripButtonShowList.Click += new System.EventHandler(this.toolStripButtonShowList_Click);
            // 
            // wxIntelliTip1
            // 
            this.wxIntelliTip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.wxIntelliTip1.Location = new System.Drawing.Point(0, 387);
            this.wxIntelliTip1.Name = "wxIntelliTip1";
            this.wxIntelliTip1.Size = new System.Drawing.Size(591, 36);
            this.wxIntelliTip1.TabIndex = 18;
            this.wxIntelliTip1.TabStop = false;
            this.wxIntelliTip1.VisibleChanged += new System.EventHandler(this.wxIntelliTip1_VisibleChanged);
            // 
            // TimerSearch
            // 
            this.TimerSearch.Interval = 500;
            this.TimerSearch.Tick += new System.EventHandler(this.TimerSearch_Tick);
            // 
            // wxEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerBase);
            this.Name = "wxEditor";
            this.Size = new System.Drawing.Size(795, 423);
            this.Load += new System.EventHandler(this.wxEditor_Load);
            this.splitContainerBase.Panel1.ResumeLayout(false);
            this.splitContainerBase.Panel2.ResumeLayout(false);
            this.splitContainerBase.Panel2.PerformLayout();
            this.splitContainerBase.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.toolStripOrcSco.ResumeLayout(false);
            this.toolStripOrcSco.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerBase;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.ListBox ListBoxBookmarks;
        private System.Windows.Forms.Label label2;
        private wxIntelliTip wxIntelliTip1;
        private System.Windows.Forms.Timer TimerSearch;
        public ScintillaTextEditor.TextEditor textEditor;
        private wxTreeView TreeViewStructure;
        private System.Windows.Forms.ToolStrip toolStripOrcSco;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButtonBrowseOrcSco;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearOrcSco;
        private System.Windows.Forms.ToolStripButton toolStripButtonSwitchOrcSco;
        private System.Windows.Forms.ToolStripLabel LabelOrcScoName;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

    }
}
