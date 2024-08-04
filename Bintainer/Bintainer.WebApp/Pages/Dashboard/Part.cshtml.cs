using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Bintainer.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();

        public List<PartCategory> Category { get; set; } = new List<PartCategory>();
        
        public List<PartGroup> Group { get; set; } = new List<PartGroup>();
        public Dictionary<int, string> AttributeTemplatesTable { get; set; } = new Dictionary<int, string>();


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
            List<DigikeyService.Parameter>? parameters = new();
            try 
            {
                var details = await _digikeyService.GetProductDetailsAsync(partNumber);
                parameters = _digikeyService.ExtractParameters(details);
            }
            catch(Exception e) 
            {
                Console.Error.WriteLine($"Error fetching Digikey parameters for part number {partNumber}: {e.Message}");
                return new JsonResult(new { Error = "An error occurred while fetching the parameters. Please check the part number and try again." })
                {
                    StatusCode = 500 // Internal Server Error
                };
            }
            
            return new JsonResult(parameters);
        }

        public void OnGet()
        {
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			LoadTemplate(userId);            
		}

        public void OnPostSearchForPart(string partNumber) 
        {

        }
        public async Task OnPostFetchDigikey(string digiKeyPartNumber) 
        {
			var result = await _digikeyService.GetProductDetailsAsync(digiKeyPartNumber);            
		}

        public void OnPostCreatePart([FromBody]CreatePartRequestModel request) 
        {
            if (ModelState.IsValid) 
            {                
                // TODO: Reuest issued two or more time. Resolve this issue later.
                Part _part= new Part();
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                _part.Name = request.PartName;
                _part.Description = request.Description;
                _part.CategoryId = request.CategoryId;

                var packageName = request.Package;

                var package = _dbcontext.PartPackages.FirstOrDefault(p => p.Name == request.Package && p.UserId == UserId);
                if(package is null) 
                {                    
                    var result = _dbcontext.PartPackages.Add(new PartPackage() { Name = request.Package, UserId = UserId });
                    _dbcontext.SaveChanges();
                    package = result.Entity;
                }
                
                _part.UserId = UserId;
                
                List<PartAttribute> attributes = new List<PartAttribute>();
                var attributeTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.Id == request.AttributeTemplateId);
                if(attributeTemplate is null) 
                {
                    var defaultTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.TemplateName == "default");
                    if (defaultTemplate is null)
                    {
                        attributeTemplate = new PartAttributeTemplate() { TemplateName = "default", UserId = UserId };
                        _dbcontext.PartAttributeTemplates.Add(attributeTemplate);
                        _dbcontext.SaveChanges();
                    }
                    else
                    {
                        attributeTemplate = defaultTemplate;
                    }

                }
                foreach (var item in request.Attributes)
                {
                    attributes.Add(new PartAttribute() { Name = item.Key, Value = item.Value, Template = attributeTemplate });                    
                    
                }
            }

        }

        private void LoadTemplate(string userId)         
        {
			foreach (var item in _dbcontext.PartAttributeTemplates.Where(p=>p.UserId==userId))
			{
				if (item.TemplateName != null)
					AttributeTemplatesTable[item.Id] = item.TemplateName;
			}
		}
    }
}
