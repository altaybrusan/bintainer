using AutoMapper;
using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Bintainer.Service
{
    public class InventoryService : IInventoryService
    {

        IInventoryRepository _inventoryRepository;
        IBinService _binService;
        IStringLocalizer<ErrorMessages> _strLocalizer;
        IAppLogger _appLogger;
        IMapper _mapper;
        IUserRepository _userRepository;

        public InventoryService(IBinService binService, 
                                IInventoryRepository inventoryRepository,
                                IStringLocalizer<ErrorMessages> localizer,
                                IMapper mapper,
                                IAppLogger appLogger,
                                IUserRepository userRepository)
        {
            _binService = binService;
            _inventoryRepository = inventoryRepository;
            _strLocalizer = localizer;
            _mapper = mapper;
            _appLogger = appLogger;
            _userRepository = userRepository;
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
        
        public Response<Inventory?> GetInventory(string admin) 
        {
            try
            {
                var inventory = _inventoryRepository.GetInventory(admin);
                return new Response<Inventory?>()
                {
                    IsSuccess = true,
                    Result = inventory
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Inventory?>() 
                { 
                    IsSuccess = false, 
                    Message = ex.Message 
                };
            }           
        }
        public Response<Inventory?> CreateOrUpdateInventory(UserViewModel userViewModel, string inventoryName, List<InventorySection>? sectionList = null)
        {
            try
            {
                
                if (userViewModel is null || 
                    string.IsNullOrEmpty(userViewModel.UserId) || 
                    string.IsNullOrEmpty(userViewModel.Name) || 
                    string.IsNullOrEmpty(inventoryName)) 
                {
                    throw new ArgumentNullException(nameof(userViewModel));
                }
                var user = _userRepository.GetUser(userViewModel.UserId);
                if (user == null) 
                {
                    throw new ArgumentNullException(nameof(user));
                }
                Inventory inventory = new()
                {
                    InventorySections = sectionList,
                    User = user,
                    UserId = user.Id,
                    Name = inventoryName                   
                };

                _inventoryRepository.CreateOrUpdateInventory(inventory);

                return new Response<Inventory?>()
                {
                    IsSuccess = true,
                    Result = inventory
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Inventory?>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }            
        }

    }
}
