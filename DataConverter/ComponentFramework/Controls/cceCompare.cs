using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win;


namespace DataConverter
{
    class cceCompare : ControlContainerEditor
    {



        private IsagCompare _compareEditor;
     
        private IsagCompare _compareRenderer;
      

        protected override object RendererValue
        {
            get
            {
                return _compareRenderer.Text;
            }
            set
            {
                _compareRenderer.Text = value.ToString();
            }
        }


        public cceCompare(IsagCompare compareEditor, IsagCompare compareRenderer)
        {

            _compareRenderer = compareRenderer;
            _compareEditor = compareEditor;
            // Set the Gauge control as the Rendering control
           this.RenderingControl = compareRenderer; // extBox;

            // Set the NumericUpDown control as the Editing control
            this.EditingControl = compareEditor;

       

        }
       
    }
}
