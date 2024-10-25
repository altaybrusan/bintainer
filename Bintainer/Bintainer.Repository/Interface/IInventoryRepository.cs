using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Interface
{
    public interface IInventoryRepository
    {
        public Inventory? GetUserInventoryByUserName(string userName);
        public Inventory? GetUserInventoryByUserId(string userId);
        public List<InventorySection>? GetAllInventorySections(int inventoryId);
        public InventorySection? GetSection(string userId, int sectionId);
        public InventorySection? UpdateSection(InventorySection? section);
        public InventorySection? AddBin(InventorySection section, Bin bin);
        public Inventory? AddAndSaveInventory(Inventory? inventory);
        public Inventory? UpdateInventory(Inventory? inventory);


    }
}
