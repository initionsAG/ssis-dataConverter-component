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
using DataConverter.ComponentFrameWork.Mapping;

namespace DataConverter
{
    /// <summary>
    ///  04.04.2011, Dennis Weise
    ///     - Version 1.0.0.0 [Von ConvertAll 1.0.0.14 abgeleitet]
    ///  04.04.2011, Dennis Weise
    ///     - Icon geändert
    ///     - Version 1.0.0.0
    ///  04.04.2011, Dennis Weise
    ///     - Setup bzgl. Icon geändert
    ///     - Version 1.0.0.0
    ///  05.04.2011, Dennis Weise
    ///     - Icon geändert
    ///  06.04.2011, Dennis Weise
    ///     - Icon geändert
    ///     - Die GUI öffnet sich nun schneller
    ///  06.04.2011, Dennis Weise
    ///     - Version 1.0.1
    ///  06.04.2011, Dennis Weise
    ///     - Scrollbar aktualisiert Grid schon beim Scrollen
    ///  06.04.2011, Dennis Weise
    ///     - Version 1.0.2
    ///  07.04.2011, Dennis Weise
    ///     - Grid Column "ErrorCounter" in "IsErrorCounter" umbenannt.
    ///     - Grid-Verhalten überarbeitet
    ///  20.04.2011, Dennis Weise
    ///     - Dem Konfigurations-String wurden Zeilenumbrüche nach jeder Spalte hinzugefügt
    ///  20.04.2011, Dennis Weise
    ///     - Version 1.0.3
    ///  05.05.2011. Dennis Weise
    ///     - Es können nun neue Spalten angelegt werden
    ///  05.05.2011, Dennis Weise
    ///     - Version 1.0.4
    ///  21.06.2011, Dennis Weise
    ///     - Fehlermeldung bei fehlendem ErrorOutput ist nun aussagekräftiger
    ///     - Spaltennamen des LOG Outputs mit isETL-Präfix erweitert
    ///  21.06.2011, Dennis Weise
    ///     - Version 1.0.5
    ///  23.06.2011, Dennis Weise
    ///     - Im ErrorOuput werden nur noch die Columnnames und Exceptions der Fehler geloggt
    ///     - Spaltennamen des Error Outputs mit isETL-Präfix erweitert 
    ///  23.06.2011, Dennis Weise
    ///     - Version 1.0.6
    ///  20.07.2011, Dennis Weise
    ///     - Ist nur Use angeklickt und ist der Input Wert Null, so wird nun explizit Null in den Output geschrieben
    ///  20.07.2011, Dennis Weise
    ///     - Version 1.0.7
    ///  01.09.2011, Dennis Weise
    ///     - Konvertierung von DateTime nach Int32/Int64 (unsigned & signed) und umgekehrt erfolgt nach DateTime Int-Format "yyyymmtt"
    ///  01.09.2011, Dennis Weise
    ///     - Version 1.0.8
    ///  19.10.2011, Dennis Weise
    ///     - Der Dataconverter ist nun eine synchrone Komponente, es werden alle Daten durchgereicht. Bei Konvertierung wird der konvertierte Wert in eine neue Spalte geschrieben
    ///  19.10.2011, Dennis Weise
    ///     - Version 1.0.9
    ///  22.11.2011, Dennis Weise
    ///     - Wird eine InputColumn umbenannt, so wird der Spaltenname auch im OuputLog übernommen
    ///  22.11.2011, Dennis Weise
    ///     - Version 1.0.10
    ///  25.11.2011, Dennis Weise
    ///     - ErrorColumns/Messages werden wieder in den Error- und LogOutput geschrieben
    ///     - OnError wird wieder berücksichtigt
    ///  12.01.2012, Dennis Weise
    ///     - Version 1.0.11
    ///  30.01.2012, Dennis Weise
    ///     - IsErrorCount: Funktionierte nur mit Integer
    ///  16.02.2012, Dennis Weise
    ///     - Wurde in ein DT_I4/DT_I8 konvertiert, so schlug eine Konvertierung bei negativen Werten fehl.
    ///     - Version 1.0.12
    ///  07.06.2012, Dennis Weise
    ///     Optional kann ein Datum in ein String mit dem Format YYYY, YYYYMM oder YYYYMMDD konvertiert werden
    ///  07.06.2012, Dennis Weise  
    ///     Version 1.0.13
    ///  11.06.2012, Dennis Weise
    ///     Umbenennung Spaltenüberschrift von "Date->Str“ nach "Conversion"
    ///  11.06.2012, Dennis Weise  
    ///     Version 1.0.14 
    ///  15.06.2012, Dennis Weise  
    ///     Optionanl kann ein string unter Anwendung einer Regel (Point2Comma, Comma2Point, AmericanDecimal, GermanDecimal) in ein Integer/Numeric konvertiert werden
    ///  15.06.2012, Dennis Weise  
    ///     Version 1.0.15
    ///  13.07.2012, Dennis Weise  
    ///     - Konvertierung "AmericanDecimal" bedeutet nun, das von AmericanDecimal nach GermanDecimal konvertiert wird
    ///  13.07.2012, Dennis Weise  
    ///     Version 1.0.16
    ///  10.08.2012, Dennis Weise  
    ///     - Der DC funktioniert nun auch ohne Log- und/oder Error-Output, wenn RunInOptimized Mode = True
    ///  10.08.2012, Dennis Weise  
    ///     Version 1.0.17
    ///  17.08.2012, Dennis Weise 
    ///     - Bei den Konvertierungsoptionen Poit2Comma/Comma2Point werden die Nachkommastellen bei der Konvertierung in ein Integer abgeschnitten
    ///  17.08.2012, Dennis Weise  
    ///     Version 1.0.18
    ///  23.08.2012, Dennis Weise 
    ///     - Klasse Constants ist nun public (für ComponentUpdater)
    ///  23.08.2012, Dennis Weise  
    ///     - Version 1.0.19
    ///  21.09.2012, Dennis Weise
    ///     - Beim Konvertieren von einem Datum nach DT_NUMERIC war das Ergebnis immer 0
    ///  21.09.2012, Dennis Weise  
    ///     - Version 1.0.20
    ///  19.10.2012, Dennis Weise  
    ///     - Spalte "USE" entfernt
    ///  19.10.2012, Dennis Weise  
    ///     - Spalte "Conversion": Nur in Bezug auf Input- und Outputdatentyp gültige Werte können ausgewählt werden
    ///  19.10.2012, Dennis Weise  
    ///     - Der DC verliert seine Einstellungen nicht mehr, wenn der InputPath entfernt wird
    ///  19.10.2012, Dennis Weise  
    ///     - Version 1.0.21
    ///  02.11.2012, Dennis Weise
    ///     - Performance Verbesserung in Bezug auf Konvertierungsfehler
    ///  16.11.2012, Dennis Weise
    ///     - Performance Verbesserung in Bezug auf Konvertierungsfehler
    ///     - Treten beim Speichern in der GUI Fehler auf, so wird der Nutzer darauf hingewiesen
    ///  16.11.2012, Dennis Weise  
    ///     - Version 1.0.22     
    ///  23.11.2012, Dennis Weise
    ///     - Korrektur zu: Performance Verbesserung in Bezug auf Konvertierungsfehler 
    ///  23.11.2012, Dennis Weise  
    ///     - Version 1.0.23  
    ///  30.11.2012, Dennis Weise 
    ///     - Ist kein Error-Output vorhanden und kann ein Datensatz zur Laufzeit nicht in den Standard-Output geschrieben werden, so bricht der DC mit einem Fehler ab.
    ///  30.11.2012, Dennis Weise  
    ///     - Version 1.0.24  
    ///  31.05.2013, Dennis Weise 
    ///     - Unterstützung von SSIS 2012 vorbereitet
    ///  31.05.2013, Dennis Weise
    ///     - Version 1.1.0
    ///  04.10.2013, Dennis Weise
    ///     - Setup angepaßt: Installationsverzeichnis wird geprüft
    ///  10.01.2014, Dennis Weise
    ///     - ColumnCompare hinzugefügt
    ///  10.01.2014, Dennis Weise
    ///     - Version 1.1.1
    ///  10.01.2014, Dennis Weise
    ///     - ColumnCompare: In die Error Tabelle wird isETL_ErrorColumnNames korrekt gefüllt
    ///  10.01.2014, Dennis Weise
    ///     - Version 1.1.2
    ///  17.01.2014, Dennis Weise
    ///     - Compare von unterschiedlichen Datentypen wird nun unterstützt
    ///     - Wird der OnError- oder OnNUll-Wert einer Spalte verwendet, so werden Compares, die diese Spalte nutzen, ignoriert
    ///     - Wird in einem Compare eine nicht vorhandene Spalte genutzt, so wird zur Design-Zeit ein Fehler ausgegeben
    ///     - Ändert sich ein Spaltenname im Datenfluss, so werden Compares entsprechend korrigiert
    ///  17.01.2014, Dennis Weise
    ///     - Version 1.1.3
    ///  24.01.2014, Dennis Weise
    ///     - Korrektur: OnError/OnNull führt nicht mehr zum Abbruch, wenn im DC kein Compare genutzt wird
    ///  24.01.2014, Dennis Weise
    ///     - Compare: Prüfung auf Kompatibilität der Datentypen der zu vergleichenden spalten hinzugefügt
    ///  24.01.2014, Dennis Weise
    ///     - Version 1.1.4
    ///  31.01.2014, Dennis Weise
    ///     - Editor für "Compare" hinzugefügt
    ///     - Ein falscher Vergleichsoperator für "Compare" führt nicht mehr zu korrupten Metadaten
    ///     - Validate: Wurde ein falscher Vergleichsoperator, führt das zu einer Fehlermeldung
    ///  31.01.2014, Dennis Weise
    ///     - Version 1.1.5
    ///  28.03.2014, Dennis Weise
    ///     - Konvertierungen von formatiertem string (z.B. DD.MM.YYYY) nach string/int hinzugefügt
    ///  28.03.2014, Dennis Weise
    ///     - Version 1.1.6
    ///  11.04.2014, Dennis Weise
    ///     - Konvertierungen von formatiertem string (z.B. DD.MM.YYYY) nach date hinzugefügt
    ///  11.04.2014, Dennis Weise
    ///     - Version 1.1.7
    ///  13.06.2014, Dennis Weise
    ///     - Konvertierungen von formatierten strings (z.B. DD.MM.YYYY): Zur kurze Werte führten bei der Konvertierung zum Abbruch des DC
    ///  13.06.2014, Dennis Weise
    ///     - Version 1.1.8
    /// 09.07.2014, Dennis Weise  
    ///     - PerformUpgrade: Upgrade von 2008 auf 2012/2014 ist nun möglich 
    /// </summary>
   
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
    public class DataConverter : PipelineComponent
    {
        private PipelineBuffer _outputErrorBuffer;
        private PipelineBuffer _outputLogBuffer;
        private int _outputID;
        private int _outputDummyID;
        private int _outputErrorID;
        private int _outputLogID;
        private BufferMapping[] _bufferMappings;
        private NewColumnMapping[] _newColumnMappings;
        private VariableValues _varValues;
        private Comparer _comparer;

        private bool _debugMode;
        //private int[] _inputColumnBufferIndexes;
        //private int[] _outputColumnBufferIndexes;

        private IsagCustomProperties _IsagCustomProperties;

        #region Validate & Reinitialize

        public override DTSValidationStatus Validate()
        {
            InitProperties();

            _IsagCustomProperties.Save(ComponentMetaData);

            Mapping.UpdateInputIdProperties(ComponentMetaData, _IsagCustomProperties);
            Mapping.AddOutputIdProperties(ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME], ComponentMetaData, _IsagCustomProperties);
            Mapping.AddOutputIdProperties(ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME], ComponentMetaData, _IsagCustomProperties);

            DTSValidationStatus status = base.Validate();
            if (status != DTSValidationStatus.VS_ISVALID) return status;

            if (!_IsagCustomProperties.IsValid(ComponentMetaData)) return DTSValidationStatus.VS_NEEDSNEWMETADATA;

            if (!this.ComponentMetaData.AreInputColumnsValid) return DTSValidationStatus.VS_NEEDSNEWMETADATA;

            return DTSValidationStatus.VS_ISVALID;
        }

        public override void ReinitializeMetaData()
        {
            base.ReinitializeMetaData();
            this.ComponentMetaData.RemoveInvalidInputColumns();

            InitProperties();
            _IsagCustomProperties.FixError(ComponentMetaData);

            _IsagCustomProperties.Save(ComponentMetaData);
        }




        #endregion

        #region DesignTime

        public override void ProvideComponentProperties()
        {
            base.ProvideComponentProperties();

            //initProperties();
            _IsagCustomProperties = new IsagCustomProperties();

            //Metadaten Version auf DLL-Version setzen 
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
            Constants.CreateStandardErrorOutput(ComponentMetaData, input.ID);

            //Custom Property: Version
            IDTSCustomProperty100 prop = ComponentMetaData.CustomPropertyCollection.New();
            prop.Name = Constants.PROP_VERSION;
            prop.Value = _IsagCustomProperties.Version;

            //Custom Property: Configuration
            prop = ComponentMetaData.CustomPropertyCollection.New();
            prop.Name = Constants.PROP_CONFIG;
        }


        private void ResetCollections()
        {
            IDTSOutputColumnCollection100 outputLogColumns = ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME].OutputColumnCollection;
            while (outputLogColumns.Count > 3) outputLogColumns.RemoveObjectByIndex(outputLogColumns.Count - 1);

            IDTSOutput100 output = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];

            for (int i = output.OutputColumnCollection.Count - 1; i >= 0; i--)
            {
                IDTSOutputColumn100 col = output.OutputColumnCollection[i];

                if (!_IsagCustomProperties.HasNewOutputColumn(col.LineageID.ToString()))
                {
                    output.OutputColumnCollection.RemoveObjectByID(col.ID);
                }
            }

            //ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME].OutputColumnCollection.RemoveAll();
            ComponentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection.RemoveAll();
        }

        public override void OnInputPathAttached(int inputID)
        {
            base.OnInputPathAttached(inputID);

            InitProperties();

            //Initialisierung falls zuvor noch keine Inputcolumns angebunden waren
            if (_IsagCustomProperties.ColumnConfigList.Count == 0)
            {
                ResetCollections();
                _IsagCustomProperties.ColumnConfigList.Clear();

                IDTSOutput100 output = this.ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];
                IDTSOutput100 outputLog = this.ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];
                IDTSInput100 input = ComponentMetaData.InputCollection.GetObjectByID(inputID);
                IDTSVirtualInput100 vInput = input.GetVirtualInput();

                _IsagCustomProperties.AddColumnConfig(vInput, input, output, outputLog);

                _IsagCustomProperties.Save(ComponentMetaData);
            }
        }

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

        private class BufferRow
        {
            public string ErrorExceptions { get; set; }
            public string ErrorColumns { get; set; }
            public string LogExceptions { get; set; }
            public string LogColumns { get; set; }
            public int ErrorCount { get; set; }
            public bool HasError { get; set; }
            public bool Log { get; set; }
            public IsagCustomProperties.ErrorRowHandling ErrorHandling { get; set; }

            public bool HasLog { get { return LogColumns != null && LogColumns != ""; } }

            public string AllExceptions
            {
                get
                {
                    if (!string.IsNullOrEmpty(ErrorExceptions) && !string.IsNullOrEmpty(LogExceptions))
                        return ErrorExceptions + ";" + LogExceptions;
                    else return ErrorExceptions + LogExceptions;
                }
            }

            public string AllColumns
            {
                get
                {
                    if (!string.IsNullOrEmpty(ErrorColumns) && !string.IsNullOrEmpty(LogColumns))
                        return ErrorColumns + ";" + LogColumns;
                    else return ErrorColumns + LogColumns;
                }

            }

            public BufferRow()
            {
                Reset();
            }

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

            public void AddError(IsagCustomProperties.ErrorRowHandling errorHandling, bool hasError, bool log, Exception ex, string columnName, bool debugMode)
            {

                Log = Log || log;
                HasError = HasError || hasError;
                if (log || hasError) ErrorCount++;

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

                if (errorHandling == IsagCustomProperties.ErrorRowHandling.RedirectRow) ErrorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

            }


        }

        private struct VariableValues
        {
            public string TaskID;
            public string TaskName;
            public string PackageID;
            public string PackageName;
            public string ExecutionID;
            public string Name;
        }

        private struct BufferMapping
        {
            public int inputBufferIndex;
            public int outputBufferIndex;
            public int outputLogBufferIndex;
            public bool hasSameDataType;
            public DataType outputDataType;
            public int lengthOfString;
            public object onNullValue;
            public bool hasOnNullValue;
            //public bool useColumn;
            public bool AllowNull;
            public object onErrorValue;
            public bool hasOnErrorValue;
            public IsagCustomProperties.ErrorRowHandling errorHandling;

            public bool convert;
            public bool compare;
            public bool hasRegEx;
            public string RegEx;

            public bool ConvertFromIntToDate;
            public bool ConvertFromDateToInt;
            public bool ConvertFromDateToNumeric;
            public bool ConvertToString;
            public bool ConvertToNumericByUsingConversionRules;
            public DateConvertTypes ConvertFromStringToNumericType;
            public DateConvertTypes ConvertFromDateToStringType;
            public string ConvertFromStringFormat;
            public DataTypeKind OutputDataTypeKindForDate;


            public bool ConvertFromString;
            public bool ConvertFromStringByFormat;
            public string ColumnName;
            public bool isErrorCounter;

            public int dateYYYYIndex;
            public int dateMMIndex;
            public int dateDDIndex;
            public int dateFirstSplitterIndex;
            public int dateSecondSpitterIndex;
            public string dateFirstSplitter;
            public string dateSecondSpitter;
        }

        private struct NewColumnMapping
        {
            public int outputBufferIndex;
            public object value;
        }

        #region PreExecute

        public override void PreExecute()
        {
            base.PreExecute();

            InitProperties();

            _debugMode = _IsagCustomProperties.DebugModus;

            InitVariables();

            IDTSInput100 input = ComponentMetaData.InputCollection[Constants.INPUT_NAME];
            IDTSOutput100 output = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME];
            IDTSOutput100 outputError = ComponentMetaData.OutputCollection[Constants.OUTPUT_ERROR_NAME];
            IDTSOutput100 outputLog = ComponentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];

            if (!outputError.IsAttached) Events.Fire(ComponentMetaData, Events.Type.Warning, "Error Output is missing.");
            if (!outputLog.IsAttached) Events.Fire(ComponentMetaData, Events.Type.Warning, "Log Output is missing.");

            InitBufferMapping(input, output, outputLog);
            InitNewColumnMapping(input, output);

            _outputID = output.ID;
            _outputDummyID = ComponentMetaData.OutputCollection[Constants.OUTPUT_NAME + "dummy"].ID;
            _outputErrorID = outputError.ID;
            _outputLogID = outputLog.ID;

        }

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
                    _varValues.Name = ComponentMetaDataTools.GetValueFromVariable(VariableDispenser, _IsagCustomProperties.ErrorName);
                }
                catch (Exception)
                {
                    _varValues.Name = _IsagCustomProperties.ErrorName;
                }
            }
            catch (Exception ex)
            {
                Events.Fire(ComponentMetaData, Events.Type.Error, "Fehler beim Einlesen der Variablen für den Error Output: " + ex.Message);
                throw;
            }
        }

        private void InitBufferMapping(IDTSInput100 input, IDTSOutput100 output, IDTSOutput100 outputLog)
        {
            bool needsComparer = _IsagCustomProperties.HasCompareColumn();

            if (needsComparer) _comparer = new Comparer();
            List<BufferMapping> mappingList = new List<BufferMapping>();

            for (int i = 0; i < input.InputColumnCollection.Count; i++)
            {
                ColumnConfig config = _IsagCustomProperties.GetColumnConfigByInputColumnName(input.InputColumnCollection[i].Name);

                BufferMapping bufferMapping = new BufferMapping();

                bufferMapping.inputBufferIndex =
                     BufferManager.FindColumnByLineageID(input.Buffer, input.InputColumnCollection[i].LineageID);

                if (outputLog.IsAttached)
                    bufferMapping.outputLogBufferIndex =
                        BufferManager.FindColumnByLineageID(
                            outputLog.Buffer,
                            outputLog.OutputColumnCollection[config.InputColumnName].LineageID
                        );
                else bufferMapping.outputLogBufferIndex = -1;

                if (config.Convert)
                {
                    bufferMapping.outputBufferIndex =
                        BufferManager.FindColumnByLineageID(
                            input.Buffer,
                            output.OutputColumnCollection[config.OutputAlias].LineageID
                        );

                    //Prüfen ob Input- und OutputDataType identisch sind
                    bufferMapping.hasSameDataType = config.HasSameInputAndOutputDataType();

                    //OutputDataType ermitteln & DefaultWert nach Output Datatype konvertieren 
                    try
                    {
                        //bufferMapping.outputDataType = output.OutputColumnCollection[config.OutputAlias].DataType;
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
                        else bufferMapping.ConvertFromDateToStringType = DateConvertTypes.None;

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
                    catch (Exception) { } //Column ist nicht im Output vorhanden

                    bufferMapping.hasOnNullValue = config.HasDefaultValue();
                    bufferMapping.hasOnErrorValue = config.HasOnErrorValue();
                    bufferMapping.AllowNull = config.AllowNull;
                    bufferMapping.errorHandling = config.ErrorHandling;
                    bufferMapping.hasRegEx = config.HasRegEx();


                    if (bufferMapping.hasRegEx) bufferMapping.RegEx = config.RegEx;

                    if (_comparer != null && config.HasCompare) _comparer.AddCompare(config.Compare, config.InputColumnName);
                }

                bufferMapping.ColumnName = config.InputColumnName;
                bufferMapping.convert = config.Convert;
                bufferMapping.isErrorCounter = config.IsErrorCounter;

                mappingList.Add(bufferMapping);


                _bufferMappings = mappingList.ToArray();



            }
        }

        private void InitNewColumnMapping(IDTSInput100 input, IDTSOutput100 output)
        {
            _newColumnMappings = new NewColumnMapping[_IsagCustomProperties.NewColumnConfigList.Count];



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

        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            base.PrimeOutput(outputs, outputIDs, buffers);

            InitProperties();

            if (buffers.Length > 0)
            {
                for (int i = 0; i < outputs; i++)
                {
                    if (outputIDs[i] == _outputErrorID) _outputErrorBuffer = buffers[i];
                    else if (outputIDs[i] == _outputLogID) _outputLogBuffer = buffers[i];
                }
            }

            Events.Fire(ComponentMetaData, Events.Type.Information, "PrimeOutput finished: " + DateTime.Now.ToLongTimeString());
        }

        #region ProcessInput

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            base.ProcessInput(inputID, buffer);

            BufferRow row = new BufferRow();

            while (buffer.NextRow())
            {

                row.Reset();  //Vorhandene Fehlermeldungen der letzten Zeile löschen               
                if (_comparer != null) _comparer.Reset(); //Gespeicherte Werte der letzten Zeile löschen              


                try
                {
                    for (int colIdx = 0; colIdx < _bufferMappings.Length; colIdx++)
                    {
                        StatusConvert status = new StatusConvert();
                        BufferMapping config = _bufferMappings[colIdx]; //Konfiguration der aktuellen Spalte laden                       

                        if (config.convert)
                        {
                            object value = getBufferValue(buffer[config.inputBufferIndex]); //Wert der Zelle ermitteln

                            //Den OnNull Value verwenden?
                            if ((value == null || (config.ConvertFromIntToDate && value.ToString() == "0")) && config.hasOnNullValue)
                            {
                                ProcessInputOnNull(buffer, row, config);
                                if (_comparer != null) _comparer.SkipColumn(config.ColumnName);
                            }
                            else
                            {
                                if (value == null && config.AllowNull) buffer.SetNull(config.outputBufferIndex); //Null weiterleiten
                                else ProcessInputConvert(buffer, row, config, value, ref status, ref _comparer); //Konvertieren                             
                            }
                        }

                        if (_comparer != null)
                        {
                            if (config.convert) _comparer.AddColumnValue(_bufferMappings[colIdx].ColumnName, buffer[config.inputBufferIndex], buffer[config.outputBufferIndex]);
                            else _comparer.AddColumnValue(_bufferMappings[colIdx].ColumnName, buffer[config.inputBufferIndex]);
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

                    if (row.Log && row.ErrorHandling == IsagCustomProperties.ErrorRowHandling.RedirectRow) ProcessInputLog(row, buffer);
                    else buffer.DirectRow(_outputID);
                }
                catch (Exception ex)
                {
                    Events.Fire(ComponentMetaData, Events.Type.Error, ex.Message);
                    throw ex;
                }
            }

            if (buffer.EndOfRowset)
            {
                if (_outputErrorBuffer != null) _outputErrorBuffer.SetEndOfRowset();
                if (_outputLogBuffer != null) _outputLogBuffer.SetEndOfRowset();
            }

            Events.Fire(ComponentMetaData, Events.Type.Information, "ProcessInput finished: " + DateTime.Now.ToLongTimeString());
        }

        private void ProcessInputLog(BufferRow row, PipelineBuffer buffer)
        {
            if (_outputLogBuffer != null)
            {
                _outputLogBuffer.AddRow();

                for (int i = 0; i < _bufferMappings.Length; i++)
                {
                    BufferMapping config = _bufferMappings[i];

                    object value = getBufferValue(buffer[_bufferMappings[i].inputBufferIndex]);
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
                        if (value == null) _outputLogBuffer.SetNull(config.outputLogBufferIndex);
                        else _outputLogBuffer[config.outputLogBufferIndex] = value;
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
            else buffer.DirectRow(_outputID);
        }

        private void ProcessInputConvert(PipelineBuffer buffer, BufferRow row, BufferMapping config, object value, ref StatusConvert status, ref Comparer comparer)
        {

            //Konvertieren falls Datentypen Input&Output nicht identisch
            //Exception wenn Fehler bei Konvertierung

            if (value == null && !config.AllowNull) status.SetError("Null values are not allowed.");
            //throw new Exception("Null values are not allowed.");

            if (!status.HasError && (!config.hasSameDataType || config.ConvertFromStringByFormat))
            {
                if (config.ConvertFromStringByFormat) value = Converter.String2YearMonthDayByFormat(config.OutputDataTypeKindForDate, config.outputDataType, config.dateYYYYIndex,
                                                                                                    config.dateMMIndex, config.dateDDIndex, config.dateFirstSplitterIndex,
                                                                                                    config.dateSecondSpitterIndex, config.dateFirstSplitter, config.dateSecondSpitter,
                                                                                                    value, ref status);
                else if (config.ConvertFromIntToDate) value = Converter.IntToDate(value, ref status);
                else if (config.ConvertFromDateToInt) value = Converter.DateToInt(value, config.outputDataType, ref status);
                else if (config.ConvertFromDateToStringType != DateConvertTypes.None) value = Converter.DateToString(value, config.ConvertFromDateToStringType, ref status);
                else if (config.ConvertToNumericByUsingConversionRules) value = Converter.String2Numeric(value, config.ConvertFromStringToNumericType, config.outputDataType, ref status);
                else value = Converter.GetConvertedValue(value, config.outputDataType, ref status, config.ConvertFromString);

                if ((config.outputDataType == DataType.DT_WSTR || config.outputDataType == DataType.DT_STR) && value != null && value.ToString().Length > config.lengthOfString)
                {
                    status.SetError(value.ToString() + " ist zu lang.");
                }
            }

            //RegEx Prüfung
            // Expception schmeißen wenn nicht gültig, sonst Schreiben in Output
            if (!status.HasError)
            {
                if (config.hasRegEx && !RegularExpressions.IsMatch(value.ToString(), config.RegEx))
                    status.SetError(value.ToString() + " entspricht nicht der Regular Expression.");
                //throw new Exception(value.ToString() + " entspricht nicht der Regular Expression.");
                else buffer[config.outputBufferIndex] = value;
            }


            if (status.HasError)
            {
                if (_comparer != null) _comparer.SkipColumn(config.ColumnName);

                if (config.hasOnErrorValue) //OnError verwenden
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
                else //Convert der Row fehlgeschlagen
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
        ///     Die Datentypen D_NTEXT, D_NVARCHARMAX, D_TEXT, D_XML übernimmt SSIS als BlobColumn. Das führt zu einen Typkonflikt. 
        ///     Workaround: Es wird auf den Typ BlobColumn geprüft und ggfs. in ein Byte-Array konvertiert
        /// </summary>
        /// <param name="bufferValue"></param>
        /// <returns></returns>
        private object getBufferValue(object bufferValue)
        {
            if (bufferValue != null && bufferValue.GetType() == typeof(BlobColumn))
            {
                BlobColumn bc = (BlobColumn)bufferValue;
                bufferValue = bc.GetBlobData(0, (int)bc.Length);
            }

            return bufferValue;
        }


        #endregion


        private void InitProperties()
        {
            object configuration = this.ComponentMetaData.CustomPropertyCollection[Constants.PROP_CONFIG].Value;
            if (configuration != null) _IsagCustomProperties = IsagCustomProperties.Load(configuration);
            else _IsagCustomProperties = new IsagCustomProperties();
        }


        #region PerfornUpgrade

        /// <summary>
        /// Upgrade von SSIS 2008 auf 2012/2014
        /// </summary>
        /// <param name="pipelineVersion"></param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            try
            {
                if (Mapping.NeedsMapping())
                {
                    InitProperties();

                    foreach (ColumnConfig config in _IsagCustomProperties.ColumnConfigList)
                    {
                        if (string.IsNullOrEmpty(config.CustomId)) config.CustomId = Guid.NewGuid().ToString();

                        AddInputColumnCustomProperty(config.InputColumnName, config.CustomId, Mapping.IdPropertyName);
                        if (config.Convert) AddOutputColumnCustomProperty(config.OutputAlias, config.CustomId, Mapping.IdPropertyName, Constants.OUTPUT_NAME);
                        AddOutputColumnCustomProperty(config.InputColumnName, config.CustomId, Mapping.IdPropertyName, Constants.OUTPUT_LOG_NAME);
                    }

                    Mapping.UpdateInputIdProperties(this.ComponentMetaData, _IsagCustomProperties);
                    _IsagCustomProperties.Save(this.ComponentMetaData);
                }

                DtsPipelineComponentAttribute attr =
                    (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
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
