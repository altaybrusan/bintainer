using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service
{
    public class TemplateService : ITemplateService
    {
        ITemplateRepository _templateRepository;
        IAppLogger _appLoger;
        IStringLocalizer _localizer;
        IMapper _mapper;
        public TemplateService(ITemplateRepository repository,
                               IAppLogger appLogger,
                               IStringLocalizer localizer,
                               IMapper mapper)
        {
            _templateRepository = repository;
            _appLoger = appLogger;
            _localizer = localizer;
            _mapper = mapper;
        }
        public Response<Dictionary<int, string?>> GetTemplateByUserId(string userId)
        {
            try
            {
                var response = _templateRepository.GetTemplatesOfUser(userId);
                return new Response<Dictionary<int, string?>>()
                {
                    IsSuccess = true,
                    Result = response
                };
            }
            catch (Exception ex)
            {
                return new Response<Dictionary<int, string?>>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public Response<List<CategoryViewModel>?> GetPartCategories(string userId)
        {
            try
            {
                var categories = _templateRepository.GetPartCategories(userId);
                var result = BuildCategoryTree(categories);

                return new Response<List<CategoryViewModel>?>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {

                return new Response<List<CategoryViewModel>?>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public Response<List<PartAttributeViewModel>> GetPartAttributes(int tableId)
        {
            try
            {
                var attributes = _templateRepository.GetPartAttributeInfo(tableId);
                var result = _mapper.Map<List<PartAttributeViewModel>>(attributes);
                return new Response<List<PartAttributeViewModel>>()
                { 
                    IsSuccess = true, 
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLoger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<PartAttributeViewModel>>()
                {
                    IsSuccess = false,
                    Message = _localizer["ErrorRetriveAttributes"]
                };
            }
        }

        private List<CategoryViewModel> BuildCategoryTree(IEnumerable<PartCategory>? categories, int? parentId = null)
        {
            return categories.Where(c => c.ParentCategoryId == parentId)
                             .Select(c => new CategoryViewModel
                             {
                                 Title = c.Name?.Trim() ?? string.Empty,
                                 Id = c.Id,
                                 Children = BuildCategoryTree(categories, c.Id)
                             }).ToList();
        }

    }
}
