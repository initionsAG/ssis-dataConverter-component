using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;

namespace ComponentFramework.Controls
{
    public partial class IsagUltraComboEditor : UltraComboEditor
    {
        public IsagUltraComboEditor()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            this.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList; 
        }

        protected override void OnAfterCloseUp(EventArgs args)
        {
            base.OnAfterCloseUp(args);

            if (this.Editor.IsInEditMode) this.Editor.ExitEditMode(false, true);
        }

        public void SetValueList(Type srcEnum)
        {
            this.ValueList.ValueListItems.Clear();

            foreach (Enum type in Enum.GetValues(srcEnum))
            {
                this.ValueList.ValueListItems.Add(type);
            }
        }

        public void SetValueList(Type srcEnum, string[] stringValue)
        {
            this.ValueList.ValueListItems.Clear();
            Array enums = Enum.GetValues(srcEnum);

            for (int i = 0; i < enums.Length; i++)
            {
                this.ValueList.ValueListItems.Add(enums.GetValue(i), stringValue[i]);
            }
        }
    }
}
