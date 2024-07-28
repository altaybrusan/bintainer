using Bintainer.WebApp.Models;
using Bintainer.WebApp.Services;
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
        DigikeyService _digikeyService;
        public PartModel(BintainerContext dbContext, DigikeyService digikeyServices)
        {
			_dbcontext = dbContext;
            _digikeyService = digikeyServices;
            Packages = _dbcontext.PartPackages.ToList();
            Category = _dbcontext.PartCategories.ToList();
            Group = _dbcontext.PartGroups.ToList();

        }

        public async Task<IActionResult> OnGetDigikeyParameters(string partNumber) 
        {
            var details = await _digikeyService.GetProductDetailsAsync(partNumber);
            var parameters = _digikeyService.ExtractParameters(details);
            return new JsonResult(parameters);
        }

        public void OnGet()
        {
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			LoadTemplate(userId);
            
		}

        public async Task OnPostFetchDigikey(string digiKeyPartNumber) 
        {
			var result = await _digikeyService.GetProductDetailsAsync(digiKeyPartNumber);
            
		}

		private void LoadTemplate(string userId)         
        {
			foreach (var item in _dbcontext.PartAttributeTemplates.Where(p=>p.UserId==userId))
			{
				if (item.TemplateName != null)
					AttributeTables[item.Id] = item.TemplateName;
			}
		}
    }
}
