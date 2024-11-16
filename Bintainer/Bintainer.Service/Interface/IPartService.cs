using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Response;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Interface
{
    public interface IPartService
    {

        public Response<PartViewModel?> GetPartByName(string partName,string userId);
        public Response<Part?> CreatePart(CreatePartRequest request, string userId);
        public Response<Part?> UpdatePart(UpdatePartRequest request, string userId);
        public Response<List<PartAttribute>?> UpdatePartAttributes(UpdateAttributeRequest request,string userId);
        public Response<PartGroup> AddPartIntoGroup(Part part, string groupName, string userId);
        public void ArrangePartRequest(ArrangePartRequest arrangeRequest, string userId);
        public Response<List<PartUsageResponse>?> UsePart(string partName, string userId);
        public Response<List<PartBinAssociation>?> TryAdjustPartQuantity(AdjustQuantityRequest request,string userId);
        public Response<List<string>?> GetPartNames(string userId);
    }
}
