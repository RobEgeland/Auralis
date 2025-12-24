using Auralis.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Auralis.Logging
{
    public sealed class FileAuralisLogger : IAuralisLogger
    {
        private readonly string _logFile;
        public FileAuralisLogger()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Auralis",
                "Logs");

            Directory.CreateDirectory(folder);

            _logFile = Path.Combine(folder, "auralis.log");
        }

        public void Info(string message) => Write("INFO", message);
        public void Warning(string message) => Write("WARN", message);
        public void Error(string message, Exception? ex = null)
        {
            Write("ERROR", $"{message} {ex}");
        }

        private void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            File.AppendAllLines(_logFile, new[] { line });
        }
    }
}
