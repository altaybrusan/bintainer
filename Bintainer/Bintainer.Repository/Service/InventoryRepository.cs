using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using Microsoft.EntityFrameworkCore;
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
            return _dbContext.InventorySections
                             .Include(x=>x.Bins)
                             .ThenInclude(b=>b.BinSubspaces)
                             .ThenInclude(b=>b.PartBinAssociations)                             
                             .Where(i => i.Inventory.UserId == userId && i.Id == sectionId)
                             .FirstOrDefault();
        }

        public InventorySection? GetSection(string userId, string sectionName)
        {
            return _dbContext.InventorySections
                             .Include(x => x.Bins)
                             .Where(i => i.Inventory.UserId == userId && i.SectionName == sectionName)
                             .FirstOrDefault();
        }

        public List<InventorySection>? GetAllInventorySections(int inventoryId)
        {
            return _dbContext.InventorySections.Where(s => s.InventoryId == inventoryId).ToList();
        }

        public Inventory? GetInventory(string? admin)
        {
            return _dbContext.Inventories.Where(i => i.Admin == admin)
                                         .Include(i => i.InventorySections)
                                            .ThenInclude(i=>i.Bins)
                                                .ThenInclude(b=>b.PartBinAssociations)
                                                .ThenInclude(b=>b.Subspace)
                                         .Include(i=>i.User)
                                         .FirstOrDefault();
        }

        public Inventory? GetInventoryById(string? userId)
        {
            return _dbContext.Inventories.Where(i => i.UserId == userId)
                                         .Include(i => i.InventorySections)
                                         .Include(i => i.User)
                                         .FirstOrDefault();
        }

        public Inventory? GetUserInventoryByUserId(string userId)
        {
            return _dbContext.Inventories.FirstOrDefault(i => i.User.Id == userId);
        }

        private Inventory? AddAndSaveInventory(Inventory? inventory)
        {
            if (inventory is null)
                return inventory;

            _dbContext.Inventories.Add(inventory);
            _dbContext.SaveChanges(true);
            return inventory;

        }

        public Inventory? CreateOrUpdateInventory(Inventory? requestModel)
        {
            
            if (requestModel is null)
                return requestModel;

            Inventory? existingInventory = GetInventory(requestModel.User!.UserName);
            
            if (existingInventory is null)
            {
                existingInventory = new Inventory()
                {
                    Admin = requestModel.User.UserName,
                    Name = requestModel.Name?.Trim(),
                    User = requestModel.User,
                    UserId = requestModel.UserId
                };
                existingInventory = AddAndSaveInventory(existingInventory);
            }

            if (existingInventory == null)
                return null;

            if(existingInventory.Name?.Trim() != requestModel.Name?.Trim())
                existingInventory.Name = requestModel.Name;
            
            // Find sections that exist in the database but not in the incoming inventory
            var sectionIds = requestModel.InventorySections.Select(s => s.Id).ToHashSet();
            var sectionsToRemove = existingInventory.InventorySections
                .Where(s => !sectionIds.Contains(s.Id))
                .ToList();

            // Remove sections that are not in the incoming list
            foreach (var section in sectionsToRemove)
            {
                RemoveAllBinsInsideSection(section);
                _dbContext.InventorySections.Remove(section);
                existingInventory.InventorySections.Remove(section);
            }

            _dbContext.SaveChanges(true);


            // Update other properties and sections as needed
            foreach (var section in requestModel.InventorySections)
            {
                var existingSection = existingInventory.InventorySections
                    .FirstOrDefault(s => s.Id == section.Id && s.SectionName.Contains(section.SectionName));

                if (existingSection != null)
                {
                    // Update the existing section
                    existingSection.Width = section.Width;
                    existingSection.Height = section.Height;
                    existingSection.SectionName = section.SectionName?.Trim();
                    existingSection.SubspaceCount = section.SubspaceCount;
                }
                else
                {
                    // Add new sections
                    existingInventory.InventorySections.Add(section);
                }
            }

            _dbContext.Inventories.Update(existingInventory);
            _dbContext.SaveChanges(true);

            return existingInventory;
        }

        public InventorySection? UpdateSection(InventorySection? section)
        {
            if (section is null)
                return section;
            _dbContext.InventorySections.Update(section);
            _dbContext.SaveChanges(true);
            return section;
        }


        private List<Bin> RemoveAllBinsInsideSection(InventorySection section)
        {

            var binIds = section.Bins.Select(b => b.Id).ToHashSet();

            var assocs = _dbContext.PartBinAssociations.Where(a => binIds.Contains(a.BinId)).ToList();
            _dbContext.PartBinAssociations.RemoveRange(assocs);


            var subspaces = _dbContext.BinSubspaces.Where(b => binIds.Contains(b.BinId!.Value)).ToList();
            _dbContext.BinSubspaces.RemoveRange(subspaces);


            //_dbContext.SaveChanges(true);

            List<Bin> result = section.Bins.ToList();
            _dbContext.Bins.RemoveRange(result);
            _dbContext.SaveChanges(true);

            return result;
        }

    }
}
