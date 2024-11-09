using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Response;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Extention;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;


namespace Bintainer.Service
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _partRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IBinService _binService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly IAppLogger _appLogger;
        private readonly IMapper _mapper;
        public PartService(IPartRepository partRepository,
                           ITemplateRepository templateRepository,
                           IInventoryRepository inventoryRepository,
                           IInventoryService inventoryService,
                           IBinService binService,
                           IStringLocalizer<ErrorMessages> stringLocalizer,
                           IAppLogger applogger,
                           IMapper mapper)
        {
            _partRepository = partRepository;
            _templateRepository = templateRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryService = inventoryService;
            _binService = binService;
            _localizer = stringLocalizer;
            _appLogger = applogger;
            _mapper = mapper;
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
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
            }

            return new Response<Part?>
            {
                IsSuccess = false,
                Result = null,
                Message = _localizer["ErrorFailedPartLoading"]

            };

        }

        public Response<List<PartAttributeViewModel>?> GetPartAttributes(string partName, string userId) 
        {
            try
            {            
                var attributes = _partRepository.GetPartAttributes(partName, userId);
                return new Response<List<PartAttributeViewModel>?>()
                {
                    IsSuccess = true,
                    Result = _mapper.Map<List<PartAttributeViewModel>>(attributes)
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<PartAttributeViewModel>?>()
                {
                    IsSuccess = false,
                    Message = _localizer["ErrorFailToRetriveAttributes"]
                };
            }
        }
        public Response<List<PartAttributeViewModel>?> MapPartAttributesToViewModel(string partName,string userId) 
        {
            try
            {
                Part? part = _partRepository.GetPartByName(partName, userId);
                if (part is null)
                {
                    return new Response<List<PartAttributeViewModel>?>()
                    {
                        IsSuccess = true,
                        Result = null,
                        Message= _localizer["WarningPartNotFound"]
                        
                    };
                }
                var result = part.AttributeTemplates?.FirstOrDefault()?.PartAttributes?
                                        .Select(attr => new PartAttributeViewModel()
                                        {
                                            Name = attr.Name != null ? attr.Name.Trim() : null,
                                            Value = attr.Value != null ? attr.Value.Trim() : null
                                        }).ToList();
                return new Response<List<PartAttributeViewModel>?>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message,LogLevel.Error);
                return new Response<List<PartAttributeViewModel>?>()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Result = null
                };
            }

        }

        public Response<Part?> CreatePartForUser(CreatePartRequest request, string userId) 
        {
            try
            {
                PartFilterCriteria filter = new()
                {
                    Name = request.PartName,
                    Supplier = request.Supplier,
                    UserId = userId
                };
                var parts = _partRepository.GetPartsByCriteria(filter);
                if (parts is not null && parts.Count > 0)
                {
                    //TODO: log event
                    _appLogger.LogMessage(_localizer["ErrorPartAlreadyExists"], LogLevel.Warning);
                    return new Response<Part?>()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = _localizer["ErrorPartAlreadyExists"]
                    };

                }

                Part part = new Part();
                part.Name = request.PartName!.Trim();
                part.Description = request.Description!.Trim();

                var Categories = _templateRepository.GetPartCategories(userId);
               
                var categoryId = FindCategoryIdByPath(Categories, request.PathToCategory);
                part.CategoryId = categoryId;
                
                
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

                return new Response<Part?>()
                {
                    IsSuccess = true,
                    Result = part,
                    Message = _localizer["InfoPartCreatedSuccessfully"]
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message,LogLevel.Error);
                return new Response<Part?>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = ex.Message
                };
            }
        }

        public Response<List<PartAttribute>?> UpdatePartAttributes(UpdateAttributeRequest request, string userId)
        {
            try
            {
                var part = _partRepository.GetPartByName(request.PartName, userId);
                if (part is null)
                {
                    return new Response<List<PartAttribute>?>()
                    {
                        IsSuccess = true,
                        Result = null,
                        Message = _localizer["WarningPartNotFound"]

                    };
                }

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
                return new Response<List<PartAttribute>?>()
                {
                    IsSuccess = true,
                    Result = existingAttributes
                };

            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                throw;
            }         
        }

        public void ArrangePartRequest(ArrangePartRequest arrangeRequest, string userId)
        {
            try
            {
                InventorySection? inventorySection = _inventoryRepository.GetSection(userId, arrangeRequest.SectionId);
                Part? part = _partRepository.GetPartByName(arrangeRequest.PartName, userId);

                if (inventorySection == null)
                {
                    _appLogger.LogMessage("ErrorInventorySectionMissing");
                    return;                   
                }

                var response = _inventoryService.GetBinFrom(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY);
                Bin? bin = response.IsSuccess && string.IsNullOrEmpty(response.Message) ? response.Result : throw new Exception(response.Message);
                
                if (bin is not null && part is not null)
                {
                    ProcessArrangePartRequest(bin, part, arrangeRequest, userId);
                }

                if (bin is null && part is not null)
                {
                    response = _inventoryService.CreateBin(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY);
                    bin = response.IsSuccess && string.IsNullOrEmpty(response.Message) ? response.Result : throw new Exception(response.Message);
                    ProcessArrangePartRequest(bin, part, arrangeRequest, userId);
                }

                if (part is null)
                {
                    _appLogger.LogMessage("ErrorInvalidPartDetails");
                    return ;
                }
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
            }
        }

        public Response<PartGroup> AddPartIntoGroup(Part part, string groupName, string userId)
        {
            try
            {
                var group = _partRepository.GetPartGroup(groupName, userId);

                if (group is not null && group.Parts.Contains(part))
                {
                    return new Response<PartGroup>()
                    {
                        IsSuccess = true,
                        Result = group
                    };
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

                return new Response<PartGroup>() { IsSuccess = true, Result = group };

            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<PartGroup>() { IsSuccess = false, Message = ex.Message };
            }
         
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

        public Response<List<PartUsageResponse>?> UsePart(string partName, string userId)
        {
            Part? part = _partRepository.GetPartByName(partName, userId);
            if (part is not null)
            {
               var result = GetPartUsageResponse(part);
                return new Response<List<PartUsageResponse>?>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            return new Response<List<PartUsageResponse>?>()
            {
                IsSuccess = false,
                Result = null,
                Message = ""
            };            
        }

        public Response<List<PartBinAssociation>?> TryAdjustPartQuantity(AdjustQuantityRequest request,string userId) 
        {
            if (request.QuantityUsed > request.Quantity)
            {
                _appLogger.LogMessage(_localizer["ErrorUsedItemsExceedAvailable"], LogLevel.Error);
            }

            try
            {
                List<int> subSpaces = ParseSubspaceIndices(request.SubspaceIndices);
                int takeOut = request.QuantityUsed;

                Part? part = _partRepository.GetPartByName(request.PartName, userId);

                if (part is null)
                {
                    return new Response<List<PartBinAssociation>?>()
                    {
                        IsSuccess = true,
                        Result = null,
                        Message = _localizer["WarningPartNotFound"]
                    };
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
                return new Response<List<PartBinAssociation>?>() 
                { 
                    IsSuccess = true, 
                    Result = result 
                };

            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<PartBinAssociation>?>()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Result = null
                };
            }
        }

        public Response<List<string>?> GetPartNames(string userId)
        {
            try
            {
                var result = _partRepository.GetPartNameList(userId);
                return new Response<List<string>?>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<string>?>()
                {
                    IsSuccess = true,
                    Message = _localizer["ErrorFailedAttributeTemplatedRemove"]
                };
            }
        }


        #region Private

        private void ProcessArrangePartRequest(Bin? bin, Part part, ArrangePartRequest arrangeRequest, string UserId)
        {
            Dictionary<int, int>? subspaceQuantity = new Dictionary<int, int>();
            if (arrangeRequest.IsFilled)
            {
                var response = _binService.DistributeQuantityAcrossSubspaces(bin, arrangeRequest.FillAllQuantity.Value); ;
                if (response.IsSuccess)
                    subspaceQuantity = response.Result;
                else
                    return;
            }
            else
            {
                subspaceQuantity = arrangeRequest.SubspaceQuantities;
            }

            var updatedBin = InsertPartIntoBin(bin, part, subspaceQuantity, arrangeRequest.Label);
            _binService.UpdateBin(updatedBin);

            if (arrangeRequest.Group is not null)
            {
                var response = AddPartIntoGroup(part, arrangeRequest.Group, UserId);
                var partGroup = response.IsSuccess && string.IsNullOrEmpty(response.Message) ? response.Result : throw new Exception(response.Message);
                part.Groups.Add(partGroup);
            }
            _partRepository.UpdatePart(part);
            _binService.UpdateBin(bin);
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

        private BinSubspace? InsertPartIntoSubspace(BinSubspace subSpace, Part part)
        {
            if (subSpace.Bin is null)
            {
                _appLogger.LogMessage(_localizer["ErrorBinCannotBeNull"], LogLevel.Error);
                return null;
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

        private bool IsSubspaceAvailableForPart(in Bin bin, in Part part, List<int>? subSpaceIndices)
        {
            var existingSubspaces = bin.BinSubspaces.Where(s => subSpaceIndices!.Contains(s.SubspaceIndex!.Value)).ToList();
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
            if (!IsSubspaceAvailableForPart(bin, part, subspaceQuantity?.Keys.ToList()))
            {
                _appLogger.LogMessage(_localizer["ErrorSubspaceInsertionFailed"], LogLevel.Error);
            }
            foreach (var subspaceIndex in subspaceQuantity.Keys)
            {
                BinSubspace subspace = MakeSubspaceInsideBin(bin, subspaceIndex, label);
                InsertPartIntoSubspace(subspace, part);
                _partRepository.UpdatePartQuantityInsideSubspace(subspace, partId, subspaceQuantity[subspaceIndex]);
            }
            return bin;
        }

        public int? FindCategoryIdByPath(List<PartCategory> Categories,List<string> path)
        {           
            return FindCategoryIdRecursively(Categories, path, 0);
        }

        private int? FindCategoryIdRecursively(List<PartCategory> categories, List<string> path, int level)
        {
            // If we've reached the end of the path, return null
            if (level >= path.Count)
            {
                return null;
            }

            foreach (var category in categories)
            {
                // Check if the current category name matches the path at the current level
                if (category.Name?.Trim() == path[level])
                {
                    // If this is the last level in the path, return the Id
                    if (level == path.Count - 1)
                    {
                        return category.Id;
                    }
                    // Otherwise, continue searching in the child categories
                    return FindCategoryIdRecursively(category.InverseParentCategory.ToList(), path, level + 1);
                }
            }

            // If no matching category is found, return null
            return null;
        }

        #endregion

    }
}
