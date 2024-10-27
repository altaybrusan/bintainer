using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.View;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
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
							 IStringLocalizer localizer,
							 IAppLogger appLogger )
		{
			_templateService = service;
			_localizer = localizer;
			_applogger = appLogger;
		}

        public async Task OnGet()
        {
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (FindRoot(userId) == null) 
			{
				AddRootNode(userId);
			}
			Categories = await GetCategoryHierarchyAsync(userId);
			LoadAttributes(userId);
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
			PartAttributeTemplate table = new() { TemplateName = attributeTable.TableName, UserId = userId };

			foreach (var item in attributeTable.Attributes)
			{
				var attribute = new PartAttribute() { Name = item.Key, Value = item.Value };
				table.PartAttributes.Add(attribute);

			}
			_dbcontext.PartAttributeTemplates.Add(table);
			_dbcontext.SaveChanges();

		}
		public async Task<IActionResult> OnPostDeleteAttributeTable(int tableId)
		{
			await _dbcontext.PartAttributes.Where(a => a.TemplateId == tableId).ExecuteDeleteAsync();
			await _dbcontext.PartAttributeTemplates.Where(t => t.Id == tableId).ExecuteDeleteAsync();
			await _dbcontext.SaveChangesAsync();
			return new OkResult();
		}

		private void LoadAttributes(string userId) 
		{
			foreach (var item in _dbcontext.PartAttributeTemplates.Where(p => p.UserId == userId))
			{
				if (item.TemplateName != null)
					AttributeTables[item.Id] = item.TemplateName;
			}
		}
		private PartCategory? FindRoot(string userId) 
		{
            return _dbcontext.PartCategories.Where(p=> p.UserId == userId).FirstOrDefault();        }
		private void AddRootNode(string userId) 
		{
			CategoryViewModel viewNode = new CategoryViewModel() {
				Title = "Root"
			};
			AddItem(viewNode, userId);

        }
		private void DeleteItem(CategoryViewModel parent) 
		{
			_dbcontext.PartCategories.Remove(_dbcontext.PartCategories.First(i => i.Id == parent.Id));
			foreach (var item in parent.Children)
			{
				DeleteItem(item);
			}						

		}
		private void AddItem(CategoryViewModel nodeView, string userId ,int? parentId=null)
		{
			PartCategory newCategory = new() { Name = nodeView.Title, UserId = userId };
			if(parentId != null) 
			{
				newCategory.ParentCategory = _dbcontext.PartCategories.First(i => i.Id == parentId);
			}
						
			_dbcontext.PartCategories.Add(newCategory);
			_dbcontext.SaveChanges();
			
			foreach (var item in nodeView.Children)
			{
				AddItem(item,userId, newCategory.Id);
			}
		}
		public async Task OnPostCategorySave([FromBody] List<CategoryViewModel> categories)
		{
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
			var original = await GetCategoryHierarchyAsync(userId);
			CategoryViewModelComparer comparer = new CategoryViewModelComparer(original, categories);
			var AddedItems = comparer.Added;
			var deletedItems = comparer.Deleted;
			var updatedItems = comparer.Updated;

            foreach (var item in updatedItems) 
			{
				var _category = _dbcontext.PartCategories.First(i => i.Id == item.Id);
				_category.Name= item.Title;
				_category.UserId = userId;

                _dbcontext.PartCategories.Update(_category);
			}

			foreach (var item in deletedItems) 
			{
				DeleteItem(item);
			}
			foreach (var item in AddedItems)
			{
				AddItem(item, userId, item.ParentId);
			}



			_dbcontext.SaveChanges();
		}
	}
}
