using System;
using System.Xml;
using System.Xml.Serialization;

namespace Servion.RISL.Utilities.DataImport
{
    [XmlRoot("CALLINFO")]
    public class CallInfo
    {
        #region Private Members

        [XmlIgnore()]
        private IFormatProvider fp = new System.Globalization.CultureInfo("en-US");

        [XmlIgnore()]
        private string callstartDateTime = string.Empty;

        [XmlIgnore()]
        private string callendDateTime = string.Empty;

        [XmlIgnore()]
        private string menuPath = string.Empty;

        #endregion

        #region Constructors

        public CallInfo() { }

        #endregion Constructors

        #region Properties

        [XmlElement("APP_ID")]
        public string APP_ID { get; set; }

        [XmlElement("APP_NAME")]
        public string APP_NAME { get; set; }

        [XmlElement("CALLID")]
        public string CALLID { get; set; }
        [XmlElement("UNIQUE_ID")]
        public string UNIQUE_ID { get; set; }

        [XmlElement("SESSION_ID")]
        public string SESSION_ID { get; set; }

        [XmlElement("SERVER_IP")]
        public string SERVER_IP { get; set; }

        [XmlElement("ANI")]
        public string ANI { get; set; }

        [XmlElement("DNIS")]
        public string DNIS { get; set; }

        [XmlElement("CALL_START_TIME")]
        public string CALL_START_TIME
        {
            get { return callstartDateTime; }
            set { callstartDateTime = value; }
        }

        [XmlElement("CALL_END_TIME")]
        public string CALL_END_TIME
        {
            get { return callendDateTime; }
            set { callendDateTime = value; }
        }

        [XmlElement("SURVEY_ID")]
        public string SURVEY_ID { get; set; }

        [XmlElement("SAMPLE_ID")]
        public string SAMPLE_ID { get; set; }

        [XmlElement("SAMPLE_NAME")]
        public string SAMPLE_NAME { get; set; }

        [XmlElement("SURVEY_MODE")]
        public string SURVEY_MODE { get; set; }

        [XmlElement("QUESTION_TYPE_ID")]
        public string QUESTION_TYPE_ID { get; set; }

        [XmlElement("QUESTION_TYPE")]
        public string QUESTION_TYPE { get; set; }

        [XmlElement("QUESTION_SET_NAME")]
        public string QUESTION_SET_NAME { get; set; }

        [XmlElement("SURVEY_STATUS")]
        public string SURVEY_STATUS { get; set; }

        [XmlElement("CAMPAIGN_NAME")]
        public string CAMPAIGN_NAME { get; set; }

        [XmlElement("DEPT_ID")]
        public string DEPT_ID { get; set; }

        [XmlElement("DEPT_NAME")]
        public string DEPT_NAME { get; set; }

        [XmlElement("LANG_ID")]
        public string LANG_ID { get; set; }

        [XmlElement("LANG_CODE")]
        public string LANG_CODE { get; set; }

        [XmlElement("INPUT_MODE")]
        public string INPUT_MODE { get; set; }

        [XmlElement("PHRASE_TYPE")]
        public string PHRASE_TYPE { get; set; }

        [XmlElement("DISPOSITION_CODE")]
        public string DISPOSITION_CODE { get; set; }


        [XmlElement("CALL_END_TYPE")]
        public string CALL_END_TYPE { get; set; }








        //[XmlElement("LANG_CODE")]
        //public string LANG_CODE { get; set; }

        //[XmlElement("CLI_REGISTERED_FLAG")]
        //public string CLI_REGISTERED_FLAG { get; set; }

        //[XmlElement("CSQ")]
        //public string CSQ { get; set; }


        //[XmlElement("ISCALLQUEUED")]
        //public string ISCALLQUEUED { get; set; }

        //[XmlElement("MENUPATH")]
        //public string MENUPATH { get; set; }

        //[XmlElement("LASTMENU_ACCESSED")]
        //public string LASTMENU_ACCESSED { get; set; }

        //[XmlElement("DISPOSITION")]
        //public string DISPOSITION { get; set; }

        //[XmlElement("END_TYPE")]
        //public string END_TYPE { get; set; }


        //[XmlElement("XFER")]
        //public string XFER { get; set; }

        //[XmlElement("ABANDONED_FLAG")]
        //public string ABANDONED_FLAG { get; set; }

        //[XmlElement("APP_NAME")]
        //public string APP_NAME { get; set; }

        //[XmlElement("UNIQUE_ID")]
        //public string UNIQUE_ID { get; set; }

        //[XmlElement("TRANSFER_REASON")]
        //public string TRANSFER_REASON { get; set; }


        //[XmlElement("TRANSFER_AGENT_ID")]
        //public string TRANSFER_AGENT_ID { get; set; }

        //[XmlElement("TRANSFER_QUEUE_WAITTIME")]
        //public string TRANSFER_QUEUE_WAITTIME { get; set; }


        #endregion

        #region Resever Fields

        [XmlElement("RESERVE1")]
        public string RESERVE1 { get; set; }

        [XmlElement("RESERVE2")]
        public string RESERVE2 { get; set; }

        [XmlElement("RESERVE3")]
        public string RESERVE3 { get; set; }

        [XmlElement("RESERVE4")]
        public string RESERVE4 { get; set; }

        [XmlElement("RESERVE5")]
        public string RESERVE5 { get; set; }

        [XmlElement("RESERVE6")]
        public string RESERVE6 { get; set; }

        [XmlElement("RESERVE7")]
        public string RESERVE7 { get; set; }

        [XmlElement("RESERVE8")]
        public string RESERVE8 { get; set; }

        [XmlElement("RESERVE9")]
        public string RESERVE9 { get; set; }

        [XmlElement("RESERVE10")]
        public string RESERVE10 { get; set; }


        #endregion


        public AdditionalCallInfo GetAdditionalCallInfo()
        {
            AdditionalCallInfo additionalInfo = new AdditionalCallInfo()
            {
                ApplicationName = string.Empty,
                ApplicationServer = string.Empty,
                ApplicationThreadName = string.Empty,
            };

            return additionalInfo;
        }

       
    }


    [XmlRoot("ANNOUNCEMENT")]
    public class ANNOUNCEMENT
    {

        [XmlElement("ANNOUNCEMENT_ID")]
        public string ANNOUNCEMENT_ID { get; set; }

        [XmlElement("ANNOUNCEMENT_TYPE")]
        public string ANNOUNCEMENT_TYPE { get; set; }

        [XmlElement("ANNOUNCEMENT_DESC")]
        public string ANNOUNCEMENT_DESC { get; set; }

        [XmlElement("ANNOUNCEMENT_CONTENT")]
        public string ANNOUNCEMENT_CONTENT { get; set; }

        [XmlElement("ANNOUNCEMENT_DISPOSITION")]
        public string ANNOUNCEMENT_DISPOSITION { get; set; }



        [XmlElement("ANNOUNCEMENT_TRACKID")]
        public string ANNOUNCEMENT_TRACKID { get; set; }

        [XmlElement("ANNOUNCEMENT_RESERVE1")]
        public string ANNOUNCEMENT_RESERVE1 { get; set; }

        [XmlElement("ANNOUNCEMENT_RESERVE2")]
        public string ANNOUNCEMENT_RESERVE2 { get; set; }

        [XmlElement("ANNOUNCEMENT_RESERVE3")]
        public string ANNOUNCEMENT_RESERVE3 { get; set; }

        [XmlElement("ANNOUNCEMENT_RESERVE4")]
        public string ANNOUNCEMENT_RESERVE4 { get; set; }







        //[XmlIgnore()]
        //private string ANNOUNCEMENTID = string.Empty;
        //[XmlIgnore()]
        //private string ANNOUNCEMENTTRACKID = string.Empty;
        //[XmlIgnore()]
        //private string ANNOUNCEMENTRESERVE1 = string.Empty;
        //[XmlIgnore()]
        //private string ANNOUNCEMENTRESERVE2 = string.Empty;
        //[XmlIgnore()]
        //private string ANNOUNCEMENTRESERVE3 = string.Empty;
        //[XmlIgnore()]
        //private string ANNOUNCEMENTRESERVE4 = string.Empty;

        //[XmlElement("ANNOUNCEMENT_ID")]
        //public string ANNOUNCEMENT_ID
        //{
        //    get { return ANNOUNCEMENTID; }
        //    set { ANNOUNCEMENTID = value; }
        //}

        //[XmlElement("ANNOUNCEMENT_TYPE")]
        //public string ANNOUNCEMENT_TYPE
        //{
        //    get { return ANNOUNCEMENT_TYPE; }
        //    set { ANNOUNCEMENT_TYPE = value; }
        //}

        //[XmlElement("ANNOUNCEMENT_RESERVE1")]
        //public string ANNOUNCEMENT_RESERVE1
        //{
        //    get { return ANNOUNCEMENTRESERVE1; }
        //    set { ANNOUNCEMENTRESERVE1 = value; }
        //}

        //[XmlElement("ANNOUNCEMENT_RESERVE2")]
        //public string ANNOUNCEMENT_RESERVE2
        //{
        //    get { return ANNOUNCEMENTRESERVE2; }
        //    set { ANNOUNCEMENTRESERVE2 = value; }
        //}

        //[XmlElement("ANNOUNCEMENT_RESERVE3")]
        //public string ANNOUNCEMENT_RESERVE3
        //{
        //    get { return ANNOUNCEMENTRESERVE3; }
        //    set { ANNOUNCEMENTRESERVE3 = value; }
        //}

        //[XmlElement("ANNOUNCEMENT_RESERVE4")]
        //public string ANNOUNCEMENT_RESERVE4
        //{
        //    get { return ANNOUNCEMENTRESERVE4; }
        //    set { ANNOUNCEMENTRESERVE4 = value; }
        //}
    }

    [XmlRoot("MENU")]
    public class MENU
    {
        [XmlElement("MENU_ID")]
        public string MENU_ID { get; set; }

        [XmlElement("MENU_TYPE")]
        public string MENU_TYPE { get; set; }

        [XmlElement("MENU_DESC")]
        public string MENU_DESC { get; set; }


        [XmlElement("MENU_CONTENT")]
        public string MENU_CONTENT { get; set; }


        [XmlElement("MENU_STARTTIME")]
        public string MENU_STARTTIME { get; set; }


        [XmlElement("MENU_ENDTIME")]
        public string MENU_ENDTIME { get; set; }


        [XmlElement("MENU_OPTION")]
        public string MENU_OPTION { get; set; }


        [XmlElement("MENU_OPTION_DESC")]
        public string MENU_OPTION_DESC { get; set; }


        [XmlElement("MENU_DISPOSITION")]
        public string MENU_DISPOSITION { get; set; }


        [XmlElement("MENU_TRACKID")]
        public string MENU_TRACKID { get; set; }


        [XmlElement("MENU_RESERVE1")]
        public string MENU_RESERVE1 { get; set; }


        [XmlElement("MENU_RESERVE2")]
        public string MENU_RESERVE2 { get; set; }

        [XmlElement("MENU_RESERVE3")]
        public string MENU_RESERVE3 { get; set; }

        [XmlElement("MENU_RESERVE4")]
        public string MENU_RESERVE4 { get; set; }


       




        //[XmlIgnore()]
        //private string MENUID = string.Empty;
        //[XmlIgnore()]
        //private string MENUDESC = string.Empty;
        //[XmlIgnore()]
        //private string MENUSTARTTIME = string.Empty;
        //[XmlIgnore()]
        //private string MENUENDTIME = string.Empty;
        //[XmlIgnore()]
        //private string MENUOPTION = string.Empty;
        //[XmlIgnore()]
        //private string MENUOPTIONDESC = string.Empty;
        //[XmlIgnore()]
        //private string MENUTRACKID = string.Empty;
        //[XmlIgnore()]
        //private string MENURESERVE1 = string.Empty;
        //[XmlIgnore()]
        //private string MENURESERVE2 = string.Empty;
        //[XmlIgnore()]
        //private string MENURESERVE3 = string.Empty;
        //[XmlIgnore()]
        //private string MENURESERVE4 = string.Empty;

        //[XmlElement("MENU_ID")]
        //public string MENU_ID
        //{
        //    get { return MENUID; }
        //    set { MENUID = value; }
        //}

        //[XmlElement("MENU_DESC")]
        //public string MENU_DESC
        //{
        //    get { return MENUDESC; }
        //    set { MENUDESC = value; }
        //}

        //[XmlElement("MENU_STARTTIME")]
        //public string MENU_STARTTIME
        //{
        //    get { return MENUSTARTTIME; }
        //    set { MENUSTARTTIME = value; }
        //}

        //[XmlElement("MENU_ENDTIME")]
        //public string MENU_ENDTIME
        //{
        //    get { return MENUENDTIME; }
        //    set { MENUENDTIME = value; }
        //}

        //[XmlElement("MENU_OPTION")]
        //public string MENU_OPTION
        //{
        //    get { return MENUOPTION; }
        //    set { MENUOPTION = value; }
        //}

        //[XmlElement("MENU_OPTION_DESC")]
        //public string MENU_OPTION_DESC
        //{
        //    get { return MENUOPTIONDESC; }
        //    set { MENUOPTIONDESC = value; }
        //}


        //[XmlElement("MENU_TRACKID")]
        //public string MENU_TRACKID
        //{
        //    get { return MENUTRACKID; }
        //    set { MENUTRACKID = value; }
        //}

        //[XmlElement("MENU_RESERVE1")]
        //public string MENU_RESERVE1
        //{
        //    get { return MENURESERVE1; }
        //    set { MENURESERVE1 = value; }
        //}

        //[XmlElement("MENU_RESERVE2")]
        //public string MENU_RESERVE2
        //{
        //    get { return MENURESERVE2; }
        //    set { MENURESERVE2 = value; }
        //}

        //[XmlElement("MENU_RESERVE3")]
        //public string MENU_RESERVE3
        //{
        //    get { return MENURESERVE3; }
        //    set { MENURESERVE3 = value; }
        //}

        //[XmlElement("MENU_RESERVE4")]
        //public string MENU_RESERVE4
        //{
        //    get { return MENURESERVE4; }
        //    set { MENURESERVE4 = value; }
        //}

    }

    [XmlRoot("HOST")]
    public class HOST
    {
        [XmlElement("HOST_URL")]
        public string HOST_URL { get; set; }

        [XmlElement("HOST_STARTTIME")]
        public string HOST_STARTTIME { get; set; }

        [XmlElement("HOST_ENDTIME")]
        public string HOST_ENDTIME { get; set; }

        [XmlElement("HOST_IN_PARAMS")]
        public string HOST_IN_PARAMS { get; set; }

        [XmlElement("HOST_OUT_PARAMS")]
        public string HOST_OUT_PARAMS { get; set; }

        [XmlElement("HOST_REFERENCE_ID")]
        public string HOST_REFERENCE_ID { get; set; }

        [XmlElement("HOST_RESPONSE")]
        public string HOST_RESPONSE { get; set; }

        [XmlElement("HOST_TRACK_ID")]
        public string HOST_TRACK_ID { get; set; }

        [XmlElement("HOST_RESERVE1")]
        public string HOST_RESERVE1 { get; set; }

       

        [XmlElement("HOST_RESERVE2")]
        public string HOST_RESERVE2 { get; set; }

        [XmlElement("HOST_RESERVE3")]
        public string HOST_RESERVE3 { get; set; }

        [XmlElement("HOST_RESERVE4")]
        public string HOST_RESERVE4 { get; set; }

       

    }


   

}
