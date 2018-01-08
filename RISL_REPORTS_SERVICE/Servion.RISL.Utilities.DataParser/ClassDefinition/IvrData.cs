using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Linq;

namespace Servion.RISL.Utilities.DataImport
{


    [XmlRoot("IVRREPORTDATA")]
    public class IvrData
    {
        [XmlIgnore()]
        private IFormatProvider fp = new System.Globalization.CultureInfo("en-US");

        [XmlIgnore()]
        private CallInfo _callInfo = new CallInfo();

        [XmlIgnore()]
        private List<ANNOUNCEMENT> _announceList = new List<ANNOUNCEMENT>();
        
        [XmlIgnore()]
        private List<MENU> _menuList = new List<MENU>();

        [XmlIgnore()]
        private List<HOST> _hostList = new List<HOST>();





        #region Private Members for Report Data

        [XmlIgnore()]
        private Dictionary<string, SummMenu> _menuSummary = new Dictionary<string, SummMenu>();

        [XmlIgnore()]
        private Dictionary<string, SummMenuOption> _menuOptionSummary = new Dictionary<string, SummMenuOption>();

        [XmlIgnore()]
        private Dictionary<string, SummHost> _hostSummary = new Dictionary<string, SummHost>();

        

        [XmlIgnore()]
        private Dictionary<string, SummAnnounce> _announceSummary = new Dictionary<string, SummAnnounce>();


        #endregion

        #region Public Properties for Summary Data

        [XmlIgnore()]
        public Dictionary<string, SummMenu> MenuSummary
        {
            get
            {
                return _menuSummary;
            }
        }

        [XmlIgnore()]
        public Dictionary<string, SummMenuOption> MenuOptionSummary
        {
            get
            {
                return _menuOptionSummary;
            }
        }
        [XmlIgnore()]
        public Dictionary<string, SummHost> HostSummary
        {
            get
            {
                return _hostSummary;
            }
        }

    
        [XmlIgnore()]
        public Dictionary<string, SummAnnounce> AnnounceSummary
        {
            get
            {
                return _announceSummary;
            }
        }

      
        #endregion

        [XmlElement("CALLINFO")]
        public CallInfo CallInformation
        {
            get 
            { 
                return this._callInfo; 
            }
            set 
            { 
                _callInfo = value;
            }
        }

        [XmlArray("MENU_DETAIL"), XmlArrayItem("MENU", typeof(MENU))]
        public MENU[] MenuList
        {
            get { return this._menuList.ToArray(); }
            set
            {
                this._menuList.Clear();
                MENU[] data = (MENU[])value;
                int menuCount = 0;
                foreach (MENU eachMenuData in data)
                {
                    menuCount++;
                    SummMenu summNodal = null;
                    SummMenuOption summNodalOption = null;

                    string menuId = eachMenuData.MENU_ID;
                    string menuWithOption = string.Format("{0}--{1}", eachMenuData.MENU_ID, eachMenuData.MENU_OPTION);
                    DateTime menuStartDate = ConvertDateTime(eachMenuData.MENU_STARTTIME);
                    DateTime menuEndDate = ConvertDateTime(eachMenuData.MENU_ENDTIME);

                    int menuduration = (int)menuEndDate.Subtract(menuStartDate).TotalSeconds; 

                    if (_menuSummary.TryGetValue(menuId, out summNodal))
                    {
                        if (summNodal.MinTimeSpent > menuduration)
                        {
                            summNodal.MinTimeSpent = menuduration;
                        }
                        else
                        {
                            summNodal.MaxTimeSpent = menuduration;
                        }
                        summNodal.MenuDuration = menuduration;
                        summNodal.TotalCount++;
                    }
                    else
                    {
                        summNodal = new SummMenu { MenuID = menuId, TotalCount = 1, MenuDuration = menuduration, MinTimeSpent = menuduration, MaxTimeSpent = menuduration };
                        _menuSummary.Add(menuId, summNodal);
                    }


                    if (_menuOptionSummary.TryGetValue(menuWithOption, out summNodalOption))
                    {
                        summNodalOption.TotalCount++;
                    }
                    else
                    {
                        summNodalOption = new SummMenuOption { MenuID = menuId, MenuOption = eachMenuData.MENU_OPTION, MenuDesc = eachMenuData.MENU_DESC, MenuOptionDesc = eachMenuData.MENU_OPTION_DESC, TotalCount = 1};
                        _menuOptionSummary.Add(menuWithOption, summNodalOption);
                    }

                    if (eachMenuData.MENU_OPTION_DESC.ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
                    {
                        summNodalOption.SuccessResponseCount++;
                        
                    }
                    else
                    {
                        summNodalOption.FailureResponseCount++;
                    }

                    _menuList.Add(eachMenuData);
                }

                if (_menuSummary.Count > 0)
                {
                    _menuSummary.Values.Last().AgentTransferCount = (_callInfo.DISPOSITION_CODE.Equals("XA")) ? 1 : 0;
                }

                if (_menuOptionSummary.Count > 0)
                {
                    _menuOptionSummary.Values.Last().AgentTransfered = (_callInfo.DISPOSITION_CODE.Equals("XA")) ? 1 : 0;
                    _menuOptionSummary.Values.Last().HangupinIVR = (_callInfo.DISPOSITION_CODE.Equals("ID")) ? 1 : 0;
                    _menuOptionSummary.Values.Last().HangupinCall= (_callInfo.DISPOSITION_CODE.Equals("CD")) ? 1 : 0;
                }
            }

        }

        [XmlArray("HOST_DETAIL"), XmlArrayItem("HOST", typeof(HOST))]
        public HOST[] HostList
        {
            get { return this._hostList.ToArray(); }
            set
            {
                this._hostList.Clear();
                HOST[] data = (HOST[])value;

                foreach (HOST eachHostData in data)
                {

                     string hostURL = eachHostData.HOST_URL;
                    // string hostMethod = eachHostData.HOST_METHOD;

                    // string hosttypeMethod = string.Format("{0}--{1}", eachHostData.HOST_URL, eachHostData.HOST_METHOD);
                    SummHost summHost = null;


                    if (_hostSummary.TryGetValue(hostURL, out summHost))
                    {
                        summHost.TotalCount++;
                    }
                    else
                    {
                        summHost = new SummHost { hostURL = eachHostData.HOST_URL,TotalCount = 1 };
                        _hostSummary.Add(hostURL, summHost);
                    }

                    //if (_hostSummary.TryGetValue(hosttypeMethod, out summHost))
                    //{
                    //    summHost.TotalCount++;
                    //}
                    //else
                    //{
                    //    summHost = new SummHost { HostType = eachHostData.HOST_TYPE, HostMethod = eachHostData.HOST_METHOD, TotalCount = 1 };
                    //    _hostSummary.Add(hosttypeMethod, summHost);
                    //}


                    //if (eachHostData.HOST_ACTUAL_RESPONSE.Equals("SUCCESS", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    summHost.SuccessCount++;
                    //}
                    //else
                    //{
                    //    summHost.FailureCount++;
                    //}


                    if (eachHostData.HOST_RESPONSE.Equals("SUCCESS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        summHost.SuccessCount++;
                    }
                    else
                    {
                        summHost.FailureCount++;
                    }

                    _hostList.Add(eachHostData);
                }
            }
        }


     

        [XmlArray("ANNOUNCEMENT_DETAIL"), XmlArrayItem("ANNOUNCEMENT", typeof(ANNOUNCEMENT))]
        public ANNOUNCEMENT[] AnnounceList
        {
            get { return this._announceList.ToArray(); }
            set
            {
                this._announceList.Clear();
                ANNOUNCEMENT[] data = (ANNOUNCEMENT[])value;

                foreach (ANNOUNCEMENT eachHostData in data)
                {

                    string announceId = eachHostData.ANNOUNCEMENT_ID;

                  
                    SummAnnounce summAnnounce = null;

                    if (_announceSummary.TryGetValue(announceId, out summAnnounce))
                    {
                        summAnnounce.TotalCount++;
                    }
                    else
                    {
                        summAnnounce = new SummAnnounce { AnnounceID = eachHostData.ANNOUNCEMENT_ID,  TotalCount = 1 };
                        _announceSummary.Add(announceId, summAnnounce);
                    }

                    _announceList.Add(eachHostData);
                }
            }
        }

        public DateTime ConvertDateTime(string date)
        {
            DateTime convertDate = DateTime.Now;

            convertDate = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:ss", fp);

            return convertDate;
        }

       
    }

}
