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
using DataConverter.ComponentFrameWork.Mapping;

namespace DataConverter
{
    public class IsagCustomProperties
    {
        public enum ErrorRowHandling { RedirectRow, FailComponent, IgnoreFailure }

        #region Properties

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


        public List<ColumnConfig> ColumnConfigList { get; set; }

        public List<NewColumnConfig> NewColumnConfigList { get; set; }

        public string AliasPrefix { get; set; }

        public bool DebugModus { get; set; }

        public string ErrorName { get; set; }

        #endregion

        #region Constructor

        public IsagCustomProperties()
        {
            this.ColumnConfigList = new List<ColumnConfig>();
        }

        public IsagCustomProperties(string aliasPrefix)
        {
            this.AliasPrefix = aliasPrefix;
            this.ColumnConfigList = new List<ColumnConfig>();
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

        public void AddColumnConfig(ColumnConfig configRow)
        {
            this.ColumnConfigList.Add(configRow);
        }

        //public void AddColumnConfig(IDTSVirtualInputColumn100 vCol, IDTSVirtualInput100 vInput,
        //                            IDTSInput100 input, IDTSOutput100 output, IDTSOutput100 outputError)
        //{
        //    this.ColumnConfigList.Add(InsertColumn(vCol, vInput, input, output, outputError));
        //}

        public void AddColumnConfig(IDTSVirtualInput100 vInput, IDTSInput100 input, IDTSOutput100 output, IDTSOutput100 outputLog)
        {
            for (int i = 0; i < vInput.VirtualInputColumnCollection.Count; i++)
            {
                AddColumnConfig(InsertColumn(vInput.VirtualInputColumnCollection[i], vInput, input, outputLog));
            }
        }

        public ColumnConfig GetColumnConfigByInputColumnName(string inputColumnName)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.InputColumnName == inputColumnName) return config;
            }

            return null;
        }

        //public ColumnConfig GetColumnConfigByLineageId(string inputLienageId)
        //{
        //    foreach (ColumnConfig config in ColumnConfigList)
        //    {
        //        if (config.InputLineageId == inputLienageId) return config;
        //    }

        //    return null;
        //}

        public ColumnConfig GetColumnConfigByOutputAlias(string outputColName)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.OutputAlias == outputColName) return config;
            }

            return null;
        }

        public ColumnConfig GetColumnConfigByOutputLineageId(string outputLienageId)
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.OutputLineageId == outputLienageId) return config;
            }

            return null;
        }

        public void AddPrefix()
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.Convert) config.OutputAlias = AliasPrefix + config.InputColumnName;
            }
        }

        #endregion

        #region Save & Load

        public void Save(IDTSComponentMetaData100 componentMetaData)
        {
            componentMetaData.CustomPropertyCollection[Constants.PROP_CONFIG].Value = SaveToXml();
        }

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


        public static IsagCustomProperties LoadFromXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(IsagCustomProperties));

            StringReader reader = new StringReader(xml);
            IsagCustomProperties result = (IsagCustomProperties)serializer.Deserialize(reader);

            return result;
        }



        #endregion

        #region Validate

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
                Events.Fire(componentMetaData, Events.Type.Error, "Der Log Ouput ist ungültig!");
                return false;
            }
            else outputLog = componentMetaData.OutputCollection[Constants.OUTPUT_LOG_NAME];

            if (!hastStandardErrorOutput)
            {
                Events.Fire(componentMetaData, Events.Type.Error, "Der Error Ouput ist ungültig!");
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

        //HasCompareColumnWithoutInputColumn


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
                                   string.Format("Das Konvertierungsformat {0} ist ungültig.", config.StrConversionByFormat));
                        return false;
                    }
                }
            }

            return true;
        }

        
        /// <summary>
        /// Prüft ob alle "Compares" korrekt sind:
        /// 1. Wird auf nicht vorhandene Spalte verwiesen?
        /// 2. Können alle Datentypen verglichen werden?
        /// </summary>
        /// <param name="componentMetaData"></param>
        /// <returns></returns>
        private bool IsCompareValid(IDTSComponentMetaData100 componentMetaData)
        {
            //Compare aufbauen
            Comparer compare = new Comparer();

            if (compare.HasInvalidOp)
            {
                Events.Fire(componentMetaData, Events.Type.Error,
                                    string.Format("Ein Compare enthält einen ungültigen Operator."));
                return false;
            }

            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare) compare.AddCompare(config.Compare, config.InputColumnName);
            }

            
            foreach (string col in compare.UsedColumns)
            {
                if (GetColumnConfigByInputColumnName(col) == null)
                {
                    Events.Fire(componentMetaData, Events.Type.Error,
                                string.Format("Ein Compare enthält die ungültige Spalte {0}. ", col));
                    return false;
                }
            }

            string errorMessage = "";
            if (!compare.AreDataTypesComparable(this, ref errorMessage))
            {
                Events.Fire(componentMetaData, Events.Type.Error, errorMessage);
                return false;
            }

            return true;
        }

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
                                string.Format("Spalte [Input={0}] enthält einen ungültigen OnError Wert: " + errorMessage, config.InputColumnName));
                    return false;
                }
            }

            return true;
        }

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
                                string.Format("Spalte [Input={0}] enthält einen ungültigen OnNull Wert: " + errorMessage, config.InputColumnName));
                    return false;
                }
            }

            return true;
        }

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
                            "Im Mapping existiert mind. eine Spalte mit einem Namen, der vom Namen der Inputspalte abweicht!");
                return false;
            }


            return true;

        }
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
                                    "Im Mapping existiert mind. eine Spalte ohne Bezug zu einer Outputspalte!");
                        return true;
                    }
                }
                catch (Exception)
                {
                    //GetOutputColumnByLineageID schmeißt eine Exception wenn es keine OutputColumn zur LineageId gibt
                    return true;
                }

            }

            return false;
        }

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

        public bool HasLogOutputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.OutputErrorLineageId == lineageId) return true;
            }

            return false;
        }
        public bool HasOutputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.Convert && config.OutputLineageId == lineageId) return true;
            }

            return false;
        }

        public bool HasCompareColumn()
        {
            foreach (ColumnConfig config in ColumnConfigList)
            {
                if (config.HasCompare) return true;
            }

            return false;
        }

        private bool HasVirtualInputColumn(string lineageId)
        {
            foreach (ColumnConfig config in this.ColumnConfigList)
            {
                if (config.InputLineageId == lineageId) return true;
            }

            return false;

        }
        public bool HasNewOutputColumn(string lineageId)
        {
            foreach (NewColumnConfig config in this.NewColumnConfigList)
            {
                if (config.OutputLineageId == lineageId) return true;
            }

            return false;

        }

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
            if (!Constants.HasStandardErrorOutput(componentMetaData)) Constants.CreateStandardErrorOutput(componentMetaData, input.ID);



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

        public ColumnConfig InsertColumn(IDTSVirtualInputColumn100 vColumn, IDTSVirtualInput100 vInput,
                                                           IDTSInput100 input, IDTSOutput100 outputLog)
        {

            vInput.SetUsageType(vColumn.LineageID, DTSUsageType.UT_READONLY);
            IDTSInputColumn100 inputColumn = ComponentMetaDataTools.GetInputColumnByLineageId(input.InputColumnCollection, vColumn.LineageID);

            //IDTSOutputColumn100 outCol = output.OutputColumnCollection.New();
            //outCol.Name = vColumn.Name;
            //outCol.SetDataTypeProperties(vColumn.DataType, vColumn.Length, vColumn.Precision, vColumn.Scale, vColumn.CodePage);

            IDTSOutputColumn100 outLogCol = outputLog.OutputColumnCollection.New();
            outLogCol.Name = vColumn.Name;
            outLogCol.SetDataTypeProperties(vColumn.DataType, vColumn.Length, vColumn.Precision, vColumn.Scale, vColumn.CodePage);

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
