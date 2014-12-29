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

        private string _comment = string.Empty;
        private string _groupName = string.Empty;
        private Uri _uri;
        private IPAddress _ipAddress;
        private bool _enabled = false;
        
        #region Properties
        public bool IsDirty {get; private set;}

        public Uri Uri
        {
            get { return _uri; }
            set
            {
                if (value != _uri)
                {
                    _uri = value;
                    IsDirty = true;
                }
            }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                    IsDirty = true;
                }
            }
        }
        
        public bool Enabled
        {
            get {return _enabled;}
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Any arbitrary comment.
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = string.Empty;
                }
                if (string.Compare(_comment, value, true) != 0)
                {
                    _comment = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Any arbitrary group name.
        /// </summary>
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = string.Empty;
                }
                if (string.Compare(_groupName, value, true) != 0)
                {
                    _groupName = value;
                    IsDirty = true;
                }
            }
        }

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
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ipAddress"></param>
        public DnsHostEntry(Uri uri, IPAddress ipAddress) : this(uri)
        {
            IPAddress = ipAddress;
            IsDirty = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uri"></param>
        public DnsHostEntry(Uri uri)
        {
            Uri = uri;
            IsDirty = false;
        }

        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} -> {1}", Uri.ToString(), IPAddress);
        }

        internal void ResetDirty()
        {
            IsDirty = false;
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
