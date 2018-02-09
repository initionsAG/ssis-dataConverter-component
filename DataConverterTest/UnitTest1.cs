using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.IO;
using Microsoft.SqlServer.Dts.DtsClient;
using System.Data;
using DataConverter;
using DataConverterTest.TestFramework;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;


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
        private bool _SavePackages = true;
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
        public void TestString2NumberFormats()
        {
            string[,] testparam = new string[,]
            {
                {"DT_WSTR", @"""1""", "DT_I1", "1"},
                {"DT_STR", @"""-2""", "DT_I2", "-2"},
                {"DT_WSTR", @"""678""", "DT_I4", "678"},
                {"DT_WSTR", @"""12345""", "DT_I8", "12345"},
                {"DT_STR", @"""1""", "DT_UI1", "1"},
                {"DT_WSTR", @"""2""", "DT_UI2", "2"},
                {"DT_WSTR", @"""678""", "DT_UI4", "678"},
                {"DT_STR", @"""12345""", "DT_UI8", "12345"},
                {"DT_WSTR", @"""1.12,34""", "DT_NUMERIC", "112.34"},
                {"DT_STR", @"""-2,34""", "DT_DECIMAL", "-2.34"},
                {"DT_WSTR", @"""678,456""", "DT_R4", "678.456"},
                {"DT_WSTR", @"""12345,678""", "DT_R8", "12345.678"},


            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        #region ByConversion

        [TestMethod]
        public void TestDate2MultipleByConversion()
        {
            object[,] testparam = new object[,]
            {
                {DateConvertTypes.YYYYMMDDHHMMSS, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_I8", @"(DT_I8) ""20151224150744"""},
                {DateConvertTypes.YYYYMMDDHHMM, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_I8", @"(DT_I8) ""201512241507"""},
                {DateConvertTypes.YYYYMMDDHH, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_I8", @"(DT_I8) ""2015122415"""},
                {DateConvertTypes.HHMMSS, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_I4", @"(DT_I8) ""150744"""},
                {DateConvertTypes.HHMM, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""15:07:44""", "DT_I4", @"(DT_I4) ""1507"""},
                {DateConvertTypes.YYYYMMDD, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_I8", @"(DT_I8) ""20151224"""},
                {DateConvertTypes.YYYYMM, "DT_DBDATE", @"(DT_DBDATE)""2015-12-24""", "DT_I4", @"(DT_I4) ""201512"""},
                {DateConvertTypes.YYYY, "DT_DBDATE", @"(DT_DBDATE)""2015-12-24""", "DT_I4", @"(DT_I4) ""2015"""},

                {DateConvertTypes.YYYYMMDDHHMMSS, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_WSTR", @"""20151224150744"""},
                {DateConvertTypes.YYYYMMDDHHMM, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_STR", @"""201512241507"""},
                {DateConvertTypes.YYYYMMDDHH, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_WSTR", @"""2015122415"""},
                {DateConvertTypes.HHMMSS, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""2015-12-24 15:07:44.000""", "DT_STR", @"""15:07:44"""},
                {DateConvertTypes.HHMM, "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP)""15:07:44""", "DT_WSTR", @"""15:07"""},
                {DateConvertTypes.YYYYMMDD, "DT_DATE", @"(DT_DBDATE)""2015-12-24""", "DT_WSTR", @"""20151224"""},
                {DateConvertTypes.YYYYMM, "DT_DBDATE", @"(DT_DBDATE)""2015-12-24""", "DT_WSTR", @"""201512"""},
                {DateConvertTypes.YYYY, "DT_DBDATE", @"(DT_DBDATE)""2015-12-24""", "DT_STR", @"""2015"""}
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName((string)testparam[i, 1]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName((string)testparam[i, 3]),
                    InputValueExpression = (string)testparam[i, 2],
                    ExpectedValueExpression = (string)testparam[i, 4],
                    Conversion = (DateConvertTypes)testparam[i, 0]
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestStringDate2MultipleByConversion()
        {
            string[,] testparam = new string[,]
            {
                {"YYYY.MM.DD", "DT_WSTR", @"""2014.12.24""", "DT_I4", "20141224"},
                {"DD.MM.YYYY", "DT_WSTR", @"""24.12.2014""", "DT_I4", "20141224"},
                {"YYYY/MM/DD", "DT_WSTR", @"""2014/12/24""", "DT_I4", "20141224"},
                {"DD/MM/YYYY", "DT_WSTR", @"""24/12/2016""", "DT_I4", "20161224"},
                {"YYYY-MM-DD", "DT_WSTR", @"""2014-12-24""", "DT_I4", "20141224"},
                {"DD-MM-YYYY", "DT_WSTR", @"""24-12-2014""", "DT_I4", "20141224"},
                {"MM-DD-YYYY", "DT_WSTR", @"""12-24-2014""", "DT_I4", "20141224"},

                {"YYYY.MM.DD", "DT_WSTR", @"""2014.12.24""", "DT_STR", "20141224"},
                {"DD.MM.YYYY", "DT_WSTR", @"""24.12.2014""", "DT_STR", "20141224"},
                {"YYYY/MM/DD", "DT_WSTR", @"""2014/12/24""", "DT_STR", "20141224"},
                {"DD/MM/YYYY", "DT_WSTR", @"""24/12/2016""", "DT_STR", "20161224"},
                {"YYYY-MM-DD", "DT_WSTR", @"""2014-12-24""", "DT_STR", "20141224"},
                {"DD-MM-YYYY", "DT_WSTR", @"""24-12-2014""", "DT_STR", "20141224"},
                {"MM-DD-YYYY", "DT_WSTR", @"""12-24-2014""", "DT_STR", "20141224"},

                {"YYYY.MM.DD", "DT_WSTR", @"""2014.12.24""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},
                {"DD.MM.YYYY", "DT_WSTR", @"""24.12.2014""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},
                {"YYYY/MM/DD", "DT_WSTR", @"""2014/12/24""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},
                {"DD/MM/YYYY", "DT_WSTR", @"""24/12/2016""", "DT_DATE", @"(DT_DATE) ""24.12.2016"""},
                {"YYYY-MM-DD", "DT_WSTR", @"""2014-12-24""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},
                {"DD-MM-YYYY", "DT_WSTR", @"""24-12-2014""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},
                {"MM-DD-YYYY", "DT_WSTR", @"""12-24-2014""", "DT_DATE", @"(DT_DATE) ""24.12.2014"""},

            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 1]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 3]),
                    InputValueExpression = testparam[i, 2],
                    ExpectedValueExpression = testparam[i, 4],
                    String_Conversion_Type = testparam[i, 0]
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestString2NumericByConversion()
        {
            object[,] testparam = new object[,]
            {
                {DateConvertTypes.Point2Comma, "DT_WSTR", @"""2.12""", "DT_NUMERIC", "2.12"},
                {DateConvertTypes.Comma2Point, "DT_WSTR", @"""2,12""", "DT_NUMERIC", "212.00000"},
                {DateConvertTypes.AmericanDecimal, "DT_WSTR", @"""2,121.145""", "DT_NUMERIC", "2121.14500"},
                {DateConvertTypes.GermanDecimal, "DT_WSTR", @"""2.121,145""", "DT_NUMERIC", "2121.14500"},
                {DateConvertTypes.Point2Comma, "DT_WSTR", @"""2.12""", "DT_I4", "3"}
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName((string)testparam[i, 1]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName((string)testparam[i, 3]),
                    InputValueExpression = (string)testparam[i, 2],
                    ExpectedValueExpression = (string)testparam[i, 4],
                    Conversion = (DateConvertTypes)testparam[i, 0]
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        #endregion

        #region Compare

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
        public void Test_CompareNull_Success()
        {
            string[,] testparam = new string[,]
              { { "DT_I4",@"NULL(DT_I4)","==","DT_I2", "1", "1"}
              };

            for (int i = 0; i < testparam.GetLength(0); i++)
            {

                Init();
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ComparedDataType = SsisDataType.GetDataTypeByName(testparam[i, 3]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = "1",
                    CompareValueExpression = testparam[i, 4],
                    CompareOperator = testparam[i, 2],
                    OnNull = testparam[i, 5]
                };

                TestResult testResult = _testPackage.StartTest(testConfig);
                Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
                Assert.AreEqual(0, testResult.CountErrorOutput);
                Assert.AreEqual(0, testResult.CountLogOutput);
            }
        }

        [TestMethod]
        public void Test_Compare_Success()
        {
            string[,] testparam = new string[,]
              { { "DT_I4", "1","==","DT_I2", "1"},
                { "DT_I4", "1",">=","DT_I4", "1"},
                { "DT_R4", "10.12","<=","DT_I4", "12.12"},
                { "DT_R4", "10.12","<","DT_R8", "12.12"},
                { "DT_WSTR", @"""def""",">","DT_WSTR", @"""abc"""},
                { "DT_STR", @"""def""","!=","DT_STR", @"""abc"""}
              };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ComparedDataType = SsisDataType.GetDataTypeByName(testparam[i, 3]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 1],
                    CompareValueExpression = testparam[i, 4],
                    CompareOperator = testparam[i, 2]
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }
        #endregion

        #region RegEx

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

        #endregion

        [TestMethod]
        public void TestNtext2Multiple()
        {
            string[,] testparam = new string[,]
            {
                { "DT_NTEXT", @"""2""", "DT_I2", "2"},
                { "DT_NTEXT", @"""äöü""","DT_WSTR", @"""äöü"""},
                { "DT_TEXT", @"""42""","DT_I4", "42"},
                { "DT_TEXT", @"""äöü""","DT_STR", @"""äöü"""},
                { "DT_TEXT", @"""äöü""","DT_NTEXT", @"""äöü"""},
                { "DT_NTEXT", @"""äöü""","DT_TEXT", @"""äöü"""},
                { "DT_NTEXT", @"""äöü""","DT_NTEXT", @"""äöü"""},
                { "DT_NTEXT", @"NULL(DT_NTEXT)", "DT_WSTR", @"NULL(DT_WSTR,255)"}
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                    AllowNull = true
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        #region ImpclicitDateConversion

        [TestMethod]
        public void TestImplicitDateConversion()
        {
            string[,] testparam = new string[,]
            {
                {"DT_WSTR", @"""12.10.1900""", "DT_DATE", @"(DT_DATE) ""12.10.1900"""},
                {"DT_STR", @"""12.10.1900""", "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP) ""12.10.1900"""},
                {"DT_WSTR", @"""12.10.1900""", "DT_DBDATE", @"(DT_DBDATE) ""12.10.1900"""},

                {"DT_I4", "20171210", "DT_DATE", @"(DT_DATE) ""10.12.2017"""},
                {"DT_I8", "20171210", "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP) ""10.12.2017"""},
                {"DT_UI4", "20171210", "DT_DATE", @"(DT_DATE) ""10.12.2017"""},
                {"DT_UI8", "20171210", "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP) ""10.12.2017"""},
                {"DT_NUMERIC", "20171210.00", "DT_DBDATE", @"(DT_DBDATE) ""10.12.2017"""},

                {"DT_DATE", @"(DT_DATE) ""12.10.1900""", "DT_I4", "19001012"},
                {"DT_DBDATE", @"(DT_DBTIMESTAMP) ""12.10.1900""", "DT_I4", "19001012"},
                {"DT_DBTIMESTAMP", @"(DT_DBDATE) ""12.10.1900""", "DT_I4", "19001012"},
                {"DT_DATE", @"(DT_DATE) ""12.10.1900""", "DT_I8", "19001012"},
                {"DT_DBDATE", @"(DT_DBTIMESTAMP) ""12.10.1900""", "DT_I8", "19001012"},
                {"DT_DBTIMESTAMP", @"(DT_DBDATE) ""12.10.1900""", "DT_I8", "19001012"},
                {"DT_DATE", @"(DT_DATE) ""12.10.1900""", "DT_UI4", "19001012"},
                {"DT_DBDATE", @"(DT_DBTIMESTAMP) ""12.10.1900""", "DT_UI4", "19001012"},
                {"DT_DBTIMESTAMP", @"(DT_DBDATE) ""12.10.1900""", "DT_UI4", "19001012"},
                {"DT_DATE", @"(DT_DATE) ""12.10.1900""", "DT_UI8", "19001012"},
                {"DT_DBDATE", @"(DT_DBTIMESTAMP) ""12.10.1900""", "DT_UI8", "19001012"},
                {"DT_DBTIMESTAMP", @"(DT_DBDATE) ""12.10.1900""", "DT_UI8", "19001012"},
                {"DT_DATE", @"(DT_DATE) ""12.10.1900""", "DT_I8", "19001012"},
                {"DT_DBDATE", @"(DT_DBTIMESTAMP) ""12.10.1900""", "DT_NUMERIC", "19001012"},
                {"DT_DBTIMESTAMP", @"(DT_DBDATE) ""12.10.1900""", "DT_NUMERIC", "19001012"},
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestImplicitDateConversion_Error()
        {
            string[,] testparam = new string[,]
            {
                {"DT_WSTR", @"""abc""", "DT_DATE", @"(DT_DATE) ""12.10.1900"""},
                {"DT_STR", @"""9999""", "DT_DBTIMESTAMP", @"(DT_DBTIMESTAMP) ""12.10.1900"""},
                {"DT_WSTR", @"""def""", "DT_DBDATE", @"(DT_DBDATE) ""12.10.1900"""},

                {"DT_I4", "42", "DT_DATE", @"(DT_DATE) ""10.12.2017"""},
                {"DT_I8", @"9.999999999 * 1000000000", "DT_DATE", @"(DT_DATE) ""10.12.2017"""},
                {"DT_NUMERIC", "0.42", "DT_DBDATE", @"(DT_DBDATE) ""10.12.2017"""},
            };
            
            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                if (i != 0) Init();

                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                };                

                TestResult testResult = _testPackage.StartTest(testConfig);

                Assert.AreEqual(0, testResult.CountOutput);
                Assert.AreEqual(1, testResult.CountErrorOutput);
                Assert.AreEqual(1, testResult.CountLogOutput); 
            }
        }

        #endregion

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
        public void TestString2Numeric_Language()
        {
            string[,] testparam = new string[,]
            {
                {"DT_WSTR", @"""2,121.145""", "DT_NUMERIC",  "2121.14500", "en-us"},
                {"DT_WSTR", @"""2.121,145""", "DT_NUMERIC",  "2121.14500", "de-de"}
            };

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                if (i != 0) Init();

                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                    Language = testparam[i,4]
                };

                TestResult testResult = _testPackage.StartTest(testConfig);

                Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
                Assert.AreEqual(0, testResult.CountErrorOutput);
                Assert.AreEqual(0, testResult.CountLogOutput);
            }
        }

        #region ErrorName
        [TestMethod]
        public void TestString2Int_ErrorNameVaraiable()
        {
            SsisDataType inputDataType = SsisDataType.GetDataTypeByName("DT_WSTR");
            SsisDataType expectedDataType = SsisDataType.GetDataTypeByName("DT_I4");

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
            SsisDataType inputDataType = SsisDataType.GetDataTypeByName("DT_WSTR");
            SsisDataType expectedDataType = SsisDataType.GetDataTypeByName("DT_I4");

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
        #endregion

        #region OnError
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
        #endregion

        [TestMethod]
        public void TestNumber2Number()
        {
            string[,] testparam = new string[,]
            {
                {"DT_I1", "1", "DT_I2", "1"},
                {"DT_I2", "-2", "DT_I4", "-2"},
                {"DT_I4", "4444", "DT_I8", "4444"},
                {"DT_I8", "-999999999", "DT_NUMERIC", "-999999999"},
                {"DT_UI1", "2", "DT_UI2", "2"},
                {"DT_UI2", "222", "DT_UI4", "222"},
                {"DT_UI4", "444", "DT_UI8", "444"},
                {"DT_UI8", "888", "DT_DECIMAL", "888"},
                {"DT_NUMERIC", "-100.234", "DT_DECIMAL", "-100.234"},
                {"DT_DECIMAL", "200.235", "DT_NUMERIC", "200.235"},
                {"DT_R4", "1142.901", "DT_NUMERIC", "1142.901"},
                {"DT_R8", "9123.123", "DT_NUMERIC", "9123.123"},
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestMultiple2String()
        {
            string[,] testparam = new string[,]
            {
                {"DT_I4", "1", "DT_WSTR", @"""1"""},
                {"DT_R4", "-2.879", "DT_STR", @"""-2,879"""},
                {"DT_DATE",  @"(DT_DATE)""2015-12-10""", "DT_WSTR", @"""10.12.2015 00:00:00"""},
                {"DT_CY", "-999999999.99", "DT_WSTR", @"""-999999999,99"""},              
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            Assert.AreEqual(0, testResult.CountLogOutput);
        }

        [TestMethod]
        public void TestOnErrorValue()
        {
            string[,] testparam = new string[,]
            {
                {"DT_WSTR", @"""999999999""", "DT_I1", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_I2", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_I4", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_I8", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_UI1", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_UI2", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_UI4", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_UI8", "1", "1"},
                {"DT_WSTR", @"""abc""", "DT_BOOL", "false", "false"},
                {"DT_WSTR", @"""abc""", "DT_CY", "145", "145"},
                {"DT_WSTR", @"""abc""", "DT_DATE", @"""14.11.2017""", "14.11.2017"},
                {"DT_WSTR", @"""abc""", "DT_DBDATE", @"""14.11.2017""", "14.11.2017"},
                {"DT_WSTR", @"""abc""", "DT_DBTIMESTAMP",@"""14.11.2017""", "14.11.2017"},
                {"DT_WSTR", @"""abc""", "DT_DECIMAL", "1.4", "1,4"},
                {"DT_WSTR", @"""1234567890123456789.123456789""", "DT_NUMERIC", "1.4", "1,4"},
                {"DT_WSTR", @"""abc""", "DT_R4", "42", "42"},
                {"DT_WSTR", @"""abc""", "DT_R8", "42", "42"},
                {"DT_I8", @"8", "DT_STR", @"""8""", "42"},
                {"DT_I4", @"42", "DT_WSTR", @"""42""", "4"},
                {"DT_I4", @"42", "DT_WSTR", @"""42""", "4"},
                {"DT_WSTR",@"""abc""", "DT_NTEXT",  @"""abc""", "text"},
                {"DT_WSTR",@"""abc""", "DT_TEXT",  @"""abc""", "text"},
            };
            ListTestConfiguration testConfigs = new ListTestConfiguration();

            for (int i = 0; i < testparam.GetLength(0); i++)
            {
                DataConverterTestConfiguration testConfig = new DataConverterTestConfiguration()
                {
                    InputDataType = SsisDataType.GetDataTypeByName(testparam[i, 0]),
                    ExpectedDataType = SsisDataType.GetDataTypeByName(testparam[i, 2]),
                    InputValueExpression = testparam[i, 1],
                    ExpectedValueExpression = testparam[i, 3],
                    OnError = testparam[i, 4]
                };

                testConfigs.Add(testConfig);
            }

            TestResult testResult = _testPackage.StartTest(testConfigs);

            Assert.IsTrue(testResult.IsSuccessful, testResult.ErrorMessage);
            Assert.AreEqual(0, testResult.CountErrorOutput);
            //Assert.AreEqual(1, testResult.CountLogOutput);
        }
    }
}

