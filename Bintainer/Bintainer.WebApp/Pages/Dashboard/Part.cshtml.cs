using Amazon.Runtime.Internal;
using Amazon.SimpleEmail.Model.Internal.MarshallTransformations;
using Azure;
using Azure.Core;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Drawing2D;
using System.Runtime;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();

        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<string> PartNumbers { get; set; } = new List<string>();
        public List<PartGroup> Group { get; set; } = new List<PartGroup>();
        public List<PartTemplateInfo> AttributeTemplatesList { get; set; }

        //public List<InventorySection> Sections { get; set; } = new();
        
        public Inventory Inventory { get; set; } = new();

        private readonly DigikeyService _digikeyService;
        private readonly ITemplateService _templateService;
        private readonly IPartService _partService;
        private readonly IInventoryService _inventoryService;
        private readonly IAppLogger _appLogger;
        private readonly IStringLocalizer _localizer;
        public PartModel(ITemplateService templateService, 
                         IPartService partService,
                         IInventoryService inventoryService,
                         IAppLogger appLogger,
                         IStringLocalizer<ErrorMessages> sringLocalizer,
                         DigikeyService digikeyServices)
        {
            _templateService = templateService;
            _partService = partService;
            _inventoryService = inventoryService;
            _digikeyService = digikeyServices;
            _appLogger = appLogger;
            _localizer = sringLocalizer;
        }

        public async Task<IActionResult> OnGetDigikeyParameters(string partNumber) 
        {
            List<DigikeyService.Parameter>? parameters = new();
            try 
            {
                var details = await _digikeyService.GetProductDetailsAsync(partNumber);
                parameters = _digikeyService.ExtractParameters(details);
            }
            catch(Exception e) 
            {
                Console.Error.WriteLine($"Error fetching Digikey parameters for part number {partNumber}: {e.Message}");
                return new JsonResult(new { Error = "An error occurred while fetching the parameters. Please check the part number and try again." })
                {
                    StatusCode = 500 // Internal Server Error
                };
            }
            
            return new JsonResult(parameters);
        }

        public void OnGet()
        {
			var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
            var response = _inventoryService.GetInventoryById(userId);
            if (!response.IsSuccess) 
            {
                return;// new JsonResult(new { message = "Could not load the page." }) { StatusCode = 500 };
            }
            AttributeTemplatesList = _templateService.GetAttributeTemplateInfoList(userId).Result;
            Categories = _templateService.GetPartCategories(userId).Result;  
            PartNumbers = _partService.GetPartNames(userId).Result;
            Inventory = response.Result;
        }
        public IActionResult OnPostCreatePart([FromBody]CreatePartRequest request) 
        {
            if (!ModelState.IsValid) 
            {
                _appLogger.LogModelError(nameof(OnPostCreatePart), ModelState);

                return BadRequest(new
                {
                    success = false,
                    message = _localizer["ErrorModelStateError"],
                });
            }
            // Example: Use the Path to find the category
            var path = request.PathToCategory;
            if (path == null || !path.Any())
            {
                return BadRequest(new { message = "Invalid category path" });
            }

            var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
            var response = _partService.CreatePart(request, UserId);
            return new JsonResult(new { message = response.Message }) { StatusCode = 200 };                      
        }

        public IActionResult OnPostUpdatePart([FromBody] UpdatePartRequest request)
        {
            if (!ModelState.IsValid)
            {
                _appLogger.LogModelError(nameof(OnPostCreatePart), ModelState);

                return BadRequest(new
                {
                    success = false,
                    message = _localizer["ErrorModelStateError"],
                });
            }
            var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            var response = _partService.UpdatePart(request, UserId);
            return new JsonResult(new { message = response.Message }) { StatusCode = 200 };

        }

        public IActionResult OnPostRetrievePart(string partNumber) 
        {
            try
            {
                var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                var response = _partService.GetPartByName(partNumber,userId);
                if (response is null)
                    return new OkResult();
                return new JsonResult(response.Result);

            }
            catch (Exception)
            {

                throw;
            }

        }
        
        public async Task OnPostFetchDigikey(string digiKeyPartNumber) 
        {
			var result = await _digikeyService.GetProductDetailsAsync(digiKeyPartNumber);            
		}

        public IActionResult OnPostUpdatePartAttribute([FromBody] UpdateAttributeRequest updatedRequest) 
        {
            if (ModelState.IsValid) 
            {
                var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                var updateResult = _partService.UpdatePartAttributes(updatedRequest, userId);
                
                if(updatedRequest is null)
                {
                    //TODO: rephrase the message.
                    return new JsonResult(new { message = "Could not find the part to be updated" }) { StatusCode = 200 };
                }

                return new JsonResult(new { message = "Part attributes updated successfully" }) { StatusCode = 200 };
            }
            // If ModelState is not valid, return validation errors
            var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return new JsonResult(new { errors = errorList }) { StatusCode = 400 };

        }

        public IActionResult OnPostArrangePart([FromBody] ArrangePartRequest arrangeRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                    _partService.ArrangePartRequest(arrangeRequest, UserId);

                    return new OkResult();
                }
                catch (Exception e)
                {

                    return StatusCode(500, new { message = "An error occurred while processing your request." });
                }

            }

            return BadRequest(new { message = "Invalid request." });
        }

        public IActionResult OnPostDeleteArrangedItem([FromBody] RemoveArrangePartRequest removeRequest) 
        {
            if (ModelState.IsValid)
            {
                var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                var deleteResult = _partService.RemoveArrangedPartRequest(removeRequest, userId);

                if (removeRequest is null)
                {
                    //TODO: rephrase the message.
                    return new JsonResult(new { message = "Could not find the part to be updated" }) { StatusCode = 200 };
                }

                return new JsonResult(new { message = "Part attributes updated successfully" }) { StatusCode = 200 };
            }

            return new OkResult();
        }

        public IActionResult OnPostUsePart(string partName)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                    var response = _partService.UsePart(partName, userId);
                    return new JsonResult(response);
                }
                catch (Exception e)
                {
                    //TODO: check this. should should if failed message is shown or not.
                    return new JsonResult(e.Message);
                }
            }            
            return new OkResult();
        }

        public IActionResult OnPostAdjustQuantity([FromBody] AdjustQuantityRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                    _partService.TryAdjustPartQuantity(request, userId);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return new OkResult();
            }

            return BadRequest(ModelState);
        } 

    }
}
