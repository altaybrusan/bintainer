using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.SharedResources.Interface
{
    public interface IAppLogger
    {
        void LogModelError(string pageName, ModelStateDictionary modelState);
        void LogMessage(string message, LogLevel level = LogLevel.Information, string? callerName = null);
    }
}
