using DataConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class DataConverterTestConfiguration
    {
        private static string COLUMN_NAME_INPUT = "inputValue";
        private string _inputColumnName = COLUMN_NAME_INPUT;
        public string InputColumnName { get { return _inputColumnName; } }

        private static string COLUMN_NAME_EXPECTED = "expectedValue";
        private string _expectedColumnName = COLUMN_NAME_EXPECTED;
        public string ExpectedColumnName { get { return _expectedColumnName; } }

        private static string COLUMN_NAME_COMPARE = "compareValue";
        private string _compareColumnName = COLUMN_NAME_COMPARE;
        public string CompareColumnName { get { return _compareColumnName; } }

        public SsisDataType InputDataType { get; set; }
        public SsisDataType ExpectedDataType { get; set; }
        public SsisDataType ComparedDataType { get; set; }
        public string InputValueExpression { get; set; }
        public string ExpectedValueExpression { get; set; }
        public string CompareValueExpression { get; set; }

        public bool HasCompare { get { return ComparedDataType != null && !string.IsNullOrEmpty(CompareOperator); } }
        

        public string OnNull { get; set; }
        public string OnError { get; set; }
        public string RegEx { get; set; }
        public bool AllowNull { get; set; }
        public bool UseErrorCount { get; set; }
        public int ExpectedErrorCount { get; set; }
        public string Language { get; set; } 
        public string Compare { get { return _inputColumnName + " " + CompareOperator + " " + _compareColumnName; } }
        public string CompareOperator { get; set; }
        public DateConvertTypes Conversion { get; set; }

        //Gültig:  "YYYY.MM.DD", "DD.MM.YYYY", "YYYY/MM/DD", "DD/MM/YYYY", "YYYY-MM-DD", "DD-MM-YYYY", "MM-DD-YYYY" 
        public string String_Conversion_Type { get; set; }

        public void AddPostfix(int index)
        {
            _inputColumnName += index.ToString();
            _expectedColumnName += index.ToString();
            _compareColumnName += index.ToString();
        }
        
    }
}
