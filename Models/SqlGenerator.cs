using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EcTools.Models
{
    public class SqlGenerator
    {
        public string GenerateSql(string fileName, string[] columns, List<string[]> rows)
        {
            StringBuilder sql = new StringBuilder();

            // Create database
            // sql.AppendLine($"CREATE DATABASE {SanitizeName(fileName)};");
            // sql.AppendLine($"USE {SanitizeName(fileName)};");

            // Create table
            sql.AppendLine($"CREATE TABLE Table_{SanitizeName(fileName)} (");

            foreach (var column in columns)
            {
                sql.AppendLine($"    [{SanitizeName(column)}] VARCHAR(MAX) NULL,");
            }

            // Remove the last comma and close the table definition
            sql.Length -= 3;
            sql.AppendLine(");");

            // Insert rows
            foreach (var row in rows)
            {
                sql.Append($"INSERT INTO {SanitizeName(fileName)}Table (");

                foreach (var column in columns)
                {
                    sql.Append($"[{SanitizeName(column)}],");
                }

                // Remove the last comma and close the column names part
                sql.Length -= 1;
                sql.Append(") VALUES (");

                foreach (var value in row)
                {
                    sql.Append($"'{SanitizeValue(value)}',");
                }

                // Remove the last comma and close the values part
                sql.Length -= 1;
                sql.AppendLine(");");
            }
            
            SaveSqlToFile(sql.ToString(),SanitizeName(fileName));
            return sql.ToString();
        }

        public static string SanitizeName(string name)
        {
            // Simple sanitization to remove invalid characters for SQL identifiers
            // This can be expanded based on specific requirements
            return name.Trim().Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        }

        public static string SanitizeValue(string value)
        {
            // Escape single quotes for SQL string literals
            return value.Replace("'", "''");
        }

        public static string SaveSqlToFile(string sql, string fileName)
        {
            
            
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/generatedSQL");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string sqlFileName = $"{Guid.NewGuid().ToString()}_{fileName}.sql";

            string outputFile = Path.Combine(outputPath, sqlFileName);

            File.WriteAllText(outputFile, sql);

            return sqlFileName;
        }
    }
}