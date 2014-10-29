using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Boolean event arg with TimeStamp.
    /// </summary>
    public class SetBusyEventArgs : GenericEventArgs<bool>
    {
        /// <summary>
        /// Contains the Busy flag.
        /// </summary>
        public bool Busy { get { return base.Data; } }

        /// <summary>
        /// Contains the message.
        /// </summary>
        public string Message { get; private set; }
        
        public SetBusyEventArgs(bool busy) : base(busy)
        {
            this.Message = string.Empty;
        }

		/// <summary>
		/// Alternate constructor.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="message">Message.</param>
        public SetBusyEventArgs(bool busy, string message) : this(busy)
		{
            this.Message = message;
		}
    }
}
