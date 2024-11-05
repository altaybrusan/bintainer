using Bintainer.Model.Entity;
using Bintainer.Model;
using Bintainer.Repository.Service;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Bintainer.Repository;

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
            var user = new AspNetUser { Id = "userId1", UserName = "user1" };
            var inventorySections = new List<InventorySection>
            {
                new InventorySection { Id = 1, SectionName = "Section 1" },
                new InventorySection { Id = 2, SectionName = "Section 2" }
            };

            var inventories = new List<Inventory>
            {
                new Inventory
                {
                    Name = "Inventory1",
                    Admin = "user1",
                    User = user,
                    InventorySections = inventorySections
                },
                new Inventory
                {
                    Name = "Inventory2",
                    Admin = "user2",
                    User = new AspNetUser { Id = "userId2", UserName = "user2" },
                    InventorySections = new List<InventorySection>()
                }
            };

            // Mock the DbSet with the inventories
            var mockDbSet = inventories.AsQueryable().BuildMockDbSet();

            // Set up the mock DbContext to return the mock DbSet
            _mockDbContext.Setup(c => c.Inventories).Returns(mockDbSet.Object);

            // Act
            var result = _inventoryRepository.GetInventory("user1");

            // Assert
            Assert.That(result, Is.Not.Null, "Expected inventory to be found.");
            Assert.That(result.Admin, Is.EqualTo("user1"), "Expected Admin to match.");
            Assert.That(result.User, Is.Not.Null, "Expected User to be included.");
            Assert.That(result.User.UserName, Is.EqualTo("user1"), "Expected User name to match.");
            Assert.That(result.InventorySections, Is.Not.Empty, "Expected InventorySections to be included.");
            Assert.That(result.InventorySections.Count, Is.EqualTo(2), "Expected two InventorySections.");
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
        public void UpdateInventory_ShouldUpdateInventory()
        {
            // Arrange
            var user = new AspNetUser { Id = "userId1", UserName = "user1" };
            var existingInventory = new Inventory
            {
                Id = 1,
                Name = "Old Inventory",
                Admin = "user1",
                User = user,
                UserId = user.Id,
                InventorySections = new List<InventorySection>
                {
                    new() { Id = 1, SectionName = "Section 1", Width = 10, Height = 5, SubspaceCount = 2 },
                    new() { Id = 2, SectionName = "Section 2", Width = 20, Height = 10, SubspaceCount = 4 }
                }
            };

            var updatedInventory = new Inventory
            {
                Id = 1,
                Name = "Updated Inventory",
                User = user,
                UserId = user.Id,
                InventorySections = new List<InventorySection>
                {
                    new() { Id = 1, SectionName = "Updated Section 1", Width = 15, Height = 7, SubspaceCount = 3 },
                    new() { Id = 3, SectionName = "New Section", Width = 25, Height = 12, SubspaceCount = 5 }
                }
            };

            _mockDbContext.Setup(c => c.Inventories)
                .Returns(new List<Inventory> { existingInventory }.AsQueryable().BuildMockDbSet().Object);
            _mockDbContext.Setup(c => c.InventorySections)
                .Returns(existingInventory.InventorySections.AsQueryable().BuildMockDbSet().Object);
            _mockDbContext.Setup(c => c.SaveChanges(true)).Returns(1);

            // Act
            var result = _inventoryRepository.CreateOrUpdateInventory(updatedInventory);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Inventory"));

            // Verify that the existing section was updated
            var updatedSection = result.InventorySections.FirstOrDefault(s => s.Id == 1);
            Assert.That(updatedSection, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedSection.Width, Is.EqualTo(10));
                Assert.That(updatedSection.Height, Is.EqualTo(5));
                Assert.That(updatedSection.SectionName, Is.EqualTo("Section 1"));
            });

            // Verify that the new section was added
            var newSection = result.InventorySections.FirstOrDefault(s => s.Id == 3);
            Assert.That(newSection, Is.Not.Null);
            Assert.That(newSection.SectionName, Is.EqualTo("New Section"));

            // Verify that the removed section was deleted
            _mockDbContext.Verify(c => c.InventorySections.Remove(It.Is<InventorySection>(s => s.Id == 2)), Times.Once);

            // Verify that the inventory was updated and saved
            _mockDbContext.Verify(c => c.Inventories.Update(It.Is<Inventory>(i => i.Name == "Updated Inventory")), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(true), Times.AtLeast(1));
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
