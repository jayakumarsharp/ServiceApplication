using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Schema;

namespace MSMQ_RFService
{
    public class CallDataValidator
    {
        private bool _isValid;
        private List<string> _validationErrMsgs = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public CallDataValidator()
        {
            _isValid = true;
            _validationErrMsgs = new List<string>();
        }

        /// <summary>
        /// To validate the call data against the XSD (Xml Schema Definition)
        /// </summary>
        /// <param name="callData">IVR Call Data</param>
        /// <param name="callDataXsd">IVR Call Data Schema</param>
        /// <param name="validationMsg">To hold the validation errors</param>
        /// <returns></returns>
        public bool Validate(string callData, string callDataXsd, ref List<string> validationMsg)
        {
            XmlReader xmlReader = null;
            XmlReaderSettings xmlSetting = null;
            XmlSchema xmlSchema = null;
            try
            {
                _isValid = true;
                _validationErrMsgs.Clear();
                xmlSchema = XmlSchema.Read(new System.IO.StringReader(callDataXsd), null);
                xmlSetting = new XmlReaderSettings();
                xmlSetting.Schemas.Add(xmlSchema);
                xmlSetting.ValidationType = ValidationType.Schema;
                //This event will be raised when the xml reader encounters validation error(while reading the call data xml node by node) 
                xmlSetting.ValidationEventHandler += new ValidationEventHandler(CallData_ValidationEventHandler);
                xmlReader = XmlReader.Create(new System.IO.StringReader(callData), xmlSetting);
                while (xmlReader.Read()) ;
                validationMsg = _validationErrMsgs;
                return _isValid;
            }
            finally
            {
                if (xmlReader != null) xmlReader.Close();
                xmlSchema = null;
                xmlSetting = null;
            }
        }

        /// <summary>
        /// XSD validator event method (invoked in case of XSD validation failure)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallData_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            _isValid = false;
            _validationErrMsgs.Add(e.Message);
        }
    }
}