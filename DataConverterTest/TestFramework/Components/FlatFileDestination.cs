using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    class FlatFileDestination : FlatFile
    {
#if (SQL2008)
        public new static string MONIKER = "DTSAdapter.FlatFileDestination.2";
#elif (SQL2012)
       public new static string MONIKER = "DTSAdapter.FlatFileDestination.3";
#elif (SQL2014)
        public new static string MONIKER = "DTSAdapter.FlatFileDestination.4";
#else
        public new static string MONIKER = "Microsoft.FlatFileDestination";
#endif

        //public new static string MONIKER = "Microsoft.FlatFileDestination.4";

        public FlatFileDestination(IDTSComponentMetaData100 metadata, string name, ConnectionManagerFlatFile conMgr) : base (metadata, name, conMgr) { }      

        public void MapColumns()
        {
            IDTSVirtualInput100 vInput = Metadata.InputCollection[0].GetVirtualInput();

            CManagedComponentWrapper instance = Metadata.Instantiate();

            foreach (IDTSExternalMetadataColumn100 extCol in Metadata.InputCollection[0].ExternalMetadataColumnCollection)
            {
                IDTSVirtualInputColumn100 vcol = vInput.VirtualInputColumnCollection[extCol.Name];
                instance.SetUsageType(Metadata.InputCollection[0].ID, vInput, vcol.LineageID, DTSUsageType.UT_READONLY);
                instance.MapInputColumn(Metadata.InputCollection[0].ID, Metadata.InputCollection[0].InputColumnCollection[extCol.Name].ID,
                    extCol.ID);
            }
 
        }
    }
}
