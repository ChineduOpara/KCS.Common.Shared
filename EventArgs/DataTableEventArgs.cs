using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains an exception.
    /// </summary>
    public class DataTableEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the DataTable.
        /// </summary>
		public DataTable DataTable { get; private set; }

	    /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
		public DataTableEventArgs(DataTable dt) : base()
        {
			this.DataTable = dt;
        }
    }
}
