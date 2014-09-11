using KCS.Common.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public event EventHandler<TimeSpanEventArgs> Reseted;
        #endregion

        private List<IISWebsiteBinding> _bindings = new List<IISWebsiteBinding>(1);
        public string Name { get; set; }

        public IISWebsiteBinding this[string uri]
        {
            get
            {
                var match = _bindings.FirstOrDefault(x => x.Uri.ToString() == uri.Trim());
                return match;
            }
        }

        public IISWebsiteBinding[] Bindings
        {
            get { return _bindings.ToArray(); }
        }

        #region Event-raising methods
        protected void OnCopyingConfigFiles()
        {
            if (Resetting != null)
            {
                Resetting(this, new TimeStampEventArgs());
            }
        }

        protected void OnCopiedConfigFiles(TimeSpan ts)
        {
            if (Reseted != null)
            {
                Reseted(this, new TimeSpanEventArgs(ts));
            }
        }
        #endregion

        public IISWebsite(string name)
        {
            Name = name;
        }

        public IISWebsiteBinding AddWebsiteBinding(string uri, string ipAddress = "127.0.0.1")
        {
            var map = new IISWebsiteBinding(uri, ipAddress);
            _bindings.Add(map);
            return map;
        }

        /// <summary>
        /// Resets the website in IIS.
        /// </summary>
        public void Reset()
        {
        }
    }
}
