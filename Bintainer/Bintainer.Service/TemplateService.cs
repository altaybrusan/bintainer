using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service
{
    public class TemplateService: ITemplateService
    {
        ITemplateRepository templateRepository;
        public TemplateService(ITemplateRepository repository )
        {
            templateRepository = repository;
        }
        public Dictionary<int, string?> GetTemplateByUserId(string userId)
        {
            return templateRepository.GetTemplatesOfUser(userId);
        }
    }
}
