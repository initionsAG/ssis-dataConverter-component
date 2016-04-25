using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline;
using System.Globalization;

namespace DataConverter
{
    /// <summary>
    /// Holds methods for conversions
    /// </summary>
    public static class Converter
    {
        public static CultureInfo USED_CULTURE = CultureInfo.CurrentCulture;

        /// <summary>
        /// Convertes a string with a given format to a string with format YYYYMMDD
        /// </summary>
        /// <param name="dataTypeKind">Conversion types datatype characteristic</param>
        /// <param name="outputDataType">output datatype</param>
        /// <param name="posY">index of year (inside string of value)</param>
        /// <param name="posM">index of month (inside string of value)</param>
        /// <param name="posD">index of day (inside string of value)</param>
        /// <param name="posSplitterFirst">index of first splitter (inside string of value)</param>
        /// <param name="posSplitterSecond">index of second splitter (inside string of value)</param>
        /// <param name="splitterFirst">first splitter</param>
        /// <param name="splitterSecond">second splitter</param>
        /// <param name="value">value to convert</param>
        /// <param name="status">filled if conversion fails</param>
        /// <returns>string with format YYYYMMDD</returns>
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

                DateTime date = new DateTime(Convert.ToInt32(year, USED_CULTURE), Convert.ToInt32(month, USED_CULTURE), Convert.ToInt32(day, USED_CULTURE));

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

        /// <summary>
        /// Converts an int to a date
        /// (int must have format YYYYMMDD)
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="status">filled if conversion fails</param>
        /// <returns>date</returns>
        public static object IntToDate(object value, ref StatusConvert status)
        {
            object result = null;

            int date = 0;
            try
            {
                date = Convert.ToInt32(value, USED_CULTURE);
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

        /// <summary>
        /// Converts an a date to a specified number datatype
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="dataType">output datatype</param>
        /// <param name="status">filled if conversion fails</param>
        /// <returns>number with the specified datatype</returns>
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
                        return Convert.ToInt32(value, USED_CULTURE);
                    case DataType.DT_I8:
                        return Convert.ToInt64(value, USED_CULTURE);
                    case DataType.DT_UI4:
                        return Convert.ToUInt32(value, USED_CULTURE);
                    case DataType.DT_UI8:
                        return Convert.ToUInt64(value, USED_CULTURE);
                    case DataType.DT_NUMERIC:
                        return Convert.ToDouble(value, USED_CULTURE);
                }
            }
            catch (Exception ex)
            {

                status.SetError(ex.ToString());
            }

            return null;
        }

        /// <summary>
        /// Converts a date to string with a given format (YYYYMMDD, YYYYMM, YYYY)
        /// </summary>
        /// <param name="value">date to convert</param>
        /// <param name="date2stringType">output format (YYYYMMDD, YYYYMM, YYYY)</param>
        /// <param name="status">filled if conversion fails</param>
        /// <returns>converted value as string</returns>
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
                    case DateConvertTypes.HHMM:
                        return date.Hour.ToString().PadLeft(2, '0') + ":" + date.Minute.ToString().PadLeft(2, '0');
                    case DateConvertTypes.HHMMSS:
                        return date.Hour.ToString().PadLeft(2, '0') + ":" + date.Minute.ToString().PadLeft(2, '0') + ":" + date.Second.ToString().PadLeft(2, '0');
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
        /// Converts a string into a number using a conversion rule
        /// </summary>
        /// <param name="value">string to convert</param>
        /// <param name="date2stringType">conversion rule</param>
        /// <param name="dataType">output datatype (DT_I4, DT_I8, DT_UI4, DT_UI8 or DT_NUMERIC) </param>
        /// <returns>converted value</returns>
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

        /// <summary>
        /// Converts a value to a specified datatype
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="dataType">output datatype</param>
        /// <param name="status">conversion status</param>
        /// <param name="convertFromString">Is input datatype == string?</param>
        /// <returns>converted value</returns>
        public static object GetConvertedValue(object value, DataType dataType, ref StatusConvert status, bool convertFromString)
        {
            return GetConvertedValue(value, dataType, ref status, convertFromString, string.Empty);
        }
        /// <summary>
        /// Converts a value to a specified datatype
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="dataType">output datatype</param>
        /// <param name="status">conversion status</param>
        /// <param name="convertFromString">Is input datatype == string?</param>
        /// <returns>converted value</returns>
        public static object GetConvertedValue(object value, DataType dataType, ref StatusConvert status, bool convertFromString, string codepage)
        {
            if (value == null) return null;
            else
            {
                try
                {
                    switch (dataType)
                    {
                        case DataType.DT_BOOL:
                            return Convert.ToBoolean(value, USED_CULTURE);
                        case DataType.DT_BYTES:
                            return (byte[])value;
                        case DataType.DT_CY:
                            return Convert.ToDecimal(value, USED_CULTURE);
                        case DataType.DT_DATE:
                            return Convert.ToDateTime(value, USED_CULTURE);
                        case DataType.DT_DBDATE:
                            return Convert.ToDateTime(value, USED_CULTURE);
                        case DataType.DT_DBTIME:
                            MessageBox.Show("Typ " + dataType + " wird nicht unterstützt.");
                            return null;
                        case DataType.DT_DBTIMESTAMP:
                            return Convert.ToDateTime(value, USED_CULTURE);
                        case DataType.DT_DECIMAL:

                            if (convertFromString)
                            {
                                decimal result;
                                if (decimal.TryParse((string)value, out result)) return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Decimal.");
                            }
                            else
                                return Convert.ToDecimal(value, USED_CULTURE);

                            break;

                        case DataType.DT_FILETIME:
                            return value;
                        case DataType.DT_GUID:
                            return (Guid)value;
                        case DataType.DT_I1:

                            if (convertFromString)
                            {
                                SByte result;
                                if (SByte.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to SByte.");
                            }
                            else
                                return Convert.ToSByte(value, USED_CULTURE);

                            break;

                        case DataType.DT_I2:

                            if (convertFromString)
                            {
                                Int16 result;
                                if (Int16.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int16.");
                            }
                            else
                                return Convert.ToInt16(value, USED_CULTURE);

                            break;

                        case DataType.DT_I4:

                            if (convertFromString)
                            {
                                Int32 result;
                                if (Int32.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int32.");
                            }
                            else return Convert.ToInt32(value);

                            break;
                        case DataType.DT_I8:

                            if (convertFromString)
                            {
                                Int64 result;
                                if (Int64.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Int64.");
                            }
                            else
                                return Convert.ToInt64(value, USED_CULTURE);

                            break;
                        case DataType.DT_IMAGE:
                            BlobColumn colDT_IMAGE = (BlobColumn)value;
                            if (colDT_IMAGE.IsNull) return null;
                            else
                                return Convert.ToInt32(colDT_IMAGE.Length, USED_CULTURE);
                        case DataType.DT_NTEXT:                          
                            if (value == null)
                                return new byte[0];
                            else
                            {
                                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(value.ToString());
                                return bytes;
                            }
                        case DataType.DT_NULL:
                            return null;
                        case DataType.DT_NUMERIC:

                            if (convertFromString)
                            {
                                decimal result;
                                if (decimal.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Decimal.");
                            }
                            else
                                return Convert.ToDecimal(value, USED_CULTURE);

                            break;

                        case DataType.DT_R4:

                            if (convertFromString)
                            {
                                Single result;
                                if (Single.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Single.");
                            }
                            else 
                                return Convert.ToSingle(value, USED_CULTURE);

                            break;

                        case DataType.DT_R8:

                            if (convertFromString)
                            {
                                Double result;
                                if (Double.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Double.");
                            }
                            else 
                                return Convert.ToDouble(value, USED_CULTURE);

                            break;
                        case DataType.DT_STR:
                            return value.ToString();
                        case DataType.DT_TEXT:
                            if (value == null)
                                return new byte[0];
                            else
                            {
                                byte[] bytes = System.Text.Encoding.GetEncoding(Int32.Parse(codepage)).GetBytes(value.ToString());
                                return bytes;
                            }
                        case DataType.DT_UI1:

                            if (convertFromString)
                            {
                                Byte result;
                                if (Byte.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to Byte.");
                            }
                            else 
                                return Convert.ToByte(value, USED_CULTURE);

                            break;

                        case DataType.DT_UI2:

                            if (convertFromString)
                            {
                                UInt16 result;
                                if (UInt16.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt16.");
                            }
                            else
                                return Convert.ToUInt16(value, USED_CULTURE);

                            break;

                        case DataType.DT_UI4:

                            if (convertFromString)
                            {
                                UInt32 result;
                                if (UInt32.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt32.");
                            }
                            else
                                return Convert.ToUInt32(value, USED_CULTURE);

                            break;

                        case DataType.DT_UI8:

                            if (convertFromString)
                            {
                                UInt64 result;
                                if (UInt64.TryParse((string) value, NumberStyles.Any, USED_CULTURE, out result))
                                    return result;
                                else status.SetError("Cannot Convert the value " + value.ToString() + " to UInt64.");
                            }
                            else
                                return Convert.ToUInt64(value, USED_CULTURE);

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
