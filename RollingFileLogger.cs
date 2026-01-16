using System;
using System.IO;
using System.Text;

namespace LayoutParserLib
{
    internal static class RollingFileLogger
    {
        private const long MaxBytes = 2049L * 1024L;
        private const int MaxFiles = 10;

        internal static void Log(string level, string message, Exception ex = null)
        {
            try
            {
                var logDir = Environment.GetEnvironmentVariable("LAYOUTPARSER_LOG_DIR") ?? "";
                var corr = Environment.GetEnvironmentVariable("LAYOUTPARSER_CORRELATION_ID") ?? "";
                if (string.IsNullOrWhiteSpace(corr))
                    corr = "N/A";

                if (string.IsNullOrWhiteSpace(logDir))
                    logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                Directory.CreateDirectory(logDir);
                var baseFileName = "layoutparserlib.log";
                var basePath = Path.Combine(logDir, baseFileName);
                RollIfNeeded(logDir, baseFileName, basePath);

                var line = $"{DateTime.UtcNow:O} [{level}] [Corr:{corr}] {message}";
                if (ex != null) line += " | " + ex;
                File.AppendAllText(basePath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        private static void RollIfNeeded(string logDir, string baseFileName, string basePath)
        {
            try
            {
                var fi = new FileInfo(basePath);
                if (!fi.Exists) return;
                if (fi.Length < MaxBytes) return;

                var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff");
                var rolled = Path.Combine(logDir, Path.GetFileNameWithoutExtension(baseFileName) + "-" + stamp + Path.GetExtension(baseFileName));
                File.Move(basePath, rolled);

                var pattern = Path.GetFileNameWithoutExtension(baseFileName) + "-*.log";
                var files = new DirectoryInfo(logDir).GetFiles(pattern);
                Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
                for (int i = MaxFiles - 1; i < files.Length; i++)
                {
                    try { files[i].Delete(); } catch { }
                }
            }
            catch { }
        }
    }
}


