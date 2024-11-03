using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;

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
        string formattedMessage = string.Format(messageTemplate, DateTime.UtcNow, pageName, string.Join(", ", errors));

        SetConsoleColor(LogLevel.Warning); // Set color for warnings
        _logger.LogWarning(formattedMessage);
        ResetConsoleColor();
    }

    public void LogMessage(string message, LogLevel level = LogLevel.Information, [CallerMemberName] string? callerName = null)
    {
        string formattedMessage = $"{DateTime.UtcNow}: {callerName} - {message}";

        SetConsoleColor(level); // Set color based on the log level
        _logger.Log(level, formattedMessage);
        ResetConsoleColor();
    }

    private void SetConsoleColor(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case LogLevel.Information:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogLevel.Critical:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case LogLevel.None:
                Console.ResetColor();
                break;
            default:
                Console.ResetColor();
                break;
        }
    }

    private void ResetConsoleColor()
    {
        Console.ResetColor();
    }
}
