using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Helper
{
    public class MappingProfile : Profile    
    {
        public MappingProfile() 
        {
            CreateMap<FilterOrderRequest, FilterOrderRequestModel>();
            CreateMap<PartAttributeInfo, PartAttributeViewModel>();
            CreateMap<PartCategory, CategoryViewModel>()
                .ForMember(dest => dest.Title, src => src.MapFrom(mbr => mbr.Name))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.InverseParentCategory))
                .ForMember(dest => dest.FlattenedHierarchy, opt => opt.MapFrom(src => GetFlattenedCategory(src)));
            CreateMap<InventorySectionViewModel, InventorySection>();
            CreateMap<PartAttribute, PartAttributeViewModel>();
            CreateMap<Part, PartViewModel>();
            CreateMap<PartGroup, PartGroupViewModel>();
            CreateMap<PartPackage, PartPackageViewModel>();



            //CreateMap<UserViewModel, AspNetUser>()
            //    .ForMember(dest => dest.UserName, src => src.MapFrom(mbr => mbr.Name))
            //    .ForMember(dest => dest.Id, src => src.MapFrom(mbr => mbr.UserId));

        }
        private string GetFlattenedCategory(PartCategory category)
        {
            var categoryNames = new List<string>();
            while (category != null)
            {
                if (!string.IsNullOrEmpty(category.Name))
                    categoryNames.Insert(0, category.Name);

                category = category.ParentCategory;
            }
            return string.Join(" > ", categoryNames);
        }
    }
}
