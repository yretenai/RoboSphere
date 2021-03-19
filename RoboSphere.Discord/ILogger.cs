using JetBrains.Annotations;
using System;
using System.Diagnostics;

namespace DragonLib.IO
{
    // Move to DragonLib
    [PublicAPI]
    public interface ILogger
    {
        void Info(string? category, string message);
        void Debug(string? category, string message);
        void Warn(string? category, string message);
        void Error(string? category, string message);
        void Error(string? category, string message, Exception e);
        void Error(string? category, Exception e);
        void Fatal(string? category, string message);
        void Fatal(string? category, string message, Exception e);
        void Fatal(string? category, Exception e);
        void Trace(string? category, string message);
        void Trace(string? category, Exception e);
        void Trace(string? category, string message, Exception e);
        void Trace(string? category, string message, StackTrace stack);
    }
}
