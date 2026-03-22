namespace WinXound_Net
{
    partial class FormInsertHeader
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
            this.nControlRate = new System.Windows.Forms.NumericUpDown();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonInsert = new System.Windows.Forms.Button();
            this.RadioButtonCurrent = new System.Windows.Forms.RadioButton();
            this.RadioButtonStart = new System.Windows.Forms.RadioButton();
            this.nChannels = new System.Windows.Forms.NumericUpDown();
            this.Label3 = new System.Windows.Forms.Label();
            this.cbSampleRate = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nControlRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nChannels)).BeginInit();
            this.SuspendLayout();
            // 
            // nControlRate
            // 
            this.nControlRate.Location = new System.Drawing.Point(94, 48);
            this.nControlRate.Maximum = new decimal(new int[] {
            96000,
            0,
            0,
            0});
            this.nControlRate.Name = "nControlRate";
            this.nControlRate.Size = new System.Drawing.Size(88, 20);
            this.nControlRate.TabIndex = 1;
            this.nControlRate.Value = new decimal(new int[] {
            4410,
            0,
            0,
            0});
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Location = new System.Drawing.Point(209, 37);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "&Cancel";
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonInsert
            // 
            this.ButtonInsert.Location = new System.Drawing.Point(209, 8);
            this.ButtonInsert.Name = "ButtonInsert";
            this.ButtonInsert.Size = new System.Drawing.Size(75, 23);
            this.ButtonInsert.TabIndex = 5;
            this.ButtonInsert.Text = "&Insert";
            this.ButtonInsert.Click += new System.EventHandler(this.ButtonInsert_Click);
            // 
            // RadioButtonCurrent
            // 
            this.RadioButtonCurrent.Location = new System.Drawing.Point(6, 152);
            this.RadioButtonCurrent.Name = "RadioButtonCurrent";
            this.RadioButtonCurrent.Size = new System.Drawing.Size(280, 24);
            this.RadioButtonCurrent.TabIndex = 4;
            this.RadioButtonCurrent.Text = "Insert at current position";
            // 
            // RadioButtonStart
            // 
            this.RadioButtonStart.Checked = true;
            this.RadioButtonStart.Location = new System.Drawing.Point(6, 128);
            this.RadioButtonStart.Name = "RadioButtonStart";
            this.RadioButtonStart.Size = new System.Drawing.Size(272, 24);
            this.RadioButtonStart.TabIndex = 3;
            this.RadioButtonStart.TabStop = true;
            this.RadioButtonStart.Text = "Insert at the beginning of <CsInstruments> section";
            // 
            // nChannels
            // 
            this.nChannels.Location = new System.Drawing.Point(94, 88);
            this.nChannels.Name = "nChannels";
            this.nChannels.Size = new System.Drawing.Size(88, 20);
            this.nChannels.TabIndex = 2;
            this.nChannels.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(6, 88);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(80, 23);
            this.Label3.TabIndex = 14;
            this.Label3.Text = "Channels:";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbSampleRate
            // 
            this.cbSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSampleRate.Items.AddRange(new object[] {
            "11025",
            "22050",
            "32000",
            "44100",
            "48000",
            "88200",
            "96000"});
            this.cbSampleRate.Location = new System.Drawing.Point(94, 8);
            this.cbSampleRate.Name = "cbSampleRate";
            this.cbSampleRate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbSampleRate.Size = new System.Drawing.Size(88, 21);
            this.cbSampleRate.TabIndex = 0;
            this.cbSampleRate.SelectedIndexChanged += new System.EventHandler(this.cbSampleRate_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(6, 8);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(80, 23);
            this.Label1.TabIndex = 10;
            this.Label1.Text = "Sample rate:";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(6, 48);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(80, 23);
            this.Label2.TabIndex = 11;
            this.Label2.Text = "Control rate:";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormInsertHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(292, 181);
            this.ControlBox = false;
            this.Controls.Add(this.nControlRate);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonInsert);
            this.Controls.Add(this.RadioButtonCurrent);
            this.Controls.Add(this.RadioButtonStart);
            this.Controls.Add(this.nChannels);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.cbSampleRate);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Name = "FormInsertHeader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Insert Orchestra Header";
            this.Load += new System.EventHandler(this.FormInsertHeader_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormInsertHeader_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.nControlRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nChannels)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.NumericUpDown nControlRate;
        internal System.Windows.Forms.Button ButtonCancel;
        internal System.Windows.Forms.Button ButtonInsert;
        internal System.Windows.Forms.RadioButton RadioButtonCurrent;
        internal System.Windows.Forms.RadioButton RadioButtonStart;
        internal System.Windows.Forms.NumericUpDown nChannels;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ComboBox cbSampleRate;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label Label2;
    }
}