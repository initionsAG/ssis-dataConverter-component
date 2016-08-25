using DataConverter;
using DataConverterTest.TestFramework;
using DataConverterTest.TestFramework.Components;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class TestPackage
    {
        public DFT Dft { get; set; }
        public ConnectionManagerFlatFile CnMgrSource { get; set; }
        public ConnectionManagerFlatFile CnMgrDestination { get; set; }
        public ConnectionManagerFlatFile CnMgrDestinationError { get; set; }
        public ConnectionManagerFlatFile CnMgrDestinationLog { get; set; }
        public FlatFileSource FF_Src { get; set; }
        public FlatFileDestination FF_Dest { get; set; }
        public FlatFileDestination FF_Dest_Error { get; set; }
        public FlatFileDestination FF_Dest_Log { get; set; }
        public DerivedColumn DER_Init { get; set; }
        public DerivedColumn DER_Output { get; set; }
        public DerivedColumn DER_LogOutput { get; set; }
        public DerivedColumn DER_ErrorOutput { get; set; }
        public DataConverterComponent DC { get; set; }

        private string OUTPUTFILE = "output" + Guid.NewGuid().ToString() + ".csv";
        private string INPUTFILE = "input" + Guid.NewGuid().ToString() + ".csv";

        private Package _package;
        private string _inputfile;
        private string _outputfile;
        private string _outputfileError;
        private string _outputfileLog;

        public bool SavePackage { get; set; }

        public TestPackage(string inputfile, string outputfile, string outputfileError, string outputfileLog)
        {
            _package = new Package();
            _package.ProtectionLevel = DTSProtectionLevel.DontSaveSensitive;

            _inputfile = inputfile;
            _outputfile = outputfile;
            _outputfileError = outputfileError;
            _outputfileLog = outputfileLog;

            //Create DFT
            Dft = new DFT(_package.Executables.Add(DFT.MONIKER), "DFT");

            //Create ConnectionManager
            AddFlatFileConnectionManagerSource();
            AddFlatFileConnectionManagerDestination();
            AddFlatFileConnectionManagerDestinationError();
            AddFlatFileConnectionManagerDestinationLog();

            //Create DataFlow Components
            FF_Src = Dft.CreateFlatFileSource("FF_SRC", CnMgrSource);
            DER_Init = Dft.CreateDerivedColumn("DER_COL Init");
            DER_Output = Dft.CreateDerivedColumn("DER_COL output");

            DER_LogOutput = Dft.CreateDerivedColumn("DER_COL logOutput");
            DER_LogOutput.AddOutputColumn("HasLog",
                new SsisDataType() { Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BOOL },
                "true");

            DER_ErrorOutput = Dft.CreateDerivedColumn("DER_COL errorOutput");           

            DC = Dft.CreateDataConverter("DC data conversions");
            FF_Dest = Dft.CreateFlatFileDestination("FF_dest", CnMgrDestination);
            FF_Dest_Error = Dft.CreateFlatFileDestination("FF_Dest_Error", CnMgrDestinationError);
            FF_Dest_Log = Dft.CreateFlatFileDestination("FF_Dest_Log", CnMgrDestinationLog);

            //Attach DataFlow Components
            Dft.ConnectComponents(FF_Src.Metadata, DER_Init.Metadata);
            Dft.ConnectComponents(DER_Init.Metadata, DC.Metadata);
            Dft.ConnectComponents(DC.Metadata, DER_Output.Metadata);
            Dft.ConnectComponents(DC.Metadata, DER_LogOutput.Metadata, 2, 0);
            Dft.ConnectComponents(DC.Metadata, DER_ErrorOutput.Metadata, 3, 0);
            Dft.ConnectComponents(DER_Output.Metadata, FF_Dest.Metadata);
            Dft.ConnectComponents(DER_ErrorOutput.Metadata, FF_Dest_Error.Metadata);
            Dft.ConnectComponents(DER_LogOutput.Metadata, FF_Dest_Log.Metadata);
        }

        private void AddFlatFileConnectionManagerSource()
        {
            ConnectionManager con = _package.Connections.Add(ConnectionManagerFlatFile.MONIKER);
            CnMgrSource = new ConnectionManagerFlatFile(con, "FF_Input", _inputfile);
            CnMgrSource.AddFlatFileColumn("dummy", lastColumn: true);
        }

        //TODO: derzeit nur Ausgabe von C_inputValue möglich (Problem: mehrere TestConfigs)
        private void AddFlatFileConnectionManagerDestination()
        {
            ConnectionManager con = _package.Connections.Add(ConnectionManagerFlatFile.MONIKER);
            CnMgrDestination = new ConnectionManagerFlatFile(con, "FF_Output", _outputfile);
            CnMgrDestination.AddFlatFileColumn("result");
            CnMgrDestination.AddFlatFileColumn("success", lastColumn: true);
        }

        private void AddFlatFileConnectionManagerDestinationError()
        {
            ConnectionManager con = _package.Connections.Add(ConnectionManagerFlatFile.MONIKER);
            CnMgrDestinationError = new ConnectionManagerFlatFile(con, "FF_Output_Error", _outputfileError);
            CnMgrDestinationError.AddFlatFileColumn("IsDataConverterErrorNameCorrect", lastColumn: true);
        }

        private void AddFlatFileConnectionManagerDestinationLog()
        {
            ConnectionManager con = _package.Connections.Add(ConnectionManagerFlatFile.MONIKER);
            CnMgrDestinationLog = new ConnectionManagerFlatFile(con, "FF_Output_Log", _outputfileLog);
            CnMgrDestinationLog.AddFlatFileColumn("HasLog", lastColumn: true);
        }

        private void AddTestResultOutputColumn(ListTestConfiguration testConfigList)
        {
            string successFriendlyExpression = "";
            string resultFriendlyExpression = "";
            foreach (DataConverterTestConfiguration testConfig in testConfigList)
            {
                //Success
                if (successFriendlyExpression != "")
                {
                    successFriendlyExpression += " && ";
                }
                successFriendlyExpression += "C_" + testConfig.InputColumnName + " == " + testConfig.ExpectedColumnName;
                if (testConfig.UseErrorCount) successFriendlyExpression += " && isETL_errorCount == " + testConfig.ExpectedErrorCount.ToString();
                successFriendlyExpression = "(" + successFriendlyExpression + ")";

                //result
                if (resultFriendlyExpression != "")
                {
                    resultFriendlyExpression += "+\"\t|\t\" + ";
                }
                resultFriendlyExpression +=
                    "\"Input: \" +  (DT_WSTR,255)" + testConfig.InputColumnName +
                    " + \" | converted: \" +  (DT_WSTR,255)C_" + testConfig.InputColumnName +
                    " + \" | expected: \" +  (DT_WSTR,255)" + testConfig.ExpectedColumnName +
                    " + \" | success: \" + (DT_WSTR,255)(C_" + testConfig.InputColumnName + " == " + testConfig.ExpectedColumnName + ")";
            }

            DER_Output.AddExpressionOutputColumn("success", new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BOOL
            },
                successFriendlyExpression
            );

            DER_Output.AddExpressionOutputColumn("result", new SsisDataType()
            {
                Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_WSTR,
                Length = 255
            },
                resultFriendlyExpression
            );


        }

        public TestResult StartTest(DataConverterTestConfiguration testConfig)
        {
            ListTestConfiguration listConfigTest = new ListTestConfiguration();
            listConfigTest.Add(testConfig);
            return StartTest(listConfigTest);

            //DER_Init.AddOutputColumn(testConfig.InputColumnName, testConfig.InputDataType, testConfig.InputValueExpression);
            //DER_Init.AddOutputColumn(testConfig.ExpectedColumnName, testConfig.ExpectedDataType, testConfig.ExpectedValueExpression);
            //if (testConfig.UseErrorCount)
            //    DER_Init.AddOutputColumn("isETL_errorCount", new SsisDataType() { Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4 }, "0");
            //if (testConfig.HasCompare)
            //    DER_Init.AddOutputColumn(testConfig.CompareColumnName, testConfig.ComparedDataType, testConfig.CompareValueExpression);

            //DC.ConfigureDataConverter(testConfig);

            //AddTestResultOutputColumn(testConfig);

            //FF_Dest.MapColumns();
            //FF_Dest_Error.MapColumns();
            //FF_Dest_Log.MapColumns();

            //if (SavePackage)
            //{
            //    Application app = new Application();
            //    app.SaveToXml("TestPackage" + Guid.NewGuid().ToString() + ".dtsx", _package, null);
            //}

            //TestResult testResult = new TestResult();

            //if (_package.Execute() != DTSExecResult.Success) testResult.SetExecutionError();
            //else testResult.AddTestResult(_outputfile, _outputfileError, _outputfileLog);

            //return testResult;
        }

        public TestResult StartTest(ListTestConfiguration testConfigList)
        {
            foreach (DataConverterTestConfiguration testConfig in testConfigList)
            {
                StartFirstStep(testConfig);
            }

            DC.Reset();
            DC.SetErrorName(testConfigList.DataConverterErrorName);

            foreach (DataConverterTestConfiguration testConfig in testConfigList)
            {
                DC.ConfigureDataConverter(testConfig);
            }

            StartSecondStep(testConfigList);
            return StartLastStep();
        }

        private void StartFirstStep(DataConverterTestConfiguration testConfig)
        {
            DER_Init.AddOutputColumn(testConfig.InputColumnName, testConfig.InputDataType, testConfig.InputValueExpression);
            DER_Init.AddOutputColumn(testConfig.ExpectedColumnName, testConfig.ExpectedDataType, testConfig.ExpectedValueExpression);
            if (testConfig.UseErrorCount)
                DER_Init.AddOutputColumn("isETL_errorCount", new SsisDataType() { Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_I4 }, "0");
            if (testConfig.HasCompare)
                DER_Init.AddOutputColumn(testConfig.CompareColumnName, testConfig.ComparedDataType, testConfig.CompareValueExpression);

           

        }

        private void StartSecondStep(ListTestConfiguration testConfigList)
        {
            AddTestResultOutputColumn(testConfigList);

            string friendlyExpression;

            friendlyExpression = testConfigList.IsDataConverterErrorNameVariable ?
                "Name ==  (DT_WSTR, 255) @[" + testConfigList.DataConverterErrorName + "] " :
                "Name == \"" + testConfigList.DataConverterErrorName + "\" ";

            DER_ErrorOutput.AddOutputColumn("IsDataConverterErrorNameCorrect",
              new SsisDataType() { Type = Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType.DT_BOOL },
              friendlyExpression);
        }

        private TestResult StartLastStep()
        {
            FF_Dest.MapColumns();
            FF_Dest_Error.MapColumns();
            FF_Dest_Log.MapColumns();

            if (SavePackage)
            {
                Application app = new Application();
                app.SaveToXml("TestPackage" + Guid.NewGuid().ToString() + ".dtsx", _package, null);
            }

            TestResult testResult = new TestResult();

            if (_package.Execute() != DTSExecResult.Success) testResult.SetExecutionError();
            else testResult.AddTestResult(_outputfile, _outputfileError, _outputfileLog);

            return testResult;
        }


    }
}
