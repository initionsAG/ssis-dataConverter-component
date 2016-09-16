using DataConverter;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    /// <summary>
    /// DataConverter component inherits from Component
    /// </summary>
    class DataConverterComponent : Component
    {
        /// <summary>
        /// string to generate DataConverter component in package
        /// </summary>
        /// 

#if (SQL2008)
        public new static string MONIKER = "DataConverter.DataConverter, DataConverter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8a91e54220f9b6ce";
#elif (SQL2012)
        public new static string MONIKER = "DataConverter.DataConverter, DataConverter2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=611facfb07109fd4";
#elif (SQL2014)
        public new static string MONIKER = "DataConverter.DataConverter, DataConverter3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1e7bd12ce9d458f0";
#elif (SQL2016)
        public new static string MONIKER = "DataConverter.DataConverter, initions.Henry.SSIS.DataConverter2016, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1e7bd12ce9d458f0";
#endif
        /// <summary>
        /// logoutput of the dataconverter
        /// </summary>
        public static string LOGOUTPUT = "logOutput";
        /// <summary>
        /// output of the dataconverter
        /// </summary>
        public static string OUTPUT = "output";
        /// <summary>
        /// name of the custom property in the DataConverter
        /// </summary>
        public static string PROPERTIES_NAME = "DataConverter Configuration";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadata">Metadata of the DataConverter</param>
        /// <param name="name">Name of the Component</param>
        public DataConverterComponent(IDTSComponentMetaData100 metadata, string name) : base(metadata, name) { }

        /// <summary>
        /// Adds a conversion to the DataConverter
        /// </summary>
        /// <param name="outputDataType">destination datatype</param>
        /// <param name="inputColumnName">name of the input column</param>
        private void AddConversion(SsisDataType outputDataType, string inputColumnName)
        {
            IsagCustomProperties isagCustomProperties = GetIsagCustomProperties();

            ColumnConfig config = isagCustomProperties.GetColumnConfigByInputColumnName(inputColumnName);
            config.Convert = true;
            config.OutputAlias = "C_" + inputColumnName;
            config.DataType = outputDataType.Type.ToString();
            config.Scale = outputDataType.Scale.ToString();
            config.Precision = outputDataType.Precision.ToString();
            config.Length = outputDataType.Length.ToString();
            config.Codepage = outputDataType.Codepage.ToString();  

            IDTSOutput100 output = Metadata.OutputCollection[0];
            IDTSOutputColumn100 colOutputValue = output.OutputColumnCollection.New();
            colOutputValue.Name = config.OutputAlias;
            colOutputValue.SetDataTypeProperties(outputDataType.Type, outputDataType.Length, outputDataType.Precision, outputDataType.Scale, outputDataType.Codepage);
            config.OutputId = colOutputValue.ID.ToString();
            config.OutputIdString = colOutputValue.IdentificationString;
            config.OutputLineageId = colOutputValue.LineageID.ToString();

            DataConverter.FrameWork.Mapping.LineageMapping.SetIdProperty(config.CustomId, colOutputValue.CustomPropertyCollection);

            isagCustomProperties.Save(Metadata);
        }

        /// <summary>
        /// set property "errorName" in the DataConverter
        /// </summary>
        /// <param name="errorName">the errorname</param>
        public void SetErrorName(string errorName)
        {
            IsagCustomProperties prop = GetIsagCustomProperties();
            prop.ErrorName = errorName;
            prop.Save(Metadata);
        }

        /// <summary>
        /// Reininitialize the metadata of the DataConverter
        /// </summary>
        public void Reset()
        {
            //Input-/Outputcolumns löschen
            Metadata.InputCollection[0].InputColumnCollection.RemoveAll();
            Metadata.OutputCollection[OUTPUT].OutputColumnCollection.RemoveAll();

            //alle Spalten (ausser Standardspalten) des Log Outputs entfernen
            IDTSOutputColumnCollection100 outputLogColumns = Metadata.OutputCollection[LOGOUTPUT].OutputColumnCollection;
            while (outputLogColumns.Count > 3)
                outputLogColumns.RemoveObjectByIndex(outputLogColumns.Count - 1);

            //ColumnConfig löschen und neu aufbauen
            DataConverter.IsagCustomProperties isagCustomProperties = GetIsagCustomProperties();
            isagCustomProperties.ColumnConfigList.Clear();
            isagCustomProperties.AddColumnConfig(Metadata.InputCollection[0].GetVirtualInput(), Metadata.InputCollection[0],
                              Metadata.OutputCollection[OUTPUT], Metadata.OutputCollection[LOGOUTPUT]);

            isagCustomProperties.Save(Metadata);
        }

        /// <summary>
        /// Loads the custom properties
        /// </summary>
        /// <returns>custom properties</returns>
        private IsagCustomProperties GetIsagCustomProperties()
        {
            return IsagCustomProperties.LoadFromXml(Metadata.CustomPropertyCollection[PROPERTIES_NAME].Value.ToString());
        }

        /// <summary>
        /// configures the errorCounter
        /// </summary>
        /// <param name="properties">properties of the DataConverter</param>
        private void AddErrorCounter(ref IsagCustomProperties properties)
        {
            properties.GetColumnConfigByInputColumnName("isETL_errorCount").IsErrorCounter = true;
            Metadata.InputCollection[0].InputColumnCollection["isETL_errorCount"].UsageType = DTSUsageType.UT_READWRITE;
        }

        /// <summary>
        /// configues the DataConverter for one testConfiguration
        /// </summary>
        /// <param name="testConfiguration">the testConfiguration</param>
        public void ConfigureDataConverter(DataConverterTestConfiguration testConfiguration)
        {
            AddConversion(testConfiguration.ExpectedDataType, testConfiguration.InputColumnName);

            IsagCustomProperties prop = GetIsagCustomProperties();
            prop.Language = testConfiguration.Language;

            ColumnConfig colConfig = prop.GetColumnConfigByInputColumnName(testConfiguration.InputColumnName);
            colConfig.OnErrorValue = testConfiguration.OnError;
            colConfig.Default = testConfiguration.OnNull;
            colConfig.RegEx = testConfiguration.RegEx;
            colConfig.AllowNull = testConfiguration.AllowNull;
            if (testConfiguration.HasCompare) colConfig.Compare = testConfiguration.Compare;
            colConfig.Date2string = testConfiguration.Conversion;

            if (!string.IsNullOrWhiteSpace(testConfiguration.String_Conversion_Type))
            {
                colConfig.Date2string = DateConvertTypes.STR2YYYYMMDD;
                colConfig.StrConversionByFormat = testConfiguration.String_Conversion_Type;
            }

            if (testConfiguration.UseErrorCount) AddErrorCounter(ref prop);

            prop.Save(Metadata);
        }
    }
}
