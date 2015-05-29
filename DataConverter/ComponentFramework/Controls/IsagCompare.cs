using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace DataConverter
{
    public partial class IsagCompare : UserControl
    {
        public IsagCompare()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get
            {
                return uceOp.Text;
            }
            set
            {
                uceOp.Text = value;
            }
        }

      
    }
}
