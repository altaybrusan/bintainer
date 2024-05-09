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
	public class TemplateModel : PageModel
    {
		BintainerContext _dbcontext;

        public List<string> AttributeTables { get; set; } = new List<string>();
         
        public TemplateModel(BintainerContext dbContext )
		{
			_dbcontext = dbContext;
			foreach (var item in _dbcontext.PartAttributeTemplates)
			{
				if (item.TemplateName != null)
					AttributeTables.Add(item.TemplateName);
			}
		}
        public void OnGet()
        {
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

    }
}
