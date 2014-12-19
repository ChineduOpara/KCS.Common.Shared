using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class DnsHostEntryEqualityComparer : IEqualityComparer<DnsHostEntry>
    {
        public bool Equals(DnsHostEntry x, DnsHostEntry y)
        {
            string xCompareString = string.Format("{0}-{1}", x.DnsSafeDisplayString, x.IPAddress == null ? "" : x.IPAddress.ToString());
            string yCompareString = string.Format("{0}-{1}", y.DnsSafeDisplayString, y.IPAddress == null ? "" : y.IPAddress.ToString());
            return string.Compare(xCompareString, yCompareString, true) == 0;
        }

        public int GetHashCode(DnsHostEntry obj)
        {
            string compareString = string.Format("{0}-{1}", obj.DnsSafeDisplayString, obj.IPAddress == null ? "" : obj.IPAddress.ToString());
            return compareString.GetHashCode();
        }
    }

    public class DnsHostEntry
    {
        public static IPAddress LocalHost = IPAddress.Parse("127.0.0.1");
        public static IPAddress NoAddress = IPAddress.Parse("0.0.0.0");

        public Uri Uri { get; set; }
        public IPAddress IPAddress { get; set; }
        public bool Enabled { get; set; }
        public string Comment { get; set; }
        
        /// <summary>
        /// If this is set, then this entry corresponds with an existing binding in IIS.
        /// </summary>
        public IISWebsite Website { get; internal set; }

        public bool IsSecure
        {
            get { return Uri.Scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.CurrentCultureIgnoreCase); }
        }

        public virtual bool IsInIIS
        {
            get { return Website != null; }
        }

        public bool IsInHostsFile  { get; internal set; }

        public virtual bool IsLocationSynced
        {
            get { return IsInIIS && IsInHostsFile; }
        }

        /// <summary>
        /// Any arbitrary string. It can be left blank.
        /// </summary>
        public string GroupName { get; set; }

        public virtual string DnsSafeDisplayString
        {
            get
            {
                var format = string.Format("{0}", Uri.DnsSafeHost);
                if (!Uri.IsDefaultPort)
                {
                    format = string.Format("{0}:{1}", format, Uri.Port);
                }
                return format;
            }
        }

        public DnsHostEntry(Uri uri, IPAddress ipAddress) : this(uri)
        {
            IPAddress = ipAddress;
        }

        public DnsHostEntry(Uri uri)
        {
            Uri = uri;
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Uri.ToString(), IPAddress);
        }

        public void SetLocationFlags()
        {
            // Set website
            var query = from site in IIS.Websites
                        from binding in site.Bindings
                        where binding.Uri.DnsSafeHost.Equals(Uri.DnsSafeHost, StringComparison.CurrentCultureIgnoreCase)
                        select site;
            Website = query.FirstOrDefault();

            // Set IsInHosts flag
            var match = IIS.DnsHostEntries.FirstOrDefault(x => x.Uri.Equals(Uri));
            IsInHostsFile = match != null;
        }
    }
}
