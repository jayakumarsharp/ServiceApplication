
namespace Servion.RISL.Utilities.DataImport
{  
    #region Menu Summary
    public class SummMenu
    {
        public string MenuID { get; set; }
        public int TotalCount { get; set; }
        public int UniqueCount
        {
            get { return 1; }
        }       
        public int AgentTransferCount { get; set; }
        public int MenuDuration { get; set; }
        public int MinTimeSpent { get; set; }
        public int MaxTimeSpent { get; set; }
       

    }

    #endregion

    #region Menu Option Summary
    public class SummMenuOption
    {
        public string MenuID { get; set; }
        public string MenuDesc { get; set; }
        public string MenuOption { get; set; }

        public string MenuOptionDesc { get; set; }
        public int TotalCount { get; set; }
        public int UniqueCount
        {
            get { return 1; }
        }       
        public int AgentTransfered { get; set; }
        public int HangupinIVR { get; set; }
        public int HangupinCall { get; set; }
        public int SuccessResponseCount { get; set; }
        public int FailureResponseCount { get; set; }

    }
    #endregion

    #region Host Summary
    public class SummHost
    {
        
        public string hostURL { get; set; }
        public string HostMethod { get; set; }
        public int TotalCount { get; set; }

        public int UniqueCount
        {
            get { return 1; }
        }
       
        public int SuccessCount { get; set; }
       
        public int FailureCount { get; set; }
    }
    #endregion


   


    #region Announce Summary
    public class SummAnnounce
    {
        public string AnnounceID { get; set; }
        public int TotalCount { get; set; }
        public int UniqueCount
        {
            get { return 1; }
        }
    }

    #endregion
}
