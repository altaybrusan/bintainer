using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using AutoMapper;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Bintainer.Model.DTO;

namespace Bintainer.Test.Service
{


    [TestFixture]
    public class TemplateServiceTests
    {
        private Mock<ITemplateRepository> _templateRepositoryMock;
        private Mock<IAppLogger> _appLoggerMock;
        private Mock<IStringLocalizer<ErrorMessages>> _localizerMock;
        private Mock<IMapper> _mapperMock;
        private TemplateService _templateService;

        [SetUp]
        public void Setup()
        {
            _templateRepositoryMock = new Mock<ITemplateRepository>();
            _appLoggerMock = new Mock<IAppLogger>();
            _localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
            _mapperMock = new Mock<IMapper>();

            _templateService = new TemplateService(
                _templateRepositoryMock.Object,
                _appLoggerMock.Object,
                _localizerMock.Object,
                _mapperMock.Object
            );
        }

          // Test GetTemplateByUserId
        [Test]
        public void GetTemplateByUserId_ShouldReturnTemplates_WhenUserExists()
        {
            // Arrange
            var userId = "user123";
            var templates = new Dictionary<int, string?> { { 1, "Template1" } };
            _templateRepositoryMock.Setup(repo => repo.GetAttributeTemplates(userId)).Returns(templates);

            // Act
            var result = _templateService.GetAttributeTemplates(userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(templates));
            });
        }

        [Test]
        public void GetTemplateByUserId_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            var userId = "user123";
            _templateRepositoryMock.Setup(repo => repo.GetAttributeTemplates(userId)).Throws(new Exception("Database error"));

            // Act
            var result = _templateService.GetAttributeTemplates(userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("Database error"));
            });
        }

        // Test GetPartCategories
        [Test]
        public void GetPartCategories_ShouldReturnCategories_WhenUserExists()
        {
            // Arrange
            var userId = "user123";
            var categories = new List<PartCategory>
            {
                new() { Id = 1, Name = "Category1", ParentCategoryId = null }
            };
            _templateRepositoryMock.Setup(repo => repo.GetCategories(userId)).Returns(categories);

            // Act
            var result = _templateService.GetPartCategories(userId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.Not.Null);
            });
            Assert.That(actual: result.Result[0].Title, Is.EqualTo("Category1"));
        }

        // Test GetPartAttributes
        [Test]
        public void GetPartAttributes_ShouldReturnAttributes_WhenTableExists()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var attributes = new List<PartAttributeInfo>
            {
                new() { Name = "Attribute1", Value = "Value1" }
            };
            var mappedAttributes = new List<PartAttributeViewModel>
            {
                new() { Name = "Attribute1", Value = "Value1" }
            };
            _templateRepositoryMock.Setup(repo => repo.GetTemplatesDefaultAttributesInfo(guid)).Returns(attributes);
            _mapperMock.Setup(mapper => mapper.Map<List<PartAttributeViewModel>>(attributes)).Returns(mappedAttributes);

            // Act
            var result = _templateService.GetTemplatesDefaultAttributes(guid);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(mappedAttributes));
            });
        }

        [Test]
        public void GetPartAttributes_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            _templateRepositoryMock.Setup(repo => repo.GetTemplatesDefaultAttributesInfo(guid)).Throws(new Exception("Error retrieving attributes"));
            _localizerMock.Setup(loc => loc["ErrorRetriveAttributes"]).Returns(new LocalizedString("ErrorRetriveAttributes", "Error retrieving attributes"));

            // Act
            var result = _templateService.GetTemplatesDefaultAttributes(guid);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("Error retrieving attributes"));
            });
            _appLoggerMock.Verify(log => log.LogMessage(It.IsAny<string>(), LogLevel.Error, It.IsAny<string>()), Times.Once);
        }

        // Test EnsureRootNodeExists
        [Test]
        public void EnsureRootNodeExists_ShouldAddRootNode_WhenRootDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            _templateRepositoryMock.Setup(repo => repo.GetPartCategoryById(userId)).Returns((PartCategory)null);

            // Act
            _templateService.EnsureRootNodeExists(userId);

            // Assert
            _templateRepositoryMock.Verify(repo => repo.AddAndSavePartCategory(It.Is<PartCategory>(c => c.Name == "Root" && c.UserId == userId)), Times.Once);
        }

        // Test SavePartCategory
        [Test]
        public void SavePartCategory_ShouldAddAndUpdateCategories_WhenValidDataProvided()
        {
            // Arrange
            var userId = "user123";
            var categories = new List<CategoryViewModel>
            {
                new() { Id = 1, Title = "NewCategory", ParentId = null }
            };
            var originalCategories = new List<PartCategory>
            {
                new() { Id = 1, Name = "OldCategory" }
            };

            _templateRepositoryMock.Setup(repo => repo.GetCategories(userId)).Returns(originalCategories);
            _mapperMock.Setup(m => m.Map<List<CategoryViewModel>>(originalCategories)).Returns(categories);

            // Act
            var result = _templateService.SavePartCategory(categories, userId);

            // Assert
            Assert.Multiple(() =>
            {

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Result, Is.EqualTo(categories));
            });
            //_templateRepositoryMock.Verify(repo => repo.AddAndSavePartCategory(It.IsAny<PartCategory>()), Times.Once);
            //_templateRepositoryMock.Verify(repo => repo.UpdateAndSaveCategory(It.IsAny<PartCategory>()), Times.Once);
        }
    }

}
