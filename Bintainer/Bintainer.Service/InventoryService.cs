using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Bintainer.Service
{
    public class InventoryService : IInventoryService
    {

        IInventoryRepository _inventoryRepository;
        IBinService _binService;
        public InventoryService(IBinService binService, IInventoryRepository inventoryRepository)
        {
            _binService = binService;
            _inventoryRepository = inventoryRepository;
        }

        public InventorySection? GetInventorySection(string? userId, int sectionId)
        {
            return _inventoryRepository.GetSection(userId, sectionId);
        }
        public Bin? GetBinFrom(InventorySection? section, int coordinateX, int coordinateY) 
        {
            return section?.Bins.FirstOrDefault(b => b.CoordinateX == coordinateX &&
                                                     b.CoordinateY == coordinateY);
        }

        public Bin? CreateBin(InventorySection section, int coordinateX, int coordinateY)
        {
            Bin bin = new Bin() { CoordinateX = coordinateX, CoordinateY = coordinateY, SectionId = section.Id };
            _binService.SaveBin(bin);
            _inventoryRepository.AddBin(section, bin);
            return bin;
        }
        public List<InventorySection>? GetInventorySectionsOfUser(string admin) 
        {
            var inventory = _inventoryRepository.GetUserInventoryByUserName(admin);
            string? InventoryName = inventory?.Name;

            if (inventory != null)
            {
                return _inventoryRepository.GetAllInventorySections(inventory.Id);
            }
            var sections = new List<InventorySection>();
            sections.Add(new InventorySection() { Height = 1, Width = 1 });
            return sections;
        }
        public Inventory CreateOrUpdateInventory(UserViewModel user, string inventoryName) 
        {
           
            var inventory = _inventoryRepository.GetUserInventoryByUserName(user.Name);
            if (inventory == null)
            {               
                inventory = new Inventory() { Admin = user.Name, Name = inventoryName?.Trim(), UserId = user.UserId };
                inventory = _inventoryRepository.AddAndSaveInventory(inventory);
            }
            else
            {
                // the user already created an inventory
                if (inventory.Name != inventoryName)
                {
                    inventory.Name = inventoryName?.Trim();
                    _inventoryRepository.UpdateInventory(inventory);
                }
            }
            return inventory;
        }

        public Inventory? AddSectionsToInventory(List<InventorySection>? sectionList, Inventory inventory) 
        {

            if (sectionList is null) 
                return inventory;

            foreach (var section in sectionList)
            {
                if (section.Id == 0)
                {
                    section.InventoryId = inventory.Id;
                    inventory.InventorySections.Add(section);
                    
                }
                else
                {
                    //TODO: test if this section is correct. The updated sections should be stored in  inventory.
                    section.InventoryId = inventory.Id;
                    _inventoryRepository.UpdateSection(section);
                }
            }
            return inventory;
        }
    }
}
