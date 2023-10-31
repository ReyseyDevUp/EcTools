using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EcTools.Models
{
    public class CSV_FileConfig
    {
        public string FileName { get; private set; }
        public int ColumnsCount { get; private set; }
        public int RowId { get; private set; }
        public string[] Line { get; private set; }

        public CSV_FileConfig(string fileName, int columnsCount, int rowId, string[] line)
        {
            FileName = fileName;
            ColumnsCount = columnsCount;
            RowId = rowId;
            Line = line;
        }
    }
}