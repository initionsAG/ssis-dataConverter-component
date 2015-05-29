using System;
using System.Collections.Generic;
using System.Text;

namespace DataConverter
{
    public class StatusConvert
    {
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        

        public StatusConvert()
        {
            HasError = false;
            ErrorMessage = "";
        }

        public void SetError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
