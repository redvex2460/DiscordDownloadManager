using DownloadManager.Core.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader.Actions
{
    public class ServerAction
    {
        #region Public Fields

        public string ApiPath;
        public byte[] Key;
        public Dictionary<string, string> Parameters;
        public string RequestId;
        public RequestType RequestType;
        public byte[] Token;

        #endregion Public Fields

        #region Public Constructors

        public ServerAction ()
        {
            RequestId = Utils.GetRequestId();
            Parameters = new Dictionary<string, string>();
        }

        #endregion Public Constructors

        #region Public Methods

        public virtual async Task<string> ExecuteRequest(string data = "")
        {
            var response = "";
            if (RequestType == RequestType.Get)
                response = await Utils.Get(GetSignatedUrl());
            else
                response = await Utils.Post(GetSignatedUrl(),data);
            response = Utils.Decrypt(response, Key);
            Logger.LogMessage(response, LogType.Debug);
            return response;
        }

        #endregion Public Methods

        #region Private Methods

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

        #endregion Private Methods
    }
}