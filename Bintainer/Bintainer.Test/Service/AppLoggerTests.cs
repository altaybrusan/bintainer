using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using Bintainer.SharedResources.Resources;
using Bintainer.SharedResources.Service;

namespace Bintainer.Test.Service
{


    [TestFixture]
    public class AppLoggerTests
    {
        private Mock<ILogger<AppLogger>> _mockLogger;
        private Mock<IStringLocalizer<ErrorMessages>> _mockLocalizer;
        private AppLogger _appLogger;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<AppLogger>>();
            _mockLocalizer = new Mock<IStringLocalizer<ErrorMessages>>();
            _appLogger = new AppLogger(_mockLogger.Object, _mockLocalizer.Object);
        }

        [Test]
        public void LogModelError_LogsExpectedWarningWithErrors()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Field1", "Error1");
            modelState.AddModelError("Field2", "Error2");

            var expectedPageName = "TestPage";
            var expectedMessageTemplate = "Model state has errors";
            _mockLocalizer.Setup(l => l["ErrorModelState"]).Returns(new LocalizedString("ErrorModelState", expectedMessageTemplate));

            // Act
            _appLogger.LogModelError(expectedPageName, modelState);

            // Assert
            _mockLogger.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        }

        [TestCase(LogLevel.Information, "Information message")]
        [TestCase(LogLevel.Error, "Error message")]
        [TestCase(LogLevel.Warning, "Warning message")]
        public void LogMessage_LogsWithCorrectLevelAndMessage(LogLevel level, string message)
        {
            // Arrange
            var callerName = "TestCaller";
            var expectedMessage = $"{DateTime.UtcNow}: {callerName} - {message}";

            // Act
            _appLogger.LogMessage(message, level, callerName);

            // Assert
            _mockLogger.Verify(logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(callerName) && v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }

}
