using DownloadManager.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace DownloadManager.Bot.Data
{
    public class SettingsFile
    {
        private static object _lock = new object();
        public const string Path = ".//config/settings.json";
        public static dynamic CachedSettings = null;
        public static dynamic Read()
        {
            dynamic settings = null;
            lock(_lock)
            {
                if (CachedSettings == null)
                    CachedSettings = JObject.Parse(File.ReadAllText(Path));
                settings = CachedSettings;
            }
            Logger.Debug = settings.Debug;
            return settings;
        }

        public static void Write()
        {
            lock (_lock)
            {
                File.WriteAllText(Path, CachedSettings);
            }
        }

        public static void CreateFromTemplate()
        {
            if (!Directory.Exists("./config/")) Directory.CreateDirectory("./config/");
            if(!File.Exists(Path))
            {
                Logger.LogMessage("Couldn´t find settings.json file, creating a new one.");
                File.Copy("settings.template.json", Path);
                Logger.LogMessage("Please fill out the settings.json and restart the bot.");
                Environment.Exit(1);
            }
        }
    }
}
