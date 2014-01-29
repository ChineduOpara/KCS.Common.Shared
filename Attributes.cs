using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    public class Attributes
    {
        /// <summary>
        /// Associates a URL with a Type.
        /// </summary>
        public class UrlAttribute : Attribute
        {
            public virtual Uri Uri { get; private set; }
            public string Description { get; set; }

            /// <summary>
            /// Constructor with Uri object.
            /// </summary>
            /// <param name="uri">Uri object.</param>
            public UrlAttribute(Uri uri)
            {
                this.Uri = uri;
            }

            /// <summary>
            /// Constructor with Url string.
            /// </summary>
            /// <param name="url">Url string.</param>
            public UrlAttribute(string url)
            {
                this.Uri = new Uri(url);
            }

            /// <summary>
            /// Constructor with Url string and description.
            /// </summary>
            /// <param name="url">Url string.</param>
            /// <param name="description">Url description.</param>
            public UrlAttribute(string url, string description) : this (url)
            {
                this.Description = description;
            }
        }
    }
}
