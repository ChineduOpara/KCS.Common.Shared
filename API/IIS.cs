using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public static class IIS
    {
        private static List<IISWebsite> _sites;
        private static List<DnsHostEntry> _dnsEntries;
        private static object _lock = new object();
        private static ServerManager _serverManager;
        private static Regex _dnsHostEntryRowPattern = new Regex(@"^#?\s*"
                + @"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|[0-9a-f:]+)\s+"
                + @"(?<hosts>(([a-z0-9][-_a-z0-9]*\.?)+\s*)+)"
                + @"(?:#\s*(?<comment>.*?)\s*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

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

        /// <summary>
        /// Contains all the DNS host entries.
        /// </summary>
        public static DnsHostEntry[] DnsHostEntries
        {
            get { return _dnsEntries.ToArray(); }
        }

        static IIS()
        {
            Load();
        }

        /// <summary>
        /// Loads websites and host file entries.
        /// </summary>
        static public void Load()
        {
            // Load host file entries
            var hostFileEntries = GetHostFileEntries();
            _dnsEntries = hostFileEntries.Entries.ToList();

            // Load all websites from IIS
            _sites = ServerManager.Sites.Select(x => new IISWebsite(x)).ToList();
        }        

        /// <summary>
        /// Parse the hosts file and get all the entries.
        /// </summary>
        /// <returns>Collection of hosts file entries</returns>
        public static GetHostFileEntriesResult GetHostFileEntries(string hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts")
        {
            var result = new GetHostFileEntriesResult();
            if (!File.Exists(hostsFilePath))
            {
                var ex = new FileNotFoundException("Hosts file was not found!", hostsFilePath);
                result.AddException(ex);
                return result;
            }

            var allLines = File.ReadAllLines(hostsFilePath);

            for(int i=0; i < allLines.Count(); i++)
            {
                var line = allLines[i].Trim();

                // Skip blank lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var match = _dnsHostEntryRowPattern.Match(line);
                if (!match.Success)
                {
                    var ex = new FormatException(string.Format("line {0:000} was not formatted as expected. Skipping.", i+1));
                    result.AddException(ex);
                    continue;
                }

                bool enabled = false;
                string ipAddressString = string.Empty;
                string hostNames = string.Empty;
                string comment = string.Empty;
                bool hidden = false;

                enabled = line[0] != '#';
                ipAddressString = match.Groups["ip"].Value.Trim();
                hostNames = match.Groups["hosts"].Value.Trim();
                comment = match.Groups["comment"].Value.Trim();

                // Skip invalid IP address
                IPAddress ipAddress;
                if (!System.Net.IPAddress.TryParse(ipAddressString, out ipAddress))
                {
                    var ex = new FormatException(string.Format("line {0:000} did not have a valid IP address. Skipping.", i+1));
                    result.AddException(ex);
                    continue;
                }

                // Comment
                if (!String.IsNullOrEmpty(comment))
                {
                    hidden = comment[0] == '!';
                    if (hidden)
                    {
                        comment = comment.Substring(1).Trim();
                    }
                }

                DnsHostEntry entry = null;
                var hosts = hostNames.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var host in hosts)
                {
                    try
                    {
                        entry = new DnsHostEntry(ipAddress, host);
                        entry.Enabled = enabled;
                        entry.Comment = comment;
                        result.AddEntry(entry);
                    }
                    catch (Exception ex)
                    {
                        var fex = new FormatException(string.Format(@"line {0:000} did not have a valid IP address. Skipping. Full error message:\r\n\{1}", i + 1, ex.GetAllExceptionsString()));
                        result.AddException(fex);
                    }
                }
            }

            return result;
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
