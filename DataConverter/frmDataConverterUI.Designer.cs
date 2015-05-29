namespace DataConverter
{
    partial class frmDataConverterUI
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
            this.components = new System.ComponentModel.Container();
            Infragistics.Win.UltraWinTabControl.UltraTab ultraTab1 = new Infragistics.Win.UltraWinTabControl.UltraTab();
            Infragistics.Win.UltraWinTabControl.UltraTab ultraTab2 = new Infragistics.Win.UltraWinTabControl.UltraTab();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataConverterUI));
            this.ultraTabPageControl1 = new Infragistics.Win.UltraWinTabControl.UltraTabPageControl();
            this.pnlDGV = new System.Windows.Forms.Panel();
            this.tbPrefix = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCopyRegEx = new System.Windows.Forms.Button();
            this.btnApplyPrefix = new Infragistics.Win.Misc.UltraButton();
            this.cmbRegEx = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblRegEx = new System.Windows.Forms.Label();
            this.ultraTabPageControl2 = new Infragistics.Win.UltraWinTabControl.UltraTabPageControl();
            this.btnAddRow = new Infragistics.Win.Misc.UltraButton();
            this.btnRemoveRow = new Infragistics.Win.Misc.UltraButton();
            this.pnlNewColumns = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbErrorHandling = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbErrorName = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.label3 = new System.Windows.Forms.Label();
            this.ultraTabControl1 = new Infragistics.Win.UltraWinTabControl.UltraTabControl();
            this.ultraTabSharedControlsPage1 = new Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage();
            this.ultraMessageBox = new Infragistics.Win.UltraMessageBox.UltraMessageBoxManager(this.components);
            this.ultraTabPageControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPrefix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRegEx)).BeginInit();
            this.ultraTabPageControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbErrorName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTabControl1)).BeginInit();
            this.ultraTabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraTabPageControl1
            // 
            this.ultraTabPageControl1.Controls.Add(this.pnlDGV);
            this.ultraTabPageControl1.Controls.Add(this.tbPrefix);
            this.ultraTabPageControl1.Controls.Add(this.label1);
            this.ultraTabPageControl1.Controls.Add(this.btnCopyRegEx);
            this.ultraTabPageControl1.Controls.Add(this.btnApplyPrefix);
            this.ultraTabPageControl1.Controls.Add(this.cmbRegEx);
            this.ultraTabPageControl1.Controls.Add(this.lblRegEx);
            this.ultraTabPageControl1.Location = new System.Drawing.Point(1, 23);
            this.ultraTabPageControl1.Name = "ultraTabPageControl1";
            this.ultraTabPageControl1.Size = new System.Drawing.Size(1051, 412);
            // 
            // pnlDGV
            // 
            this.pnlDGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDGV.Location = new System.Drawing.Point(7, 3);
            this.pnlDGV.Name = "pnlDGV";
            this.pnlDGV.Size = new System.Drawing.Size(1041, 376);
            this.pnlDGV.TabIndex = 12;
            // 
            // tbPrefix
            // 
            this.tbPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbPrefix.Location = new System.Drawing.Point(73, 386);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(126, 21);
            this.tbPrefix.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 390);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Alias Prefix";
            // 
            // btnCopyRegEx
            // 
            this.btnCopyRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopyRegEx.BackgroundImage = global::DataConverter.Properties.Resources.copy;
            this.btnCopyRegEx.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopyRegEx.Location = new System.Drawing.Point(463, 385);
            this.btnCopyRegEx.Name = "btnCopyRegEx";
            this.btnCopyRegEx.Size = new System.Drawing.Size(26, 23);
            this.btnCopyRegEx.TabIndex = 17;
            this.btnCopyRegEx.UseVisualStyleBackColor = true;
            this.btnCopyRegEx.Click += new System.EventHandler(this.btnCopyRegEx_Click);
            // 
            // btnApplyPrefix
            // 
            this.btnApplyPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnApplyPrefix.Location = new System.Drawing.Point(207, 385);
            this.btnApplyPrefix.Name = "btnApplyPrefix";
            this.btnApplyPrefix.Size = new System.Drawing.Size(45, 23);
            this.btnApplyPrefix.TabIndex = 14;
            this.btnApplyPrefix.Text = "Apply";
            this.btnApplyPrefix.Click += new System.EventHandler(this.btnApplyPrefix_Click);
            // 
            // cmbRegEx
            // 
            this.cmbRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbRegEx.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.WindowsVista;
            this.cmbRegEx.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            this.cmbRegEx.Location = new System.Drawing.Point(327, 386);
            this.cmbRegEx.Name = "cmbRegEx";
            this.cmbRegEx.Size = new System.Drawing.Size(133, 21);
            this.cmbRegEx.TabIndex = 16;
            // 
            // lblRegEx
            // 
            this.lblRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRegEx.AutoSize = true;
            this.lblRegEx.Location = new System.Drawing.Point(283, 390);
            this.lblRegEx.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRegEx.Name = "lblRegEx";
            this.lblRegEx.Size = new System.Drawing.Size(39, 13);
            this.lblRegEx.TabIndex = 8;
            this.lblRegEx.Text = "RegEx";
            // 
            // ultraTabPageControl2
            // 
            this.ultraTabPageControl2.Controls.Add(this.btnAddRow);
            this.ultraTabPageControl2.Controls.Add(this.btnRemoveRow);
            this.ultraTabPageControl2.Controls.Add(this.pnlNewColumns);
            this.ultraTabPageControl2.Location = new System.Drawing.Point(-10000, -10000);
            this.ultraTabPageControl2.Name = "ultraTabPageControl2";
            this.ultraTabPageControl2.Size = new System.Drawing.Size(1051, 412);
            // 
            // btnAddRow
            // 
            this.btnAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddRow.Location = new System.Drawing.Point(7, 385);
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Size = new System.Drawing.Size(97, 21);
            this.btnAddRow.TabIndex = 24;
            this.btnAddRow.Text = "Add Row";
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);
            // 
            // btnRemoveRow
            // 
            this.btnRemoveRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveRow.Location = new System.Drawing.Point(110, 385);
            this.btnRemoveRow.Name = "btnRemoveRow";
            this.btnRemoveRow.Size = new System.Drawing.Size(97, 21);
            this.btnRemoveRow.TabIndex = 25;
            this.btnRemoveRow.Text = "Remove Row(s)";
            this.btnRemoveRow.Click += new System.EventHandler(this.btnRemoveRow_Click);
            // 
            // pnlNewColumns
            // 
            this.pnlNewColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlNewColumns.Location = new System.Drawing.Point(5, 3);
            this.pnlNewColumns.Name = "pnlNewColumns";
            this.pnlNewColumns.Size = new System.Drawing.Size(1041, 376);
            this.pnlNewColumns.TabIndex = 13;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(978, 447);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(74, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(897, 447);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(74, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cbErrorHandling
            // 
            this.cbErrorHandling.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbErrorHandling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbErrorHandling.FormattingEnabled = true;
            this.cbErrorHandling.Location = new System.Drawing.Point(258, 454);
            this.cbErrorHandling.Name = "cbErrorHandling";
            this.cbErrorHandling.Size = new System.Drawing.Size(121, 21);
            this.cbErrorHandling.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(224, 457);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Error";
            // 
            // cmbErrorName
            // 
            this.cmbErrorName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbErrorName.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.WindowsVista;
            this.cmbErrorName.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.OnMouseEnter;
            this.cmbErrorName.DropDownListWidth = 200;
            this.cmbErrorName.Location = new System.Drawing.Point(68, 453);
            this.cmbErrorName.Name = "cmbErrorName";
            this.cmbErrorName.Nullable = false;
            this.cmbErrorName.Size = new System.Drawing.Size(133, 21);
            this.cmbErrorName.TabIndex = 19;
            this.cmbErrorName.ValueChanged += new System.EventHandler(this.cmbErrorName_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 457);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "ErrorName";
            // 
            // ultraTabControl1
            // 
            this.ultraTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ultraTabControl1.Controls.Add(this.ultraTabSharedControlsPage1);
            this.ultraTabControl1.Controls.Add(this.ultraTabPageControl1);
            this.ultraTabControl1.Controls.Add(this.ultraTabPageControl2);
            this.ultraTabControl1.Location = new System.Drawing.Point(6, 3);
            this.ultraTabControl1.Name = "ultraTabControl1";
            this.ultraTabControl1.SharedControlsPage = this.ultraTabSharedControlsPage1;
            this.ultraTabControl1.Size = new System.Drawing.Size(1055, 438);
            this.ultraTabControl1.TabIndex = 20;
            ultraTab1.TabPage = this.ultraTabPageControl1;
            ultraTab1.Text = "Column Mapping";
            ultraTab2.TabPage = this.ultraTabPageControl2;
            ultraTab2.Text = "New Columns";
            ultraTab2.Visible = false;
            this.ultraTabControl1.Tabs.AddRange(new Infragistics.Win.UltraWinTabControl.UltraTab[] {
            ultraTab1,
            ultraTab2});
            // 
            // ultraTabSharedControlsPage1
            // 
            this.ultraTabSharedControlsPage1.Location = new System.Drawing.Point(-10000, -10000);
            this.ultraTabSharedControlsPage1.Name = "ultraTabSharedControlsPage1";
            this.ultraTabSharedControlsPage1.Size = new System.Drawing.Size(1051, 412);
            // 
            // ultraMessageBox
            // 
            this.ultraMessageBox.ContainingControl = this;
            this.ultraMessageBox.MinimumWidth = 775;
            this.ultraMessageBox.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // frmDataConverterUI
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1065, 482);
            this.Controls.Add(this.ultraTabControl1);
            this.Controls.Add(this.cmbErrorName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbErrorHandling);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "frmDataConverterUI";
            this.Text = "DataConverter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDataConverterUI_FormClosing);
            this.ultraTabPageControl1.ResumeLayout(false);
            this.ultraTabPageControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPrefix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRegEx)).EndInit();
            this.ultraTabPageControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cmbErrorName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTabControl1)).EndInit();
            this.ultraTabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlDGV;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor tbPrefix;
        private Infragistics.Win.Misc.UltraButton btnApplyPrefix;
        private System.Windows.Forms.ComboBox cbErrorHandling;
        private System.Windows.Forms.Label label2;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbRegEx;
        private System.Windows.Forms.Label lblRegEx;
        private System.Windows.Forms.Button btnCopyRegEx;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbErrorName;
        private System.Windows.Forms.Label label3;
        private Infragistics.Win.UltraWinTabControl.UltraTabControl ultraTabControl1;
        private Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage ultraTabSharedControlsPage1;
        private Infragistics.Win.UltraWinTabControl.UltraTabPageControl ultraTabPageControl1;
        private Infragistics.Win.UltraWinTabControl.UltraTabPageControl ultraTabPageControl2;
        private System.Windows.Forms.Panel pnlNewColumns;
        private Infragistics.Win.Misc.UltraButton btnAddRow;
        private Infragistics.Win.Misc.UltraButton btnRemoveRow;
        private Infragistics.Win.UltraMessageBox.UltraMessageBoxManager ultraMessageBox;
    }
}