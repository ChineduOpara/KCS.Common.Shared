using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains ListBox item click event data.
    /// </summary>
    public class ListBoxItemClickEventArgs : EventArgs
    {
        /// <summary>
        /// Contains the selected item.
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Selected ListBoxItem.</param>
        public ListBoxItemClickEventArgs(object item) : base()
        {
            Item = item;
        }
    }
}
