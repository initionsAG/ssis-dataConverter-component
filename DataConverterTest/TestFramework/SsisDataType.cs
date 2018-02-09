using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class SsisDataType
    {
        public DataType Type { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
        public int Codepage { get; set; }
        public int Length { get; set; }

        public SsisDataType()
        {
            Scale = 0;
            Precision = 0;
            Codepage = 0;
            Length = 0;
        }

        public string StringCastExpression
        {
            get
            {
                return Codepage == 0 ? "(DT_WSTR, 255)" : "(DT_STR, 255, 1252)";
            }
        }

        public static SsisDataType GetDataTypeByName(string name)
        {
            switch (name)
            {
                case "DT_I1": return new SsisDataType() { Type = DataType.DT_I1 };
                case "DT_I2": return new SsisDataType() { Type = DataType.DT_I2 };
                case "DT_I4": return new SsisDataType() { Type = DataType.DT_I4 };
                case "DT_I8": return new SsisDataType() { Type = DataType.DT_I8 };
                case "DT_UI1": return new SsisDataType() { Type = DataType.DT_UI1 };
                case "DT_UI2": return new SsisDataType() { Type = DataType.DT_UI2 };
                case "DT_UI4": return new SsisDataType() { Type = DataType.DT_UI4 };
                case "DT_UI8": return new SsisDataType() { Type = DataType.DT_UI8 };
                case "DT_R4": return new SsisDataType() { Type = DataType.DT_R4 };
                case "DT_R8": return new SsisDataType() { Type = DataType.DT_R8 };
                case "DT_NUMERIC": return new SsisDataType() { Type = DataType.DT_NUMERIC, Precision = 18, Scale = 5 };
                case "DT_DECIMAL": return new SsisDataType() { Type = DataType.DT_DECIMAL, Scale = 5 };
                case "DT_WSTR": return new SsisDataType() { Type = DataType.DT_WSTR, Length = 255 };
                case "DT_STR": return new SsisDataType() { Type = DataType.DT_STR, Length = 255, Codepage = 1252 };
                case "DT_NTEXT": return new SsisDataType() { Type = DataType.DT_NTEXT };
                case "DT_TEXT": return new SsisDataType() { Type = DataType.DT_TEXT, Codepage = 1252 };
                case "DT_DBTIMESTAMP": return new SsisDataType() { Type = DataType.DT_DBTIMESTAMP };
                case "DT_DBDATE": return new SsisDataType() { Type = DataType.DT_DBDATE };
                case "DT_DBTIME": return new SsisDataType() { Type = DataType.DT_DBTIME };
                case "DT_DATE": return new SsisDataType() { Type = DataType.DT_DATE };
                case "DT_CY": return new SsisDataType() { Type = DataType.DT_CY };
                case "DT_BOOL": return new SsisDataType() { Type = DataType.DT_BOOL };
                case "DT_BYTES": return new SsisDataType() { Type = DataType.DT_BYTES, Length = 255 };
                default:
                    throw new Exception("Factory does not accept datatype name " + name);
            }
        }
    }
}
