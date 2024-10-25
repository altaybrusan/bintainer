using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Extention
{
    public static class AttributeExtentions
    {
        public static List<PartAttribute> ToPartAttributeList(this Dictionary<string, string>? attributes, PartAttributeTemplate? template)
        {
            if(attributes is null || template is null) 
            {
                return new List<PartAttribute>();
            }
            return attributes.Select(attr => new PartAttribute
            {
                Name = attr.Key,
                Value = attr.Value,
                Template = template
            }).ToList();
        }
    }
}
