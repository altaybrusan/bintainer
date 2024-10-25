using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class InventoryRepository: IInventoryRepository
    {
        BintainerDbContext _dbContext;
        public InventoryRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public InventorySection? AddBin(InventorySection section, Bin bin)
        {
            section.Bins.Add(bin);
            _dbContext.InventorySections.Update(section);
            _dbContext.SaveChanges(true);
            return section;
        }

        public InventorySection? GetSection(string userId, int sectionId)
        {
            return _dbContext.InventorySections.Where(i => i.Inventory.UserId == userId && i.Id == sectionId).FirstOrDefault();
        }

        public List<InventorySection>? GetAllInventorySections(int inventoryId)
        {
            return _dbContext.InventorySections.Where(s => s.InventoryId == inventoryId).ToList();
        }


        public Inventory? GetUserInventoryByUserName(string userName)
        {
            return _dbContext.Inventories.FirstOrDefault(i => i.Name == userName);
        }

        public Inventory? GetUserInventoryByUserId(string userId)
        {
            return _dbContext.Inventories.FirstOrDefault(i => i.User.Id == userId);
        }

        public Inventory? AddAndSaveInventory(Inventory? inventory)
        {
            if (inventory is null)
                return inventory;

            _dbContext.Inventories.Add(inventory);
            _dbContext.SaveChanges(true);
            return inventory;

        }

        public Inventory? UpdateInventory(Inventory? inventory)
        {
            if (inventory is null)
                return inventory;

            _dbContext.Inventories.Update(inventory);
            _dbContext.SaveChanges(true);
            return inventory;
        }

        public InventorySection? UpdateSection(InventorySection? section)
        {
            if (section is null)
                return section;
            _dbContext.InventorySections.Update(section);
            _dbContext.SaveChanges(true);
            return section;
        }
    }
}
