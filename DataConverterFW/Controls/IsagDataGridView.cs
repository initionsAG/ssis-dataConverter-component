using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ComponentFramework.Controls {
    /// <summary>
    /// Enhanced DataGridView that accepts a standard DataSource (i.e. DataTable) and DataSources (BindingList<object>) for ItemLists of combobox cells
    /// 
    /// 1. a datasource has to be bound to the grid
    /// 2. for a textbox column that is bounded to that datasource (bounded column), a combobox column is added.
    /// 3. the bounded column is not visible. 
    /// 4. If a cell value that is part of the databounded - or combobox, the other cell is updated 
    /// 5. Each combobox cell has a different ItemSource. The ItemList of each combobox cell contains the items of the itemSource (plus the cell value)
    /// 6. If a cell value is not part of the itemSource, its fore color will be red, if ComboboxConfigType == MARK_INVALID
    /// 
    /// Attention: removing/adding rows is only allowed if all combobox cells for a combobox column have the same itemSource
    /// </summary>
    public partial class IsagDataGridView: DataGridView {
        /// <summary>
        /// MARK_INVALID: items/values that are not part of an itemSource of a combobox cell are marked red
        /// </summary>
        public enum ComboboxConfigType { NONE = 0, MARK_INVALID = 1 }

        /// <summary>
        /// Flag for disableing the cell value changed event
        /// </summary>
        private bool IsCellValueChangeEventDisabled = false;

        /// <summary>
        /// DataSources for the ItemLists
        /// int: ColumnIndex of the combobox column in the DataGridView
        /// ComboBoxConfiguration: DataSource for the ItemList
        /// </summary>
        private Dictionary<int, ComboBoxConfiguration> _cmbItemSources = new Dictionary<int, ComboBoxConfiguration>();

        /// <summary>
        /// ComboBox Columns gets the name of the column that it is bound to plus this prefix
        /// </summary>
        public const string CMB_COLUMN_PREFIX = "BoundedCombo_";

        /// <summary>
        /// the constructor
        /// </summary>
        public IsagDataGridView()
        {
            InitializeComponent();

            this.AllowUserToAddRows = false;
            this.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
        }

        /// <summary>
        /// Sort Datagrid
        /// <param name="e">event arguments</param>
        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            bool sortComboBoxCell = _cmbItemSources.ContainsKey(e.ColumnIndex);

            //Get the sort column (if it is a columnbox, the column that is bounded to the datasource has to be sorted)
            DataGridViewColumn column = sortComboBoxCell ?
                this.Columns[GetBoundedColumnIndex(e.ColumnIndex)] : this.Columns[e.ColumnIndex];


            //sort order is ascending, if columns last sort order has not been ascending
            System.Windows.Forms.SortOrder sortOrder = System.Windows.Forms.SortOrder.Ascending;
            ListSortDirection listSortOrder = ListSortDirection.Ascending;

            if (column.HeaderCell.SortGlyphDirection == System.Windows.Forms.SortOrder.Ascending)
            {
                listSortOrder = ListSortDirection.Descending;
                sortOrder = System.Windows.Forms.SortOrder.Descending;
            }

            // Remove SortGlyphs (necessary because of comboboxe cells)
            foreach (DataGridViewColumn col in this.Columns)
            {
                col.HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.None;
            }

            //Glyphs is needed on the ComboBoxCell, sorting happens on the bounded cell
            if (sortComboBoxCell)
            {
                this.Columns[e.ColumnIndex].SortMode = DataGridViewColumnSortMode.Programmatic;
                this.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortOrder;
            }

            column.SortMode = DataGridViewColumnSortMode.Programmatic;
            column.HeaderCell.SortGlyphDirection = sortOrder;
            this.Sort(column, listSortOrder);

            //Refresh comboboxes (displayed value is lost because comboBox cell is not directly bounded to the datasource
            foreach (int colIdx in _cmbItemSources.Keys)
            {
                string columnName = this.Columns[GetBoundedColumnIndex(colIdx)].Name;
                RefreshCellBoundComboBox(columnName);
            }

            base.OnColumnHeaderMouseClick(e);
        }

        /// <summary>
        /// Each combobox column has to be updated whenever the bounded column has changed
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            foreach (int columnIndex in _cmbItemSources.Keys)
            {
                RefreshCellBoundComboBox(this.Columns[GetBoundedColumnIndex(columnIndex)].Name);
            }

            base.OnDataBindingComplete(e);
        }

        /// <summary>
        /// Whenn a Combox is opened register to the DrawItem Event
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            if (_cmbItemSources.Keys.Contains(this.SelectedCells[0].ColumnIndex))
            {
                ComboBox cmb = (ComboBox) e.Control;

                if (cmb != null)
                {
                    cmb.DrawMode = DrawMode.OwnerDrawFixed;
                    cmb.DrawItem -= cmb_DrawItem;
                    cmb.DrawItem += cmb_DrawItem;
                }
            }
            base.OnEditingControlShowing(e);
        }

        /// <summary>
        /// Draws the items if a combobox is opend.
        /// Invalid Items (cell values that is not contained in the ItemSource) are marked red
        /// if ComboboxConfigType ==  MARK_INVALID
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void cmb_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index != -1)
            {
                ComboBox cmb = (ComboBox) EditingControl;

                object itemValue = ((ComboBox) sender).Items[e.Index];
                object databoundedItem = this.SelectedCells[0].OwningRow.DataBoundItem;

                Color color = _cmbItemSources[this.SelectedCells[0].ColumnIndex].GetForeColor(itemValue, databoundedItem);

                using (var brush = new SolidBrush(color))
                    e.Graphics.DrawString(itemValue.ToString(), e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Commits the cells value change earlier (does not apply to textbox cells)
        /// This way the cell does not have to loose focus to trigger cell value change event.
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnCurrentCellDirtyStateChanged(EventArgs e)
        {
            if (this.IsCurrentCellDirty && !(this.CurrentCell is DataGridViewTextBoxCell))
                this.CommitEdit(DataGridViewDataErrorContexts.Commit);

            base.OnCurrentCellDirtyStateChanged(e);
        }

        /// <summary>
        /// Reacts on cell value changes
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            if (!IsCellValueChangeEventDisabled)
            {
                DoCellValueChanged(e);
                base.OnCellValueChanged(e);
            }
        }

        /// <summary>
        /// Set cells back color. Color depends on ReadOnly Property of the cell.
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
            {
                bool readOnly = this.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly;
                e.CellStyle.BackColor = readOnly ? Color.LightGray : Color.White;
            }

            base.OnCellFormatting(e);
        }

        /// <summary>
        /// When the ReadOnly Property of a cell has changed, the cell has to be redrawn in order to change the back color.
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnCellStateChanged(DataGridViewCellStateChangedEventArgs e)
        {
            if (e.StateChanged == DataGridViewElementStates.ReadOnly)
                Refresh();
            base.OnCellStateChanged(e);
        }

        /// <summary>
        ///    If a cell value has changed
        ///    1. the ItemList has to be updated (remove or add invalid item)
        ///    2. the bounded cell has to be updated
        ///    3. the fore color of the combobox value has to be updated
        /// </summary>
        /// <param name="e">event arguments</param>
        private void DoCellValueChanged(DataGridViewCellEventArgs e)
        {
            IsCellValueChangeEventDisabled = true;

            if (_cmbItemSources.Keys.Contains(e.ColumnIndex))
            {
                DataGridViewRow row = this.Rows[e.RowIndex];
                ComboBoxConfiguration comboConfig = _cmbItemSources[e.ColumnIndex];

                //Get the new value of the cell (if value has been choosen by a an EdintingControl like a combobox, the cell still contains the old value)
                object value = (EditingControl != null ? EditingControl.Text : this.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                if (value == null)
                    value = "";

                //Update combobox Itemlist
                List<object> itemList = comboConfig.GetItemList(row.DataBoundItem);

                if (!itemList.Contains(value))
                    itemList.Insert(0, value);
                DataGridViewComboBoxCell cell = ((DataGridViewComboBoxCell) row.Cells[e.ColumnIndex]);
                SetItemList(cell, itemList);

                //Update bounded Cell (it is databounded to the GridViews DataSource)
                row.Cells[GetBoundedColumnIndex(e.ColumnIndex)].Value = value;

                //Update ForeColor of ComboBoxCell
                row.Cells[e.ColumnIndex].Style.ForeColor = comboConfig.GetForeColor(value, row.DataBoundItem);
            }

            IsCellValueChangeEventDisabled = false;
        }

        /// <summary>
        /// if a row has been added the ComboBoxConfiguration has to be updated
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            if (RowCount > 0)
            {
                foreach (ComboBoxConfiguration config in _cmbItemSources.Values)
                {
                    config.AddItemSourceElement(this.Rows[e.RowIndex].DataBoundItem);
                }
            }
            base.OnRowsAdded(e);
        }

        /// <summary>
        /// /// if a row has been removed the ComboBoxConfiguration has to be updated
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            if (RowCount > 0)
            {
                foreach (ComboBoxConfiguration config in _cmbItemSources.Values)
                {
                    config.RemoveItemSourceElement(this.Rows[e.RowIndex].DataBoundItem);
                }
            }
            base.OnRowsRemoved(e);
        }

        #region AddCellBoundedColumn

        /// <summary>
        /// Creates a new ComboBoxConfiguration with a single itemSource for all combobox cell of a combobox column
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="itemListSource">the ItemSource for the ItemList of all combobox cells</param>
        /// <param name="sortItemList">sort the itemLists?</param>
        /// <returns>configation for a comboBoxColumn</returns>
        private ComboBoxConfiguration GetComboBoxConfigFromSingleDataSource(BindingList<object> itemListSource, ComboboxConfigType configurationType, bool sortItemList)
        {
            Dictionary<object, BindingList<object>> dataSourceDictionary = new Dictionary<object, BindingList<object>>();

            foreach (DataGridViewRow row in this.Rows)
            {
                dataSourceDictionary.Add(row.DataBoundItem, itemListSource);
            }

            //only one registration is necessary because all datasources are equal
            itemListSource.ListChanged += DataSource_ListChanged;

            //Create combobox configuration and add item source template for adding new columns
            ComboBoxConfiguration comboConfig = new ComboBoxConfiguration(dataSourceDictionary, configurationType, sortItemList);
            comboConfig.AddItenSourceTemplate(itemListSource);

            return comboConfig;
        }

        /// <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// (each combobox cell has the same itemSource)
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="itemListSource">the ItemSource for the ItemList of all combobox cells</param>
        /// <param name="sortItemList">sort the itemLists?</param>
        public void AddCellBoundedComboBox(string srcColumnName, BindingList<object> itemListSource, bool sortItemList)
        {
            ComboBoxConfiguration comboConfig = GetComboBoxConfigFromSingleDataSource(itemListSource, ComboboxConfigType.NONE, sortItemList);

            AddCellBoundedComboBox(srcColumnName, comboConfig);
        }

        /// <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// (all combobox cells have different itemSources
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="itemListSource">the ItemSources for the ItemLists of the combobox cells</param>
        /// <param name="configurationType">configuration type</param>
        /// <param name="sortItemList">sort the itemLists?</param>
        public void AddCellBoundedComboBox(string srcColumnName, BindingList<object> itemListSource, ComboboxConfigType configurationType, bool sortItemList)
        {
            ComboBoxConfiguration comboConfig = GetComboBoxConfigFromSingleDataSource(itemListSource, configurationType, sortItemList);

            AddCellBoundedComboBox(srcColumnName, comboConfig);
        }


        /// <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="itemListSource">the ItemSources for the ItemLists of the combobox cells</param>
        /// <param name="sortItemList">sort the itemLists?</param>
        public void AddCellBoundedComboBox(string srcColumnName, Dictionary<object, BindingList<object>> itemListSource, bool sortItemList)
        {
            ComboBoxConfiguration comboConfig = new ComboBoxConfiguration(itemListSource, ComboboxConfigType.NONE, sortItemList);

            AddCellBoundedComboBox(srcColumnName, comboConfig);
        }

        /// <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// (all combobox cells have different itemSources
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="itemListSource">the ItemSources for the ItemLists of the combobox cells</param>
        /// <param name="configurationType">configuration type</param>
        /// <param name="sortItemList">sort the itemLists?</param>
        public void AddCellBoundedComboBox(string srcColumnName, Dictionary<object, BindingList<object>> itemListSource, ComboboxConfigType configurationType, bool sortItemList)
        {
            ComboBoxConfiguration comboConfig = new ComboBoxConfiguration(itemListSource, configurationType, sortItemList);

            AddCellBoundedComboBox(srcColumnName, comboConfig);
        }

        // <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// </summary>
        /// <param name="srcColumnName">the name of the column that the combobox column is bounded to</param>
        /// <param name="comboConfig">the configuration of the combobox cell</param>
        private void AddCellBoundedComboBox(string srcColumnName, ComboBoxConfiguration comboConfig)
        {
            IsCellValueChangeEventDisabled = true;

            DataGridViewComboBoxColumn cmbColumn = new DataGridViewComboBoxColumn();
            DataGridViewColumn srcColumn = this.Columns[srcColumnName];
            srcColumn.Visible = false;

            cmbColumn.Name = CMB_COLUMN_PREFIX + srcColumnName;
            cmbColumn.HeaderText = srcColumn.HeaderText;
            cmbColumn.ValueType = srcColumn.ValueType;
            cmbColumn.FlatStyle = FlatStyle.Flat;

            int index = _cmbItemSources.Count;
            _cmbItemSources.Add(index, comboConfig);
            this.Columns.Insert(index, cmbColumn);
            this.Columns[index].DisplayIndex = srcColumn.Index;

            //Copy Values from the bounded cell (bounded to grid datasource) to the combobox cell
            foreach (DataGridViewRow row in this.Rows)
            {
                List<object> itemList = comboConfig.GetItemList(row.DataBoundItem);
                object value = row.Cells[srcColumn.Index].Value;
                if (!itemList.Contains(value))
                    itemList.Insert(0, value);
                DataGridViewComboBoxCell cell = ((DataGridViewComboBoxCell) row.Cells[cmbColumn.Index]);
                SetItemList(cell, itemList);

                row.Cells[cmbColumn.Index].Value = row.Cells[srcColumn.Index].Value;
            }

            IsCellValueChangeEventDisabled = false;
        }

        /// <summary>
        /// Adds a combobox column that is bounded to another Column (bounded column)
        /// The itemSource (an enum) is equal for all combobox cell and cannot be changed
        /// </summary>
        /// <param name="srcColumnName"></param>
        /// <param name="srcEnum"></param>
        /// <param name="sortItemList"></param>
        public void AddCellBoundedComboBox(string srcColumnName, Type srcEnum, bool sortItemList)
        {
            BindingList<object> dataSource = new BindingList<object>();
            Array enums = Enum.GetValues(srcEnum);

            for (int i = 0; i < enums.Length; i++)
            {
                dataSource.Add(enums.GetValue(i));
            }

            AddCellBoundedComboBox(srcColumnName, dataSource, ComboboxConfigType.MARK_INVALID, sortItemList);
        }


        #endregion

        /// <summary>
        /// React if the ItemSource for the ItemList has changed
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void DataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            IsCellValueChangeEventDisabled = true;

            BindingList<object> itemSource = (BindingList<object>) sender;

            // Iterate through all combobox configurations to find the config that contains the changed itemSource
            foreach (int columnIndex in _cmbItemSources.Keys)
            {
                ComboBoxConfiguration comboConfig = _cmbItemSources[columnIndex];

                if (comboConfig.HasItemSource(itemSource))
                {
                    foreach (DataGridViewRow row in this.Rows)
                    {
                        if (comboConfig.HasItemSource(itemSource, row.DataBoundItem))
                        {
                            List<object> itemList = comboConfig.GetItemList(row.DataBoundItem);

                            object value = row.Cells[columnIndex].Value;

                            DataGridViewComboBoxCell cell = ((DataGridViewComboBoxCell) row.Cells[columnIndex]);

                            //the cells value must be containced in the itemList:
                            if (!itemList.Contains(value))
                                itemList.Insert(0, value);

                            //Update ItemList
                            SetItemList(cell, itemList);

                            //Update Fore Color of the combobox cell
                            row.Cells[columnIndex].Style.ForeColor = comboConfig.GetForeColor(value, row.DataBoundItem);
                        }
                    }
                }
            }

            IsCellValueChangeEventDisabled = false;
        }



        /// <summary>
        /// Gets the column index of the bounded column 
        /// (by removing the prefix CMB_COLUMN_PREFIX of the combobox column name)
        /// </summary>
        /// <param name="cmbColumnIndex">column index of the combobox column</param>
        /// <returns>column index of bounded column</returns>
        private int GetBoundedColumnIndex(int cmbColumnIndex)
        {
            string boundedColumnName = this.Columns[cmbColumnIndex].Name.Substring(CMB_COLUMN_PREFIX.Length);
            return this.Columns[boundedColumnName].Index;
        }

        /// <summary>
        /// Removes all selected rows
        /// </summary>
        public void RemoveSelectedRows()
        {
            while (SelectedRows.Count > 0)
            {
                this.Rows.Remove(SelectedRows[0]);
            }
        }

        /// <summary>
        /// Set the select status of all selected checkbox cells
        /// </summary>
        /// <param name="select">check or uncheck the cell?</param>
        public void SelectCheckBoxes(bool select)
        {
            IsCellValueChangeEventDisabled = true;

            foreach (DataGridViewCell cell in this.SelectedCells)
            {
                if (cell is DataGridViewCheckBoxCell && !cell.ReadOnly)
                {
                    DirtyEditCell(cell, select);
                }
            }

            foreach (DataGridViewRow row in this.SelectedRows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell is DataGridViewCheckBoxCell && !cell.ReadOnly)
                    {
                        DirtyEditCell(cell, select);
                    }
                }
            }

            foreach (DataGridViewColumn col in this.SelectedColumns)
            {
                if (col is DataGridViewCheckBoxColumn && !col.ReadOnly)
                {
                    foreach (DataGridViewRow row in this.Rows)
                    {
                        if (!row.Cells[col.Index].ReadOnly)
                        {
                            DirtyEditCell(row.Cells[col.Index], select);
                        }
                    }
                }
            }

            IsCellValueChangeEventDisabled = false;
        }

        /// <summary>
        /// Sets the cells value and triggers the OnCurrentCellDirtyStateChanged event 
        /// to commit change immediately
        /// </summary>
        /// <param name="cell">the grid cell</param>
        /// <param name="value">the new cells value</param>
        public void DirtyEditCell(DataGridViewCell cell, object value)
        {
            //OnCurrentCellDirtyStateChanged event triggers only if the current cell changes
            DataGridViewCell currentCell = this.CurrentCell;
            this.CurrentCell = cell;
            cell.Value = value;
            this.CurrentCell = currentCell;
        }

        /// <summary>
        /// Sets values and itemList for all combobox cells for a bounded column spefified by the parameter columnName
        /// </summary>
        /// <param name="columnName">column name of the bounde column</param>
        public void RefreshCellBoundComboBox(string columnName)
        {
            IsCellValueChangeEventDisabled = true;

            int columnIndex = this.Columns[CMB_COLUMN_PREFIX + columnName].Index;
            int boundedColumnIndex = GetBoundedColumnIndex(columnIndex);

            ComboBoxConfiguration comboConfig = _cmbItemSources[columnIndex];

            foreach (DataGridViewRow row in this.Rows)
            {
                DataGridViewCell boundedCell = row.Cells[boundedColumnIndex];
                try
                {
                    List<object> itemList = comboConfig.GetItemList(row.DataBoundItem);

                    object value = boundedCell.Value;
                    //null values are not allowed
                    if (value == null)
                    {
                        boundedCell.Value = "";
                        value = "";
                    }
                    if (!itemList.Contains(value))
                        itemList.Insert(0, value);
                    DataGridViewComboBoxCell comboboxCell = ((DataGridViewComboBoxCell) row.Cells[columnIndex]);
                    SetItemList(comboboxCell, itemList);
                    comboboxCell.Value = value;
                }
                catch (Exception)
                {

                }
            }

            IsCellValueChangeEventDisabled = false;
        }


        /// <summary>
        /// Sets the itemList of a ComboBoxCell.
        /// Old Items are removed after new items have been added.
        /// This is to ensure that the cells value is always part of the itemList
        /// </summary>
        /// <param name="cell">the datagridview cell</param>
        /// <param name="itemList">the itenList</param>
        private void SetItemList(DataGridViewComboBoxCell cell, List<object> itemList)
        {
            int count = cell.Items.Count;
            // Add new Items
            cell.Items.AddRange(itemList.ToArray<object>());

            //Remove old items 
            for (int i = 0; i < count; i++)
            {
                cell.Items.RemoveAt(0);
            }
        }

    }


    /// <summary>
    /// Configuration for a cell bounded ComboBoxCell
    /// </summary>
    class ComboBoxConfiguration {
        //a Dictionary of dataBoundItems (objects that are bound to a row of the grid) and a datasource for the itemList of a comboBox
        private Dictionary<object, BindingList<object>> _itemSource;

        //ConfigurationType: None or Mark Invalid (invalid items foreColor is red)
        public ComponentFramework.Controls.IsagDataGridView.ComboboxConfigType ConfigType { get; set; }

        //Sort the itemList of the comboBox?
        public bool SortItemList { get; set; }

        /// <summary>
        /// ItemSource for added columns
        /// </summary>
        private BindingList<object> _itemSourceTemplate;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="itemSource">dataSource for itemList</param>
        /// <param name="configType">configuration type</param>
        /// <param name="sortItemList">sort ItemList?</param>
        public ComboBoxConfiguration(Dictionary<object, BindingList<object>> itemSource, ComponentFramework.Controls.IsagDataGridView.ComboboxConfigType configType, bool sortItemList)
        {
            _itemSource = itemSource;
            ConfigType = configType;
            SortItemList = sortItemList;
        }

        /// <summary>
        /// Add itemSource for new columns
        /// </summary>
        /// <param name="itemSourceTemplate">ItemSource for new columns</param>
        public void AddItenSourceTemplate(BindingList<object> itemSourceTemplate)
        {
            _itemSourceTemplate = itemSourceTemplate;
        }

        /// <summary>
        /// Adds an element to the _itemSource
        /// (happens when a row is added to the grid)
        /// </summary>
        /// <param name="dataBoundItem">dataBoundItem</param>
        public void AddItemSourceElement(object dataBoundItem)
        {
            if (!_itemSource.ContainsKey(dataBoundItem))
                _itemSource.Add(dataBoundItem, _itemSourceTemplate);
        }

        /// <summary>
        /// Removes an element from the _itemSource
        /// (happens when a row is removed from the grid)
        /// </summary>
        /// <param name="dataBoundItem">dataBoundItem</param>
        public void RemoveItemSourceElement(object dataBoundItem)
        {
            if (_itemSource.Count > 0 && _itemSource.ContainsKey(dataBoundItem))
                _itemSource.Remove(dataBoundItem);
        }

        /// <summary>
        /// Returns the ForeColor for the value inside the comboBoxCell
        /// </summary>
        /// <param name="item">item value</param>
        /// <param name="databoundedItem">dataBoundItem</param>
        /// <returns>cells fore color</returns>
        public Color GetForeColor(object item, object databoundedItem)
        {

            if (ConfigType == ComponentFramework.Controls.IsagDataGridView.ComboboxConfigType.MARK_INVALID)
                return IsValid(databoundedItem, item) ? Color.Black : Color.Red;

            //Use Standard Color
            return Color.Black;
        }

        /// <summary>
        /// Returns the ItemList for a databoundedItem.
        /// The itemList will be sorted if property SortItemList is true 
        /// </summary>
        /// <param name="databoundedItem">databoundedItem</param>
        /// <returns>itemList</returns>
        public List<object> GetItemList(object databoundedItem)
        {
            List<object> itemList = _itemSource[databoundedItem].ToList<Object>();
            if (SortItemList)
                System.Collections.ArrayList.Adapter(itemList).Sort(new ToStringComparer());
            return itemList.ToList<object>();
        }


        /// <summary>
        /// Is the item part of the itemList of a the databoundedItem?
        /// </summary>
        /// <param name="databoundedItem">databoundedItem</param>
        /// <param name="item">item value</param>
        /// <returns>is cell value valid?</returns>
        private bool IsValid(object databoundedItem, object item)
        {
            return _itemSource[databoundedItem].Contains(item);
        }

        /// <summary>
        /// Does the configuration contain an itemSource?
        /// </summary>
        /// <param name="itemSource">itemSource</param>
        /// <returns>is itemSource contained in this configuration?</returns>
        public bool HasItemSource(BindingList<object> itemSource)
        {
            return _itemSource.ContainsValue(itemSource);
        }

        /// <summary>
        /// Does the configuration contain an itemSource for a given dataBoundItem?
        /// </summary>
        /// <param name="itemSource">itemSource</param>
        /// <param name="databoundedItem">databoundedItem</param>
        /// <returns></returns>
        public bool HasItemSource(BindingList<object> itemSource, object databoundedItem)
        {
            return _itemSource.ContainsKey(databoundedItem) && _itemSource[databoundedItem] == itemSource;
        }


    }

    /// <summary>
    /// Compares 2 objects. If objects are not IComparable, string values are compared.
    /// </summary>
    public class ToStringComparer: System.Collections.IComparer {

        /// <summary>
        /// compares 2 values
        /// </summary>
        /// <param name="left">left value</param>
        /// <param name="right">right value</param>
        /// <returns></returns>
        public int Compare(object left, object right)
        {
            object leftValue = left;
            object rightValue = right;

            int compareResult;


            if (leftValue == null)
            {
                compareResult = (rightValue == null) ? 0 : -1; //treat nulls as equal
            }
            if (rightValue == null)
            {
                compareResult = 1;
            }
            if (leftValue is IComparable)
            {
                compareResult = ((IComparable) leftValue).CompareTo(rightValue);
            }
            if (leftValue.Equals(rightValue))
            {
                compareResult = 0;
            }
            else
            {
                //not IComparable -> compare strings
                compareResult = leftValue.ToString().CompareTo(rightValue.ToString());
            }

            return compareResult;
        }
    }

}
