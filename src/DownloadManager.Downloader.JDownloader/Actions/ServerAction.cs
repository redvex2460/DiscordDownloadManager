using DownloadManager.Core.Logging;
using System.Collections.Generic;

namespace DownloadManager.Downloader.JDownloader.Actions
{
    public abstract class ServerAction
    {
        public byte[] Token;
        public byte[] Key;
        public string ApiPath;
        public string RequestId;
        public RequestType RequestType;
        public Dictionary<string,string> Parameters;
        public ServerAction ()
        {
            RequestId = Utils.GetRequestId();
            Parameters = new Dictionary<string, string>();
        }
        public virtual string ExecuteRequest(string data = "")
        {
            var response = "";
            if (RequestType == RequestType.Get)
                response = Utils.Get(GetSignatedUrl());
            else
                response = Utils.Post(GetSignatedUrl(),data);
            response = Utils.Decrypt(response, Key);
            Logger.LogMessage(response, LogType.Debug);
            return response;
        }
        private string GetSignatedUrl()
        {
            string url = $"{ApiPath}?";
            var i = 0;
            foreach(var param in Parameters)
            {
                if(i > 0)
                    url = $"{url}&{param.Key}={param.Value}";
                else
                    url = $"{url}{param.Key}={param.Value}";
                i++;
            }
            url = $"{url}&signature={Utils.GetSignature(url, Key)}";
            Logger.LogMessage($"{GetType().Name} : GetSignatedUrl : {url}", LogType.Debug);
            return $"https://api.jdownloader.org{url}";
        }
    }
}