using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader.Models
{
    public class QueryPackagesServerResponse
    {
        public string ActiveTask { get; set; }
        public long BytesLoaded { get; set; }
        public long BytesTotal { get; set; }
        public int ChildCount { get; set; }
        public string Comment { get; set; }
        public string DownloadPassword { get; set; }
        public bool Enabled { get; set; }
        public long eta { get; set; }
        public DateTime EtaTime => DateTimeOffset.FromUnixTimeMilliseconds(eta).LocalDateTime;
        public bool Finished { get; set; }
        public List<string> Hosts { get; set; }
        public string Name { get; set; }
        public bool Running { get; set; }
        public string SaveTo { get; set; }
        public long Speed { get; set; }
        public string Status { get; set; }
        public long UUID { get; set; }
    }
}
