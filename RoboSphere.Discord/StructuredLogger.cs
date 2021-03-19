using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using ILogger = DragonLib.IO.ILogger;

namespace RoboSphere.Discord
{
    internal class StructuredLogger<T> : ILogger
    {
        public StructuredLogger(ILogger<T> logger) => Logger = logger;

        private ILogger<T> Logger { get; }

        public void Info(string? category, string message) => Logger.LogInformation(message);
        public void Debug(string? category, string message) => Logger.LogDebug(message);
        public void Warn(string? category, string message) => Logger.LogWarning(message);
        public void Error(string? category, string message) => Logger.LogError(message);
        public void Error(string? category, string message, Exception e) => Logger.LogError(e, message);
        public void Error(string? category, Exception e) => Logger.LogError(e, e.Message);
        public void Fatal(string? category, string message) => Logger.LogCritical(message);
        public void Fatal(string? category, string message, Exception e) => Logger.LogCritical(message, e);
        public void Fatal(string? category, Exception e) => Logger.LogCritical(e, e.Message);
        public void Trace(string? category, string message) => Logger.LogTrace(message);
        public void Trace(string? category, Exception e) => Logger.LogTrace(e, e.Message);
        public void Trace(string? category, string message, Exception e) => Logger.LogTrace(e, message);
        public void Trace(string? category, string message, StackTrace stack) => Logger.LogTrace(message);
    }
}
