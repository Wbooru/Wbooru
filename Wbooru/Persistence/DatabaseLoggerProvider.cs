using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Wbooru.Settings;

namespace Wbooru.Persistence
{
    internal sealed class DatabaseLoggerProvider : ILoggerProvider
    {
        private class DatabaseLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel)
            {
                if (logLevel != LogLevel.Debug || logLevel != LogLevel.Trace || Setting<GlobalSetting>.Current.EnableOutputDebugMessage)
                    return true;

                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = $"[{logLevel}]  {formatter?.Invoke(state, exception)}";

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        Wbooru.Log.Debug(message, "Database");
                        break;
                    case LogLevel.None:
                    case LogLevel.Information:
                        Wbooru.Log.Info(message, "Database");
                        break;
                    case LogLevel.Warning:
                        Wbooru.Log.Warn(message, "Database");
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Wbooru.Log.Error(message, "Database");
                        break;
                    default:
                        break;
                }
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger();
        }

        public void Dispose()
        {

        }
    }
}
