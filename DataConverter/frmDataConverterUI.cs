using System;
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
using DataConverter.ComponentFrameWork.Mapping;
using Lookup2.ComponentFramework.Controls;

namespace DataConverter
{
    public partial class frmDataConverterUI : Form
    {
        private IDTSComponentMetaData100 _metadata;
        private Variables _variables;
        private IsagCustomProperties _IsagCustomProperties;


        private Dictionary<string, IDTSInputColumn100> _inputColumnsDictionary;
        private bool _abortClosing = false;

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

            cbErrorHandling.SelectedIndexChanged += new System.EventHandler(cbErrorHandling_SelectedIndexChanged);

            SetToolTips();
            InitializeContextMenu();


        }


        private void ShowEditor()
        {
            string inputColumnName = idgvMapping.Rows[idgvMapping.CurrentCell.RowIndex].Cells["InputColumnName"].Value.ToString();
            frmEditor editor = new frmEditor(inputColumnName, idgvMapping.CurrentCell.Value.ToString(), _IsagCustomProperties.GetInputColumns());

            if (editor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                idgvMapping.CurrentCell.Value = editor.Value;
        }





        private void PopulateMappingGrid()
        {
            idgvMapping.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnValidation;
            idgvMapping.DataSource = _IsagCustomProperties.ColumnConfigList;
            idgvMapping.AddCellBoundedComboBox("DataType", Constants.DATATYPE_LIST());
            idgvMapping.AddCellBoundedComboBox("ErrorHandling", typeof(IsagCustomProperties.ErrorRowHandling));

            List<object> listConversion = Common.GetListFromEnum(typeof(DateConvertTypes));
            listConversion.Remove(DateConvertTypes.STR2YYYYMMDD.ToString());
            listConversion.AddRange(Constants.STRING_CONVERSION_TYPES);
            idgvMapping.AddCellBoundedComboBox("ConversionAsString", new BindingList<object>(listConversion));

            foreach (DataGridViewRow row in idgvMapping.Rows)
            {
                SetGridColumnsActivationState(row);
            }

            tbPrefix.DataBindings.Add("Text", _IsagCustomProperties, "AliasPrefix");


            //Regular Expressions

            cmbRegEx.DataSource=  RegularExpressions.LoadFromXml(Constants.PATH_REGEX);
            cmbRegEx.DisplayMember = "Name";
            cmbRegEx.ValueMember = "Pattern";           

            //conversion Column            
            SetConversionColumnState();

        }

        private void PopulateComboVariables()
        {
            foreach (Variable variable in _variables)
            {
                cmbErrorName.Items.Add(variable.QualifiedName);
            }           

            cmbErrorName.Text  = _IsagCustomProperties.ErrorName;
        }

        private void SetToolTips()
        {
            idgvMapping.Columns["Default"].ToolTipText = "Replaces NULL values with the OnNull value.";
            idgvMapping.Columns["OnErrorValue"].ToolTipText = "If the value cannot be converted, the OnError value is used. /n If \"Redirect\" has been choosen, this row will also sent to the error putput.";
            idgvMapping.Columns["AllowNull"].ToolTipText = "If \"AllowNull\" has been selected, input columns with NULL values are allowed and and sent to the output.";
            idgvMapping.Columns["IsErrorCounter"].ToolTipText = "If selected the column will never be converted. \n For each Row the error counter will be increased for each error.";
        }

        private void PopulateRowDisposition()
        {
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_FailComponent);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_IgnoreFailure);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_RedirectRow);

            cbErrorHandling.SelectedItem = _metadata.InputCollection[Constants.INPUT_NAME].ErrorRowDisposition;
        }

        private void SetOuputDataTypeAsInputDataType(DataGridViewRow row)
        {
            IDTSInputColumn100 inputColumn = _inputColumnsDictionary[row.Cells["InputColumnName"].Value.ToString()];

            row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "DataType"].Value = inputColumn.DataType.ToString();
            row.Cells["Length"].Value = inputColumn.Length;
            row.Cells["Precision"].Value = inputColumn.Precision;
            row.Cells["Scale"].Value = inputColumn.Scale;
            row.Cells["Codepage"].Value = inputColumn.CodePage;
        }

        private void DisableOutputColumnData(DataGridViewRow row)
        {
            SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "DataType"], true);
            SetCellEnabledStatus(row.Cells["Length"], true);
            SetCellEnabledStatus(row.Cells["Precision"], true);
            SetCellEnabledStatus(row.Cells["Scale"], true);
            SetCellEnabledStatus(row.Cells["Codepage"], true);
        }

        private void SetConversionColumnState(DataGridViewRow row)
        {
            ColumnConfig column = _IsagCustomProperties.GetColumnConfigByInputColumnName(row.Cells["InputColumnName"].Value.ToString());

            DataGridViewCell cell = row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ConversionAsString"];
            SetCellEnabledStatus(cell, !column.SupportsConversion);


            // row.Cells["ConversionAsString"].ValueList = Common.CreateValueList(column.SupportedConversions);
        }
        private void SetConversionColumnState()
        {
            foreach (DataGridViewRow row in idgvMapping.Rows)
            {
                SetConversionColumnState(row);
            }
        }




        private void SetCellEnabledStatus(DataGridViewCell cell, bool readOnly)
        {
            cell.ReadOnly = readOnly;

            cell.Style.BackColor = readOnly ? Color.LightGray : Color.White;
        }

        public void SetGridColumnsActivationState(DataGridViewRow row)
        {

            //Always readonly:
            SetCellEnabledStatus(row.Cells["DataTypeInput"], true);
            SetCellEnabledStatus(row.Cells["InputColumnName"], true);

            if ((bool)row.Cells["IsErrorCounter"].Value)
            {

                SetCellEnabledStatus(row.Cells["Convert"], true);
                SetCellEnabledStatus(row.Cells["Default"], true);
                SetCellEnabledStatus(row.Cells["OnErrorValue"], true);
                SetCellEnabledStatus(row.Cells["RegEx"], true);
                SetCellEnabledStatus(row.Cells["AllowNull"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ErrorHandling"], true);

                DisableOutputColumnData(row);
                SetOuputDataTypeAsInputDataType(row);

                SetCellEnabledStatus(row.Cells["OutputAlias"], false);
                SetCellEnabledStatus(row.Cells["IsErrorCounter"], false);

                idgvMapping.Refresh();
                return;
            }

            if (!(bool)row.Cells["Convert"].Value)
            {
                DisableOutputColumnData(row);
                SetOuputDataTypeAsInputDataType(row);

                SetCellEnabledStatus(row.Cells["Default"], true);
                SetCellEnabledStatus(row.Cells["RegEx"], true);
                SetCellEnabledStatus(row.Cells["OnErrorValue"], true);
                SetCellEnabledStatus(row.Cells["AllowNull"], true);
                SetCellEnabledStatus(row.Cells[IsagDataGridView.CMB_COLUMN_PREFIX + "ErrorHandling"], true);

                SetCellEnabledStatus(row.Cells["IsErrorCounter"], false);
                SetCellEnabledStatus(row.Cells["Convert"], false);
                SetCellEnabledStatus(row.Cells["OutputAlias"], false);

                idgvMapping.Refresh();
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


            idgvMapping.Refresh();
        }

        #region Load & Save

        private void LoadCustomProperties()
        {
            object configuration = _metadata.CustomPropertyCollection[Constants.PROP_CONFIG].Value;

            _IsagCustomProperties = IsagCustomProperties.Load(configuration);

            if (_IsagCustomProperties == null) _IsagCustomProperties = new IsagCustomProperties();

        }

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
                //Inputspalten + Convert
                foreach (ColumnConfig config in _IsagCustomProperties.ColumnConfigList)
                {
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection[config.InputColumnName];
                    if (config.IsErrorCounter) vInput.SetUsageType(inputColumn.LineageID, DTSUsageType.UT_READWRITE);
                    else vInput.SetUsageType(inputColumn.LineageID, DTSUsageType.UT_READONLY);

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
                            Mapping.SetIdProperty(config.CustomId, convertCol.CustomPropertyCollection);
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

                            MessageBox.Show("Fehler beim Setzen der Datentyp-Eigenschaften für die Spalte " + convertCol.Name + ".", "DataConverter");
                            return false;
                        }

                    }
                    else config.RemoveOutput();
                }

                //Neue Spalten
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

                    //DataType setzen
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            _abortClosing = !save();
        }

        private void btnApplyPrefix_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in idgvMapping.Rows)
            {
                if (((bool)row.Cells["Convert"].Value))
                    row.Cells["OutputAlias"].Value = tbPrefix.Text + row.Cells["InputColumnName"].Value.ToString();
            }
        }

        private void SelectCheckBoxes(bool select)
        {
            foreach (DataGridViewCell cell in idgvMapping.SelectedCells)
            {
                if (idgvMapping.Columns[cell.ColumnIndex].Name == "Convert" || idgvMapping.Columns[cell.ColumnIndex].Name == "Allow Null")
                {
                    cell.Value = select;
                }
            }

            foreach (DataGridViewRow row in idgvMapping.SelectedRows)
            {
                row.Cells["Convert"].Value = select;
                row.Cells["AllowNull"].Value = select;
            }

            foreach (DataGridViewColumn col in idgvMapping.SelectedColumns)
            {
                if (col.Name == "Convert" || col.Name == "Allow Null")
                {
                    foreach (DataGridViewRow row in idgvMapping.Rows)
                    {
                        row.Cells[col.Index].Value = select;
                    }
                }
            }

            //if cells have been selected, one chekcbox cell value change is visible after user clicks elsewhere. Workaround:
            DataGridViewCell currentCell = idgvMapping.CurrentCell;
            idgvMapping.CurrentCell = null;
            idgvMapping.CurrentCell = currentCell;
        }


        private void idgvMapping_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewColumn col = idgvMapping.Columns[e.ColumnIndex];
            DataGridViewRow row = idgvMapping.Rows[e.RowIndex];
            if (col.Name == IsagDataGridView.CMB_COLUMN_PREFIX + "DataType" || col.Name == "IsErrorCounter" || col.Name == "Convert") SetGridColumnsActivationState(row);
            else if (col.Name == "ErrorHandling") cbErrorHandling.SelectedItem = _IsagCustomProperties.GetRowDisposition();

            SetConversionColumnState(row);
        }

        private void TestRegEx(DataGridViewRow row)
        {
            //^(2[0-3]|[0-1]?[0-9])(:[0-5][0-9]){2}$(2[0-3]|[0-1]?[0-9])
            string pattern = row.Cells["RegEx"].Value.ToString();
            string value = row.Cells["OnErrorValue"].Value.ToString();
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MessageBox.Show(rgx.IsMatch(value).ToString());

        }

        private void cbErrorHandling_SelectedIndexChanged(object sender, EventArgs e)
        {
            _IsagCustomProperties.SetRowDisposition((DTSRowDisposition)cbErrorHandling.SelectedItem);
        } 

        

        #endregion

        #region ContextMenu
        private void idgvMapping_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //Menu Item "Editor" is visiblie if user clicked on a "Compare" cell
                bool showEdit = idgvMapping.Columns[idgvMapping.CurrentCell.ColumnIndex].Name == "Compare";
                idgvMapping.ContextMenu.MenuItems[0].Visible = showEdit;
                idgvMapping.ContextMenu.MenuItems[1].Visible = showEdit;

                //Menu Item "Editor" is visiblie if user clicked on a "Compare" cell
                bool showRegEx = idgvMapping.Columns[idgvMapping.CurrentCell.ColumnIndex].Name == "RegEx" && !idgvMapping.CurrentCell.ReadOnly;
                idgvMapping.ContextMenu.MenuItems[2].Visible = showRegEx;
                idgvMapping.ContextMenu.MenuItems[3].Visible = showRegEx;

                // show context menu
                idgvMapping.ContextMenu.Show(idgvMapping, new Point(e.X, e.Y));
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            switch (item.Text)
            {
                case "DebugMode":
                    item.Checked = !item.Checked;
                    _IsagCustomProperties.DebugModus = item.Checked;
                    break;
                case "Select":
                    SelectCheckBoxes(true);
                    break;
                case "DeSelect":
                    SelectCheckBoxes(false);
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
        private void InitializeContextMenu()
        {
            idgvMapping.ContextMenu = new ContextMenu();
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Editor", menuItem_Click));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Insert RegEx", menuItem_Click));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Select", menuItem_Click));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("DeSelect", menuItem_Click));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("Apply Alias Prefix", menuItem_Click));
            idgvMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem item = new MenuItem("DebugMode", menuItem_Click);
            item.Checked = _IsagCustomProperties.DebugModus;


            idgvMapping.ContextMenu.MenuItems.Add(item);
        }

        #endregion

    
        private void frmDataConverterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_abortClosing && this.DialogResult == System.Windows.Forms.DialogResult.OK &&
                MessageBox.Show("Error while saving. Changes would be lost. Abort closing the window?", "DataConverter",MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                e.Cancel = _abortClosing;
        }

        private void cmbErrorName_TextChanged(object sender, EventArgs e)
        {
            _IsagCustomProperties.ErrorName = cmbErrorName.Text;
        }


        private void InsertRegEx()
        {
            if (cmbRegEx.SelectedValue != null) idgvMapping.CurrentCell.Value = cmbRegEx.SelectedValue.ToString();
        }
       

    }
}
