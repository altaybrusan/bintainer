using Azure;
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
    }
}
