// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey.TestDoubles
{
    public class SpyLogger : ILogger
    {
        public readonly List<string> Messages = new List<string>();

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception exception, Object context)
        {
            throw new NotImplementedException();
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType logType, object message)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType logType, object message, Object context)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType logType, string tag, object message)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType logType, string tag, object message, Object context)
        {
            throw new NotImplementedException();
        }

        public void Log(object message)
        {
            this.Messages.Add(message as string);
        }

        public void Log(string tag, object message)
        {
            throw new NotImplementedException();
        }

        public void Log(string tag, object message, Object context)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string tag, object message)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string tag, object message, Object context)
        {
            throw new NotImplementedException();
        }

        public void LogError(string tag, object message)
        {
            throw new NotImplementedException();
        }

        public void LogError(string tag, object message, Object context)
        {
            throw new NotImplementedException();
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get; set; }
    }
}
