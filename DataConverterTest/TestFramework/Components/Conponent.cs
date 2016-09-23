using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework.Components
{

    /// <summary>
    /// Abstract Component, used to implement concrete SSIS components
    /// </summary>
    abstract class Component
    {
        /// <summary>
        /// the components metadata
        /// </summary>
        public IDTSComponentMetaData100 Metadata { get; set; }

        /// <summary>
        /// string to generate SSIS component in the package
        /// </summary>
        public static string MONIKER = "";

        /// <summary>
        /// Constructor of the abstract component
        /// </summary>
        /// <param name="metadata">the components metadata</param>
        /// <param name="name">the name of the component in the package</param>
        public Component(IDTSComponentMetaData100 metadata, string name)
        {
            Metadata = metadata;

            CManagedComponentWrapper instance = Metadata.Instantiate();
            instance.ProvideComponentProperties();
            Metadata.Name = name;
        }
    }
}
