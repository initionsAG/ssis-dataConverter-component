using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class ConnectionManagerFlatFile
    {
        public static string MONIKER = "FLATFILE";
        public ConnectionManager ConnectionManager { get; set; }
        

        private static Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType DATATYPE = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR;
        private static string DELIMITER_ROW = "\r";
        private static string DELIMITER_COLUMN = ";";
        private static int WIDTH = 4000;
        private static string COLUMNTYPE = "Delimited";
        private static bool TEXTQUALIFIED = true;

        public ConnectionManagerFlatFile(ConnectionManager connectionManager, string name,  string fileName)
        {
            connectionManager.ConnectionString = fileName;
            connectionManager.Name = name;

            ConnectionManager = connectionManager;
        }

        public void AddFlatFileColumn(string colName, bool lastColumn = false)
        {
            Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSConnectionManagerFlatFile100 _conMgr =
                (Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSConnectionManagerFlatFile100)(((ConnectionManager)(ConnectionManager)).InnerObject);

            Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSConnectionManagerFlatFileColumn100 newCol = _conMgr.Columns.Add();
            newCol.DataType = DATATYPE;
            newCol.MaximumWidth = WIDTH;
            newCol.ColumnType = COLUMNTYPE;
            newCol.TextQualified = TEXTQUALIFIED;
            newCol.ColumnDelimiter = lastColumn ? DELIMITER_ROW : DELIMITER_COLUMN;

            (newCol as Microsoft.SqlServer.Dts.Runtime.Wrapper.IDTSName100).Name = colName;
        }
    }
}
