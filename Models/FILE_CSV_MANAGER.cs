using EcTools.Pages;
using System.Text;
using System.Text.RegularExpressions;

namespace EcTools.Models
{
    public class FILE_CSV_MANAGER
    {
        public static void csvToSQL(string fileName, string originalFileName)
        {
            // SQL SCRIPT
            StringBuilder sql = new StringBuilder();


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
            int         rowsId = 0;
            int         columnsCount = 0;
            int         lineNumber = 0;


            // Create table
            sql.AppendLine($"CREATE TABLE Table_{SqlGenerator.SanitizeName(fileName)} (");

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

                    
                    sql.Append($"INSERT INTO {SqlGenerator.SanitizeName(fileName)}Table (");

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

            SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(fileName));

            // new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            // SHOULD RETURN THE FILE NAME OF THE SQL VERSION.
        }

        public static void csvToSQLWithDefaultValus(string fileName, string originalFileName, string defaultFieldValue)
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

            SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(fileName));

            // new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            // SHOULD RETURN THE FILE NAME OF THE SQL VERSION.
        }

        public static void csvToSQLWithNewColumns(string fileName, string originalFileName)
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
            sql.AppendLine($"CREATE TABLE Table_{SqlGenerator.SanitizeName(fileName)} (");

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


                    sql.Append($"INSERT INTO {SqlGenerator.SanitizeName(fileName)}Table (");

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

            SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(fileName));

            // new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            // SHOULD RETURN THE FILE NAME OF THE SQL VERSION.
        }
    }
}
