using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader.Models
{
    public class QueryLinksServerResponse
    {
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
        [JsonProperty(propertyName: "packageUUID")]
        public long PackageUUID { get; set; }
        [JsonProperty(propertyName: "uuid")]
        public long UUID { get; set; }
        [JsonProperty(propertyName: "enabled")]
        public bool Enabled { get; set; }
        [JsonProperty(propertyName: "finished")]
        public bool Finished { get; set; }
        [JsonProperty(propertyName: "addedDate")]
        public long addedDate { get; set; }
        public DateTime AddedTime => DateTimeOffset.FromUnixTimeMilliseconds(addedDate).LocalDateTime;
    }
}
