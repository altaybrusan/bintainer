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
		private IAppLogger _appLogger;

		public TemplateModel(ITemplateService service,
							 IStringLocalizer<ErrorMessages> localizer,
							 IAppLogger appLogger )
		{
			_templateService = service;
			_localizer = localizer;
			_appLogger = appLogger;
		}

        public void OnGet()
        {
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			_templateService.EnsureRootNodeExists(userId);
            
			var response = _templateService.GetPartCategories(userId);
			if(response.IsSuccess && response.Result is not null)
			{
				Categories = response.Result;
			}
            var attributeResponse = _templateService.GetAttributeTemplates(userId);
            if (attributeResponse.IsSuccess && response.Result is not null)
            {
				AttributeTables = attributeResponse.Result!;
            }
		}
		
	       
		public IActionResult OnPostLoadTemplatesDefaultAttributes(Guid templateGuid) 
		{
			
			var response = _templateService.GetTemplatesDefaultAttributes(templateGuid);
			if (!response.IsSuccess)
				return new JsonResult(new { success = false, message = response.Message });

			return new JsonResult(response.Result);
		}
		public void OnPostAttributesTemplateSave([FromBody] CreateAttributeTemplateRequest attributeTable) 
        {			
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			_templateService.SaveAttributeTemplate(attributeTable, userId);
		}
		
		public IActionResult OnPostDeleteAttributeTable(int tableId)
		{
			if (!ModelState.IsValid)
			{
				_appLogger.LogModelError(nameof(OnPostDeleteAttributeTable), ModelState);

				return BadRequest(new
				{
					success = false,
					message = _localizer["ErrorModelStateError"],
				});
			}
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			_templateService.RemoveAttributeTemplate(userId, tableId);
			return new OkResult();
		}

		public IActionResult OnPostCategorySave([FromBody] List<CategoryViewModel> categories)
		{
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			var response = _templateService.SavePartCategory(categories, userId);
            if (!response.IsSuccess)
                return new JsonResult(new { success = false, message = response.Message });

            return new JsonResult(response.Result);
        }
    }
}
