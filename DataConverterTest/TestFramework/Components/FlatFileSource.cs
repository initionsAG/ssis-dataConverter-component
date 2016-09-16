
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    class FlatFileSource : FlatFile
    {
#if   (SQL2008)
        public new static string MONIKER = "DTSAdapter.FlatFileSource.2";
#elif (SQL2012)
       public new static string MONIKER = "DTSAdapter.FlatFileSource.3";
#elif (SQL2014)
        public new static string MONIKER = "DTSAdapter.FlatFileSource.4";
#else
        public new static string MONIKER = "Microsoft.FlatFileSource";
#endif

        public FlatFileSource(IDTSComponentMetaData100 metadata, string name, ConnectionManagerFlatFile conMgr) : base(metadata, name, conMgr) { }
    }
}
