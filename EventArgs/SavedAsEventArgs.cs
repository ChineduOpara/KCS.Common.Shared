using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Contains event data for saving items under a new item group (with a new name).
    /// </summary>
    public class SavedAsEventArgs : SuccessEventArgs
    {
        /// <summary>
        /// Contains the Name of the new item group.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Contains the New Id of the saved item group.
        /// </summary>
        public long ObjectId { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of new item group.</param>
        /// <param name="success">Status of operation.</param>
        public SavedAsEventArgs(string name, bool success) : base(success)
        {
            Name = name;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectId">Id of new item group.</param>
        /// <param name="name">Name of new item group.</param>
        /// <param name="success">Status of operation.</param>
        public SavedAsEventArgs(long objectId, string name, bool success) : this(name, success)
        {
            ObjectId = objectId;
        }
    }
}
