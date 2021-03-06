﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dts.Runtime;
using ComponentFramework.Controls;
using DataConverter.FrameWork.Mapping;



namespace DataConverter {
    /// <summary>
    /// The GUI for the DataConverter component
    /// </summary>
    public partial class frmDataConverterUI: Form {
        /// <summary>
        /// SSIS metadata for the component
        /// </summary>
        private IDTSComponentMetaData100 _metadata;

        /// <summary>
        /// SSIS variables
        /// </summary>
        private Variables _variables;

        /// <summary>
        /// custom properties for the component
        /// </summary>
        private IsagCustomProperties _IsagCustomProperties;

        /// <summary>
        /// Dictionary of input column names and SSIS input columns
        /// </summary>
        private Dictionary<string, IDTSInputColumn100> _inputColumnsDictionary;

        /// <summary>
        /// Abort closing the GUI?
        /// </summary>
        private bool _abortClosing = false;

        /// <summary>
        /// constructor of the GUI
        /// </summary>
        /// <param name="metadata">SSIS metadata for the component</param>
        /// <param name="variables">SSIS variables</param>
        public frmDataConverterUI(IDTSComponentMetaData100 metadata, Variables variables)
        {
            InitializeComponent();


            _metadata = metadata;
            _variables = variables;

            LoadCustomProperties();
            this.Text += " " + _IsagCustomProperties.Version;

            _inputColumnsDictionary = ComponentMetaDataTools.GetInputDictionary(metadata);
            PopulateMappingGrid();

            PopulateRowDisposition();
            PopulateComboVariables();
            PopulateLanguage();

            cbErrorHandling.SelectedIndexChanged += new System.EventHandler(cbErrorHandling_SelectedIndexChanged);

            SetToolTips();
            InitializeContextMenu();

            this.Load += frmDataConverterUI_Load;

        }

        /// <summary>
        /// After Windows Form has been loaded the grid cells readonly property has to be set
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void frmDataConverterUI_Load(object sender, EventArgs e)
        {
            SetGridColumnsActivationState();
        }

        /// <summary>
        /// Show compare expression editor
        /// </summary>
        private void ShowEditor()
        {
            string inputColumnName = dgvMapping.Rows[dgvMapping.CurrentCell.RowIndex].Cells["InputColumnName"].Value.ToString();
            frmEditor editor = new frmEditor(inputColumnName, dgvMapping.CurrentCell.Value.ToString(), _IsagCustomProperties.GetInputColumns());

            if (editor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                dgvMapping.CurrentCell.Value = editor.Value;
        }

        /// <summary>
        /// - Adds data and events to the grid.
        /// - Prefix and regular Expression combobox is set 
        /// - Set readonly property of grid cells for conversions  
        /// </summary>
        private void PopulateMappingGrid()
        {
            //Grid
            dgvMapping.DataSource = _IsagCustomProperties.ColumnConfigList;

            dgvMapping.AddCellBoundedComboBox("DataType", Constants.DATATYPE_LIST(), true);
            dgvMapping.AddCellBoundedComboBox("ErrorHandling", typeof(IsagCustomProperties.ErrorRowHandling), false);

            Dictionary<object, BindingList<object>> itemListSource = new Dictionary<object, BindingList<object>>();
            foreach (ColumnConfig config in _IsagCustomProperties.ColumnConfigList)
            {
                itemListSource.Add(config, config.SupportedConversions);
            }

            dgvMapping.AddCellBoundedComboBox("ConversionAsString", itemListSource, IsagDataGridView.ComboboxConfigType.MARK_INVALID, false);

            dgvMapping.CellValueChanged += dgvMapping_CellValueChanged;
            dgvMapping.MouseDown += dgvMapping_MouseDown;

            //Prefix
            tbPrefix.DataBindings.Add("Text", _IsagCustomProperties, "AliasPrefix");

            //Regular Expressions
            cmbRegEx.DataSource = RegularExpressions.LoadFromXml(Constants.PATH_REGEX);
            cmbRegEx.DisplayMember = "Name";
            cmbRegEx.ValueMember = "Pattern";

            //Set readonly property of grid cells for conversions       
            SetConversionColumnState();

        }

        /// <summary>
        /// Populate Error Name combobox with variables
        /// </summary>
        private void PopulateComboVariables()
        {
            foreach (Variable variable in _variables)
            {
                cmbErrorName.Items.Add(variable.QualifiedName);
            }

            cmbErrorName.Text = _IsagCustomProperties.ErrorName;
        }

        /// <summary>
        /// Set tool tips
        /// </summary>
        private void SetToolTips()
        {
            dgvMapping.Columns["Default"].ToolTipText = "Replaces NULL values with the OnNull value.";
            dgvMapping.Columns["OnErrorValue"].ToolTipText = "If the value cannot be converted, the OnError value is used. /n If \"Redirect\" has been choosen, this row will also sent to the error putput.";
            dgvMapping.Columns["AllowNull"].ToolTipText = "If \"AllowNull\" has been selected, input columns with NULL values are allowed and and sent to the output.";
            dgvMapping.Columns["IsErrorCounter"].ToolTipText = "If selected the column will never be converted. \n For each Row the error counter will be increased for each error.";
        }

        /// <summary>
        /// Fill grid cell comboboxes itemlists for row dispositions 
        /// </summary>
        private void PopulateRowDisposition()
        {
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_FailComponent);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_IgnoreFailure);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_RedirectRow);

            cbErrorHandling.SelectedItem = _metadata.InputCollection[Constants.INPUT_NAME].ErrorRowDisposition;
        }

        /// <summary>
        /// Fill Language / CultureInfo itemlist 
        /// </summary>
        private void PopulateLanguage()
        {
            cmbLanguage.DataBindings.Add("Text", _IsagCustomProperties, "Language", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbLanguage.Items.AddRange(_IsagCustomProperties.LanguageItemList);
        }

        /// <summary>
        /// Sets output datatype by using input column datatype
        /// </summary>
        /// <param name="row">datagridview row</param>
        private void SetOuputDataTypeAsInputDataType(DataGridViewRow row)
        {
            IDTSInputColumn100 inputColumn = _inputColumnsDictionary[row.Cells["InputColumnName"].Value.ToString()];

            row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "DataType"].Value = inputColumn.DataType.ToString();
            row.Cells["Length"].Value = inputColumn.Length;
            row.Cells["Precision"].Value = inputColumn.Precision;
            row.Cells["Scale"].Value = inputColumn.Scale;
            row.Cells["Codepage"].Value = inputColumn.CodePage;
        }

        /// <summary>
        /// Set readonly property of grid cells containing output column datatype
        /// </summary>
        /// <param name="row">datagridview row</param>
        private void DisableOutputColumnDataType(DataGridViewRow row)
        {
            SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "DataType"], true);
            SetCellEnabledStatus(row.Cells["Length"], true);
            SetCellEnabledStatus(row.Cells["Precision"], true);
            SetCellEnabledStatus(row.Cells["Scale"], true);
            SetCellEnabledStatus(row.Cells["Codepage"], true);
        }

        /// <summary>
        /// Set readonly property of grid cell containing the conversion type (for a single row)    
        /// </summary>
        /// <param name="row">grid row</param>
        private void SetConversionColumnState(DataGridViewRow row)
        {
            ColumnConfig column = _IsagCustomProperties.GetColumnConfigByInputColumnName(row.Cells["InputColumnName"].Value.ToString());

            DataGridViewCell cell = row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ConversionAsString"];
            SetCellEnabledStatus(cell, !column.SupportsConversion);
        }

        /// <summary>
        /// Set readonly property of all grid cells containing the conversion type
        /// </summary>
        private void SetConversionColumnState()
        {
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                SetConversionColumnState(row);
            }
        }

        /// <summary>
        /// Set readonly property of a grid cell
        /// </summary>
        /// <param name="cell">grid cell</param>
        /// <param name="readOnly">readonly?</param>
        private void SetCellEnabledStatus(DataGridViewCell cell, bool readOnly)
        {
            cell.ReadOnly = readOnly;
        }

        /// <summary>
        /// Set readonly property for cells of all rows
        /// </summary>
        public void SetGridColumnsActivationState()
        {
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                SetGridColumnsReadonlyState(row);
            }
        }

        /// <summary>
        /// Set readonly property for cell of a single grid row
        /// </summary>
        /// <param name="row">grid row</param>
        public void SetGridColumnsReadonlyState(DataGridViewRow row)
        {

            //Always readonly:
            SetCellEnabledStatus(row.Cells["DataTypeInput"], true);
            SetCellEnabledStatus(row.Cells["InputColumnName"], true);

            if ((bool) row.Cells["IsErrorCounter"].Value)
            {

                SetCellEnabledStatus(row.Cells["Convert"], true);
                SetCellEnabledStatus(row.Cells["Default"], true);
                SetCellEnabledStatus(row.Cells["OnErrorValue"], true);
                SetCellEnabledStatus(row.Cells["RegEx"], true);
                SetCellEnabledStatus(row.Cells["AllowNull"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ErrorHandling"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ConversionAsString"], true);

                DisableOutputColumnDataType(row);
                SetOuputDataTypeAsInputDataType(row);

                SetCellEnabledStatus(row.Cells["OutputAlias"], false);
                SetCellEnabledStatus(row.Cells["IsErrorCounter"], false);

                return;
            }

            if (!(bool) row.Cells["Convert"].Value)
            {
                DisableOutputColumnDataType(row);
                SetOuputDataTypeAsInputDataType(row);

                SetCellEnabledStatus(row.Cells["Default"], true);
                SetCellEnabledStatus(row.Cells["RegEx"], true);
                SetCellEnabledStatus(row.Cells["OnErrorValue"], true);
                SetCellEnabledStatus(row.Cells["AllowNull"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ErrorHandling"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ConversionAsString"], true);

                SetCellEnabledStatus(row.Cells["IsErrorCounter"], false);
                SetCellEnabledStatus(row.Cells["Convert"], false);
                SetCellEnabledStatus(row.Cells["OutputAlias"], false);

                return;
            }

            ColumnConfig config = _IsagCustomProperties.GetColumnConfigByInputColumnName(row.Cells["InputColumnName"].Value.ToString());

            SetCellEnabledStatus(row.Cells["Convert"], false);
            SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "DataType"], false);
            SetCellEnabledStatus(row.Cells["Default"], false);
            SetCellEnabledStatus(row.Cells["Length"], !config.HasLength());
            SetCellEnabledStatus(row.Cells["Precision"], !config.HasPrecision());
            SetCellEnabledStatus(row.Cells["Scale"], !config.HasScale());
            SetCellEnabledStatus(row.Cells["Codepage"], !config.HasCodePage());
            SetCellEnabledStatus(row.Cells["RegEx"], false);
            SetCellEnabledStatus(row.Cells["OnErrorValue"], false);
            SetCellEnabledStatus(row.Cells["AllowNull"], false);
            SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ErrorHandling"], false);
            SetCellEnabledStatus(row.Cells["IsErrorCounter"], false);
            SetCellEnabledStatus(row.Cells["OutputAlias"], false);
            SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ConversionAsString"], false);
        }

        #region Load & Save

        private void LoadCustomProperties()
        {
            object configuration = _metadata.CustomPropertyCollection[Constants.PROP_CONFIG].Value;

            _IsagCustomProperties = IsagCustomProperties.Load(configuration);

            if (_IsagCustomProperties == null)
                _IsagCustomProperties = new IsagCustomProperties();

        }

        /// <summary>
        /// Saves the custom properties
        /// </summary>
        /// <returns>saving succesful?</returns>
        private bool save()
        {
            IDTSOutput100 output = _metadata.OutputCollection[Constants.OUTPUT_NAME];
            IDTSInput100 input = _metadata.InputCollection[Constants.INPUT_NAME];
            IDTSVirtualInput100 vInput = input.GetVirtualInput();

            if (output.SynchronousInputID == 0)
            {
                output.OutputColumnCollection.RemoveAll();
                output.SynchronousInputID = input.ID;
                output.ExclusionGroup = 1;

                IDTSOutput100 outputDummy = _metadata.OutputCollection.New();
                outputDummy.Name = Constants.OUTPUT_NAME + "dummy";
                outputDummy.SynchronousInputID = input.ID; //0 -> asynchron, ID Input -> synchron
                outputDummy.ExclusionGroup = 1;


            }

            try
            {
                //input columns + convert
                foreach (ColumnConfig config in _IsagCustomProperties.ColumnConfigList)
                {
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection[config.InputColumnName];
                    if (config.IsErrorCounter)
                        vInput.SetUsageType(inputColumn.LineageID, DTSUsageType.UT_READWRITE);
                    else
                        vInput.SetUsageType(inputColumn.LineageID, DTSUsageType.UT_READONLY);

                    if (config.Convert)
                    {
                        IDTSOutputColumn100 convertCol = null;

                        try
                        {
                            convertCol = output.OutputColumnCollection.GetOutputColumnByLineageID(Convert.ToInt32(config.OutputLineageId));
                        }
                        catch (Exception)
                        {
                            convertCol = output.OutputColumnCollection.New();
                            config.OutputLineageId = convertCol.LineageID.ToString();
                            config.OutputId = convertCol.ID.ToString();
                            config.OutputIdString = convertCol.IdentificationString;
                            LineageMapping.SetIdProperty(config.CustomId, convertCol.CustomPropertyCollection);
                        }

                        convertCol.Name = config.OutputAlias;
                        try
                        {
                            convertCol.SetDataTypeProperties(
                                ComponentMetaDataTools.GetDataType(config.DataType),
                                Convert.ToInt32(config.Length),
                                Convert.ToInt32(config.Precision),
                                Convert.ToInt32(config.Scale),
                                Convert.ToInt32(config.Codepage));
                        }
                        catch (Exception)
                        {

                            MessageBox.Show("Saving datatype properties for column " + convertCol.Name + " failed.", "DataConverter");
                            return false;
                        }

                    }
                    else
                        config.RemoveOutput();
                }

                //new columns (not used anymore)
                foreach (NewColumnConfig config in _IsagCustomProperties.NewColumnConfigList)
                {
                    IDTSOutputColumn100 outputColumn = null;

                    if (config.HasOutput())
                    {
                        outputColumn = output.OutputColumnCollection.GetObjectByID(Convert.ToInt32(config.OutputId));

                        outputColumn.Name = config.OutputAlias;
                    }
                    else
                    {
                        outputColumn = output.OutputColumnCollection.New();
                        outputColumn.Name = config.OutputAlias;

                        config.OutputId = outputColumn.ID.ToString();
                        config.OutputIdString = outputColumn.IdentificationString;
                        config.OutputLineageId = outputColumn.LineageID.ToString();
                    }

                    //set datatype
                    try
                    {
                        outputColumn.SetDataTypeProperties(
                                        ComponentMetaDataTools.GetDataType(config.DataType),
                                        Convert.ToInt32(config.Length),
                                        Convert.ToInt32(config.Precision),
                                        Convert.ToInt32(config.Scale),
                                        Convert.ToInt32(config.Codepage));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fehler beim Erstellen der Output-Spalte " + config.OutputAlias + ".", "DataConverter");
                        throw ex;
                    }
                }

                for (int i = output.OutputColumnCollection.Count - 1; i >= input.InputColumnCollection.Count; i++)
                {
                    if (!_IsagCustomProperties.HasOutputColumn(output.OutputColumnCollection[i].LineageID.ToString()) &&
                        !_IsagCustomProperties.HasNewOutputColumn(output.OutputColumnCollection[i].LineageID.ToString()))
                    {
                        output.OutputColumnCollection.RemoveObjectByIndex(i);
                    }
                }

                if (!_IsagCustomProperties.IsOnErrorValid(_metadata))
                {
                    MessageBox.Show("The On Error value is invalid", "DataConverter");
                    return false;
                }

                if (!_IsagCustomProperties.IsOnNullValid(_metadata))
                {
                    MessageBox.Show("The On Null value is invalid", "DataConverter");
                    return false;
                }

                if (!_IsagCustomProperties.IsCompareValid(_metadata))
                {
                    MessageBox.Show("The compare expression is invalid", "DataConverter");
                    return false;
                }

                _metadata.InputCollection[Constants.INPUT_NAME].ErrorRowDisposition = _IsagCustomProperties.GetRowDisposition();
                _IsagCustomProperties.Save(_metadata);
            }
            catch (Exception)
            {
                return false;
            }


            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// When OK button is clicked the user may abort leaving the GUI if saving is impossible due to errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            _abortClosing = !save();
        }

        /// <summary>
        /// Sets all output column names to prefix + input column name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApplyPrefix_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                if (((bool) row.Cells["Convert"].Value))
                    row.Cells["OutputAlias"].Value = tbPrefix.Text + row.Cells["InputColumnName"].Value.ToString();
            }
        }

        /// <summary>
        /// Reacts on datagridview cell value changed
        /// - if error handling cell changed the all row disposition are recalculated
        /// - if output datatype, IsErrorCounter or Convert cell has changed, readonly state is changed
        /// - readonly state of conversion columns is set
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void dgvMapping_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewColumn col = dgvMapping.Columns[e.ColumnIndex];
            DataGridViewRow row = dgvMapping.Rows[e.RowIndex];
            if (col.Name == IsagDataGridView.CMB_COLUMN_PREFIX + "DataType" || col.Name == "IsErrorCounter" || col.Name == "Convert")
                SetGridColumnsReadonlyState(row);
            else if (col.Name == "ErrorHandling")
                cbErrorHandling.SelectedItem = _IsagCustomProperties.GetRowDisposition();

            SetConversionColumnState(row);
        }

        //private void TestRegEx(DataGridViewRow row)
        //{
        //    //^(2[0-3]|[0-1]?[0-9])(:[0-5][0-9]){2}$(2[0-3]|[0-1]?[0-9])
        //    string pattern = row.Cells["RegEx"].Value.ToString();
        //    string value = row.Cells["OnErrorValue"].Value.ToString();
        //    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        //    MessageBox.Show(rgx.IsMatch(value).ToString());

        //}

        /// <summary>
        /// Set row disposition if ErrorHandling value changed
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void cbErrorHandling_SelectedIndexChanged(object sender, EventArgs e)
        {
            _IsagCustomProperties.SetRowDisposition((DTSRowDisposition) cbErrorHandling.SelectedItem);
        }



        #endregion

        #region ContextMenu

        /// <summary>
        /// If left mouse button is pressed, context menu is updated and shown
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void dgvMapping_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //Menu Item "Editor" is visiblie if user clicked on a "Compare" cell
                bool showEdit = dgvMapping.Columns[dgvMapping.CurrentCell.ColumnIndex].Name == "Compare";
                dgvMapping.ContextMenu.MenuItems[0].Visible = showEdit;
                dgvMapping.ContextMenu.MenuItems[1].Visible = showEdit;

                //Menu Item "Editor" is visiblie if user clicked on a "Compare" cell
                bool showRegEx = dgvMapping.Columns[dgvMapping.CurrentCell.ColumnIndex].Name == "RegEx" && !dgvMapping.CurrentCell.ReadOnly;
                dgvMapping.ContextMenu.MenuItems[2].Visible = showRegEx;
                dgvMapping.ContextMenu.MenuItems[3].Visible = showRegEx;

                // show context menu
                dgvMapping.ContextMenu.Show(dgvMapping, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// If menu item has been click proper method is called
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void menuItem_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem) sender;

            switch (item.Text)
            {
                case "DebugMode":
                    item.Checked = !item.Checked;
                    _IsagCustomProperties.DebugModus = item.Checked;
                    break;
                case "Select":
                    dgvMapping.SelectCheckBoxes(true);
                    break;
                case "DeSelect":
                    dgvMapping.SelectCheckBoxes(false);
                    break;
                case "Apply Alias Prefix":
                    btnApplyPrefix_Click(null, null);
                    break;
                case "Editor":
                    ShowEditor();
                    break;
                case "Insert RegEx":
                    InsertRegEx();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes the context menu
        /// </summary>
        private void InitializeContextMenu()
        {
            dgvMapping.ContextMenu = new ContextMenu();
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Editor", menuItem_Click));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Insert RegEx", menuItem_Click));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Select", menuItem_Click));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("DeSelect", menuItem_Click));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Apply Alias Prefix", menuItem_Click));
            dgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem item = new MenuItem("DebugMode", menuItem_Click);
            item.Checked = _IsagCustomProperties.DebugModus;

            dgvMapping.ContextMenu.MenuItems.Add(item);
        }

        #endregion

        /// <summary>
        /// If GUI is about to be closed and configuration contains errors the user has to decide if closing the window is aborted
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void frmDataConverterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_abortClosing && this.DialogResult == System.Windows.Forms.DialogResult.OK &&
                MessageBox.Show("Error while saving. Changes would be lost. Abort closing the window?", "DataConverter", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                e.Cancel = _abortClosing;
        }

        /// <summary>
        /// Set Error Name if user changed the value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbErrorName_TextChanged(object sender, EventArgs e)
        {
            _IsagCustomProperties.ErrorName = cmbErrorName.Text;
        }

        /// <summary>
        /// Inserts selected regular expression into current cell
        /// </summary>
        private void InsertRegEx()
        {
            if (cmbRegEx.SelectedValue != null)
                dgvMapping.CurrentCell.Value = cmbRegEx.SelectedValue.ToString();
        }


    }
}
