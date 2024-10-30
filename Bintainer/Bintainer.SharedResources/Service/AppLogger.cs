using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.SharedResources.Service
{
    public class AppLogger : IAppLogger
    {
        private readonly ILogger<AppLogger> _logger;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public AppLogger(ILogger<AppLogger> logger, IStringLocalizer<ErrorMessages> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        public void LogModelError(string pageName, ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            string messageTemplate = _localizer["ErrorModelState"];
            _logger.LogWarning(messageTemplate, DateTime.UtcNow, pageName, errors);
        }
        public void LogMessage(string message, LogLevel level = LogLevel.Information,
        [CallerMemberName] string? callerName = null)
        {
            string formattedMessage = $"{DateTime.UtcNow}: {callerName} - {message}";

            _logger.Log(level, formattedMessage);
        }
    }
}
