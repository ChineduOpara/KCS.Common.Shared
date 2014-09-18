using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public static class IIS
    {
        private static List<IISWebsite> _sites = new List<IISWebsite>();
        private static object _lock = new object();
        private static ServerManager _serverManager;

        public static ServerManager ServerManager
        {
            get
            {
                lock (_lock)
                {
                    if (_serverManager == null)
                    {
                        _serverManager = new ServerManager();
                    }
                }
                return _serverManager;
            }
        }

        static IIS()
        {
            foreach (var site in ServerManager.Sites)
            {
                var newSite = new IISWebsite(site);
                _sites.Add(newSite);
            }
        }

        /// <summary>
        /// Gets all websites, or just those matching the given application root.
        /// </summary>
        /// <param name="wwwRoot">Root directory.</param>
        /// <returns>Array if IISWebsites</returns>
        public static IISWebsite[] GetWebsites(string wwwRoot = "")
        {       
            bool filterByDirectoryMapping = !string.IsNullOrWhiteSpace(wwwRoot);
            if (filterByDirectoryMapping)
            {
                return _sites.Where(x => x.PhysicalPath.Equals(wwwRoot, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            }
            else
            {
                return _sites.ToArray();
            }
        }

        /// <summary>
        /// Recycle local IIS.
        /// </summary>
        public static void Recycle(string serverName = "localhost")
        {
            if (serverName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
            {
                var info = new ProcessStartInfo("iisreset.exe", "/RESTART");
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = false;
                var process = Process.Start(info);
                process.WaitForExit();
            }
            else
            {
                throw new NotSupportedException("Non-local servers are not yet supported");
            }
        }
    }
}
