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
    class DataConverterUI : IDtsComponentUI
    {
        IDTSComponentMetaData100 _metadata;
        IServiceProvider _serviceProvider;

        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            frmDataConverterUI frm = new frmDataConverterUI(_metadata, variables);
            return  frm.ShowDialog(parentWindow) == DialogResult.OK;
        }

        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            _metadata = dtsComponentMetadata;
            _serviceProvider = serviceProvider;
        }

        public void New(IWin32Window parentWindow) { }
        public void Help(IWin32Window parentWindow) { }
        public void Delete(IWin32Window parentWindow) { }
    }
 
}
