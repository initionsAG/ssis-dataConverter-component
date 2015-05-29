namespace DataConverter
{
    partial class IsagCompare
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
            Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem3 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem4 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem5 = new Infragistics.Win.ValueListItem();
            Infragistics.Win.ValueListItem valueListItem6 = new Infragistics.Win.ValueListItem();
            this.uceLeftColumn = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.uceOp = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.uceRightColumn = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            ((System.ComponentModel.ISupportInitialize)(this.uceLeftColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uceOp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uceRightColumn)).BeginInit();
            this.SuspendLayout();
            // 
            // uceLeftColumn
            // 
            this.uceLeftColumn.Location = new System.Drawing.Point(3, 3);
            this.uceLeftColumn.Name = "uceLeftColumn";
            this.uceLeftColumn.Size = new System.Drawing.Size(144, 21);
            this.uceLeftColumn.TabIndex = 0;
            // 
            // uceOp
            // 
            valueListItem1.DataValue = "ValueListItem0";
            valueListItem1.DisplayText = ">";
            valueListItem2.DataValue = "ValueListItem1";
            valueListItem2.DisplayText = "<";
            valueListItem3.DataValue = "ValueListItem2";
            valueListItem3.DisplayText = "==";
            valueListItem4.DataValue = "ValueListItem3";
            valueListItem4.DisplayText = "!=";
            valueListItem5.DataValue = "ValueListItem4";
            valueListItem5.DisplayText = ">=";
            valueListItem6.DataValue = "ValueListItem5";
            valueListItem6.DisplayText = "<=";
            this.uceOp.Items.AddRange(new Infragistics.Win.ValueListItem[] {
            valueListItem1,
            valueListItem2,
            valueListItem3,
            valueListItem4,
            valueListItem5,
            valueListItem6});
            this.uceOp.Location = new System.Drawing.Point(153, 3);
            this.uceOp.Name = "uceOp";
            this.uceOp.Size = new System.Drawing.Size(43, 21);
            this.uceOp.TabIndex = 1;
            // 
            // uceRightColumn
            // 
            this.uceRightColumn.Location = new System.Drawing.Point(202, 3);
            this.uceRightColumn.Name = "uceRightColumn";
            this.uceRightColumn.Size = new System.Drawing.Size(144, 21);
            this.uceRightColumn.TabIndex = 2;
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uceRightColumn);
            this.Controls.Add(this.uceOp);
            this.Controls.Add(this.uceLeftColumn);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(350, 27);
            ((System.ComponentModel.ISupportInitialize)(this.uceLeftColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uceOp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uceRightColumn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Infragistics.Win.UltraWinEditors.UltraComboEditor uceLeftColumn;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor uceOp;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor uceRightColumn;
    }
}
