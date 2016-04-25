using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Windows.Forms;

namespace DataConverter
{
    /// <summary>
    /// Helper methods for SSIS API
    /// </summary>
    public class ComponentMetaDataTools
    {

        /// <summary>
        /// Returns SSIS imput column by lineageId
        /// </summary>
        /// <param name="inputColumns">input column collection</param>
        /// <param name="lineageId">lineageId</param>
        /// <returns>SSIS input column</returns>
        public static IDTSInputColumn100 GetInputColumnByLineageId(IDTSInputColumnCollection100 inputColumns, int lineageId)
        {

            IDTSInputColumn100 result = null;

            for (int i = 0; i < inputColumns.Count; i++)
            {
                if (inputColumns[i].LineageID == lineageId) result = inputColumns[i];
            }

            return result;
        }

        /// <summary>
        /// Returns all input column collection with their name
        /// </summary>
        /// <param name="componentMetaData">components metadata</param>
        /// <returns>Dictionary (key: input name, value SSIS Input)</returns>
        public static Dictionary<string, IDTSInputColumn100> GetInputDictionary(IDTSComponentMetaData100 componentMetaData)
        {
            Dictionary<string, IDTSInputColumn100> result = new Dictionary<string, IDTSInputColumn100>();
            foreach (IDTSInputColumn100 column in componentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection)
            {
                result.Add(column.Name, column);
            }

            return result;
        }

        /// <summary>
        /// Returns output column by name
        /// </summary>
        /// <param name="columnName">output column name</param>
        /// <param name="outputColumns">SSIS output column collection</param>
        /// <returns></returns>
        public static IDTSOutputColumn100 GetOutputColumnByColumnName(string columnName, IDTSOutputColumnCollection100 outputColumns)
        {

            foreach (IDTSOutputColumn100 column in outputColumns)
            {
                if (column.Name == columnName) return column;
            }

            return null;
        }

        /// <summary>
        /// Does a virtual input contains a column with a given lineageId?
        /// </summary>
        /// <param name="vInput">SSIS virtual input</param>
        /// <param name="lineageId">lineageId</param>
        /// <returns></returns>
        public static bool HasVirtualInputColumn(IDTSVirtualInput100 vInput, int lineageId)
        {
            bool result = false;

            foreach (IDTSVirtualInputColumn100 col in vInput.VirtualInputColumnCollection)
            {
                if (col.LineageID == lineageId) result = true;
            }

            return result;
        }

        /// <summary>
        /// Sets metadata version to assemblies current version
        /// </summary>
        /// <param name="component">pipeline component</param>
        /// <param name="componentMetaData">components metdadata</param>
        public static void UpdateVersion(PipelineComponent component, IDTSComponentMetaData100 componentMetaData)
        {
            DtsPipelineComponentAttribute componentAttr =
                 (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(component.GetType(), typeof(DtsPipelineComponentAttribute), false);
            int binaryVersion = componentAttr.CurrentVersion;
            componentMetaData.Version = binaryVersion;
        }

        /// <summary>
        /// Gets SSIS datatype from string 
        /// </summary>
        /// <param name="name">string representation of SSIS datatype</param>
        /// <returns></returns>
        public static DataType GetDataType(string name)
        {
            switch (name)
            {
                case "DT_BOOL":
                    return DataType.DT_BOOL;
                case "DT_BYTES":
                    return DataType.DT_BYTES;
                case "DT_CY":
                    return DataType.DT_CY;
                case "DT_DATE":
                    return DataType.DT_DATE;
                case "DT_DBDATE":
                    return DataType.DT_DBDATE;
                case "DT_DBTIMESTAMP":
                    return DataType.DT_DBTIMESTAMP;
                case "DT_DECIMAL":
                    return DataType.DT_DECIMAL;
                case "DT_FILETIME":
                    return DataType.DT_FILETIME;
                case "DT_GUID":
                    return DataType.DT_GUID;
                case "DT_I1":
                    return DataType.DT_I1;
                case "DT_I2":
                    return DataType.DT_I2;
                case "DT_I4":
                    return DataType.DT_I4;
                case "DT_I8":
                    return DataType.DT_I8;
                case "DT_IMAGE":
                    return DataType.DT_IMAGE;
                case "DT_NTEXT":
                    return DataType.DT_NTEXT;
                case "DT_NULL":
                    return DataType.DT_NULL;
                case "DT_NUMERIC":
                    return DataType.DT_NUMERIC;
                case "DT_R4":
                    return DataType.DT_R4;
                case "DT_R8":
                    return DataType.DT_R8;
                case "DT_STR":
                    return DataType.DT_STR;
                case "DT_TEXT":
                    return DataType.DT_TEXT;
                case "DT_UI1":
                    return DataType.DT_UI1;
                case "DT_UI2":
                    return DataType.DT_UI2;
                case "DT_UI4":
                    return DataType.DT_UI4;
                case "DT_UI8":
                    return DataType.DT_UI8;
                case "DT_WSTR":
                    return DataType.DT_WSTR;
                default:
                    return DataType.DT_NULL; //Exception
            }
        }

        /// <summary>
        /// Returns value of a variable
        /// </summary>
        /// <param name="variableDispenser">SSIS variable dispenser</param>
        /// <param name="variableName">variable name</param>
        /// <returns></returns>
        public static string GetValueFromVariable(IDTSVariableDispenser100 variableDispenser, string variableName)
        {
            string result;

            IDTSVariables100 var = null;
            variableDispenser.LockOneForRead(variableName, ref var);
            result = var[variableName].Value.ToString();
            var.Unlock();

            return result;
        }

        /// <summary>
        /// Returns if a value can be converted to a specified datatype
        /// (fills errorMessage if conversion fails)
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="dataType">destination datatype</param>
        /// <param name="length">datatype length</param>
        /// <param name="scale">datatype scale</param>
        /// <param name="precision">datatype precision</param>
        /// <param name="errorMessage">errorMessage that is filled if conversion fails</param>
        /// <returns>Returns if a value can be converted to a specified datatype</returns>
        public static bool CanConvertTo(object value, DataType dataType, int length, int scale, int precision, out string errorMessage, int codepage)
        {
            errorMessage = "";

            try
            {
                switch (dataType)
                {
                    case DataType.DT_BOOL:
                        Convert.ToBoolean(value);
                        return true;
                    case DataType.DT_BYTES:
                        byte[] b = (byte[])value;
                        if (b.Length > length) throw new Exception();
                        return true;
                    case DataType.DT_CY:
                        Convert.ToDecimal(value);
                        return true;
                    case DataType.DT_DATE:
                        Convert.ToDateTime(value);
                        return true;
                    case DataType.DT_DBDATE:
                        Convert.ToDateTime(value);
                        return true;
                    case DataType.DT_DBTIME:
                        MessageBox.Show("Typ " + dataType + " wird nicht unterstützt.");
                        return false;
                    case DataType.DT_DBTIMESTAMP:
                        Convert.ToDateTime(value);
                        return true;
                    case DataType.DT_DECIMAL:
                        if (!IsValidDecimal(Convert.ToDecimal(value), scale, 0))
                            throw new Exception(String.Format("Conversion to DT_Decimal [Scale={0}] failed.", scale));
                        return true;
                    case DataType.DT_FILETIME:
                        throw new Exception("Datatype " + dataType.ToString() + " is not supported.");
                    case DataType.DT_GUID:
                        Guid g = (Guid)value;
                        return true;
                    case DataType.DT_I1:
                        Convert.ToSByte(value);
                        return true;
                    case DataType.DT_I2:
                        Convert.ToInt16(value);
                        return true;
                    case DataType.DT_I4:
                        Convert.ToInt32(value);
                        return true;
                    case DataType.DT_I8:
                        Convert.ToInt64(value);
                        return true;
                    case DataType.DT_IMAGE:
                        BlobColumn colDT_IMAGE = (BlobColumn)value;
                        if (colDT_IMAGE.IsNull) throw new Exception();
                        else Convert.ToInt32(colDT_IMAGE.Length);
                        return true;
                    case DataType.DT_NTEXT:                       
                        if (value == null)
                            return false;
                        else
                        {
                            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(value.ToString());
                            return true;
                        }
                    case DataType.DT_NULL:
                        return (value == null);
                    case DataType.DT_NUMERIC:
                        if (!IsValidDecimal(Convert.ToDecimal(value), scale, precision)) 
                            throw new Exception(String.Format("Conversion to DT_Numeric [Scale={0}, Precision={1}] failed.", scale, precision));
                        return true;
                    case DataType.DT_R4:
                        Convert.ToSingle(value);
                        return true;
                    case DataType.DT_R8:
                        Convert.ToDouble(value);
                        return true;
                    case DataType.DT_STR:
                        string s = value.ToString();
                        if (s.Length > length) throw new Exception(string.Format("Conversion to DT_STR[Length={0}] failed.", length));
                        return true;
                    case DataType.DT_TEXT:
                        if (value == null)
                            return false;
                        else
                        {
                            byte[] bytes = System.Text.Encoding.GetEncoding(codepage).GetBytes(value.ToString());
                            return true;
                        }
                    case DataType.DT_UI1:
                        Convert.ToByte(value);
                        return true;
                    case DataType.DT_UI2:
                        Convert.ToUInt16(value);
                        return true;
                    case DataType.DT_UI4:
                        Convert.ToUInt32(value);
                        return true;
                    case DataType.DT_UI8:
                        Convert.ToUInt64(value);
                        return true;
                    case DataType.DT_WSTR:
                        string ws = value.ToString();
                        if (ws.Length > length) throw new Exception(string.Format("Conversion to DT_WSTR [Length={0}] failed.", length));
                        return true;
                    default:
                        throw new Exception("Datatype " + dataType.ToString() + " is not supported.");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }            
        }
        /// <summary>
        /// Checks if decimal value is valid for a specified scale and precision
        /// (precision = 0: any precision is valid)
        /// </summary>
        /// <param name="value">the decimal to validate</param>
        /// <param name="scale">number of digits to the right of the decimal point</param>
        /// <param name="precision">the number of digits in a number</param>
        /// <returns></returns>
        /// 
        private static bool IsValidDecimal(decimal value, int scale, int precision)
        {
            value = Math.Abs(value); //remove negative sign

            if (precision != 0)
            {
                //get values precision: string value without ,
                int digitsCount = value.ToString().Contains(",") ? value.ToString().Length - 1 : value.ToString().Length;
                if (digitsCount > precision) return false;
            }

            decimal floor = (value - Math.Floor(value)); //convert to 0.xxx or 0
            if (floor.ToString().Length > 2 && //ignore 0 / 0.0
                floor.ToString().Length > scale + 2) return false;

            return true;
        }
    }
}
