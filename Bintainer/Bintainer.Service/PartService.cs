using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Response;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Extention;
using Bintainer.Service.Interface;
using Microsoft.Extensions.Localization;


namespace Bintainer.Service
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _partRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IBinService _binService;
        private IStringLocalizer _stringLocalizer;
        public PartService(IPartRepository partRepository,
                           ITemplateRepository templateRepository,
                           IInventoryRepository inventoryRepository,
                           IInventoryService inventoryService,
                           IBinService binService,
                           IStringLocalizer stringLocalizer)
        {
            _partRepository = partRepository;
            _templateRepository = templateRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryService = inventoryService;
            _binService = binService;
            _stringLocalizer = stringLocalizer;
        }

        public Response<Part?> GetPartByName(string partName, string userId) 
        {
            try
            {
                var result = _partRepository.GetPartByName(partName, userId);
                return new Response<Part?>
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                                
            }

            return new Response<Part?>
            {
                IsSuccess = false,
                Result = null,
                Message = _stringLocalizer["ErrorFailedPartLoading"]

            };

        }

        public List<PartAttributeViewModel>? MapPartAttributesToViewModel(string partName,string userId) 
        {
            Part? part = _partRepository.GetPartByName(partName, userId);
            if (part == null) 
            {
                return null;
            }
            return part.AttributeTemplates?.FirstOrDefault()?.PartAttributes?
                                    .Select(attr => new PartAttributeViewModel() 
                                    { 
                                        Name = attr.Name != null ? attr.Name.Trim() : null, 
                                        Value = attr.Value != null ? attr.Value.Trim() : null
                                    }).ToList();
        }

        public Part? CreatePartForUser(CreatePartRequest request, string userId) 
        {
            PartFilterCriteria filter= new() 
            { 
                Name = request.PartName,
                Supplier = request.Supplier,
                UserId = userId
            };
            var parts = _partRepository.GetPartsByCriteria(filter);
            if(parts is not null) 
            {
                //TODO: log event
                throw new Exception("The part already exists");
            }

            Part part = new Part();
            part.Name = request.PartName!.Trim();
            part.Description = request.Description!.Trim();
            part.CategoryId = request.CategoryId;
            part.UserId = userId;
            part.Supplier = request.Supplier!.Trim();

            //TODO: links should be fetched here.
            //TODO: check if undefined is acceptable or not
            string packageName = string.IsNullOrEmpty(request.Package) ? "undefined" : request.Package.Trim();
            var package = _partRepository.GetOrCreatePartPackage(packageName, userId);
            part.Package = package;
                       
            var attributeTemplate = _templateRepository.GetAttributeTemplateById(request.AttributeTemplateId);            
            if (attributeTemplate is null)
            {
                var defaultTemplate = _templateRepository.GetAttributeTemplateByName(request.PartName, userId);
                if (defaultTemplate is null)
                {
                    // The part is fetched from external source (e.g., DigiKey)
                    attributeTemplate = _templateRepository.CreateAttributeTemplateByName(request.PartName, userId);
                }
                else
                {
                    attributeTemplate = defaultTemplate;
                }
            }
           
            var attributes = request.Attributes.ToPartAttributeList(attributeTemplate);
           _templateRepository.SaveAttributes(attributes);

            part.AttributeTemplates.Add(attributeTemplate);
            _partRepository.UpdatePart(part);
            
            PartAttributeTemplate partAttributeTemplate = null;

            //TODO: complete the order in new part registration form.
            //if (!string.IsNullOrEmpty(request.OrderNumber))
            //{
            //    Order _order = _dbcontext.Orders.Where(o => o.OrderNumber == request.OrderNumber && o.UserId == UserId).FirstOrDefault();
            //}

            return part;

        }

        public List<PartAttribute>? UpdatePartAttributes(UpdateAttributeRequest request, string userId)
        {
            // Fetch the part by name and userId
            var part = _partRepository.GetPartByName(request.PartName, userId);
            if (part is null)
                return null;

            // Get existing part attributes from the first AttributeTemplate
            var existingAttributes = part.AttributeTemplates.FirstOrDefault()?.PartAttributes.ToList();
            if (existingAttributes == null)
                existingAttributes = new List<PartAttribute>();

            // Get incoming attribute keys and values from the request
            var incomingAttributes = request.Attributes;

            // 1. DELETE attributes that are not in the incoming request
            existingAttributes.RemoveAll(existingAttribute => !incomingAttributes.ContainsKey(existingAttribute.Name?.Trim()));

            // 2. UPDATE existing attributes or add new ones
            foreach (var incomingAttribute in incomingAttributes)
            {
                var existingAttribute = existingAttributes.FirstOrDefault(a => a.Name?.Trim() == incomingAttribute.Key.Trim());

                if (existingAttribute != null)
                {
                    // If attribute exists, update its value
                    existingAttribute.Value = incomingAttribute.Value.Trim();
                }
                else
                {
                    // If attribute doesn't exist, add it as a new attribute
                    existingAttributes.Add(new PartAttribute
                    {
                        Name = incomingAttribute.Key.Trim(),
                        Value = incomingAttribute.Value.Trim(),
                        TemplateId = part.AttributeTemplates.FirstOrDefault()?.Id ?? 0 // Ensure TemplateId is set correctly
                    });
                }
            }

            // Save changes to the database (pseudo-code)
            _partRepository.UpdatePart(part);

            // Return the updated list of part attributes
            return existingAttributes;
        }

        public void ArrangePartRequest(ArrangePartRequest arrangeRequest, string userId)
        {
            InventorySection? inventorySection = _inventoryRepository.GetSection(userId, arrangeRequest.SectionId);
            Part? part = _partRepository.GetPartByName(arrangeRequest.PartName, userId);

            if (inventorySection == null)
            {
                //TODO: update return type
                throw new Exception("Inventory section not found.");
            }

            Bin? bin = _inventoryService.GetBinFrom(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY);

            if (bin is not null && part is not null)
            {
                ProcessArrangePartRequest(bin, part, arrangeRequest, userId);
            }

            if (bin is null && part is not null)
            {
                bin = _inventoryService.CreateBin(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY);
                ProcessArrangePartRequest(bin, part, arrangeRequest, userId);
            }

            if (part is null)
            {
                // TODO: log
                throw new Exception("The part you are trying to arrange is not valid!");
            }
            return;
        }

        private void ProcessArrangePartRequest(Bin bin, Part part, ArrangePartRequest arrangeRequest, string UserId)
        {
            Dictionary<int, int>? subspaceQuantity = new Dictionary<int, int>();
            if (arrangeRequest.IsFilled)
            {               
                subspaceQuantity = _binService.DistributeQuantityAcrossSubspaces(bin, arrangeRequest.FillAllQuantity.Value);
            }
            else
            {
                subspaceQuantity = arrangeRequest.SubspaceQuantities;
            }

            var updatedBin = InsertPartIntoBin(bin, part, subspaceQuantity, arrangeRequest.Label);
            _binService.UpdateBin(updatedBin);

            if (arrangeRequest.Group is not null)
            {
                var partGroup = AddPartIntoGroup(part, arrangeRequest.Group, UserId);
                part.Groups.Add(partGroup);
            }
            _partRepository.UpdatePart(part);
            _binService.UpdateBin(bin);
        }

        public PartGroup AddPartIntoGroup(Part part, string groupName, string userId)
        {
            var group = _partRepository.GetPartGroup(groupName, userId);

            if (group is not null && group.Parts.Contains(part))
            {
                return group;
            }
            if (group is not null && !group.Parts.Contains(part))
            {
                group.Parts.Add(part);
            }
            if (group is null)
            {
                group = new PartGroup() { Name = groupName, UserId = userId };
                group.Parts.Add(part);
            }

            return group;
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

        public Response<List<PartUsageResponse>> UsePart(string partName, string userId)
        {
            Part? part = _partRepository.GetPartByName(partName, userId);
            if (part == null)
            {
               var result = GetPartUsageResponse(part);
                return new Response<List<PartUsageResponse>>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            return new Response<List<PartUsageResponse>>()
            {
                IsSuccess = false,
                Result = null,
                Message = ""
            };
            throw new NotImplementedException();
        }


        public List<PartBinAssociation>? TryAdjustPartQuantity(AdjustQuantityRequest request,string userId) 
        {
            if (request.QuantityUsed > request.Quantity)
            {
                throw new Exception("The number of used items should be less than available ones.");                
            }

            List<int> subSpaces = ParseSubspaceIndices(request.SubspaceIndices);
            int takeOut = request.QuantityUsed;

            // Get the part by its name
            Part? part = _partRepository.GetPartByName(request.PartName,userId);

            if (part == null)
            {
                throw new Exception("Part not found.");
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

            var result = _partRepository.UpdatePartBinassociations(associations);
            return result;
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

        private BinSubspace InsertPartIntoSubspace(BinSubspace subSpace, Part part)
        {
            if (subSpace.Bin is null)
            {
                // TODO: log
                throw new Exception("the bin can not be null while trying to insert a part inside the subspace");
            }
            var assoc = new PartBinAssociation
            {
                PartId = part.Id,
                BinId = subSpace.Bin!.Id,
                SubspaceId = subSpace.Id
            };
            subSpace.PartBinAssociations.Add(assoc);

            return subSpace;
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

        private BinSubspace MakeSubspaceInsideBin(Bin bin, int subspaceIndex, string? label)
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
        
        private Bin InsertPartIntoBin(Bin bin, in Part part, Dictionary<int, int>? subspaceQuantity, string label)
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
                _partRepository.UpdatePartQuantityInsideSubspace(subspace, partId, subspaceQuantity[subspaceIndex]);
            }
            return bin;
        }

       
    }
}
