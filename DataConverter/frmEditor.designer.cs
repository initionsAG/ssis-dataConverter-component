namespace DataConverter
{
    partial class frmEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditor));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnInsertInputColumn = new Infragistics.Win.Misc.UltraButton();
            this.tbValue = new Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor();
            this.cbColumnList = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            ((System.ComponentModel.ISupportInitialize)(this.cbColumnList)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(351, 114);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 9;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(432, 114);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnInsertInputColumn
            // 
            this.btnInsertInputColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInsertInputColumn.Location = new System.Drawing.Point(12, 114);
            this.btnInsertInputColumn.Name = "btnInsertInputColumn";
            this.btnInsertInputColumn.Size = new System.Drawing.Size(75, 23);
            this.btnInsertInputColumn.TabIndex = 11;
            this.btnInsertInputColumn.Text = "Insert InputColumn";
            this.btnInsertInputColumn.Click += new System.EventHandler(this.btnInsertInputColumn_Click);
            // 
            // tbValue
            // 
            this.tbValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbValue.Location = new System.Drawing.Point(12, 12);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(495, 96);
            this.tbValue.TabIndex = 12;
            this.tbValue.Value = "";
            // 
            // cbColumnList
            // 
            this.cbColumnList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbColumnList.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
            this.cbColumnList.AutoSuggestFilterMode = Infragistics.Win.AutoSuggestFilterMode.StartsWith;
            this.cbColumnList.Location = new System.Drawing.Point(93, 116);
            this.cbColumnList.Name = "cbColumnList";
            this.cbColumnList.Nullable = false;
            this.cbColumnList.Size = new System.Drawing.Size(144, 21);
            this.cbColumnList.TabIndex = 14;
            // 
            // frmEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 149);
            this.Controls.Add(this.cbColumnList);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.btnInsertInputColumn);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmEditor";
            this.Text = "DataConverter: Compare Editor";
            ((System.ComponentModel.ISupportInitialize)(this.cbColumnList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private Infragistics.Win.Misc.UltraButton btnInsertInputColumn;
        private Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor tbValue;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor cbColumnList;

    }
}