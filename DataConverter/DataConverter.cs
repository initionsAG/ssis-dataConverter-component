using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using DataConverter.FrameWork.Mapping;

namespace DataConverter {

#if     (SQL2008)
    [DtsPipelineComponent(DisplayName = "DataConverter",
    ComponentType = ComponentType.Transform,
    CurrentVersion = 1,
    IconResource = "DataConverter.Resources.DataConverter_DC.ico",
    UITypeName = "DataConverter.DataConverterUI, DataConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8a91e54220f9b6ce")]
#elif   (SQL2012)
    [DtsPipelineComponent(DisplayName = "DataConverter",
    ComponentType = ComponentType.Transform,
    CurrentVersion = 0,
    IconResource = "DataConverter.Resources.DataConverter_DC.ico",
    UITypeName = "DataConverter.DataConverterUI, DataConverter2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=611facfb07109fd4")]
#elif   (SQL2014)
    [DtsPipelineComponent(DisplayName = "DataConverter",
    ComponentType = ComponentType.Transform,
    CurrentVersion = 1,
    IconResource = "DataConverter.Resources.DataConverter_DC.ico",
    UITypeName = "DataConverter.DataConverterUI, DataConverter3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1e7bd12ce9d458f0")]
#else
    [DtsPipelineComponent(DisplayName = "DataConverter",
    ComponentType = ComponentType.Transform,
    CurrentVersion = 1,
    IconResource = "DataConverter.Resources.DataConverter_DC.ico",
    UITypeName = "DataConverter.DataConverterUI, DataConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8a91e54220f9b6ce")]
#endif
    /// <summary>
    /// the pipeline component DataConverter
    /// </summary>
    public class DataConverter: PipelineComponent {
        /// <summary>
        /// SSIS error output buffer
        /// </summary>
        private PipelineBuffer _outputErrorBuffer;
        /// <summary>
        /// SSIS log output buffer
        /// </summary>
        private PipelineBuffer _outputLogBuffer;
        /// <summary>
        /// SSIS output ID
        /// </summary>
        private int _outputID;
        /// <summary>
        /// SSIS dummy output ID
        /// </summary>
        private int _outputDummyID;
        /// <summary>
        /// SSIS error output ID
        /// </summary>
        private int _outputErrorID;
        /// <summary>
        /// SSIS log output ID
        /// </summary>
        private int _outputLogID;
        /// <summary>
        /// buffer mapping (conatins informations gathered in pre execute phase)
        /// </summary>
        private BufferMapping[] _bufferMappings;
        /// <summary>
        /// not used
        /// </summary>
        private NewColumnMapping[] _newColumnMappings;
        /// <summary>
        /// SSIS variable values that are written to the error output
        /// </summary>
        private VariableValues _varValues;
        /// <summary>
        /// Column Comparer
        /// </summary>
        private Comparer _comparer;

        /// <summary>
        /// If debug mode is set, additional info is written to log output
        /// </summary>
        private bool _debugMode;

        /// <summary>
        /// custom properties of this component
        /// </summary>
        private IsagCustomProperties _isagCustomProperties;

        #region Validate & Reinitialize

        /// <summary>
        /// Validates the component metadata
        /// </summary>
        /// <returns>Is component configuration valid?</returns>
        public override DTSValidationStatus Validate()
        {
            InitProperties();

            _isagCustomProperties.Save(ComponentMetaData);

            LineageMapping.UpdateInputIdProperties(ComponentMetaData, _isagCustomProperties);
            LineageMapping.AddOutputIdProperties(ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME], ComponentMetaData, _isagCustomProperties);
            LineageMapping.AddOutputIdProperties(ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME], ComponentMetaData, _isagCustomProperties);

            DTSValidationStatus status = base.Validate();
            if (status != DTSValidationStatus.VS_ISVALID)
                return status;

            if (!_isagCustomProperties.IsValid(ComponentMetaData))
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;

            if (!this.ComponentMetaData.AreInputColumnsValid)
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;

            return DTSValidationStatus.VS_ISVALID;
        }

        /// <summary>
        /// Reiniitalized the components metadata
        /// </summary>
        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            this.ComponentMetaData.RemoveInvalidInputColumns();

            InitProperties();
            _isagCustomProperties.FixError(ComponentMetaData);

            _isagCustomProperties.Save(ComponentMetaData);
        }




        #endregion

        #region DesignTime
        /// <summary>
        /// Provides the component properties
        /// </summary>
        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();

            //initProperties();
            _isagCustomProperties = new IsagCustomProperties();

            //Set metadata version to DLL-Version
            ComponentMetaDataTools.UpdateVersion(this, ComponentMetaData);

            //Clear out base implmentation
            this.ComponentMetaData.RuntimeConnectionCollection.RemoveAll();
            this.ComponentMetaData.InputCollection.RemoveAll();
            this.ComponentMetaData.OutputCollection.RemoveAll();
            this.RemoveAllInputsOutputsAndCustomProperties();

            ComponentMetaData.UsesDispositions = true;

            //Input
            IDTSInput100 input = this.ComponentMetaData.InputCollection.New();
            input.Name = Constants.INPUT_NAME;
            input.ErrorRowDisposition = DTSRowDisposition.RD_RedirectRow;

            //Output
            IDTSOutput100 output = this.ComponentMetaData.OutputCollection.New();
            output.Name = Constants.OUTPUT_NAME;
            output.SynchronousInputID = input.ID; //0 -> asynchron, ID Input -> synchron
            output.ExclusionGroup = 1;

            output = this.ComponentMetaData.OutputCollection.New();
            output.Name = Constants.OUTPUT_NAME + "dummy";
            output.SynchronousInputID = input.ID; //0 -> asynchron, ID Input -> synchron
            output.ExclusionGroup = 1;

            //Output LOG
            Constants.CreateStandardLogOutput(ComponentMetaData);

            ////Error Output
            Constants.CreateStandardErrorOutput(ComponentMetaData);

            //Custom Property: Version
            IDTSCustomProperty100 prop = ComponentMetaData.CustomPropertyCollection.New();
            prop.Name = Constants.PROP_VERSION;
            prop.Value = _isagCustomProperties.Version;

            //Custom Property: Configuration
            prop = ComponentMetaData.CustomPropertyCollection.New();
            prop.Name = Constants.PROP_CONFIG;
        }

        /// <summary>
        /// Remove all input columns and all output&log column that are mapped to input columns
        /// </summary>
        private void ResetCollections()
        {
            //remove all but 3 columns from logOutput (-> columns mapped to input columns are removed)
            IDTSOutputColumnCollection100 outputLogColumns = ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME].OutputColumnCollection;
            while (outputLogColumns.Count > 3)
                outputLogColumns.RemoveObjectByIndex(outputLogColumns.Count - 1);

            //remove all output columns that are mapped to intput columns
            IDTSOutput100 output = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];

            for (int i = output.OutputColumnCollection.Count - 1; i >= 0; i--)
            {
                IDTSOutputColumn100 col = output.OutputColumnCollection[i];

                //new outputcolumnd are not used
                if (!_isagCustomProperties.HasNewOutputColumn(col.LineageID.ToString()))
                {
                    output.OutputColumnCollection.RemoveObjectByID(col.ID);
                }
            }

            //ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME].OutputColumnCollection.RemoveAll();
            //remove all input columns
            ComponentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection.RemoveAll();
        }

        /// <summary>
        /// React if input path has been attached
        /// </summary>
        /// <param name="inputID">SSIS input ID</param>
        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);

            InitProperties();

            //Initialize IsagCustomProperties if column config list is empty
            //(so detaching and attaching an input path does not delete all connfingurations)
            if (_isagCustomProperties.ColumnConfigList.Count == 0)
            {
                ResetCollections();
                _isagCustomProperties.ColumnConfigList.Clear();

                IDTSOutput100 output = this.ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];
                IDTSOutput100 outputLog = this.ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];
                IDTSInput100 input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
                IDTSVirtualInput100 vInput = input.GetVirtualInput();

                _isagCustomProperties.AddColumnConfig(vInput, input, output, outputLog);

                _isagCustomProperties.Save(ComponentMetaData);
            }
        }

        /// <summary>
        /// Reacts if input path is detached
        /// </summary>
        /// <param name="inputID"></param>
        public override void OnInputPathDetached(int inputID)
        {
            base.OnInputPathDetached(inputID);

            //initProperties();
            //ResetCollections();
            //_IsagCustomProperties.ColumnConfigList.Clear();

            //_IsagCustomProperties.Save(ComponentMetaData);
        }

        #endregion

        #region RunTime

        /// <summary>
        /// Holds informations (logs, errors,...) for a SSIS buffer row
        /// </summary>
        private class BufferRow {
            /// <summary>
            /// error exceptions (as string) thrown while processing input data
            /// </summary>
            public string ErrorExceptions { get; set; }

            /// <summary>
            /// columns that causes error exceptions
            /// </summary>
            public string ErrorColumns { get; set; }

            /// <summary>
            /// log exceptions (as string) thrown while processing input data
            /// </summary>
            public string LogExceptions { get; set; }

            /// <summary>
            /// columns that causes log exceptions
            /// </summary>
            public string LogColumns { get; set; }

            /// <summary>
            /// Error count
            /// </summary>
            public int ErrorCount { get; set; }

            /// <summary>
            /// Has buffer row an error?
            /// </summary>
            public bool HasError { get; set; }

            /// <summary>
            /// Has buffer row a log entry that has to be sent to log output?
            /// </summary>
            public bool Log { get; set; }

            /// <summary>
            /// Error handling 
            /// </summary>
            public IsagCustomProperties.ErrorRowHandling ErrorHandling { get; set; }

            /// <summary>
            /// Has column list for log?
            /// </summary>
            public bool HasLog { get { return LogColumns != null && LogColumns != ""; } }

            /// <summary>
            /// Concatination of log and error exceptions
            /// </summary>
            public string AllExceptions
            {
                get
                {
                    if (!string.IsNullOrEmpty(ErrorExceptions) && !string.IsNullOrEmpty(LogExceptions))
                        return ErrorExceptions + ";" + LogExceptions;
                    else
                        return ErrorExceptions + LogExceptions;
                }
            }

            //Concatination of log and error columns
            public string AllColumns
            {
                get
                {
                    if (!string.IsNullOrEmpty(ErrorColumns) && !string.IsNullOrEmpty(LogColumns))
                        return ErrorColumns + ";" + LogColumns;
                    else
                        return ErrorColumns + LogColumns;
                }

            }

            /// <summary>
            /// constructor
            /// </summary>
            public BufferRow()
            {
                Reset();
            }

            /// <summary>
            /// Reset/Initialized properties
            /// </summary>
            public void Reset()
            {
                ErrorExceptions = "";
                ErrorColumns = "";
                LogExceptions = "";
                LogColumns = "";
                ErrorCount = 0;
                HasError = false;
                Log = false;
                ErrorHandling = IsagCustomProperties.ErrorRowHandling.IgnoreFailure;
            }

            /// <summary>
            /// Add error or log 
            /// </summary>
            /// <param name="errorHandling">error handling</param>
            /// <param name="hasError">add error?</param>
            /// <param name="log">add log?</param>
            /// <param name="ex">exception</param>
            /// <param name="columnName">column name that caused the exception</param>
            /// <param name="debugMode">Dataconverter debug moder enabled?</param>
            public void AddError(IsagCustomProperties.ErrorRowHandling errorHandling, bool hasError, bool log, Exception ex, string columnName, bool debugMode)
            {

                Log = Log || log;
                HasError = HasError || hasError;
                if (log || hasError)
                    ErrorCount++;

                if (hasError)
                {
                    if (ErrorExceptions != "")
                    {
                        ErrorExceptions += ";";
                        ErrorColumns += ";";
                    }

                    ErrorExceptions += ex.Message;
                    ErrorColumns += columnName;
                }
                else if (debugMode)
                {
                    if (LogExceptions != "")
                    {
                        LogExceptions += ";";
                        LogColumns += ";";
                    }

                    LogExceptions += ex.Message;
                    LogColumns += columnName;
                }

                if (errorHandling == IsagCustomProperties.ErrorRowHandling.RedirectRow)
                    ErrorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

            }


        }

        /// <summary>
        /// SSIS variable values that are written to the error output 
        /// </summary>
        private struct VariableValues {
            public string TaskID;
            public string TaskName;
            public string PackageID;
            public string PackageName;
            public string ExecutionID;
            public string Name;
        }

        /// <summary>
        /// information needed to process each cell of a buffer row
        /// (gathered in pre execute phase)
        /// </summary>
        private struct BufferMapping {
            /// <summary>
            /// buffer index of input column
            /// </summary>
            public int inputBufferIndex;

            /// <summary>
            /// buffer index of output column
            /// </summary>
            public int outputBufferIndex;
            
            /// <summary>
            /// buffer index of log output column
            /// </summary>
            public int outputLogBufferIndex;

            /// <summary>
            /// Is input and output datatype equal?
            /// </summary>
            public bool hasSameDataType;

            /// <summary>
            /// output column  datatype
            /// </summary>
            public DataType outputDataType;

            /// <summary>
            /// length of string
            /// </summary>
            public int lengthOfString;

            /// <summary>
            /// default value to replace null values
            /// </summary>
            public object onNullValue;

            /// <summary>
            /// Has OnNull value? (onNullValue is not empty or null)
            /// </summary>
            public bool hasOnNullValue;

            /// <summary>
            /// Are null values allowed?
            /// </summary>
            public bool AllowNull;

            /// <summary>
            /// default value to replace values that cannot be converted
            /// </summary>
            public object onErrorValue;

            /// <summary>
            /// Has OnErrorValue? 
            /// </summary>
            public bool hasOnErrorValue;

            /// <summary>
            /// Error Handling
            /// </summary>
            public IsagCustomProperties.ErrorRowHandling errorHandling;

            /// <summary>
            /// Convert column?
            /// </summary>
            public bool convert;

            /// <summary>
            /// Has regular expression?
            /// </summary>
            public bool hasRegEx;

            /// <summary>
            /// Regular expression
            /// </summary>
            public string RegEx;

            /// <summary>
            /// Conversion from int to date?
            /// </summary>
            public bool ConvertFromIntToDate;
            /// <summary>
            /// Conversion from date to int?
            /// </summary>
            public bool ConvertFromDateToInt;
            /// <summary>
            /// Conversion to string?
            /// </summary>
            public bool ConvertToString;
            /// <summary>
            /// Conversion to nummeric with conversion rules?
            /// </summary>
            public bool ConvertToNumericByUsingConversionRules;
            /// <summary>
            /// Conversion from string to numeric?
            /// </summary>
            public DateConvertTypes ConvertFromStringToNumericType;
            /// <summary>
            /// Conversion from date to string?
            /// </summary>
            public DateConvertTypes ConvertFromDateToStringType;
            /// <summary>
            /// Conversion from formatted string?
            /// </summary>
            public string ConvertFromStringFormat;
            /// <summary>
            /// Output column datatype characteristic for date
            /// </summary>
            public DataTypeKind OutputDataTypeKindForDate;

            /// <summary>
            /// Convert from string?
            /// </summary>
            public bool ConvertFromString;
            /// <summary>
            /// Convert from formatted string?
            /// </summary>
            public bool ConvertFromStringByFormat;
            /// <summary>
            /// column name
            /// </summary>
            public string ColumnName;
            /// <summary>
            /// Is column marked as an error counter?
            /// </summary>
            public bool isErrorCounter;

            /// <summary>
            /// Index of year in date string
            /// </summary>
            public int dateYYYYIndex;
            /// <summary>
            /// Index of month in date string
            /// </summary>
            public int dateMMIndex;
            /// <summary>
            /// Index od day in date string
            /// </summary>
            public int dateDDIndex;
            /// <summary>
            /// First index of splitter in string (i.e. 2 for DD.MM.YYYY)
            /// </summary>
            public int dateFirstSplitterIndex;
            /// <summary>
            /// Second index of splitter in string (i.e. 5 for DD.MM.YYYY)
            /// </summary>
            public int dateSecondSpitterIndex;
            /// <summary>
            /// First date spliter (i.e. "." in DD.MM.YYYY)
            /// </summary>
            public string dateFirstSplitter;
            /// <summary>
            /// Second date spliter (i.e. "." in DD.MM.YYYY)
            /// </summary>
            public string dateSecondSpitter;
        }

        //not used
        private struct NewColumnMapping {
            public int outputBufferIndex;
            public object value;
        }

        #region PreExecute

        /// <summary>
        /// PreExecute phase: Gather all needed informations
        /// </summary>
        public override void PreExecute()
        {
            base.PreExecute();

            InitProperties();

            _debugMode = _isagCustomProperties.DebugModus;

            Converter.USED_CULTURE = string.IsNullOrEmpty(_isagCustomProperties.Language) || _isagCustomProperties.Language.Trim() == ""?
               Converter.USED_CULTURE : new CultureInfo(_isagCustomProperties.Language); 

            InitVariables();

            IDTSInput100 input = ComponentMetaData.InputCollection[Constants.INPUT_NAME];
            IDTSOutput100 output = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];
            IDTSOutput100 outputError = ComponentMetaData.OutputCollection[Constants.OUTPUT_ERROR_NAME];
            IDTSOutput100 outputLog = ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];

            if (!outputError.IsAttached)
                Events.Fire(ComponentMetaData, Events.Type.Warning, "Error Output is missing.");
            if (!outputLog.IsAttached)
                Events.Fire(ComponentMetaData, Events.Type.Warning, "Log Output is missing.");

            InitBufferMapping(input, output, outputLog);
            InitNewColumnMapping(input, output);

            _outputID = output.ID;
            _outputDummyID = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME + "dummy"].ID;
            _outputErrorID = outputError.ID;
            _outputLogID = outputLog.ID;

        }

        /// <summary>
        /// Get variable values needed for error output 
        /// </summary>
        private void InitVariables()
        {
            try
            {
                _varValues = new VariableValues();
                _varValues.TaskID = ComponentMetaDataTools.GetValueFromVariable(this.VariableDispenser, "TaskID");
                _varValues.TaskName = ComponentMetaDataTools.GetValueFromVariable(this.VariableDispenser, "TaskName");
                _varValues.PackageID = ComponentMetaDataTools.GetValueFromVariable(this.VariableDispenser, "PackageID");
                _varValues.PackageName = ComponentMetaDataTools.GetValueFromVariable(this.VariableDispenser, "PackageName");
                _varValues.ExecutionID = ComponentMetaDataTools.GetValueFromVariable(this.VariableDispenser, "ExecutionInstanceGUID"); //ExecID
                try
                {
                    _varValues.Name = ComponentMetaDataTools.GetValueFromVariable(VariableDispenser, _isagCustomProperties.ErrorName);
                }
                catch (Exception)
                {
                    _varValues.Name = _isagCustomProperties.ErrorName;
                }
            }
            catch (Exception ex)
            {
                Events.Fire(ComponentMetaData, Events.Type.Error, "Reading variable values for error output caused an error: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Fills Buffermapping
        /// </summary>
        /// <param name="input">SSIS input</param>
        /// <param name="output">SSIS output</param>
        /// <param name="outputLog">SSIS log output</param>
        private void InitBufferMapping(IDTSInput100 input, IDTSOutput100 output, IDTSOutput100 outputLog)
        {
            bool needsComparer = _isagCustomProperties.HasCompareColumn();

            if (needsComparer)
                _comparer = new Comparer();
            List<BufferMapping> mappingList = new List<BufferMapping>();

            for (int i = 0; i < input.InputColumnCollection.Count; i++)
            {
                ColumnConfig config = _isagCustomProperties.GetColumnConfigByInputColumnName(input.InputColumnCollection[i].Name);

                BufferMapping bufferMapping = new BufferMapping();

                bufferMapping.inputBufferIndex =
                     BufferManager.FindColumnByLineageID(input.Buffer, input.InputColumnCollection[i].LineageID);

                if (outputLog.IsAttached)
                    bufferMapping.outputLogBufferIndex =
                        BufferManager.FindColumnByLineageID(
                            outputLog.Buffer,
                            outputLog.OutputColumnCollection[config.InputColumnName].LineageID
                        );
                else
                    bufferMapping.outputLogBufferIndex = -1;

                if (config.Convert)
                {
                    bufferMapping.outputBufferIndex =
                        BufferManager.FindColumnByLineageID(
                            input.Buffer,
                            output.OutputColumnCollection[config.OutputAlias].LineageID
                        );

                    //Are input and output datatypes equal?
                    bufferMapping.hasSameDataType = config.HasSameInputAndOutputDataType();

                    //get output datatype & convert default values to output datatype
                    try
                    {
                        bufferMapping.OutputDataTypeKindForDate = config.OutputDataTypeKindForDate;

                        DataType inputType = input.InputColumnCollection[i].DataType;
                        DataType outputType = output.OutputColumnCollection[config.OutputAlias].DataType;

                        bufferMapping.outputDataType = outputType;
                        bufferMapping.lengthOfString = output.OutputColumnCollection[config.OutputAlias].Length;

                        bufferMapping.ConvertFromString = (inputType == DataType.DT_STR || inputType == DataType.DT_WSTR);

                        bufferMapping.ConvertToNumericByUsingConversionRules = (
                            (outputType == DataType.DT_UI4 || outputType == DataType.DT_UI8 || outputType == DataType.DT_I4 || outputType == DataType.DT_I8 || outputType == DataType.DT_NUMERIC) &&
                            (inputType == DataType.DT_WSTR || inputType == DataType.DT_STR) &&
                            (config.Date2string == DateConvertTypes.Comma2Point || config.Date2string == DateConvertTypes.Point2Comma |
                             config.Date2string == DateConvertTypes.AmericanDecimal || config.Date2string == DateConvertTypes.GermanDecimal)
                        );

                        bufferMapping.ConvertFromStringToNumericType = bufferMapping.ConvertToNumericByUsingConversionRules ? config.Date2string : DateConvertTypes.None;

                        bufferMapping.ConvertFromIntToDate = (
                            (inputType == DataType.DT_UI4 || inputType == DataType.DT_UI8 || inputType == DataType.DT_I4 || inputType == DataType.DT_I8 || inputType == DataType.DT_NUMERIC) &&
                             (outputType == DataType.DT_DBTIMESTAMP || outputType == DataType.DT_DBDATE || outputType == DataType.DT_DATE)
                        );

                        bufferMapping.ConvertFromDateToInt = (
                            (inputType == DataType.DT_DBTIMESTAMP || inputType == DataType.DT_DBDATE || inputType == DataType.DT_DATE) &&
                            (outputType == DataType.DT_UI4 || outputType == DataType.DT_UI8 || outputType == DataType.DT_I4 || outputType == DataType.DT_I8 || outputType == DataType.DT_NUMERIC)
                        );

                        if (!bufferMapping.ConvertToNumericByUsingConversionRules &&
                             (inputType == DataType.DT_DBTIMESTAMP || inputType == DataType.DT_DBDATE || inputType == DataType.DT_DATE) &&
                             (outputType == DataType.DT_WSTR || outputType == DataType.DT_STR))
                            bufferMapping.ConvertFromDateToStringType = config.Date2string;
                        else
                            bufferMapping.ConvertFromDateToStringType = DateConvertTypes.None;

                        bufferMapping.ConvertFromStringByFormat = config.Date2string == DateConvertTypes.STR2YYYYMMDD;
                        bufferMapping.ConvertFromStringFormat = config.StrConversionByFormat;
                        bufferMapping.ConvertToString = (outputType == DataType.DT_WSTR || outputType == DataType.DT_STR);


                        if (bufferMapping.ConvertFromStringByFormat)
                        {
                            bufferMapping.dateYYYYIndex = bufferMapping.ConvertFromStringFormat.IndexOf("YYYY");
                            bufferMapping.dateMMIndex = bufferMapping.ConvertFromStringFormat.IndexOf("MM");
                            bufferMapping.dateDDIndex = bufferMapping.ConvertFromStringFormat.IndexOf("DD");

                            List<int> sortedIndex = new List<int>();
                            sortedIndex.Add(bufferMapping.dateYYYYIndex);
                            sortedIndex.Add(bufferMapping.dateMMIndex);
                            sortedIndex.Add(bufferMapping.dateDDIndex);
                            sortedIndex.Sort();
                            bufferMapping.dateFirstSplitterIndex = sortedIndex[1] - 1;
                            bufferMapping.dateSecondSpitterIndex = sortedIndex[2] - 1;
                            bufferMapping.dateFirstSplitter = bufferMapping.ConvertFromStringFormat.Substring(bufferMapping.dateFirstSplitterIndex, 1);
                            bufferMapping.dateSecondSpitter = bufferMapping.ConvertFromStringFormat.Substring(bufferMapping.dateSecondSpitterIndex, 1);
                        }

                        if (config.HasDefaultValue())
                        {
                            StatusConvert status = new StatusConvert();
                            bufferMapping.onNullValue = Converter.GetConvertedValue(config.Default, bufferMapping.outputDataType, ref status, false);
                            if (status.HasError)
                            {
                                Events.Fire(ComponentMetaData, Events.Type.Error, "Der OnNull Wert kann nicht in den Datentyp der Zielspalte konvertiert werden: " + status.ErrorMessage);
                                throw new Exception(status.ErrorMessage);
                            }
                        }

                        if (config.HasOnErrorValue())
                        {
                            StatusConvert status = new StatusConvert();
                            bufferMapping.onErrorValue = Converter.GetConvertedValue(config.OnErrorValue, bufferMapping.outputDataType, ref status, false);
                            if (status.HasError)
                            {
                                Events.Fire(ComponentMetaData, Events.Type.Error, "Der OnNull Wert kann nicht in den Datentyp der Zielspalte konvertiert werden: " + status.ErrorMessage);
                                throw new Exception(status.ErrorMessage);
                            }
                        }
                    }
                    catch (Exception) { } //output does not contain column 

                    bufferMapping.hasOnNullValue = config.HasDefaultValue();
                    bufferMapping.hasOnErrorValue = config.HasOnErrorValue();
                    bufferMapping.AllowNull = config.AllowNull;
                    bufferMapping.errorHandling = config.ErrorHandling;
                    bufferMapping.hasRegEx = config.HasRegEx();


                    if (bufferMapping.hasRegEx)
                        bufferMapping.RegEx = config.RegEx;

                    if (_comparer != null && config.HasCompare)
                        _comparer.AddCompare(config.Compare, config.InputColumnName);
                }

                bufferMapping.ColumnName = config.InputColumnName;
                bufferMapping.convert = config.Convert;
                bufferMapping.isErrorCounter = config.IsErrorCounter;

                mappingList.Add(bufferMapping);

                _bufferMappings = mappingList.ToArray();
            }
        }

        /// <summary>
        /// not used
        /// </summary>
        /// <param name="input">SSIS input</param>
        /// <param name="output">SSIS output</param>
        private void InitNewColumnMapping(IDTSInput100 input, IDTSOutput100 output)
        {
            _newColumnMappings = new NewColumnMapping[_isagCustomProperties.NewColumnConfigList.Count];



            //try
            //{
            //    for (int i = 0; i < _IsagCustomProperties.NewColumnConfigList.Count; i++)
            //    {
            //        NewColumnConfig config = _IsagCustomProperties.NewColumnConfigList[i];

            //        NewColumnMapping mapping = new NewColumnMapping();
            //        mapping.outputBufferIndex = BufferManager.FindColumnByLineageID(
            //                                        input.Buffer,
            //                                        output.OutputColumnCollection[config.OutputAlias].LineageID
            //                                    );

            //        StatusConvert status = new StatusConvert();
            //        mapping.value = Converter.GetConvertedValue(config.Default, output.OutputColumnCollection[config.OutputAlias].DataType, ref status, false);
            //        if (status.HasError) throw new Exception(status.ErrorMessage);
            //        _newColumnMappings[i] = mapping;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Events.Fire(ComponentMetaData, Events.Type.Error, "Cannot get the NewColumn Lineage ID or the Value is not valid: " + ex.Message);
            //    throw ex;
            //}

        }

        #endregion

        /// <summary>
        /// SSIS prime output phase:
        /// Get pipeline output buffers
        /// </summary>
        /// <param name="outputs">buffer count</param>
        /// <param name="outputIDs">output IDs</param>
        /// <param name="buffers">pipeline buffers</param>
        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            base.PrimeOutput(outputs, outputIDs, buffers);

            InitProperties();

            if (buffers.Length > 0)
            {
                for (int i = 0; i < outputs; i++)
                {
                    if (outputIDs[i] == _outputErrorID)
                        _outputErrorBuffer = buffers[i];
                    else if (outputIDs[i] == _outputLogID)
                        _outputLogBuffer = buffers[i];
                }
            }

            Events.Fire(ComponentMetaData, Events.Type.Information, "PrimeOutput finished: " + DateTime.Now.ToLongTimeString());
        }

        #region ProcessInput

        /// <summary>
        /// process input phase
        /// </summary>
        /// <param name="inputID">ID of the SSIS input</param>
        /// <param name="buffer">SSIS pipeline buffer</param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            base.ProcessInput(inputID, buffer);

            BufferRow row = new BufferRow();

            while (buffer.NextRow())
            {

                row.Reset();  //remove error messages from last buffer row
                if (_comparer != null)
                    _comparer.Reset(); //remove stored values from last buffer row


                try
                {
                    for (int colIdx = 0; colIdx < _bufferMappings.Length; colIdx++)
                    {
                        StatusConvert status = new StatusConvert();
                        BufferMapping config = _bufferMappings[colIdx]; //load current columns configuration

                        if (config.convert)
                        {
                            object value = GetBufferValue(buffer[config.inputBufferIndex]); //get cell value

                            //Use OnNull value 
                            if ((value == null || (config.ConvertFromIntToDate && value.ToString() == "0")) && config.hasOnNullValue)
                            {
                                ProcessInputOnNull(buffer, row, config);
                                if (_comparer != null)
                                    _comparer.SkipColumn(config.ColumnName);
                            }
                            else
                            {
                                if (value == null && config.AllowNull)
                                    buffer.SetNull(config.outputBufferIndex); //Null weiterleiten
                                else
                                    ProcessInputConvert(buffer, row, config, value, ref status, ref _comparer); //Konvertieren                             
                            }
                        }

                        if (_comparer != null)
                        {
                            if (config.convert)
                                _comparer.AddColumnValue(_bufferMappings[colIdx].ColumnName, buffer[config.inputBufferIndex], buffer[config.outputBufferIndex]);
                            else
                                _comparer.AddColumnValue(_bufferMappings[colIdx].ColumnName, buffer[config.inputBufferIndex]);
                        }
                    }

                    string errorMessages = "";
                    string errorColumns = "";
                    if (_comparer != null && !_comparer.IsValid(ref errorMessages, ref errorColumns))
                    {
                        row.AddError(IsagCustomProperties.ErrorRowHandling.RedirectRow, true, true, new Exception(errorMessages), errorColumns, _debugMode);
                    }

                    foreach (NewColumnMapping config in _newColumnMappings)
                    {
                        buffer[config.outputBufferIndex] = config.value;
                    }

                    if (row.Log && row.ErrorHandling == IsagCustomProperties.ErrorRowHandling.RedirectRow)
                        ProcessInputLog(row, buffer);
                    else
                        buffer.DirectRow(_outputID);
                }
                catch (Exception ex)
                {
                    Events.Fire(ComponentMetaData, Events.Type.Error, ex.Message);
                    throw ex;
                }
            }

            if (buffer.EndOfRowset)
            {
                if (_outputErrorBuffer != null)
                    _outputErrorBuffer.SetEndOfRowset();
                if (_outputLogBuffer != null)
                    _outputLogBuffer.SetEndOfRowset();
            }

            Events.Fire(ComponentMetaData, Events.Type.Information, "ProcessInput finished: " + DateTime.Now.ToLongTimeString());
        }

        /// <summary>
        /// Process input phase: Write data to log output
        /// </summary>
        /// <param name="row">bufferrow</param>
        /// <param name="buffer">pipelinebuffer</param>
        private void ProcessInputLog(BufferRow row, PipelineBuffer buffer)
        {
            if (_outputLogBuffer != null)
            {
                _outputLogBuffer.AddRow();

                for (int i = 0; i < _bufferMappings.Length; i++)
                {
                    BufferMapping config = _bufferMappings[i];

                    object value = GetBufferValue(buffer[_bufferMappings[i].inputBufferIndex]);
                    //buffer.GetInt32(_bufferMappings[i].inputBufferIndex);
                    //Spalten umleiten
                    if (_bufferMappings[i].isErrorCounter) //ErrorCounter berechnen, wenn nötig
                    {
                        try
                        {
                            value = int.Parse(value.ToString()) + row.ErrorCount;
                            buffer[config.inputBufferIndex] = value; //ErrorCount korrigieren
                        }
                        catch (Exception exErrorCounter)
                        {
                            Events.Fire(ComponentMetaData, Events.Type.Error, "Fehler beim Erhöhen des ErrorCounters: " + exErrorCounter.Message);
                            throw exErrorCounter;
                        }

                    }

                    if (config.outputLogBufferIndex != -1)
                    {
                        if (value == null)
                            _outputLogBuffer.SetNull(config.outputLogBufferIndex);
                        else
                            _outputLogBuffer[config.outputLogBufferIndex] = value;
                    }

                    _outputLogBuffer[0] = row.HasError ? 1 : 0; ///Fehlerart (0 = konnte korrigiert werden, 1 = keine Korrektur möglich)

                    //Log: Fehlerbeschreibung hinzufügen
                    if ((row.HasLog || row.HasError) && _debugMode)
                    {
                        _outputLogBuffer[1] = row.AllColumns;
                        _outputLogBuffer[2] = row.AllExceptions;
                    }

                }
            }

            if (row.HasError)
            {
                buffer.DirectRow(_outputDummyID);


                //ErrorOutput füllen
                if (_outputErrorBuffer != null)
                {
                    _outputErrorBuffer.AddRow();

                    _outputErrorBuffer[2] = _varValues.TaskID;
                    _outputErrorBuffer[3] = _varValues.TaskName;
                    _outputErrorBuffer[4] = _varValues.PackageID;
                    _outputErrorBuffer[5] = _varValues.PackageName;
                    _outputErrorBuffer[6] = _varValues.ExecutionID;
                    _outputErrorBuffer[7] = _varValues.Name;
                    _outputErrorBuffer[8] = row.ErrorColumns;
                    _outputErrorBuffer[9] = row.ErrorExceptions;
                }
                else
                    throw new Exception("Error Output is missing | ErrorColumns: " + row.ErrorColumns + " | Exceptions: " + row.ErrorExceptions);
            }
            else
                buffer.DirectRow(_outputID);
        }

        /// <summary>
        /// process input phase: 
        /// convert values if input and output column datatype are not equal
        /// throws exception if conversion fails
        /// </summary>
        /// <param name="buffer">pipelinebuffer</param>
        /// <param name="row">bufferrow</param>
        /// <param name="config"><buffermapping/param>
        /// <param name="value">value to convert</param>
        /// <param name="status">conversion status for current bufferrow</param>
        /// <param name="comparer"></param>
        private void ProcessInputConvert(PipelineBuffer buffer, BufferRow row, BufferMapping config, object value, ref StatusConvert status, ref Comparer comparer)
        {
            if (value == null && !config.AllowNull)
                status.SetError("Null values are not allowed.");

            if (!status.HasError && (!config.hasSameDataType || config.ConvertFromStringByFormat))
            {
                if (config.ConvertFromStringByFormat)
                    value = Converter.String2YearMonthDayByFormat(config.OutputDataTypeKindForDate, config.outputDataType, config.dateYYYYIndex,
                                                                  config.dateMMIndex, config.dateDDIndex, config.dateFirstSplitterIndex,
                                                                  config.dateSecondSpitterIndex, config.dateFirstSplitter, config.dateSecondSpitter,
                                                                  value, ref status);
                else if (config.ConvertFromIntToDate)
                    value = Converter.IntToDate(value, ref status);
                else if (config.ConvertFromDateToInt)
                    value = Converter.DateToInt(value, config.outputDataType, ref status);
                else if (config.ConvertFromDateToStringType != DateConvertTypes.None)
                    value = Converter.DateToString(value, config.ConvertFromDateToStringType, ref status);
                else if (config.ConvertToNumericByUsingConversionRules)
                    value = Converter.String2Numeric(value, config.ConvertFromStringToNumericType, config.outputDataType, ref status);
                else
                    value = Converter.GetConvertedValue(value, config.outputDataType, ref status, config.ConvertFromString);

                if ((config.outputDataType == DataType.DT_WSTR || config.outputDataType == DataType.DT_STR) && value != null && value.ToString().Length > config.lengthOfString)
                {
                    status.SetError(value.ToString() + " ist zu lang.");
                }
            }

            //check regular expression and write to output 
            if (!status.HasError)
            {
                if (config.hasRegEx && !RegularExpressions.IsMatch(value.ToString(), config.RegEx))
                    status.SetError(value.ToString() + " entspricht nicht der Regular Expression.");
                else
                    try
                    {
                        buffer[config.outputBufferIndex] = value;
                    }
                    catch (Exception)
                    {
                        status.SetError(value.ToString() + " kann konvertiert werden, ist aber nicht kompatibel mit dem SSIS Datentypen.");
                    }
                   
            }

            if (status.HasError)
            {
                if (_comparer != null)
                    _comparer.SkipColumn(config.ColumnName);

                if (config.hasOnErrorValue) //Use OnError value
                {
                    try
                    {
                        buffer[config.outputBufferIndex] = config.onErrorValue;
                        row.AddError(config.errorHandling, false, true, new Exception(status.ErrorMessage), config.ColumnName, _debugMode);
                    }
                    catch (Exception exOnError)
                    {
                        row.ErrorHandling = IsagCustomProperties.ErrorRowHandling.FailComponent;
                        Events.Fire(ComponentMetaData, Events.Type.Error, "Der OnValue Wert ist ungültig: " + exOnError.Message);
                        throw exOnError;
                    }
                }
                else //converion of buffer row failed
                {
                    if (config.errorHandling == IsagCustomProperties.ErrorRowHandling.IgnoreFailure)
                    {
                        row.ErrorHandling = IsagCustomProperties.ErrorRowHandling.FailComponent;
                        Events.Fire(ComponentMetaData, Events.Type.Error, "Fehler beim Konvertieren (IgnoreFailure wird ignoriert): " + status.ErrorMessage);
                        throw new Exception(status.ErrorMessage);
                    }
                    else if (config.errorHandling == IsagCustomProperties.ErrorRowHandling.FailComponent)
                    {
                        row.ErrorHandling = IsagCustomProperties.ErrorRowHandling.FailComponent;
                        Events.Fire(ComponentMetaData, Events.Type.Error, "Fehler beim Konvertieren: " + status.ErrorMessage);
                        throw new Exception(status.ErrorMessage);
                    }
                    else
                    {
                        row.AddError(config.errorHandling, true, true, new Exception(status.ErrorMessage), config.ColumnName, _debugMode);
                        status = new StatusConvert();
                    }

                }
            }
        }

        /// <summary>
        /// Process input phase:
        /// Use OnNullValue
        /// </summary>
        /// <param name="buffer">pipelinebuffer</param>
        /// <param name="row">buffer row</param>
        /// <param name="config">buffermapping</param>
        private void ProcessInputOnNull(PipelineBuffer buffer, BufferRow row, BufferMapping config)
        {
            try
            {
                buffer[config.outputBufferIndex] = config.onNullValue;
            }
            catch (Exception exOnNull)
            {
                row.ErrorHandling = IsagCustomProperties.ErrorRowHandling.FailComponent;
                Events.Fire(ComponentMetaData, Events.Type.Error, "Der OnNull Wert ist ungültig: " + exOnNull.Message);
                throw exOnNull;
            }
        }

        #endregion


        /// <summary>
        /// 
        ///     SSIS datatypes D_NTEXT, D_NVARCHARMAX, D_TEXT, D_XML are handled as BlobColumns. 
        ///     Those datatypes have to be converted to byte arrays
        /// </summary>
        /// <param name="bufferValue">buffer value</param>
        /// <returns>converted buffer value</returns>
        private object GetBufferValue(object bufferValue)
        {
            if (bufferValue != null && bufferValue.GetType() == typeof(BlobColumn))
            {
                BlobColumn bc = (BlobColumn) bufferValue;
                bufferValue = bc.GetBlobData(0, (int) bc.Length);
            }

            return bufferValue;
        }


        #endregion

        /// <summary>
        /// Initializes buffer value
        /// </summary>
        private void InitProperties()
        {
            object configuration = this.ComponentMetaData.CustomPropertyCollection[Constants.PROP_CONFIG].Value;
            if (configuration != null)
                _isagCustomProperties = IsagCustomProperties.Load(configuration);
            else
                _isagCustomProperties = new IsagCustomProperties();
        }


        #region PerfornUpgrade

        /// <summary>
        /// Upgrade von SSIS 2008 auf 2012/2014
        /// </summary>
        /// <param name="pipelineVersion">components pipeline verion</param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            try
            {
                if (LineageMapping.NeedsMapping())
                {
                    InitProperties();

                    foreach (ColumnConfig config in _isagCustomProperties.ColumnConfigList)
                    {
                        if (string.IsNullOrEmpty(config.CustomId))
                            config.CustomId = Guid.NewGuid().ToString();

                        AddInputColumnCustomProperty(config.InputColumnName, config.CustomId, LineageMapping.IdPropertyName);
                        if (config.Convert)
                            AddOutputColumnCustomProperty(config.OutputAlias, config.CustomId, LineageMapping.IdPropertyName, Constants.OUTPUT_NAME);
                        AddOutputColumnCustomProperty(config.InputColumnName, config.CustomId, LineageMapping.IdPropertyName, Constants.OUTPUT_LOG_NAME);
                    }

                    LineageMapping.UpdateInputIdProperties(this.ComponentMetaData, _isagCustomProperties);
                    _isagCustomProperties.Save(this.ComponentMetaData);
                }

                DtsPipelineComponentAttribute attr =
                    (DtsPipelineComponentAttribute) Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
                ComponentMetaData.Version = attr.CurrentVersion;
            }
            catch (Exception ex)
            {
                bool cancel = false;
                this.ComponentMetaData.FireError(0, "DataConverter Upgrade", ex.ToString(), "", 0, out cancel);
                throw (ex);
            }
        }


        /// <summary>
        ///  Adds a custom property to an output column and sets the value
        ///  (has no effect if custom property already exists)
        /// </summary>
        /// <param name="colName">The name of the input column</param>
        /// <param name="value">the value of the custom property</param>
        /// <param name="propertyName">the name of the custom property</param>
        private void AddOutputColumnCustomProperty(string colName, string value, string propertyName, string outputCollectionName)
        {
            IDTSOutputColumn100 outputCol = this.ComponentMetaData.OutputCollection[outputCollectionName].OutputColumnCollection[colName];
            AddCustomProperty(outputCol.CustomPropertyCollection, value, propertyName);
        }


        /// <summary>
        ///  Adds a custom property to an input column and sets the value
        ///  (has no effect if custom property already exists)
        /// </summary>
        /// <param name="colName">The name of the input column</param>
        /// <param name="value">the value of the custom property</param>
        /// <param name="propertyName">the name of the custom property</param>
        private void AddInputColumnCustomProperty(string colName, string value, string propertyName)
        {
            IDTSInputColumn100 inputCol = this.ComponentMetaData.InputCollection[0].InputColumnCollection[colName];
            AddCustomProperty(inputCol.CustomPropertyCollection, value, propertyName);
        }

        /// <summary>
        ///  Adds a custom property to a CustomPropertyCollection and sets the value
        ///  (has no effect if custom property already exists)
        /// </summary>
        /// <param name="propCollection">the CustomPropertyCollection</param>
        /// <param name="value">the value of the custom property</param>
        /// <param name="propertyName">the name of the custom property</param>
        private void AddCustomProperty(IDTSCustomPropertyCollection100 propCollection, string value, string propertyName)
        {
            IDTSCustomProperty100 prop = null;
            try
            {
                //do nothing if custom property exists:
                prop = propCollection[propertyName];
            }
            catch (Exception)
            {
                prop = propCollection.New();
                prop.Name = propertyName;
                prop.Value = value;
            }
        }

        #endregion

    }
}
