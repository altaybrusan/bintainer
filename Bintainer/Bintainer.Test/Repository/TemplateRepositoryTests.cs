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

namespace Bintainer.Test.Repository
{
    [TestFixture]
    public class TemplateRepositoryTests
    {
        private Mock<BintainerDbContext> _mockDbContext;
        private TemplateRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<BintainerDbContext>();
            _repository = new TemplateRepository(_mockDbContext.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());
            return mockSet;
        }

        [Test]
        public void GetTemplatesOfUser_ReturnsTemplates_WhenTemplatesExist()
        {
            // Arrange
            var userId = "user123";
            var templates = new List<PartAttributeTemplate>
        {
            new PartAttributeTemplate { Id = 1, UserId = userId, TemplateName = "Template1" },
            new PartAttributeTemplate { Id = 2, UserId = userId, TemplateName = "Template2" }
        };

            var mockSet = CreateMockDbSet(templates);
            _mockDbContext.Setup(c => c.PartAttributeTemplates).Returns(mockSet.Object);

            // Act
            var result = _repository.GetTemplatesOfUser(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo("Template1"));
            Assert.That(result[2], Is.EqualTo("Template2"));
        }

        [Test]
        public void GetAttributeTemplateById_ReturnsTemplate_WhenTemplateExists()
        {
            // Arrange
            var templateId = 1;
            var templates = new List<PartAttributeTemplate>
        {
            new PartAttributeTemplate { Id = templateId, TemplateName = "Template1" }
        };

            var mockSet = CreateMockDbSet(templates);
            _mockDbContext.Setup(c => c.PartAttributeTemplates).Returns(mockSet.Object);

            // Act
            var result = _repository.GetAttributeTemplateById(templateId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TemplateName, Is.EqualTo("Template1"));
        }

        [Test]
        public void CreateAttributeTemplateByName_AddsTemplate_WhenCalled()
        {
            // Arrange
            var templateName = "NewTemplate";
            var userId = "user123";
            var mockSet = new Mock<DbSet<PartAttributeTemplate>>();

            _mockDbContext.Setup(c => c.PartAttributeTemplates).Returns(mockSet.Object);

            // Act
            var result = _repository.CreateAttributeTemplateByName(templateName, userId);

            // Assert
            mockSet.Verify(m => m.Add(It.Is<PartAttributeTemplate>(t => t.TemplateName == templateName && t.UserId == userId)), Times.Once);
            _mockDbContext.Verify(m => m.SaveChanges(true), Times.Once);
        }

        [Test]
        public void SaveAttributes_AddsAttributes_WhenCalled()
        {
            // Arrange
            var attributes = new List<PartAttribute>
        {
            new PartAttribute { Name = "Attribute1", Value = "Value1" },
            new PartAttribute { Name = "Attribute2", Value = "Value2" }
        };

            var mockSet = new Mock<DbSet<PartAttribute>>();
            _mockDbContext.Setup(c => c.PartAttributes).Returns(mockSet.Object);

            // Act
            _repository.SaveAttributes(attributes);

            // Assert
            mockSet.Verify(m => m.AddRange(attributes), Times.Once);
            _mockDbContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Test]
        public void GetPartCategories_ReturnsCategories_ForGivenUserId()
        {
            // Arrange
            var userId = "user123";
            var categories = new List<PartCategory>
        {
            new PartCategory { Id = 1, UserId = userId, Name = "Category1" }
        };

            var mockSet = CreateMockDbSet(categories);
            _mockDbContext.Setup(c => c.PartCategories).Returns(mockSet.Object);

            // Act
            var result = _repository.GetPartCategories(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Category1"));
        }

        [Test]
        public void GetCategory_ReturnsCategory_ById()
        {
            // Arrange
            var categories = new List<PartCategory>
        {
            new PartCategory { Id = 1, Name = "Category1" }
        };

            var mockSet = CreateMockDbSet(categories);
            _mockDbContext.Setup(c => c.PartCategories).Returns(mockSet.Object);

            // Act
            var result = _repository.GetCategory(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Category1"));
        }

        [Test]
        public void AddAndSavePartCategory_AddsAndSavesCategory()
        {
            // Arrange
            var category = new PartCategory { Name = "NewCategory" };
            var mockSet = new Mock<DbSet<PartCategory>>();

            _mockDbContext.Setup(c => c.PartCategories).Returns(mockSet.Object);

            // Act
            var result = _repository.AddAndSavePartCategory(category);

            // Assert
            mockSet.Verify(m => m.Add(It.Is<PartCategory>(c => c.Name == "NewCategory")), Times.Once);
            _mockDbContext.Verify(m => m.SaveChanges(true), Times.Once);
            Assert.That(result, Is.EqualTo(category));
        }

        [Test]
        public void RemovePartCategory_RemovesCategory_ById()
        {
            // Arrange
            var category = new PartCategory { Id = 1, Name = "Category1" };
            var categories = new List<PartCategory> { category };

            var mockSet = CreateMockDbSet(categories);
            _mockDbContext.Setup(c => c.PartCategories).Returns(mockSet.Object);

            // Act
            _repository.RemovePartCategory(1);

            // Assert
            mockSet.Verify(m => m.Remove(It.Is<PartCategory>(c => c.Id == 1)), Times.Once);
        }

        [Test]
        public void LoadAttributes_ReturnsAttributesDictionary_ForUser()
        {
            // Arrange
            var userId = "user123";
            var templates = new List<PartAttributeTemplate>
        {
            new PartAttributeTemplate { Id = 1, UserId = userId, TemplateName = "Template1" },
            new PartAttributeTemplate { Id = 2, UserId = userId, TemplateName = "Template2" }
        };

            var mockSet = CreateMockDbSet(templates);
            _mockDbContext.Setup(c => c.PartAttributeTemplates).Returns(mockSet.Object);

            // Act
            var result = _repository.GetAttributeTemplates(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo("Template1"));
            Assert.That(result[2], Is.EqualTo("Template2"));
        }
    }

}

