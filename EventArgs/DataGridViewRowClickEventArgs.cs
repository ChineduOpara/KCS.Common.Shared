using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains ListViewItem click event data.
    /// </summary>
    public class DataGridViewRowClickEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the selected ListViewItem.
        /// </summary>
        public DataGridViewRow DataGridViewRow { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="row">Selected item.</param>
		public DataGridViewRowClickEventArgs(DataGridViewRow row) : base()
        {
			DataGridViewRow = row;
        }
    }
}
