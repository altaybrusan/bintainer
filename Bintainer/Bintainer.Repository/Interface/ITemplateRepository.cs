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
        public List<PartTemplateInfo> GetAttributeTemplateInfoList(string userId);
        public PartAttributeTemplate? GetTemplate(Guid? templateGuid);
        public PartAttributeTemplate? GetTemplate(string partName, string userId);
        public PartAttributeTemplate? CreateTemplate(string partName, string userId);
        public List<PartCategory>? GetCategories(string userId);
        public PartCategory? GetPartCategoryById(string userId);
        public PartCategory? GetCategory(int? id);
        public PartCategory? UpdateAndSaveCategory(PartCategory category);
        public List<PartAttributeInfo> GetTemplatesDefaultAttributesInfo(Guid templateGuid);
        public PartCategory AddAndSavePartCategory(PartCategory category);
        public void RemovePartCategory(int? id);
        public Dictionary<int, string> GetAttributeTemplates(string userId);
        public PartAttributeTemplate AddAndSavePartAttribute(PartAttributeTemplate partAttribute);
        public void SaveChanges();
        public void RemoveAttributeTemplate(string userId,int attributeId);

    }
}
