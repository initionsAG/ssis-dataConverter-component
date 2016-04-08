using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Design;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;

namespace DataConverter
{
    /// <summary>
    /// Implemets SSIS UI Interface, so that SSIS can show the components GUI
    /// </summary>
    class DataConverterUI : IDtsComponentUI
    {
        /// <summary>
        /// SSIS metadata for the component
        /// </summary>
        IDTSComponentMetaData100 _metadata;

        /// <summary>
        /// Defines a mechanism for retrieving a service object; that is, an object that provides custom support to other objects.
        /// </summary>
        IServiceProvider _serviceProvider;

        /// <summary>
        /// implements edit interface method: Called when a component is edited.
        /// </summary>
        /// <param name="parentWindow">parentWindow</param>
        /// <param name="variables">SSIS variables</param>
        /// <param name="connections">SSIS connections</param>
        /// <returns>Save changes?</returns>
        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            frmDataConverterUI frm = new frmDataConverterUI(_metadata, variables);
            return  frm.ShowDialog(parentWindow) == DialogResult.OK;
        }

        /// <summary>
        /// implements Initialize interface method: Called to initialize the user interface of the component.
        /// </summary>
        /// <param name="dtsComponentMetadata">SSIS metadata for the component</param>
        /// <param name="serviceProvider">serviceProvider</param>
        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            _metadata = dtsComponentMetadata;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// implements New interface method: called when a component is initially added to a Data Flow task.
        /// </summary>
        /// <param name="parentWindow">parentWindow</param>
        public void New(IWin32Window parentWindow) { }
        /// <summary>
        /// implements Help interface method: not used by SSIS yet
        /// </summary>
        /// <param name="parentWindow">parentWindow</param>
        public void Help(IWin32Window parentWindow) { }
        /// <summary>
        /// implements Delete interface method: Called when the component is deleted from the SSIS Designer surface.
        /// </summary>
        /// <param name="parentWindow">parentWindow</param>
        public void Delete(IWin32Window parentWindow) { }
    }
 
}
