using Microsoft.AspNetCore.Mvc;

namespace EcTools
{
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                // Define a unique name for the file
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;

                // Define a path to save the file (e.g., wwwroot/uploads/)
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                // Save the file to the defined path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                // Optionally, process the file or store the path in a database

                return RedirectToAction("Success");
            }

            return RedirectToAction("Error");
        }
    }
}
