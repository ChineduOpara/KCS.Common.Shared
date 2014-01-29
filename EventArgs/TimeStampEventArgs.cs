using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class TimeStampEventArgs : GenericEventArgs<string>
    {
        /// <summary>
        /// Contains the TImeStamp
        /// </summary>
        public DateTime TimeStamp { get; private set; }

		public string Message
		{
			get { return base.Data; }
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public TimeStampEventArgs() : this(DateTime.Now)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public TimeStampEventArgs(DateTime timeStamp) : base("")
        {
            TimeStamp = timeStamp;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
		public TimeStampEventArgs(DateTime timeStamp, string message) : this(timeStamp)
		{
			base.Data = message;
		}
    }
}
