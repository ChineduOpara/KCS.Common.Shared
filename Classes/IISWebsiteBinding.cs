using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class IISWebsiteBinding
    {
        public Uri Uri { get; set; }
        public IPAddress IPAddress { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Constructor that defaults the target IP Address to localhost.
        /// </summary>
        /// <param name="uri"></param>
        public IISWebsiteBinding(string uri) : this(uri, "127.0.0.1")
        {
        }

        /// <summary>
        /// Constructor that allows ipAddress specification.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="ipAddress"></param>
        public IISWebsiteBinding(string uri, string ipAddress)
        {
            var encoding = new System.Text.ASCIIEncoding();
            Uri = new Uri(uri);
            IPAddress = new IPAddress(encoding.GetBytes(ipAddress));
        }
    }
}
