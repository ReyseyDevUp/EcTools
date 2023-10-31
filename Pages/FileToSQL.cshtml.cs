using EcTools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace EcTools.Pages
{
    public class FileToSQLModel : PageModel
    {

        public string Message { get; set; }
        public int rowsId { get; set; }
        public List<string> DisplayLines { get; set; } = new List<string>();
        public string[] columnNames { get; set; }
        public int columnsCount { get; set; }
        public List<string[]> rows { get; set; } = new List<string[]>();
        public List<int> dataLogs { get; set; } = new List<int>();
        public string orginalFileName { get; set; }
        public string FileName { get; set; }

        public List<CSV_FileConfig> fileConfigs { get; set; } = new List<CSV_FileConfig>();


        private readonly ILogger<IndexModel> _logger;

        public FileToSQLModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }


        public IActionResult OnPost(IFormCollection formCollection)
        {
            var UploadedFile = formCollection.Files.FirstOrDefault();
            if (UploadedFile != null && UploadedFile.Length > 0)
            { 
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadedFile.FileName;

                orginalFileName = UploadedFile.FileName;
                FileName = uniqueFileName;

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    UploadedFile.CopyTo(stream);
                }

                Message = "File uploaded successfully!";
                
                (columnNames,rows) = ProcessCsvFile(UploadedFile);

            }
            else
            {
                Message = "Please select a file to upload.";
            }

            return Page();
        }


        public (string[] columnNames, List<string[]> rows) ProcessCsvFile(IFormFile file)
        {
            // Lists to store the column names and rows
            string[] columnNames = null;
            var rows = new List<string[]>();
            rowsId = 0;

            // Check if the file is not null and has content
            if (file != null && file.Length > 0)
            {
                using var reader = new StreamReader(file.OpenReadStream());
                int lineNumber = 0;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string pattern = @"&\w+;";
                    string cleaned = Regex.Replace(line, pattern, "");
                    var values = cleaned.Split(';');

                    // 
                    // CSV_FileConfig cfg = new CSV_FileConfig()
                    _logger.LogInformation($"FileName [{orginalFileName}] values.Length[{values.Length}], rowsId[{rowsId}]");
                    fileConfigs.Add(new CSV_FileConfig(orginalFileName, values.Length, rowsId, values));

                    if (lineNumber == 0)
                    {
                        // First line contains column names
                        columnNames = values;
                        columnsCount = values.Length;
                        _logger.LogInformation("Columns Count: "+columnsCount);
                    }
                    else
                    {
                        if(values.Length > columnsCount)
                        {
                            // WE HAVE MORE DATA THAN THE AVAILABLE COLUMNS
                            // MEAN I NEED TO PROMPT THE USER TO ADD MORE COLUMNS
                            // CASE IF THIS HEPPEN WITHIN THE 2,3,4 LINES MEANS THAT THE DATA IS CORRECT AND COLUMNS NAMES ARE MISSING
                            // CASE IF THIS HAPPEN WITHIN THE 2 BUT NOT IN THE OTHER LINES IT MEANS THAT THE DATA VULES IS WRONG.

                            _logger.LogInformation($"values.Length({values.Length}) > columnsCount({columnsCount})");
                            rows.Add(values);
                        }
                        else if(values.Length < columnsCount)
                        {

                            // WE HAVE MORE COLUMNS THAN THE GIVEN VALUES IN THIS LINE
                            // MEAN WE HAVE TO CHECK IF WE HAVE A DEFINED VALUE
                            // OR PROMPT AN INTERFACE TO THE USER WHERE HE CAN SET THE DEFAULT VALUE
                            
                            int dif = columnsCount - values.Length;

                            var valuesList = values.ToList();
                            for (int i = 0 ; i < dif; i++)
                            {
                                valuesList.Add("NULL");
                            }
                            values = valuesList.ToArray();

                            rows.Add(values);
                            _logger.LogInformation(line);
                            _logger.LogInformation("Rows Data Columns Count: " + values.Length);
                            // ADD DEFAULT VALUES ("NULL")

                            dataLogs.Add(lineNumber);
                        }
                        else
                        {
                            rows.Add(values);
                            _logger.LogInformation(line);
                            _logger.LogInformation("Rows Data Columns Count: " + values.Length);
                        }
                        
                    }

                    lineNumber++;
                    rowsId++;
                }

                new SqlGenerator().GenerateSql(orginalFileName, columnNames, rows);
            }

            return (columnNames, rows);
        }

        public IActionResult OnPostCorrectData(string defaultValue)
        {
            foreach (var lineNumber in dataLogs)
            {
                var row = rows[lineNumber - 1]; // -1 because the first line is column names
                var correctedRow = new List<string>(row);

                while (correctedRow.Count < columnNames.Length)
                {
                    correctedRow.Add(defaultValue);
                }

                rows[lineNumber - 1] = correctedRow.ToArray();
            }

            dataLogs.Clear(); // Clear the error logs after correction

            return Page();
        }


        public void outputData(string[] columns, List<string[]> rows)
        {


            // CREATE THE OUPTPUT FOLDER
            var outputsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/outputs");

            if (!Directory.Exists(outputsFolder))
            {
                Directory.CreateDirectory(outputsFolder);
            }

            foreach (var column in columns) {

            }
            foreach (var row in rows) { 
            }

        }
    }
}
