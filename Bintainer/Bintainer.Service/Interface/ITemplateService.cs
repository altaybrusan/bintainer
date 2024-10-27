using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Interface
{
    public interface ITemplateService
    {
        public Response<Dictionary<int,string?>> GetTemplateByUserId(string userId);
        public Response<List<CategoryViewModel>?> GetPartCategories(string userId);
        public Response<List<PartAttributeViewModel>> GetPartAttributes(int tableId);
    }
}
