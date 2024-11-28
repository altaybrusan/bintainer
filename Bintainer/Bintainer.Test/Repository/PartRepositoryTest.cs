using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;
using Bintainer.Model;
using Bintainer.Repository.Service;
using Bintainer.Model.Entity;
using Bintainer.Model.DTO;
using Bintainer.Repository;

namespace Bintainer.Test.Repository
{

    [TestFixture]
    public class PartRepositoryTests
    {
        private Mock<BintainerDbContext> _dbContextMock;
        private PartRepository _partRepository;

        [SetUp]
        public void SetUp()
        {
            _dbContextMock = new Mock<BintainerDbContext>();
            _partRepository = new PartRepository(_dbContextMock.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockDbSet;
        }

        [Test]
        public void GetPartByName_ReturnsPart_WhenPartExists()
        {
            // Arrange
            string partName = "TestPart";
            string userId = "user123";
            var expectedPart = new Part { Number = partName, UserId = userId };
            var parts = new List<Part> { expectedPart }.AsQueryable();

            var mockDbSet = CreateMockDbSet(parts);
            _dbContextMock.Setup(db => db.Parts).Returns(mockDbSet.Object);

            // Act
            var result = _partRepository.GetPart(partName, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPart.Number, result.Number);
            Assert.AreEqual(expectedPart.UserId, result.UserId);
        }

        [Test]
        public void GetPartById_ReturnsCorrectPart_WhenIdExists()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var expectedPart = new Part { GuidId = guid };
            var parts = new List<Part> { expectedPart }.AsQueryable();

            var mockDbSet = CreateMockDbSet(parts);
            _dbContextMock.Setup(db => db.Parts).Returns(mockDbSet.Object);

            // Act
            var result = _partRepository.GetPart(guid);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedPart.Id, result.Id);
        }

        [Test]
        public void GetPartsByCriteria_ReturnsParts_MatchingCriteria()
        {
            // Arrange
            var criteria = new PartFilterCriteria { Number = "TestPart", Supplier = "SupplierX", UserId = "user123" };
            var part1 = new Part { Number = "TestPart", Supplier = "SupplierX", UserId = "user123" };
            var part2 = new Part { Number = "OtherPart", Supplier = "SupplierX", UserId = "user123" };
            var parts = new List<Part> { part1, part2 }.AsQueryable();

            var mockDbSet = CreateMockDbSet(parts);
            _dbContextMock.Setup(db => db.Parts).Returns(mockDbSet.Object);

            // Act
            var result = _partRepository.GetParts(criteria);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(part1.Number, result[0].Number);
        }


        [Test]
        public void GetOrCreatePartPackage_ReturnsExistingPackage_WhenPackageExists()
        {
            // Arrange
            string packageName = "TestPackage";
            string userId = "user123";
            var existingPackage = new PartPackage { Name = packageName, UserId = userId };
            var packages = new List<PartPackage> { existingPackage }.AsQueryable();

            var mockDbSet = CreateMockDbSet(packages);
            _dbContextMock.Setup(db => db.PartPackages).Returns(mockDbSet.Object);

            // Act
            var result = _partRepository.GetOrCreatePackage(packageName, userId);

            // Assert
            Assert.AreEqual(existingPackage, result);
        }

        [Test]
        public void UpdatePart_SavesChanges_WhenPartIsNotNull()
        {
            // Arrange
            var part = new Part { Id = 1, Number = "UpdatedPart" };

            // Set up the DbSet to handle Update without requiring the object to be present
            var mockDbSet = new Mock<DbSet<Part>>();
            mockDbSet.Setup(db => db.Update(It.IsAny<Part>())).Verifiable();

            // Setup the DbContext mock to return this mocked DbSet
            _dbContextMock.Setup(db => db.Parts).Returns(mockDbSet.Object);

            // Mock SaveChanges to return a non-zero value
            _dbContextMock.Setup(db => db.SaveChanges(It.IsAny<bool>())).Returns(1);

            // Act
            var result = _partRepository.UpdatePart(part);

            // Assert
            _dbContextMock.Verify(db => db.Parts.Update(part), Times.Once);
            _dbContextMock.Verify(db => db.SaveChanges(true), Times.Once);
            Assert.AreEqual(part, result);
        }


        [Test]
        public void UpdatePartQuantityInsideSubspace_UpdatesQuantity_WhenAssociationExists()
        {
            // Arrange
            int partId = 1;
            int initialQuantity = 5;
            int partQuantity = 3;
            var association = new PartBinAssociation { PartId = partId, Quantity = initialQuantity };

            var subspace = new BinSubspace
            {
                PartBinAssociations = new List<PartBinAssociation> { association }
            };

            var mockDbSet = new Mock<DbSet<PartBinAssociation>>();
            _dbContextMock.Setup(db => db.PartBinAssociations).Returns(mockDbSet.Object);

            // Act
            _partRepository.UpdatePartQuantityInsideSubspace(subspace, partId, partQuantity);

            // Assert
            Assert.AreEqual(initialQuantity + partQuantity, association.Quantity);
            _dbContextMock.Verify(db => db.PartBinAssociations.Update(association), Times.Once);
            _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
        }

    }


}