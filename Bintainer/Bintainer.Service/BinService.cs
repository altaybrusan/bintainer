using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service
{
    public class BinService : IBinService
    {
        private readonly IBinRepository _binRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private IAppLogger _appLogger;
        public BinService(IBinRepository binRepository, 
                          IInventoryRepository inventoryRepository,
                          IStringLocalizer<ErrorMessages> localizer,
                          IAppLogger appLooger)
        {
            _binRepository = binRepository;
            _inventoryRepository = inventoryRepository;
            _localizer = localizer;
            _appLogger = appLooger;
        }

        public Response<Bin> UpdateBin(Bin bin)
        {
            try
            {
                _binRepository.UpdateBin(bin);

                return new Response<Bin>() 
                {
                    IsSuccess = true,
                    Result = bin
                };

            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);

                return new Response<Bin>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
            
        }

        public Response<Bin> SaveBin(Bin bin)
        {
            try
            {
                _binRepository.SaveBin(bin);

                return new Response<Bin>() 
                { 
                    IsSuccess = true, 
                    Result = bin 
                };
            }
            catch (Exception ex)
            {
                _appLogger.LogMessage(ex.Message, LogLevel.Error);

                return new Response<Bin>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public Response<Dictionary<int,int>?> DistributeQuantityAcrossSubspaces(in Bin bin, int totalQuantity)
        {
            if(bin is null || bin.Section is null) 
            {
                return new Response<Dictionary<int, int>?>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = _localizer["ErrorBinOrSectionUnavailable"]
                };
            }

            int? subspaceCount = bin.Section.SubspaceCount;
            
            if ( subspaceCount is null || subspaceCount == 0)
            {
                var message = string.Format(_localizer["ErrorZeroSubspaceCount"], bin.Section.SectionName); ;
                return new Response<Dictionary<int, int>?>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = message
                };
            }
            
            int[] quantities = DividePartsEvenly(totalQuantity, subspaceCount!.Value);
            
            Dictionary<int, int> subspaceQuantity = new Dictionary<int, int>();

            for (int i = 1; i <= quantities.Length; i++)
            {
                subspaceQuantity.Add(i, quantities[i - 1]);
            }

            return new Response<Dictionary<int, int>?>()
            {
                IsSuccess = true,
                Result = subspaceQuantity
            };
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


    }
}
