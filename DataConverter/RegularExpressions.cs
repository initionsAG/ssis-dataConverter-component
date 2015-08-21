using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;

namespace DataConverter
{
    public class RegularExpressions : List<RegularExpression>
    {
        
        public static bool IsMatch(string value, string pattern)
        {
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            return rgx.IsMatch(value);

        }

        public static RegularExpressions LoadFromXml(string fileName)
        {
            RegularExpressions r = new RegularExpressions();
            RegularExpression re = new RegularExpression();
            re.Name = "Na";
            re.Pattern = "Pa";
            r.Add(re);

            StringBuilder builder;
            XmlSerializer serializer1;
            System.Xml.XmlWriter writer;
            XmlSerializerNamespaces namespaces;

            builder = new StringBuilder();
            serializer1 = new XmlSerializer(r.GetType());
            writer = System.Xml.XmlWriter.Create(builder);
            namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            serializer1.Serialize(writer, r, namespaces);

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

    public class RegularExpression
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
    }
}
