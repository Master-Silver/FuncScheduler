using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace FuncScheduler.Tests
{
    public class FakeLogger : ILogger
    {
        public ConcurrentBag<FakeLog> Logs { get; } = new ConcurrentBag<FakeLog>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(new FakeLog(logLevel, eventId, exception, formatter(state, exception)));
        }
    }

    public class FakeLog
    {
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public Exception Exception { get; }
        public string Message { get; }

        public FakeLog(LogLevel logLevel, EventId eventId, Exception exception, string message)
        {
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
            Message = message;
        }
    }
}