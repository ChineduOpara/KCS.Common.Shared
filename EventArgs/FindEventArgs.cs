using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Search event args.
    /// </summary>
    public class FindEventArgs : GenericEventArgs<string>
    {
		/// <summary>
		/// Contains the text to be located.
		/// </summary>
		public string Text
		{
			get { return base.Data; }
		}

		/// <summary>
		/// Contains the Search Reversed flag.
		/// </summary>
		public bool Reverse { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Data to search for.</param>
        public FindEventArgs(string data) : base(data)
        {
        }
    }
}
