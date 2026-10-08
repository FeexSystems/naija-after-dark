using UnityEngine;

namespace NAAD.Core.Logging
{
    public sealed class UnityNAADLogger : INAADLogger
    {
        public void Log(LogLevel level, string category, string message)
        {
            var line = $"[NAAD][{category}] {message}";
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(line);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(line);
                    break;
                case LogLevel.Error:
                    Debug.LogError(line);
                    break;
            }
        }

        public void Debug(string category, string message) => Log(LogLevel.Debug, category, message);
        public void Info(string category, string message) => Log(LogLevel.Info, category, message);
        public void Warn(string category, string message) => Log(LogLevel.Warn, category, message);
        public void Error(string category, string message) => Log(LogLevel.Error, category, message);
    }
}
