using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Servion.RISL.Utilities.DataImport
{
    [XmlRoot("IVRREPORTDATA")]
    public class IvrReportData
    {
        private IvrData _callData = null;

        private AdditionalCallInfo _additonalCallInfo = null;

        public IvrReportData() { }

        public IvrReportData(IvrData callData)
        {
            _callData = callData;
            _additonalCallInfo = _callData == null ? new AdditionalCallInfo() : callData.CallInformation.GetAdditionalCallInfo();
          
        }

        public IvrData IVRDATA 
        {
            get
            {
                return _callData;
            }
            set
            {
                _callData = value;
            }
        }

        public AdditionalCallInfo AdditionalCallInformation
        {
            get
            {
                return _additonalCallInfo;
            }
            set
            {
                _additonalCallInfo = value;
            }
        }

        [XmlArray("MenuSummary"), XmlArrayItem("Menu", typeof(SummMenu))]
        public List<SummMenu> MenuSummary
        {
            get 
            {
                return (_callData == null ? new List<SummMenu>() : _callData.MenuSummary.Values.OfType<SummMenu>().ToList());
            }
        }

        [XmlArray("MenuOptionSummary"), XmlArrayItem("MenuOption", typeof(SummMenuOption))]
        public List<SummMenuOption> MenuOptionSummary
        {
            get
            {
                return (_callData == null ? new List<SummMenuOption>() : _callData.MenuOptionSummary.Values.OfType<SummMenuOption>().ToList());
            }
        }

        [XmlArray("HostSummary"), XmlArrayItem("Host", typeof(SummHost))]
        public List<SummHost> HostSummary
        {
            get
            {
                return (_callData == null ? new List<SummHost>() : _callData.HostSummary.Values.OfType<SummHost>().ToList());
            }
        }

       

        [XmlArray("AnnounceSummary"), XmlArrayItem("Announce", typeof(SummAnnounce))]
        public List<SummAnnounce> AnnounceSummary
        {
            get
            {
                return (_callData == null ? new List<SummAnnounce>() : _callData.AnnounceSummary.Values.OfType<SummAnnounce>().ToList());
            }
        }

       
    }
}
