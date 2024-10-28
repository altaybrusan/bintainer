using Amazon.Runtime.Internal;
using Amazon.SimpleEmail.Model.Internal.MarshallTransformations;
using Azure;
using Azure.Core;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Service;
using Bintainer.Service.Interface;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Runtime;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();

        public List<PartCategory> Category { get; set; } = new List<PartCategory>();
        public List<Part> Part { get; set; } = new List<Part>();
        
        public List<PartGroup> Group { get; set; } = new List<PartGroup>();
        public Dictionary<int,string?> AttributeTemplatesTable { get; set; } = new Dictionary<int, string>();

        public List<InventorySection> Sections { get; set; } = new();
        public Inventory Inventory { get; set; } = new();

        DigikeyService _digikeyService;
        ITemplateService _templateService;
        IPartService _partService;
        public PartModel(ITemplateService templateService, IPartService partService, DigikeyService digikeyServices)
        {
            _templateService = templateService;
            _partService = partService;

            _digikeyService = digikeyServices;
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
            
            AttributeTemplatesTable = _templateService.GetTemplateByUserId(userId).Result;            
		}

        public IActionResult OnPostRetrievePartAttributeDetails(string partNumber) 
        {
            try
            {
                var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                var results = _partService.GetPartByName(partNumber,userId);
                if (results is null)
                    return new OkResult();

                return new JsonResult(results);

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

        public IActionResult OnPostCreatePart([FromBody]CreatePartRequest request) 
        {
            if (ModelState.IsValid) 
            {                
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                _partService.CreatePartForUser(request, UserId);               
            }
            return new JsonResult(new { message = "New part created." }) { StatusCode = 200 };                      
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
