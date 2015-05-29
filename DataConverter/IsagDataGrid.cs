using System;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace DataConverter
{
    public partial class IsagDataGrid : UltraGrid
    {
        public IsagDataGrid()
        {
            InitializeComponent();

            this.UpdateMode = Infragistics.Win.UltraWinGrid.UpdateMode.OnCellChangeOrLostFocus;
            this.Dock = DockStyle.Fill;
            this.Text = "";
            this.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

            this.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            this.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            this.DisplayLayout.Override.ReadOnlyCellAppearance.BackColor = Color.LightGray;
            this.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            this.DisplayLayout.Override.HeaderCheckBoxVisibility = HeaderCheckBoxVisibility.WhenUsingCheckEditor;
            this.DisplayLayout.Bands[0].ColHeaderLines = 2;

            this.DisplayLayout.ViewStyle = ViewStyle.SingleBand;

            //this.InitializeLayout += new InitializeLayoutEventHandler(IsagDataGrid_InitializeLayout);

        }

        //private void IsagDataGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        //{
        //    if (e.Layout.Bands[0].Columns.Contains("Compare"))
        //    {
        //        cceCompare editor = new cceCompare(new IsagCompare(), new IsagCompare());
        //        e.Layout.Bands[0].Columns["Compare"].Editor = editor;
        //    }
        //}


        protected override void OnBeforeCellUpdate(BeforeCellUpdateEventArgs e)
        {
            base.OnBeforeCellUpdate(e);

            if (e.Cell.Column.DataType == typeof(System.Boolean) && e.Cell.Activation != Activation.AllowEdit)
                e.Cancel = true;
        }

        protected override void OnInitializeRow(InitializeRowEventArgs e)
        {
            base.OnInitializeRow(e);

            foreach (UltraGridColumn columns in e.Row.Band.Columns)
            {
                columns.Nullable = Infragistics.Win.UltraWinGrid.Nullable.Nothing;
            }
        }

        /// <summary>
        /// Aktionen, welche beim Ändern der Grid-Zelle ausgeführt werden
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCellChange(Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            base.OnCellChange(e);

            // Wenn die Boolean-Wert im Grid geändert wird, soll die Aktualisierung des Grundobjektes
            // gleich angestößen werden (nicht erst beim Verlassen des Feldes)
            if (e.Cell.Column.DataType == typeof(System.Boolean))
                DataChangedCommit(new System.ComponentModel.CancelEventArgs());

            e.Cell.Row.Update();
        }

        //protected override void OnAfterCellUpdate(CellEventArgs e)
        //{
        //    this.UpdateData();
        //}

        /// <summary>
        /// Nach dem Schließen einer Comboliste automatisch ein Update des Databindings feuern
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAfterCellListCloseUp(CellEventArgs e)
        {
            base.OnAfterCellListCloseUp(e);

            DataChangedCommit(new System.ComponentModel.CancelEventArgs());
            e.Cell.Row.Update();
        }

        /// <summary>
        /// Änderungen des Feldes gleich an das gebundene Objekt mittetilen
        /// </summary>
        /// <param name="e"></param>
        private void DataChangedCommit(System.ComponentModel.CancelEventArgs e)
        {
            // Wenn aktive Row modifiziert, jetzt committen
            if (this.ActiveRow != null && this.ActiveRow.DataChanged)
            {
                try
                {
                    if (this.ActiveCell != null)
                    {
                        //EditMode verlassen und intern validieren (OnCellDataError)
                        if (this.ActiveCell.IsInEditMode)
                            this.ActiveCell.EditorResolved.ExitEditMode(false, true);

                        // Editmode verlassen fehlerhaft? (Format-Fehler aus OnCellDataError)
                        if (this.ActiveCell != null && this.ActiveCell.IsInEditMode)
                            e.Cancel = true;
                    }

                }
                catch (Exception)
                {
                    e.Cancel = true;
                }
            }

        }

        public void SelectCheckBoxes(bool select)
        {

            foreach (UltraGridCell cell in this.Selected.Cells)
            {

                if (!cell.Column.IsReadOnly && cell.StyleResolved == Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox &&
                    cell.Activation == Activation.AllowEdit)
                {
                    cell.Value = select;
                    cell.Row.Update();
                }
            }

            foreach (UltraGridRow row in this.Selected.Rows)
            {
                bool doUpdate = false;

                foreach (UltraGridCell cell in row.Cells)
                {
                    if (cell.StyleResolved == Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox && !cell.Column.IsReadOnly &&
                        cell.Activation == Activation.AllowEdit)
                    {
                        cell.Value = select;
                        doUpdate = true;
                    }
                }

                if (doUpdate) row.Update();
            }

            foreach (Infragistics.Win.UltraWinGrid.ColumnHeader col in this.Selected.Columns)
            {


                if (col.Column.DataType.Name == "Boolean" && !col.Column.IsReadOnly)
                {
                    foreach (UltraGridRow row in this.Rows)
                    {
                        if (row.Cells[col.Column].Activation == Activation.AllowEdit)
                        {
                            row.Cells[col.Column].Value = select;
                            row.Update();
                        }
                    }
                }
            }
        }
    }
}
