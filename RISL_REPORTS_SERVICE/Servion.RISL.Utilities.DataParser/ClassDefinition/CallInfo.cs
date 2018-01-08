using System;
using System.Xml.Serialization;

namespace Servion.Sso.Utilities.DataImport
{
    [XmlRoot("CALLINFO")]
    public class CallInfo  
    {
        #region Private Members
        [XmlIgnore()]
        private IFormatProvider fp = new System.Globalization.CultureInfo("en-US");
        
        [XmlIgnore()]
        private string startDateTime = string.Empty;
        
        [XmlIgnore()]
        private string endDateTime = string.Empty;

        [XmlIgnore()]
        private string menuPath = string.Empty;
        #endregion 

        #region Constructors

        public CallInfo() { }

        #endregion Constructors

        #region Properties

        [XmlElement("CALLID")]
        public string CallID { get; set; }

        [XmlElement("SESSIONID")]
        public string SessionID { get; set; }

        [XmlElement("DNIS")]
        public string Dnis { get; set; }

        [XmlElement("GATEWAY_IP")]
        public string GatewayIP { get; set; }

        [XmlElement("SITEID")]
        public string SiteID { get; set; }

        [XmlElement("VXML_IP")]
        public string VxmlIP { get; set; }

        [XmlElement("APP_ID")]
        public string AppID { get; set; }

        [XmlElement("CLI")]
        public string Cli { get; set; }

        [XmlElement("START_DATE")]
        public string StartDateTime
        {
            get { return startDateTime; }
            set { startDateTime = value; }
        }

        [XmlElement("END_DATE")]
        public string EndDateTime
        {
            get { return endDateTime; }
            set { endDateTime = value; }
        }

        [XmlElement("ABANDONED")]
        public string AbandonedFlag { get; set; }

        [XmlElement("LANG_ID")]
        public string LanguageID { get; set; }

        [XmlElement("AGENT_ID")]
        public string AgentID { get; set; }

        [XmlElement("TPIN_VALID")]
        public string TpinValid { get; set; }

        [XmlElement("MENUPATH")]
        public string MenuPath
        {
            get { return menuPath; }
            set
            {
                if (value.Length > 3999)
                    menuPath = value.Substring(0, 3999);
                else
                    menuPath = value;
            }
        }

        [XmlElement("EMP_ID")]
        public string EmployeeID { get; set; }

        [XmlElement("LASTMENU")]
        public string LastMenu { get; set; }

        [XmlElement("TRANSFERED")]
        public string TransferredFlag { get; set; }

        [XmlElement("TRANSCODE")]
        public string TransCode { get; set; }

        [XmlElement("ENDTYPE")]
        public string EndType { get; set; }

        [XmlElement("ICM_LABEL")]
        public string IcmLabel { get; set; }

        [XmlElement("HOW_CALL_ENDED")]
        public string HowCallEnded { get; set; }

        [XmlElement("CALL_END_REASON")]
        public string CallEndReason { get; set; }

        [XmlElement("ERR_REASON")]
        public string ErrorReason { get; set; }
 
        #endregion

        #region Resever Fields
        [XmlElement("RESERVE1")]
        public string Reserve1 { get; set; }

        [XmlElement("RESERVE2")]
        public string Reserve2 { get; set; }

        [XmlElement("RESERVE3")]
        public string Reserve3 { get; set; }

        [XmlElement("RESERVE4")]
        public string Reserve4 { get; set; }

        [XmlElement("RESERVE5")]
        public string Reserve5 { get; set; }
        #endregion

        #region Additional Call Info
        [XmlIgnore()]
        public DateTime StartDate
        {
            get { return ConvertDateTime(this.startDateTime); }
        }

        [XmlIgnore()]
        public DateTime EndDate
        {
            get { return ConvertDateTime(this.endDateTime); }
        }

        [XmlIgnore()]
        public int Duration
        {
            get { return (int)this.EndDate.Subtract(StartDate).TotalSeconds; }
        }

        [XmlIgnore()]
        public string SummaryDate
        {
            get { return this.StartDate.ToString("dd/MM/yyyy"); }
        }

        [XmlIgnore()]
        public int SummHour
        {
            get { return this.StartDate.Hour; }
        }

        [XmlIgnore()]
        public int HourPart
        {
            get { return GetHourPart(this.StartDate); }
        }

        [XmlIgnore()]
        public int TimeInterval
        {
            get { return (SummHour * 60) + (HourPart * 15); }

        }
        #endregion

        public AdditionalCallInfo GetAdditionalCallInfo()
        {
            AdditionalCallInfo additionalInfo = new AdditionalCallInfo()
            {
                ApplicationName = string.Empty,
                ApplicationServer = string.Empty,
                ApplicationThreadName = string.Empty,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                Duration = this.Duration,
                SummaryDate = this.SummaryDate,
                SummHour = this.SummHour,
                HourPart = this.HourPart,
                TimeInterval = this.TimeInterval
            };

            return additionalInfo;
        }

        private int GetHourPart(DateTime time)
        {
            int currentMin = time.Minute;

            if (currentMin < 15) return 1;
            else if (currentMin >= 15 && currentMin < 30) return 2;
            else if (currentMin >= 30 && currentMin < 45) return 3;
            else return 4;
        }

        public DateTime ConvertDateTime(string date)
        {
            DateTime convertDate = DateTime.Now;

            convertDate = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:ss", fp);

            return convertDate;
        }
    }

}
