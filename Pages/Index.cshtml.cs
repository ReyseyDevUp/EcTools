using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Emit;

namespace EcTools.Pages
{
    public class IndexModel : PageModel
    {

        public string dev = "Mehdi Reysey";

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Models.Lab.Main(_logger);
        }

        public void OnPost()
        {

        }

    }
}