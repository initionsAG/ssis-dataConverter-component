using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Windows.Forms;

namespace DataConverter
{
    public class ComponentMetaDataTools
    {
        public static IDTSInputColumn100 GetInputColumnByColumnName(IDTSInputColumnCollection100 inputColumns, string colName)
        {

            try
            {
                return inputColumns[colName];
            }
            catch (Exception)
            {
                
                return null;
            }          
        }

        public static IDTSInputColumn100 GetInputColumnByLineageId(IDTSInputColumnCollection100 inputColumns, int lineageId)
        {

            IDTSInputColumn100 result = null;

            for (int i = 0; i < inputColumns.Count; i++)
            {
                if (inputColumns[i].LineageID == lineageId) result = inputColumns[i];
            }

            return result;
        }

        public static IDTSInputColumn100 GetInputIdByColumnName(string columnName, IDTSComponentMetaData100 componentMetaData)
        {

            foreach (IDTSInputColumn100 column in componentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection)
            {
                if (column.Name == columnName) return column;
            }

            return null;
        }

        public static Dictionary<string, IDTSInputColumn100> GetInputDictionary(IDTSComponentMetaData100 componentMetaData)
        {
            Dictionary<string, IDTSInputColumn100> result = new Dictionary<string, IDTSInputColumn100>();
            foreach (IDTSInputColumn100 column in componentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection)
            {
                result.Add(column.Name, column);
            }

            return result;
        }

        public static IDTSOutputColumn100 GetOutputColumnByColumnName(string columnName, IDTSOutputColumnCollection100 outputColumns)
        {

            foreach (IDTSOutputColumn100 column in outputColumns)
            {
                if (column.Name == columnName) return column;
            }

            return null;
        }

        public static bool HasVirtualInputColumn(IDTSVirtualInput100 vInput, int lineageId)
        {
            bool result = false;

            foreach (IDTSVirtualInputColumn100 col in vInput.VirtualInputColumnCollection)
            {
                if (col.LineageID == lineageId) result = true;
            }

            return result;
        }

        public static void SetUsageTypeReadOnly(IDTSVirtualInput100 virtualInput)
        {
            for (int i = 0; i < virtualInput.VirtualInputColumnCollection.Count; i++)
            {
                virtualInput.SetUsageType(virtualInput.VirtualInputColumnCollection[i].LineageID, DTSUsageType.UT_READONLY);
            }
        }

        public static void RemoveCollections(IDTSComponentMetaData100 componentMetaData)
        {
            try
            {
                componentMetaData.OutputCollection[Constants.OUTPUT_ERROR_NAME].OutputColumnCollection.RemoveAll();
            }
            catch (Exception)
            {
                //Die Spalten ErrorCode und ErrorColumn können nicht entfernt werden
            }
            componentMetaData.OutputCollection[Constants.OUTPUT_NAME].OutputColumnCollection.RemoveAll();

            componentMetaData.InputCollection[Constants.INPUT_NAME].InputColumnCollection.RemoveAll();
        }

        /// <summary>
        /// Metadaten Version auf DLL-Version setzen 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="componentMetaData"></param>
        public static void UpdateVersion(PipelineComponent component, IDTSComponentMetaData100 componentMetaData)
        {
            DtsPipelineComponentAttribute componentAttr =
                 (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(component.GetType(), typeof(DtsPipelineComponentAttribute), false);
            int binaryVersion = componentAttr.CurrentVersion;
            componentMetaData.Version = binaryVersion;
        }

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

        public static string GetValueFromVariable(IDTSVariableDispenser100 variableDispenser, string variableName)
        {
            string result;

            IDTSVariables100 var = null;
            variableDispenser.LockOneForRead(variableName, ref var);
            result = var[variableName].Value.ToString();
            var.Unlock();

            return result;
        }

        public static bool CanConvertTo(object value, DataType dataType, int length, int scale, int precision, out string errorMessage)
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
                        DateTime d = (DateTime)value;
                        return true;
                    case DataType.DT_DBTIME:
                        MessageBox.Show("Typ " + dataType + " wird nicht unterstützt.");
                        return false;
                    case DataType.DT_DBTIMESTAMP:
                        Convert.ToDateTime(value);
                        return true;
                    case DataType.DT_DECIMAL:
                        if (!IsValidDecimal(Convert.ToDecimal(value), scale, 0))
                            throw new Exception(String.Format("Konvertierung nach DT.Decimal [Scale={0}] gescheitert.", scale));
                        return true;
                    case DataType.DT_FILETIME:
                        throw new Exception("Typ " + dataType.ToString() + " wird nicht unterstützt.");
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
                        BlobColumn colDT_NTEXT = (BlobColumn)value;
                        if (colDT_NTEXT.IsNull) throw new Exception();
                        else Convert.ToInt32(colDT_NTEXT.Length);
                        return true;
                    case DataType.DT_NULL:
                        return (value == null);
                    case DataType.DT_NUMERIC:
                        if (!IsValidDecimal(Convert.ToDecimal(value), scale, precision)) 
                            throw new Exception(String.Format("Konvertierung nach DT.Numeric [Scale={0}, Precision={1}] gescheitert.", scale, precision));
                        return true;
                    case DataType.DT_R4:
                        Convert.ToSingle(value);
                        return true;
                    case DataType.DT_R8:
                        Convert.ToDouble(value);
                        return true;
                    case DataType.DT_STR:
                        string s = value.ToString();
                        if (s.Length > length) throw new Exception(string.Format("Konvertierung nach DT_STR[Length={0}] gescheitert.", length));
                        return true;
                    case DataType.DT_TEXT:
                        BlobColumn colDT_TEXT = (BlobColumn)value;
                        if (colDT_TEXT.IsNull) throw new Exception();
                        else Convert.ToInt32(colDT_TEXT.Length);
                        return true;
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
                        if (ws.Length > length) throw new Exception(string.Format("Konvertierung nach DT_WSTR [Length={0}] gescheitert.", length));
                        return true;
                    default:
                        throw new Exception("Typ " + dataType.ToString() + " wird nicht unterstützt.");
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            
        }
        /// <summary>
        /// Prüft ob ein Decimal Wert einen Scale von "scale" und eine Precision von "precision" hat. 
        /// (precision = 0 gibt an, dass jede precision gültig ist)
        /// </summary>
        /// <param name="value">the decimal to validate</param>
        /// <param name="scale">number of digits to the right of the decimal point</param>
        /// <param name="precision">the number of digits in a number</param>
        /// <returns></returns>
        /// 
        private static bool IsValidDecimal(decimal value, int scale, int precision)
        {
            value = Math.Abs(value); //Negatives Vorzeichen entfernen

            if (precision != 0)
            {
                int digitsCount = value.ToString().Contains(",") ? value.ToString().Length - 1 : value.ToString().Length;
                if (digitsCount > precision) return false;
            }

            decimal floor = (value - Math.Floor(value)); //Umwandeln in 0.xxx oder 0
            if (floor.ToString().Length > 2 && //auf 0 ohne Nachkommastelle prüfen
                floor.ToString().Length > scale + 2) return false;

            return true;
        }
    }
}
