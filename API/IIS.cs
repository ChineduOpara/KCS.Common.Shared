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
        public enum WebsiteAction
        {
            Recycle,
            Start,
            Stop
        }

        private static string _valuesTrackerKey;
        private const string HostEntryGroupNames = "HostEntryGroupNames";
        private const string DefaultHostFilePath = @"C:\Windows\System32\drivers\etc\hosts";
        private static List<IISWebsite> _sites;
        private static List<DnsHostEntry> _dnsEntries;
        private static object _lock = new object();
        private static ServerManager _serverManager;
        private static Regex _dnsHostEntryRowPattern = new Regex(@"^#?\s*"
                + @"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|[0-9a-f:]+)\s+"
                + @"(?<hosts>(([a-z0-9][-_a-z0-9]*\.?)+\s*)+)"
                + @"(?:#\s*(?<comment>.*?)\s*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        #region Properties
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

        public static IISWebsite[] Websites
        {
            get { return _sites.ToArray(); }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        static IIS()
        {
            _valuesTrackerKey = Environment.UserName + "-IIS";
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
            LoadDnsEntryGroupNames();

            // Load all websites from IIS
            _sites = ServerManager.Sites.Select(x => new IISWebsite(x)).ToList();

            // Update the Website property of each DNS entry
            foreach (var entry in _dnsEntries)
            {
                // Website
                var query = from site in _sites
                            from binding in site.Bindings
                            where binding.Uri.DnsSafeHost.Equals(entry.Uri.DnsSafeHost, StringComparison.CurrentCultureIgnoreCase)
                            select site;
                entry.Website = query.FirstOrDefault();
            }

            // Set location flags of each website binding
            _dnsEntries.ForEach(x => x.SetLocationFlags());
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
                        var ub = new UriBuilder(System.Uri.UriSchemeHttp.ToString(), host);
                        entry = new DnsHostEntry(ub.Uri, ipAddress);
                        entry.Enabled = enabled;
                        entry.Comment = comment;
                        entry.IsInHostsFile = true;
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
        public static IEnumerable<IISWebsite> GetWebsites(string wwwRoot)
        {       
            return _sites.Where(x => x.PhysicalPath.Equals(wwwRoot, StringComparison.CurrentCultureIgnoreCase));
        }

        public static SaveIISWebsiteBindingsResult UpdateIISWebsiteBindings(IEnumerable<DnsHostEntry> entriesAdded, IEnumerable<DnsHostEntry> entriesDeleted, IEnumerable<DnsHostEntry> entriesModified)
        {
            var results = new SaveIISWebsiteBindingsResult();

            // Skip all deleted items
            //var bindingsPrivate = new List<IISWebsiteBinding>(bindings.Where(x => !x.IsDeleted));

            // Get all relevant websites
            //var relevantWebsitesNames = bindings.Select(x => x.Site.Name).Distinct(); 
            //IEnumerable<IISWebsiteBinding> relevantWebsitesNames;

            //var matchingSites = ServerManager.Sites.Where(x => relevantWebsitesNames.Contains(x.Name));
            //foreach (var site in matchingSites)
            {
                //var matchingBindings = bindingsPrivate.Where(x => x.Site.Name.Equals(site.Name, StringComparison.CurrentCultureIgnoreCase));
                //var matchingBindingsHostNames = matchingBindings.Select(x => x.Uri.DnsSafeHost);
                //var deletedBindingsHostNames = matchingBindings.Where(x => x.IsDeleted).Select(x => x.Uri.DnsSafeHost);

                //var currentSiteBindings = site.Bindings.Where(x => !string.IsNullOrWhiteSpace(x.Host));
                //var currentSiteBindingsHostNames = currentSiteBindings.Select(x => x.Host);

                //#region Remove bindings
                //// Find the bindings that will be removed.                    
                //foreach (var sb in currentSiteBindings)
                //{
                //    if (!matchingBindingsHostNames.Contains(sb.Host) || deletedBindingsHostNames.Contains(sb.Host))
                //    {
                //        bindingsToRemove.Add(sb);
                //    }
                //}

                //// Remove bindings
                //foreach (var sb in bindingsToRemove)
                //{
                //    try
                //    {
                //        site.Bindings.Remove(sb);                            
                //        results.AddRemoved(sb);
                //    }
                //    catch (Exception ex)
                //    {
                //        results.AddException(sb, SaveIISWebsiteBindingsResult.BindingAction.Removing, ex);
                //    }
                //}
                //#endregion

                //#region Add bindings
                //// Find the bindings that need to be added
                //foreach (var b in matchingBindings)
                //{
                //    if (!currentSiteBindingsHostNames.Contains(b.Uri.DnsSafeHost))
                //    {
                //        bindingsToAdd.Add(b);
                //    }
                //}

                //// Add new bindings
                //foreach (var mb in bindingsToAdd)
                //{
                //    try
                //    {
                //        var protocol = mb.IsSecure ? System.Uri.UriSchemeHttps : System.Uri.UriSchemeHttp;
                //        var sb = site.Bindings.Add(string.Format("*:{0}:{1}", mb.Uri.Port, mb.Uri.DnsSafeHost), protocol);
                //        results.AddAdded(sb);
                //    }
                //    catch (Exception ex)
                //    {
                //        results.AddException(mb, SaveIISWebsiteBindingsResult.BindingAction.Adding, ex);
                //    }
                //}
                //#endregion

                #region Modify bindings port numbers
                //foreach (var sb in currentSiteBindings)
                //{
                //    if (matchingBindingsHostNames.Contains(sb.Host) || !deletedBindingsHostNames.Contains(sb.Host))
                //    {
                //        var match = matchingBindings.FirstOrDefault(x => x.Uri.DnsSafeHost.Equals(sb.Host, StringComparison.CurrentCultureIgnoreCase));
                //        if (match != null && match.Uri.Scheme == sb.Protocol)
                //        {
                //            if (sb.EndPoint.Port != match.Uri.Port)
                //            {
                //                try
                //                {
                //                    sb.BindingInformation = string.Format("*:{0}:{1}", match.Uri.Port, match.Uri.DnsSafeHost);
                //                    results.AddUpdated(sb);
                //                }
                //                catch (Exception ex)
                //                {
                //                    results.AddException(sb, SaveIISWebsiteBindingsResult.BindingAction.Updating, ex);
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
            }

            // Commit all changes
            try
            {
                ServerManager.CommitChanges();
            }
            catch (Exception ex)
            {
                results.AddException(ex);
            }

            return results;            
        }

        public static UpdateHostFileResult UpdateHostsFile(string hostsFilePath = null)
        {
            var result = new UpdateHostFileResult();
            uint counter;
            var exception = WriteDnsEntries(hostsFilePath, out counter);

            if (exception != null)
            {
                result.AddException(exception);
            }
            result.Written = counter;

            _dnsEntries.ForEach(x => x.ResetDirty());

            return result;
        }

        public static UpdateHostFileResult DisableDnsEntries(string hostsFilePath = null)
        {
            var result = new UpdateHostFileResult();

            // If there are no enabled entries, bail out.
            var enabledEntries = _dnsEntries.Where(x => x.Enabled);
            if (!enabledEntries.Any())
            {
                return result;
            }

            // Disable whichever ones are enabled.
            uint counter = 0;
            foreach(var entry in enabledEntries)
            {
                entry.Enabled = false;
                counter++;
            };
            result.Written = counter;

            uint writtenCount;
            var exception = WriteDnsEntries(hostsFilePath, out writtenCount);

            if (exception != null)
            {
                result.AddException(exception);
            }            

            _dnsEntries.ForEach(x => x.ResetDirty());

            return result;
        }

        /// <summary>
        /// Group the list and output to file.
        /// </summary>
        /// <param name="entries"></param>
        private static Exception WriteDnsEntries(string hostsFilePath, out uint counter)
        {
            Exception exception = null;
            counter = 0;

            // Make sure the hosts file is valid
            if (string.IsNullOrWhiteSpace(hostsFilePath) || !File.Exists(hostsFilePath))
            {
                hostsFilePath = DefaultHostFilePath;
            }

            // Back the file up.
            var backupFilePath = BackupHostsFile(hostsFilePath);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("# Created by ADH utility on {0}", DateTime.Now));
            sb.AppendLine(string.Format("# Backup file is here: {0}", backupFilePath));
            sb.AppendLine();

            var eq = new DnsHostEntryEqualityComparer();            
            var grouping = _dnsEntries.Distinct(eq).GroupBy(x => x.GroupName);
            foreach (var group in grouping.OrderBy(x => x.Key))
            {
                var groupKey = group.Key;
                sb.AppendLine("#" + groupKey);
                sb.AppendLine("#");
                foreach (var dnsEntry in group.OrderBy(x => x.DnsSafeDisplayString))
                {
                    sb.Append(dnsEntry.Enabled ? " " : "#");
                    sb.Append(dnsEntry.IPAddress.ToString() + "    ");
                    sb.Append(dnsEntry.DnsSafeDisplayString);

                    if (!string.IsNullOrWhiteSpace(dnsEntry.Comment))
                    {
                        sb.AppendFormat(new string(' ', 20) + "# {0}", dnsEntry.Comment);
                    }
                    sb.AppendLine();
                    counter++;
                }
                sb.AppendLine();
            }

            // Write the contents to the hosts file
            try
            {
                File.WriteAllText(hostsFilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Save the group names
            if (exception == null)
            {
                SaveDnsEntryGroupNames();    
            }

            return exception;
        }

        private static void SaveDnsEntryGroupNames()
        {
            var vt = new ValuesTracker(_valuesTrackerKey);
            var groupNames = new Dictionary<string, string>();
            foreach (var entry in _dnsEntries.Where(x => !string.IsNullOrWhiteSpace(x.GroupName)))
            {
                if (groupNames.ContainsKey(entry.ToString()))
                {
                    groupNames[entry.ToString()] = entry.GroupName;
                }
                else
                {
                    groupNames.Add(entry.ToString(), entry.GroupName);
                }
            }
            vt.AddValue(HostEntryGroupNames, groupNames);
            vt.Save();
        }

        private static void LoadDnsEntryGroupNames()
        {
            var emptyGroupNames = new Dictionary<string, string>();
            var vt = new ValuesTracker(_valuesTrackerKey);
            vt.Load();
            var groupNames = vt.GetValue(HostEntryGroupNames, emptyGroupNames);

            foreach (var entry in _dnsEntries)
            {
                var key = entry.ToString();
                if (groupNames.ContainsKey(key))
                {
                    entry.GroupName = groupNames[key];
                }
            }
        }

        private static string BackupHostsFile(string hostsFilePath)
        {
            // Create filenames until a unique one is found.
            var backupFilePath = string.Format("{0}_bak1", hostsFilePath);
            int counter = 1;
            bool backedUp = false;
            do
            {
                backupFilePath = string.Format("{0}_bak{1}", hostsFilePath, counter++);
                if (!File.Exists(backupFilePath))
                {
                    File.Copy(hostsFilePath, backupFilePath);
                    backedUp = true;
                }

            } while (!backedUp);

            return backupFilePath;
        }        

        /// <summary>
        /// Recycle local IIS.
        /// </summary>
        public static Process Recycle(IntPtr windowHandle, bool waitForExit = true)
        {
            var info = new ProcessStartInfo("iisreset.exe", "/RESTART");
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            if (windowHandle != IntPtr.Zero)
            {
                info.ErrorDialogParentHandle = windowHandle;
            }
            var process = Process.Start(info);
            if (waitForExit)
            {
                process.WaitForExit();
            }
            return process;
        }

        /// <summary>
        /// Recycle IIS on a PC.
        /// </summary>
        public static Process Recycle(IntPtr windowHandle, string psExecPath, string serverName, NetworkCredential credential = null, bool waitForExit = true)
        {
            if (serverName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
            {
                return Recycle(windowHandle, waitForExit);
            }
            else
            {
                if (!File.Exists(psExecPath))
                {
                    return null;
                }

                string cmdParameters = string.Empty;
                if (credential == null)
                {
                    cmdParameters = string.Format(@"\\{0} iisreset.exe /RESTART", serverName);
                }
                else
                {
                    cmdParameters = string.Format(@"\\{0} -u {1} -p {2} iisreset.exe /RESTART", serverName, credential.UserName, credential.Password);
                }

                var info = new ProcessStartInfo(psExecPath, cmdParameters);
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = false;
                if (windowHandle != IntPtr.Zero)
                {
                    info.ErrorDialogParentHandle = windowHandle;
                }
                var process = Process.Start(info);
                if (waitForExit)
                {
                    process.WaitForExit();
                }

                return process;
            }
        }

        /// <summary>
        /// Starts or stops websites on the local IIS.
        /// </summary>
        /// <param name="windowHandle"></param>
        /// <param name="websites"></param>
        /// <param name="waitForExit"></param>
        /// <returns></returns>
        public static string[] StartOrStopWebsites(IntPtr windowHandle, IEnumerable<string> websites, WebsiteAction action)
        {
            var successSites = new List<string>(websites);
            var matchingSites = new List<Site>(websites.Count());
            var targetWebsites = websites.ToList();
            foreach (var s in ServerManager.Sites)
            {
                if (targetWebsites.Contains(s.Name, StringComparer.OrdinalIgnoreCase))                
                {
                    matchingSites.Add(s);
                }
            }

            foreach (var site in matchingSites)
            {
                var state = ObjectState.Stopped;
                if (action == WebsiteAction.Recycle || action == WebsiteAction.Stop)
                {
                    site.Stop();                    
                }
                if (site.State == ObjectState.Stopped)
                {
                    if (action == WebsiteAction.Stop)
                    {
                        successSites.Add(site.Name);
                    }

                    if (action == WebsiteAction.Recycle || action == WebsiteAction.Start)
                    {
                        site.Start();
                        if (site.State == ObjectState.Started)
                        {
                            successSites.Add(site.Name);
                        }
                    }
                }
            }

            return successSites.ToArray();
        }

        /// <summary>
        /// Starts or stops website on the remote server.
        /// </summary>
        /// <param name="windowHandle"></param>
        /// <param name="psExecPath"></param>
        /// <param name="serverName"></param>
        /// <param name="websites"></param>
        /// <param name="action"></param>
        /// <param name="waitForExit"></param>
        /// <returns></returns>
        public static Process StartOrStopWebsites(IntPtr windowHandle, string psExecPath, string serverName, IEnumerable<string> websites, WebsiteAction action, NetworkCredential credential = null, bool waitForExit = true)
        {
            if (serverName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
            {
                StartOrStopWebsites(windowHandle, websites, action);
                return null;
            }
            else
            {
                if (!File.Exists(psExecPath))
                {
                    return null;
                }

                var psExecFile = new FileInfo(psExecPath);
                var currentDir = Environment.CurrentDirectory;
                Environment.CurrentDirectory = psExecFile.Directory.FullName;                

                // Generate the batch file that will contain the commands
                var batchFileName = "Websites.bat";
                var batchFilePath = Path.Combine(psExecFile.Directory.FullName, batchFileName);
                var sb = new StringBuilder(@"SET APPCMD=%windir%\system32\inetsrv\appcmd.exe");
                sb.AppendLine();
                foreach (var website in websites)
                {
                    if (action == WebsiteAction.Recycle || action == WebsiteAction.Stop)
                    {
                        sb.AppendFormat("%APPCMD% stop sites \"{0}\"\r\n", website);
                    }
                    if (action == WebsiteAction.Recycle || action == WebsiteAction.Start)
                    {
                        sb.AppendFormat("%APPCMD% start sites \"{0}\"\r\n", website);
                    }
                }
                File.WriteAllText(batchFilePath, sb.ToString());

                string cmdParameters = string.Empty;
                if (credential == null)
                {
                    cmdParameters = string.Format(@"\\{0} -c {1}", serverName, batchFileName);
                }
                else
                {
                    cmdParameters = string.Format(@"\\{0} -c {1} -u {2} -p {3}", serverName, credential.UserName, credential.Password, batchFileName);
                }

                var info = new ProcessStartInfo(psExecFile.Name, cmdParameters);
                info.WorkingDirectory = Environment.CurrentDirectory;
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = false;
                if (windowHandle != IntPtr.Zero)
                {
                    info.ErrorDialogParentHandle = windowHandle;
                }
                var process = Process.Start(info);
                if (waitForExit)
                {
                    process.WaitForExit();
                    // If there were no errors, delete the batch file.
                    if (process.ExitCode == 0)
                    {
                        File.Delete(batchFilePath);
                    }
                }

                Environment.CurrentDirectory = currentDir;
                return process;
            }
        }
    }
}
