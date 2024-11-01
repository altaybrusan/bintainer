using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Const;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Bintainer.WebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Text.Json.Serialization;


namespace Bintainer.WebApp.Pages.Dashboard
{
    public class InventoryModel : PageModel
    {
        SignInManager<IdentityUser> _SignInManager;
        public List<InventorySection>? Sections { get; set; } = new();
        public string InventoryName { get; set; } = string.Empty;
        
        public int MaxSectionWidth { get; private set; }
        public int MaxSectionHeight { get; private set; }
        public int MinSectionWidth {  get; private set; }
        public int MinSectionHeight {  get; private set; }
        public int MaxSubspace {  get; private set; }
        public int MinSubspace {  get; private set; }

        private readonly IInventoryService _inventoryService;
        private readonly IStringLocalizer _localizer;
        private readonly IAppLogger _appLogger;
        public InventoryModel(IInventoryService inventoryService, 
                              SignInManager<IdentityUser> signInManager,
                              IStringLocalizer<ErrorMessages> localizer,
                              IAppLogger appLogger)
        {
            _SignInManager = signInManager;
            _inventoryService = inventoryService;
            _localizer = localizer;
            _appLogger = appLogger;

            MinSectionWidth = GlobalConstants.MinSectionWidth;
            MinSectionHeight = GlobalConstants.MinSectionHeight;
            MaxSectionWidth = GlobalConstants.MaxSectionWidth;
            MaxSectionHeight = GlobalConstants.MaxSectionHeight;
            MaxSubspace = GlobalConstants.MaxSubspace;
            MinSubspace = GlobalConstants.MinSubspace;
        }
        public void OnGet()
        {
            if(User.Identity is not null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                var response = _inventoryService.GetInventorySectionsOfUser(userName);
                if (response is not null && response.IsSuccess && string.IsNullOrEmpty(response.Message)) 
                {
                    Sections = response.Result;
                }
            }            
            _appLogger.LogMessage(_localizer["WarningInvalidUser"], LogLevel.Warning);
        }
        
        public IActionResult OnPostSubmitForm([FromBody] List<InventorySection> sectionList, string inventoryName) 
        {

            if (!ModelState.IsValid)
            {
                _appLogger.LogModelError(nameof(OnPostSubmitForm), ModelState);

                return BadRequest(new
                {
                    success = false,
                    message = _localizer["ErrorModelStateError"],
                });
            }

            if (User.Identity != null) 
            {
                string userName = User.Identity.Name ?? string.Empty;
                var UserId = User.Claims.ToList().FirstOrDefault(c=>c.Type.Contains("nameidentifier"))?.Value;
                
                UserViewModel user = new() { Name = userName, UserId = UserId };
               
                var inventory = _inventoryService.CreateOrUpdateInventory(user, inventoryName);
                if(inventory is not null && inventory.IsSuccess && inventory.Result is not null) 
                {
                    _ = _inventoryService.AddSectionsToInventory(sectionList, inventory.Result);
                }
                else 
                {
                    _appLogger.LogMessage(_localizer["WarningInventoryNotExist"], LogLevel.Information);

                    return new JsonResult(new { success = false, message = inventory?.Message });
                }
                _appLogger.LogMessage(_localizer["InfoRepositoryCreateOrUpdateSuccess"], LogLevel.Information);
                return new JsonResult(new { success = true, message = _localizer["InfoRepositoryCreateOrUpdateSuccess"] });
            }

            _appLogger.LogMessage(_localizer["WarningInvalidUser"], LogLevel.Warning);
            
            return new JsonResult(new { success = false, message = _localizer["WarningInvalidUser"] });

        }
    }
}
