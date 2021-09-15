using Newtonsoft.Json;
using System.Dynamic;

namespace DownloadManager.Downloader.JDownloader.Models
{
    public class DeviceRequestData
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        public int ApiVer { get; set; } = 1;
        [JsonProperty(PropertyName = "params")]
        public dynamic Data { get; set; } = new ExpandoObject();
        [JsonProperty(PropertyName = "rid")]
        public string RequestId { get; set; }
        [JsonIgnore]
        public Device Device { get; set; }
        private bool serialized { get; set; }

        public DeviceRequestData(string apiPath)
        {
            Url = apiPath;
            RequestId = Utils.GetRequestId();
        }

        public DeviceRequestData(string apiPath, dynamic data) : this(apiPath)
        {
            Data = data;
        }

        public string Finalize()
        {
            if (!serialized)
            {
                var newData = new[] { JsonConvert.SerializeObject(Data) };
                Data = newData;
                serialized = true;
            }
            return JsonConvert.SerializeObject(this);
        }
    }
}
