using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains ListViewItem click event data.
    /// </summary>
    public class ListViewItemClickEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the selected ListViewItem.
        /// </summary>
        public ListViewItem ListViewItem { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Selected item.</param>
        public ListViewItemClickEventArgs(ListViewItem item) : base()
        {
            ListViewItem = item;
        }
    }
}
