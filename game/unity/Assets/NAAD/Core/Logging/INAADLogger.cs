namespace NAAD.Core.Logging
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    }

    /// <summary>
    /// Structured logging boundary. Keeps UnityEngine.Debug behind an interface
    /// so systems stay testable and free of static spaghetti.
    /// </summary>
    public interface INAADLogger
    {
        void Log(LogLevel level, string category, string message);
        void Debug(string category, string message);
        void Info(string category, string message);
        void Warn(string category, string message);
        void Error(string category, string message);
    }
}
