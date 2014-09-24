using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class DnsHostEntry
    {
        public Uri Uri { get; protected set; }
        public IPAddress IPAddress { get; protected set; }
        public bool Enabled { get; set; }
        public string Comment { get; set; }

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

        public DnsHostEntry(IPAddress ipAddress, string url) : this(url)
        {
            IPAddress = ipAddress;
        }

        public DnsHostEntry(string url)
        {
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1");
            var ub = new UriBuilder(url);
            Uri = ub.Uri;
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Uri.ToString(), IPAddress);
        }
    }
}
