namespace EcTools.Models
{
    public class CSV_ERROR
    {
        public string   ErrorType { get; set; }
        public string   ErrorMessage { get; set; }
        public int      ErrorLine { get; set; }
        public int      MissingColumnsCount { get; set; }
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
        
        public CSV_ERROR(string errorType, string errorMessage, int errorLine, string[] errorLineContent, string[] errorColumns, int missingColumnsCount)
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage;
            ErrorLine = errorLine;
            ErrorLineContent = errorLineContent;
            ErrorColumns = errorColumns;
            MissingColumnsCount = missingColumnsCount;
        }


    }
}
