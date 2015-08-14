using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Win;
using System.Windows.Forms;
using Infragistics.Win.UltraMessageBox;

namespace DataConverter
{
    public class Common
    {
        public static bool ContainsOnce(string str, string contained)
        { 
            int posFirst = str.IndexOf(contained);
            int posLast = str.LastIndexOf(contained);

            return posFirst != -1 && posFirst == posLast;
        }

        public static ValueList CreateValueList(List<object> items)
        {
            ValueList result = new ValueList();

            foreach (object item in items)
            {
                result.ValueListItems.Add(item.ToString());
            }

            return result;

        }

        /// <summary>
        /// Message anzeigen
        /// </summary>
        /// <returns>DialogResult</returns>
        public static DialogResult ShowMessage(string message, string header, MessageBoxIcon icon, MessageBoxButtons buttons, UltraMessageBoxManager ultraMessageBox)
        {
            UltraMessageBoxInfo info = new UltraMessageBoxInfo();
            info.TextFormatted = message;
            info.Header = header;
            info.Caption = "DataConverter";
            info.Icon = icon;
            info.Buttons = buttons;
            return ultraMessageBox.ShowMessageBox(info);

        }

        public static List<object> GetListFromEnum(Type srcEnum)
        {
            List<object> result = new List<object>();
            Array enums = Enum.GetValues(srcEnum);

            for (int i = 0; i < enums.Length; i++)
            {
                result.Add(enums.GetValue(i).ToString());
            }

            return result;
        }
    }
}
