using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public class SuccessEventArgs : GenericEventArgs<string>
    {
        /// <summary>
        /// Contains the Success flag.
        /// </summary>
        public bool Success { get; private set; }

		public string Message
		{
			get { return base.Data; }
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
        public SuccessEventArgs(bool success) : base("Success")
        {
            Success = success;
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="success">Success value.</param>
		public SuccessEventArgs(bool success, string message) : this(success)
		{
			base.Data = message;
		}
    }
}
