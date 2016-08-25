
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
        public new static string MONIKER = "Microsoft.FlatFileSource";

        public FlatFileSource(IDTSComponentMetaData100 metadata, string name, ConnectionManagerFlatFile conMgr) : base (metadata, name, conMgr) { }
    }
}
