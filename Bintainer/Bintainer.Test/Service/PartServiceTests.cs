using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using System.Linq;
using System;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using Bintainer.Service;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.Extensions.Logging;

namespace Bintainer.Test.Service
{


    [TestFixture]
    public class PartServiceTests
    {
        private Mock<IPartRepository> _partRepositoryMock;
        private Mock<ITemplateRepository> _templateRepositoryMock;
        private Mock<IInventoryRepository> _inventoryRepositoryMock;
        private Mock<IInventoryService> _inventoryServiceMock;
        private Mock<IBinService> _binServiceMock;
        private Mock<IStringLocalizer<ErrorMessages>> _localizerMock;
        private Mock<IAppLogger> _appLoggerMock;
        private PartService _partService;

        [SetUp]
        public void Setup()
        {
            _partRepositoryMock = new Mock<IPartRepository>();
            _templateRepositoryMock = new Mock<ITemplateRepository>();
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _inventoryServiceMock = new Mock<IInventoryService>();
            _binServiceMock = new Mock<IBinService>();
            _localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
            _appLoggerMock = new Mock<IAppLogger>();

            _partService = new PartService(
                _partRepositoryMock.Object,
                _templateRepositoryMock.Object,
                _inventoryRepositoryMock.Object,
                _inventoryServiceMock.Object,
                _binServiceMock.Object,
                _localizerMock.Object,
                _appLoggerMock.Object
            );
        }

        // Example 1: GetPartByName Test
        [Test]
        public void GetPartByName_ShouldReturnPart_WhenPartExists()
        {
            // Arrange
            var partName = "Part1";
            var userId = "user123";
            var part = new Part { Name = partName };
            _partRepositoryMock.Setup(repo => repo.GetPartByName(partName, userId)).Returns(part);

            // Act
            var response = _partService.GetPartByName(partName, userId);

            // Assert
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Result, Is.EqualTo(part));
            _appLoggerMock.Verify(log => log.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetPartByName_ShouldReturnError_WhenPartNotFound()
        {
            // Arrange
            var partName = "NonexistentPart";
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartByName(partName, userId)).Returns((Part)null);

            // Act
            var response = _partService.GetPartByName(partName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Null);
            });
            //Assert.That(response.Message, Is.EqualTo("Error loading part"));
            //_appLoggerMock.Verify(log => log.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()), Times.Once);
        }

        // Example 2: MapPartAttributesToViewModel Test
        [Test]
        public void MapPartAttributesToViewModel_ShouldReturnAttributes_WhenPartExists()
        {
            // Arrange
            var partName = "Part1";
            var userId = "user123";
            var part = new Part
            {
                Name = partName,
                AttributeTemplates = new List<PartAttributeTemplate>
                {
                new()
                {
                    PartAttributes = new List<PartAttribute>
                    {
                        new() { Name = "Attribute1", Value = "Value1" },
                        new() { Name = "Attribute2", Value = "Value2" }
                    }
                }
            }
            };
            _partRepositoryMock.Setup(repo => repo.GetPartByName(partName, userId)).Returns(part);

            // Act
            var response = _partService.MapPartAttributesToViewModel(partName, userId);

            // Assert
            Assert.Multiple(() =>
            {                
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Not.Null);
                Assert.That(response.Result, Has.Count.EqualTo(2));
                Assert.That(actual: response.Result[0].Name, Is.EqualTo("Attribute1"));
                Assert.That(response.Result[0].Value, Is.EqualTo("Value1"));
            });
        }

        // Example 3: CreatePartForUser Test
        [Test]
        public void CreatePartForUser_ShouldReturnError_WhenPartAlreadyExists()
        {
            // Arrange
            var request = new CreatePartRequest { PartName = "ExistingPart" };
            var userId = "user123";
            var existingParts = new List<Part> { new() { Name = "ExistingPart" } };
            _partRepositoryMock.Setup(repo => repo.GetPartsByCriteria(It.IsAny<PartFilterCriteria>())).Returns(existingParts);

            var localizedErrorMessage = "Part already exists";  // expected message
            _localizerMock.Setup(l => l["ErrorPartAlreadyExists"]).Returns(new LocalizedString("ErrorPartAlreadyExists", localizedErrorMessage));

            // Act
            var response = _partService.CreatePartForUser(request, userId);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            Assert.That(response.Message, Is.EqualTo(localizedErrorMessage));
        }

        [Test]
        public void CreatePartForUser_ShouldCreatePart_WhenPartDoesNotExist()
        {
            // Arrange
            var request = new CreatePartRequest { PartName = "NewPart", Supplier = "Supplier1", Description= "Desc1", Group = "group 1" ,Attributes = new Dictionary<string, string>() };
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartsByCriteria(It.IsAny<PartFilterCriteria>())).Returns((List<Part>)null);

            // Act
            var response = _partService.CreatePartForUser(request, userId);

            // Assert
            Assert.Multiple(() =>
            {                
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Not.Null);
            });
            Assert.That(response.Result!.Name, Is.EqualTo(request.PartName));
        }

        // Example 4: ParseSubspaceIndices Test
        [Test]
        public void ParseSubspaceIndices_ShouldReturnParsedIndices()
        {
            // Arrange
            var commaSeparatedIndices = "1,2,3";

            // Act
            var result = _partService.ParseSubspaceIndices(commaSeparatedIndices);

            // Assert
            Assert.That(result, Is.EqualTo(new List<int> { 1, 2, 3 }));
        }
    }

}
