using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    class DerivedColumn : Component
    {
        public new static string MONIKER = "DTSTransform.DerivedColumn.5";
        public bool NeedsIdProperty { get { return true; } }       

        public DerivedColumn(IDTSComponentMetaData100 metadata, string name) : base(metadata, name) { }

        public void AddOutputColumn(string columnName, SsisDataType dataType, string value)
        {
            IDTSOutputColumn100 col = GetNewOutputColumn(Metadata, columnName, dataType);
            AddCustomProperty(ReplaceColumsWithLineageId(value), "Expression", col);
            AddCustomProperty(value, "FriendlyExpression", col);
        }

        public void AddExpressionOutputColumn(string columnName, SsisDataType dataType, string friendlyExpression)
        {
            IDTSOutputColumn100 col = GetNewOutputColumn(Metadata, columnName, dataType);
            AddCustomProperty(friendlyExpression, "FriendlyExpression", col);
            AddCustomProperty(ReplaceColumsWithLineageId(friendlyExpression), "Expression", col);
        }

        private IDTSOutputColumn100 GetNewOutputColumn(IDTSComponentMetaData100 componentMetaData, string columnName, SsisDataType dataType)
        {
            IDTSOutputColumn100 outputColumn = componentMetaData.OutputCollection[0].OutputColumnCollection.New();

            outputColumn.Name = columnName;
            outputColumn.SetDataTypeProperties(dataType.Type, dataType.Length, dataType.Precision, dataType.Scale, dataType.Codepage);
            outputColumn.ErrorRowDisposition = DTSRowDisposition.RD_FailComponent;
            outputColumn.TruncationRowDisposition = DTSRowDisposition.RD_FailComponent;
            outputColumn.ErrorOrTruncationOperation = "Computation";

            return outputColumn;
        }


        private void AddCustomProperty(string expression, string propertyName, IDTSOutputColumn100 col)
        {
            IDTSCustomProperty100 prop = col.CustomPropertyCollection.New();
            prop.Name = propertyName;
            prop.Value = expression;

            if (NeedsIdProperty) prop.ContainsID = true;
        }

        public IDTSInputColumn100 GetColumn(string name)
        {
            return Metadata.InputCollection[0].InputColumnCollection[name];
        }


        protected string ReplaceColumsWithLineageId(string friendlyExpression)
        {
            IDTSVirtualInput100 virtualInput = GetVirtualInput();

            string expression = friendlyExpression;
            Dictionary<string, int> usedColumns = new Dictionary<string, int>();
            List<string> usedColNames = GetSordetInputColumnList(virtualInput, expression, ref usedColumns);

            foreach (string colName in usedColNames)
            {

                if (ReplaceString(colName, "#" + usedColumns[colName].ToString(), ref expression, alsoReplaceInBrackets: true)
                    && virtualInput.VirtualInputColumnCollection[colName].UsageType != DTSUsageType.UT_READWRITE)
                    virtualInput.SetUsageType(usedColumns[colName], DTSUsageType.UT_READONLY);
            }

            return expression;
        }

        /// <summary>
        /// Replaces all occurences of a value (oldValue) with another value (newValue).
        /// matches inside quotes are ignored.
        /// </summary>
        /// <param name="oldValue">the string to be replaced</param>
        /// <param name="newValue">the string to replace all occurences of the old value</param>
        /// <param name="expression">a string containing old values that will be replaced.</param>
        /// <param name="alsoReplaceInBrackets">also replace old values in brackets ([])?</param>
        /// <returns>Has at least one replacement been occured?</returns>
        protected bool ReplaceString(string oldValue, string newValue, ref string expression, bool alsoReplaceInBrackets)
        {
            bool hasReplaced = false;
            string oldValueInBrackets = "[" + oldValue + "]";

            string[] split = expression.Split(@"""".ToCharArray());

            for (int i = 0; i < split.Length; i++)
            {
                if ((i) % 2 == 0)
                //part without quotes --> replacement occures
                //(or first/last element in split that is empty in case of a quote at the end/beginning of the expression
                {
                    hasReplaced = hasReplaced || split[i].Contains(oldValue) || (alsoReplaceInBrackets && split[i].Contains(oldValueInBrackets));
                    if (alsoReplaceInBrackets) split[i] = split[i].Replace(oldValueInBrackets, newValue);
                    split[i] = split[i].Replace(oldValue, newValue);
                }
                else //part with quotes --> no replacement, just add quotes, that have been lost due to split function
                {
                    split[i] = "\"" + split[i] + "\"";
                }
            }

            //Rebuild expression
            expression = string.Empty;
            for (int i = 0; i < split.Length; i++)
            {
                expression += split[i];
            }

            return hasReplaced;
        }

        private static List<string> GetSordetInputColumnList(IDTSVirtualInput100 virtualInput, string expression, ref Dictionary<string, int> usedColumns)
        {
            List<string> usedColNames = new List<string>();
            foreach (IDTSVirtualInputColumn100 vCol in virtualInput.VirtualInputColumnCollection)
            {
                if (expression.Contains(vCol.Name) || expression.Contains("[" + vCol.Name + "]"))
                {
                    usedColumns.Add(vCol.Name, vCol.LineageID);
                    usedColNames.Add(vCol.Name);
                }
            }

            usedColNames.Sort((str1, str2) =>
            {
                return str2.Length.CompareTo(str1.Length);
            });
            return usedColNames;
        }

        protected IDTSVirtualInput100 GetVirtualInput()
        {
            return GetInput().GetVirtualInput();
        }

        protected IDTSInput100 GetInput()
        {
            return Metadata.InputCollection[0];
        }
    }
}
