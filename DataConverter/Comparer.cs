using System;
using System.Collections.Generic;
using System.Text;

namespace DataConverter
{
    /// <summary>
    /// Handles compare conditions
    /// </summary>
    class Comparer
    {
        /// <summary>
        /// Does the compare expression contain an invalid operator?
        /// </summary>
        public bool HasInvalidOp { get; set; }

        /// <summary>
        /// Dictionary(Key: column name, value: 2 column values)
        /// </summary>
        private Dictionary<string, ColumnValues> _columnValues = new Dictionary<string, ColumnValues>();
        /// <summary>
        /// List of all columns that are needed for all compares
        /// </summary>
        private List<string> _neededColumns = new List<string>();

        /// <summary>
        /// List of column compares
        /// </summary>
        private List<ColumnCompare> _columnCompareList = new List<ColumnCompare>();

        /// <summary>
        /// skip compare if a needed column ist part of this list
        /// (i.e. if conversion of that column already failed)
        /// </summary>
        private List<string> _skipList = new List<string>();

        /// <summary>
        /// List of all columns that are needed for all compares
        /// </summary>
        public List<string> UsedColumns { get { return _neededColumns; } }

        /// <summary>
        /// Adds column values to a column values dictionary
        /// </summary>
        /// <param name="name">input column name / dictionary key</param>
        /// <param name="inputValue">left column value</param>
        /// <param name="outputValue">rigth column value</param>
        public void AddColumnValue(string name, object inputValue, object outputValue)
        {
            if (_neededColumns.Contains(name)) _columnValues.Add(name, new ColumnValues(inputValue, outputValue));
        }

        /// <summary>
        /// Adds column values to a column values dictionary
        /// </summary>
        /// <param name="name">input column name / dictionary key</param>
        /// <param name="inputValue">left column value</param>
        public void AddColumnValue(string name, object inputValue)
        {
            AddColumnValue(name, inputValue, inputValue);
        }

        /// <summary>
        /// Adds a ColumnCompare
        /// </summary>
        /// <param name="compare">compare expression</param>
        /// <param name="columnName">input column name</param>
        public void AddCompare(string compare, string columnName)
        {
            ColumnCompare columnCompare = CreateColumnCompare(compare, columnName);
            _columnCompareList.Add(columnCompare);
            _neededColumns.Add(columnCompare.Column1);
            _neededColumns.Add(columnCompare.Column2);
        }


        /// <summary>
        /// Creates a ColumnCompare
        /// </summary>
        /// <param name="compare">compare expression</param>
        /// <param name="columnName">input columnn name</param>
        /// <returns></returns>
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
                return new ColumnCompare("", "", "", compare, columnName); 
            }

            int pos = compare.IndexOf(op);
            string leftColumn = compare.Substring(0, pos).Trim();
            string rightColumn = compare.Substring(pos + op.Length).Trim();

            ColumnCompare columnCompare = new ColumnCompare(leftColumn, rightColumn, op, compare, columnName);

            return columnCompare;
        }

        /// <summary>
        /// Are all compare condition fulfilled?
        /// (if not errorMessages and errorColumns are gathered)
        /// </summary>
        /// <param name="errorMessages"></param>
        /// <param name="errorColumns"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds column to skipList if needed for comparism
        /// </summary>
        /// <param name="columnName">column name that has to be skipped</param>
        public void SkipColumn(string columnName)
        {
            if (_neededColumns.Contains(columnName) && !_skipList.Contains(columnName)) _skipList.Add(columnName);
        }

        /// <summary>
        /// Resets column values and skip list 
        /// (necessary for each data row)
        /// </summary>
        public void Reset()
        {
            _columnValues.Clear();
            _skipList.Clear();
        }

        /// <summary>
        /// Are all datatypes of all ColumnCompare comparable?
        /// </summary>
        /// <param name="prop">components custom properties</param>
        /// <param name="errorMessage">errorMessage that is filled if datatypes are not comparable </param>
        /// <returns>Are all datatypes of all ColumnCompare comparable?</returns>
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

    /// <summary>
    /// Pair of column values
    /// </summary>
    class ColumnValues
    {
        /// <summary>
        /// input column value (not useed for comparism -> remove property?)
        /// </summary>
        public object InputValue { get; set; }
        /// <summary>
        /// output column value
        /// </summary>
        public object OutputValue { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="outputValue"></param>
        public ColumnValues(object inputValue, object outputValue)
        {
            InputValue = inputValue;
            OutputValue = outputValue;
        }
    }

    class ColumnCompare
    {
        /// <summary>
        /// not used
        /// </summary>
        public string ConvertedColumn { get; set; }
        /// <summary>
        /// First column of compare expression (i.e. Column1 &lt;= Column2)
        /// </summary>
        public string Column1 { get; set; }
        /// <summary>
        /// Second column of compare expression (i.e. Column1 &lt;= Column2)
        /// </summary>
        public string Column2 { get; set; }
        /// <summary>
        /// Operator (i.e. ==) to compare 2 values
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// Compare expression
        /// </summary>
        public string Compare { get; set; }
        /// <summary>
        /// SSIS input column name: if compare fails the conversion for this column fails
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Error message if compare fails
        /// </summary>
        public string ErrorMessage { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="column1">First column of compare expression (i.e. Column1 &lt;= Column2)</param>
        /// <param name="column2">Second column of compare expression (i.e. Column1 &lt;= Column2)</param>
        /// <param name="op">Operator (i.e. ==) to compare 2 values</param>
        /// <param name="compare">Compare expression</param>
        /// <param name="columnName">SSIS input column name: if compare fails the conversion for this column fails</param>
        public ColumnCompare(string column1, string column2, string op, string compare, string columnName)
        {
            Column1 = column1;
            Column2 = column2;
            Operator = op;
            Compare = compare;
            ColumnName = columnName;
        }

        /// <summary>
        /// Uses compare expression and 2 columns values (parameter) to determine if compare result is true
        /// </summary>
        /// <param name="value1">left value</param>
        /// <param name="value2">right value</param>
        /// <returns>Is compare result == true?</returns>
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
                ErrorMessage = string.Format("{0} ({1} {2} {3}) : Compare condition not fulfilled!", Compare, v1, Operator, v2);
            }

            return isValid;
        }

        /// <summary>
        /// Converts 2 values, so that their dataTypes are comaparable
        /// </summary>
        /// <param name="value1">left value</param>
        /// <param name="value2">right value</param>
        private void GetComparableValues(ref object value1, ref object value2)
        {
            if (value1.GetType() == value2.GetType()) return; //datatypes are equal, so values can be compared

            //Assumption: if datatypes differ only number datatypes are relevant            
            value1 = Convert.ToDecimal(value1);
            value2 = Convert.ToDecimal(value2);
        }

        /// <summary>
        /// 2 IComparables are compared, result (parameter compareResult) is compared with operator to determine if compare is true
        /// </summary>
        /// <param name="compareResult">result of comparism of 2 values</param>
        /// <returns>Is compare result == true?</returns>
        private bool IsValid(int compareResult)
        {
            if (Operator == "<=") return compareResult <= 0;
            else if (Operator == "<") return compareResult < 0;
            else if (Operator == ">=") return compareResult >= 0;
            else if (Operator == ">") return compareResult > 0;
            else if (Operator == "!=") return compareResult != 0;
            else if (Operator == "==") return compareResult == 0;

            throw new Exception("Wrong Operator: " + Operator);
        }

    }
}
