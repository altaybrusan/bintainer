using Bintainer.WebApp.Data;
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
                Part _part= new Part();
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                _part.Name = request.PartName;
                _part.Description = request.Description;
                _part.CategoryId = request.CategoryId;
                _part.UserId = UserId;
                _part.Supplier = !string.IsNullOrEmpty(request.Supplier) ? request.Supplier : null;
                //TODO: links should be fetched here.

                string packageName;
                if ( string.IsNullOrEmpty(request.Package)) 
                {
                    packageName= "default";
                }
                else 
                {
                    packageName= request.Package;
                }

                var package = _dbcontext.PartPackages.FirstOrDefault(p => p.Name == packageName && p.UserId == UserId);
                if(package is null) 
                {                    
                    var result = _dbcontext.PartPackages.Add(new PartPackage() { Name = packageName, UserId = UserId });
                    _dbcontext.SaveChanges();
                    package = result.Entity;
                }

                _part.PackageId = package.Id;
                                
                List<PartAttribute> attributes = new List<PartAttribute>();
                var attributeTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.Id == request.AttributeTemplateId);
                if(attributeTemplate is null) 
                {
                    var defaultTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.TemplateName == _part.Name && t.UserId == UserId);
                    if (defaultTemplate is null)
                    {
                        // The part is fetched from external source (e.g., DigiKey)
                        attributeTemplate = new PartAttributeTemplate() { TemplateName = _part.Name, UserId = UserId };                        
                        _dbcontext.PartAttributeTemplates.Add(attributeTemplate);
                        _dbcontext.SaveChanges();

                        //PartTemplate partTemplate = new()
                        //{
                        //    Supplier = "DigiKey",
                        //    PartName = _part.Name,
                        //    ImageUri = "TODO: fetch from attributes",
                        //    DatasheetUri = "TODO: datasheet",
                        //    UserId = UserId
                        //};
                        //_part.PartTemplates.Add(partTemplate);

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
                if (attributes.Any()) 
                {
                    _dbcontext.PartAttributes.AddRange(attributes);
                    _dbcontext.SaveChanges(true);
                }
                
                PartAttributeTemplate partAttributeTemplate = null;
                
                _dbcontext.Parts.Add(_part);
                _dbcontext.SaveChanges(true);
                
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
