using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;

namespace DataConverter
{
    public partial class frmEditor : Form
    {
        public string Value
        {
            get { return tbValue.Text; }
        }

        private string _inputColumnName;


        private List<string> _inputColumnNameList = new List<string>();
        public bool IsSelectedColumnValid
        {
            get
            {
                return _inputColumnNameList.Contains(cbColumnList.Text);
            }
        }

        public frmEditor(string inputColumnName, string value, string[] inputColumnNameList)
        {
            InitializeComponent();

            _inputColumnName = inputColumnName;
            tbValue.Text = value;
          
            ValueList valueList = new ValueList();
            foreach (string columnName in inputColumnNameList) valueList.ValueListItems.Add(columnName);
            _inputColumnNameList.AddRange(inputColumnNameList);
            cbColumnList.ValueList = valueList;
            cbColumnList.Text = inputColumnName;

            cbColumnList.TextChanged += new EventHandler(cbColumnList_TextChanged);
            cbColumnList.KeyUp += new KeyEventHandler(cbColumnList_TextChanged);
        }

        void cbColumnList_TextChanged(object sender, EventArgs e)
        {
            btnInsertInputColumn.Enabled = IsSelectedColumnValid;
        }

        private string GetInputReference()
        {
            return cbColumnList.Text;
        }
        private void btnInsertInputColumn_Click(object sender, EventArgs e)
        {
            Insert(GetInputReference());
        }



        private void Insert(string value)
        {
            object oldValue = System.Windows.Forms.Clipboard.GetDataObject();
            System.Windows.Forms.Clipboard.SetDataObject(value, true);
            tbValue.EditInfo.Paste();
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(oldValue, true);
            }
            catch (Exception){}
            
            tbValue.Focus();
        }
    }
}
