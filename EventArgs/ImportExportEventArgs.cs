using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains information about an Import or Export operation.
    /// </summary>
    public class ImportExportEventArgs : SuccessEventArgs
	{
		#region Properties
		/// <summary>
        /// Contains the number of items imported or exported.
        /// </summary>
        public long Count { get; set; }
		#endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImportExportEventArgs(long count, bool success) : base(success)
        {
            Count = count;
        }

        /// <summary>
        /// Constructor, defaults to Success.
        /// </summary>
        public ImportExportEventArgs(long count) : this(count, true)
        {
        }
    }
}
