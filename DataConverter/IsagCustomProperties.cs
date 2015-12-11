using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using ComponentFramework;
using DataConverter.Framework;

namespace DataConverter
{
    /// <summary>
    /// custom properties for this component
    /// </summary>
    public class IsagCustomProperties: INotifyPropertyChanged
    {
        /// <summary>
        /// SSIS Error handling
        /// </summary>
        public enum ErrorRowHandling { RedirectRow, FailComponent, IgnoreFailure }

        /// <summary>
        /// List of CultureInfos (languages for conversions)
        /// </summary>
        public string[] LanguageItemList = new string[] { "", "de-de", "en-us" };

        /// <summary>
        /// Property changed event
        /// (implements Interface of INotifyPropertyChanged)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Announces that a properties value has changed
        /// </summary>
        /// <param name="info">property name</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }

        }

        #region Properties

        /// <summary>
        /// file version of the assembly
        /// </summary>
        [BrowsableAttribute(false)]
        public string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return fvi.FileVersion;
            }
        }

        /// <summary>
        /// List of column conversions
        /// </summary>
        public SortableBindingList<ColumnConfig> ColumnConfigList { get; set; }

        /// <summary>
        /// not used
        /// </summary>
        public List<NewColumnConfig> NewColumnConfigList { get; set; }

        /// <summary>
        /// output column name
        /// </summary>
        public string AliasPrefix { get; set; }

        /// <summary>
        /// Is debug modus enabled?
        /// (debug modus: data of log output contains more informations
        /// </summary>
        public bool DebugModus { get; set; }

        /// <summary>
        /// Error name that is written to the error output
        /// (if error name is a SSIS variable name the variables value be written to the error output)
        /// </summary>
        public string ErrorName { get; set; }

        /// <summary>
        /// Language (CultureInfo) used for conversions
        /// </summary>
        public string Language { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// (initializes column config list)
        /// </summary>
        public IsagCustomProperties()
        {
            this.ColumnConfigList = new SortableBindingList<ColumnConfig>();
        }

        /// <summary>
        /// construtcor
        /// (initializes column config list and alias prefix)
        /// </summary>
        /// <param name="aliasPrefix">alias prefix</param>
        public IsagCustomProperties(string aliasPrefix)
        {
            this.AliasPrefix = aliasPrefix;
            this.ColumnConfigList = new SortableBindingList<ColumnConfig>();
        }

        /// <summary>
        /// Erzeugt eine Instanz der Klasse IsagCustomProperties, sofern der Parameter configuration (sollte ein XML-String sein) 
        /// dieses erlaubt.
        /// </summary>
        /// <param name="configuration">XML String als object</param>
        /// <returns></returns>
        public static IsagCustomProperties Load(object configuration)
        {
            IsagCustomProperties customProperties = null;

            if (configuration != null && configuration.ToString() != "")
            {
                customProperties = IsagCustomProperties.LoadFromXml(configuration.ToString());
            }

            return customProperties;
        }
        #endregion


        #region NewColumnsConfig

        /// <summary>
        /// not used
        /// </summary>
        /// <param name="name">not used</param>
        /// <returns>not used</returns>
        public NewColumnConfig GetNewColumnConfigByName(string name)
        {
            foreach (NewColumnConfig config in NewColumnConfigList)
            {
                if (config.OutputAlias == name) return config;
            }

            return null;
        }

        #endregion

        #region ColumnConfig

        /// <summary>
        /// Adds a configuration to the column config list.
        /// </summary>
        /// <param name="configRow">column configuration</param>
        public void AddColumnConfig(ColumnConfig configRow)
        {
            this.ColumnConfigList.Add(configRow);
        }

        /// <summary>
        /// Creates a column configuration from each SSIS virtual input column and
        /// adds it to the column config list.
        /// </summary>
        /// <param name="vInput">SSIS virtual input</param>
        /// <param name="input">SSIS input</param>
        /// <param name="output">SSIS output</param>
        /// <param name="outputLog">SSIS log output</param>
        public void AddColumnConfig(IDTSVirtualInput100 vInput, IDTSInput100 input, IDTSOutput100 output, IDTSOutput100 outputLog)
        {
            for (int i = 0; i < vInput.VirtualInputColumnCollection.Count; i++)
            {
                AddColumnConfig(InsertColumn(vInput.VirtualInputColumnCollection[i], vInput, input, outputLog));
            }
        }

        /// <summary>
        /// Gets a column configuration from an input column name
        /// </summary>
        /// <param name="inputColumnName"></param>
        /// <returns>column configuration</returns>
        public ColumnConfig GetColumnConfigByInputColumnName(string inputColumnName)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.InputColumnName == inputColumnName) return config;
            }

            return null;
        }

        /// <summary>
        /// Gets a column configuration from an output column name
        /// </summary>
        /// <param name="outputColName"></param>
        /// <returns>column configuration</returns>
        public ColumnConfig GetColumnConfigByOutputAlias(string outputColName)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.OutputAlias == outputColName) return config;
            }

            return null;
        }

        /// <summary>
        /// Gets a column configuration from an output column lineageId
        /// </summary>
        /// <param name="outputLienageId">output column lineage id</param>
        /// <returns>column configuration</returns>
        public ColumnConfig GetColumnConfigByOutputLineageId(string outputLienageId)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.OutputLineageId == outputLienageId) return config;
            }

            return null;
        }

        /// <summary>
        /// Sets output column names (prefix + input column name)
        /// </summary>
        public void AddPrefix()
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.Convert) config.OutputAlias = AliasPrefix + config.InputColumnName;
            }            
        }

        #endregion

        #region Save & Load

        /// <summary>
        /// Saves this custom properties
        /// </summary>
        /// <param name="componentMetaData">the components metddata</param>
        public void Save(IDTSComponentMetaData100 componentMetaData)
        {
            componentMetaData.CustomPropertyCollection[Constants.PROP_CONFIG].Value = SaveToXml();
        }

        /// <summary>
        /// Saves this properties to an xml string
        /// </summary>
        /// <returns>xml string</returns>
        public string SaveToXml()
        {
            StringBuilder builder;
            XmlSerializer serializer;
            XmlWriter writer;
            XmlSerializerNamespaces namespaces;

            builder = new StringBuilder();
            serializer = new XmlSerializer(this.GetType());
            writer = XmlWriter.Create(builder);
            namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            serializer.Serialize(writer, this, namespaces);

            return builder.ToString().Replace("/>", "/>" + Environment.NewLine);
        }

        /// <summary>
        /// load this properties from an xml string
        /// </summary>
        /// <param name="xml">xml string</param>
        /// <returns>this properties (IsagCustomProperties)</returns>
        public static IsagCustomProperties LoadFromXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IsagCustomProperties));

            StringReader reader = new StringReader(xml);
            IsagCustomProperties result = (IsagCustomProperties)serializer.Deserialize(reader);

            return result;
        }



        #endregion

        #region Validate

        /// <summary>
        /// Is this component configuration valid?
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Is this component configuration valid?</returns>
        public bool IsValid(IDTSComponentMetaData100 componentMetaData)
        {
            IDTSInput100 input = componentMetaData.InputCollection[Constants.INPUT_NAME];
            IDTSVirtualInput100 vInput = input.GetVirtualInput();
            IDTSOutput100 output = componentMetaData.OutputCollection[Constants.OUTPUT_NAME];
            IDTSOutput100 outputLog;

            bool hastStandardLogOutput = Constants.HasStandardLogOutput(componentMetaData);
            bool hastStandardErrorOutput = Constants.HasStandardErrorOutput(componentMetaData);

            if (!hastStandardLogOutput)
            {
                Events.Fire(componentMetaData, Events.Type.Error, "The log ouput is invalid!");
                return false;
            }
            else outputLog = componentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];

            if (!hastStandardErrorOutput)
            {
                Events.Fire(componentMetaData, Events.Type.Error, "The error output is invalid!");
                return false;
            }

            return !ContainsColumnConfigWithoutInput(vInput, componentMetaData) & !ContainsInputWithoutColumnConfig(vInput, componentMetaData) &
                   !ContainsOutputWithoutColumnConfig(output, componentMetaData) & !ContainsColumnConfigWithoutOutput(output, componentMetaData) &
                   !ContainsLogOutputWithoutColumnConfig(outputLog, componentMetaData) & !ContainsColumnConfigWithoutLogOutput(outputLog, componentMetaData) &
                   AreColumnNamesAndDataTypesValid(input, componentMetaData) &
                   !ContainsWrongUsageType(vInput.VirtualInputColumnCollection, componentMetaData) &
                   hastStandardLogOutput & hastStandardErrorOutput &
                   IsOnErrorValid(componentMetaData) & IsOnNullValid(componentMetaData) &
                   IsCompareValid(componentMetaData) & IsStrConversionByFormatValid(componentMetaData);

        }


        /// <summary>
        /// Is configuration for DateConvertTypes.STR2YYYYMMDD valid?
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Is configuration for DateConvertTypes.STR2YYYYMMDD valid?</returns>
        private bool IsStrConversionByFormatValid(IDTSComponentMetaData100 componentMetaData)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.Date2string == DateConvertTypes.STR2YYYYMMDD)
                {
                    if (!Common.ContainsOnce(config.StrConversionByFormat, "YYYY") ||
                        !Common.ContainsOnce(config.StrConversionByFormat, "MM") ||
                        !Common.ContainsOnce(config.StrConversionByFormat, "DD"))
                    {
                        Events.Fire(componentMetaData, Events.Type.Error,
                                   string.Format("The conversion format {0} is invalid.", config.StrConversionByFormat));
                        return false;
                    }
                }
            }

            return true;
        }

        
        /// <summary>
        /// Ckecks if all compare expressions are correct.:
        /// 1. Does the compare expressions contain an invalid operator?
        /// 2. Does an expression contain an invalid column? 
        /// 3. Are all datatypes compareable?
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Are all compare expression valid?</returns>
        private bool IsCompareValid(IDTSComponentMetaData100 componentMetaData)
        {
            
            Comparer comparer = new Comparer();

            //Fill comparer
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare) comparer.AddCompare(config.Compare, config.InputColumnName);
            }

            //Does the compare expression contain an invalid operator?
            if (comparer.HasInvalidOp)
            {
                Events.Fire(componentMetaData, Events.Type.Error,
                                    string.Format("At least one compare expression contains an invalid operator."));
                return false;
            }

            //Does an expression contain an invalid column? 
            foreach (string col in comparer.UsedColumns)
            {
                if (GetColumnConfigByInputColumnName(col) == null)
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                string.Format("At least one compare expression contains the imvalid colum {0}. ", col));
                    return false;
                }
            }

            //Are all datatypes compareable?
            string errorMessage = "";
            if (!comparer.AreDataTypesComparable(this, ref errorMessage))
            {
                Events.Fire(componentMetaData, Events.Type.Error, errorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is OnError value valid?
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Is OnError value valid?</returns>
        private bool IsOnErrorValid(IDTSComponentMetaData100 componentMetaData)
        {
            string errorMessage;

            foreach (ColumnConfig config in ColumnConfigList)
            {
                IDTSOutputColumn100 col = ComponentMetaDataTools.GetOutputColumnByColumnName(config.OutputAlias, componentMetaData.OutputCollection[Constants.OUTPUT_NAME].OutputColumnCollection);
                if (col != null && //Dieses wird schon in der Methode ContainsColumnConfigWithoutOutput als Fehler zurückgegeben
                    config.HasOnErrorValue() && !ComponentMetaDataTools.CanConvertTo(config.OnErrorValue, col.DataType, col.Length, col.Scale, col.Precision, out errorMessage))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                string.Format("Spalte [input column={0}] contains an invalid OnError value: " + errorMessage, config.InputColumnName));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Is OnNull value valid?
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Is OnNull value valid?</returns>
        private bool IsOnNullValid(IDTSComponentMetaData100 componentMetaData)
        {
            string errorMessage;

            foreach (ColumnConfig config in ColumnConfigList)
            {
                IDTSOutputColumn100 col = ComponentMetaDataTools.GetOutputColumnByColumnName(config.OutputAlias, componentMetaData.OutputCollection[Constants.OUTPUT_NAME].OutputColumnCollection);
                if (col != null && //Dieses wird schon in der Methode ContainsColumnConfigWithoutOutput als Fehler zurückgegeben
                    config.HasDefaultValue() && !ComponentMetaDataTools.CanConvertTo(config.Default, col.DataType, col.Length, col.Scale, col.Precision, out errorMessage))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                string.Format("Spalte [input column={0}] contains an invalid OnNull value: " + errorMessage, config.InputColumnName));
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Are all column name and their datatypes valid?
        /// </summary>
        /// <param name="input">SSIS input</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Are all column name and their datatypes valid?</returns>
        private bool AreColumnNamesAndDataTypesValid(IDTSInput100 input, IDTSComponentMetaData100 componentMetaData)
        {
            try
            {
                foreach (ColumnConfig config in this.ColumnConfigList)
                {
                    IDTSInputColumn100 inputColumn = input.InputColumnCollection.GetInputColumnByLineageID(Int32.Parse(config.InputLineageId));

                    string inputDataType = config.CreateInputDataTypeString(inputColumn.DataType.ToString(), inputColumn.Length.ToString(),
                                                                            inputColumn.Precision.ToString(), inputColumn.Scale.ToString(),
                                                                            inputColumn.CodePage.ToString());
                    if (inputColumn.Name != config.InputColumnName || inputDataType != config.DataTypeInput)
                    {
                        Events.Fire(componentMetaData, Events.Type.Error,
                                                      "Im Mapping existiert mind. eine Spalte mit einem Namen, bzw.Datentyp, der von der Inputspalte abweicht!");

                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Events.Fire(componentMetaData, Events.Type.Error,
                            "The mapping contains at least one column with a name differing from the mapped input columns name!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Does the column configuration list contain a column that does not exist in the SSIS input column list?
        /// </summary>
        /// <param name="vInput">SSIS virtual input</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Does the column configuration list contain a column that does not exist in the SSIS input column list?</returns>
        private bool ContainsColumnConfigWithoutInput(IDTSVirtualInput100 vInput, IDTSComponentMetaData100 componentMetaData)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (!ComponentMetaDataTools.HasVirtualInputColumn(vInput, Int32.Parse(config.InputLineageId)))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                "Im Mapping existiert mind. eine Spalte ohne Bezug zu einer Inputspalte!");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the SSIS input column collection contain a column that is not part of the column configuration list?
        /// </summary>
        /// <param name="vInput">SSIS virtual input</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Does the SSIS input column collection contain a column that is not part of the column configuration list?</returns>
        private bool ContainsInputWithoutColumnConfig(IDTSVirtualInput100 vInput, IDTSComponentMetaData100 componentMetaData)
        {
            for (int i = 0; i < vInput.VirtualInputColumnCollection.Count; i++)
            {
                if (!HasVirtualInputColumn(vInput.VirtualInputColumnCollection[i].LineageID.ToString()))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                "Im Input existiert mind. eine Spalte, die keinen Bezug zum Mapping hat!");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the column configuration list contain a column that does not exist in the SSIS output column list?
        /// </summary>
        /// <param name="output">SSIS output</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns></returns>
        private bool ContainsColumnConfigWithoutOutput(IDTSOutput100 output, IDTSComponentMetaData100 componentMetaData)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                try
                {
                    if (config.Convert &&
                        output.OutputColumnCollection.GetOutputColumnByLineageID(Int32.Parse(config.OutputLineageId)) == null)
                    {
                        Events.Fire(componentMetaData, Events.Type.Error,
                                    "The mapping contains at least one column that is not contained in the SSIS output column list!");
                        return true;
                    }
                }
                catch (Exception)
                {
                    //GetOutputColumnByLineageID throws an exception if no output column with the given lineageId can be found
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the SSIS output column collection contain a column that is not part of the column configuration list?
        /// </summary>
        /// <param name="output">SSIS output</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns></returns>
        private bool ContainsOutputWithoutColumnConfig(IDTSOutput100 output, IDTSComponentMetaData100 componentMetaData)
        {
            for (int i = 0; i < output.OutputColumnCollection.Count; i++)
            {
                string lineageId = output.OutputColumnCollection[i].LineageID.ToString();

                if (!HasOutputColumn(lineageId) && !HasNewOutputColumn(lineageId))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                "Im Ouput existiert mind. eine Spalte, die keinen Bezug zum Mapping hat!");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the column configuration list contain a column that does not exist in the SSIS log output column list?
        /// </summary>
        /// <param name="logOutput">log output</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Does the column configuration list contain a column that does not exist in the SSIS log output column list?</returns>
        private bool ContainsColumnConfigWithoutLogOutput(IDTSOutput100 logOutput, IDTSComponentMetaData100 componentMetaData)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                try
                {
                    if (logOutput.OutputColumnCollection.GetOutputColumnByLineageID(Int32.Parse(config.OutputErrorLineageId)) == null)
                        throw new Exception();
                }
                catch (Exception)
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                    "Im Mapping existiert mind. eine Spalte ohne Bezug zu einer logOuput-Spalte!");
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        ///  Does the SSIS log output column collection contain a column that is not part of the column configuration list?
        /// </summary>
        /// <param name="logOutput">log output</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns> Does the SSIS log output column collection contain a column that is not part of the column configuration list?</returns>
        private bool ContainsLogOutputWithoutColumnConfig(IDTSOutput100 logOutput, IDTSComponentMetaData100 componentMetaData)
        {
            for (int i = 3; i < logOutput.OutputColumnCollection.Count; i++)
            {
                IDTSOutputColumn100 col = logOutput.OutputColumnCollection[i];
                if (!HasLogOutputColumn(col.LineageID.ToString()))
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                "Im LogOuput existiert mind. eine Spalte, die keinen Bezug zum Mapping hat!");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the input column collecction contains a column with an invalid usage type?
        /// </summary>
        /// <param name="vInputColumnCollection">virtual input collection</param>
        /// <param name="componentMetaData">the components metadata</param>
        /// <returns>Does the input column collecction contains a column with an invalid usage type?</returns>
        public bool ContainsWrongUsageType(IDTSVirtualInputColumnCollection100 vInputColumnCollection, IDTSComponentMetaData100 componentMetaData)
        {
            for (int i = 0; i < vInputColumnCollection.Count; i++)
            {
                ColumnConfig config = GetColumnConfigByInputColumnName(vInputColumnCollection[i].Name);

                if (config != null && config.IsErrorCounter && vInputColumnCollection[i].UsageType != DTSUsageType.UT_READWRITE)
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                "Der UsageType aller InputColumns, die als IsErrorCounter markiert sind, muss ReadWrite sein!");
                    return true;
                }
                else if (config != null && !config.IsErrorCounter && vInputColumnCollection[i].UsageType != DTSUsageType.UT_READONLY)
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                    "Der UsageType aller InputColumns, die nicht konvertiert werden, muss ReadOnly sein!");
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Helper

        /// <summary>
        /// Does the column config list contain a configuration with the specified error output column lineageId?
        /// </summary>
        /// <param name="lineageId">the lineageId to search for</param>
        /// <returns> Does the column config list contain a configuration with the specified error output column lineageId?</returns>
        public bool HasLogOutputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.OutputErrorLineageId == lineageId) return true;
            }

            return false;
        }

        /// <summary>
        ///  Does the column config list contain a configuration with the specified output column lineageId?
        /// </summary>
        /// <param name="lineageId">the lineageId to search for</param>
        /// <returns>Does the column config list contain a configuration with the specified output column lineageId??</returns>
        public bool HasOutputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.Convert && config.OutputLineageId == lineageId) return true;
            }

            return false;
        }

        /// <summary>
        /// Does the column config list contain at least one configuration with a compare expression?
        /// </summary>
        /// <returns>Does the column config list contain at least one configuration with a compare expression?</returns>
        public bool HasCompareColumn()
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare) return true;
            }

            return false;
        }

        /// <summary>
        /// Does the column configuration list contain a configuration with the specified input column lineageId?
        /// </summary>
        /// <param name="lineageId">the lineageId to search for</param>
        /// <returns></returns>
        private bool HasVirtualInputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.InputLineageId == lineageId) return true;
            }

            return false;
        }

        /// <summary>
        /// New output columns are not used
        /// </summary>
        /// <param name="lineageId">not used</param>
        /// <returns>not used</returns>
        public bool HasNewOutputColumn(string lineageId)
        {
            foreach (NewColumnConfig config in this.NewColumnConfigList)
            {
                if (config.OutputLineageId == lineageId) return true;
            }

            return false;

        }

        /// <summary>
        /// Gets the configured row disposition
        /// </summary>
        /// <returns>the configured row disposition</returns>
        public DTSRowDisposition GetRowDisposition()
        {
            if (ColumnConfigList.Count == 0) return DTSRowDisposition.RD_RedirectRow;

            ErrorRowHandling errorHandling = ErrorRowHandling.IgnoreFailure;

            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.ErrorHandling == ErrorRowHandling.RedirectRow) return DTSRowDisposition.RD_RedirectRow;
                else if (config.ErrorHandling == ErrorRowHandling.FailComponent) errorHandling = ErrorRowHandling.FailComponent;
            }

            if (errorHandling == ErrorRowHandling.IgnoreFailure) return DTSRowDisposition.RD_IgnoreFailure;
            else return DTSRowDisposition.RD_FailComponent;

        }

        /// <summary>
        /// Sets the row disposition
        /// </summary>
        /// <param name="rowDisposition">row disposition</param>
        public void SetRowDisposition(DTSRowDisposition rowDisposition)
        {
            ErrorRowHandling errorHandling = ErrorRowHandling.RedirectRow;

            switch (rowDisposition)
            {
                case DTSRowDisposition.RD_FailComponent:
                    errorHandling = ErrorRowHandling.FailComponent;
                    break;
                case DTSRowDisposition.RD_IgnoreFailure:
                    errorHandling = ErrorRowHandling.IgnoreFailure;
                    break;
                case DTSRowDisposition.RD_RedirectRow:
                    errorHandling = ErrorRowHandling.RedirectRow;
                    break;
                default:
                    break;
            }

            foreach (ColumnConfig config in ColumnConfigList)
            {
                config.ErrorHandling = errorHandling;
            }

        }

        /// <summary>
        /// Gets an array of all input column names
        /// </summary>
        /// <returns>array of all input column names</returns>
        public string[] GetInputColumns()
        {
            List<string> result = new List<string>();

            foreach (ColumnConfig config in ColumnConfigList)
            {
                result.Add(config.InputColumnName);
            }

            return result.ToArray();
        }
        #endregion

        #region Rebuild Configuration


        /// <summary>
        /// Tries to fix metadata errors
        /// </summary>
        /// <param name="componentMetaData">the components metadata</param>
        public void FixError(IDTSComponentMetaData100 componentMetaData)
        {
            IDTSInput100 input = componentMetaData.InputCollection[Constants.INPUT_NAME];
            IDTSVirtualInput100 vInput = input.GetVirtualInput();

            IDTSOutput100 output = componentMetaData.OutputCollection[Constants.OUTPUT_NAME];
            IDTSOutput100 outputLog;


            //OuputLog neu generieren falls nötig
            if (!Constants.HasStandardLogOutput(componentMetaData)) outputLog = Constants.CreateStandardLogOutput(componentMetaData);
            else outputLog = componentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];

            //OuputError neu generieren falls nötig
            if (!Constants.HasStandardErrorOutput(componentMetaData)) Constants.CreateStandardErrorOutput(componentMetaData);



            //Compare aufbauen
            Comparer compare = new Comparer();
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare) compare.AddCompare(config.Compare, config.InputColumnName);
            }

            //Hash aus ColumnName, ColumnConfig aufbauen (nur Spalten die für Compare benötigt werden)
            Dictionary<string, ColumnConfig> compareColumns = new Dictionary<string, ColumnConfig>();
            foreach (string column in compare.UsedColumns)
            {
                if (!compareColumns.ContainsKey(column))
                {
                    ColumnConfig config = GetColumnConfigByInputColumnName(column);
                    if (config != null) compareColumns.Add(column, config);
                }
            }

            //Config neu aufbauen
            for (int i = ColumnConfigList.Count - 1; i >= 0; i--)
            {
                ColumnConfig config = ColumnConfigList[i];

                //Im Input nicht vorhanden? -> Löschen
                if (!ComponentMetaDataTools.HasVirtualInputColumn(vInput, Int32.Parse(config.InputLineageId)))
                {
                    if (config.Convert && !string.IsNullOrEmpty(config.OutputLineageId))
                    {
                        output.OutputColumnCollection.RemoveObjectByID(
                            output.OutputColumnCollection.GetOutputColumnByLineageID(Convert.ToInt32(config.OutputLineageId)).ID
                        );
                    }

                    try
                    {
                        outputLog.OutputColumnCollection.RemoveObjectByID(
                            outputLog.OutputColumnCollection.GetOutputColumnByLineageID(Convert.ToInt32(config.OutputErrorLineageId)).ID
                        );
                    }
                    catch (Exception)
                    {
                        //war eh nicht mehr vorhanden                        
                    }

                    ColumnConfigList.Remove(config);
                }
                else
                {
                    IDTSVirtualInputColumn100 vCol = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(Int32.Parse(config.InputLineageId));

                    if (config.Convert && !string.IsNullOrEmpty(config.OutputLineageId))
                    {
                        IDTSOutputColumn100 outCol = output.OutputColumnCollection.GetOutputColumnByLineageID(Convert.ToInt32(config.OutputLineageId));
                        IDTSOutputColumn100 outLogCol = outputLog.OutputColumnCollection.GetOutputColumnByLineageID(Convert.ToInt32(config.OutputErrorLineageId));

                        outCol.SetDataTypeProperties(vCol.DataType, vCol.Length, vCol.Precision, vCol.Scale, vCol.CodePage);
                        outLogCol.SetDataTypeProperties(vCol.DataType, vCol.Length, vCol.Precision, vCol.Scale, vCol.CodePage);

                        outLogCol.Name = vCol.Name;
                    }

                    if (!config.Convert)
                    {
                        config.DataType = vCol.DataType.ToString();
                        config.Precision = vCol.Precision.ToString();
                        config.Length = vCol.Length.ToString();
                        config.Codepage = vCol.CodePage.ToString();
                        config.Scale = vCol.Scale.ToString();
                    }
                    config.DataTypeInput = config.CreateInputDataTypeString(vCol.DataType.ToString(), vCol.Length.ToString(),
                                                                      vCol.Precision.ToString(), vCol.Scale.ToString(),
                                                                      vCol.CodePage.ToString());
                    config.InputColumnName = vCol.Name;

                }
            }


            // ColumnCompare bzgl. geänderter Spaltennamen korrigieren
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare)
                {
                    ColumnCompare columnCompare = compare.CreateColumnCompare(config.Compare, config.InputColumnName);

                    if (compareColumns.ContainsKey(columnCompare.Column1) && compareColumns[columnCompare.Column1].InputColumnName != columnCompare.Column1)
                    {
                        columnCompare.Column1 = compareColumns[columnCompare.Column1].InputColumnName;
                    }

                    if (compareColumns.ContainsKey(columnCompare.Column2) && compareColumns[columnCompare.Column2].InputColumnName != columnCompare.Column2)
                    {
                        columnCompare.Column2 = compareColumns[columnCompare.Column2].InputColumnName;
                    }

                    config.Compare = columnCompare.Column1 + " " + columnCompare.Operator + " " + columnCompare.Column2;
                }
            }

            //OutputColumns nicht als Convert markiert? -->Entfernen
            for (int i = output.OutputColumnCollection.Count - 1; i >= 0; i--)
            {
                IDTSOutputColumn100 outCol = output.OutputColumnCollection[i];
                ColumnConfig config = GetColumnConfigByOutputLineageId(outCol.LineageID.ToString());

                if (config == null || !config.Convert)
                {
                    output.OutputColumnCollection.RemoveObjectByIndex(i);
                }

            }

            //UsageType setzen
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.IsErrorCounter) vInput.SetUsageType(Convert.ToInt32(config.InputLineageId), DTSUsageType.UT_READWRITE);
                else vInput.SetUsageType(Convert.ToInt32(config.InputLineageId), DTSUsageType.UT_READONLY);
            }


            //Log OutputColumns: Umbenennung von InputColumns erkennen und OutputLog entsprechend korrigieren:
            for (int i = outputLog.OutputColumnCollection.Count - 1; i >= 0; i--)
            {
                IDTSOutputColumn100 col = outputLog.OutputColumnCollection[i];

                foreach (ColumnConfig config in this.ColumnConfigList)
                {
                    if (config.OutputErrorLineageId == col.LineageID.ToString() && config.InputColumnName != col.Name) col.Name = config.InputColumnName;
                }

            }


            //Neue Spalten zum OutputLog hinzufügen
            for (int i = 0; i < vInput.VirtualInputColumnCollection.Count; i++)
            {
                IDTSVirtualInputColumn100 vCol = vInput.VirtualInputColumnCollection[i];

                try
                {
                    IDTSOutputColumn100 outCol = outputLog.OutputColumnCollection[vCol.Name];
                    outCol.SetDataTypeProperties(vCol.DataType, vCol.Length, vCol.Precision, vCol.Scale, vCol.CodePage);
                }
                catch (Exception)
                {
                    AddColumnConfig(InsertColumn(vCol, vInput, input, outputLog));
                }
            }
        }

        /// <summary>
        /// Creates a new column configuration, adds a column to the logout and sets the input column usage type
        /// </summary>
        /// <param name="vColumn">SSIS virtual input column</param>
        /// <param name="vInput">SSIS virtual input</param>
        /// <param name="input">input</param>
        /// <param name="outputLog">log output</param>
        /// <returns>a new column configuration</returns>
        public ColumnConfig InsertColumn(IDTSVirtualInputColumn100 vColumn, IDTSVirtualInput100 vInput,
                                                           IDTSInput100 input, IDTSOutput100 outputLog)
        {

            //Sets the input columns usage type
            vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_READONLY);
            IDTSInputColumn100 inputColumn = ComponentMetaDataTools.GetInputColumnByLineageId(input.InputColumnCollection, vColumn.LineageID);

            //Adds new column to the log output
            IDTSOutputColumn100 outLogCol = outputLog.OutputColumnCollection.New();
            outLogCol.Name = vColumn.Name;
            outLogCol.SetDataTypeProperties(vColumn.DataType, vColumn.Length, vColumn.Precision, vColumn.Scale, vColumn.CodePage);

            //creates a column configuration
            ColumnConfig config = new ColumnConfig(vColumn.Name, false, "", vColumn.DataType.ToString(), vColumn.DataType.ToString(),
                                                   vColumn.Length.ToString(), vColumn.Precision.ToString(), vColumn.Scale.ToString(), vColumn.CodePage.ToString(),
                                                   Constants.PREFIX_OUTPUT_COL_NAME_DEFAULT,
                                                   inputColumn.ID.ToString(), inputColumn.IdentificationString, inputColumn.LineageID.ToString(),
                                                   "", "", "",
                                                   outLogCol.ID.ToString(), outLogCol.IdentificationString, outLogCol.LineageID.ToString(), true,
                                                   inputColumn, null, outLogCol);

            return config;
        }

        #endregion
    }
}
