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
    /// <summary>
    /// DataConverter (special) conversion types
    /// </summary>
    public enum DateConvertTypes { None, YYYYMMDD, YYYYMM, YYYY, Point2Comma, Comma2Point, AmericanDecimal, GermanDecimal, STR2YYYYMMDD, HHMM, HHMMSS }

    /// <summary>
    /// Conversion types datatype characteristic
    /// </summary>
    public enum DataTypeKind { TextDate, NumberDate, Date, None };

    /// <summary>
    /// configuration for a conversion of an input column
    /// </summary>
    public class ColumnConfig: IXmlSerializable, INotifyPropertyChanged {
        
        /// <summary>
        /// interface for INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// triggers PropertyChangedEventHandler
        /// </summary>
        /// <param name="info">property name</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region Properties

        /// <summary>
        /// input column name
        /// </summary>
        private string _inputColumnName;
        /// <summary>
        /// input column name
        /// </summary>
        [DisplayName("Input Column Name"), ReadOnly(true)]
        public string InputColumnName
        {
            get { return _inputColumnName; }
            set { _inputColumnName = value; }
        }

        /// <summary>
        /// convert column value?
        /// </summary>
        private bool _convert;
        /// <summary>
        /// convert column value?
        /// </summary>
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

        /// <summary>
        /// output column name
        /// </summary>
        private string _outputAlias;
        /// <summary>
        /// output column name
        /// </summary>
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

        /// <summary>
        /// input datatype as string
        /// </summary>
        private string _dataTypeInput;
        /// <summary>
        /// input datatype as string
        /// </summary>
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

        /// <summary>
        /// output datatype as string
        /// </summary>
        private string _dataType;
        /// <summary>
        /// output datatype as string
        /// </summary>
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

        /// <summary>
        /// default value for null values
        /// </summary>
        private string _default;
        /// <summary>
        /// default value for null values
        /// </summary>
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

        /// <summary>
        /// output datatype length
        /// </summary>
        private string _length;
        /// <summary>
        /// output datatype length
        /// </summary>
        [DisplayName("Length")]
        public string Length
        {
            get { return _length; }
            set { _length = value; NotifyPropertyChanged("Length"); }
        }

        /// <summary>
        /// output datatype precision
        /// </summary>
        private string _precision;
        /// <summary>
        /// output datatype precision
        /// </summary>
        [DisplayName("Precision")]
        public string Precision
        {
            get { return _precision; }
            set { _precision = value; NotifyPropertyChanged("Precision"); }
        }

        /// <summary>
        /// output datatype scale
        /// </summary>
        private string _scale;
        /// <summary>
        /// output datatype scale
        /// </summary>
        [DisplayName("Scale")]
        public string Scale
        {
            get { return _scale; }
            set { _scale = value; NotifyPropertyChanged("Scale"); }
        }

        /// <summary>
        /// output datatype codepage
        /// </summary>
        private string _codepage;
        /// <summary>
        /// output datatype codepage
        /// </summary>
        [DisplayName("Codepage")]
        public string Codepage
        {
            get { return _codepage; }
            set { _codepage = value; NotifyPropertyChanged("Codepage"); }
        }

        /// <summary>
        /// DataConverter conversion type (DateConvertTypes)
        /// </summary>
        private DateConvertTypes _date2string;
        /// <summary>
        /// DataConverter conversion type (DateConvertTypes)
        /// </summary>
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

        /// <summary>
        /// destination format for conversion type (DateConvertTypes) STR2YYYYMMDD
        /// </summary>
        private string _strConversionByFormat;
        /// <summary>
        /// destination format for conversion type (DateConvertTypes) STR2YYYYMMDD
        /// </summary>
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

        /// <summary>
        /// DataConverter conversion type (DateConvertTypes) as string
        /// exception: conversion type STR2YYYYMMDD is replaced with StrConversionByFormat
        /// </summary>
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

        /// <summary>
        /// regular expression (value has to match this expression)
        /// </summary>
        private string _regEx;
        /// <summary>
        /// regular expression (value has to match this expression)
        /// </summary>
        [DisplayName("RegEx")]
        public string RegEx
        {
            get { return _regEx; }
            set { _regEx = value; NotifyPropertyChanged("RegEx"); }
        }

        /// <summary>
        /// compare expression (value is compared to antother value (i.e. another column)
        /// </summary>
        private string _compare;
        /// <summary>
        /// compare expression (value is compared to antother value (i.e. another column)
        /// </summary>
        [DisplayName("Compare")]
        public string Compare
        {
            get { return _compare; }
            set { _compare = value; NotifyPropertyChanged("Compare"); }
        }

        /// <summary>
        /// replacement value is conversion is not successful
        /// </summary>
        private string _onErrorValue;
        /// <summary>
        /// replacement value is conversion is not successful
        /// </summary>
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

        /// <summary>
        /// Are null values allowed?
        /// </summary>
        [DisplayName("Allow Null")]
        public bool AllowNull { get; set; }

        /// <summary>
        /// Errorhandling (ignore, failure, redirect)
        /// </summary>
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

        /// <summary>
        /// Is column used as an errorCounter)
        /// </summary>
        private bool _isErrorCounter;
        /// <summary>
        /// Is column used as an errorCounter)
        /// </summary>
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

        /// <summary>
        /// input column id
        /// </summary>
        private string _inputId;
        /// <summary>
        /// input column id
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string InputId
        {
            get { return _inputId; }
            set { _inputId = value; }
        }
        /// <summary>
        /// input column idString
        /// </summary>
        private string _inputIdString;
        /// <summary>
        /// input column idString
        /// </summary>
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

        /// <summary>
        /// output column id
        /// </summary>
        private string _outputId;
        /// <summary>
        /// output column id
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputId
        {
            get { return _outputId; }
            set { _outputId = value; }
        }
        /// <summary>
        /// output column idString
        /// </summary>
        private string _outputIdString;
        /// <summary>
        /// output column idString
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputIdString
        {
            get { return _outputIdString; }
            set { _outputIdString = value; }
        }
        /// <summary>
        /// output column lineageId
        /// </summary>
        private string _outputLineageId;
        /// <summary>
        /// output column lineageId
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputLineageId
        {
            get { return _outputLineageId; }
            set { _outputLineageId = value; }
        }

        /// <summary>
        /// output column (logoutput) id
        /// </summary>
        private string _outputErrorId;
        /// <summary>
        /// output column (logoutput) id
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorId
        {
            get { return _outputErrorId; }
            set { _outputErrorId = value; }
        }

        /// <summary>
        /// output column (logoutput) idString
        /// </summary>
        private string _outputErrorIdString;
        /// <summary>
        /// output column (logoutput) idString
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorIdString
        {
            get { return _outputErrorIdString; }
            set { _outputErrorIdString = value; }
        }

        /// <summary>
        /// output column (logoutput) lineageId
        /// </summary>
        private string _outputErrorLineageId;
        /// <summary>
        /// output column (logoutput) lineageId
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string OutputErrorLineageId
        {
            get { return _outputErrorLineageId; }
            set { _outputErrorLineageId = value; }
        }

        /// <summary>
        /// input columns custom id (GUID)  
        /// </summary>
        private string _customId;
        /// <summary>
        /// input columns custom id (GUID)  
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public string CustomId
        {
            get { return _customId; }
            set { _customId = value; }
        }



        #region Conversion

        /// <summary>
        /// Datatype characteristic for DataConverter conversion type 
        /// </summary>
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

        /// <summary>
        /// Do current column setting allow usage of DataConveter conversion types?
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversion
        {
            get { return (SupportsConversionDate || SupportsConversionNumeric || SupportsConversionStrByFormat) && !IsErrorCounter && Convert; }
        }

        /// <summary>
        /// Do column input and output datatype allow usage of DataConverter conversion types YYYYMMDD, YYYYMM, YYYY?
        /// </summary>
        [BrowsableAttribute(false), ReadOnly(true)]
        public bool SupportsConversionDate
        {
            get
            {
                return (DataType == "DT_WSTR" || DataType == "DT_STR") &&
                       (DataTypeInput.StartsWith("DT_DATE") || DataTypeInput.StartsWith("DT_DBDATE") || DataTypeInput.StartsWith("DT_DBTIMESTAMP"));
            }
        }

        /// <summary>
        /// Do column input and output datatype allow usage of DataConverter conversion types Point2Comma, Comma2Point, AmericanDecimal, GermanDecimal?
        /// </summary>
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

        /// <summary>
        /// Do column input and output datatype allow usage of DataConverter conversion type STR2YYYYMMDD?
        /// </summary>
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

        /// <summary>
        /// List of supported DataConverter conversion types
        /// (STR2YYYYMMDD is replaced by Constants.STRING_CONVERSION_TYPES, i.e. "YYYY.MM.DD")
        /// </summary>
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
                    _supportedConversions.Add(DateConvertTypes.HHMM.ToString());
                    _supportedConversions.Add(DateConvertTypes.HHMMSS.ToString());
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
                }

                _supportedConversions.RaiseListChangedEvents = true;
                return _supportedConversions;

            }
        }

        /// <summary>
        /// Sets conversion type to None / StrConversionByFormat to "" if current setting does not allow usage of that conversion
        /// </summary>
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

        /// <summary>
        /// Does this column has a compare condition?
        /// </summary>
        [BrowsableAttribute(false)]
        public bool HasCompare
        {
            get { return Compare != null && Compare.Trim() != String.Empty; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        public ColumnConfig() { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="inputColumnName">input column name</param>
        /// <param name="convert">convert column value?</param>
        /// <param name="outputAlias">outpu column name</param>
        /// <param name="dataTypeInput">input column datatype</param>
        /// <param name="dataType">output column datatype</param>
        /// <param name="length">output column datatype length</param>
        /// <param name="precision">output column datatype precision</param>
        /// <param name="scale">output column datatype scale</param>
        /// <param name="codepage">output column datatype codepage</param>
        /// <param name="defaultValue">deault value for null values</param>
        /// <param name="inputId">input column id</param>
        /// <param name="inputIdString">input column idString</param>
        /// <param name="inputLineageId">input column lineageId</param>
        /// <param name="outputId">output column id</param>
        /// <param name="outputIdString">output column idString</param>
        /// <param name="outputLineageId">output column lineageId</param>
        /// <param name="outpuErrortId">log output column id</param>
        /// <param name="outputErrorIdString">log output column idString</param>
        /// <param name="outputErrorLineageId">log output column lineageId</param>
        /// <param name="allowNull">are null values allowed?</param>
        /// <param name="inputCol">SSIS input column</param>
        /// <param name="outCol">SSIS output column</param>
        /// <param name="outLogCol">SSIS log output column</param>
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
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="inputColumnName">input column name</param>
        /// <param name="convert">convert column value?</param>
        /// <param name="outputAlias">outpu column name</param>
        /// <param name="dataTypeInput">input column datatype</param>
        /// <param name="dataType">output column datatype</param>
        /// <param name="length">output column datatype length</param>
        /// <param name="precision">output column datatype precision</param>
        /// <param name="scale">output column datatype scale</param>
        /// <param name="codepage">output column datatype codepage</param>
        /// <param name="defaultValue">deault value for null values</param>
        /// <param name="inputId">input column id</param>
        /// <param name="inputIdString">input column idString</param>
        /// <param name="inputLineageId">input column lineageId</param>
        /// <param name="outputId">output column id</param>
        /// <param name="outputIdString">output column idString</param>
        /// <param name="outputLineageId">output column lineageId</param>
        /// <param name="outpuErrortId">log output column id</param>
        /// <param name="outputErrorIdString">log output column idString</param>
        /// <param name="outputErrorLineageId">log output column lineageId</param>
        /// <param name="allowNull">are null values allowed?</param>
        /// <param name="dateConverttype">Dataconverte conversion type</param>
        /// <param name="inputCol">SSIS input column</param>
        /// <param name="outCol">SSIS output column</param>
        /// <param name="outLogCol">SSIS log output column</param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputColumnName">input column name</param>
        /// <param name="convert">convert column value?</param>
        /// <param name="outputAlias">outpu column name</param>
        /// <param name="dataTypeInput">input column datatype</param>
        /// <param name="dataType">output column datatype</param>
        /// <param name="length">output column datatype length</param>
        /// <param name="precision">output column datatype precision</param>
        /// <param name="scale">output column datatype scale</param>
        /// <param name="codepage">output column datatype codepage</param>
        /// <param name="defaultValue">deault value for null values</param>
        /// <param name="inputId">input column id</param>
        /// <param name="inputIdString">input column idString</param>
        /// <param name="inputLineageId">input column lineageId</param>
        /// <param name="outputId">output column id</param>
        /// <param name="outputIdString">output column idString</param>
        /// <param name="outputLineageId">output column lineageId</param>
        /// <param name="outpuErrortId">log output column id</param>
        /// <param name="outputErrorIdString">log output column idString</param>
        /// <param name="outputErrorLineageId">log output column lineageId</param>
        /// <param name="allowNull">are null values allowed?</param>
        /// <param name="dateConverttype">Dataconverte conversion type</param>
        /// <param name="inputCol">SSIS input column</param>
        /// <param name="outCol">SSIS output column</param>
        /// <param name="outLogCol">SSIS log output column</param>
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

            LineageMapping.SetIdProperty(_customId, inputCol.CustomPropertyCollection);
            if (outCol != null)
                LineageMapping.SetIdProperty(_customId, outCol.CustomPropertyCollection);
            if (outLogCol != null)
                LineageMapping.SetIdProperty(_customId, outLogCol.CustomPropertyCollection);

        }

        /// <summary>
        /// Get input column datatype as string
        /// </summary>
        /// <param name="dataType">output column datatype</param>
        /// <param name="length">output column datatype length</param>
        /// <param name="precision">output column datatype precision</param>
        /// <param name="scale">output column datatype scale</param>
        /// <param name="codepage">output column datatype codepage</param>
        /// <returns></returns>
        public string CreateInputDataTypeString(string dataType, string length, string precision, string scale, string codepage)
        {
            return dataType + " (" + length + "," + precision + "," + scale + "," + codepage.ToString() + ")";
        }

        #endregion

        #region IXmlSerializable

        /// <summary>
        /// Gets XML schema (not used, so null is returned)
        /// </summary>
        /// <returns>XmlSchema (null)</returns>
        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        /// <summary>
        /// Reads a ColumnConfig from an XML reader
        /// </summary>
        /// <param name="reader">xml reader</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {

            reader.MoveToContent();

            try
            {;
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

        /// <summary>
        /// Writes this ColumnConfig to an XML writer
        /// </summary>
        /// <param name="writer">xml writer</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
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

        /// <summary>
        /// Output column dataType has length attribute?
        /// </summary>
        /// <returns>Output column dataType has length attribute?</returns>
        public bool HasLength()
        {
            ArrayList length = new ArrayList(Constants.DATATYPE_LENGTH);
            ArrayList lengthCodepage = new ArrayList(Constants.DATATYPE_LENGTH_CODEPAGE);

            return (length.Contains(DataType) || lengthCodepage.Contains(DataType));
        }

        /// <summary>
        /// Output column dataType has precision attribute?
        /// </summary>
        /// <returns>Output column dataType has precision attribute?</returns>
        public bool HasPrecision()
        {
            ArrayList precisionScale = new ArrayList(Constants.DATATYPE_PRECISION_SCALE);

            return precisionScale.Contains(DataType);
        }

        /// <summary>
        /// Output column dataType has scale attribute?
        /// </summary>
        /// <returns>Output column dataType has scale attribute?</returns>
        public bool HasScale()
        {
            ArrayList precisionScale = new ArrayList(Constants.DATATYPE_PRECISION_SCALE);
            ArrayList scale = new ArrayList(Constants.DATATYPE_SCALE);

            return (precisionScale.Contains(DataType) || scale.Contains(DataType));
        }

        /// <summary>
        /// Output column dataType has codepage attribute?
        /// </summary>
        /// <returns>Output column dataType has codepage attribute?</returns>
        public bool HasCodePage()
        {
            ArrayList lengthCodepage = new ArrayList(Constants.DATATYPE_LENGTH_CODEPAGE);
            ArrayList codepage = new ArrayList(Constants.DATATYPE_CODEPAGE);


            return (lengthCodepage.Contains(DataType) || codepage.Contains(DataType));
        }


        /// <summary>
        /// If output column datatype has changed, reset of datatype attributes might be necessary
        /// </summary>
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

        /// <summary>
        /// Removes output IDs 
        /// </summary>
        public void RemoveOutput()
        {
            _outputId = "";
            _outputIdString = "";
            _outputLineageId = "";
        }

        /// <summary>
        /// Are input and out datatypes equal?
        /// </summary>
        /// <returns>Are input and out datatypes equal?</returns>
        public bool HasSameInputAndOutputDataType()
        {
            return (DataTypeInput == CreateInputDataTypeString(DataType, Length, Precision, Scale, Codepage));
        }

        /// <summary>
        /// Has default value for null values?
        /// </summary>
        /// <returns>Has default value for null values?</returns>
        public bool HasDefaultValue()
        {
            return (Default != null && Default != "");
        }

        /// <summary>
        /// Has value that replaces values that cannot be converted?
        /// </summary>
        /// <returns>Has value that replaces values that cannot be converted?</returns>
        public bool HasOnErrorValue()
        {
            return (OnErrorValue != null && OnErrorValue != "");
        }

        /// <summary>
        /// Has regular expression?
        /// </summary>
        /// <returns>Has regular expression?</returns>
        public bool HasRegEx()
        {
            return (RegEx != null && RegEx != "" && Convert);
        }

        #endregion
    }
}
