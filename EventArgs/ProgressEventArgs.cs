using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// Indicates progress.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
		private decimal _current = 0;
		private decimal _total;

		/// <summary>
		/// Gets or sets the index of the item that was last processed.
		/// </summary>
		public decimal Current
		{
			get { return _current; }
			set
			{
				_current = value;
				Progress = (_current / _total) * 100;
			}
		}

		/// <summary>
		/// Gets or sets the total number of items to be processed.
		/// </summary>
		public decimal Total
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets or sets progress, as a percentage (it will be 100% or less).
        /// </summary>
        public decimal Progress
        {
            get;
			private set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProgressEventArgs()
        {
			Total = 0;
			Current = 0;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="total">Total number of items being processed.</param>
        public ProgressEventArgs(decimal total): this()
        {
			_total = total;
        }
    }
}
