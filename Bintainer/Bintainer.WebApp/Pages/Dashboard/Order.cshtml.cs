using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class OrderModel : PageModel
    {
        BintainerDbContext _dbcontext;
        public List<Part> Part { get; set; } = new List<Part>();

        public OrderModel(BintainerDbContext dbContext)
        {
            _dbcontext = dbContext;
            Part= _dbcontext.Parts.ToList();
        }

        public void OnGet()
        {

        }
    }
}
