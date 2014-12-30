using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class CancelEventArgs : GenericEventArgs<string>
    {
        /// <summary>
        /// Contains the Cancelled flag.
        /// </summary>
        public bool Cancel { get; set; }

		public string Message
		{
			get { return base.Data; }
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cancelled">Success value.</param>
        public CancelEventArgs(bool cancel) : this(cancel, "")
        {
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cancel">Success value.</param>
		public CancelEventArgs(bool cancel, string message = "") : base(message)
		{
            this.Cancel = cancel;
		}
    }
}
