using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();
        public List<PartCategory> Category { get; set; } = new List<PartCategory>();
        public List<PartGroup> Group { get; set; } = new List<PartGroup>();
        public Dictionary<int, string> AttributeTables { get; set; } = new Dictionary<int, string>();


        BintainerContext _dbcontext;
		public PartModel(BintainerContext dbContext)
        {
			_dbcontext = dbContext;
            Packages = _dbcontext.PartPackages.ToList();
            Category = _dbcontext.PartCategories.ToList();
            Group = _dbcontext.PartGroups.ToList();

            foreach (var item in _dbcontext.PartAttributeTemplates)
            {
                if (item.TemplateName != null)
                    AttributeTables[item.Id] = item.TemplateName;
            }
        }

        public void OnGet()
        {

        }
    }
}
