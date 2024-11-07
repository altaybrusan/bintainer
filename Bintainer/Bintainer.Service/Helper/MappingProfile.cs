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
            CreateMap<PartCategory, CategoryViewModel>();
            CreateMap<InventorySectionViewModel, InventorySection>();
            //CreateMap<UserViewModel, AspNetUser>()
            //    .ForMember(dest => dest.UserName, src => src.MapFrom(mbr => mbr.Name))
            //    .ForMember(dest => dest.Id, src => src.MapFrom(mbr => mbr.UserId));
            
        }
    }
}
