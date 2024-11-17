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
        public Inventory? GetInventory(string admin);
        public Inventory? GetInventoryById(string userId);
        public Inventory? GetUserInventoryByUserId(string userId);
        public List<InventorySection>? GetAllInventorySections(int inventoryId);
        public InventorySection? GetSection(string userId, int sectionId);
        public InventorySection? UpdateSection(InventorySection? section);
        public InventorySection? AddBin(InventorySection section, Bin bin);
        public Inventory? CreateOrUpdateInventory(Inventory? inventory);


    }
}
