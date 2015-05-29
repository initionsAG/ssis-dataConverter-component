using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace DataConverter
{
    public class Constants
    {
        public const string PROP_CONFIG = "DataConverter Configuration";

        public const string PROP_OUTPUT_CONFIGURATION = "OutputConfiguration";
        public const string PROP_OUTPUT_CONFIGURATION_COUNT = "OutputConfigurationCount";

        public const string PROP_VERSION = "ConvertAll Version";

        public const string PROP_PREFIX_OUTPUT_COL_NAME = "Postfix Output";
        public const string PREFIX_OUTPUT_COL_NAME_DEFAULT = "";

        public const string PATH_REGEX = @"\\localhost\SSIS_RegEx\regex.xml";
       
        //Mapping Konstanten 
        //Je Spalte wird ein Konfigurations-Array genutzt.
        //Die idx-Konstanten geben die Position einer Einstellung innerhalb des Arrays an.
        public const int MAPPING_IDX_USE = 0;
        public const int MAPPING_IDX_INPUT_COL_NAME = 1;
        public const int MAPPING_IDX_CONVERT = 2;
        public const int MAPPING_IDX_OUTPUT_ALIAS = 3;
        public const int MAPPING_IDX_DATATYPE_INPUT = 4;
        public const int MAPPING_IDX_DATATYPE = 5;
        public const int MAPPING_IDX_LENGTH = 6;
        public const int MAPPING_IDX_PRECISION = 7;
        public const int MAPPING_IDX_SCALE = 8;
        public const int MAPPING_IDX_CODEPAGE = 9;
        public const int MAPPING_IDX_DEFAULT = 10;
        public const int MAPPING_IDX_INPUT_ID = 11;
        public const int MAPPING_IDX_INPUT_ID_STRING = 12;
        public const int MAPPING_IDX_INPUT_LINEAGE_ID = 13;
        public const int MAPPING_IDX_OUTPUT_ID = 14;
        public const int MAPPING_IDX_OUTPUT_ID_STRING = 15;
        public const int MAPPING_IDX_OUTPUT_LINEAGE_ID = 16;
        public const int MAPPING_IDX_OUTPUT_ERROR_ID = 17;
        public const int MAPPING_IDX_OUTPUT_ERROR_ID_STRING = 18;
        public const int MAPPING_IDX_OUTPUT_ERROR_LINEAGE_ID = 19;
        public const int MAPPING_COUNT = 20; //Anzahl Elemente der Mapping-Config


        //Namen der Output- und Input-Collections
        public const string INPUT_NAME = "input";
        public const string OUTPUT_NAME = "output";
        public const string OUTPUT_LOG_NAME = "logOutput";
        public const string OUTPUT_ERROR_NAME = "errorOutput";


        public static readonly string[] STRING_CONVERSION_TYPES = { "YYYY.MM.DD", "DD.MM.YYYY", "YYYY/MM/DD", "DD/MM/YYYY", "YYYY-MM-DD", "DD-MM-YYYY", "MM-DD-YYYY" };

        public static readonly string[] DATATYPE_SIMPLE = 
            {"DT_BOOL","DT_CY","DT_DBDATE","DT_UI1","DT_UI2","DT_UI4","DT_UI8",
             "DT_I1","DT_I2","DT_I4","DT_I8","DT_R4","DT_R8","DT_IMAGE","DT_DATE",
             "DT_FILETIME","DT_GUID","DT_NTEXT","DT_DBTIMESTAMP"};
        public static readonly string[] DATATYPE_LENGTH = { "DT_BYTES", "DT_WSTR" };
        public static readonly string[] DATATYPE_PRECISION_SCALE = { "DT_NUMERIC" };
        public static readonly string[] DATATYPE_SCALE = { "DT_DECIMAL" };
        public static readonly string[] DATATYPE_LENGTH_CODEPAGE = { "DT_STR" };
        public static readonly string[] DATATYPE_CODEPAGE = { "DT_TEXT" };

        public static readonly string[] DATATYPE_NUMBER = {"DT_CY","DT_UI1","DT_UI2","DT_UI4","DT_UI8",
             "DT_I1","DT_I2","DT_I4","DT_I8","DT_R4","DT_R8", "DT_NUMERIC", "DT_DECIMAL"};
        public static readonly string[] DATATYPE_STRING = { "DT_STR", "DT_WSTR" };
        public static readonly string[] DATATYPE_DATE = { "DT_DBTIMESTAMP", "DT_DBDATE", "DT_DATE" };

        //On Information Event Codes
        public const int INFO_NONE = 1;
        public const int INFO_PRESQL = 1;
        public const int INFO_POSTSQL = 2;
        public const int INFO_MERGE = 3;
        public const int INFO_DELETE = 4;
        public const int INFO_INSERT = 5;
        public const int INFO_UPDATE = 6;
        public const int INFO_CREATE = 7;
        public const int INFO_SP = 8;

        public static readonly string[] INFO_NAME = {"NONE","PRE SQL", "POST SQL", "Merge", "Delete", "Insert", 
                                                     "Update", "Create", "Stored Procedure"};

        //StandardLogOuput Column Names
        public const string LOG_OUTPUT_ISERROR = "isETL_isError";
        public const string LOG_OUTPUT_ERRORCOLUMNS = "isETL_ErrorColumnNames";
        public const string LOG_OUTPUT_ERRORMESSAGES = "isETL_ErrorMessages";


        public static IDTSOutput100 CreateStandardLogOutput(IDTSComponentMetaData100 metadata)
        {
            IDTSOutput100 logOutput;

            try
            {
                logOutput = metadata.OutputCollection[OUTPUT_LOG_NAME];
                logOutput.OutputColumnCollection.RemoveAll();
            }
            catch (Exception)
            {

                logOutput = metadata.OutputCollection.New();
            }

            logOutput.Name = Constants.OUTPUT_LOG_NAME;
            logOutput.SynchronousInputID = 0; //0 -> asynchron, ID Input -> synchron

            IDTSOutputColumn100 outputColumn = logOutput.OutputColumnCollection.New();
            outputColumn.Name = LOG_OUTPUT_ISERROR;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = logOutput.OutputColumnCollection.New();
            outputColumn.Name = LOG_OUTPUT_ERRORCOLUMNS;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);

            outputColumn = logOutput.OutputColumnCollection.New();
            outputColumn.Name = LOG_OUTPUT_ERRORMESSAGES;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);

            return logOutput;
        }

        public static bool HasStandardLogOutput(IDTSComponentMetaData100 metadata)
        {
            try
            {
                IDTSOutput100 outputLog = metadata.OutputCollection[OUTPUT_LOG_NAME];

                if (outputLog.OutputColumnCollection[0].Name != LOG_OUTPUT_ISERROR) throw new Exception();
                if (outputLog.OutputColumnCollection[1].Name != LOG_OUTPUT_ERRORCOLUMNS) throw new Exception();
                if (outputLog.OutputColumnCollection[2].Name != LOG_OUTPUT_ERRORMESSAGES) throw new Exception();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IDTSOutput100 CreateStandardErrorOutput(IDTSComponentMetaData100 metadata, int inputId)
        {
            IDTSOutput100 errorOutput;

            try
            {
                errorOutput = metadata.OutputCollection[OUTPUT_ERROR_NAME];

                try {errorOutput.OutputColumnCollection.RemoveAll();                }
                catch (Exception) {} //Standard Errorspalten können nicht gelöscht werden                
            }
            catch (Exception)
            {

                errorOutput = metadata.OutputCollection.New();
            }

            errorOutput.Name = Constants.OUTPUT_ERROR_NAME;
            errorOutput.IsErrorOut = true;
            //errorOutput.SynchronousInputID = inputId;

            IDTSOutputColumn100 outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "TaskID";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "TaskName";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "PackageID";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "PackageName";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "ExecID";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = "Name";
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 100, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = Constants.LOG_OUTPUT_ERRORCOLUMNS;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);

            outputColumn = errorOutput.OutputColumnCollection.New();
            outputColumn.Name = Constants.LOG_OUTPUT_ERRORMESSAGES;
            outputColumn.SetDataTypeProperties(DataType.DT_WSTR, 4000, 0, 0, 0);

            return errorOutput;
        }

        public static bool HasStandardErrorOutput(IDTSComponentMetaData100 metadata)
        {
            try
            {
                IDTSOutput100 errorOutput = metadata.OutputCollection[OUTPUT_ERROR_NAME];

                if (errorOutput.OutputColumnCollection[2].Name != "TaskID") throw new Exception();
                if (errorOutput.OutputColumnCollection[3].Name != "TaskName") throw new Exception();
                if (errorOutput.OutputColumnCollection[4].Name != "PackageID") throw new Exception();
                if (errorOutput.OutputColumnCollection[5].Name != "PackageName") throw new Exception();
                if (errorOutput.OutputColumnCollection[6].Name != "ExecID") throw new Exception();
                if (errorOutput.OutputColumnCollection[7].Name != "Name") throw new Exception();
                if (errorOutput.OutputColumnCollection[8].Name != Constants.LOG_OUTPUT_ERRORCOLUMNS) throw new Exception();
                if (errorOutput.OutputColumnCollection[9].Name != Constants.LOG_OUTPUT_ERRORMESSAGES) throw new Exception();

                if (errorOutput.IsErrorOut != true) throw new Exception();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
