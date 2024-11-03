using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using Bintainer.Service;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Bintainer.Test.Service
{


    [TestFixture]
    public class InventoryServiceTests
    {
        private Mock<IInventoryRepository> _mockInventoryRepository;
        private Mock<IBinService> _mockBinService;
        private Mock<IStringLocalizer<ErrorMessages>> _mockLocalizer;
        private Mock<IAppLogger> _mockAppLogger;
        private InventoryService _inventoryService;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void SetUp()
        {
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockBinService = new Mock<IBinService>();
            _mockLocalizer = new Mock<IStringLocalizer<ErrorMessages>>();
            _mockAppLogger = new Mock<IAppLogger>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
                        
            _inventoryService = new InventoryService(
                _mockBinService.Object,
                _mockInventoryRepository.Object,                 
                _mockLocalizer.Object,
                _mockMapper.Object,
                _mockAppLogger.Object,
                _mockUserRepository.Object
            );
        }

        [Test]
        public void GetInventorySection_WhenSectionExists_ReturnsSection()
        {
            // Arrange
            var sectionId = 1;
            var userId = "user123";
            var section = new InventorySection { Id = sectionId };
            _mockInventoryRepository.Setup(r => r.GetSection(userId, sectionId)).Returns(section);

            // Act
            var result = _inventoryService.GetInventorySection(userId, sectionId);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(section, result.Result);
        }

        [Test]
        public void GetInventorySection_WhenExceptionOccurs_ReturnsFailureResponse()
        {
            // Arrange
            var sectionId = 1;
            var userId = "user123";
            _mockInventoryRepository.Setup(r => r.GetSection(userId, sectionId)).Throws(new Exception("Error"));

            // Act
            var result = _inventoryService.GetInventorySection(userId, sectionId);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            _mockAppLogger.Verify(logger =>
            logger.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()), Times.Once);

        }

        [Test]
        public void GetBinFrom_WhenBinExists_ReturnsBin()
        {
            // Arrange
            var coordinateX = 10;
            var coordinateY = 20;
            var section = new InventorySection
            {
                Bins = new List<Bin> { new Bin { CoordinateX = coordinateX, CoordinateY = coordinateY } }
            };

            // Act
            var result = _inventoryService.GetBinFrom(section, coordinateX, coordinateY);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(coordinateX, result.Result?.CoordinateX);
            Assert.AreEqual(coordinateY, result.Result?.CoordinateY);
        }

        [Test]
        public void CreateBin_WhenCalled_CreatesAndSavesBin()
        {
            // Arrange
            var section = new InventorySection { Id = 1 };
            var coordinateX = 5;
            var coordinateY = 10;

            // Act
            var result = _inventoryService.CreateBin(section, coordinateX, coordinateY);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(coordinateX, result.Result?.CoordinateX);
            Assert.AreEqual(coordinateY, result.Result?.CoordinateY);
            _mockBinService.Verify(b => b.SaveBin(It.IsAny<Bin>()), Times.Once);
            _mockInventoryRepository.Verify(r => r.AddBin(section, It.IsAny<Bin>()), Times.Once);
        }

        [Test]
        public void CreateOrUpdateInventory_WhenNewInventory_ReturnsCreatedInventory()
        {
            // Arrange
            var user = new UserViewModel { Name = "user1", UserId = "userId1" };
            var inventoryName = "New Inventory";
            var sectionList = new List<InventorySection>(); // Example section list (can be empty or populated)

            var mockUser = new AspNetUser { Id = user.UserId, UserName = user.Name };

            _mockUserRepository.Setup(r => r.GetUser(user.UserId)).Returns(mockUser);
            _mockInventoryRepository.Setup(r => r.CreateOrUpdateInventory(It.IsAny<Inventory>()));

            // Act
            var result = _inventoryService.CreateOrUpdateInventory(user, inventoryName, sectionList);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True, "Expected IsSuccess to be true.");
                Assert.That(result.Result?.Name, Is.EqualTo(inventoryName), "Expected inventory name to match.");
                Assert.That(result.Result?.User, Is.EqualTo(mockUser), "Expected the User to be correctly assigned.");
            });

            // Verify that the CreateOrUpdateInventory method was called
            _mockInventoryRepository.Verify(r => r.CreateOrUpdateInventory(It.IsAny<Inventory>()), Times.Once);
        }

    }

}
