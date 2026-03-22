namespace WinXound_Net
{
    partial class wxCSoundOpcodesRepository
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
            this.treeViewOpcodes = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.wxIntelliTipRepository = new WinXound_Net.wxIntelliTip();
            this.SuspendLayout();
            // 
            // treeViewOpcodes
            // 
            this.treeViewOpcodes.AllowDrop = true;
            this.treeViewOpcodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewOpcodes.Location = new System.Drawing.Point(0, 20);
            this.treeViewOpcodes.Name = "treeViewOpcodes";
            this.treeViewOpcodes.Size = new System.Drawing.Size(607, 245);
            this.treeViewOpcodes.TabIndex = 0;
            this.treeViewOpcodes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewOpcodes_AfterSelect);
            this.treeViewOpcodes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeViewOpcodes_MouseMove);
            this.treeViewOpcodes.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewOpcodes_MouseDown);
            this.treeViewOpcodes.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewOpcodes_DragEnter);
            this.treeViewOpcodes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewOpcodes_KeyDown);
            this.treeViewOpcodes.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewOpcodes_ItemDrag);
            this.treeViewOpcodes.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewOpcodes_DragOver);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(607, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Info: Drag and Drop to insert opcode name  /  Ctrl + Drag and Drop to insert opco" +
                "de synopsis";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wxIntelliTipRepository
            // 
            this.wxIntelliTipRepository.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.wxIntelliTipRepository.Location = new System.Drawing.Point(0, 265);
            this.wxIntelliTipRepository.Name = "wxIntelliTipRepository";
            this.wxIntelliTipRepository.Size = new System.Drawing.Size(607, 36);
            this.wxIntelliTipRepository.TabIndex = 1;
            this.wxIntelliTipRepository.TabStop = false;
            // 
            // wxCSoundOpcodesRepository
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeViewOpcodes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.wxIntelliTipRepository);
            this.Name = "wxCSoundOpcodesRepository";
            this.Size = new System.Drawing.Size(607, 301);
            this.Load += new System.EventHandler(this.wxCSoundOpcodesRepository_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView treeViewOpcodes;
        private wxIntelliTip wxIntelliTipRepository;
        private System.Windows.Forms.Label label1;

    }
}
