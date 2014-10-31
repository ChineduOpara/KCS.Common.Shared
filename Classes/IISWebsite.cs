using KCS.Common.Shared;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Represents a websiote
    /// </summary>
    public class IISWebsite
    {
        #region Events
        public event EventHandler<TimeStampEventArgs> Resetting;
        public event EventHandler<TimeSpanEventArgs> Resetted;
        #endregion
        
        private List<string> _appPoolNames = new List<string>(1);
        private List<DnsHostEntry> _bindings = new List<DnsHostEntry>(1);

        /// <summary>
        /// Website name, as shown in IIS.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Physical path, as shown in IIS.
        /// </summary>
        public string PhysicalPath { get; internal set; }

        /// <summary>
        /// Index into the binding.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public DnsHostEntry this[string uri]
        {
            get
            {
                var match = _bindings.FirstOrDefault(x => x.Uri.ToString() == uri.Trim());
                return match;
            }
        }

        /// <summary>
        /// Gets all the bindings.
        /// </summary>
        public DnsHostEntry[] Bindings
        {
            get { return _bindings.ToArray(); }
        }

        #region Event-raising methods
        protected void OnResetting()
        {
            if (Resetting != null)
            {
                Resetting(this, new TimeStampEventArgs());
            }
        }

        protected void OnResetted(TimeSpan ts)
        {
            if (Resetted != null)
            {
                Resetted(this, new TimeSpanEventArgs(ts));
            }
        }
        #endregion

        /// <summary>
        /// Constructor for blank website.
        /// </summary>
        /// <param name="name"></param>
        public IISWebsite(string name, string physicalPath)
        {
            Name = name;
            PhysicalPath = physicalPath;
        }

        /// <summary>
        /// Constructor for a website that exists in IIS.
        /// </summary>
        /// <param name="site"></param>
        public IISWebsite(Site site)
        {
            Name = site.Name;
            var applicationRoot = site.Applications.Where(a => a.Path == "/").Single();
            var virtualRoot = applicationRoot.VirtualDirectories.Where(v => v.Path == "/").Single();
            PhysicalPath = virtualRoot.PhysicalPath;

            foreach (var app in site.Applications)
            {
                _appPoolNames.Add(app.ApplicationPoolName);
            }

            foreach (var binding in site.Bindings)
            {
                if (binding.EndPoint != null && !string.IsNullOrWhiteSpace(binding.Host))
                {
                    var urlBuilder = new UriBuilder(binding.Protocol, binding.Host, binding.EndPoint.Port);
                    IPAddress address = binding.EndPoint.Address;
                    if (address.Equals(DnsHostEntry.NoAddress))
                    {
                        address = DnsHostEntry.LocalHost;
                    }
                    var newBinding = new DnsHostEntry(urlBuilder.Uri, address);
                    _bindings.Add(newBinding);                    
                }
            }
        }

        /// <summary>
        /// Checks to see if this website is in IIS.
        /// </summary>
        /// <returns></returns>
        public bool ValidateInIIS()
        {
            var site = IIS.ServerManager.Sites.FirstOrDefault(x => x.Name.Equals(this.Name, StringComparison.CurrentCultureIgnoreCase));
            return site != null;
        }

        /// <summary>
        /// Resets the website in IIS.
        /// </summary>
        public void Reset()
        {
            OnResetting();
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            try
            {
                var site = IIS.ServerManager.Sites.FirstOrDefault(x => x.Name.Equals(this.Name, StringComparison.CurrentCultureIgnoreCase));
                if (site != null)
                {
                    // Get all application pools
                    var appPools = IIS.ServerManager.ApplicationPools;

                    // Stop the website
                    site.Stop();
                    while (site.State != ObjectState.Stopped)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }

                    // Recycle the matching application pools
                    foreach (var appPoolName in _appPoolNames)
                    {
                        var appPoolMatch = appPools[appPoolName];
                        if (appPoolMatch != null)
                        {
                            appPoolMatch.Recycle();
                            while (appPoolMatch.State != ObjectState.Started)
                            {
                                System.Threading.Thread.Sleep(500);
                            }
                        }
                    }

                    // Restart the website
                    site.Start();
                    while (site.State == ObjectState.Started)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
                OnResetted(stopwatch.Elapsed);
            }
        }
    }
}
