using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataConverter.ComponentFramework.Controls
{
    /// <summary>
    /// Enhanced ComboBox that accepts bindings to the combobox properties and a datasource of type BindingList<string> for the ItemList.
    /// If not included in the ItemList, values of the combobox text property are added to the ItemList and shwon in red (as they are invalid).
    /// 
    /// Example:
    /// cmbTable.DataBindings.Add("Text", _IsagCustomProperties, "LU_Table");
    /// (Binds _IsagCustomProperties.LU_Table the combobox Text Property)
    /// cmbTable.SetItemSource(_sqlTableList); 
    /// (Sets _sqlTableList as the datsource for the ItemList of the combobox)
    /// </summary>
    public partial class IsagComboBox : ComboBox
    {
        /// <summary>
        /// DataSource for the ItemList
        /// </summary>
        private BindingList<string> _itemSource = new BindingList<string>();
       
        /// <summary>
        /// Setter for the ItemSource
        /// </summary>
        /// <param name="itemSource"></param>
        public void SetItemSource(BindingList<string> itemSource)
        {
            _itemSource.ListChanged -= ItemSource_ListChanged;
            _itemSource = itemSource;
            _itemSource.ListChanged += ItemSource_ListChanged;

            UpdateItemSource();
        }


        /// <summary>
        /// the constructor
        /// </summary>
        public IsagComboBox()
        {
            InitializeComponent();

            this.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            this.DropDown += IsagComboBox_DropDown;
            this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.DrawItem += IsagComboBox_DrawItem;            
        }

        /// <summary>
        /// Updates the ItemList:
        /// 1. Change Color and add this.Text to ItemSource if ItemSource does not contain this.Text 
        /// 2. Remove invalid items from ItemList (except this.Text) 
        /// </summary>
        private void UpdateItemSource()
        {
            //Change Color and add this.Text to ItemSource if this.Text is invalid
            if (!_itemSource.Contains(this.Text))
            {
                this.ForeColor = Color.Red;
                if (!this.Items.Contains(this.Text)) this.Items.Add(this.Text);
            }
            else this.ForeColor = Color.Black;

            //Remove invalid items from ItemList (except this.Text)
            for (int i = this.Items.Count - 1; i >= 0; i--)
            {
                string item = this.Items[i].ToString();
                if (item != this.Text && !_itemSource.Contains(item)) this.Items.RemoveAt(i);
            }
        }

        /// <summary>
        /// React if the DataSource for the ItemList has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.Items.Clear();
            this.Items.AddRange(_itemSource.ToArray<String>());
            UpdateItemSource();
        }


       
        /// <summary>
        /// Sets the Color of an item if combobox is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index != -1)
            {
                string itemValue = ((ComboBox)sender).Items[e.Index].ToString();
                bool isValid = _itemSource.Contains(itemValue);
                Color color = isValid ? Color.Black : Color.Red;

                using (var brush = new SolidBrush(color))
                    e.Graphics.DrawString(itemValue, e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }  

        /// <summary>
        /// ItemList has to be updated if combobox opens
        /// (necesarry because the text property could habe changed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsagComboBox_DropDown(object sender, EventArgs e)
        {
            UpdateItemSource();
        }
        //public void SetValueList(Type srcEnum)
        //{
        //    this.Items.Clear();

        //    foreach (Enum type in Enum.GetValues(srcEnum))
        //    {
        //        this.Items.Add(type);
        //    }
        //}

        //public void SetValueList(Type srcEnum, string[] stringValue)
        //{
        //    this.Items.Clear();
        //    Array enums = Enum.GetValues(srcEnum);

        //    //TODO TL3/DC: not needed in LU2
        //    for (int i = 0; i < enums.Length; i++)
        //    {
        //        //this.
        //        //this.ValueList.ValueListItems.Add(enums.GetValue(i), stringValue[i]);
        //    }
        //}
    }
}
