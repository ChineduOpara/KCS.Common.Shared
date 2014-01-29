using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains an exception.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the Exception.
        /// </summary>
        public Exception Exception { get; private set; }

	    /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Exception.</param>
		public ExceptionEventArgs(Exception ex) : base()
        {
			this.Exception = ex;
        }
    }
}
