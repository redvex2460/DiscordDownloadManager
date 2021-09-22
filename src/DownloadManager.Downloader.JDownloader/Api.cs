using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader.Actions;
using DownloadManager.Downloader.JDownloader.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        private List<QueryPackagesServerResponse> activeDownloads;
        #region Public Constructors

        public Api(string email, string password)
        {
            Logger.LogMessage(@"Starting API for ""JDownloader""");
            Instance = this;
            Email = email;
            LoginSecret = Utils.GetSecret(email, password, "server");
            DeviceSecret = Utils.GetSecret(email, password, "device");
            _ = Connect().Result;
            Devices = GetDevices().Result;
            RefreshDevicesTimer.Elapsed += RefreshDevices;
            RefreshDevicesTimer.AutoReset = true;
            RefreshDevicesTimer.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
            RefreshDevicesTimer.Enabled = true;

            activeDownloads = new ActivePackageList();
            RefreshActiveDownloadsTimer.Interval = TimeSpan.FromSeconds(40).TotalMilliseconds;
            RefreshActiveDownloadsTimer.Elapsed += RefreshActiveDownloads;
            RefreshActiveDownloadsTimer.AutoReset = true;
            RefreshActiveDownloadsTimer.Enabled = true;

            Logger.LogMessage(@"""JDownloader"" API Started..");
        }

        public delegate Task DownloadFinished(QueryPackagesServerResponse package);
        public event DownloadFinished OnDownloadFinished;

        private void RefreshActiveDownloads(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var newActiveDownloads = await QueryLinks();
                foreach(QueryPackagesServerResponse activeDownload in activeDownloads)
                {
                    if (!newActiveDownloads.Contains(activeDownload.UUID))
                        OnDownloadFinished?.Invoke(activeDownload);

                }
                activeDownloads = newActiveDownloads;
            });
        }

        #endregion Public Constructors

        #region Public Properties

        public DateTime LastLogin { get; set; }
        public byte[] SessionToken { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private Task[] activeTasks { get; set; }
        private byte[] DeviceEncryptionToken { get; set; }
        private List<Device> Devices { get; set; }
        private byte[] DeviceSecret { get; set; }
        private List<ActiveDownload> Downloads { get; set; }
        private string Email { get; set; }
        private byte[] LoginSecret { get; set; }
        private Timer RefreshDevicesTimer { get; set; } = new Timer();
        private Timer RefreshActiveDownloadsTimer { get; set; } = new Timer();
        private byte[] ServerEncryptionToken { get; set; }

        #endregion Private Properties

        #region Public Methods

        public async Task<bool> AddDownloadLink(string link, string name, Device dev = null, string password = "", string linkpassword = "", bool autodownload = false)
        {
            if (dev == null)
                dev = Devices.FirstOrDefault();
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

            try
            {
                var response = CallDeviceAction<ServerResponse<dynamic>>(requestData);
                var timer = Task.Delay(((int)TimeSpan.FromSeconds(20).TotalMilliseconds));
                var finishedTask = await Task.WhenAny(response, timer);
                if (response == finishedTask)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, LogType.Error);
                return false;
            }
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

        public async Task<bool> PingDevice(Device device = null)
        {
            if (device == null) device = Devices.FirstOrDefault();
            DeviceRequestData requestData = new DeviceRequestData("/device/ping");
            requestData.Device = device;
            var result = await CallDeviceAction<ServerResponse<bool>>(requestData);
            return result.Data;
        }

        public async Task<ActivePackageList> QueryLinks(Device dev = null)
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
            var response = await CallDeviceAction<ServerResponse<ActivePackageList>>(requestData);
            return response.Data;
        }

        #endregion Public Methods

        #region Private Methods

        private async Task<T> CallDeviceAction<T>(DeviceRequestData deviceRequestData)
        {
            if ((LastLogin + TimeSpan.FromMinutes(15)) <= DateTime.Now)
            {
                Logger.LogMessage($"{GetType().Name} : Last Login is above 15 minutes ago, relogin now");
                bool loggedIn = await Connect();
            }
            string url = $"https://api.jdownloader.org/t_{HttpUtility.UrlEncode(SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")))}_{HttpUtility.UrlEncode(deviceRequestData.Device.Id)}{deviceRequestData.Url}";
            Logger.LogMessage($"{GetType().Name} : data : {deviceRequestData.Finalize()}", LogType.Debug);
            var response = await Utils.Post(url, Utils.Encrypt(deviceRequestData.Finalize(), DeviceEncryptionToken));
            var decryptedResponse = Utils.Decrypt(response, DeviceEncryptionToken);
            Logger.LogMessage(decryptedResponse, LogType.Debug);
            try
            {
                return JsonConvert.DeserializeObject<T>(decryptedResponse);
            }
            catch (Exception e)
            {
                Logger.LogMessage(e.Message, LogType.Error);
                return default(T);
            }
        }

        private async Task<bool> Connect()
        {
            string url = $"/my/connect?email={HttpUtility.UrlEncode(Email.ToLower())}&appkey={AppKey}&rid={Utils.GetRequestId()}";
            string signature = Utils.GetSignature(url, LoginSecret);
            url = $"https://api.jdownloader.org{url}&signature={signature}";
            var response = await Utils.Get(url);
            if (string.IsNullOrEmpty(response))
                return false;
            var decryptedResponse = Utils.Decrypt(response, LoginSecret);
            //JObject.Parse(response)["sessiontoken"].ToString()
            LastLogin = DateTime.Now;
            SessionToken = BigInteger.Parse(JObject.Parse(decryptedResponse)["sessiontoken"].ToString(), System.Globalization.NumberStyles.HexNumber).ToByteArray().Reverse().ToArray();
            Logger.LogMessage($"Sessiontoken : {SessionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
            ServerEncryptionToken = Utils.UpdateTokens(LoginSecret, SessionToken);
            Logger.LogMessage($"ServerEncryptionToken : {ServerEncryptionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
            DeviceEncryptionToken = Utils.UpdateTokens(DeviceSecret, SessionToken);
            Logger.LogMessage($"DeviceEncryptionToken : {DeviceEncryptionToken.Aggregate("", (c, t) => c + t.ToString("X2"))}", LogType.Debug);
            return true;
        }

        private async Task<List<Device>> GetDevices()
        {
            Logger.LogMessage("Searching for Devices");
            var result = new List<Device>();

            var action = new ServerAction()
            {
                ApiPath = "/my/listdevices",
                Key = ServerEncryptionToken,
                Parameters = new Dictionary<string, string>()
                {
                    { "sessiontoken", SessionToken.Aggregate("", (c, t) => c + t.ToString("X2")) },
                    { "rid", Utils.GetRequestId() }
                },
                RequestType = RequestType.Post,
                Token = SessionToken
            };

            var response = await action.ExecuteRequest();
            foreach (JObject device in JObject.Parse(response)["list"].ToObject<JArray>())
            {
                result.Add(device.ToObject<Device>());
            }
            if (result.Count > 0)
            {
                Logger.LogMessage("Found the following devices:");
                foreach (Device dev in result)
                {
                    Logger.LogMessage($"{dev.Name} - {dev.Id}");
                }
            }
            else
                Logger.LogMessage("Couldn´t find active JDownloader instances on your account!");

            return result;
        }

        private void RefreshDevices(object sender, ElapsedEventArgs e)
        {
            try
            {
                var devices = GetDevices().Result;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, LogType.Error);
            }
        }

        #endregion Private Methods
    }
}