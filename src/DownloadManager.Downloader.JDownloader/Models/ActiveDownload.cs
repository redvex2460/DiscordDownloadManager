using System;

namespace DownloadManager.Downloader.JDownloader.Models
{
    internal class ActiveDownload
    {

        public DateTime AddedTime { get; set; }
        public double BytesLoaded { get; set; }
        public double BytesTotal { get; set; }
        public string Comment { get; set; }
        public string Eta { get; set; }
        public string ExtractionStatus { get; set; }
        public bool Finished { get; set; }
        public string Host { get; set; }
        public string JobUUIDs { get; set; }
        public string PackageUUIDs { get; set; }
        public string Password { get; set; }
        public string Priority { get; set; }
        public bool Skipped { get; set; }
        public bool Running { get; set; }
        public double Speed { get; set; }
        public DateTime StartAt { get; set; }
        public string Status { get; set; }
        public string Url { get; set; }
    }
}