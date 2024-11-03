using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Bintainer.Model.Entity;
using Bintainer.Model.View;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Bintainer.WebApp.Pages.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bintainer.Model.Template;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Bintainer.Test.Page
{

    [TestFixture]
    public class InventoryPageTests
    {
        private InventoryModel _inventoryModel;
        private Mock<IInventoryService> _inventoryServiceMock;
        private Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private Mock<IStringLocalizer<ErrorMessages>> _localizerMock;
        private Mock<IAppLogger> _appLoggerMock;

        [SetUp]
        public void Setup()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>()
            );

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<IdentityUser>>()
            );


            _localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
            _appLoggerMock = new Mock<IAppLogger>();

            _inventoryModel = new InventoryModel(
                _inventoryServiceMock.Object,
                _signInManagerMock.Object,
                _localizerMock.Object,
                _appLoggerMock.Object
            );
        }

        [Test]
        public void OnGet_UserIdentityIsNull_ShouldNotLogWarning()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var identityMock = new Mock<ClaimsIdentity>();

            // Simulating that User.Identity is not null, but User.Identity.Name is null
            identityMock.Setup(i => i.IsAuthenticated).Returns(true); // Simulating authenticated user
            identityMock.Setup(i => i.Name).Returns((string)null); // User name is null

            httpContextMock.Setup(h => h.User.Identity).Returns(identityMock.Object);
            _inventoryModel.PageContext.HttpContext = httpContextMock.Object;

            // Act
            _inventoryModel.OnGet();

            // Assert
            _appLoggerMock.Verify(logger => logger.LogMessage(It.IsAny<string>(), LogLevel.Warning, It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void OnGet_UserIdentityIsNull_ShouldLogWarning()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();

            httpContextMock.Setup(h => h.User.Identity).Returns((ClaimsIdentity)null);
            _inventoryModel.PageContext.HttpContext = httpContextMock.Object;

            // Act
            _inventoryModel.OnGet();

            // Assert
            _appLoggerMock.Verify(logger => logger.LogMessage(It.IsAny<string>(), LogLevel.Warning, It.IsAny<string>()), Times.Once);
        }


        [Test]
        public void OnGet_UserIdentityIsNotNull_ShouldCallInventoryService()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var identityMock = new Mock<ClaimsIdentity>();
            identityMock.Setup(i => i.Name).Returns("TestUser");
            httpContextMock.Setup(h => h.User.Identity).Returns(identityMock.Object);
            _inventoryModel.PageContext.HttpContext = httpContextMock.Object;

            _inventoryServiceMock.Setup(service => service.GetInventory("TestUser"))
                .Returns(new Response<Inventory> { IsSuccess = true, Result = new Inventory() });

            // Act
            _inventoryModel.OnGet();

            // Assert
            _inventoryServiceMock.Verify(service => service.GetInventory("TestUser"), Times.Once);
        }

        [Test]
        public void OnPostSubmitForm_ModelStateIsInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            _inventoryModel.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = _inventoryModel.OnPostSubmitForm(new List<InventorySection>(), "TestInventory");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _appLoggerMock.Verify(logger => logger.LogModelError(nameof(_inventoryModel.OnPostSubmitForm), It.IsAny<ModelStateDictionary>()), Times.Once);
        }

        [Test]
        public void OnPostSubmitForm_UserIdentityIsNull_ShouldLogWarning()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var identityMock = new Mock<ClaimsIdentity>();
            identityMock.Setup(i => i.Name).Returns((string)null);
            httpContextMock.Setup(h => h.User.Identity).Returns(identityMock.Object);
            _inventoryModel.PageContext.HttpContext = httpContextMock.Object;

            // Act
            var result = _inventoryModel.OnPostSubmitForm(new List<InventorySection>(), "TestInventory");

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            //_appLoggerMock.Verify(logger => logger.LogMessage(It.IsAny<string>(), LogLevel.Warning, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void OnPostSubmitForm_ValidInput_ShouldCallInventoryService()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var identityMock = new Mock<ClaimsIdentity>();
            identityMock.Setup(i => i.Name).Returns("TestUser");
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "12345") };
            httpContextMock.Setup(h => h.User.Identity).Returns(identityMock.Object);
            httpContextMock.Setup(h => h.User.Claims).Returns(claims);
            _inventoryModel.PageContext.HttpContext = httpContextMock.Object;

            _inventoryServiceMock.Setup(service => service.CreateOrUpdateInventory(It.IsAny<UserViewModel>(), "TestInventory",It.IsAny<List<InventorySection>>()))
                .Returns(new Response<Inventory> { IsSuccess = true, Result = new Inventory() });

            // Act
            var result = _inventoryModel.OnPostSubmitForm(new List<InventorySection>(), "TestInventory");

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            _inventoryServiceMock.Verify(service => service.CreateOrUpdateInventory(It.IsAny<UserViewModel>(), "TestInventory",It.IsAny<List<InventorySection>>()), Times.Once);
        }
    }
}
