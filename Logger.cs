using System;
using System.IO;

namespace SimpleApiServer
{
    public static class Logger
    {
        public static string LogFilePath;

        public static void LogInfo(string msg) => Write("INFO", msg);
        public static void LogError(string msg, Exception ex = null) =>
            Write("ERROR", ex != null ? $"{msg}: {ex.Message}" : msg);

        private static void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            Console.WriteLine(line);
            if (!string.IsNullOrEmpty(LogFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
        }
    }
}