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
    }
}
