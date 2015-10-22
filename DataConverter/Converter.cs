using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline;

namespace DataConverter
{
    /// <summary>
    /// Holds methods for conversions
    /// </summary>
    public static class Converter
    {

        public static object String2YearMonthDayByFormat(DataTypeKind dataTypeKind, DataType outputDataType,
            int posY, int posM, int posD, int posSplitterFirst, int posSplitterSecond, string splitterFirst, string splitterSecond,
            object value, ref StatusConvert status)
        {
            object result;

            try
            {
                string valueStr = value.ToString();

                string year = valueStr.Substring(posY, 4);
                string month = valueStr.Substring(posM, 2);
                string day = valueStr.Substring(posD, 2);

                DateTime date = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                switch (dataTypeKind)
                {
                    case DataTypeKind.TextDate:
                        result = year + month + day;
                        break;
                    case DataTypeKind.NumberDate:
                        result = GetConvertedValue(year + month + day, outputDataType, ref status, true);
                        break;
                    case DataTypeKind.Date:
                        return date;
                        break;
                    case DataTypeKind.None:
                        break; 
                    default:
                        break;
                }


                result = valueStr.Substring(posY, 4) + valueStr.Substring(posM, 2) + valueStr.Substring(posD, 2);
                if (valueStr.Substring(posSplitterFirst, 1) != splitterFirst || valueStr.Substring(posSplitterSecond, 1) != splitterSecond)
                {
                    status.SetError("Error: Cannot convert String to String by using the conversion format.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                status.SetError("Error (Convert String to String with Format): " + ex.ToString());
                return null;
            }

          
            return result;

        }

        public static object IntToDate(object value, ref StatusConvert status)
        {
            object result = null;

            int date = 0;
            try
            {
                date = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                status.SetError(ex.ToString());
                return null;
            }

            if (date < 10000000 || date > 99999999) status.SetError("Convert Integer to Date: Length of integer must be exactly 8");
            else
            {
                try
                {
                    result = new DateTime(date / 10000, date / 100 % 100, date % 100);
                }
                catch (Exception ex)
                {
                    status.SetError("Error (Convert Integer to Date): " + ex.ToString());
                    return null;
                }
            }

            return result;
        }

        public static object DateToInt(object value, DataType dataType, ref StatusConvert status)
        {
            try
            {
                DateTime date = (DateTime)value;

                string month = date.Month.ToString();
                if (month.Length == 1) month = "0" + month;

                string day = date.Day.ToString();
                if (day.Length == 1) day = "0" + day;

                value = date.Year.ToString() + month + day;

                switch (dataType)
                {
                    case DataType.DT_I4:
                        return Convert.ToInt32(value);
                    case DataType.DT_I8:
                        return Convert.ToInt64(value);
                    case DataType.DT_UI4:
                        return Convert.ToUInt32(value);
                    case DataType.DT_UI8:
                        return Convert.ToUInt64(value);
                    case DataType.DT_NUMERIC:
                        return Convert.ToDouble(value);
                }
            }
            catch (Exception ex)
            {

                status.SetError(ex.ToString());
            }



            return null;

        }

        public static object DateToString(object value, DateConvertTypes date2stringType, ref StatusConvert status)
        {
            try
            {
                DateTime date = (DateTime)value;

                string month = date.Month.ToString();
                if (month.Length == 1) month = "0" + month;

                string day = date.Day.ToString();
                if (day.Length == 1) day = "0" + day;

                switch (date2stringType)
                {
                    case DateConvertTypes.None:
                        break;
                    case DateConvertTypes.YYYYMMDD:
                        return date.Year.ToString() + month + day;
                    case DateConvertTypes.YYYYMM:
                        return date.Year.ToString() + month;
                    case DateConvertTypes.YYYY:
                        return date.Year.ToString();
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                status.SetError(ex.ToString());
            }


            return null;

        }

        /// <summary>
        /// Konvertiert einen string nach einer ausgewählten Regel in eine Zahl
        /// </summary>
        /// <param name="value">Der zu konvertierende string</param>
        /// <param name="date2stringType">Die Konvertierungsregel</param>
        /// <param name="dataType">Der Zieldatentyp (muss DT_I4, DT_I8, DT_UI4, DT_UI8 oder DT_NUMERIC sein) </param>
        /// <returns>den konvertierten Wert</returns>
        public static object String2Numeric(object value, DateConvertTypes date2stringType, DataType dataType, ref StatusConvert status)
        {
            if (dataType != DataType.DT_I4 && dataType != DataType.DT_I8 && dataType != DataType.DT_UI4 && dataType != DataType.DT_UI8 && dataType != DataType.DT_NUMERIC)
                throw new Exception("Fehler: ConvertToNumeric erwartet einen folgender Zieldatentypen: DT_I4, DT_I8, DT_UI4, DT_UI8, DT_NUMERIC.");

            string strValue = value.ToString();

            switch (date2stringType)
            {
                case DateConvertTypes.Point2Comma:
                    strValue = strValue.Replace(".", ",");
                    break;
                case DateConvertTypes.Comma2Point:
                    strValue = strValue.Replace(",", ".");
                    break;
                case DateConvertTypes.AmericanDecimal:
                    strValue = strValue.Replace(",", "");
                    strValue = strValue.Replace(".", ",");
                    break;
                case DateConvertTypes.GermanDecimal:
                    strValue = strValue.Replace(".", "");
                    break;
                default:
                    break;
            }

            object result = Converter.GetConvertedValue(strValue, DataType.DT_NUMERIC, ref status, true);
            if (dataType != DataType.DT_NUMERIC && result != null)
            {
                try
                {
                    result = Decimal.Ceiling(((decimal)result));
                }
                catch (Exception) //Ceiling sprengt den Wertebereich von Decimal
                {
                    status.SetError("Can convert the value " + value.ToString() + " to decimal but not to " + dataType.ToString());
                }

                result = Converter.GetConvertedValue(result.ToString(), dataType, ref status, true);
                if (result == null) status.SetError("Can convert the value " + value.ToString() + " to decimal but not to " + dataType.ToString());
            }

            return result;
        }


        public static object GetConvertedValue(object value, DataType dataType, ref StatusConvert status, bool convertFromString)
        {
            if (value == null) return null;
            else
            {
                try
                {
                    switch (dataType)
                    {
                        case DataType.DT_BOOL:
                            return Convert.ToBoolean(value);
                        case DataType.DT_BYTES:
                            return (byte[])value;
                        case DataType.DT_CY:
                            return Convert.ToDecimal(value);
                        case DataType.DT_DATE:
                            return Convert.ToDateTime(value);
                        case DataType.DT_DBDATE:
                            return (DateTime)value;
                        case DataType.DT_DBTIME:
                            MessageBox.Show("Typ " + dataType + " wird nicht unterstützt.");
                            return null;
                        case DataType.DT_DBTIMESTAMP:
                            return Convert.ToDateTime(value);
                        case DataType.DT_DECIMAL:

                            if (convertFromString)
                            {
                                decimal result;
                                if (decimal.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Decimal.");
                            }
                            else return Convert.ToDecimal(value);

                            break;

                        case DataType.DT_FILETIME:
                            return value;
                        case DataType.DT_GUID:
                            return (Guid)value;
                        case DataType.DT_I1:

                            if (convertFromString)
                            {
                                SByte result;
                                if (SByte.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to SByte.");
                            }
                            else return Convert.ToSByte(value);

                            break;

                        case DataType.DT_I2:

                            if (convertFromString)
                            {
                                Int16 result;
                                if (Int16.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int16.");
                            }
                            else return Convert.ToInt16(value);

                            break;

                        case DataType.DT_I4:

                            if (convertFromString)
                            {
                                Int32 result;
                                if (Int32.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int32.");
                            }
                            else return Convert.ToInt32(value);

                            break;
                        case DataType.DT_I8:

                            if (convertFromString)
                            {
                                Int64 result;
                                if (Int64.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int64.");
                            }
                            else return Convert.ToInt64(value);

                            break;
                        case DataType.DT_IMAGE:
                            BlobColumn colDT_IMAGE = (BlobColumn)value;
                            if (colDT_IMAGE.IsNull) return null;
                            else return Convert.ToInt32(colDT_IMAGE.Length);
                        case DataType.DT_NTEXT:
                            BlobColumn colDT_NTEXT = (BlobColumn)value; //CType(value, BlobColumn)
                            if (colDT_NTEXT.IsNull) return null;
                            else return Convert.ToInt32(colDT_NTEXT.Length);
                        case DataType.DT_NULL:
                            return null;
                        case DataType.DT_NUMERIC:

                            if (convertFromString)
                            {
                                decimal result;
                                if (decimal.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Decimal.");
                            }
                            else return Convert.ToDecimal(value);

                            break;

                        case DataType.DT_R4:

                            if (convertFromString)
                            {
                                Single result;
                                if (Single.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Single.");
                            }
                            else return Convert.ToSingle(value);

                            break;

                        case DataType.DT_R8:

                            if (convertFromString)
                            {
                                Double result;
                                if (Double.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Double.");
                            }
                            else return Convert.ToDouble(value);

                            break;

                        case DataType.DT_STR:
                            return value.ToString();
                        case DataType.DT_TEXT:
                            BlobColumn colDT_TEXT = (BlobColumn)value;
                            if (colDT_TEXT.IsNull) return null;
                            else return Convert.ToInt32(colDT_TEXT.Length);
                        case DataType.DT_UI1:

                            if (convertFromString)
                            {
                                Byte result;
                                if (Byte.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Byte.");
                            }
                            else return Convert.ToByte(value);

                            break;

                        case DataType.DT_UI2:

                            if (convertFromString)
                            {
                                UInt16 result;
                                if (UInt16.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt16.");
                            }
                            else return Convert.ToUInt16(value);

                            break;

                        case DataType.DT_UI4:

                            if (convertFromString)
                            {
                                UInt32 result;
                                if (UInt32.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt32.");
                            }
                            else return Convert.ToUInt32(value);

                            break;

                        case DataType.DT_UI8:

                            if (convertFromString)
                            {
                                UInt64 result;
                                if (UInt64.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt64.");
                            }
                            else return Convert.ToUInt64(value);

                            break;

                        case DataType.DT_WSTR:
                            return value.ToString();
                        default:
                            MessageBox.Show("Typ " + dataType + " wird nicht unterstützt.");
                            break;
                    }
                }
                catch (Exception ex)
                {

                    status.SetError(ex.ToString());
                }
            }


            return null;
        }
    }
}
