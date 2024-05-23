using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<string?> Packages { get; set; } = new List<string?>();
        BintainerContext _dbcontext;
		public PartModel(BintainerContext dbContext)
        {
			_dbcontext = dbContext;
            Packages = _dbcontext.PartPackages.Select(p => p.Name).ToList();
		}

        public void OnGet()
        {

        }
    }
}
