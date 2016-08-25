using DataConverter;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    class DataConverterComponent : Component
    {
        public new static string MONIKER = "DataConverter.DataConverter, initions.Henry.SSIS.DataConverter2016, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1e7bd12ce9d458f0";
        public static string LOGOUTPUT = "logOutput";
        public static string OUTPUT = "output";
        public static string PROPERTIES_NAME = "DataConverter Configuration";

        public DataConverterComponent(IDTSComponentMetaData100 metadata, string name) : base(metadata, name) { }

        private void AddConversion(SsisDataType outputDataType, string inputColumnName)
        {
            // Reset();
            IsagCustomProperties isagCustomProperties = GetIsagCustomProperties();

            ColumnConfig config = isagCustomProperties.GetColumnConfigByInputColumnName(inputColumnName);
            config.Convert = true;
            config.OutputAlias = "C_" + inputColumnName;
            config.DataType = outputDataType.Type.ToString();
            config.Scale = outputDataType.Scale.ToString();
            config.Precision = outputDataType.Precision.ToString();
            config.Length = outputDataType.Length.ToString();
            config.Codepage = outputDataType.Codepage.ToString();

            isagCustomProperties.Save(Metadata);
            isagCustomProperties = GetIsagCustomProperties();

            IDTSOutput100 output = Metadata.OutputCollection[0];
            IDTSInputColumn100 colInputValue = Metadata.InputCollection[0].InputColumnCollection[inputColumnName];
            IDTSOutputColumn100 colOutputValue = output.OutputColumnCollection.New();
            colOutputValue.Name = config.OutputAlias;
            colOutputValue.SetDataTypeProperties(outputDataType.Type, outputDataType.Length, outputDataType.Precision, outputDataType.Scale, outputDataType.Codepage);
            config.OutputId = colOutputValue.ID.ToString();
            config.OutputIdString = colOutputValue.IdentificationString;
            config.OutputLineageId = colOutputValue.LineageID.ToString();

            DataConverter.FrameWork.Mapping.LineageMapping.SetIdProperty(config.CustomId, colOutputValue.CustomPropertyCollection);

            isagCustomProperties.Save(Metadata);
        }

        public void SetErrorName(string errorName)
        {
            IsagCustomProperties prop = GetIsagCustomProperties();
            prop.ErrorName = errorName;
            prop.Save(Metadata);
        }

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

        private IsagCustomProperties GetIsagCustomProperties()
        {
            return IsagCustomProperties.LoadFromXml(Metadata.CustomPropertyCollection[PROPERTIES_NAME].Value.ToString());
        }

        private void AddErrorCounter(ref IsagCustomProperties properties)
        {
            properties.GetColumnConfigByInputColumnName("isETL_errorCount").IsErrorCounter = true;
            Metadata.InputCollection[0].InputColumnCollection["isETL_errorCount"].UsageType = DTSUsageType.UT_READWRITE;
        }

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
