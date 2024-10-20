using Amazon.Runtime.Internal;
using Amazon.SimpleEmail.Model.Internal.MarshallTransformations;
using Azure.Core;
using Bintainer.WebApp.Data;
using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Bintainer.WebApp.Services;
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
        public string? Label { get; set; }
        public string? Group { get; set; }
        public Dictionary<int,int>? SubspaceQuantities { get; set; }
        public bool IsFilled { get; set; }
        public int? FillAllQuantity { get; set; }
    }

    public class AdjustQuantityRequest 
    {
        public int QuantityUsed { get; set; }
        public int BinId { get; set; }
        public string? PartName { get; set; }
        public int Quantity { get; set; }
        public string? SubspaceIndices { get; set; }
    }

    public class PartUsageResponse 
    {
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public string? Section { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public string? SubspaceIndices { get; set; }
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public int BinId { get; set; }
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
                try
                {
                    var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                    InventorySection? inventorySection = _dbcontext.InventorySections.Where(i => i.Inventory.UserId == UserId && i.Id == arrangeRequest.SectionId)
                                                                                     .FirstOrDefault();
                    Part? part = _dbcontext.Parts.Where(p => p.Name.Trim() == arrangeRequest.PartName).FirstOrDefault();

                    if (inventorySection == null)
                    {
                        return NotFound(new { message = "Inventory section not found." });
                    }

                    Bin? bin = inventorySection?.Bins.FirstOrDefault(b => b.CoordinateX == arrangeRequest.CoordinateX &&
                                                                         b.CoordinateY == arrangeRequest.CoordinateY);

                    if (bin is not null && part is not null)
                    {
                        ProcessArrangePartRequest(bin, part, arrangeRequest, UserId);
                    }

                    if (bin is null && part is not null)
                    {
                        bin = CreateBin(arrangeRequest.CoordinateX, arrangeRequest.CoordinateY, inventorySection);
                        ProcessArrangePartRequest(bin, part, arrangeRequest, UserId);
                    }

                    if (part is null)
                    {
                        // TODO: log
                        throw new Exception("The part you are trying to arrange is not valid!");
                    }

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
                Part? part = GetPartByName(partName);

                if (part is not null)
                {
                    var response = GetPartUsageResponse(part);
                    return new JsonResult(response);
                }
            }            
            return new OkResult();
        }

        public IActionResult OnPostAdjustQuantity([FromBody] AdjustQuantityRequest request)
        {
            if (ModelState.IsValid)
            {
                if (request.QuantityUsed > request.Quantity)
                {
                    return BadRequest(new { message = "The number of used items should be less than available ones." });
                }

                List<int> subSpaces = ParseSubspaceIndices(request.SubspaceIndices);
                int takeOut = request.QuantityUsed;

                // Get the part by its name
                Part? part = GetPartByName(request.PartName);

                if (part == null)
                {
                    return NotFound(new { message = "Part not found." });
                }

                // Fetch relevant part-bin associations
                var associations = part.PartBinAssociations
                                       .Where(a => a.BinId == request.BinId && subSpaces.Contains(a.Subspace.SubspaceIndex!.Value))
                                       .OrderBy(a => a.SubspaceId) // Ensure a consistent order for processing
                                       .ToList();

                foreach (var assoc in associations)
                {
                    if (takeOut <= 0) break; 

                    if (assoc.Quantity >= takeOut)
                    {
                        assoc.Quantity -= takeOut;
                        takeOut = 0; 
                    }
                    else
                    {
                        takeOut -= assoc.Quantity;
                        assoc.Quantity = 0;
                    }
                }

                _dbcontext.PartBinAssociations.UpdateRange(associations);
                _dbcontext.SaveChanges();

                return new OkResult();
            }

            return BadRequest(ModelState);
        }

        private List<PartUsageResponse> GetPartUsageResponse(Part part) 
        {
            var response = part.PartBinAssociations
                               .GroupBy(a => a.BinId)
                               .Select(g => new PartUsageResponse
                               {
                                 BinId = g.Key,
                                 Label = g.First().Bin.BinSubspaces.FirstOrDefault()?.Label?.Trim(),                                
                                 PartName = g.First().Part.Name?.Trim(),
                                 CoordinateX = g.First().Bin.CoordinateX,
                                 CoordinateY = g.First().Bin.CoordinateY,
                                 Section = g.First().Bin.Section?.SectionName?.Trim(),
                                 Quantity = g.Sum(a => a.Quantity),
                                 SubspaceIndices = string.Join(", ", g.First().Bin.BinSubspaces.Select(s => s.SubspaceIndex))
                               })
                               .ToList();
            return response;
        }

        private Part? GetPartByName(string partName) 
        {
            Part? part = _dbcontext.Parts.Include(p => p.PartBinAssociations)
                                         .ThenInclude(b=>b.Bin)
                                         .ThenInclude(b => b.BinSubspaces)
                                         .Include(p => p.OrderPartAssociations)
                                         .Where(p => p.Name.Contains(partName))
                                         .FirstOrDefault();
            return part;        
        }

        private void LoadTemplate(string userId)
        {
			foreach (var item in _dbcontext.PartAttributeTemplates.Where(p=>p.UserId==userId))
			{
				if (item.TemplateName != null)
					AttributeTemplatesTable[item.Id] = item.TemplateName;
			}
		}

        private bool TryInsertPartIntoBin(List<int> targetIndices,string label,ref Part part, ref Bin bin)
        {
            foreach (int subSpaceIndex in targetIndices)
            {
                var targetSubspace = bin.BinSubspaces.Where(s => s.SubspaceIndex == subSpaceIndex).FirstOrDefault();
                if(targetSubspace is null)
                {
                    // the bin's subspace is not already used
                    targetSubspace = new BinSubspace()
                    {
                        SubspaceIndex = subSpaceIndex,
                        Label = label
                    };
                    bin.BinSubspaces.Add(targetSubspace);                    
                }
                else 
                {
                    int partId = part.Id;
                    int binId = bin.Id;
                    int subspaceId = targetSubspace.Id;                    
                    if (_dbcontext.PartBinAssociations.Where(a => a.PartId == partId && a.SubspaceId == subspaceId && a.BinId== binId).Any()) 
                    {
                        // the incomming part is same as already on
                    }
                    else 
                    {

                    }
                }
            }
            return true;

        }

        private bool IsSubspaceAvailableForPart(in Bin bin, in Part part, List<int> subSpaceIndices)
        {
            var existingSubspaces = bin.BinSubspaces.Where(s => subSpaceIndices.Contains(s.SubspaceIndex.Value)).ToList();
            if (existingSubspaces is null || existingSubspaces.Count() == 0)
                return true;

            int binId = bin.Id;
            int partId = part.Id;
            return existingSubspaces.TrueForAll(s => s.PartBinAssociations.Where(a => a.BinId == binId && a.PartId == partId).FirstOrDefault() != null);
        }  

        private BinSubspace MakeSubspaceInsideBin(Bin bin,int subspaceIndex,string? label) 
        {
            BinSubspace? subspace = bin.BinSubspaces.Where(b => b.SubspaceIndex == subspaceIndex).FirstOrDefault();
            if (subspace is null)
            {
                subspace = new BinSubspace()
                {
                    BinId = bin.Id,
                    SubspaceIndex = subspaceIndex,
                    Label = label
                };
                bin.BinSubspaces.Add(subspace);
                subspace.Bin = bin;
            }
            return subspace;
        }        
        private void UpdatePartQuantityInsideSubspace(BinSubspace subspace, int partId, int partQuantity) 
        {
            var assoc = subspace.PartBinAssociations.Where(a => a.PartId == partId).FirstOrDefault();
            if (assoc is not null) 
            {
                int updatedValue = assoc.Quantity + partQuantity;
                if (updatedValue < 0) { updatedValue = 0; }
                assoc.Quantity = updatedValue;
                _dbcontext.PartBinAssociations.Update(assoc);
                _dbcontext.SaveChanges();
            }

            return;
        }
        private Bin InsertPartIntoBin(Bin bin,in Part part, Dictionary<int,int>? subspaceQuantity,string label) 
        {
            int binId = bin.Id;
            int partId = part.Id;
            if (!IsSubspaceAvailableForPart(bin, part, subspaceQuantity.Keys.ToList())) 
            {
                // TODO : log event
                throw new Exception("Can not insert element into subspace");

            }
            foreach (var subspaceIndex in subspaceQuantity.Keys)
            {
                BinSubspace subspace = MakeSubspaceInsideBin(bin, subspaceIndex, label);
                InsertPartIntoSubspace(subspace, part);
                UpdatePartQuantityInsideSubspace(subspace, partId, subspaceQuantity[subspaceIndex]);                                
            }
            return bin;            
        }

        private BinSubspace InsertPartIntoSubspace(BinSubspace subSpace,Part part) 
        {
            if(subSpace.Bin is null)
            {
                // TODO: log
                throw new Exception("the bin can not be null while trying to insert a part inside the subspace");
            }
            var assoc = new PartBinAssociation 
            {
                PartId= part.Id,
                BinId = subSpace.Bin!.Id,
                SubspaceId= subSpace.Id
            };
            subSpace.PartBinAssociations.Add(assoc);

            return subSpace;
        }
        public List<int> ParseSubspaceIndices(string commaSeparatedIndices)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedIndices))
            {
                return new List<int>(); 
            }

            return commaSeparatedIndices
                .Split(',')
                .Select(int.Parse)
                .ToList();
        }

        private Dictionary<int, int> DistributeQuantityAcrossSubspaces(in Bin bin, int totalQuantity)
        {
            int subspaceCount = bin.Section.SubspaceCount.Value;
            int[] quantities = DividePartsEvenly(totalQuantity, subspaceCount);
            Dictionary<int, int> subspaceQuantity = new Dictionary<int, int>();

            for (int i = 1; i <= quantities.Length; i++)
            {
                subspaceQuantity.Add(i, quantities[i-1]);
            }

            return subspaceQuantity;
        }
        private Bin CreateBin(int coordinateX, int coordinateY,InventorySection section) 
        {
            Bin bin = new  Bin() { CoordinateX = coordinateX, CoordinateY = coordinateY, SectionId = section.Id };
            SaveBin(bin);
            section.Bins.Add(bin);
            return bin;
        }
        private void SaveBin(Bin bin) 
        {
            _dbcontext.Bins.Add(bin);
            _dbcontext.SaveChanges();
            
        }
        private void UpdateBin(Bin bin) 
        {
            _dbcontext.Bins.Update(bin);
            _dbcontext.SaveChanges(true);
        }
        private int[] DividePartsEvenly(int totalParts, int totalSubspaces)
        {
            int[] result = new int[totalSubspaces];
            int baseDivision = totalParts / totalSubspaces;
            int remainder = totalParts % totalSubspaces;
            
            for (int i = 0; i < totalSubspaces; i++)
            {
                result[i] = baseDivision;
            }

            for (int i = 0; i < remainder; i++)
            {
                result[i]++;
            }

            return result;
        }
        
        private void ProcessArrangePartRequest(Bin bin, Part part, ArrangePartRequest arrangeRequest, string UserId) 
        {
            Dictionary<int, int>? subspaceQuantity = new Dictionary<int, int>();
            if (arrangeRequest.IsFilled)
            {
                subspaceQuantity = DistributeQuantityAcrossSubspaces(bin, arrangeRequest.FillAllQuantity.Value);
            }
            else
            {
                subspaceQuantity = arrangeRequest.SubspaceQuantities;
            }

            var updatedBin = InsertPartIntoBin(bin, part, subspaceQuantity, arrangeRequest.Label);
            UpdateBin(updatedBin);

            if (arrangeRequest.Group is not null)
            {
                var partGroup = AddPartIntoGroup(part, arrangeRequest.Group, UserId);
                part.Groups.Add(partGroup);
            }

            _dbcontext.Parts.Update(part);
            _dbcontext.Bins.Update(bin);
            _dbcontext.SaveChanges(true);
        }
        private PartGroup? GetPartGroup(string name, string userId) 
        {
            return _dbcontext.PartGroups.Where(g => g.Name == name && g.UserId== userId).FirstOrDefault();
        }

        public PartGroup AddPartIntoGroup(Part part, string groupName, string userId)
        {
           var group = GetPartGroup(groupName, userId);  
            
           if(group is not null && group.Parts.Contains(part)) 
            {
                return group;
            }
           if(group is not null && !group.Parts.Contains(part)) 
            {
                group.Parts.Add(part);
            }
           if(group is null) 
            {
                group = new PartGroup() {Name = groupName, UserId = userId };
                group.Parts.Add(part);
            }                     
           
           return group;
        }

    }
}
