using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private ITemplateRepository _templateRepository;
        private IAppLogger _appLoger;
        private IStringLocalizer<ErrorMessages> _localizer;
        private IMapper _mapper;
        public TemplateService(ITemplateRepository repository,
                               IAppLogger appLogger,
                               IStringLocalizer<ErrorMessages> localizer,
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

        public void EnsureRootNodeExists(string userId)
        {
            var root = _templateRepository.GetPartCategoryById(userId);
            if (root is null)
            {
                AddRootNode(userId);
            }
        }

        private List<CategoryViewModel> BuildCategoryTree(IEnumerable<PartCategory>? categories, int? parentId = null)
        {
            if (categories == null) return new List<CategoryViewModel>();

            // Build the tree starting with root categories (ParentCategoryId == parentId)
            var rootCategories = categories.Where(c => c.ParentCategoryId == parentId)
                                           .Select(c => new CategoryViewModel
                                           {
                                               Title = c.Name?.Trim() ?? string.Empty,
                                               Id = c.Id,
                                               // Recursively build the children and set to null if the list is empty
                                               Children = BuildCategoryTree(categories, c.Id)
                                                           .ToList()
                                                           .Any() ? BuildCategoryTree(categories, c.Id) : null
                                           })
                                           .ToList();

            return rootCategories;
        }



        private void AddRootNode(string userId)
        {
            CategoryViewModel viewNode = new CategoryViewModel()
            {
                Title = "Root"
            };
            AddItem(viewNode, userId);

        }
        
        private void DeleteItem(CategoryViewModel parent)
        {
            _templateRepository.RemovePartCategory(parent.Id);
            foreach (var item in parent.Children)
            {
                DeleteItem(item);
            }
        }
        
        private void AddItem(CategoryViewModel nodeView, string userId, int? parentId = null)
        {
            PartCategory newCategory = new() { Name = nodeView.Title, UserId = userId };
            if (parentId != null)
            {
                newCategory.ParentCategory = _templateRepository.GetCategory(parentId);
            }

            _templateRepository.AddAndSavePartCategory(newCategory);


            foreach (var item in nodeView.Children)
            {
                AddItem(item, userId, newCategory.Id);
            }
        }

        public Response<Dictionary<int, string>> GetAttributeTemplates(string userId)
        {
            try
            {
                var result = _templateRepository.GetAttributeTemplates(userId);
                return new Response<Dictionary<int, string>>()
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                _appLoger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<Dictionary<int, string>>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

        }

        public Response<PartAttributeTemplate> SaveAttributeTemplate(CreateAttributeTemplateRequest request, string userId)
        {
            try
            {
                PartAttributeTemplate table = new() { TemplateName = request.TableName, UserId = userId };

                foreach (var item in request.Attributes)
                {
                    var attribute = new PartAttribute() { Name = item.Key, Value = item.Value };
                    table.PartAttributes.Add(attribute);
                }
                _templateRepository.AddAndSavePartAttribute(table);

                return new Response<PartAttributeTemplate>()
                {
                    IsSuccess = true,
                    Result = table
                };
            }
            catch (Exception ex)
            {
                _appLoger.LogMessage(ex.Message,LogLevel.Error);
                return new Response<PartAttributeTemplate>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            } 
        }

        //TODO: needs rewrite
        public Response<List<CategoryViewModel>> SavePartCategory(List<CategoryViewModel> categories, string userId) 
        {
            try
            {
                var original = _templateRepository.GetPartCategories(userId);
                var mappedOriginal = _mapper.Map<List<CategoryViewModel>>(original);
                CategoryViewModelComparer comparer = new CategoryViewModelComparer(mappedOriginal, categories);
                var AddedItems = comparer.Added;
                var deletedItems = comparer.Deleted;
                var updatedItems = comparer.Updated;

                foreach (var item in updatedItems)
                {
                    var _category = _templateRepository.GetCategory(item.Id);
                    _category.Name = item.Title;
                    _category.UserId = userId;
                    //TODO: refactor this
                    _templateRepository.UpdateAndSaveCategory(_category);
                }
                foreach (var item in deletedItems)
                {
                    DeleteItem(item);
                }
                foreach (var item in AddedItems)
                {
                    AddItem(item, userId, item.ParentId);
                }
                _templateRepository.SaveChanges();

                return new Response<List<CategoryViewModel>>()
                {
                    IsSuccess = true,                    
                    Result = mappedOriginal
                };

            }
            catch (Exception ex)
            {
                _appLoger.LogMessage(ex.Message, LogLevel.Error);
                return new Response<List<CategoryViewModel>>()
                {
                    IsSuccess = false,
                    Message = _localizer["ErrorFailedSavingPartCategories"],
                    Result = null
                };
            }
 
        }

		public Response<string> RemoveAttributeTemplate(string userId, int templateId)
		{
            try
            {
                _templateRepository.RemoveAttributeTemplate(userId, templateId);
                return new Response<string>()
                {
                    IsSuccess = true,
                    Message = _localizer["InfoAttributeTemplatedRemoved"]
                };
            }
            catch (Exception ex)
            {
                _appLoger.LogMessage(ex.Message, LogLevel.Error);
				return new Response<string>()
				{
					IsSuccess = true,
					Message = _localizer["ErrorFailedAttributeTemplatedRemove"]
				};
			}

		}
	}
}
