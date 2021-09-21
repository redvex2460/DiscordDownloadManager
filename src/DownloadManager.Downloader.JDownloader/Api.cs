using DownloadManager.Bot.Data;
using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader.Actions;
using DownloadManager.Downloader.JDownloader.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace DownloadManager.Downloader.JDownloader
{
    public partial class Api
    {
        #region Public Fields

        public const string AppKey = "DiscordDownloadManager";
        public static Api Instance;

        #endregion Public Fields

        #region Private Fields

        private const string BaseUrl = "https://api.jdownloader.org";

        #endregion Private Fields

        #region Public Constructors

        public Api(string email, string password)
        {
            Logger.LogMessage(@"Starting API for ""JDownloader""");
            Instance = this;
            Email = email;
            LoginSecret = Utils.GetSecret(email, password, "server");
            DeviceSecret = Utils.GetSecret(email, password, "device");
            Connect();
            Devices = GetDevices();
            RefreshDevicesTimer.Elapsed += RefreshDevices;
            RefreshDevicesTimer.AutoReset = true;
            RefreshDevicesTimer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            RefreshDevicesTimer.Enabled = true;
            Logger.LogMessage(@"""JDownloader"" API Started..");
        }

        #endregion Public Constructors

        #region Public Properties

        public DateTime LastLogin { get; set; }
        public byte[] SessionToken { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private byte[] DeviceEncryptionToken { get; set; }
        private List<Device> Devices { get; set; }
        private byte[] DeviceSecret { get; set; }
        private List<ActiveDownload> Downloads { get; set; }
        private string Email { get; set; }
        private byte[] LoginSecret { get; set; }
        private Timer RefreshDevicesTimer { get; set; }
        private byte[] ServerEncryptionToken { get; set; }

        #endregion Private Properties

        #region Public Methods

        public bool AddDownloadLink(Device dev, string link, string name, string password = "", string linkpassword = "", bool autodownload = false)
        {
            DeviceRequestData requestData = new DeviceRequestData("/linkgrabberv2/addLinks");
            requestData.Device = dev;
            requestData.Data.links = link;
            requestData.Data.autostart = autodownload;
            if (!string.IsNullOrEmpty(name))
                requestData.Data.packageName = name;
            if (!string.IsNullOrEmpty(password))
                requestData.Data.extractPassword = password;
            if (!string.IsNullOrEmpty(linkpassword))
                requestData.Data.linkpassword = linkpassword;
            var id = CallDeviceAction<ServerResponse<dynamic>>(requestData);
            if (id.Data != null)
                return true;
            return false;
        }

        public bool AddDownloadLink(string link, string name = "", string password = "", string linkpassword = "", bool autodownload = false)
        {
            Device dev = Devices.FirstOrDefault();
            return AddDownloadLink(dev, link, name, password, linkpassword);
        }

        public Device GetDeviceByName(string name)
        {

            var device = Devices.FirstOrDefault(dev => dev.Name.ToLower() == name.ToLower());
            if (device == null)
            {
                if (name != "")
                    Logger.LogMessage($"Couldn´t find Device {name}, taking first available!");
                device = Devices[0];
                if (device == null)
                {
                    Logger.LogMessage($"Couldn´t find a Device closing now");
                    Environment.Exit(1);
                }
            }
            return device;
        }

        public void PingDevice(Device device = null)
        {
            if (device == null) device = Devices.FirstOrDefault();
            DeviceRequestData requestData = new DeviceRequestData("/device/ping");
            requestData.Device = device;
            CallDeviceAction<ServerResponse<bool>>(requestData);
        }

        public async Task<List<QueryPackagesServerResponse>> QueryLinks(Device dev = null)
        {
            if (dev == null) dev = Devices.FirstOrDefault();
            DeviceRequestData requestData = new DeviceRequestData("/downloadsV2/queryPackages");
            requestData.Device = dev;
            //requestData.Data.bytesLoaded = true;
            //requestData.Data.bytesTotal = true;
            requestData.Data.childCount = true;
            //requestData.Data.comment = true;
            requestData.Data.enabled = true;
            //requestData.Data.eta = true;
            requestData.Data.finished = false;
            //requestData.Data.hosts = true;
            requestData.Data.maxResults = 100;
            requestData.Data.running = true;
            //requestData.Data.saveTo = true;
            //requestData.Data.speed = true;
            requestData.Data.status = true;
            var response = CallDeviceAction<ServerResponse<List<QueryPackagesServerResponse>>>(requestData);
            return response.Data;
        }

        #endregion Public Methods

        #region Private Methods

        private T CallDeviceAction<T>(DeviceRequestData requestData)
        {
            if ((LastLogin + TimeSpan.FromMinutes(15)) <= DateTime.Now)
            {
                Logger.LogMessage($"{GetType().Name} : Last Login is above 15 minutes ago, relogin now");
                Connect();
            }
            string url = $"https://api.jdownloader.org/t_{HttpUtility.UrlEncode(SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")))}_{HttpUtility.UrlEncode(requestData.Device.Id)}{requestData.Url}";
            Logger.LogMessage($"{GetType().Name} : data : {requestData.Finalize()}", LogType.Debug);
            var response = Utils.Decrypt(Utils.Post(url, Utils.Encrypt(requestData.Finalize(), DeviceEncryptionToken)), DeviceEncryptionToken);
            Logger.LogMessage(response, LogType.Debug);
            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {

                Logger.LogMessage(e.Message, LogType.Error);
            }
            return Activator.CreateInstance<T>();
        }

        private void Connect()
        {
            string url = $"/my/connect?email={HttpUtility.UrlEncode(Email.ToLower())}&appkey={AppKey}&rid={Utils.GetRequestId()}";
            string signature = Utils.GetSignature(url, LoginSecret);
            url = $"https://api.jdownloader.org{url}&signature={signature}";
            var response = Utils.Decrypt(Utils.Get(url), LoginSecret);
            //JObject.Parse(response)["sessiontoken"].ToString()
            LastLogin = DateTime.Now;
            SessionToken = BigInteger.Parse(JObject.Parse(response)["sessiontoken"].ToString(), System.Globalization.NumberStyles.HexNumber).ToByteArray().Reverse().ToArray();
            Logger.LogMessage($"Sessiontoken : {SessionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
            ServerEncryptionToken = Utils.UpdateTokens(LoginSecret, SessionToken);
            Logger.LogMessage($"ServerEncryptionToken : {ServerEncryptionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
            DeviceEncryptionToken = Utils.UpdateTokens(DeviceSecret, SessionToken);
            Logger.LogMessage($"DeviceEncryptionToken : {DeviceEncryptionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
        }

        private List<Device> GetDevices()
        {
            var enumerateDevicesServerAction = new EnumerateDevicesServerAction();
            enumerateDevicesServerAction.Token = SessionToken;
            enumerateDevicesServerAction.Key = ServerEncryptionToken;
            enumerateDevicesServerAction.ApiPath = "/my/listdevices";
            enumerateDevicesServerAction.RequestType = RequestType.Post;
            enumerateDevicesServerAction.Parameters.Add("sessiontoken", SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")));
            enumerateDevicesServerAction.Parameters.Add("rid", Utils.GetRequestId());
            return enumerateDevicesServerAction.GetResult();
        }

        private void RefreshDevices(object sender, ElapsedEventArgs e)
        {
            Devices = GetDevices();
        }

        #endregion Private Methods
    }
}
