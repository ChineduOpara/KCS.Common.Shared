using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class IISWebsiteBinding : DnsHostEntry//, IComparable<IISWebsiteBinding>
    {
        public IISWebsite Site { get; set; }

        public bool IsSecure
        {
            get { return Uri.Scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.CurrentCultureIgnoreCase); }
        }

        public bool IsInHostsFile
        {
            get
            {
                var match = GetMatchingDnsHostEntry();
                return match != null;
            }
        }

        private DnsHostEntry GetMatchingDnsHostEntry()
        {
            return IIS.DnsHostEntries.FirstOrDefault(x => x.Uri.Equals(this.Uri));
        }

        public IISWebsiteBinding(Uri uri, IISWebsite site) : base(uri)
        {
            this.Site = site;

            if (IsInHostsFile)
            {
                var match = GetMatchingDnsHostEntry();
                IPAddress = match.IPAddress;
                Enabled = match.Enabled;
                Comment = match.Comment;
                GroupName = match.GroupName;
            }
        }

        public override string ToString()
        {
            if (IPAddress == null)
            {
                return DnsSafeDisplayString;                
            }
            else
            {
                return string.Format("{0} -> {1}", DnsSafeDisplayString, IPAddress);
            }
        }

        //public int CompareTo(IISWebsiteBinding other)
        //{
        //    return base.CompareTo(other);
        //}
    }
}
