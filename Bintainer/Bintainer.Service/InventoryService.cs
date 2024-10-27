using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Bintainer.Service
{
    public class InventoryService : IInventoryService
    {

        IInventoryRepository _inventoryRepository;
        IBinService _binService;
        IStringLocalizer _strLocalizer;
        IAppLogger _appLogger;
        public InventoryService(IBinService binService, 
                                IInventoryRepository inventoryRepository,
                                IStringLocalizer localizer,
                                IAppLogger appLogger)
        {
            _binService = binService;
            _inventoryRepository = inventoryRepository;
            _strLocalizer = localizer;
            _appLogger = appLogger;
        }

        public Response<InventorySection?> GetInventorySection(string? userId, int sectionId)
        {
            try
            {
                var result = _inventoryRepository.GetSection(userId, sectionId);

                return new Response<InventorySection?>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
              
                return new Response<InventorySection?>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public Response<Bin?> GetBinFrom(InventorySection? section, int coordinateX, int coordinateY) 
        {
            try
            {
                var response = section?.Bins.FirstOrDefault(b => b.CoordinateX == coordinateX &&
                                                                 b.CoordinateY == coordinateY);

                return new Response<Bin?>() 
                { 
                    IsSuccess = true, 
                    Result = response 
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Bin?>() 
                { 
                    IsSuccess = false, 
                    Message = ex.Message 
                };
            }
        }

        public Response<Bin?> CreateBin(InventorySection section, int coordinateX, int coordinateY)
        {
            try
            {
                Bin bin = new Bin() { CoordinateX = coordinateX, CoordinateY = coordinateY, SectionId = section.Id };
                _binService.SaveBin(bin);
                _inventoryRepository.AddBin(section, bin);

                return new Response<Bin?>() 
                { 
                    IsSuccess = true, 
                    Result = bin 
                };
            }
            catch (Exception ex)
            {
                return new Response<Bin?>() 
                { 
                    IsSuccess = false, 
                    Message = ex.Message 
                };
            }
            
        }
        
        public Response<List<InventorySection>?> GetInventorySectionsOfUser(string admin) 
        {
            try
            {
                var inventory = _inventoryRepository.GetUserInventoryByUserName(admin);
                string? InventoryName = inventory?.Name;

                if (inventory is not null)
                {
                    var result = _inventoryRepository.GetAllInventorySections(inventory.Id);
                    return new Response<List<InventorySection>?>()
                    {
                        IsSuccess = true,
                        Result = result
                    };
                }
                var sections = new List<InventorySection>();
                sections.Add(new InventorySection() { Height = 1, Width = 1 });

                return new Response<List<InventorySection>?> { IsSuccess = true, Result = sections };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<InventorySection>?>() 
                { 
                    IsSuccess = false, 
                    Message = ex.Message 
                };
            }           
        }
        public Response<Inventory> CreateOrUpdateInventory(UserViewModel user, string inventoryName) 
        {
            try
            {
                var inventory = _inventoryRepository.GetUserInventoryByUserName(user.Name);
                if (inventory is null)
                {
                    inventory = new Inventory() { Admin = user.Name, Name = inventoryName?.Trim(), UserId = user.UserId };
                    inventory = _inventoryRepository.AddAndSaveInventory(inventory);
                }
                else
                {
                    // the user already created an inventory
                    if (inventory.Name != inventoryName)
                    {
                        inventory.Name = inventoryName?.Trim();
                        _inventoryRepository.UpdateInventory(inventory);
                    }
                }
                return new Response<Inventory>()
                {
                    IsSuccess = true,
                    Result = inventory
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Inventory>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
            
        }

        public Response<Inventory?> AddSectionsToInventory(List<InventorySection>? sectionList, Inventory inventory) 
        {
            try
            {
                if (sectionList is null) 
                {
                    return new Response<Inventory?>
                    {
                        IsSuccess = true,
                        Result = inventory
                    };
                }

                foreach (var section in sectionList)
                {
                    if (section.Id == 0)
                    {
                        section.InventoryId = inventory.Id;
                        inventory.InventorySections.Add(section);

                    }
                    else
                    {
                        //TODO: test if this section is correct. The updated sections should be stored in  inventory.
                        section.InventoryId = inventory.Id;
                        _inventoryRepository.UpdateSection(section);
                    }
                }
                return new Response<Inventory?>
                {
                    IsSuccess = true,
                    Result = inventory
                };

            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Inventory?>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
         
        }
    }
}
