using Amazon.Runtime.Internal;
using Azure.Core;
using Bintainer.WebApp.Data;
using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Bintainer.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class UpdateAttributeTableRequest
    {
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public string PartName { get; set; } = string.Empty;
    }
    public class ArrangePartRequest 
    {
        public string? PartName { get; set; }
        public int SectionId { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public int Subspace { get; set; }
        public string? Label { get; set; }
        public string? Group { get; set; }
        public List<string>? Subspaces { get; set; }
        public bool IsFilled { get; set; }
    }

    public class PartUsageResponse 
    {
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public string? Section { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public int? Subspace { get; set; }
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public bool? IsFilled { get; set; }
    }

    public class PartModel : PageModel
    {
        public List<PartPackage> Packages { get; set; } = new List<PartPackage>();

        public List<PartCategory> Category { get; set; } = new List<PartCategory>();
        public List<Part> Part { get; set; } = new List<Part>();
        
        public List<PartGroup> Group { get; set; } = new List<PartGroup>();
        public Dictionary<int, string> AttributeTemplatesTable { get; set; } = new Dictionary<int, string>();

        public List<InventorySection> Sections { get; set; } = new();
        public Inventory Inventory { get; set; } = new();

        BintainerDbContext _dbcontext;
        DigikeyService _digikeyService;
        public PartModel(BintainerDbContext dbContext, DigikeyService digikeyServices)
        {
			_dbcontext = dbContext;
            _digikeyService = digikeyServices;
            Packages = _dbcontext.PartPackages.ToList();
            Category = _dbcontext.PartCategories.ToList();
            Group = _dbcontext.PartGroups.ToList();
            Part = _dbcontext.Parts.Include(p => p.Package).ToList();
            Inventory = _dbcontext.Inventories.Include(i => i.InventorySections).FirstOrDefault();
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
			LoadTemplate(userId);            
		}

        public IActionResult OnPostSearchForPart(string partNumber) 
        {
            var _part = _dbcontext.Parts.Include(p => p.AttributeTemplates).FirstOrDefault(p => p.Name == partNumber);
            if (_part is null)
                return new OkResult();
            var attributes = _dbcontext.PartAttributes.Where(a => a.TemplateId == _part.AttributeTemplates.FirstOrDefault().Id)
                .Select(a => new
                {
                    Name = a.Name != null ? a.Name.Trim() : null, // Trim if Name is not null
                    Value = a.Value != null ? a.Value.Trim() : null // Trim if Value is not null
                }) // Select only Name and Value
                .ToList();
            return new JsonResult(attributes);

        }
        public async Task OnPostFetchDigikey(string digiKeyPartNumber) 
        {
			var result = await _digikeyService.GetProductDetailsAsync(digiKeyPartNumber);            
		}

        public IActionResult OnPostCreatePart([FromBody]CreatePartRequestModel request) 
        {
            if (ModelState.IsValid) 
            {                
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                if (_dbcontext.Parts.Any(p => p.Name == request.PartName && p.Supplier == request.Supplier && p.UserId == UserId)) 
                {
                    return new JsonResult(new { message = "The part already exists" }) { StatusCode = 200 };
                }
                Part _part= new Part();
                _part.Name = request.PartName.Trim();
                _part.Description = request.Description.Trim();
                _part.CategoryId = request.CategoryId;
                _part.UserId = UserId;
                _part.Supplier = request.Supplier.Trim();
                //TODO: links should be fetched here.

                string packageName = string.IsNullOrEmpty(request.Package) ? "undefined" : request.Package.Trim();                
                
                var package = _dbcontext.PartPackages.FirstOrDefault(p => p.Name == packageName && p.UserId == UserId);

                if(package is null) 
                {
                    package = new PartPackage() { Name = packageName, UserId = UserId };
                    //var result = _dbcontext.PartPackages.Add(new PartPackage() { Name = packageName, UserId = UserId });
                    //_dbcontext.SaveChanges();
                    //package = result.Entity;
                }

                _part.Package= package;
                //_part.PackageId = package.Id;
                                
                List<PartAttribute> attributes = new List<PartAttribute>();
                var attributeTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.Id == request.AttributeTemplateId);
                if(attributeTemplate is null)
                {
                    var defaultTemplate = _dbcontext.PartAttributeTemplates.FirstOrDefault(t => t.TemplateName == _part.Name && t.UserId == UserId);
                    if (defaultTemplate is null)
                    {
                        // The part is fetched from external source (e.g., DigiKey)
                        attributeTemplate = new PartAttributeTemplate() { TemplateName = _part.Name, UserId = UserId };
                        _dbcontext.PartAttributeTemplates.Add(attributeTemplate);
                        _dbcontext.SaveChanges(true);
                    }
                    else
                    {
                        attributeTemplate = defaultTemplate;
                    }
                }
                foreach (var item in request.Attributes)
                {
                    attributes.Add(new PartAttribute() { Name = item.Key, Value = item.Value, Template = attributeTemplate });                                   
                }
                if (attributes.Any()) 
                {
                    _dbcontext.PartAttributes.AddRange(attributes);
                    _dbcontext.SaveChanges(true);
                }
                
                PartAttributeTemplate partAttributeTemplate = null;
                try
                {
                    _part.AttributeTemplates.Add(attributeTemplate);
                    _dbcontext.Parts.Add(_part);
                    _dbcontext.SaveChanges(true);                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(  ex.Message);
                    throw;
                }
                if (!string.IsNullOrEmpty(request.OrderNumber))                 
                {
                    Order _order = _dbcontext.Orders.Where(o => o.OrderNumber == request.OrderNumber && o.UserId == UserId).FirstOrDefault();                    
                }
            }
            return new JsonResult(new { message = "New part created." }) { StatusCode = 200 };                      
        }

        public IActionResult OnPostUpdatePartAttribute([FromBody] UpdateAttributeTableRequest updatedRequest) 
        {
            if (ModelState.IsValid) 
            {
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                
                var part = _dbcontext.Parts
                    .Include(p=> p.AttributeTemplates)
                    .Where(p=>p.Name== updatedRequest.PartName && p.UserId == UserId)
                    .FirstOrDefault();

                if (part is null)
                {
                    return new JsonResult(new { message = "The part already exists" }) { StatusCode = 200 };
                }
               
                var templateId = part.AttributeTemplates.FirstOrDefault()?.Id;
                if (templateId is null)
                {
                    return new JsonResult(new { message = "No attribute template found" }) { StatusCode = 400 };
                }

                var partAttributes = 
                    _dbcontext.PartAttributes
                    .Where(a => a.TemplateId == templateId)                   
                    .ToList();

                var updatedAttributeKeys = updatedRequest.Attributes.Keys.ToList();
                
                // 1. DELETE attributes that do not exist in the updated request
                var attributesToDelete = partAttributes
                                          .Where(pa => !updatedAttributeKeys.Contains(pa.Name?.Trim()))
                                          .ToList();
                if (attributesToDelete.Any())
                {
                    _dbcontext.PartAttributes.RemoveRange(attributesToDelete);
                }

                // 2. UPDATE existing attributes or add new ones
                foreach (var attribute in updatedRequest.Attributes)
                {
                    var existingAttribute = partAttributes.FirstOrDefault(pa => pa.Name.Trim() == attribute.Key.Trim());

                    if (existingAttribute != null)
                    {
                        // Update existing attribute value
                        existingAttribute.Value = attribute.Value;
                    }
                    else
                    {
                        // Add new attribute
                        var newAttribute = new PartAttribute
                        {
                            Name = attribute.Key.Trim(),
                            Value = attribute.Value.Trim(),
                            TemplateId = templateId.Value
                        };
                        _dbcontext.PartAttributes.Add(newAttribute);
                    }
                }

                // 3. Save changes to the database
                _dbcontext.SaveChanges();

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
                //try
                //{
                //    var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                //    InventorySection? inventorySection = _dbcontext.InventorySections.Where(i => i.Inventory.UserId == UserId && i.Id == arrangeRequest.SectionId)
                //                                                                     .Include(i=>i.Bins)
                //                                                                     .ThenInclude(b=>b.Parts)
                //                                                                     .ThenInclude(b=>b.Groups)
                //                                                                     .FirstOrDefault();
                //    if (inventorySection == null)
                //    {
                //        // Return an appropriate response, such as NotFound
                //        return NotFound(new { message = "Inventory section not found." });
                //    }

                //    Bin? bin = inventorySection?.Bins.FirstOrDefault(b => b.CoordinateX == arrangeRequest.CoordinateX &&
                //                                                          b.CoordinateY == arrangeRequest.CoordinateY &&
                //                                                          b.Parts.Any(p => p.Id == arrangeRequest.PartId));

                //    if (bin is not null) 
                //    {
                //        Part? part = bin.Parts.Where(p => p.Id == arrangeRequest.PartId).FirstOrDefault();
                //        if (!part.Groups.Any(g => g.Name.Trim() == arrangeRequest.Group))
                //            part.Groups.Add(new PartGroup() { Name = arrangeRequest.Group, UserId = UserId });
                //        if (!bin.BinSubspaces.Any(s => s.Id == arrangeRequest.Subspace))
                //            bin.BinSubspaces.Add(new BinSubspace() { SubspaceIndex = arrangeRequest.Subspace, Label = arrangeRequest.Label });
                //        else
                //        {
                //            var registeredSubspace = bin.BinSubspaces.Where(s => s.SubspaceIndex == arrangeRequest.Subspace).FirstOrDefault();
                //            registeredSubspace.Label= arrangeRequest.Label;
                //        }
                //        bin.IsFilled = arrangeRequest.IsFilled;
                //        _dbcontext.Parts.Update(part);
                //        _dbcontext.Bins.Update(bin);
                //        _dbcontext.SaveChanges(true);
                //    }
                //    else 
                //    {
                //        Bin newBin = new Bin()
                //        {
                //            CoordinateX = arrangeRequest.CoordinateX,
                //            CoordinateY = arrangeRequest.CoordinateY,
                //            SectionId = arrangeRequest.SectionId,
                //            IsFilled = arrangeRequest.IsFilled
                //        };
                //        BinSubspace newSubspace = new BinSubspace()
                //        {
                //            SubspaceIndex = arrangeRequest.Subspace,
                //            Label = arrangeRequest.Label,
                //        };                        
                //        PartGroup newPartGroup = new PartGroup()
                //        {
                //            UserId = UserId,
                //            Name = arrangeRequest.Group
                //        };
                //        Part? part = _dbcontext.Parts.Where(p => p.Id == arrangeRequest.PartId)
                //                                     .Include(p => p.Groups)
                //                                     .Include(p => p.Category)
                //                                     .FirstOrDefault();
                //        if (part == null)
                //        {
                //            return NotFound(new { message = "Part not found." });
                //        }
                //        if (!part.Groups.Any(g=>g.Name.Trim()==arrangeRequest.Group))
                //        {
                //            part.Groups.Add(newPartGroup);
                //        }
                        
                //        newBin.Parts.Add(part);
                //        newBin.BinSubspaces.Add(newSubspace);
                //        inventorySection.Bins.Add(newBin);
                //        _dbcontext.SaveChanges(true);

                //    }
                //    return new OkResult();
                //}
                //catch (Exception e)
                //{

                //    return StatusCode(500, new { message = "An error occurred while processing your request." });
                //}

            }

            return BadRequest(new { message = "Invalid request." });
        }

        public IActionResult OnPostUsePart(string partName) 
        {
            Part? part = _dbcontext.Parts.Include(p=> p.Bins)
                                         .ThenInclude(b=>b.BinSubspaces)
                                         .Include(p=>p.OrderPartAssociations)
                                         .Where(p => p.Name.Contains(partName)).FirstOrDefault();
            if(part is not null) 
            {
                int? quantity = part.OrderPartAssociations.Where(op => op.PartId == part.Id).Select(op => op.Qunatity).Sum();

                List<PartUsageResponse> results = new List<PartUsageResponse>();
                
                foreach (Bin bin in part.Bins) 
                {
                    if (bin.IsFilled == true) 
                    {
                        results.Add(new PartUsageResponse
                        {
                            PartId = part.Id,
                            PartName = part.Name,
                            CoordinateX = bin.CoordinateX,
                            CoordinateY = bin.CoordinateY,
                            Subspace = bin.BinSubspaces.FirstOrDefault()?.SubspaceIndex,
                            Label = bin.BinSubspaces.FirstOrDefault()?.Label,
                            IsFilled = true
                        });
                    }
                    else 
                    {
                        foreach (BinSubspace subSpace in bin.BinSubspaces)
                        {

                        }

                    }

                }


            }
            return new OkResult();
        }

        private void LoadTemplate(string userId)
        {
			foreach (var item in _dbcontext.PartAttributeTemplates.Where(p=>p.UserId==userId))
			{
				if (item.TemplateName != null)
					AttributeTemplatesTable[item.Id] = item.TemplateName;
			}
		}
    }
}
