using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConverterTest.TestFramework
{
    class TestResult
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public int CountOutput { get; set; }
        public int CountErrorOutput { get; set; }
        public int CountLogOutput { get; set; }
        public bool IsDataConverterErrorNameCorrect { get; set; }
        public string ErrorMessageDataConverterErrorName { get { return "DataConverter ErrorName is not correct."; } }

        public void AddTestResult(string filePath, string filePathError, string filePathLog)
        {
            if (File.Exists(filePath))
            {
                string result = File.ReadAllText(filePath);

                string[] resultLine = result.Split(Environment.NewLine.ToCharArray());
                IsSuccessful = true;
                List<string> outputValue = new List<string>();
                for (int i = 1; i < resultLine.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(resultLine[i]))
                    {
                        string[] resultValues = resultLine[i].Split(";".ToCharArray());
                        outputValue.Add(resultValues[0]);
                        IsSuccessful = IsSuccessful && resultValues[1].ToUpper() == "TRUE";
                    }
                }               

                if (!IsSuccessful) ErrorMessage = "converted value: " + outputValue[0];

                CountErrorOutput = GetCsvRowCount(filePathError);
                CountLogOutput = GetCsvRowCount(filePathLog);
                CountOutput = GetCsvRowCount(filePath);
                IsDataConverterErrorNameCorrect = GetIsDataConverterErrorNameCorrect(filePathError);
            }            
        }

        private bool GetIsDataConverterErrorNameCorrect(string filePath)
        {
            if (File.Exists(filePath))
            {
                string result = File.ReadAllText(filePath);
                string[] resultLine = result.Split(Environment.NewLine.ToCharArray());

                bool isNameCorrect = true;

                for (int i = 1; i < resultLine.Length-1; i++)
                {
                    isNameCorrect = isNameCorrect && (resultLine[i].ToUpper() == "TRUE");
                }

                return isNameCorrect;
            }
            else throw new Exception("Ouput csv file does not exist!" + Environment.NewLine + filePath);
        }

        private int GetCsvRowCount(string filePath)
        {
            if (File.Exists(filePath))
            {
                string result = File.ReadAllText(filePath);
                string[] resultLine = result.Split(Environment.NewLine.ToCharArray());

                //ignore empty array element (Count)
                //ignore header row (-1)
                return resultLine.Count(item => !string.IsNullOrEmpty(item)) - 1; 
            }
            else throw new Exception("Ouput csv file does not exist!" + Environment.NewLine + filePath);                
        }

        public void SetExecutionError()
        {
            IsSuccessful = false;
            ErrorMessage = "Package execution aborted.";
        }
    }
}
