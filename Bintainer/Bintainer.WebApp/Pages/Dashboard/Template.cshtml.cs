using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.View;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Bintainer.WebApp.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using System.Runtime.Serialization;
using System.Security.Policy;


namespace Bintainer.WebApp.Pages.Dashboard
{
    
	public class TemplateModel : PageModel
    {

        public Dictionary<int,string> AttributeTables { get; set; } = new Dictionary<int, string>();
		public List<CategoryViewModel> Categories { get; set; } = new();
       
		private ITemplateService _templateService;
		private IStringLocalizer _localizer;
		private IAppLogger _applogger;

		public TemplateModel(ITemplateService service,
							 IStringLocalizer<ErrorMessages> localizer,
							 IAppLogger appLogger )
		{
			_templateService = service;
			_localizer = localizer;
			_applogger = appLogger;
		}

        public IActionResult OnGet()
        {
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			_templateService.EnsureRootNodeExists(userId);
            
			var response = _templateService.GetPartCategories(userId);
			if(!response.IsSuccess && response.Result is null)
			{
				return new JsonResult(new{ success = false, message = response.Message });
			}
			_templateService.LoadAttributes(userId);

			return new OkResult();
		}
		
		//TODO: update the javascript to meet new return back.
        public IActionResult GetCategoryHierarchy(string userId)
        {
            var categories = _templateService.GetPartCategories(userId);

			if (!categories.IsSuccess)
				return new JsonResult(new { success = true, result = categories });

            return new JsonResult(categories.Result);
        }
        
		public IActionResult OnPostLoadAttributeTable(int tableId) 
		{
			var response = _templateService.GetPartAttributes(tableId);
			if (!response.IsSuccess)
				return new JsonResult(new { success = false, message = response.Message });

			return new JsonResult(response.Result);
		}
		public void OnPostAttributesTemplateSave([FromBody] CreateAttributeTemplateRequest attributeTable) 
        {			
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			_templateService.SaveAttributeTemplate(attributeTable, userId);
		}
		//TODO: rewrite this
		//public async Task<IActionResult> OnPostDeleteAttributeTable(int tableId)
		//{
		//	await _dbcontext.PartAttributes.Where(a => a.TemplateId == tableId).ExecuteDeleteAsync();
		//	await _dbcontext.PartAttributeTemplates.Where(t => t.Id == tableId).ExecuteDeleteAsync();
		//	await _dbcontext.SaveChangesAsync();
		//	return new OkResult();
		//}

		public async Task OnPostCategorySave([FromBody] List<CategoryViewModel> categories)
		{
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;


        }
    }
}
