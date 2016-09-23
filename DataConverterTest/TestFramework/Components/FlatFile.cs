
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    abstract class FlatFile : Component
    {
        public FlatFile(IDTSComponentMetaData100 metadata, string name, ConnectionManagerFlatFile conMgr) : base(metadata, name)
        {
            //Assign ConnectionManager to FF Component
            metadata.RuntimeConnectionCollection[0].ConnectionManager = DtsConvert.GetExtendedInterface(conMgr.ConnectionManager);
            metadata.RuntimeConnectionCollection[0].ConnectionManagerID = conMgr.ConnectionManager.ID;

            //Reinitialize FF Component 
            CManagedComponentWrapper instance = Metadata.Instantiate();
            instance = metadata.Instantiate();
            instance.AcquireConnections(null);
            instance.ReinitializeMetaData();
            instance.ReleaseConnections();
        }
    }
}
