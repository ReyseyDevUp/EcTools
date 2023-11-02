namespace EcTools.Models
{
    public class CSV_ERROR
    {
        public string   ErrorType { get; set; }
        public string   ErrorMessage { get; set; }
        public int      ErrorLine { get; set; }
        public string[] ErrorLineContent { get; set; }
        public string[] ErrorColumns { get; set; }


        public CSV_ERROR(string errorType, string errorMessage, int errorLine, string[] errorLineContent, string[] errorColumns)
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage;
            ErrorLine = errorLine;
            ErrorLineContent = errorLineContent;
            ErrorColumns = errorColumns;
        }


    }
}
