using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bintainer.WebApp.Pages.Dashboard
{
	public class AttributeSetTemplate
	{
		public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
		public string TableName { get; set; } = string.Empty;
	}
	public class TemplateModel : PageModel
    {
        public void OnGet()
        {
        }


        public void OnPostTest([FromBody] AttributeSetTemplate attributeTable) 
        {

		}

    }
}
