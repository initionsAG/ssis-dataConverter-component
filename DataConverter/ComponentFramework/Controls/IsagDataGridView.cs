using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lookup2.ComponentFramework.Controls
{
    /// <summary>
    /// Enhanced DataGridView that accepts a standard DataSource (i.e. DataTable) and a DataSource (BindingList<string>) for the ItemList for a combobox cell 
    /// If a cell value is not included in the ItemList, the cell value is added to the ItemList and shwon in red (as the value is invalid).
    /// 
    /// This DataGridView needs a standard DataSource. Then it is prossible to add a "bounded" combobox column for each column contained in the DataSource.
    /// The "bounded" combobox column is visible while the column created from the DataSource is invisible. If the text property of the combobox column cell changes 
    /// the value of the invisible cell also changes.
    /// 
    /// Example:
    /// idgvOutputColumns.DataSource = _IsagCustomProperties.OutputConfigList;
    /// (set the DataSource for the DataGridVíew)
    /// idgvOutputColumns.AddCellBoundedComboBox("SqlColumn", _sqlColumnList);
    /// (set _sqlColumnList as the datsource for the ItemList of the combobox column,
    /// the column name of the DataGridView is "SqlColumn")
    /// 
    /// </summary>
    public partial class IsagDataGridView : DataGridView
    {
        /// <summary>
        /// DataSources for the ItemLists
        /// int: ColumnIndex in the DataGridView
        /// BindingList<string>: DataSource for the ItemList
        /// </summary>
        private Dictionary<int, BindingList<object>> _cmbItemSources = new Dictionary<int, BindingList<object>>();

        /// <summary>
        /// ComboBox Columns gets the name of the column that it is bound to pls this prefix
        /// </summary>
        public const string CMB_COLUMN_PREFIX = "BoundedCombo_";

        /// <summary>
        /// the constructor
        /// </summary>
        public IsagDataGridView()
        {
            InitializeComponent();
            this.AllowUserToAddRows = false;
            this.CellValueChanged += IsagDataGridView_CellValueChanged;
            this.EditingControlShowing += IsagDataGridView_EditingControlShowing;
            this.DataBindingComplete += IsagDataGridView_DataBindingComplete;

            this.CurrentCellDirtyStateChanged += IsagDataGridView_CurrentCellDirtyStateChanged;

        }

        /// <summary>
        /// Commits the cells value change earlier
        /// This way the cell does not have to loose focus to trigger cell value change event.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.IsCurrentCellDirty) this.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            UpdateCellBoundComboBox();
        }

        /// <summary>
        /// Whenn a Combox is opened register to the DrawItem Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_cmbItemSources.Keys.Contains(this.SelectedCells[0].ColumnIndex))
            {
                ComboBox cmb = (ComboBox)e.Control;

                if (cmb != null)
                {
                    cmb.DrawMode = DrawMode.OwnerDrawFixed;
                    cmb.DrawItem -= cmb_DrawItem;
                    cmb.DrawItem += cmb_DrawItem;
                }
            }
        }

        /// <summary>
        /// Draws the items if a combobox is opend.
        /// Invalid Items (cell values that is not contained in the ItemSource) are marked red
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index != -1)
            {
                object itemValue = ((ComboBox)sender).Items[e.Index]; 
                bool isValid = _cmbItemSources[this.SelectedCells[0].ColumnIndex].Contains(itemValue);
                Color color = isValid ? Color.Black : Color.Red;

                using (var brush = new SolidBrush(color))
                    e.Graphics.DrawString(itemValue.ToString(), e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// If a cell value has change the ItemList has to be updated.
        /// - invalid items (not included in the ItemSource but in the ItemList) has to be removed from the ItemList
        /// - if the cell value is not contained in the ItemList it has to be added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_cmbItemSources.Keys.Contains(e.ColumnIndex))
            {
                //if (this.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null) this.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
                object value = this.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                //Update Cell that is databounded to the GridVeiws DataSource
                this.Rows[e.RowIndex].Cells[GetBoundedColumnIndex(e.ColumnIndex)].Value = value;

                //Remove invalid items from ItemList (except Cell Values)
                RemoveUnusedInvalidItems(e.ColumnIndex);

                UpdateColumnBoxStyle(e.ColumnIndex, this.Rows[e.RowIndex]);
            }
        }

        /// <summary>
        /// Removes invalid Items from the ItemList if the item does not equal the cell value
        /// </summary>
        /// <param name="columnIndex">the column Index of the combobox column</param>
        private void RemoveUnusedInvalidItems(int columnIndex)
        {
            DataGridViewComboBoxColumn cmbColumn = (DataGridViewComboBoxColumn)this.Columns[columnIndex];

            List<object> invalidItems = new List<object>();
            foreach (DataGridViewRow row in this.Rows)
            {
                object rowValue = row.Cells[columnIndex].Value;
                if (!_cmbItemSources[columnIndex].Contains(rowValue))
                {
                    invalidItems.Add(rowValue);
                }
            }
            for (int i = cmbColumn.Items.Count - 1; i >= 0; i--)
            {

                object item = cmbColumn.Items[i];
                if (!invalidItems.Contains(item) && !_cmbItemSources[columnIndex].Contains(item)) cmbColumn.Items.RemoveAt(i);
            }
        }

        /// <summary>
        /// Updates the style of a combobox for each row
        /// </summary>
        /// <param name="columnIndex">the column Index of the combobox column</param>
        private void UpdateColumnBoxStyle(int columnIndex)
        {
            foreach (DataGridViewRow row in Rows)
            {
                UpdateColumnBoxStyle(columnIndex, row);
            }
        }
        /// <summary>
        /// Updates the style of a combobox cell
        /// (change colors depending on item state (valid or invalid) 
        /// </summary>
        /// <param name="columnIndex">the column Index of the combobox column</param>
        /// <param name="row">the row that contains the combobox cell that has to be updated</param>
        private void UpdateColumnBoxStyle(int columnIndex, DataGridViewRow row)
        {
            DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)row.Cells[columnIndex];

            if (cell.Value != null)
            {
                bool isValid = _cmbItemSources[columnIndex].Contains(cell.Value);
                Color color = isValid ? Color.Black : Color.Red;
                Color backColor = isValid ? Color.Empty : Color.White;

                cell.Style.ForeColor = color;
                cell.Style.SelectionBackColor = backColor;
                cell.Style.SelectionForeColor = color;
            }
        }

        /// <summary>
        /// Adds a combobox column that is bounded to another Column
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="dataSource">the DataSource for the ItemList of the combobox</param>
        public void AddCellBoundedComboBox(string srcColumnName, BindingList<object> dataSource)
        {
            dataSource.ListChanged += DataSource_ListChanged;

            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            DataGridViewColumn srcColumn = this.Columns[srcColumnName];
            srcColumn.Visible = false;

            cmbColumn.Name = CMB_COLUMN_PREFIX + srcColumnName;
            cmbColumn.HeaderText = srcColumn.HeaderText;
            cmbColumn.Sorted = true;
            cmbColumn.ValueType = dataSource[0].GetType();

            cmbColumn.Items.AddRange(dataSource.ToArray<object>());
            cmbColumn.FlatStyle = FlatStyle.Flat;

            int index = _cmbItemSources.Count;
            _cmbItemSources.Add(index, dataSource);
            this.Columns.Insert(index, cmbColumn);
            this.Columns[index].DisplayIndex = srcColumn.Index;
        }

        public void AddCellBoundedComboBox(string srcColumnName, Type srcEnum)
        {
            BindingList<object> dataSource = new BindingList<object>();
            Array enums = Enum.GetValues(srcEnum);

             for (int i = 0; i < enums.Length; i++)
             {
                 dataSource.Add(enums.GetValue(i));
             }

             AddCellBoundedComboBox(srcColumnName, dataSource);
        }

        //public void SetValueList(Type srcEnum, string[] stringValue)
        //{
        //    this.Items.Clear();
        //    Array enums = Enum.GetValues(srcEnum);

        //    //TODO TL3/DC: not needed in LU2
        //    for (int i = 0; i < enums.Length; i++)
        //    {
        //        //this.
        //        //this.ValueList.ValueListItems.Add(enums.GetValue(i), stringValue[i]);
        //    }
        //}
        /// <summary>
        /// Updates the "bounded" combobox column
        /// (remove invalid items, add invalid items that euqals a cell value of the combobox column
        /// </summary>
        /// <param name="columnIndex">the column index of the combobox column</param>
        private void UpdateCellBoundComboBox(int columnIndex)
        {
            DataGridViewComboBoxColumn cmbColumn = (DataGridViewComboBoxColumn)this.Columns[columnIndex];
            DataGridViewColumn srcColumn = this.Columns[columnIndex + 1];
            int boundedColumnIndex = GetBoundedColumnIndex(columnIndex);

            cmbColumn.Items.Clear();
            cmbColumn.Items.AddRange(_cmbItemSources[columnIndex].ToArray<object>());

            foreach (DataGridViewRow row in this.Rows)
            {
                DataGridViewCell boundedCell = row.Cells[boundedColumnIndex];
                if (boundedCell.Value == null) boundedCell.Value = "";

                if (!_cmbItemSources[columnIndex].Contains(boundedCell.Value))
                {
                    cmbColumn.Items.Add(boundedCell.Value);
                }
                try
                {
                    row.Cells[columnIndex].Value = boundedCell.Value;
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Updates all "bounded" combobox columns
        /// </summary>
        public void UpdateCellBoundComboBox()
        {
            foreach (int key in _cmbItemSources.Keys)
            {
                UpdateCellBoundComboBox(key);
            }
        }

        /// <summary>
        /// React if the DataSource for the ItemList has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            BindingList<object> datasource = (BindingList<object>)sender;

            foreach (int key in _cmbItemSources.Keys)
            {
                if (_cmbItemSources[key] == datasource)
                    UpdateCellBoundComboBox(key);
                UpdateColumnBoxStyle(key);
            }
        }

        /// <summary>
        /// Gets the column index of the column that the combobox column is bounded to
        /// (by removing the prefix CMB_COLUMN_PREFIX of the combobox column name
        /// </summary>
        /// <param name="cmbColumnIndex"></param>
        /// <returns></returns>
        private int GetBoundedColumnIndex(int cmbColumnIndex)
        {
            string boundedColuimnName = this.Columns[cmbColumnIndex].Name.Substring(CMB_COLUMN_PREFIX.Length);
            return this.Columns[boundedColuimnName].Index;
        }

        public void RemoveSelectedRows()
        {
            while (SelectedRows.Count > 0)
            {
                this.Rows.Remove(SelectedRows[0]);
            }
        }





    }
}
