using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace DownloadManager.Downloader.JDownloader.Actions
{
    public abstract class DeviceAction
    {
        /*public Device Device { get; set; }
        public byte[] SessionToken { get; set; }
        public byte[] EncryptionToken { get; set; }
        public string RequestId { get; }
        public string ApiPath { get; set; }

        public DeviceAction(Device device)
        {
            Device = device;
            SessionToken = sessionToken;
            EncryptionToken = encryptionToken;
            RequestId = Utils.GetRequestId();
        }
        public DeviceAction()
        {
            Device = Api.Instance.GetDeviceByName("");
            return new DeviceAction(Device);
        }
        public virtual string ExecuteRequest(dynamic dataObject = null)
        {
            dynamic p = new ExpandoObject();
            var a = new[] { JsonConvert.SerializeObject(dataObject) };
            p.url = ApiPath;
            if(dataObject != null)
            {
                p.@params = a;
            }
            p.rid = RequestId;
            p.ApiVer = 1;
            string data = JsonConvert.SerializeObject(p);
            Logger.LogMessage($"{GetType().Name} : data : {data}");
            var response = Utils.Decrypt(Utils.Post(BuildUrl(), Utils.Encrypt(data, EncryptionToken)),EncryptionToken);
            Logger.LogMessage(response, LogType.Debug);
            return response;
        }

        public string BuildUrl()
        {
            return $"https://api.jdownloader.org/t_{HttpUtility.UrlEncode(SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")))}_{HttpUtility.UrlEncode(Device.Id)}{ApiPath}";
        }

        private string GetSignatedUrl()
        {
            string url = $"/t_{HttpUtility.UrlEncode(SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")))}_{HttpUtility.UrlEncode(Device.Id)}{ApiPath}";
            url = $"{url}&signature={Utils.GetSignature(url, EncryptionToken)}";
            Logger.LogMessage($"{GetType().Name} : GetSignatedUrl : {url}");
            return $"https://api.jdownloader.org{url}";
        }*/
    }
}