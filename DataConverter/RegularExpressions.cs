using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;

namespace DataConverter
{
    /// <summary>
    /// List of regular expressions
    /// </summary>
    public class RegularExpressions : List<RegularExpression>
    {
        /// <summary>
        /// Does the regular expression finds a match in the string value?
        /// </summary>
        /// <param name="value">string value</param>
        /// <param name="pattern">regular expression pattern</param>
        /// <returns></returns>
        public static bool IsMatch(string value, string pattern)
        {
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            return rgx.IsMatch(value);

        }

        /// <summary>
        /// Creates an instance of RegularExpressions
        /// (if regex list cannot be loaded from file an emptey list will be created)
        /// </summary>
        /// <param name="fileName">filename (file contains a list of regular expression templates)</param>
        /// <returns>List of regular expressions</returns>
        public static RegularExpressions LoadFromXml(string fileName)
        {
            //RegularExpressions regExList = new RegularExpressions();
            //RegularExpression regEx = new RegularExpression();
            //regEx.Name = "Na";
            //regEx.Pattern = "Pa";
            //regExList.Add(regEx);

            //StringBuilder builder;
            //XmlSerializer serializer1;
            //System.Xml.XmlWriter writer;
            //XmlSerializerNamespaces namespaces;

            //builder = new StringBuilder();
            //serializer1 = new XmlSerializer(regExList.GetType());
            //writer = System.Xml.XmlWriter.Create(builder);
            //namespaces = new XmlSerializerNamespaces();
            //namespaces.Add("", "");
            //serializer1.Serialize(writer, regExList, namespaces);

            RegularExpressions result;

            try
            {
                StreamReader sr = File.OpenText(fileName);

                XmlSerializer serializer = new XmlSerializer(typeof(RegularExpressions));
                result = (RegularExpressions)serializer.Deserialize(sr);

                sr.Close();
            }
            catch (Exception)
            {
                result = new RegularExpressions();
            }            

            return result;
        }

      
    }

    /// <summary>
    /// Definition for a regular expression
    /// </summary>
    public class RegularExpression
    {
        /// <summary>
        /// Regular expressions name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Regular expression pattern
        /// </summary>
        public string Pattern { get; set; }
    }
}
