using System;
using System.Collections.Generic;
using System.Text;

namespace DataConverter
{
    /// <summary>
    /// Status of conversion
    /// </summary>
    public class StatusConvert
    {
        /// <summary>
        /// At least one conversion failure happened
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// constructor
        /// </summary>
        public StatusConvert()
        {
            HasError = false;
            ErrorMessage = "";
        }

        /// <summary>
        /// Set Error
        /// </summary>
        /// <param name="message">error message</param>
        public void SetError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
