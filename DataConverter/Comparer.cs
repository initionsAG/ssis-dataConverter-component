using System;
using System.Collections.Generic;
using System.Text;

namespace DataConverter
{
    class Comparer
    {
        public bool HasInvalidOp { get; set; }
        private Dictionary<string, ColumnValues> _columnValues = new Dictionary<string, ColumnValues>();
        private List<string> _neededColumns = new List<string>();
        private List<ColumnCompare> _columnCompareList = new List<ColumnCompare>();
        private List<string> _skipList = new List<string>();

        public List<string> UsedColumns { get { return _neededColumns; } }

        public void AddColumnValue(string name, object inputValue, object outputValue)
        {
            if (_neededColumns.Contains(name)) _columnValues.Add(name, new ColumnValues(inputValue, outputValue));
        }

        public void AddColumnValue(string name, object inputValue)
        {
            AddColumnValue(name, inputValue, inputValue);
        }

        public void AddCompare(string compare, string columnName)
        {
            ColumnCompare columnCompare = CreateColumnCompare(compare, columnName);
            _columnCompareList.Add(columnCompare);
            _neededColumns.Add(columnCompare.Column1);
            _neededColumns.Add(columnCompare.Column2);
        }



        public ColumnCompare CreateColumnCompare(string compare, string columnName)
        {
            string op = "";

            if (compare.Contains("<=")) op = "<=";
            else if (compare.Contains("<")) op = "<";
            else if (compare.Contains(">=")) op = ">=";
            else if (compare.Contains(">")) op = ">";
            else if (compare.Contains("!=")) op = "!=";
            else if (compare.Contains("==")) op = "==";

            if (op == "")
            {
                HasInvalidOp = true;
                return new ColumnCompare("", "", "", compare, columnName); //throw new Exception("Kein Operator im Vergleich " + compare + " gefunden.");
            }

            int pos = compare.IndexOf(op);
            string leftColumn = compare.Substring(0, pos).Trim();
            string rightColumn = compare.Substring(pos + op.Length).Trim();

            ColumnCompare columnCompare = new ColumnCompare(leftColumn, rightColumn, op, compare, columnName);

            return columnCompare;
        }

        public bool IsValid(ref string errorMessages, ref string errorColumns)
        {
            bool isValid = true;

            foreach (ColumnCompare columnCompare in _columnCompareList)
            {
                if (!_skipList.Contains(columnCompare.Column1) && !_skipList.Contains(columnCompare.Column2))
                {
                    bool isCurrentCompareValid = columnCompare.IsValid(_columnValues[columnCompare.Column1].OutputValue, _columnValues[columnCompare.Column2].OutputValue);
                    isValid = isValid && isCurrentCompareValid;
                    if (!isCurrentCompareValid)
                    {
                        errorMessages += columnCompare.ErrorMessage + ";";
                        errorColumns += columnCompare.ColumnName + ";";
                    }
                }
            }

            return isValid;
        }

        public void SkipColumn(string columnName)
        {
            if (_neededColumns.Contains(columnName) && !_skipList.Contains(columnName)) _skipList.Add(columnName);
        }

        public void Reset()
        {
            _columnValues.Clear();
            _skipList.Clear();
        }

        /// <summary>
        /// Können alle Datentypen verglichen werden?
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool AreDataTypesComparable(IsagCustomProperties prop, ref string errorMessage)
        {
            foreach (ColumnCompare compare in _columnCompareList)
            {
                ColumnConfig config1 = prop.GetColumnConfigByInputColumnName(compare.Column1);
                ColumnConfig config2 = prop.GetColumnConfigByInputColumnName(compare.Column2);

                List<string> stringDataType = new List<string>(Constants.DATATYPE_STRING);
                List<string> numberDataType = new List<string>(Constants.DATATYPE_NUMBER);
                List<string> dateDataType = new List<string>(Constants.DATATYPE_DATE);

                if (config1.DataType != config2.DataType && 
                    !(
                        (stringDataType.Contains(config1.DataType) && stringDataType.Contains(config2.DataType)) ||
                        (numberDataType.Contains(config1.DataType) && numberDataType.Contains(config2.DataType)) ||
                        (dateDataType.Contains(config1.DataType) && dateDataType.Contains(config2.DataType))
                    ))                    
                {
                    errorMessage = string.Format("Spalte {0}: Cannot compare datatypes {1} and {2}.", compare.ColumnName, config1.DataType, config2.DataType);
                    return false;
                }


            }

            return true;
        }

    }

    class ColumnValues
    {
        public object InputValue { get; set; }
        public object OutputValue { get; set; }


        public ColumnValues(object inputValue, object outputValue)
        {
            InputValue = inputValue;
            OutputValue = outputValue;
        }
    }

    class ColumnCompare
    {
        public string ConvertedColumn { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Operator { get; set; }
        public string Compare { get; set; }
        public string ColumnName { get; set; }
        public string ErrorMessage { get; set; }


        public ColumnCompare(string column1, string column2, string op, string compare, string columnName)
        {
            Column1 = column1;
            Column2 = column2;
            Operator = op;
            Compare = compare;
            ColumnName = columnName;
        }

        public bool IsValid(object value1, object value2)
        {
            bool isValid;

            if (value1 == null || value2 == null) isValid = false;
            else
            {

                GetComparableValues(ref value1, ref value2);
                IComparable leftValue = (IComparable)value1;
                IComparable rightValue = (IComparable)value2;

                isValid = IsValid(leftValue.CompareTo(rightValue));
            }

            if (!isValid)
            {
                string v1 = value1 == null ? "NULL" : value1.ToString();
                string v2 = value2 == null ? "NULL" : value2.ToString();
                ErrorMessage = string.Format("{0} ({1} {2} {3}) : Bedingung nicht erfüllt!", Compare, v1, Operator, v2);
            }

            return isValid;
        }

        private void GetComparableValues(ref object value1, ref object value2)
        {
            if (value1.GetType() == value2.GetType()) return; //immer gültig für string, dates, bool, Guid            

            //Annahme: relevante Datentypen, die in .NET unterschiedlich sein können, sind nur Zahlen

            value1 = Convert.ToDecimal(value1);
            value2 = Convert.ToDecimal(value2);
        }

        private bool IsValid(int compareResult)
        {
            if (Operator == "<=") return compareResult <= 0;
            else if (Operator == "<") return compareResult < 0;
            else if (Operator == ">=") return compareResult >= 0;
            else if (Operator == ">") return compareResult > 0;
            else if (Operator == "!=") return compareResult != 0;
            else if (Operator == "==") return compareResult == 0;

            throw new Exception("Falscher Operator: " + Operator);
        }

    }
}
