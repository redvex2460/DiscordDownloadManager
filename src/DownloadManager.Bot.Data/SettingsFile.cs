﻿using DownloadManager.Bot.Data.Models;
using DownloadManager.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace DownloadManager.Bot.Data
{
    public partial class SettingsFile
    {
        #region Public Fields

        public const string Path = ".//config/settings.json";
        public static SettingsFile CachedSettings = null;

        #endregion Public Fields

        #region Private Fields

        private static readonly object _lock = new object();

        #endregion Private Fields

        #region Public Properties

        public bool Debug { get; set; }
        public DiscordSettings Discord { get; set; }
        public JDownloaderSettings JDownloader { get; set; }

        #endregion Public Properties

        #region Public Methods

        public static void CreateFromTemplate()
        {
            if (!Directory.Exists("./config/")) Directory.CreateDirectory("./config/");
            if (!File.Exists(Path))
            {
                Logger.LogMessage("Couldn´t find settings.json file, creating a new one.");
                File.Copy("settings.template.json", Path);
                Logger.LogMessage("Please fill out the settings.json and restart the bot.");
                Environment.Exit(1);
            }
        }

        public static dynamic Read()
        {
            dynamic settings = null;
            lock(_lock)
            {
                if (CachedSettings == null)
                    CachedSettings = JsonConvert.DeserializeObject<SettingsFile>(File.ReadAllText(Path));
                settings = CachedSettings;
            }
            Logger.Debug = settings.Debug;
            return settings;
        }

        public static void Write()
        {
            lock (_lock)
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(CachedSettings));
            }
        }

        #endregion Public Methods
    }
}
