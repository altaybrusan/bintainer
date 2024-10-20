using Bintainer.Model;
using Bintainer.WebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class InventoryModel : PageModel
    {
        SignInManager<IdentityUser> _SignInManager;
        BintainerDbContext _dbContext;

        public List<InventorySection> Sections { get; set; } = new();
        public string InventoryName { get; set; } = string.Empty;

        public InventoryModel(SignInManager<IdentityUser> signInManager, BintainerDbContext dbContext)
        {
            _SignInManager = signInManager;
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            if(User.Identity != null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                var inventory = _dbContext.Inventories.FirstOrDefault(i => i.Admin == userName);
                InventoryName = inventory?.Name;

                if(inventory != null) 
                {
                    Sections = _dbContext.InventorySections.Where(s=>s.InventoryId==inventory.Id).ToList();
                }
                else 
                {
                    Sections = new List<InventorySection>();
                    Sections.Add(new InventorySection() { Height = 1, Width = 1 });
                }
            }            
        }
        public void OnPostSubmitForm([FromBody] List<InventorySection> sectionList, string inventoryName) 
        {
            if(User.Identity != null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                var inventory = _dbContext.Inventories.FirstOrDefault(i => i.Admin == userName);
                if (inventory == null)
                {
                    var UserId = User.Claims.ToList().FirstOrDefault(c=>c.Type.Contains("nameidentifier"))?.Value;
                    inventory = new Inventory() { Admin = userName, Name = inventoryName?.Trim(), UserId = UserId };
                    _dbContext.Inventories.Add(inventory);
                    _dbContext.SaveChanges();
                }
                else 
                {
                    // the user already created an inventory
                   if( inventory.Name != inventoryName) 
                   {
                        inventory.Name = inventoryName?.Trim();
                        _dbContext.Update(inventory);
                        _dbContext.SaveChanges(true);
                   }

                }

                foreach (var item in sectionList)
                {
                    if (item.Id == 0) 
                    {
                        item.InventoryId = inventory.Id;
                        _dbContext.InventorySections.Add(item);
                    }
                    else
                    {
                        item.InventoryId = inventory.Id;
                        _dbContext.InventorySections.Update(item);
                        _dbContext.SaveChanges(true);
                    }
                    
                }

                _dbContext.SaveChanges();
            }            
        }
    }
}
