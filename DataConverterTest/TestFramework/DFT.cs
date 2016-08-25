
using DataConverterTest.TestFramework.Components;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
   
    class DFT
    {
        public static string MONIKER = "Stock:PipelineTask";

        private MainPipe _dft;

        public DFT(Executable executableDft, string name)
        {
            TaskHost taskHost = (TaskHost)executableDft;
            taskHost.Name = name;
            _dft = (MainPipe)taskHost.InnerObject;
            _dft.RunInOptimizedMode = false; //TODO: entfernen sobald FFs an allen DC outputs hängen
        }

        public void ConnectComponents(IDTSComponentMetaData100 componentStart, IDTSComponentMetaData100 componentEnd)
        {
            ConnectComponents(componentStart, componentEnd, 0, 0);
        }
        public void ConnectComponents(IDTSComponentMetaData100 componentStart, IDTSComponentMetaData100 componentEnd,
            int idOutput, int idInput)
        {
            IDTSPath100 newPath = _dft.PathCollection.New();
            newPath.AttachPathAndPropagateNotifications(componentStart.OutputCollection[idOutput], componentEnd.InputCollection[idInput]);
        }

        //@param componentClassID, ist abhängig von SQL Version. Bei SQL2016 beispielsweise 5. Muss für andere Versionen noch kompatibel werden.
        private IDTSComponentMetaData100 AddComponent(string componentClassID)
        {
            IDTSComponentMetaData100 componentMetaData = _dft.ComponentMetaDataCollection.New();
            componentMetaData.ComponentClassID = componentClassID;

            return componentMetaData;
        }

        public DerivedColumn CreateDerivedColumn(string name)
        {
            return new DerivedColumn(AddComponent(DerivedColumn.MONIKER), name);
        }

        public FlatFileSource CreateFlatFileSource(string name, ConnectionManagerFlatFile cnMgrFlatFile)
        {
            return new FlatFileSource(AddComponent(FlatFileSource.MONIKER), name, cnMgrFlatFile);
        }

        public FlatFileDestination CreateFlatFileDestination(string name, ConnectionManagerFlatFile cnMgrFlatFile)
        {
            return new FlatFileDestination(AddComponent(FlatFileDestination.MONIKER), name, cnMgrFlatFile);
        }

        public DataConverterComponent CreateDataConverter(string name)
        {
            return new DataConverterComponent(AddComponent(DataConverterComponent.MONIKER), name);
        }
    }
}
