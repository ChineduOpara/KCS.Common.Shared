using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class UrlEventArgs : GenericEventArgs<string>
    {
        /// <summary>
        /// Contains the Uri.
        /// </summary>
        public Uri Uri { get; private set; }

        public string Message
		{
			get { return base.Data; }
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public UrlEventArgs(Uri uri) : base("")
        {
            this.Uri = uri;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
        public UrlEventArgs(Uri uri, string message) : this(uri)
		{
			base.Data = message;
		}
    }
}
