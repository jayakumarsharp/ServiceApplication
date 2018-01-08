
namespace Servion.RISL.Utilities.DataImport
{
    public enum ImportationFailureMode
    {
        None = 0,           //There is no failure
        BadXml,             //If the xml data is empty or not well formed
        InvalidData,        //If the xml data is mismatched with call id/app id 
        InvalidXml,         //Invalid xml against XSD
        InvalidConfig,      //Invalid or missing configuration settings
        XsdFailed,          //Xsd validation failure
        DatabaseFailed,     //Database connection failure
        DuplicateXml,       //Call ID alreday exist in database
        ParserFailed,       //Parser failure
        ImportFailed,       //Database import failure
        ApplicationFailed   //Other exceptions
    }

    public class ImportationResponse
    {
        public ImportationResponse()
        {
            HasImported = false;
            FailureMode = ImportationFailureMode.None;
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }

        public bool HasImported { get; set; }

        public ImportationFailureMode FailureMode { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
