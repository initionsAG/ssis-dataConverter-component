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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataConverterUI));
            this.tbPrefix = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.label1 = new System.Windows.Forms.Label();
            this.btnApplyPrefix = new Infragistics.Win.Misc.UltraButton();
            this.cmbRegEx = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.lblRegEx = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbErrorHandling = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbErrorName = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.label3 = new System.Windows.Forms.Label();
            this.ultraMessageBox = new Infragistics.Win.UltraMessageBox.UltraMessageBoxManager(this.components);
            this.pnlMapping = new System.Windows.Forms.Panel();
            this.btnCopyRegEx = new System.Windows.Forms.Button();
            this.idgvMapping = new Lookup2.ComponentFramework.Controls.IsagDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.tbPrefix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRegEx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbErrorName)).BeginInit();
            this.pnlMapping.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.idgvMapping)).BeginInit();
            this.SuspendLayout();
            // 
            // tbPrefix
            // 
            this.tbPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbPrefix.Location = new System.Drawing.Point(73, 397);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(126, 21);
            this.tbPrefix.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 401);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Alias Prefix";
            // 
            // btnApplyPrefix
            // 
            this.btnApplyPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnApplyPrefix.Location = new System.Drawing.Point(207, 396);
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
            this.cmbRegEx.Location = new System.Drawing.Point(327, 397);
            this.cmbRegEx.Name = "cmbRegEx";
            this.cmbRegEx.Size = new System.Drawing.Size(133, 21);
            this.cmbRegEx.TabIndex = 16;
            // 
            // lblRegEx
            // 
            this.lblRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRegEx.AutoSize = true;
            this.lblRegEx.Location = new System.Drawing.Point(283, 401);
            this.lblRegEx.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRegEx.Name = "lblRegEx";
            this.lblRegEx.Size = new System.Drawing.Size(39, 13);
            this.lblRegEx.TabIndex = 8;
            this.lblRegEx.Text = "RegEx";
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
            // ultraMessageBox
            // 
            this.ultraMessageBox.ContainingControl = this;
            this.ultraMessageBox.MinimumWidth = 775;
            this.ultraMessageBox.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // pnlMapping
            // 
            this.pnlMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMapping.Controls.Add(this.idgvMapping);
            this.pnlMapping.Controls.Add(this.label1);
            this.pnlMapping.Controls.Add(this.tbPrefix);
            this.pnlMapping.Controls.Add(this.lblRegEx);
            this.pnlMapping.Controls.Add(this.cmbRegEx);
            this.pnlMapping.Controls.Add(this.btnCopyRegEx);
            this.pnlMapping.Controls.Add(this.btnApplyPrefix);
            this.pnlMapping.Location = new System.Drawing.Point(7, 12);
            this.pnlMapping.Name = "pnlMapping";
            this.pnlMapping.Size = new System.Drawing.Size(1054, 429);
            this.pnlMapping.TabIndex = 21;
            // 
            // btnCopyRegEx
            // 
            this.btnCopyRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopyRegEx.BackgroundImage = global::DataConverter.Properties.Resources.copy;
            this.btnCopyRegEx.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopyRegEx.Location = new System.Drawing.Point(463, 396);
            this.btnCopyRegEx.Name = "btnCopyRegEx";
            this.btnCopyRegEx.Size = new System.Drawing.Size(26, 23);
            this.btnCopyRegEx.TabIndex = 17;
            this.btnCopyRegEx.UseVisualStyleBackColor = true;
            this.btnCopyRegEx.Click += new System.EventHandler(this.btnCopyRegEx_Click);
            // 
            // idgvMapping
            // 
            this.idgvMapping.AllowUserToAddRows = false;
            this.idgvMapping.AllowUserToDeleteRows = false;
            this.idgvMapping.AllowUserToOrderColumns = true;
            this.idgvMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.idgvMapping.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.idgvMapping.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.idgvMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.idgvMapping.DefaultCellStyle = dataGridViewCellStyle2;
            this.idgvMapping.Location = new System.Drawing.Point(0, 0);
            this.idgvMapping.Name = "idgvMapping";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.idgvMapping.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.idgvMapping.ShowEditingIcon = false;
            this.idgvMapping.Size = new System.Drawing.Size(1051, 391);
            this.idgvMapping.TabIndex = 18;
            this.idgvMapping.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.idgvMapping_CellValueChanged);
            // 
            // frmDataConverterUI
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1065, 482);
            this.Controls.Add(this.pnlMapping);
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
            ((System.ComponentModel.ISupportInitialize)(this.tbPrefix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbRegEx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbErrorName)).EndInit();
            this.pnlMapping.ResumeLayout(false);
            this.pnlMapping.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.idgvMapping)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor tbPrefix;
        private Infragistics.Win.Misc.UltraButton btnApplyPrefix;
        private System.Windows.Forms.ComboBox cbErrorHandling;
        private System.Windows.Forms.Label label2;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbRegEx;
        private System.Windows.Forms.Label lblRegEx;
        private System.Windows.Forms.Button btnCopyRegEx;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cmbErrorName;
        private System.Windows.Forms.Label label3;
        private Infragistics.Win.UltraMessageBox.UltraMessageBoxManager ultraMessageBox;
        private System.Windows.Forms.Panel pnlMapping;
        private Lookup2.ComponentFramework.Controls.IsagDataGridView idgvMapping;

    }
}