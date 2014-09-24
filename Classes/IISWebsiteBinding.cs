using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class IISWebsiteBinding : DnsHostEntry
    {
        private bool? _isInHostsFile;

        public IISWebsite Site { get; private set; }
        //public bool ValidateInWebsite { get; set; }

        public bool IsSecure
        {
            get { return Uri.Scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.CurrentCultureIgnoreCase); }
        }

        public bool IsInHostsFile
        {
            get { return _isInHostsFile.Value; }
        }

        public IISWebsiteBinding(IISWebsite site, Uri uri) : base(uri.DnsSafeHost)
        {
            this.Site = site;
            Uri = uri;

            var match = IIS.DnsHostEntries.FirstOrDefault(x => x.Uri.Equals(this.Uri));
            _isInHostsFile = match != null;
            if (match != null)
            {
                Enabled = match.Enabled;
            }
        }
    }
}
