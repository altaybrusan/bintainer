using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Service;
using Bintainer.Service.Interface;
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
        public List<InventorySection>? Sections { get; set; } = new();
        public string InventoryName { get; set; } = string.Empty;

        readonly IInventoryService _inventoryService;
        public InventoryModel(IInventoryService inventoryService, SignInManager<IdentityUser> signInManager)
        {
            _SignInManager = signInManager;
            _inventoryService = inventoryService;
           
        }
        public void OnGet()
        {
            if(User.Identity != null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                Sections = _inventoryService.GetInventorySectionsOfUser(userName);
            }            
        }
        public void OnPostSubmitForm([FromBody] List<InventorySection> sectionList, string inventoryName) 
        {
            //TODO: check if this logic is fine or not
            if(User.Identity != null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                var UserId = User.Claims.ToList().FirstOrDefault(c=>c.Type.Contains("nameidentifier"))?.Value;
                
                UserViewModel user = new() { Name = userName, UserId = UserId };
               
                var inventory = _inventoryService.CreateOrUpdateInventory(user, inventoryName);
                _ = _inventoryService.AddSectionsToInventory(sectionList, inventory);
                
            }            
        }
    }
}
