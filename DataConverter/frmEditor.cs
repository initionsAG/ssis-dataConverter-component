using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DataConverter
{
    public partial class frmEditor : Form
    {
        public string Value
        {
            get { return tbValue.Text; }
        }

        


        
        public bool IsSelectedColumnValid
        {
            get
            {
                return cbColumnList.Items.Contains(cbColumnList.Text);
            }
        }

        public frmEditor(string inputColumnName,string value, string[] inputColumnNameList)
        {
            InitializeComponent();

           
            tbValue.Text = value;
                      
            cbColumnList.Items.AddRange(inputColumnNameList);
            cbColumnList.Text = inputColumnName;

            cbColumnList.TextChanged += new EventHandler(cbColumnList_TextChanged);
            cbColumnList.KeyUp += new KeyEventHandler(cbColumnList_TextChanged);
        }

        void cbColumnList_TextChanged(object sender, EventArgs e)
        {
            btnInsertInputColumn.Enabled = IsSelectedColumnValid;
        }
 
        private void Insert(string value)
        {
            tbValue.Text = tbValue.Text.Insert(tbValue.SelectionStart, value);          
           
            tbValue.Focus();
        }

        private void btnInsertInputColumn_Click_1(object sender, EventArgs e)
        {
            Insert(cbColumnList.Text);
        }
    }
}
