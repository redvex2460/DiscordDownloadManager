using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader.Actions
{
    class EnumerateDevicesServerAction : ServerAction
    {
        public List<Device> GetResult()
        {
            Logger.LogMessage("Searching for Devices");
            var result = new List<Device>();
            foreach (JObject device in JObject.Parse(ExecuteRequest())["list"].ToObject<JArray>())
            {
                result.Add(device.ToObject<Device>());
            }
            if (result.Count > 0)
            {
                Logger.LogMessage("Found the following devices:");
                foreach(Device dev in result)
                {
                    Logger.LogMessage($"{dev.Name} - {dev.Id}");
                }
            }
            else
                Logger.LogMessage("Couldn´t find active JDownloader instances on your account!");

            return result;
        }
    }
}
