using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EcTools.Models;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace EcTools.Pages
{
    public class ToSQLModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string FileName { get; set; }
        public string FileFullPath { get; set; }
        public List<int> ErrorRowsIds { get; set; } = new List<int>();
        public List<string[]> ErrorRowsContent { get; set; } = new List<string[]>();
        public int RowId { get; set; } = 0;
        public string[] columnNames { get; set; }
        public int columnsCount { get; set; }
        public List<string[]> rows { get; set; } = new List<string[]>();
        public bool FileIsValid = false;
        public List<CSV_ERROR> csvErrorList { get; set; } = new List<CSV_ERROR>();
        public Stream CurrentFile { get; set; }
        public bool isEmptyFieldsValue { get; set; } = false;
        public string EmptyFieldsValue { get; set; } = "";

        public ToSQLModel(ILogger<IndexModel> logger)
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
                                    
                                    FileName = guid + entryFileName;

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
                    return new JsonResult(new { success = false, message = "Error uploading file.", filefullpath= FileFullPath, filename= guid+FileName, originalFileName = FileName });
                }

                return new JsonResult(new { success = true, message = "File uploaded successfully!", filefullpath = FileFullPath, filename= guid+FileName, originalFileName= FileName });
            }

            return new BadRequestResult();
        }

        public IActionResult OnGetVerifyCSV([FromQuery] string filename)
        {
            _logger.LogInformation("OnPostVerifyCSV:"+ filename);

            return new JsonResult(VerifyCsvFile(filename));


        }
        public List<CSV_ERROR> VerifyCsvFile(string fileName)
        {
            // Construct the full path to the file in the uploads directory
            var _uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(_uploads, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"File {fileName} not found in uploads directory.");
                return new List<CSV_ERROR> { new CSV_ERROR("FILE_NOT_FOUND", $"File {fileName} not found.", 0, null,null) };
            }

            // Open the file stream
            using Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Lists to store the column names and rows
            string[] columnNames = null;
            var rows = new List<string[]>();

            RowId = 0;
            ErrorRowsContent = new List<string[]>();
            ErrorRowsIds = new List<int>();
            csvErrorList = new List<CSV_ERROR>();

            // Process the file content
            using var reader = new StreamReader(fileStream);
            int lineNumber = 0;

            while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string pattern = @"&\w+;";
                    string cleaned = Regex.Replace(line, pattern, "");
                    var values = cleaned.Split(';');

                    // 
                    // CSV_FileConfig cfg = new CSV_FileConfig()
                    _logger.LogInformation($"FileName [{FileName}] values.Length[{values.Length}], rowsId[{RowId}]");
                    // fileConfigs.Add(new CSV_FileConfig(orginalFileName, values.Length, rowsId, values));
                    if(lineNumber < 5)
                    {
                        if (lineNumber == 0)
                        {
                            // First line contains column names
                            columnNames = Tools.ReplaceSpecialCharacters(values).ToArray();
                            columnsCount = values.Length;
                            _logger.LogInformation("Columns Count: " + columnsCount);
                        }
                        else
                        {
                            if (values.Length > columnsCount)
                            {
                                // WE HAVE MORE DATA THAN THE AVAILABLE COLUMNS
                                // MEAN I NEED TO PROMPT THE USER TO ADD MORE COLUMNS
                                // CASE IF THIS HEPPEN WITHIN THE 2,3,4 LINES MEANS THAT THE DATA IS CORRECT AND COLUMNS NAMES ARE MISSING
                                // CASE IF THIS HAPPEN WITHIN THE 2 BUT NOT IN THE OTHER LINES IT MEANS THAT THE DATA VULES IS WRONG.

                                _logger.LogInformation($"values.Length({values.Length}) > columnsCount({columnsCount})");
                                rows.Add(values);

                                 csvErrorList.Add(new CSV_ERROR(
                                          "MORE_DATA_THAN_COLUMNS"
                                        , "We have more data in this line than the available or defined columns"
                                        , RowId
                                        , values
                                        , columnNames
                                        ));

                                return csvErrorList;
                            }
                            else if (values.Length < columnsCount)
                            {

                                // WE HAVE MORE COLUMNS THAN THE GIVEN VALUES IN THIS LINE
                                // MEAN WE HAVE TO CHECK IF WE HAVE A DEFINED VALUE
                                // OR PROMPT AN INTERFACE TO THE USER WHERE HE CAN SET THE DEFAULT VALUE

                                rows.Add(values);
                                _logger.LogInformation(line);
                                _logger.LogInformation("Rows Data Columns Count: " + values.Length);
                                // ADD DEFAULT VALUES ("NULL") 

                                ErrorRowsIds.Add(lineNumber);

                                 csvErrorList.Add(new CSV_ERROR(
                                          "MORE_COLUMNS_THAN_DATA"
                                        , "We have more columns in this line than the available data"
                                        , RowId
                                        , values
                                        , columnNames
                                        ));
                                return csvErrorList;
                        }
                            else
                            {
                                rows.Add(values);
                                _logger.LogInformation(line);
                                _logger.LogInformation("Rows Data Columns Count: " + values.Length);
                            }

                        }
                    } else
                    {
                        return csvErrorList;
                    }
                    
                    lineNumber++;
                    RowId++;
                }
            return csvErrorList;
        }


        public async Task<IActionResult> OnPostFieldsDefaultValueManager()
        {
            // Read the request body asynchronously
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Deserialize the JSON payload to get the defaultValue
            var jsonDocument = JsonDocument.Parse(body);
            if (!jsonDocument.RootElement.TryGetProperty("defaultValue", out var defaultValueElement))
            {
                return new BadRequestObjectResult(new { success = false, message = "defaultValue not provided." });
            }
            if (!jsonDocument.RootElement.TryGetProperty("fileName", out var fileName))
            {
                return new BadRequestObjectResult(new { success = false, message = "fileName not provided." });
            }
            if (!jsonDocument.RootElement.TryGetProperty("originalFileName", out var originalFileName))
            {
                return new BadRequestObjectResult(new { success = false, message = "originalFileName not provided." });
            }

            // PROCESS THE FILE AND FILL THE EMPTY FIELDS WITH THE GIVEN VALUE 'defaultValue'
            isEmptyFieldsValue  = true;
            EmptyFieldsValue    = defaultValueElement.GetString();

            FILE_CSV_MANAGER.csvToSQLWithDefaultValus(fileName.GetString(), originalFileName.GetString(), defaultValueElement.GetString());

            return new JsonResult(new { success = true, message = "Default value applied successfully.", filename = fileName.GetString() });
        }

        public IActionResult OnPostColumnsValueManager()
        {
            return new JsonResult(new { success = true, message = "DEV MODE" });
        }

    }
}
