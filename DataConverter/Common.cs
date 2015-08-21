using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


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
