using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Collections;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using DataConverter.FrameWork.Mapping;

namespace DataConverter
{
    public enum DateConvertTypes { None, YYYYMMDD, YYYYMM, YYYY, Point2Comma, Comma2Point, AmericanDecimal, GermanDecimal, STR2YYYYMMDD }
    public enum DataTypeKind { TextDate, NumberDate, Date, None };

    public class ColumnConfig: IXmlSerializable, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region Properties

        private string _inputColumnName;
        [DisplayName("Input Column Name"), ReadOnly(true)]
        public string InputColumnName
        {
            get { return _inputColumnName; }
            set { _inputColumnName = value; }
        }

        private bool _convert;
        [DisplayName("Convert")]
        public bool Convert
        {
            get { return _convert; }
            set
            {
                _convert = value;
                if (_convert)
                {
                    //_use = true;
                    if (string.IsNullOrEmpty(_outputAlias))
                        _outputAlias = "C_" + InputColumnName;
                }
                NotifyPropertyChanged("Convert");

            }
        }

        private string _outputAlias;
        [DisplayName("Output Alias")]
        public string OutputAlias
        {
            get { return _outputAlias; }
            set
            {
                _outputAlias = value;
                NotifyPropertyChanged("OutputAlias");
            }
        }

        private string _dataTypeInput;
        [DisplayName("DataType (Input)"), ReadOnly(true)]
        public string DataTypeInput
        {
            get { return _dataTypeInput; }
            set
            {
                _dataTypeInput = value;
                UpdateConversion();

                NotifyPropertyChanged("DataTypeInput");
                NotifyPropertyChanged("SupportedConversions");
            }
        }

        private string _dataType;
        [DisplayName("DataType (Output)")]
        public string DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                if (!HasLength())
                    Length = "0";
                if (!HasPrecision())
                    Precision = "0";
                if (!HasScale())
                    Scale = "0";
                if (!HasCodePage())
                    Codepage = "0";
                UpdateConversion();

                NotifyPropertyChanged("DataType");
                NotifyPropertyChanged("SupportedConversions");     
            }
        }

        private string _default;
        [DisplayName("OnNull")]
        public string Default
        {
            get { return _default; }
            set
            {
                _default = value;
                NotifyPropertyChanged("Default");
            }
        }

        private string _length;
        [DisplayName("Length")]
        public string Length
        {
            get { return _length; }
            set { _length = value; NotifyPropertyChanged("Length"); }
        }

        private string _precision;
        [DisplayName("Precision")]
        public string Precision
        {
            get { return _precision; }
            set { _precision = value; NotifyPropertyChanged("Precision"); }
        }

        private string _scale;
        [DisplayName("Scale")]
        public string Scale
        {
            get { return _scale; }
            set { _scale = value; NotifyPropertyChanged("Scale"); }
        }

        private string _codepage;
        [DisplayName("Codepage")]
        public string Codepage
        {
            get { return _codepage; }
            set { _codepage = value; NotifyPropertyChanged("Codepage"); }
        }


        private DateConvertTypes _date2string;
        [BrowsableAttribute(false)]
        public DateConvertTypes Date2string
        {
            get { return _date2string; }
            set
            {
                _date2string = value;
                NotifyPropertyChanged("Date2string");
            }
        }

        private string _strConversionByFormat;
        [BrowsableAttribute(false)]
        public string StrConversionByFormat
        {
            get { return _strConversionByFormat == null ? string.Empty : _strConversionByFormat; }
            set
            {
                _strConversionByFormat = value == null ? string.Empty : value;
                NotifyPropertyChanged("StrConversionByFormat");
            }
        }

        [DisplayName("Conversion")]
        public string ConversionAsString
        {
            get
            {
                if (Date2string == DateConvertTypes.STR2YYYYMMDD)
                    return StrConversionByFormat;
                else
                    return Date2string.ToString();
            }

            set
            {
                if (Enum.IsDefined(typeof(DateConvertTypes), value))
                {
                    Date2string = (DateConvertTypes) Enum.Parse(typeof(DateConvertTypes), value);
                    StrConversionByFormat = "";
                }
                else if (SupportsConversionStrByFormat)
                {
                    Date2string = DateConvertTypes.STR2YYYYMMDD;
                    StrConversionByFormat = value;
                }
                else
                {
                    Date2string = DateConvertTypes.None;
                    StrConversionByFormat = "";
                }

                NotifyPropertyChanged("ConversionAsString");
            }
        }

        private string _regEx;
        [DisplayName("RegEx")]
        public string RegEx
        {
            get { return _regEx; }
            set { _regEx = value; NotifyPropertyChanged("RegEx"); }
        }

        private string _compare;
        [DisplayName("Compare")]
        public string Compare
        {
            get { return _compare; }
            set { _compare = value; NotifyPropertyChanged("Compare"); }
        }



        private string _onErrorValue;
        [DisplayName("OnError")]
        public string OnErrorValue
        {
            get { return _onErrorValue; }
            set
            {
                _onErrorValue = value;
                if (HasOnErrorValue() && ErrorHandling == IsagCustomProperties.ErrorRowHandling.FailComponent)
                    _errorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

                if (!HasOnErrorValue() && ErrorHandling == IsagCustomProperties.ErrorRowHandling.IgnoreFailure)
                    _errorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

                NotifyPropertyChanged("OnErrorValue");
            }

        }

        [DisplayName("Allow Null")]
        public bool AllowNull { get; set; }

        private IsagCustomProperties.ErrorRowHandling _errorHandling;
        [DisplayName("ErrorHandling")]
        public IsagCustomProperties.ErrorRowHandling ErrorHandling
        {
            get { return _errorHandling; }
            set
            {
                _errorHandling = value;
                if (HasOnErrorValue() && ErrorHandling == IsagCustomProperties.ErrorRowHandling.FailComponent)
                    _errorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

                if (!HasOnErrorValue() && ErrorHandling == IsagCustomProperties.ErrorRowHandling.IgnoreFailure)
                    _errorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;

                NotifyPropertyChanged("AllowNull");
            }
        }


        private bool _isErrorCounter;
        [DisplayName("IsErrorCounter")]
        public bool IsErrorCounter
        {
            get { return _isErrorCounter; }
            set
            {
                _isErrorCounter = value;
                if (_isErrorCounter)
                    Convert = false;

                NotifyPropertyChanged("IsErrorCounter");
            }
        }

        private string _inputId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string InputId
        {
            get { return _inputId; }
            set { _inputId = value; }
        }
        private string _inputIdString;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string InputIdString
        {
            get { return _inputIdString; }
            set { _inputIdString = value; }
        }
        private string _inputLineageId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string InputLineageId
        {
            get { return _inputLineageId; }
            set { _inputLineageId = value; }
        }

        private string _outputId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputId
        {
            get { return _outputId; }
            set { _outputId = value; }
        }
        private string _outputIdString;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputIdString
        {
            get { return _outputIdString; }
            set { _outputIdString = value; }
        }
        private string _outputLineageId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputLineageId
        {
            get { return _outputLineageId; }
            set { _outputLineageId = value; }
        }

        private string _outputErrorId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorId
        {
            get { return _outputErrorId; }
            set { _outputErrorId = value; }
        }
        private string _outputErrorIdString;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorIdString
        {
            get { return _outputErrorIdString; }
            set { _outputErrorIdString = value; }
        }
        private string _outputErrorLineageId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorLineageId
        {
            get { return _outputErrorLineageId; }
            set { _outputErrorLineageId = value; }
        }

        private string _customId;
        [BrowsableAttribute(false), ReadOnly(true)]
        public string CustomId
        {
            get { return _customId; }
            set { _customId = value; }
        }



        #region Conversion



        [BrowsableAttribute(false), ReadOnly(true)]
        public DataTypeKind OutputDataTypeKindForDate
        {
            get
            {

                if ((DataType == "DT_NUMERIC" ||
                      DataType == "DT_I4" || DataType == "DT_I8" ||
                      DataType == "DT_UI4" || DataType == "DT_UI8"))
                    return DataTypeKind.NumberDate;
                else if ((DataType.StartsWith("DT_DATE") || DataType.StartsWith("DT_DBDATE") || DataType.StartsWith("DT_DBTIMESTAMP")))
                    return DataTypeKind.Date;
                else if (DataType == "DT_WSTR" || DataType == "DT_STR")
                    return DataTypeKind.TextDate;

                return DataTypeKind.None;
            }
        }

        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversion
        {
            get { return (SupportsConversionDate || SupportsConversionNumeric || SupportsConversionStrByFormat) && !IsErrorCounter && Convert; }
        }

        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversionDate
        {
            get
            {
                return (DataType == "DT_WSTR" || DataType == "DT_STR") &&
                       (DataTypeInput.StartsWith("DT_DATE") || DataTypeInput.StartsWith("DT_DBDATE") || DataTypeInput.StartsWith("DT_DBTIMESTAMP"));
            }
        }

        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversionNumeric
        {
            get
            {
                return (DataType == "DT_NUMERIC" ||
                       DataType == "DT_I1" || DataType == "DT_I2" || DataType == "DT_I4" || DataType == "DT_I8" ||
                       DataType == "DT_UI1" || DataType == "DT_UI2" || DataType == "DT_UI4" || DataType == "DT_UI8") &&
                       (DataTypeInput.StartsWith("DT_WSTR") || DataTypeInput.StartsWith("DT_STR"));
            }
        }

        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversionStrByFormat
        {
            get
            {
                return (DataTypeInput.StartsWith("DT_WSTR") || DataTypeInput.StartsWith("DT_STR")) &&
                       (DataType.StartsWith("DT_WSTR") || DataType.StartsWith("DT_STR") ||
                        DataType == "DT_UI4" || DataType == "DT_UI8" ||
                        DataType == "DT_I4" || DataType == "DT_I8" || DataType == "DT_NUMERIC" ||
                        DataType.StartsWith("DT_DATE") || DataType.StartsWith("DT_DBDATE") || DataType.StartsWith("DT_DBTIMESTAMP"));
            }
        }
        [BrowsableAttribute(false), ReadOnly(true)]

        BindingList<object> _supportedConversions;
        [BrowsableAttribute(false), ReadOnly(true), XmlIgnore]
        public BindingList<object> SupportedConversions
        {
            get
            {
                if (_supportedConversions == null)
                    _supportedConversions = new BindingList<object>();

                _supportedConversions.RaiseListChangedEvents = false;
                _supportedConversions.Clear();

                _supportedConversions.Add(DateConvertTypes.None.ToString());

                if (SupportsConversionDate)
                {
                    _supportedConversions.Add(DateConvertTypes.YYYY.ToString());
                    _supportedConversions.Add(DateConvertTypes.YYYYMM.ToString());
                    _supportedConversions.Add(DateConvertTypes.YYYYMMDD.ToString());
                }

                if (SupportsConversionNumeric)
                {
                    _supportedConversions.Add(DateConvertTypes.Point2Comma.ToString());
                    _supportedConversions.Add(DateConvertTypes.Comma2Point.ToString());
                    _supportedConversions.Add(DateConvertTypes.AmericanDecimal.ToString());
                    _supportedConversions.Add(DateConvertTypes.GermanDecimal.ToString());
                }

                if (SupportsConversionStrByFormat)
                {
                    foreach (string item in Constants.STRING_CONVERSION_TYPES)
                    {
                        _supportedConversions.Add(item);
                    }
                    //result.AddRange(Constants.STRING_CONVERSION_TYPES);
                }

                _supportedConversions.RaiseListChangedEvents = true;
                return _supportedConversions;

            }
        }


        private void UpdateConversion()
        {

            if (!SupportedConversions.Contains(Date2string) && !(SupportsConversionStrByFormat && Date2string == DateConvertTypes.STR2YYYYMMDD))
            {
                Date2string = DateConvertTypes.None;
            }

            if (!SupportsConversionStrByFormat)
                StrConversionByFormat = "";
        }


        #endregion

        [BrowsableAttribute(false)]
        public bool HasCompare
        {
            get { return Compare != null && Compare.Trim() != String.Empty; }
        }

        #endregion

        #region Constructor

        public ColumnConfig() { }

        public ColumnConfig(string inputColumnName, bool convert, string outputAlias, string dataTypeInput,
                           string dataType, string length, string precision, string scale, string codepage, string defaultValue,
                           string inputId, string inputIdString, string inputLineageId,
                           string outputId, string outputIdString, string outputLineageId,
                           string outpuErrortId, string outputErrorIdString, string outputErrorLineageId,
                           bool allowNull,
                           IDTSInputColumn100 inputCol, IDTSOutputColumn100 outCol, IDTSOutputColumn100 outLogCol)
        {
            Init(inputColumnName, convert, outputAlias, dataTypeInput,
                            dataType, length, precision, scale, codepage, defaultValue,
                            inputId, inputIdString, inputLineageId,
                            outputId, outputIdString, outputLineageId,
                            outpuErrortId, outputErrorIdString, outputErrorLineageId,
                            allowNull, DateConvertTypes.None,
                            inputCol, outCol, outLogCol);
        }
        public ColumnConfig(string inputColumnName, bool convert, string outputAlias, string dataTypeInput,
                            string dataType, string length, string precision, string scale, string codepage, string defaultValue,
                            string inputId, string inputIdString, string inputLineageId,
                            string outputId, string outputIdString, string outputLineageId,
                            string outpuErrortId, string outputErrorIdString, string outputErrorLineageId,
                            bool allowNull, DateConvertTypes dateConverttype,
                            IDTSInputColumn100 inputCol, IDTSOutputColumn100 outCol, IDTSOutputColumn100 outLogCol)
        {
            Init(inputColumnName, convert, outputAlias, dataTypeInput,
                            dataType, length, precision, scale, codepage, defaultValue,
                            inputId, inputIdString, inputLineageId,
                            outputId, outputIdString, outputLineageId,
                            outpuErrortId, outputErrorIdString, outputErrorLineageId,
                            allowNull, dateConverttype,
                            inputCol, outCol, outLogCol);
        }

        private void Init(string inputColumnName, bool convert, string outputAlias, string dataTypeInput,
                            string dataType, string length, string precision, string scale, string codepage, string defaultValue,
                            string inputId, string inputIdString, string inputLineageId,
                            string outputId, string outputIdString, string outputLineageId,
                            string outpuErrortId, string outputErrorIdString, string outputErrorLineageId,
                            bool allowNull, DateConvertTypes dateConverttype,
                            IDTSInputColumn100 inputCol, IDTSOutputColumn100 outCol, IDTSOutputColumn100 outLogCol)
        {
            // _use = use;
            _inputColumnName = inputColumnName;
            _convert = convert;
            _outputAlias = outputAlias;
            _dataTypeInput = CreateInputDataTypeString(dataTypeInput, length, precision, scale, codepage);
            _dataType = dataType;
            _length = length;
            _precision = precision;
            _scale = scale;
            _codepage = codepage;
            _default = defaultValue;
            AllowNull = allowNull;

            _inputId = inputId;
            _inputIdString = inputIdString;
            _inputLineageId = inputLineageId;

            _outputId = outputId;
            _outputIdString = outputIdString;
            _outputLineageId = outputLineageId;

            _outputErrorId = outpuErrortId;
            _outputErrorIdString = outputErrorIdString;
            _outputErrorLineageId = outputErrorLineageId;

            IsErrorCounter = false;
            ErrorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;
            OnErrorValue = "";

            _date2string = dateConverttype;
            _customId = Guid.NewGuid().ToString();

            Mapping.SetIdProperty(_customId, inputCol.CustomPropertyCollection);
            if (outCol != null)
                Mapping.SetIdProperty(_customId, outCol.CustomPropertyCollection);
            if (outLogCol != null)
                Mapping.SetIdProperty(_customId, outLogCol.CustomPropertyCollection);

        }
        public string CreateInputDataTypeString(string dataType, string length, string precision, string scale, string codepage)
        {
            return dataType + " (" + length + "," + precision + "," + scale + "," + codepage.ToString() + ")";
        }

        #endregion

        #region IXmlSerializable

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {

            reader.MoveToContent();

            try
            {
                // _use = (reader.GetAttribute("Use") == "True");
                _inputColumnName = reader.GetAttribute("InputColumnName");
                _convert = (reader.GetAttribute("Convert") == "True");
                _outputAlias = reader.GetAttribute("OutputAlias");
                _dataTypeInput = reader.GetAttribute("DataTypeInput");
                _dataType = reader.GetAttribute("DataType");
                _length = reader.GetAttribute("Length");
                _precision = reader.GetAttribute("Precision");
                _scale = reader.GetAttribute("Scale");
                _codepage = reader.GetAttribute("Codepage");
                _regEx = reader.GetAttribute("RegEx");
                _compare = reader.GetAttribute("Compare");
                _default = reader.GetAttribute("Default");
                OnErrorValue = reader.GetAttribute("OnErrorValue");
                AllowNull = (reader.GetAttribute("AllowNull") == "True");
                IsErrorCounter = (reader.GetAttribute("IsErrorCounter") == "True");

                try
                {
                    Date2string = (DateConvertTypes) Enum.Parse(typeof(DateConvertTypes), reader.GetAttribute("Date2string"));
                }
                catch (Exception)
                {
                    Date2string = DateConvertTypes.None;
                }

                StrConversionByFormat = reader.GetAttribute("StrConversionByFormat");

                try
                {
                    ErrorHandling = (IsagCustomProperties.ErrorRowHandling) Enum.Parse(typeof(IsagCustomProperties.ErrorRowHandling), reader.GetAttribute("ErrorHandling"));
                }
                catch (Exception)
                {
                    ErrorHandling = IsagCustomProperties.ErrorRowHandling.RedirectRow;
                }


                _inputId = reader.GetAttribute("InputId");
                _inputIdString = reader.GetAttribute("InputIdString");
                _inputLineageId = reader.GetAttribute("InputLineageId");

                _outputId = reader.GetAttribute("OutputId");
                _outputIdString = reader.GetAttribute("OutputIdString");
                _outputLineageId = reader.GetAttribute("OutputLineageId");

                _outputErrorId = reader.GetAttribute("OutputErrorId");
                _outputErrorIdString = reader.GetAttribute("OutputErrorIdString");
                _outputErrorLineageId = reader.GetAttribute("OutputErrorLineageId");

                _customId = reader.GetAttribute("CustomId");

                reader.Read();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                throw;
            }


        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {

            // writer.WriteAttributeString("Use", _use.ToString());
            writer.WriteAttributeString("InputColumnName", _inputColumnName);
            writer.WriteAttributeString("Convert", _convert.ToString());
            writer.WriteAttributeString("OutputAlias", _outputAlias);
            writer.WriteAttributeString("DataTypeInput", _dataTypeInput);
            writer.WriteAttributeString("DataType", _dataType);
            writer.WriteAttributeString("Length", _length);
            writer.WriteAttributeString("Precision", _precision);
            writer.WriteAttributeString("Scale", _scale);
            writer.WriteAttributeString("Codepage", _codepage);
            writer.WriteAttributeString("RegEx", _regEx);
            writer.WriteAttributeString("Compare", _compare);
            writer.WriteAttributeString("Default", _default);
            writer.WriteAttributeString("OnErrorValue", _onErrorValue);
            writer.WriteAttributeString("AllowNull", AllowNull.ToString());
            writer.WriteAttributeString("Date2string", _date2string.ToString());
            writer.WriteAttributeString("StrConversionByFormat", StrConversionByFormat.ToString());
            writer.WriteAttributeString("ErrorHandling", _errorHandling.ToString());
            writer.WriteAttributeString("IsErrorCounter", _isErrorCounter.ToString());

            writer.WriteAttributeString("InputId", _inputId);
            writer.WriteAttributeString("InputIdString", _inputIdString);
            writer.WriteAttributeString("InputLineageId", _inputLineageId);

            writer.WriteAttributeString("OutputId", _outputId);
            writer.WriteAttributeString("OutputIdString", _outputIdString);
            writer.WriteAttributeString("OutputLineageId", _outputLineageId);

            writer.WriteAttributeString("OutputErrorId", _outputErrorId);
            writer.WriteAttributeString("OutputErrorIdString", _outputErrorIdString);
            writer.WriteAttributeString("OutputErrorLineageId", _outputErrorLineageId);

            writer.WriteAttributeString("CustomId", _customId);


        }

        #endregion

        #region Helper

        public bool HasLength()
        {
            ArrayList length = new ArrayList(Constants.DATATYPE_LENGTH);
            ArrayList lengthCodepage = new ArrayList(Constants.DATATYPE_LENGTH_CODEPAGE);

            return (length.Contains(DataType) || lengthCodepage.Contains(DataType));
        }

        public bool HasPrecision()
        {
            ArrayList precisionScale = new ArrayList(Constants.DATATYPE_PRECISION_SCALE);

            return precisionScale.Contains(DataType);
        }

        public bool HasScale()
        {
            ArrayList precisionScale = new ArrayList(Constants.DATATYPE_PRECISION_SCALE);
            ArrayList scale = new ArrayList(Constants.DATATYPE_SCALE);

            return (precisionScale.Contains(DataType) || scale.Contains(DataType));
        }

        public bool HasCodePage()
        {
            ArrayList lengthCodepage = new ArrayList(Constants.DATATYPE_LENGTH_CODEPAGE);
            ArrayList codepage = new ArrayList(Constants.DATATYPE_CODEPAGE);


            return (lengthCodepage.Contains(DataType) || codepage.Contains(DataType));
        }

        private void ReactOnDataTypeChanged()
        {
            if (!HasLength())
                Length = "0";
            if (!HasPrecision())
                Precision = "0";
            if (!HasScale())
                Scale = "0";
            if (!HasCodePage())
                Codepage = "0";
        }

        //public bool HasOutput()
        //{
        //    return !string.IsNullOrEmpty(OutputId);
        //}

        //public bool HasErrorOutput()
        //{
        //    return !string.IsNullOrEmpty(OutputErrorId);
        //}

        public void RemoveOutput()
        {
            _outputId = "";
            _outputIdString = "";
            _outputLineageId = "";
        }

        public bool HasSameInputAndOutputDataType()
        {
            return (DataTypeInput == CreateInputDataTypeString(DataType, Length, Precision, Scale, Codepage));
        }

        public bool HasDefaultValue()
        {
            return (Default != null && Default != "");
        }


        public bool HasOnErrorValue()
        {
            return (OnErrorValue != null && OnErrorValue != "");
        }

        public bool HasRegEx()
        {
            return (RegEx != null && RegEx != "" && Convert);
        }

        #endregion
    }
}
