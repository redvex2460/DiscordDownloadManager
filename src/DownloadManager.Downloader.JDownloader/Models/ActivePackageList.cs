using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Downloader.JDownloader.Models
{
    public class ActivePackageList : List<QueryPackagesServerResponse>
    {
        public bool Contains(long uuid) => this.FirstOrDefault(item => item.UUID == uuid) != null;
    }
}
