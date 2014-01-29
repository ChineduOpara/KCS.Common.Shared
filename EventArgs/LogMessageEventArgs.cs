using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    public class EventLogMessageEventArgs : TimeStampEventArgs
    {
        /// <summary>
        /// Contains the message timestamp.
        /// </summary>
        public Enumerations.MessageType EntryType { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public EventLogMessageEventArgs(string message) : base(DateTime.Now, message)
        {
            EntryType = Enumerations.MessageType.Information;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public EventLogMessageEventArgs(string message, Enumerations.MessageType entryType)
            : base(DateTime.Now, message)
        {
            EntryType = entryType;
        }
    }
}
