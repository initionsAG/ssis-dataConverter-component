using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.IO;
using Microsoft.SqlServer.Dts.DtsClient;
using System.Data;
using DataConverter;
using DataConverterTest.TestFramework;


//using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace DataConverterTest
{
    [TestClass]
    public class UnitTest1
    {
        private TestPackage _testPackage;
        private string _outputfile;
        private string _outputfileError;
        private string _outputfileLog;
        private string _inputfile;

        //Parameter for testing new tests
        //
        //_SavePackages = true -> packages will be saved in output directory of Visual Studio
        //i.e.. C:\Visual Studio\Visual Studio TFS\Henry_SSIS\Henry.SSIS.DataConverter\DataConverterTest\bin\Debug
        //gespeichert.
        //
        //_deleteInputCsvFile = false -> Input CSV Files werden nicht gelöscht, die gespeicherten Pakete sind dadurch lauffähig
        //i.e. C:\Users\Henry\AppData\Local\Temp
        private bool _SavePackages = false;
        private bool _deleteInputCsvFile = true;
        private bool _deleteOutputCsvFile = true;

        [TestInitialize]
        public void Init()
        {
            string guid = Guid.NewGuid().ToString();
            _outputfile = Path.GetTempPath() + "output_" + guid + ".csv";
            _outputfileError = Path.GetTempPath() + "outputError_" + guid + ".csv";
            _outputfileLog = Path.GetTempPath() + "outputLog_" + guid + ".csv";
            _inputfile = Path.GetTempPath() + "input_" + guid + ".csv";
            File.WriteAllText(_inputfile, "dummy" + Environment.NewLine);
            _testPackage = new TestPackage(_inputfile, _outputfile, _outputfileError, _outputfileLog);
            _testPackage.SavePackage = _SavePackages;
        }

        [TestCleanup]
        public void Finish()
        {
            if (File.Exists(_inputfile) && _deleteInputCsvFile) File.Delete(_inputfile);
            if (File.Exists(_outputfile) && _deleteOutputCsvFile) File.Delete(_outputfile);
        }

        [TestMethod]
        public void TestString2Integer()
        {
            Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] dataTypes =
                 new Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] {
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I1,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I2,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I8 };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            foreach (Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType dataType in dataTypes)
            {

                SsisDataType inputDataType = new SsisDataType()
                {
                    Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                    Length = 255
                };

                SsisDataType expectedDataType = new SsisDataType()
                {
                    Type = dataType
                };

                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = inputDataType,
                    ExpectedDataType = expectedDataType,
                    InputValueExpression = @"""3""",
                    ExpectedValueExpression = "3",
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);


        }

        [TestMethod]
        public void TestString2Int_OnError()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""a""",
                ExpectedValueExpression = "0",
                OnError = "0"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Int_OnError_ErrorCount()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""a""",
                ExpectedValueExpression = "0",
                OnError = "0",
                UseErrorCount = true,
                ExpectedErrorCount = 1
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""3""",
                ExpectedValueExpression = "3"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Date()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""12.10.1900""",
                ExpectedValueExpression = @"(DT_DATE) ""12.10.1900"""
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestInt2Date()
        {
            Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] inputDataTypes =
                new Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] {
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I8 };

            Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] outputDataTypes =
                new Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] {
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DBDATE ,
               Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DBTIMESTAMP};

            ListTestConfiguration testConfigs = new ListTestConfiguration();

            foreach (Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType dataTypeInput in inputDataTypes)
            {
                foreach (Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType dataTypeOutput in outputDataTypes)
                {
                    SsisDataType inputDataType = new SsisDataType()
                    {
                        Type = dataTypeInput
                    };

                    SsisDataType expectedDataType = new SsisDataType()
                    {
                        Type = dataTypeOutput
                    };

                    DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                    {
                        InputDataType = inputDataType,
                        ExpectedDataType = expectedDataType,
                        InputValueExpression = @"19001012",
                        ExpectedValueExpression = @"(DT_DATE) ""12.10.1900""" //dummy
                    };

                    testConfigs.Add(testConfig);
                }
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(1, testResult.CountOutput);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestInt2Date_Error()
        {
            Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] inputDataTypes =
                new Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] {
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I8 };

            Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] outputDataTypes =
                new Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType[] {
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE,
                     Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DBDATE ,
                Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DBTIMESTAMP};

            ListTestConfiguration testConfigs = new ListTestConfiguration();

            foreach (Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType dataTypeInput in inputDataTypes)
            {
                foreach (Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType dataTypeOutput in outputDataTypes)
                {
                    SsisDataType inputDataType = new SsisDataType()
                    {
                        Type = dataTypeInput
                    };

                    SsisDataType expectedDataType = new SsisDataType()
                    {
                        Type = dataTypeOutput
                    };

                    DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                    {
                        InputDataType = inputDataType,
                        ExpectedDataType = expectedDataType,
                        InputValueExpression = @"10000",
                        ExpectedValueExpression = @"(DT_DATE) ""12.10.1900""" //dummy
                    };

                    testConfigs.Add(testConfig);
                }
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.AreEqual(0, testResult.CountOutput);
            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestI82Date_Error()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I8
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"9.999999999 * 1000000000", //workaround as DerivedColumn does not accept bigint values like 9999999999
                ExpectedValueExpression = @"(DT_DATE) ""12.10.1900""" //dummy
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.AreEqual(0, testResult.CountOutput);
            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }
        [TestMethod]
        public void TestString2Date_OnNull()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"NULL(DT_WSTR, 255)",
                ExpectedValueExpression = @"(DT_DATE) ""12.10.1900""",
                OnNull = "12.10.1900"
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Date_AllowNull()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"NULL(DT_WSTR, 255)",
                ExpectedValueExpression = @"(DT_DATE) ""12.10.1900""", //dummy
                AllowNull = false
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.AreEqual(0, testResult.CountOutput);
            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestDate2Int()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DBDATE
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"(DT_DBDATE) ""12.10.1900""",
                ExpectedValueExpression = "19001012"

            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2String_RegExEmail()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 30
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""Henry@initions.com""",
                ExpectedValueExpression = @"""Henry@initions.com""",
                RegEx = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])"
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2String_RegExEmail_Invalid()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 30
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""Henryinitionscom""",
                ExpectedValueExpression = @"""Henryinitionscom""", //dummy as not needed for result, but for package execution
                RegEx = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])"
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.AreEqual(0, testResult.CountOutput);
            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestDate2String_ConversionYYYYMM()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 6
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"(DT_DATE) ""24.12.2016""",
                ExpectedValueExpression = @"""201612""",
                Conversion = DateConvertTypes.YYYYMM
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Date_StringConversion()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 10
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_DATE
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""24/12/2016""",
                ExpectedValueExpression = @"(DT_DATE) ""24.12.2016""",
                String_Conversion_Type = "DD/MM/YYYY"
            };
            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_ConversionPoint2Comma()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2022.42""",
                ExpectedValueExpression = "2022.42",
                Conversion = DateConvertTypes.Point2Comma
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_ConversionComma2Point()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2022,42""",
                ExpectedValueExpression = "202242",
                Conversion = DateConvertTypes.Comma2Point
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_AmericanDecimal()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2,121.145""",
                ExpectedValueExpression = "2121.14500",
                Conversion = DateConvertTypes.AmericanDecimal
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_GermanDecimal()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2.121,145""",
                ExpectedValueExpression = "2121.14500",
                Conversion = DateConvertTypes.GermanDecimal
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_LanguageEN()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2,121.145""",
                ExpectedValueExpression = "2121.14500",
                Language = "en-us"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2Numeric_LanguageDE()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_NUMERIC,
                Scale = 5,
                Precision = 18
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""2.121,145""",
                ExpectedValueExpression = "2121.14500",
                Language = "de-de"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestInt_Compare_Failure()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = inputDataType,
                ComparedDataType = inputDataType,
                InputValueExpression = "1",
                ExpectedValueExpression = "1",
                CompareValueExpression = "42",
                CompareOperator = ">"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.AreEqual(0, testResult.CountOutput);
            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestInt_CompareSuccess()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = inputDataType,
                ComparedDataType = inputDataType,
                InputValueExpression = "1",
                ExpectedValueExpression = "1",
                CompareValueExpression = "42",
                CompareOperator = "<"
                //Compare = "inputValue < compareValue"
            };

            TestResult testResult = _testPackage.StartTest(testConfig);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }


        [TestMethod]
        public void TestString2IntMultiple()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            ListTestConfiguration testConfigList = new ListTestConfiguration();


            testConfigList.Add(new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""3""",
                ExpectedValueExpression = "3",
            });

            testConfigList.Add(new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""4""",
                ExpectedValueExpression = "4",
            });

            TestResult testResult = _testPackage.StartTest(testConfigList);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }


        [TestMethod]
        public void TestString2Int_ErrorNameVaraiable()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""a""",
                ExpectedValueExpression = "3",
            };

            ListTestConfiguration testConfigList = new TestFramework.ListTestConfiguration();
            testConfigList.DataConverterErrorName = "System::StartTime";
            testConfigList.IsDataConverterErrorNameVariable = true;
            testConfigList.Add(testConfig);

            TestResult testResult = _testPackage.StartTest(testConfigList);

            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
            Assert.IsTrue(testResult.IsDataConverterErrorNameCorrect, testResult.ErrorMessageDataConverterErrorName);
        }

        [TestMethod]
        public void TestString2Int_ErrorNameText()
        {
            SsisDataType inputDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            };

            SsisDataType expectedDataType = new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4
            };

            DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
            {
                InputDataType = inputDataType,
                ExpectedDataType = expectedDataType,
                InputValueExpression = @"""a""",
                ExpectedValueExpression = "3",
            };

            ListTestConfiguration testConfigList = new TestFramework.ListTestConfiguration();
            testConfigList.DataConverterErrorName = "TestErrorName";
            testConfigList.Add(testConfig);

            TestResult testResult = _testPackage.StartTest(testConfigList);

            Assert.AreEqual(1, testResult.CountErrorOutput);
            Assert.AreEqual(1, testResult.CountLogOutput);
            Assert.IsTrue(testResult.IsDataConverterErrorNameCorrect);
        }
    }
}

