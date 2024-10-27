using Bintainer.Model;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class TemplateRepository:ITemplateRepository
    {
        readonly BintainerDbContext _dbContext;
        public TemplateRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Dictionary<int, string> AttributeTemplatesTable { get; set; } = new Dictionary<int, string>();

        public Dictionary<int,string?> GetTemplatesOfUser(string userId)
        {
           return  _dbContext.PartAttributeTemplates
                             .Where(t => t.UserId == userId && t.TemplateName != null)
                             .ToDictionary(t => t.Id, t => t.TemplateName);
            

        }

        public PartAttributeTemplate? GetAttributeTemplateById(int? templateId)
        {
            if (templateId is null)
                return null;
            return _dbContext.PartAttributeTemplates.Where(t => t.Id == templateId).FirstOrDefault();
        }

        public PartAttributeTemplate? GetAttributeTemplateByName(string partName, string userId)
        {
            string trimmedPartName = partName.Trim();
            string trimmedUserId = userId.Trim();
            return _dbContext.PartAttributeTemplates.FirstOrDefault(t => t.TemplateName == trimmedPartName && t.UserId.Trim() == trimmedUserId);
        }

        public PartAttributeTemplate? CreateAttributeTemplateByName(string partName, string userId)
        {
            string trimmedPartName = partName.Trim();
            var attributeTemplate = new PartAttributeTemplate() { TemplateName = trimmedPartName, UserId = userId };
            _dbContext.PartAttributeTemplates.Add(attributeTemplate);
            _dbContext.SaveChanges(true);
            return attributeTemplate;
        }

        public void SaveAttributes(List<PartAttribute> attributes)
        {
            _dbContext.PartAttributes.AddRange(attributes);
            _dbContext.SaveChanges();
        }
        public List<PartCategory>? GetPartCategories(string userId)
        {
            return _dbContext.PartCategories.Where(p => p.UserId == userId).ToList();
        }

        public List<PartAttributeInfo> GetPartAttributeInfo(int tableId)
        {
            var resultList = _dbContext.PartAttributes
                                       .Where(t => t.TemplateId == tableId)
                                       .Select(attribute => new PartAttributeInfo (){ Name = attribute.Name, Value = attribute.Value })
                                       .ToList();
            return resultList;
        }
    }
}
