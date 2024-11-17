using Bintainer.Model;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class TemplateRepository : ITemplateRepository
    {
        readonly BintainerDbContext _dbContext;
        public TemplateRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Dictionary<int, string> AttributeTemplatesTable { get; set; } = new Dictionary<int, string>();

        public List<PartTemplateInfo> GetAttributeTemplateInfoList(string userId)
        {
            return _dbContext.PartAttributeTemplates
                              .Where(t => t.UserId == userId && t.TemplateName != null)
                              .Select(p => new PartTemplateInfo()
                              {
                                  GuidId = p.GuidId,
                                  TemplateName = p.TemplateName != null ? p.TemplateName.Trim() : null,
                                  PartAttributes = p.PartAttributeDefinitions
                                                    .Select(a => new PartAttributeInfo()
                                                    {
                                                        GuidId = a.GuidId,
                                                        Name = a.Name != null ? a.Name.Trim() : null,
                                                        Value = a.Value != null ? a.Value.Trim() : null
                                                    }).ToList(),
                              }).ToList();


        }

        public PartAttributeTemplate? GetTemplate(Guid? templateGuid)
        {
            if (templateGuid == null) 
            {
                return null;
            }
            return _dbContext.PartAttributeTemplates.Where(t => t.GuidId == templateGuid).FirstOrDefault();
        }

        public PartAttributeTemplate? GetTemplate(string partName, string userId)
        {
            string trimmedPartName = partName.Trim();
            string trimmedUserId = userId.Trim();
            return _dbContext.PartAttributeTemplates.FirstOrDefault(t => t.TemplateName == trimmedPartName && t.UserId.Trim() == trimmedUserId);
        }

        public PartAttributeTemplate? CreateTemplate(string partName, string userId)
        {
            string trimmedPartName = partName.Trim();
            var attributeTemplate = new PartAttributeTemplate() { TemplateName = trimmedPartName, UserId = userId };
            _dbContext.PartAttributeTemplates.Add(attributeTemplate);
            _dbContext.SaveChanges(true);
            return attributeTemplate;
        }

        //public void SaveAttributes(List<PartAttribute> attributes)
        //{
        //    _dbContext.PartAttributes.AddRange(attributes);
        //    _dbContext.SaveChanges();
        //}
        
        public List<PartCategory>? GetCategories(string userId)
        {
            return _dbContext.PartCategories.Where(p => p.UserId == userId).ToList();
        }
        
        public PartCategory? GetPartCategoryById(string userId)
        {
            return _dbContext.PartCategories.Where(p => p.UserId == userId).FirstOrDefault();
        }

        public List<PartAttributeInfo> GetTemplatesDefaultAttributesInfo(Guid templateGuid)
        {
            var resultList = _dbContext.PartAttributeTemplates
                                       .Include(temp => temp.PartAttributeDefinitions) 
                                       .Where(temp => temp.GuidId == templateGuid) 
                                       .SelectMany(temp => temp.PartAttributeDefinitions)
                                       .Select(att => new PartAttributeInfo
                                       {
                                           GuidId = att.GuidId,
                                           Name = att.Name,
                                           Value = att.Value
                                       })
                                       .ToList();

            return resultList;
        }


        public PartCategory? GetCategory(int? id)
        {
            return _dbContext.PartCategories.FirstOrDefault(i => i.Id == id);
        }
        
        public PartCategory? UpdateAndSaveCategory(PartCategory category) 
        {
            _dbContext.PartCategories.Update(category);
            _dbContext.SaveChanges();
            return category;

        }

        public PartCategory AddAndSavePartCategory(PartCategory category)
        {
            _dbContext.PartCategories.Add(category);
            _dbContext.SaveChanges(true);
            return category;
        }
        
        public void RemovePartCategory(int? id)
        {
            var category = _dbContext.PartCategories.FirstOrDefault(i => i.Id == id);
            if (category is not null)
                _dbContext.PartCategories.Remove(category);
        }
        
        public Dictionary<int,string> GetAttributeTemplates(string userId) 
        {
            var attributes = _dbContext.PartAttributeTemplates.Where(p => p.UserId == userId);
            Dictionary<int,string>  AttributeTables = new Dictionary<int,string>();
            foreach (var item in attributes)
            {
                if (item.TemplateName != null)
                    AttributeTables[item.Id] = item.TemplateName;
            }
            return AttributeTables;
        }

        public PartAttributeTemplate AddAndSavePartAttribute(PartAttributeTemplate partAttribute)
        {
            _dbContext.PartAttributeTemplates.Add(partAttribute);
            _dbContext.SaveChanges();
            return partAttribute;
        }

        public void SaveChanges() 
        {
            _dbContext.SaveChanges();
        }

		public void RemoveAttributeTemplate(string userId, int templateId)
		{
			//_dbContext.PartAttributes.Where(a => a.TemplateId == templateId && userId == userId)?.ExecuteDelete();
			_dbContext.PartAttributeTemplates.Where(t => t.Id == templateId && userId == userId)?.ExecuteDelete();
			_dbContext.SaveChanges();
		}
    }
}
