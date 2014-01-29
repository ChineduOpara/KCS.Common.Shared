using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class TimeSpanEventArgs : GenericEventArgs<string>
    {
        /// <summary>
        /// Contains the TimeSpan
        /// </summary>
        public TimeSpan TimeSpan { get; private set; }

		public string Message
		{
			get { return base.Data; }
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public TimeSpanEventArgs(TimeSpan timeSpan) : base("")
        {
            TimeSpan = timeSpan;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
		public TimeSpanEventArgs(DateTime startTime, DateTime endTime, string message) : base("")
		{
            TimeSpan = endTime - startTime;
			base.Data = message;
		}
    }
}
