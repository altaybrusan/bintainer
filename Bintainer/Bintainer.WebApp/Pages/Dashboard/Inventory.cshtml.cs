using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class InventoryViewModel 
    {
        [JsonPropertyName("sectionName")]
        public string SectionName { get; set; }= string.Empty;
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("subSections")]
        public int Subsections { get; set; }
    };
    public class InventoryModel : PageModel
    {
        SignInManager<IdentityUser> _SignInManager;
        BintainerContext _dbContext;


        public InventoryModel(SignInManager<IdentityUser> signInManager, BintainerContext dbContext)
        {
            _SignInManager = signInManager;
            _dbContext = dbContext;

        }

        public void OnGet()
        {
        }
        public void OnPostSubmitForm([FromBody] List<InventoryViewModel> sections) 
        {
            foreach (var item in sections)
            {
                _dbContext.InventorySections.Add(new InventorySection() { SectionName = item.SectionName, Width = item.Width, Height = item.Height });
            }

            _dbContext.SaveChanges();
        }
    }
}
