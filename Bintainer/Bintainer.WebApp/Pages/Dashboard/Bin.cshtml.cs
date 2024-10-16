using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class BinModel : PageModel
    {
        public List<Part> Part { get; set; } = new List<Part>();

        public BinModel(BintainerDbContext dbContext)
        {
            Part = dbContext.Parts.ToList();

        }
        public void OnGet()
        {
        }
    }
}
