using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class ListTestConfiguration: List<DataConverterTestConfiguration>
    {
        public string DataConverterErrorName { get; set; }
        public bool IsDataConverterErrorNameVariable { get; set; }

        public ListTestConfiguration()
        {
            DataConverterErrorName = "";
            IsDataConverterErrorNameVariable = false;
        }

        public new void Add(DataConverterTestConfiguration testConfig)
        {
            testConfig.AddPostfix(this.Count());
            base.Add(testConfig);
        }
    }
}
