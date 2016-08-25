using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{
    abstract class Component
    {
        public IDTSComponentMetaData100 Metadata { get; set; }
        public static string MONIKER = "";

        public Component(IDTSComponentMetaData100 metadata, string name)
        {
            Metadata = metadata;

            CManagedComponentWrapper instance = Metadata.Instantiate();
            instance.ProvideComponentProperties();

            Metadata.Name = name;
        }
    }
}
