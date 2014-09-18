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
        public IISWebsite Site { get; private set; }
        public Uri Uri { get; set; }
        public IPAddress IPAddress { get; set; }
        public bool Enabled { get; set; }
        public bool ValidateInWebsite { get; set; }
        /// <summary>
        /// Any arbitrary string. It can be left blank.
        /// </summary>
        public string GroupName { get; set; }

        ///// <summary>
        ///// Constructor that defaults the target IP Address to localhost.
        ///// </summary>
        ///// <param name="uri"></param>
        //public IISWebsiteBinding(IISWebsite site, string uri, bool enabled = false, string groupName = "") : this(site, new IPAddress((new System.Text.ASCIIEncoding()).GetBytes("127.0.0.1")))
        //{
        //    this.GroupName = groupName;
        //}

        ///// <summary>
        ///// Constructor that allows ipAddress specification.
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <param name="ipAddress"></param>
        //public IISWebsiteBinding(IISWebsite site, string ipAddress) : this(site, )
        //{
        //}

        public IISWebsiteBinding(IISWebsite site, Uri uri)
        {
            this.Site = site;
            Uri = uri;
            //IPAddress = ipAddress;
        }
    }
}
