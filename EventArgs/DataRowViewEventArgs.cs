using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains an exception.
    /// </summary>
    public class DataRowViewEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the DataTable.
        /// </summary>
		public DataRowView DataRowView { get; private set; }

	    /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="success">Success value.</param>
		public DataRowViewEventArgs(DataRowView drv) : base()
        {
			this.DataRowView = drv;
        }
    }
}
