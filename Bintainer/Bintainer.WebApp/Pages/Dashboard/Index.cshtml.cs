using Bintainer.SharedResources.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class Index : PageModel
    {
        IAppLogger _appLogger;
        IStringLocalizer<ErrorMessages> _stringLocalizer;
        public Index(IAppLogger appLogger, 
                     IStringLocalizer<ErrorMessages> localizer)
        {
            _appLogger = appLogger;
            _stringLocalizer = localizer;
        }
        public void OnGet()
        {

#if DEBUG
            _appLogger.LogMessage(_stringLocalizer["TraceLogTestMessage"], LogLevel.Trace);
            _appLogger.LogMessage(_stringLocalizer["DebugLogTestMessage"], LogLevel.Debug);
            _appLogger.LogMessage(_stringLocalizer["InformationLogTestMessage"], LogLevel.Information);
            _appLogger.LogMessage(_stringLocalizer["WarningLogTestMessage"], LogLevel.Warning);
            _appLogger.LogMessage(_stringLocalizer["ErrorLogTestMessage"], LogLevel.Error);
            _appLogger.LogMessage(_stringLocalizer["CriticalLogTestMessage"], LogLevel.Critical);
#endif
        }
    }
}
