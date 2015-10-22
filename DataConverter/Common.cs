using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


namespace DataConverter
{
    public class Common
    {
        /// <summary>
        /// Does a string contain a string only once?
        /// </summary>
        /// <param name="str">whole string</param>
        /// <param name="contained">conatined string</param>
        /// <returns></returns>
        public static bool ContainsOnce(string str, string contained)
        { 
            int posFirst = str.IndexOf(contained);
            int posLast = str.LastIndexOf(contained);

            return posFirst != -1 && posFirst == posLast;
        }
                  
    }
}
