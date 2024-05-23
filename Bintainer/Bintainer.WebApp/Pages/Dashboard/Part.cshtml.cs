using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();
        BintainerContext _dbcontext;
		public PartModel(BintainerContext dbContext)
        {
			_dbcontext = dbContext;
            Packages = _dbcontext.PartPackages.ToList();
		}

        public void OnGet()
        {

        }
    }
}
