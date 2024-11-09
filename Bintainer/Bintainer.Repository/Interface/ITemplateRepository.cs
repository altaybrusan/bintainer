using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Interface
{
    public interface ITemplateRepository
    {
        public List<PartTemplateInfo> GetUserTemplatesInfo(string userId);
        public PartAttributeTemplate? GetAttributeTemplateById(int? templateId);
        public PartAttributeTemplate? GetAttributeTemplateByName(string partName, string userId);
        public PartAttributeTemplate? CreateAttributeTemplateByName(string partName, string userId);
        public void SaveAttributes(List<PartAttribute> attributes);
        public List<PartCategory>? GetPartCategories(string userId);
        public PartCategory? GetPartCategoryById(string userId);
        public PartCategory? GetCategory(int? id);
        public PartCategory? UpdateAndSaveCategory(PartCategory category);
        public List<PartAttributeInfo> GetPartAttributeInfo(int tableId);
        public PartCategory AddAndSavePartCategory(PartCategory category);
        public void RemovePartCategory(int? id);
        public Dictionary<int, string> GetAttributeTemplates(string userId);
        public PartAttributeTemplate AddAndSavePartAttribute(PartAttributeTemplate partAttribute);
        public void SaveChanges();
        public void RemoveAttributeTemplate(string userId,int attributeId);



    }
}
