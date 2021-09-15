using System;

namespace DownloadManager.Core.Logging
{
    public class Logger
    {
        public static bool Debug = false;
        public static void LogMessage(string message, LogType logType = LogType.Info)
        {
            switch(logType)
            {
                case LogType.Debug:
                    if (!Debug) return;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"DEBUG ::: {message}");
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error ::: {message}");
                    break;
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Info ::: {message}");
                    break;
                case LogType.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warn ::: {message}");
                    break;

            }
        }
    }
}
