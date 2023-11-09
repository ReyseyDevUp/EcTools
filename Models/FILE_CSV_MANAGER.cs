using EcTools.Pages;
using System.Text;
using System.Text.RegularExpressions;

namespace EcTools.Models
{
    public class FILE_CSV_MANAGER
    {
        public static string      csvToSQL(string fileName, string originalFileName)
        {
            
            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath)){}

            // Open the file stream
            using Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream);

            // Lists to store the column names and rows
            string[]    columnNames = null;
            var         rows = new List<string[]>();
            int         columnsCount = 0;
            int         lineNumber = 0;


            // SQL SCRIPT
            StringBuilder sql = new StringBuilder();
            string tableName = SqlGenerator.SanitizeName(originalFileName);
            sql.AppendLine($"CREATE TABLE Table_{tableName} (");
            string insertInto = $"INSERT INTO Table_{tableName} (";
            StringBuilder insertIntoColumns = new StringBuilder();


            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string pattern = @"&\w+;";
                string cleaned = Regex.Replace(line, pattern, "");
                // FIND SEMICOLONES WITHIN HTML TAGS AND CONVERT THEM TO HTML CHAR CODE
                string output = Regex.Replace(cleaned, @"(?<=<[^>| ^/>]+>)([^</]+)([^</| <\w+/>]+)(?=<)", m => m.Value.Replace(";", ""));
                //var values = cleaned.Split(';');
                var values = Regex.Split(output, @"(?<!&semi)(?<!&nbsp);");

                if (lineNumber == 0)
                {
                    // First line contains column names
                    columnNames = Tools.UpdateDuplicates(Tools.ReplaceSpecialCharacters(values).ToArray());
                    columnsCount = values.Length;

                    foreach (var column in columnNames)
                    {
                        string columnName = SqlGenerator.SanitizeName(column);
                        sql.AppendLine($"    [{columnName}] VARCHAR(MAX) NULL,");
                        insertIntoColumns.Append($"[{columnName}],");
                    }
                    sql.Length -= 1;
                    sql.AppendLine(")");
                    sql.AppendLine("GO");
                    sql.AppendLine("");

                    insertIntoColumns.Length -= 1;
                    insertIntoColumns.Append(") VALUES (");

                }
                else
                {
                    if (values.Length > columnsCount)
                    {
                        rows.Add(values);
                    }
                    else if (values.Length < columnsCount)
                    {
                        int dif = columnsCount - values.Length;

                        var valuesList = values.ToList();
                        for (int i = 0; i < dif; i++)
                        {
                            valuesList.Add("NULL");
                        }
                        values = valuesList.ToArray();
                        rows.Add(values);
                    }
                    else
                    {
                        rows.Add(values);
                    }

                    sql.Append(insertInto);

                    sql.Append(insertIntoColumns);

                    foreach (var value in values)
                    {
                        sql.Append($"'{SqlGenerator.SanitizeValue(value)}',");
                    }

                    // Remove the last comma and close the values part
                    sql.Length -= 1;
                    sql.AppendLine(");");
                }

                lineNumber++;
            }

            return SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(originalFileName));
        }

        public static string    csvToSQLWithDefaultValus(string fileName, string originalFileName, string defaultFieldValue)
        {
            // SQL SCRIPT
            StringBuilder sql = new StringBuilder();


            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath)) { }

            // Open the file stream
            using Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream);

            // Lists to store the column names and rows
            string[] columnNames = null;
            var rows = new List<string[]>();
            int rowsId = 0;
            int columnsCount = 0;
            int lineNumber = 0;


            // Create table
            sql.AppendLine($"CREATE TABLE Table_{SqlGenerator.SanitizeName(originalFileName)} (");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string pattern = @"&\w+;";
                string cleaned = Regex.Replace(line, pattern, "");
                var values = cleaned.Split(';');

                if (lineNumber == 0)
                {
                    // First line contains column names
                    columnNames = Tools.UpdateDuplicates(Tools.ReplaceSpecialCharacters(values).ToArray());
                    columnsCount = values.Length;

                    foreach (var column in columnNames)
                    {
                        sql.AppendLine($"    [{SqlGenerator.SanitizeName(column)}] VARCHAR(MAX) NULL,");
                    }
                    sql.Length -= 1;
                    sql.AppendLine(")");
                    sql.AppendLine("GO");
                    sql.AppendLine("");

                }
                else
                {
                    if (values.Length > columnsCount)
                    {
                        rows.Add(values);
                    }
                    else if (values.Length < columnsCount)
                    {
                        int dif = columnsCount - values.Length;

                        var valuesList = values.ToList();
                        for (int i = 0; i < dif; i++)
                        {
                            valuesList.Add(defaultFieldValue);
                        }
                        values = valuesList.ToArray();
                        rows.Add(values);
                    }
                    else
                    {
                        rows.Add(values);
                    }


                    sql.Append($"INSERT INTO Table_{SqlGenerator.SanitizeName(originalFileName)} (");

                    foreach (var column in columnNames)
                    {
                        sql.Append($"[{SqlGenerator.SanitizeName(column)}],");
                    }

                    // Remove the last comma and close the column names part
                    sql.Length -= 1;
                    sql.Append(") VALUES (");

                    foreach (var value in values)
                    {
                        sql.Append($"'{SqlGenerator.SanitizeValue(value)}',");
                    }

                    // Remove the last comma and close the values part
                    sql.Length -= 1;
                    sql.AppendLine(");");

                }

                lineNumber++;
            }

            string sqlFileName = SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(originalFileName));

            // new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            // SHOULD RETURN THE FILE NAME OF THE SQL VERSION.
            return sqlFileName;
        }

        public static string    csvToSQLWithNewColumns(List<string> columnsList, string fileName, string originalFileName)
        {
            // SQL SCRIPT
            StringBuilder sql = new StringBuilder();


            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath)) { }

            // Open the file stream
            using Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream);

            // Lists to store the column names and rows
            string[] columnNames = null;
            var rows = new List<string[]>();
            int rowsId = 0;
            int columnsCount = 0;
            int lineNumber = 0;


            // Create table
            sql.AppendLine($"CREATE TABLE Table_{SqlGenerator.SanitizeName(originalFileName)} (");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string pattern = @"&\w+;";
                string cleaned = Regex.Replace(line, pattern, "");
                var values = cleaned.Split(';');

                if (lineNumber == 0)
                {
                    // First line contains column names
                    
                    var columnNamesList = values.ToList();

                    foreach (string colName in columnsList)
                    {
                        columnNamesList.Add(colName);
                    }

                    values = columnNamesList.ToArray();
                    columnsCount = values.Length;
                    columnNames = Tools.UpdateDuplicates(Tools.ReplaceSpecialCharacters(values).ToArray());

                    foreach (var column in columnNames)
                    {
                        sql.AppendLine($"    [{SqlGenerator.SanitizeName(column)}] VARCHAR(MAX) NULL,");
                    }
                    sql.Length -= 1;
                    sql.AppendLine(")");
                    sql.AppendLine("GO");
                    sql.AppendLine("");
                }
                else
                {
                    if (values.Length > columnsCount)
                    {
                        rows.Add(values);
                    }
                    else if (values.Length < columnsCount)
                    {
                        int dif = columnsCount - values.Length;

                        var valuesList = values.ToList();
                        for (int i = 0; i < dif; i++)
                        {
                            valuesList.Add("NULL");
                        }
                        values = valuesList.ToArray();
                        rows.Add(values);
                    }
                    else
                    {
                        rows.Add(values);
                    }


                    sql.Append($"INSERT INTO Table_{SqlGenerator.SanitizeName(originalFileName)} (");

                    foreach (var column in columnNames)
                    {
                        sql.Append($"[{SqlGenerator.SanitizeName(column)}],");
                    }

                    // Remove the last comma and close the column names part
                    sql.Length -= 1;
                    sql.Append(") VALUES (");

                    foreach (var value in values)
                    {
                        sql.Append($"'{SqlGenerator.SanitizeValue(value)}',");
                    }

                    // Remove the last comma and close the values part
                    sql.Length -= 1;
                    sql.AppendLine(");");

                }

                lineNumber++;
            }

            string sqlFileName = SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(originalFileName));

            // new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            // SHOULD RETURN THE FILE NAME OF THE SQL VERSION.
            return sqlFileName;
        }
    }
}
