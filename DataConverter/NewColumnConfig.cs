using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Collections;

namespace DataConverter
{
    /// <summary>
    /// New SSIS columns are no longer supperted
    /// </summary>
    public class NewColumnConfig : IXmlSerializable
    {
        #region Properties (external)

        private string _outputAlias = "";
        [DisplayName("Output Alias")]
        public string OutputAlias
        {
            get { return _outputAlias; }
            set { _outputAlias = value; }
        }

        private string _dataType = "DT_WSTR";
        [DisplayName("DataType (Output)")]
        public string DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                if (!HasLength()) Length = "0";
                if (!HasPrecision()) Precision = "0";
                if (!HasScale()) Scale = "0";
                if (!HasCodePage()) Codepage = "0";
            }
        }

        private string _length = "0";
        [DisplayName("Length")]
        public string Length
        {
            get { return _length; }
            set { _length = value; }
        }

        private string _precision = "0";
        [DisplayName("Precision")]
        public string Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        private string _scale = "0";
        [DisplayName("Scale")]
        public string Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        private string _codepage = "0";
        [DisplayName("Codepage")]
        public string Codepage
        {
            get { return _codepage; }
            set { _codepage = value; }
        }

        private string _default = "";
        [DisplayName("Value")]
        public string Default
        {
            get { return _default; }
            set { _default = value; }
        }
        #endregion

        #region Properties (internal)
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
            if (!HasLength()) Length = "0";
            if (!HasPrecision()) Precision = "0";
            if (!HasScale()) Scale = "0";
            if (!HasCodePage()) Codepage = "0";
        }

        public bool HasOutput()
        {
            return !string.IsNullOrEmpty(OutputId);
        }

        public void RemoveOutput()
        {
            _outputId = "";
            _outputIdString = "";
            _outputLineageId = "";
        }



        public bool HasDefaultValue()
        {
            return (Default != null && Default != "");
        }






        #endregion

        #region IXmlSerializable

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {

            reader.MoveToContent();

            try
            {
                _outputAlias = reader.GetAttribute("OutputAlias");
                _dataType = reader.GetAttribute("DataType");
                _length = reader.GetAttribute("Length");
                _precision = reader.GetAttribute("Precision");
                _scale = reader.GetAttribute("Scale");
                _codepage = reader.GetAttribute("Codepage");
                _default = reader.GetAttribute("Default");

                _outputId = reader.GetAttribute("OutputId");
                _outputIdString = reader.GetAttribute("OutputIdString");
                _outputLineageId = reader.GetAttribute("OutputLineageId");

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
            writer.WriteAttributeString("OutputAlias", _outputAlias);
            writer.WriteAttributeString("DataType", _dataType);
            writer.WriteAttributeString("Length", _length);
            writer.WriteAttributeString("Precision", _precision);
            writer.WriteAttributeString("Scale", _scale);
            writer.WriteAttributeString("Codepage", _codepage);
            writer.WriteAttributeString("Default", _default);

            writer.WriteAttributeString("OutputId", _outputId);
            writer.WriteAttributeString("OutputIdString", _outputIdString);
            writer.WriteAttributeString("OutputLineageId", _outputLineageId);
        }

        #endregion
    }
}
