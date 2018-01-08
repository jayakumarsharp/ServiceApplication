using System;
using System.Xml.Serialization;

namespace Servion.RISL.Utilities.DataImport
{
    [XmlRoot("AdditionalCallInfo")]
    public class AdditionalCallInfo
    {
       

        [XmlElement("ApplicationServer")]
        public string ApplicationServer { get; set; }

        [XmlElement("ApplicationName")]
        public string ApplicationName { get; set; }

        [XmlElement("ApplicationThreadName")]
        public string ApplicationThreadName { get; set; }



    }
}
