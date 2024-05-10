using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace Bintainer.WebApp.Pages.Dashboard
{
	public class AttributeTableTemplate
	{
		public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
		public string TableName { get; set; } = string.Empty;
	}
	public class CategoryView
	{
		public string Title { get; set; } = string.Empty;
		public int? Id { get; set; }
		public int? ParentId { get; set; }
		public List<CategoryView> Children { get; set; } = new();
	}
	public class CategoryViewComparer
	{
		public List<CategoryView> Added { get; private set; }
		public List<CategoryView> Deleted { get; private set; }
		public List<CategoryView> Unchanged { get; private set; }
		public List<CategoryView> Updated { get; private set; }

		public CategoryViewComparer(List<CategoryView> original, List<CategoryView> updated)
		{
			Added = new List<CategoryView>();
			Deleted = new List<CategoryView>();
			Unchanged = new List<CategoryView>();
			Updated = new List<CategoryView>();

			CompareLists(original, updated, null);
		}

		private void CompareLists(List<CategoryView> original, List<CategoryView> updated, int? parentId)
		{
			var originalDict = original.ToDictionary(item => item.Id ?? -1); 

			foreach (var item in updated)
			{

				if (!originalDict.ContainsKey(item.Id ?? -1))
				{
					item.ParentId = parentId;
					Added.Add(item);
				}
				else
				{
					var originalItem = originalDict.ContainsKey(item.Id ?? -1) ? originalDict[item.Id ?? -1] : null;
					if (IsUpdated(originalItem, item))
					{
						Updated.Add(new CategoryView { Title = item.Title, Id = item.Id });
					}
					else
					{
						Unchanged.Add(new CategoryView { Title = originalItem.Title, Id = item.Id });
					}
					originalDict.Remove(item.Id ?? -1);
				}

			}

			Deleted.AddRange(originalDict.Values);

			foreach (var originalItem in original)
			{
				var updatedChild = updated.FirstOrDefault(u => u.Id == originalItem.Id);
				if (updatedChild != null)
				{
					CompareLists(originalItem.Children, updatedChild?.Children ?? new List<CategoryView>(), originalItem.Id); // Pass parent ID
				}
			}
		}

		private bool IsUpdated(CategoryView original, CategoryView updated)
		{
			return original.Title != updated.Title;
		}
	}


	public class TemplateModel : PageModel
    {
		BintainerContext _dbcontext;

        public List<string> AttributeTables { get; set; } = new List<string>();
		public List<CategoryView> Categories { get; set; } = new();


        public List<CategoryView> BuildCategoryTree(IEnumerable<PartCategory> categories, int? parentId = null)
		{
			return categories.Where(c => c.ParentCategoryId == parentId)
							 .Select(c => new CategoryView
							 {
								 Title = c.Name?.Trim() ?? string.Empty,
								 Id= c.Id,
								 Children = BuildCategoryTree(categories, c.Id)
							 }).ToList();
		}

		public async Task<List<CategoryView>> GetCategoryHierarchyAsync()
		{
			var categories = await _dbcontext.PartCategories.ToListAsync();
			return BuildCategoryTree(categories);
		}

		public TemplateModel(BintainerContext dbContext )
		{
			_dbcontext = dbContext;
			foreach (var item in _dbcontext.PartAttributeTemplates)
			{
				if (item.TemplateName != null)
					AttributeTables.Add(item.TemplateName);
			}
		}
        public async Task OnGet()
        {
			Categories = await GetCategoryHierarchyAsync();
		}


        public void OnPostTest([FromBody] AttributeTableTemplate attributeTable) 
        {
			
			PartAttributeTemplate table = new() { TemplateName = attributeTable.TableName };

			foreach (var item in attributeTable.Attributes)
			{
				var attribute = new PartAttribute() { Name = item.Key, Value = item.Value };
				table.PartAttributes.Add(attribute);

			}
			_dbcontext.PartAttributeTemplates.Add(table);
			_dbcontext.SaveChanges();

		}


		public async Task OnPostTest2([FromBody] List<CategoryView> categories)
		{
			var original = await GetCategoryHierarchyAsync();
			CategoryViewComparer comparer = new CategoryViewComparer(original, categories);
			var result1 = comparer.Added;
			var result2 = comparer.Deleted;
			var result3 = comparer.Unchanged;
			var result4 = comparer.Updated;


		}
	}
}
