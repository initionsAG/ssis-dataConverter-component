using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
//using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Collections;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dts.Runtime;
using DataConverter.ComponentFrameWork.Mapping;

namespace DataConverter
{
    public partial class frmDataConverterUI : Form
    {
        private IDTSComponentMetaData100 _metadata;
        private Variables _variables;
        private IsagCustomProperties _IsagCustomProperties;
        private IsagDataGrid _ugMapping = new IsagDataGrid();
        private IsagDataGrid _ugNewColumns = new IsagDataGrid();
        private Dictionary<string, IDTSInputColumn100> _inputColumnsDictionary;
        private bool _abortClosing = false;

        public frmDataConverterUI(IDTSComponentMetaData100 metadata, Variables variables)
        {
            InitializeComponent();
            InitializeCustomComponents();

            _metadata = metadata;
            _variables = variables;

            LoadCustomProperties();
            this.Text += " " + _IsagCustomProperties.Version;

            _inputColumnsDictionary = ComponentMetaDataTools.GetInputDictionary(metadata);
            PopulateMappingGrid();
            PopulateNewColumnsGrid();
            PopulateRowDisposition();
            PopulateComboVariables();

            _ugMapping.AfterCellUpdate += new CellEventHandler(ugMapping_AfterCellUpdate);
            _ugNewColumns.AfterCellUpdate += new CellEventHandler(_ugNewColumns_AfterCellUpdate);
            cbErrorHandling.SelectedIndexChanged += new System.EventHandler(cbErrorHandling_SelectedIndexChanged);

            SetToolTips();
            InitializeContextMenu();

            InitializeCompareButtons();
        }

        private void InitializeCompareButtons()
        {
            _ugMapping.DisplayLayout.Bands[0].Columns["Compare"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.EditButton;
            _ugMapping.DisplayLayout.Override.ButtonStyle = UIElementButtonStyle.Office2007RibbonButton;
            _ugMapping.ClickCellButton += new CellEventHandler(ugMapping_ClickCellButton);
        }
        void ugMapping_ClickCellButton(object sender, CellEventArgs e)
        {
            ColumnConfig config = (ColumnConfig)e.Cell.Row.ListObject;

            string value = e.Cell.Value == null ? "" : e.Cell.Value.ToString();
            frmEditor editor = new frmEditor(config.InputColumnName, value, _IsagCustomProperties.GetInputColumns());                                                             

            if (editor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                e.Cell.Value = editor.Value;
        }

        private void InitializeCustomComponents()
        {
            pnlDGV.Controls.Add(_ugMapping);
            pnlNewColumns.Controls.Add(_ugNewColumns);
        }

        private void PopulateNewColumnsGrid()
        {
            _ugNewColumns.DataSource = _IsagCustomProperties.NewColumnConfigList;
            _ugNewColumns.DataBind();

            ValueList valueList = _ugNewColumns.DisplayLayout.ValueLists.Add("IS_Datatypes");

            ArrayList datatypes = new ArrayList();
            datatypes.AddRange(Constants.DATATYPE_SIMPLE);
            datatypes.AddRange(Constants.DATATYPE_LENGTH);
            datatypes.AddRange(Constants.DATATYPE_PRECISION_SCALE);
            datatypes.AddRange(Constants.DATATYPE_SCALE);
            datatypes.AddRange(Constants.DATATYPE_LENGTH_CODEPAGE);
            datatypes.AddRange(Constants.DATATYPE_CODEPAGE);
            datatypes.Sort();

            foreach (string dataType in datatypes)
            {
                valueList.ValueListItems.Add(dataType);
            }

            _ugNewColumns.DisplayLayout.Bands[0].Columns["DataType"].ValueList = valueList;
            _ugNewColumns.DisplayLayout.Bands[0].Columns["DataType"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;

            foreach (UltraGridRow row in _ugNewColumns.Rows)
            {
                SetNewColumnsGridDataCellActivation(row);
            }
        }

        private void PopulateMappingGrid()
        {

            idgvMapping.DataSource = _IsagCustomProperties.ColumnConfigList;
            idgvMapping.AddCellBoundedComboBox("DataType", Constants.DATATYPE_LIST());
       //     idgvMapping.AddCellBoundedComboBox("ErrorHandling", typeof(IsagCustomProperties.ErrorRowHandling));

            List<string> listConversion = Common.GetListFromEnum(typeof(DateConvertTypes));
            listConversion.AddRange(Constants.STRING_CONVERSION_TYPES);
            idgvMapping.AddCellBoundedComboBox("ConversionAsString", new BindingList<string>(listConversion));
            


            tbPrefix.DataBindings.Add("Text", _IsagCustomProperties, "AliasPrefix");

            _ugMapping.DataSource = _IsagCustomProperties.ColumnConfigList;
            _ugMapping.DataBind();

            foreach (UltraGridColumn col in _ugMapping.DisplayLayout.Bands[0].Columns)
            {
                col.Header.Column.SortIndicator = SortIndicator.Ascending;
            }

            ValueList valueList = _ugMapping.DisplayLayout.ValueLists.Add("IS_Datatypes");

            ArrayList datatypes = new ArrayList();
            datatypes.AddRange(Constants.DATATYPE_SIMPLE);
            datatypes.AddRange(Constants.DATATYPE_LENGTH);
            datatypes.AddRange(Constants.DATATYPE_PRECISION_SCALE);
            datatypes.AddRange(Constants.DATATYPE_SCALE);
            datatypes.AddRange(Constants.DATATYPE_LENGTH_CODEPAGE);
            datatypes.AddRange(Constants.DATATYPE_CODEPAGE);
            datatypes.Sort();

            foreach (string dataType in datatypes)
            {
                valueList.ValueListItems.Add(dataType);
            }

            _ugMapping.DisplayLayout.Bands[0].Columns["DataType"].ValueList = valueList;
            _ugMapping.DisplayLayout.Bands[0].Columns["DataType"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;

            foreach (UltraGridRow row in _ugMapping.Rows)
            {
                SetGridColumnsActivationState(row);
            }

            //ErrorHandling Column
            valueList = _ugMapping.DisplayLayout.ValueLists.Add("ErrorHandling");
            valueList.ValueListItems.Add(IsagCustomProperties.ErrorRowHandling.RedirectRow);
            valueList.ValueListItems.Add(IsagCustomProperties.ErrorRowHandling.FailComponent);
            valueList.ValueListItems.Add(IsagCustomProperties.ErrorRowHandling.IgnoreFailure);
            _ugMapping.DisplayLayout.Bands[0].Columns["ErrorHandling"].ValueList = valueList;
            _ugMapping.DisplayLayout.Bands[0].Columns["ErrorHandling"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;

            //Regular Expressions

            cmbRegEx.ValueList = RegularExpressions.LoadFromXml(Constants.PATH_REGEX).GetValueList();

            //conversion Column            
            SetConversionColumnState();
            _ugMapping.DisplayLayout.Bands[0].Columns["ConversionAsString"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;
        }

        private void PopulateComboVariables()
        {
            foreach (Variable variable in _variables)
            {
                cmbErrorName.ValueList.ValueListItems.Add(variable.QualifiedName);
            }

            cmbErrorName.SortStyle = ValueListSortStyle.AscendingByValue;

            cmbErrorName.Value = _IsagCustomProperties.ErrorName;
            //cmbErrorName.SetDataBinding(_IsagCustomProperties, "ErrorName");
            //cmbErrorName.DataBind();
        }

        private void SetToolTips()
        {
            _ugMapping.DisplayLayout.Bands[0].Columns["Default"].Header.ToolTipText = "Ist ein Wert Null, so wird der OnNull-Wert verwendet.";
            _ugMapping.DisplayLayout.Bands[0].Columns["OnErrorValue"].Header.ToolTipText =
                "Kann ein Wert nicht konvertiert werden,\nso wird der Wert durch den OnError Wert ersetzt.\nIst \"ReDirect\" gewählt, so wird die Zeile zusätlich an den Error Ouput gesendet.";
            _ugMapping.DisplayLayout.Bands[0].Columns["AllowNull"].Header.ToolTipText =
                "Ist \"AllowNull\" ausgewählt und ist der Input Wert Null, so wird wird Null an den Output weitergeleitet.";
            _ugMapping.DisplayLayout.Bands[0].Columns["IsErrorCounter"].Header.ToolTipText =
                "Eine Spalte die als ErrorCounter markiert ist wird niemals konvertiert.\nPro Zeile können mehrere Fehler auftreten, der ErrorCounter wird entsprechend erhöht.";
        }

        private void PopulateRowDisposition()
        {
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_FailComponent);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_IgnoreFailure);
            cbErrorHandling.Items.Add(DTSRowDisposition.RD_RedirectRow);

            cbErrorHandling.SelectedItem = _metadata.InputCollection[Constants.INPUT_NAME].ErrorRowDisposition;
        }

        private void SetOuputDataTypeAsInputDataType(UltraGridRow row)
        {
            IDTSInputColumn100 inputColumn = _inputColumnsDictionary[row.Cells["InputColumnName"].Value.ToString()];

            row.Cells["DataType"].Value = inputColumn.DataType.ToString();
            row.Cells["Length"].Value = inputColumn.Length;
            row.Cells["Precision"].Value = inputColumn.Precision;
            row.Cells["Scale"].Value = inputColumn.Scale;
            row.Cells["Codepage"].Value = inputColumn.CodePage;
        }

        private void DisableOutputColumnData(UltraGridRow row)
        {
            row.Cells["DataType"].Activation = Activation.ActivateOnly;

            row.Cells["Length"].Activation = Activation.ActivateOnly;
            row.Cells["Precision"].Activation = Activation.ActivateOnly;
            row.Cells["Scale"].Activation = Activation.ActivateOnly;
            row.Cells["Codepage"].Activation = Activation.ActivateOnly;
        }


        private void SetConversionColumnState(UltraGridRow row)
        {
            ColumnConfig column = _IsagCustomProperties.GetColumnConfigByInputColumnName(row.Cells["InputColumnName"].Value.ToString());
            row.Cells["ConversionAsString"].Activation = column.SupportsConversion ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["ConversionAsString"].ValueList = Common.CreateValueList(column.SupportedConversions);         
        }


        private void SetConversionColumnState()
        {
            foreach (UltraGridRow row in _ugMapping.Rows)
            {
                SetConversionColumnState(row);
            }
        }

        public void SetGridColumnsActivationState(UltraGridRow row)
        {

            if ((bool)row.Cells["IsErrorCounter"].Value)
            {
                row.Cells["Convert"].Activation = Activation.ActivateOnly;
                row.Cells["Default"].Activation = Activation.ActivateOnly;
                row.Cells["OnErrorValue"].Activation = Activation.ActivateOnly;
                row.Cells["RegEx"].Activation = Activation.ActivateOnly;
                row.Cells["AllowNull"].Activation = Activation.ActivateOnly;
                row.Cells["ErrorHandling"].Activation = Activation.ActivateOnly;

                DisableOutputColumnData(row);
                SetOuputDataTypeAsInputDataType(row);

                row.Cells["OutputAlias"].Activation = Activation.AllowEdit;
                row.Cells["IsErrorCounter"].Activation = Activation.AllowEdit;

                row.Update();
                return;
            }

            if (!(bool)row.Cells["Convert"].Value)
            {
                DisableOutputColumnData(row);
                SetOuputDataTypeAsInputDataType(row);

                row.Cells["Default"].Activation = Activation.ActivateOnly;
                row.Cells["RegEx"].Activation = Activation.ActivateOnly;
                row.Cells["OnErrorValue"].Activation = Activation.ActivateOnly;
                row.Cells["AllowNull"].Activation = Activation.ActivateOnly;
                row.Cells["ErrorHandling"].Activation = Activation.ActivateOnly;

                row.Cells["IsErrorCounter"].Activation = Activation.AllowEdit;
                row.Cells["Convert"].Activation = Activation.AllowEdit;
                row.Cells["OutputAlias"].Activation = Activation.AllowEdit;

                row.Update();
                return;
            }

            ColumnConfig config = _IsagCustomProperties.GetColumnConfigByInputColumnName(row.Cells["InputColumnName"].Value.ToString());

            row.Cells["Convert"].Activation = Activation.AllowEdit;
            row.Cells["DataType"].Activation = Activation.AllowEdit;
            row.Cells["Default"].Activation = Activation.AllowEdit;
            row.Cells["Length"].Activation = config.HasLength() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Precision"].Activation = config.HasPrecision() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Scale"].Activation = config.HasScale() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Codepage"].Activation = config.HasCodePage() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["RegEx"].Activation = Activation.AllowEdit;
            row.Cells["OnErrorValue"].Activation = Activation.AllowEdit;
            row.Cells["AllowNull"].Activation = Activation.AllowEdit;
            row.Cells["ErrorHandling"].Activation = Activation.AllowEdit;
            row.Cells["IsErrorCounter"].Activation = Activation.AllowEdit;
            row.Cells["OutputAlias"].Activation = Activation.AllowEdit;

            row.Update();
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
            _IsagCustomProperties.AddPrefix();
            _ugMapping.DataBind();
        }

        private void SelectCheckBoxes(bool select)
        {
            foreach (UltraGridCell cell in _ugMapping.Selected.Cells)
            {
                if (cell.Column.Header.Caption == "Convert" || cell.Column.Header.Caption == "Allow Null")
                    cell.Value = select;
            }

            foreach (UltraGridRow row in _ugMapping.Selected.Rows)
            {
                //row.Cells["Use"].Value = select;
                row.Cells["Convert"].Value = select;
                row.Cells["AllowNull"].Value = select;
            }

            foreach (Infragistics.Win.UltraWinGrid.ColumnHeader col in _ugMapping.Selected.Columns)
            {
                if (col.Caption == "Convert" || col.Caption == "Allow Null")
                {
                    foreach (UltraGridRow row in _ugMapping.Rows)
                    {
                        row.Cells[col.Column].Value = select;
                    }
                }
            }
        }

        private void ugMapping_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (e.Cell.Column.Key == "DataType" || e.Cell.Column.Key == "IsErrorCounter" || e.Cell.Column.Key == "Convert") SetGridColumnsActivationState(e.Cell.Row);
            else if (e.Cell.Column.Key == "ErrorHandling") cbErrorHandling.SelectedItem = _IsagCustomProperties.GetRowDisposition();
            //else if (e.Cell.Column.Key == "ConversionAsString") _ugMapping.DataBind();
            SetConversionColumnState(e.Cell.Row);
        }




        private void TestRegEx(UltraGridRow row)
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
            _ugMapping.DataBind();
        }

        private void btnCopyRegEx_Click(object sender, EventArgs e)
        {
            if (cmbRegEx.Value != null) System.Windows.Forms.Clipboard.SetDataObject(cmbRegEx.Value.ToString(), true);
        }

        #endregion

        #region ContextMenu
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
                default:
                    break;
            }
        }
        private void InitializeContextMenu()
        {

            _ugMapping.ContextMenu = new ContextMenu();
            _ugMapping.ContextMenu.MenuItems.Add(new MenuItem("Select", menuItem_Click));
            _ugMapping.ContextMenu.MenuItems.Add(new MenuItem("DeSelect", menuItem_Click));
            _ugMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));
            _ugMapping.ContextMenu.MenuItems.Add(new MenuItem("Apply Alias Prefix", menuItem_Click));
            _ugMapping.ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem item = new MenuItem("DebugMode", menuItem_Click);
            item.Checked = _IsagCustomProperties.DebugModus;

            _ugMapping.ContextMenu.MenuItems.Add(item);

        }

        #endregion

        private void cmbErrorName_ValueChanged(object sender, EventArgs e)
        {
            _IsagCustomProperties.ErrorName = cmbErrorName.Value.ToString();
        }


        #region NewColumnsConfig

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        private void btnRemoveRow_Click(object sender, EventArgs e)
        {
            RemoveRows();
        }

        private void AddRow()
        {
            NewColumnConfig config = new NewColumnConfig();

            int idx = 0;
            for (int i = 0; i < _IsagCustomProperties.NewColumnConfigList.Count; i++)
            {
                if (_IsagCustomProperties.NewColumnConfigList[i].OutputAlias == "NewColumn" + idx.ToString())
                {
                    i = -1;
                    idx++;
                }
            }

            config.OutputAlias = "NewColumn" + idx.ToString();

            _IsagCustomProperties.NewColumnConfigList.Add(config);
            _ugNewColumns.DataBind();
            SetNewColumnsGridDataCellActivation(_ugNewColumns.Rows[_ugNewColumns.Rows.Count - 1]);
        }

        private void RemoveRows()
        {
            _ugNewColumns.DeleteSelectedRows();
        }

        private void SetNewColumnsGridDataCellActivation(UltraGridRow row)
        {
            NewColumnConfig config = _IsagCustomProperties.GetNewColumnConfigByName(row.Cells["OutputAlias"].Value.ToString());

            row.Cells["Length"].Activation = config.HasLength() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Precision"].Activation = config.HasPrecision() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Scale"].Activation = config.HasScale() ? Activation.AllowEdit : Activation.ActivateOnly;
            row.Cells["Codepage"].Activation = config.HasCodePage() ? Activation.AllowEdit : Activation.ActivateOnly;
        }

        void _ugNewColumns_AfterCellUpdate(object sender, CellEventArgs e)
        {
            SetNewColumnsGridDataCellActivation(e.Cell.Row);
        }

        #endregion

        private void frmDataConverterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_abortClosing && this.DialogResult == System.Windows.Forms.DialogResult.OK &&
              Common.ShowMessage("Da beim Speichern ein Fehler aufgetreten ist, würden Änderungen beim Schließen des DataConverters verworfen werden. <br/><br/>" +
                          "Soll das Schließen des DataConverters abgebrochen werden?", "",
                          MessageBoxIcon.Question, MessageBoxButtons.YesNo, ultraMessageBox) == System.Windows.Forms.DialogResult.Yes)
                e.Cancel = _abortClosing;
        }
    }
}
