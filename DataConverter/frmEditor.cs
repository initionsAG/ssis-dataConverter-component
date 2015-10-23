using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DataConverter
{
    /// <summary>
    /// Window form for editing a compare expression
    /// </summary>
    public partial class frmEditor : Form
    {
        /// <summary>
        /// The compare expression to edit
        /// </summary>
        public string Value
        {
            get { return tbValue.Text; }
        }

        /// <summary>
        /// Is the selected column valid?
        /// (-> does the combobox itemlist contain the column?)
        /// </summary>
        public bool IsSelectedColumnValid
        {
            get
            {
                return cbColumnList.Items.Contains(cbColumnList.Text);
            }
        }

        /// <summary>
        /// contructor
        /// Initializes properties, fills gui elements
        /// </summary>
        /// <param name="inputColumnName">input column name for the compare expression</param>
        /// <param name="value">current compare expression</param>
        /// <param name="inputColumnNameList">list of input column names</param>
        public frmEditor(string inputColumnName,string value, string[] inputColumnNameList)
        {
            InitializeComponent();
           
            tbValue.Text = value;
                      
            cbColumnList.Items.AddRange(inputColumnNameList);
            cbColumnList.Text = inputColumnName;

            cbColumnList.TextChanged += new EventHandler(cbColumnList_TextChanged);
            cbColumnList.KeyUp += new KeyEventHandler(cbColumnList_TextChanged);
        }

        /// <summary>
        /// Reacts on text change event
        /// (Insert button is enabled only if selected column is valid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbColumnList_TextChanged(object sender, EventArgs e)
        {
            btnInsertInputColumn.Enabled = IsSelectedColumnValid;
        }
 
        /// <summary>
        /// Insert selected column into compare expression
        /// </summary>
        /// <param name="value">column name of selected column</param>
        private void Insert(string value)
        {
            tbValue.Text = tbValue.Text.Insert(tbValue.SelectionStart, value);          
           
            tbValue.Focus();
        }

        /// <summary>
        /// Insert selected column into compare expression
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event arguments</param>
        private void btnInsertInputColumn_Click_1(object sender, EventArgs e)
        {
            Insert(cbColumnList.Text);
        }
    }
}
