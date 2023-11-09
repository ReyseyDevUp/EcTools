using EcTools.Models;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static OfficeOpenXml.ExcelErrorValue;

namespace EcTools.Pages
{
    public class xlsxToSQLModel : PageModel
    {
        public Stream CurrentFile { get; private set; }
        public string FileName { get; private set; }
        public string FileFullPath { get; private set; }

        private readonly ILogger<IndexModel> _logger;
        public xlsxToSQLModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var file = Request.Form.Files[0];
            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var guid = Guid.NewGuid().ToString() + "_";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var filePath = Path.Combine(uploads, guid + file.FileName);

                if (file.FileName.EndsWith(".zip"))
                {
                    // Handle ZIP files
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    using (var zip = new ICSharpCode.SharpZipLib.Zip.ZipFile(filePath))
                    {
                        foreach (ZipEntry entry in zip)
                        {
                            if (!entry.IsFile) continue; // Ignore directories

                            var entryFileName = entry.Name;


                            if (entryFileName.EndsWith(".csv") || entryFileName.EndsWith(".xls") || entryFileName.EndsWith(".xlsx"))
                            {
                                var buffer = new byte[4096];
                                using (var zipStream = zip.GetInputStream(entry))
                                {
                                    CurrentFile = zipStream;

                                    FileName = entryFileName;

                                    using (var entryStream = new FileStream(Path.Combine(uploads, guid + entryFileName), FileMode.Create))
                                    {
                                        StreamUtils.Copy(zipStream, entryStream, buffer);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (file.FileName.EndsWith(".csv") || file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
                {
                    // Handle CSV, XLS, XLSX files
                    FileFullPath = filePath;
                    FileName = file.FileName;
                    CurrentFile = file.OpenReadStream();

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Error uploading file.", filefullpath = FileFullPath, filename = guid + FileName, originalFileName = FileName });
                }

                return new JsonResult(new { success = true, message = "File uploaded successfully!", filefullpath = FileFullPath, filename = guid + FileName, originalFileName = FileName });
            }

            return new BadRequestResult();
        }


        public IActionResult OnGetFileWorksheets([FromQuery] string filename)
        {
            _logger.LogInformation("OnGetFileWorksheets:" + filename);



            return new JsonResult(GetWorksheetNames(filename));


        }

        public List<string> GetWorksheetNames(string _filePath)
        {
            var sheetNames = new List<string>();
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, _filePath);
            var fileInfo = new FileInfo(filePath);

            // Ensure the file exists
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("The specified file was not found.", filePath);
            }

            // EPPlus requires a license context for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(fileInfo))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    sheetNames.Add(worksheet.Name);
                }
            }

            return sheetNames;
        }

        public List<string> __getWorksheets(string fileName)
        {
            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"File {fileName} not found in uploads directory.");
                return new List<string> { "FILE_NOT_FOUND", $"File {fileName} not found." };
            }

            var dataTable = new DataTable();
            List<string> worksheets = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                int totalRows = worksheet.Dimension.Rows;
                int totalCols = worksheet.Dimension.Columns;

                // Get the header information (assuming the first row contains headers)
                for (int col = 1; col <= totalCols; col++)
                {
                    dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                }

                // Get the row data
                for (int row = 2; row <= totalRows; row++)
                {
                    var dataRow = dataTable.NewRow();
                    for (int col = 1; col <= totalCols; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return null;
        }

        public async Task<IActionResult> OnPostProcessValidFile()
        {
            // Read the request body asynchronously
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Deserialize the JSON payload to get the defaultValue
            var jsonDocument = JsonDocument.Parse(body);

            if (!jsonDocument.RootElement.TryGetProperty("fileName", out var fileName))
            {
                return new BadRequestObjectResult(new { success = false, message = "fileName not provided." });
            }
            if (!jsonDocument.RootElement.TryGetProperty("originalFileName", out var originalFileName))
            {
                return new BadRequestObjectResult(new { success = false, message = "originalFileName not provided." });
            }
            if (!jsonDocument.RootElement.TryGetProperty("worksheetIndex", out var worksheetIndex))
            {
                return new BadRequestObjectResult(new { success = false, message = "originalFileName not provided." });
            }


            string sqlFileName = this.xlsxToSQL(fileName.GetString(), originalFileName.GetString(), int.Parse(worksheetIndex.ToString()));

            return new JsonResult(new { success = true, message = $"File <b>{originalFileName}</b> Converted to SQL Successfully", sqlFileName = sqlFileName });
        }

        public string xlsxToSQL(string fileName, string originalFileName, int worksheetIndex)
        {
            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"File {fileName} not found in uploads directory.");
            }

            // Lists to store the column names and rows
            string[] columnNames = null;
            var rows = new List<string[]>();
            int columnsCount = 0;
            int lineNumber = 0;

            // SQL SCRIPT
            StringBuilder sql = new StringBuilder();
            string tableName = SqlGenerator.SanitizeName(originalFileName);
            sql.AppendLine($"CREATE TABLE Table_{tableName} (");
            string insertInto = $"INSERT INTO Table_{tableName} (";
            StringBuilder insertIntoColumns = new StringBuilder();

            List<string> dataTableColumns = new List<string>();
            List<string> worksheets = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                int totalRows = worksheet.Dimension.Rows;
                int totalCols = worksheet.Dimension.Columns;

                // Get the header information (assuming the first row contains headers)
                for (int col = 1; col <= totalCols; col++)
                {
                    dataTableColumns.Add(worksheet.Cells[1, col].Text);
                }

                columnNames = Tools.UpdateDuplicates(Tools.ReplaceSpecialCharacters(dataTableColumns.ToArray()).ToArray());
                columnsCount = columnNames.Length;

                for (int col = 1; col <= columnsCount; col++)
                {
                    // First line contains column names

                    string columnName = Tools.ReplaceSpecialCharacters(worksheet.Cells[1, col].Text);

                    sql.AppendLine($"    [{columnName}] VARCHAR(MAX) NULL,");
                    insertIntoColumns.Append($"[{columnName}],");
                }

                sql.Length -= 1;
                sql.AppendLine(")");
                sql.AppendLine("GO");
                sql.AppendLine("");

                insertIntoColumns.Length -= 1;
                insertIntoColumns.Append(") VALUES (");




                // Get the row data
                for (int row = 2; row <= totalRows; row++)
                {
                    sql.Append(insertInto);

                    sql.Append(insertIntoColumns);

                    for (int col = 1; col <= totalCols; col++)
                    {
                        sql.Append($"'{SqlGenerator.SanitizeValue(worksheet.Cells[row, col].Text)}',");
                    }

                    // Remove the last comma and close the values part
                    sql.Length -= 1;
                    sql.AppendLine(");");
                    lineNumber++;

                }
            }
             

            return SqlGenerator.SaveSqlToFile(sql.ToString(), SqlGenerator.SanitizeName(originalFileName));
        }

        public DataTable ReadExcel(string filePath)
        {
            var dataTable = new DataTable();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                int totalRows = worksheet.Dimension.Rows;
                int totalCols = worksheet.Dimension.Columns;

                // Get the header information (assuming the first row contains headers)
                for (int col = 1; col <= totalCols; col++)
                {
                    dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                }

                // Get the row data
                for (int row = 2; row <= totalRows; row++)
                {
                    var dataRow = dataTable.NewRow();
                    for (int col = 1; col <= totalCols; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }
    }
}
