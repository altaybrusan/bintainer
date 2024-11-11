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
    public interface IInventoryService
    {
        //public Response<InventorySection?> GetInventorySection(string? userId, int sectionId);
        public Response<Bin?> GetBinFrom(InventorySection? section, int coordinateX, int coordinateY);
        public Response<Bin?> CreateBin(InventorySection section, int coordinateX, int coordinateY);
        public Response<Inventory?> GetInventory(string admin);
        public Response<Inventory?> CreateOrUpdateInventory(UserViewModel user, string inventoryName, List<InventorySection>? sectionList = null);

    }
}
