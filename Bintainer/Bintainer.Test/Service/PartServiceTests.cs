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
using Bintainer.Model.Template;

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
        private InventoryService _inventoryService;


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

            _inventoryService = new InventoryService(_binServiceMock.Object, _inventoryRepositoryMock.Object, _localizerMock.Object, _appLoggerMock.Object);
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
                Assert.That(response.Result[0].Name, Is.EqualTo("Attribute1"));
                Assert.That(response.Result[0].Value, Is.EqualTo("Value1"));
            });
        }

        [Test]
        public void MapPartAttributesToViewModel_ShouldReturnError_WhenPartNotFound()
        {
            // Arrange
            var partName = "NonexistentPart";
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartByName(partName, userId)).Throws(new Exception("Error loading part attributes"));

            // Act
            var response = _partService.MapPartAttributesToViewModel(partName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Error loading part attributes"));
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
            var request = new CreatePartRequest { PartName = "NewPart", Supplier = "Supplier1", Description = "Desc1", Group = "group 1", Attributes = new Dictionary<string, string>() };
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartsByCriteria(It.IsAny<PartFilterCriteria>())).Returns(new List<Part>());

            // Act
            var response = _partService.CreatePartForUser(request, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Not.Null);
                Assert.That(response.Result.Name, Is.EqualTo(request.PartName));
            });
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

        [Test]
        public void ParseSubspaceIndices_ShouldReturnEmptyList_WhenInputIsEmpty()
        {
            // Arrange
            var commaSeparatedIndices = "";

            // Act
            var result = _partService.ParseSubspaceIndices(commaSeparatedIndices);

            // Assert
            Assert.That(result, Is.EqualTo(new List<int>()));
        }

        [Test]
        public void ParseSubspaceIndices_ShouldThrowFormatException_WhenInputIsInvalid()
        {
            // Arrange
            var commaSeparatedIndices = "1,a,3";

            // Act & Assert
            Assert.Throws<FormatException>(() => _partService.ParseSubspaceIndices(commaSeparatedIndices));
        }

        // Additional Tests for Catch Phrases
        [Test]
        public void CreatePartForUser_ShouldLogError_WhenCreatingPartFails()
        {
            // Arrange
            var request = new CreatePartRequest { PartName = "NewPart", Description = "Desc" };
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartsByCriteria(It.IsAny<PartFilterCriteria>())).Returns(new List<Part>());
            //_partRepositoryMock.Setup(repo => repo.AddPart(It.IsAny<Part>())).Throws(new Exception("Database error"));

            // Act
            var response = _partService.CreatePartForUser(request, userId);

            // Assert
            Assert.IsFalse(response.IsSuccess);
            _appLoggerMock.Verify(log => log.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void AddPartIntoGroup_ShouldReturnGroup_WhenPartAlreadyExistsInGroup()
        {
            // Arrange
            var part = new Part { Name = "Part1" };
            var groupName = "Group1";
            var userId = "user123";
            var existingGroup = new PartGroup { Name = groupName, UserId = userId, Parts = new List<Part> { part } };

            _partRepositoryMock.Setup(repo => repo.GetPartGroup(groupName, userId)).Returns(existingGroup);

            // Act
            var response = _partService.AddPartIntoGroup(part, groupName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.EqualTo(existingGroup));
            });
        }

        // Test case: Adding a part to an existing group that does not contain the part
        [Test]
        public void AddPartIntoGroup_ShouldAddPart_WhenPartDoesNotExistInGroup()
        {
            // Arrange
            var part = new Part { Name = "Part1" };
            var groupName = "Group1";
            var userId = "user123";
            var existingGroup = new PartGroup { Name = groupName, UserId = userId, Parts = new List<Part>() };

            _partRepositoryMock.Setup(repo => repo.GetPartGroup(groupName, userId)).Returns(existingGroup);

            // Act
            var response = _partService.AddPartIntoGroup(part, groupName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result.Parts, Contains.Item(part));
            });
        }

        // Test case: Creating a new group and adding the part when the group does not exist
        [Test]
        public void AddPartIntoGroup_ShouldCreateNewGroup_WhenGroupDoesNotExist()
        {
            // Arrange
            var part = new Part { Name = "Part1" };
            var groupName = "Group1";
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartGroup(groupName, userId)).Returns((PartGroup)null);

            // Act
            var response = _partService.AddPartIntoGroup(part, groupName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result.Name, Is.EqualTo(groupName));
                Assert.That(response.Result.Parts, Contains.Item(part));
            });
        }

        // Test case: Handling exceptions thrown during execution
        [Test]
        public void AddPartIntoGroup_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            var part = new Part { Name = "Part1" };
            var groupName = "Group1";
            var userId = "user123";
            _partRepositoryMock.Setup(repo => repo.GetPartGroup(groupName, userId)).Throws(new Exception("Database error"));

            // Act
            var response = _partService.AddPartIntoGroup(part, groupName, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Database error"));
            });
        }

        // Test case: Quantity used exceeds available quantity
        [Test]
        public void TryAdjustPartQuantity_ShouldLogError_WhenQuantityUsedExceedsAvailable()
        {
            // Arrange
            var request = new AdjustQuantityRequest { Quantity = 5, QuantityUsed = 6 };
            var userId = "user123";

            // Act
            _partService.TryAdjustPartQuantity(request, userId);

            // Assert
            _appLoggerMock.Verify(logger => logger.LogMessage(It.IsAny<string>(), LogLevel.Error,It.IsAny<string>()), Times.Once);
        }

        // Test case: Part not found
        [Test]
        public void TryAdjustPartQuantity_ShouldReturnWarning_WhenPartNotFound()
        {
            // Arrange
            var request = new AdjustQuantityRequest { Quantity = 5, QuantityUsed = 3, PartName = "Part1", BinId = 1, SubspaceIndices = "1,2" };
            var userId = "user123";
            LocalizedString localizedString = new LocalizedString("WarningPartNotFound", "Part not found.");

            _partRepositoryMock.Setup(repo => repo.GetPartByName(request.PartName, userId)).Returns((Part)null);
            _localizerMock.Setup(localizer => localizer["WarningPartNotFound"]).Returns(localizedString);

            // Act
            var response = _partService.TryAdjustPartQuantity(request, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Null);
                Assert.That(response.Message, Is.EqualTo("Part not found."));
            });
        }

        // Test case: Adjusting quantity successfully
        [Test]
        public void TryAdjustPartQuantity_ShouldAdjustQuantities_WhenPartFoundAndQuantityIsValid()
        {
            // Arrange
            var request = new AdjustQuantityRequest { Quantity = 5, QuantityUsed = 3, PartName = "Part1", BinId = 1, SubspaceIndices = "0" };
            var userId = "user123";

            var part = new Part
            {
                PartBinAssociations = new List<PartBinAssociation>
            {
                new PartBinAssociation { BinId = request.BinId, Quantity = 5, Subspace = new BinSubspace { SubspaceIndex = 0 } }
            }
            };

            _partRepositoryMock.Setup(repo => repo.GetPartByName(request.PartName, userId)).Returns(part);
            _partRepositoryMock.Setup(repo => repo.UpdatePartBinassociations(It.IsAny<List<PartBinAssociation>>())).Returns((List<PartBinAssociation>?)part.PartBinAssociations);

            // Act
            var response = _partService.TryAdjustPartQuantity(request, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Result, Is.Not.Null);
                Assert.That(response.Result.First().Quantity, Is.EqualTo(2)); // 5 - 3 = 2
            });
        }

        // Test case: Exception handling
        [Test]
        public void TryAdjustPartQuantity_ShouldLogErrorAndReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            var request = new AdjustQuantityRequest { Quantity = 5, QuantityUsed = 3, PartName = "Part1", BinId = 1, SubspaceIndices = "0" };
            var userId = "user123";

            _partRepositoryMock.Setup(repo => repo.GetPartByName(request.PartName, userId)).Throws(new Exception("Database error"));
            _appLoggerMock.Setup(logger => logger.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()));

            // Act
            var response = _partService.TryAdjustPartQuantity(request, userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Database error"));
                Assert.That(response.Result, Is.Null);
            });
        }


        // Test case: Inventory section missing
        [Test]
        public void ArrangePartRequest_ShouldLogError_WhenInventorySectionIsMissing()
        {
            // Arrange
            var arrangeRequest = new ArrangePartRequest { SectionId = 1, PartName = "Part1", CoordinateX = 0, CoordinateY = 0 };
            var userId = "user123";

            _inventoryRepositoryMock.Setup(repo => repo.GetSection(userId, arrangeRequest.SectionId)).Returns((InventorySection)null);

            // Act
            _partService.ArrangePartRequest(arrangeRequest, userId);

            // Assert

            _appLoggerMock.Verify(logger => logger.LogMessage("ErrorInventorySectionMissing", It.IsAny<LogLevel>(),It.IsAny<string>()), Times.Once);
        }

        // Test case: Part not found
        [Test]
        public void ArrangePartRequest_ShouldLogError_WhenPartNotFound()
        {
            // Arrange
            var arrangeRequest = new ArrangePartRequest { SectionId = 1, PartName = "Part1", CoordinateX = 0, CoordinateY = 0 };
            var userId = "user123";
            var inventorySection = new InventorySection();

            _inventoryRepositoryMock.Setup(repo => repo.GetSection(userId, arrangeRequest.SectionId)).Returns(inventorySection);
            _partRepositoryMock.Setup(repo => repo.GetPartByName(arrangeRequest.PartName, userId)).Returns((Part)null);

            // Act
            _partService.ArrangePartRequest(arrangeRequest, userId);

            // Assert
            _appLoggerMock.Verify(logger => logger.LogMessage(It.IsAny<string>(),It.IsAny<LogLevel>(),It.IsAny<string>()), Times.Once);
        }

        // Test case: Existing bin found
        [Test]
        public void ArrangePartRequest_ShouldProcessArrangeRequest_WhenBinFound()
        {
            // Arrange
            var arrangeRequest = new ArrangePartRequest { SectionId = 1, PartName = "Part1", CoordinateX = 0, CoordinateY = 0 };
            var userId = "user123";
            var inventorySection = new InventorySection();
            var part = new Part();
            var bin = new Bin();

            _inventoryRepositoryMock.Setup(repo => repo.GetSection(userId, arrangeRequest.SectionId)).Returns(inventorySection);
            _partRepositoryMock.Setup(repo => repo.GetPartByName(arrangeRequest.PartName, userId)).Returns(part);
            _inventoryServiceMock.Setup(service => service.GetBinFrom(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY))
                .Returns(new Model.Template.Response<Bin> { IsSuccess = true, Result = bin });

            // Act
            _partService.ArrangePartRequest(arrangeRequest, userId);

            // Assert
            // You would need to verify that ProcessArrangePartRequest was called with the correct parameters
            // This requires you to expose ProcessArrangePartRequest as a virtual method or use another technique to verify.
        }

        // Test case: Bin not found and new bin created
        [Test]
        public void ArrangePartRequest_ShouldCreateNewBin_WhenBinNotFound()
        {
            // Arrange
            var arrangeRequest = new ArrangePartRequest { SectionId = 1, PartName = "Part1", CoordinateX = 0, CoordinateY = 0 };
            var userId = "user123";
            var inventorySection = new InventorySection();
            var part = new Part();
            var newBin = new Bin();

            _inventoryRepositoryMock.Setup(repo => repo.GetSection(userId, arrangeRequest.SectionId)).Returns(inventorySection);
            _partRepositoryMock.Setup(repo => repo.GetPartByName(arrangeRequest.PartName, userId)).Returns(part);
            _inventoryServiceMock.Setup(service => service.GetBinFrom(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY))
                .Returns(new Response<Bin> { IsSuccess = false, Message = "Bin not found" });

            _inventoryServiceMock.Setup(service => service.CreateBin(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY))
                .Returns(new Response<Bin> { IsSuccess = true, Result = newBin });

            // Act
            _partService.ArrangePartRequest(arrangeRequest, userId);

            // Assert
            // Again, verify that ProcessArrangePartRequest was called with the correct parameters
        }

        // Test case: Exception handling
        [Test]
        public void ArrangePartRequest_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var arrangeRequest = new ArrangePartRequest { SectionId = 1, PartName = "Part1", CoordinateX = 0, CoordinateY = 0 };
            var userId = "user123";
            var inventorySection = new InventorySection();
            var part = new Part();

            _inventoryRepositoryMock.Setup(repo => repo.GetSection(userId, arrangeRequest.SectionId)).Returns(inventorySection);
            _partRepositoryMock.Setup(repo => repo.GetPartByName(arrangeRequest.PartName, userId)).Returns(part);
            _inventoryServiceMock.Setup(service => service.GetBinFrom(inventorySection, arrangeRequest.CoordinateX, arrangeRequest.CoordinateY))
                .Throws(new Exception("Database error"));

            // Act
            _partService.ArrangePartRequest(arrangeRequest, userId);

            // Assert
            _appLoggerMock.Verify(logger => logger.LogMessage("Database error", LogLevel.Error,It.IsAny<string>()), Times.Once);
        }
    }


}
