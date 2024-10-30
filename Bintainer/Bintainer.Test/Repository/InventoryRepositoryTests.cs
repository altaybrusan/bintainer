using Bintainer.Model.Entity;
using Bintainer.Model;
using Bintainer.Repository.Service;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Bintainer.Test.Repository
{

    [TestFixture]
    public class InventoryRepositoryTests
    {
        private Mock<BintainerDbContext> _mockDbContext;
        private InventoryRepository _inventoryRepository;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<BintainerDbContext>();
            _inventoryRepository = new InventoryRepository(_mockDbContext.Object);
        }

        [Test]
        public void AddBin_ShouldAddBinToSection()
        {
            // Arrange
            var section = new InventorySection
            {
                Id = 1,
                Bins = new List<Bin>()
            };
            var bin = new Bin { Id = 1 };

            _mockDbContext.Setup(c => c.InventorySections.Update(It.IsAny<InventorySection>()));
            _mockDbContext.Setup(c => c.SaveChanges(true)).Returns(1);

            // Act
            var result = _inventoryRepository.AddBin(section, bin);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Bins, Has.Count.EqualTo(1));
            Assert.That(result.Bins.First().Id, Is.EqualTo(bin.Id));
            _mockDbContext.Verify(c => c.InventorySections.Update(section), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(true), Times.Once);
        }

        [Test]
        public void GetSection_ShouldReturnCorrectSection()
        {
            // Arrange
            
            var inventory = new Inventory()
            {
                Id = 1,
                Admin = "admin1",
                Name = "test inventory",
                UserId = "user1"
            };

            var sections = new List<InventorySection>
            {
                new() { Id = 1, InventoryId = 1, Inventory = inventory },
                new() { Id = 2, InventoryId = 1, Inventory = inventory }
            };

            _mockDbContext.Setup(c => c.InventorySections).Returns(sections.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = _inventoryRepository.GetSection("user1", 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void GetAllInventorySections_ShouldReturnAllSections()
        {
            // Arrange
            var sections = new List<InventorySection>
        {
            new InventorySection { Id = 1, InventoryId = 1 },
            new InventorySection { Id = 2, InventoryId = 1 }
        };

            _mockDbContext.Setup(c => c.InventorySections).Returns(sections.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = _inventoryRepository.GetAllInventorySections(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void GetUserInventoryByUserName_ShouldReturnCorrectInventory()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new() { Name = "user1" },
                new() { Name = "user2" }
            };

            _mockDbContext.Setup(c => c.Inventories).Returns(inventories.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = _inventoryRepository.GetUserInventoryByUserName("user1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("user1"));
        }

        [Test]
        public void GetUserInventoryByUserId_ShouldReturnCorrectInventory()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new() { User = new AspNetUser { Id = "user1" } },
                new() { User = new AspNetUser { Id = "user2" } }
            };

            _mockDbContext.Setup(c => c.Inventories).Returns(inventories.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = _inventoryRepository.GetUserInventoryByUserId("user1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.User.Id, Is.EqualTo("user1"));
        }

        [Test]
        public void AddAndSaveInventory_ShouldAddInventory()
        {
            // Arrange
            var inventory = new Inventory { Name = "New Inventory" };
            _mockDbContext.Setup(c => c.Inventories.Add(inventory));
            _mockDbContext.Setup(c => c.SaveChanges(true)).Returns(1);

            // Act
            var result = _inventoryRepository.AddAndSaveInventory(inventory);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Inventory"));
            _mockDbContext.Verify(c => c.Inventories.Add(inventory), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(true), Times.Once);
        }

        [Test]
        public void UpdateInventory_ShouldUpdateInventory()
        {
            // Arrange
            var inventory = new Inventory { Id = 1, Name = "Updated Inventory" };
            _mockDbContext.Setup(c => c.Inventories.Update(inventory));
            _mockDbContext.Setup(c => c.SaveChanges(true)).Returns(1);

            // Act
            var result = _inventoryRepository.UpdateInventory(inventory);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Inventory"));
            _mockDbContext.Verify(c => c.Inventories.Update(inventory), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(true), Times.Once);
        }

        [Test]
        public void UpdateSection_ShouldUpdateSection()
        {
            // Arrange
            var inventory = new Inventory()
            {
                Id = 1,
                Admin = "admin1",
                Name = "test inventory",
                UserId = "user1"
            };
            var section = new InventorySection { Id = 1, Inventory = inventory };
            _mockDbContext.Setup(c => c.InventorySections.Update(section));
            _mockDbContext.Setup(c => c.SaveChanges(true)).Returns(1);

            // Act
            var result = _inventoryRepository.UpdateSection(section);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            _mockDbContext.Verify(c => c.InventorySections.Update(section), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(true), Times.Once);
        }
    }

}
