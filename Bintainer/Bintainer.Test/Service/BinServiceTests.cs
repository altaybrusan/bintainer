using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Localization; // Assuming you're using Microsoft.Extensions.Localization
using System;
using System.Collections.Generic;
using Bintainer.Model.Entity;
using Bintainer.Repository.Interface;
using Bintainer.Service;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using static System.Collections.Specialized.BitVector32;
using Microsoft.Extensions.Logging;

namespace Bintainer.Test.Service
{
 
    [TestFixture]
    public class BinServiceTests
    {
        private Mock<IBinRepository> _mockBinRepository;
        private Mock<IInventoryRepository> _mockInventoryRepository;
        private Mock<IStringLocalizer<ErrorMessages>> _mockLocalizer;
        private Mock<IAppLogger> _mockAppLogger;
        private BinService _binService;

        [SetUp]
        public void SetUp()
        {
            _mockBinRepository = new Mock<IBinRepository>();
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockLocalizer = new Mock<IStringLocalizer<ErrorMessages>>();
            _mockAppLogger = new Mock<IAppLogger>();

            _binService = new BinService(
                _mockBinRepository.Object,
                _mockInventoryRepository.Object,
                _mockLocalizer.Object,
                _mockAppLogger.Object);
        }

        [Test]
        public void UpdateBin_ShouldReturnSuccess_WhenBinIsUpdated()
        {
            // Arrange
            var bin = new Bin(); // Create a bin instance as needed
            _mockBinRepository.Setup(repo => repo.UpdateBin(bin)).Verifiable();

            // Act
            var result = _binService.UpdateBin(bin);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(bin, result.Result);
            _mockBinRepository.Verify(repo => repo.UpdateBin(bin), Times.Once);
        }

        [Test]
        public void UpdateBin_ShouldReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            var bin = new Bin(); // Create a bin instance as needed
            var exceptionMessage = "An error occurred";
            _mockBinRepository.Setup(repo => repo.UpdateBin(bin)).Throws(new Exception(exceptionMessage));

            // Act
            var result = _binService.UpdateBin(bin);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo(exceptionMessage));
            _mockAppLogger.Verify(logger => logger.LogMessage(exceptionMessage, LogLevel.Error, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void SaveBin_ShouldReturnSuccess_WhenBinIsSaved()
        {
            // Arrange
            var bin = new Bin(); // Create a bin instance as needed
            _mockBinRepository.Setup(repo => repo.SaveBin(bin)).Verifiable();

            // Act
            var result = _binService.SaveBin(bin);
            
            // Assert
            Assert.Multiple(() =>
            {                
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(bin));
            });
            _mockBinRepository.Verify(repo => repo.SaveBin(bin), Times.Once);
        }

        [Test]
        public void SaveBin_ShouldReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            var bin = new Bin(); // Create a bin instance as needed
            var exceptionMessage = "An error occurred";
            _mockBinRepository.Setup(repo => repo.SaveBin(bin)).Throws(new Exception(exceptionMessage));

            // Act
            var result = _binService.SaveBin(bin);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo(exceptionMessage));
            _mockAppLogger.Verify(logger => logger.LogMessage(exceptionMessage, LogLevel.Error, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void DistributeQuantityAcrossSubspaces_ShouldReturnSuccess_WhenValidInput()
        {
            // Arrange
            var bin = new Bin
            {
                Id = 1,
                CoordinateX = 1,
                CoordinateY = 1,
                Section = new InventorySection
                {
                    Id = 1,
                    SectionName = "test section",
                    SubspaceCount = 3 // Set SubspaceCount to 3 for distribution
                },
                BinSubspaces = new List<BinSubspace>
        {
            new() { BinId = 1, Label = "label1" },
            new() { BinId = 1, Label = "label2" },
            new() { BinId = 1, Label = "label3" }
        }
            };

            int totalQuantity = 10;

            // Act
            var result = _binService.DistributeQuantityAcrossSubspaces(bin, totalQuantity);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.Not.Null);
            });
            Assert.That(result.Result, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(result.Result[1], Is.EqualTo(4)); // First subspace should get the base division + 1
                Assert.That(result.Result[2], Is.EqualTo(3)); // Second subspace
                Assert.That(result.Result[3], Is.EqualTo(3)); // Third subspace
            });
        }

        [Test]
        public void DistributeQuantityAcrossSubspaces_ShouldReturnFailure_WhenBinIsNull()
        {
            // Arrange
            var localizedString = new LocalizedString("ErrorBinOrSectionUnavailable", "The bin or section is unavailable.");
            _mockLocalizer.Setup(l => l["ErrorBinOrSectionUnavailable"]).Returns(localizedString);

            // Act
            var result = _binService.DistributeQuantityAcrossSubspaces(null, 10);

            // Assert
            Assert.Multiple(() =>
            {                
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("The bin or section is unavailable."));
            });
        }

        [Test]
        public void DistributeQuantityAcrossSubspaces_ShouldReturnFailure_WhenZeroSubspaceCount()
        {
            // Arrange
            var localizedStringZeroSubspaceCount = new LocalizedString("ErrorZeroSubspaceCount", "There are zero subspaces in {0}.");
            _mockLocalizer.Setup(l => l["ErrorZeroSubspaceCount"]).Returns(localizedStringZeroSubspaceCount);

            var bin = new Bin
            {
                Id = 1,
                CoordinateX = 1,
                CoordinateY = 1,
                Section = new InventorySection
                {
                    Id = 1,
                    SectionName = "test section",
                    SubspaceCount = 0 // Set SubspaceCount to 0 to trigger the error
                },
                BinSubspaces = new List<BinSubspace>()
        {
            new()
            {
                BinId = 1,
                Label = "label1"
            }
        }
            };

            // Act
            var result = _binService.DistributeQuantityAcrossSubspaces(bin, 10);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(string.Format("There are zero subspaces in {0}.", bin.Section.SectionName), result.Message);
        }

    }

}
