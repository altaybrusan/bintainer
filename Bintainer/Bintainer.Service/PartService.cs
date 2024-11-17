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
        //private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IBinService _binService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly IAppLogger _appLogger;
        private readonly IMapper _mapper;
        public PartService(IPartRepository partRepository,
                           ITemplateRepository templateRepository,
                           //IInventoryRepository inventoryRepository,
                           IInventoryService inventoryService,
                           IBinService binService,
                           IStringLocalizer<ErrorMessages> stringLocalizer,
                           IAppLogger applogger,
                           IMapper mapper)
        {
            _partRepository = partRepository;
            _templateRepository = templateRepository;
            _inventoryService = inventoryService;
            _binService = binService;
            _localizer = stringLocalizer;
            _appLogger = applogger;
            _mapper = mapper;
        }

        public Response<PartViewModel?> GetPartByName(string partName, string userId) 
        {
            try
            {
                var part = _partRepository.GetPart(partName, userId);
                
                var result = _mapper.Map<PartViewModel>(part);
                var attributes = _mapper.Map<List<PartAttributeViewModel>>(part.PartAttributes);
                result.Attributes = attributes;
                return new Response<PartViewModel?>
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
            }

            return new Response<PartViewModel?>
            {
                IsSuccess = false,
                Result = null,
                Message = _localizer["ErrorFailedPartLoading"]

            };

        }

        public Response<Part?> CreatePart(CreatePartRequest request, string userId) 
        {
            try
            {
                PartFilterCriteria filter = new()
                {
                    Number = request.PartNumber,
                    Supplier = request.Supplier,
                    UserId = userId
                };
                var parts = _partRepository.GetParts(filter);
                if (parts is not null && parts.Count > 0)
                {
                    _appLogger.LogMessage(_localizer["ErrorPartAlreadyExists"], LogLevel.Warning);
                    return new Response<Part?>()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = _localizer["ErrorPartAlreadyExists"]
                    };

                }

                Part part = new();
                part.Number = request.PartNumber!.Trim();
                part.Description = request.Description!.Trim();

                var Categories = _templateRepository.GetCategories(userId);               
                var categoryId = ExtractCategoryIdFromPath(Categories, request.PathToCategory!);
                part.CategoryId = categoryId;
                                
                part.UserId = userId;
                part.Supplier = request.Supplier!.Trim();

                List<PartGroup> groups = new List<PartGroup>();
                if(request.Group is not null)
                {
                    string?[] groupNames = request.Group!.Split(',');
                    groups = _partRepository.CreateOrUpdateGroup(groupNames,userId);
                }
                part.Groups = groups;

                //TODO: links should be fetched here.
                string packageName = string.IsNullOrEmpty(request.Package) ? "NA" : request.Package.Trim();
                var package = _partRepository.GetOrCreatePackage(packageName, userId);
                part.Package = package;

                var attributeTemplate = _templateRepository.GetTemplate(request.AttributeTemplateGuid);
                if (attributeTemplate is null)
                {
                    var defaultTemplate = _templateRepository.GetTemplate(request.PartNumber, userId);
                    if (defaultTemplate is null)
                    {
                        // The part is fetched from external source (e.g., DigiKey) or
                        // no attribute is assigned to the part
                        if(request.Attributes is not null && request.Attributes.Count>0)
                            attributeTemplate = _templateRepository.CreateTemplate(request.PartNumber, userId);
                    }
                    else
                    {
                        attributeTemplate = defaultTemplate;
                    }
                }

                var attributes = request.Attributes.ToPartAttributeList(attributeTemplate);

                part.Template = attributeTemplate;
                part.PartAttributes = attributes;
                _partRepository.UpdatePart(part);


                //TODO: complete the order in new part registration form.
                //if (!string.IsNullOrEmpty(request.OrderNumber))
                //{
                //    Order _order = _dbcontext.Orders.Where(o => o.OrderNumber == request.OrderNumber && o.UserId == UserId).FirstOrDefault();
                //}

                return new Response<Part?>()
                {
                    IsSuccess = true,
                    Result = part,
                    Message = _localizer["InfoPartUpdatedSuccessfully"]
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

        public Response<Part?> UpdatePart(UpdatePartRequest request, string userId)
        {
            try
            {
                var part = _partRepository.GetPart(request.GuidId!.Value);
                if (part is null)
                {
                    _appLogger.LogMessage(_localizer["WarningPartNotFound"], LogLevel.Warning);
                    return new Response<Part?>()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = _localizer["WarningPartNotFound"]
                    };

                }                             
                
                part.Number = request.PartNumber!.Trim();
                part.Description = request.Description!.Trim();

                if(request.PathToCategory != null && request.PathToCategory.Count > 0) 
                {
                    var Categories = _templateRepository.GetCategories(userId);
                    var categoryId = ExtractCategoryIdFromPath(Categories, request.PathToCategory!);
                    part.CategoryId = categoryId;
                }

                part.UserId = userId;
                part.Supplier = request.Supplier!.Trim();

                List<PartGroup> groups = new List<PartGroup>();
                if (request.Group is not null)
                {
                    string?[] groupNames = request.Group!.Split(',');
                    groups = _partRepository.CreateOrUpdateGroup(groupNames, userId);
                }
                part.Groups = groups;

                string packageName = string.IsNullOrEmpty(request.Package) ? "NA" : request.Package.Trim();
                var package = _partRepository.GetOrCreatePackage(packageName, userId);
                part.Package = package;

                UpdatePartAttributes(request.Attributes, ref part);
                                
                _partRepository.UpdatePart(part);


                return new Response<Part?>()
                {
                    IsSuccess = true,
                    Result = part,
                    Message = _localizer["InfoPartCreatedSuccessfully"]
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Part?>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = ex.Message
                };
            }
        }
        private void UpdatePartAttributes(List<PartAttributeViewModel> updatedAttributes, ref Part part)
        {
            // Create a dictionary for efficient lookup of updated attributes by GuidId
            var updatedAttributesDict = updatedAttributes
                .Where(attr => attr.GuidId.HasValue)
                .ToDictionary(attr => attr.GuidId.Value, attr => attr);

            // Find attributes that should be deleted (existing in part but not in updatedAttributes)
            var attributesToDelete = part.PartAttributes
                .Where(existingAttr => existingAttr.GuidId.HasValue && !updatedAttributesDict.ContainsKey(existingAttr.GuidId.Value))
                .ToList();

            // Remove those attributes from part.PartAttributes
            foreach (var attrToDelete in attributesToDelete)
            {
                part.PartAttributes.Remove(attrToDelete);
            }

            // Update existing attributes or add new ones
            foreach (var updatedAttr in updatedAttributes)
            {
                if (updatedAttr.GuidId.HasValue)
                {
                    // Check if this attribute already exists in part.PartAttributes
                    var existingAttr = part.PartAttributes
                        .FirstOrDefault(attr => attr.GuidId.HasValue && attr.GuidId.Value == updatedAttr.GuidId.Value);

                    if (existingAttr != null)
                    {
                        // Update the existing attribute
                        existingAttr.Name = updatedAttr.Name;
                        existingAttr.Value = updatedAttr.Value;
                    }
                    else
                    {
                        // Add new attribute if not found
                        part.PartAttributes.Add(new PartAttribute
                        {
                            GuidId = updatedAttr.GuidId,
                            Name = updatedAttr.Name,
                            Value = updatedAttr.Value,
                            PartId = part.Id
                        });
                    }
                }
                else
                {
                    // If GuidId is null, add as a new attribute
                    part.PartAttributes.Add(new PartAttribute
                    {
                        Name = updatedAttr.Name,
                        Value = updatedAttr.Value,
                        PartId = part.Id
                    });
                }
            }
        }


        public Response<List<PartAttribute>?> UpdatePartAttributes(UpdateAttributeRequest request, string userId)
        {
            try
            {
                var part = _partRepository.GetPart(request.PartName, userId);
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

                var existingAttributes = part.PartAttributes.ToList();
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
                            //TemplateId = part.Template?.Id ?? 0 // Ensure TemplateId is set correctly
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

        public Response<string> ArrangePartRequest(ArrangePartRequest arrangeRequest, string userId)
        {
            try
            {
                var sectionResponse = _inventoryService.GetInventorySection(userId, arrangeRequest.SectionId);
                InventorySection? inventorySection = sectionResponse.IsSuccess && string.IsNullOrEmpty(sectionResponse.Message) ? sectionResponse.Result : throw new Exception(sectionResponse.Message);
                Part? part = _partRepository.GetPart(arrangeRequest.PartNumber, userId);

                if (inventorySection == null)
                {
                    _appLogger.LogMessage("ErrorInventorySectionMissing");
                    return new Response<string>()
                    {
                        IsSuccess = false,
                        Message = _localizer["ErrorInventorySectionMissing"],
                        Result = string.Empty,
                    };
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
                    return new Response<string>()
                    {
                        IsSuccess = false,
                        Message = _localizer["ErrorInvalidPartDetails"],
                        Result = string.Empty,
                    };
                }
                return new Response<string>()
                {
                    IsSuccess = false,
                    Message = _localizer["InfoPartArrangSuccessfully"],
                    Result = string.Empty,
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<string>()
                {
                    IsSuccess = false,
                    Message = _localizer["ErrorFailedPartArrange"],
                    Result = string.Empty,
                };
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
            Part? part = _partRepository.GetPart(partName, userId);
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

                Part? part = _partRepository.GetPart(request.PartName, userId);

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
                var response = _binService.DistributeQuantityAcrossSubspaces(bin, arrangeRequest.FillAllQuantity.Value);
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
                                   PartName = g.First().Part.Number?.Trim(),
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

        private int? ExtractCategoryIdFromPath(List<PartCategory>? Categories,List<string> path)
        {
            if(Categories is null)
                return null;
            return FindCategoryIdRecursively(Categories, path, 0);
        }

        private int? FindCategoryIdRecursively(List<PartCategory> categories, List<string> path, int level)
        {
            if (level >= path.Count)
            {
                return null;
            }

            foreach (var category in categories)
            {
                if (category.Name?.Trim() == path[level])
                {

                    if (level == path.Count - 1)
                    {
                        return category.Id;
                    }
                    return FindCategoryIdRecursively(category.InverseParentCategory.ToList(), path, level + 1);
                }
            }

            // If no matching category is found, return null
            return null;
        }

        #endregion

    }
}
