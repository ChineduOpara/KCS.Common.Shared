using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class TimeStampEventArgs : GenericEventArgs<DateTime>
    {
        /// <summary>
        /// Contains the TimeStamp
        /// </summary>
        public DateTime TimeStamp
        {
            get { return base.Data; }
            private set { base.Data = value; }
        }

        /// <summary>
        /// Contains the message
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public TimeStampEventArgs() : base(DateTime.Now)
        {
        }


        public TimeStampEventArgs(string message) : this()
        {
            this.Message = message;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
		public TimeStampEventArgs(DateTime timeStamp, string message) : base(timeStamp)
		{
            this.Message = message;
		}
    }
}
